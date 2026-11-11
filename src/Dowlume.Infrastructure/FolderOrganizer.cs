using System.Security.Cryptography;
using System.Text;
using Dowlume.Core;

namespace Dowlume.Infrastructure;

/// <summary>Collects stable top-level directories by atomic rename, without merging their contents.</summary>
public sealed class FolderOrganizer(IStateStore store, TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private readonly Dictionary<string, (string Snapshot, DateTimeOffset Since)> _observations = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _gate = new(1, 1);

    public IReadOnlyList<string> Preview(AppSettings settings)
    {
        settings.Validate();
        if (!settings.OrganizeFolders)
            return [];
        PathSafety.ValidateRoot(settings.DownloadsPath);
        var protectedNames = settings.Categories.Select(rule => rule.FolderName.Normalize()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        protectedNames.Add(AppSettings.ReviewFolderName);
        protectedNames.Add(settings.CollectedFoldersName.Normalize());
        // Retain protection for renamed/deleted rules and all previously collected containers.
        IReadOnlyList<OperationRecord> operations = store.GetOperations();
        foreach (OperationRecord unresolved in operations.Where(operation => operation.Kind == OperationKind.OrganizeFolder &&
                     operation.Status is OperationStatus.Pending or OperationStatus.NeedsAttention))
            if (PathSafety.AreSame(Path.GetDirectoryName(unresolved.SourcePath)!, settings.DownloadsPath))
                protectedNames.Add(Path.GetFileName(unresolved.SourcePath).Normalize());
        IEnumerable<string> recordedPaths = store.GetFiles().Select(file => file.CurrentPath)
            .Concat(operations.SelectMany(operation => new[] { operation.SourcePath, operation.DestinationPath ?? operation.SourcePath }));
        foreach (string path in recordedPaths.Where(path => PathSafety.IsWithin(settings.DownloadsPath, path)))
        {
            string relative = Path.GetRelativePath(settings.DownloadsPath, path);
            if (relative.Contains(Path.DirectorySeparatorChar))
                protectedNames.Add(relative.Split(Path.DirectorySeparatorChar)[0].Normalize());
        }
        return Directory.EnumerateDirectories(settings.DownloadsPath)
            .Where(path => !protectedNames.Contains(Path.GetFileName(path).Normalize()) && !Path.GetFileName(path).StartsWith('.'))
            .Order(StringComparer.Ordinal).ToArray();
    }

    public async Task ScanAsync(AppSettings settings, CancellationToken token = default)
    {
        await _gate.WaitAsync(token);
        try
        {
            IReadOnlyList<string> candidates = Preview(settings);
            foreach (string old in _observations.Keys.Except(candidates).ToArray())
                _observations.Remove(old);
            foreach (string source in candidates)
            {
                token.ThrowIfCancellationRequested();
                string? snapshot = Capture(source, settings, token);
                if (snapshot is null)
                {
                    _observations.Remove(source);
                    continue;
                }
                if (!_observations.TryGetValue(source, out var previous) || previous.Snapshot != snapshot)
                {
                    _observations[source] = (snapshot, _clock.GetUtcNow());
                    continue;
                }
                if (_clock.GetUtcNow() - previous.Since < TimeSpan.FromMinutes(settings.FolderIntervalMinutes))
                    continue;
                string destination = Path.Combine(settings.DownloadsPath, settings.CollectedFoldersName, Path.GetFileName(source));
                // Collisions are left in place; never merge or invent a destination name.
                if (Path.Exists(destination))
                    continue;
                var after = new TrackedFile(Guid.NewGuid(), source, destination, FileCategory.Other,
                    _clock.GetUtcNow(), LifecycleStatus.Moved, snapshot);
                var operation = new OperationRecord(Guid.NewGuid(), OperationKind.OrganizeFolder, OperationStatus.Pending,
                    _clock.GetUtcNow(), null, after, source, destination);
                store.SaveOperation(operation);
                try
                {
                    PathSafety.ValidateRoot(settings.DownloadsPath);
                    PathSafety.RequireWithin(settings.DownloadsPath, source);
                    PathSafety.RequireWithin(settings.DownloadsPath, destination);
                    if (Capture(source, settings, token) != snapshot)
                        throw new IOException("The folder changed before moving. It was left in place.");
                    SafeFolderMove.Move(source, destination);
                    // Folder records are activity only: never enroll their children in file review.
                    store.SaveOperation(operation with
                    {
                        Status = OperationStatus.Completed
                    });
                    _observations.Remove(source);
                }
                catch (Exception error)
                {
                    store.SaveOperation(operation with
                    {
                        Status = OperationStatus.NeedsAttention,
                        Error = DownloadOrganizer.FriendlyError(error)
                    });
                    throw;
                }
            }
        }
        finally { _gate.Release(); }
    }

    private string? Capture(string root, AppSettings settings, CancellationToken token)
    {
        try
        {
            var pending = new Stack<string>();
            pending.Push(root);
            var entries = new List<string>();
            while (pending.TryPop(out string? path))
            {
                token.ThrowIfCancellationRequested();
                if (entries.Count >= 10000)
                    return null;
                PathSafety.RejectLinks(path);
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint | FileAttributes.Offline)) != 0 || Path.GetFileName(path).StartsWith('.'))
                    return null;
                bool directory = (attributes & FileAttributes.Directory) != 0;
                if (!directory && FileClassifier.IsExcluded(path, settings))
                    return null;
                DateTime modified = File.GetLastWriteTimeUtc(path);
                if (_clock.GetUtcNow().UtcDateTime - modified < TimeSpan.FromMinutes(settings.FolderIntervalMinutes))
                    return null;
                long length = 0;
                if (directory)
                {
                    if (OperatingSystem.IsLinux())
                    {
                        using var baseline = LinuxFileSystem.OpenDirectory(root);
                        using var current = LinuxFileSystem.OpenDirectory(path);
                        _ = LinuxFileSystem.Volume(current);
                        if (LinuxFileSystem.Inspect(baseline).MountId != LinuxFileSystem.Inspect(current).MountId)
                            return null;
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        using var baseline = MacFileSystem.OpenDirectory(root);
                        using var current = MacFileSystem.OpenDirectory(path);
                        MacFileSystem.RequireLocalApfs(current);
                        if (MacFileSystem.Volume(baseline) != MacFileSystem.Volume(current))
                            return null;
                    }
                    foreach (string child in Directory.EnumerateFileSystemEntries(path))
                    {
                        if (pending.Count + entries.Count >= 10000)
                            return null;
                        pending.Push(child);
                    }
                }
                else
                {
                    // On Windows this refuses active writers; Unix writers may ignore sharing locks.
                    using var read = OperatingSystem.IsLinux() ? LinuxFileSystem.OpenRead(path)
                        : OperatingSystem.IsMacOS() ? MacFileHandle.OpenRead(path)
                        : new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    length = read.Length;
                }
                entries.Add($"{Path.GetRelativePath(root, path)}\0{(int)attributes}\0{length}\0{modified.Ticks}\0{File.GetCreationTimeUtc(path).Ticks}");
            }
            entries.Sort(StringComparer.Ordinal);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', entries))));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException) { return null; }
    }
}
