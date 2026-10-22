using System.Runtime.Versioning;
using System.Text;
using Dowlume.Core;
using Dowlume.Infrastructure;

namespace Dowlume.Tests;

[SupportedOSPlatform("macos")]
[Collection("Mac filesystem")]
[Trait("Category", "MacNativeMove")]
public sealed class MacFileMoveTests
{
    [MacFact]
    public void AtomicMoveRetainsNativeIdentityMetadataAndContent()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("café.txt", "content");
        string destination = Path.Combine(workspace.Downloads, "Documents", "café.txt");
        File.SetUnixFileMode(source, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        DateTime modified = new(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(source, modified);
        RunNative("/usr/bin/xattr", "-w", "org.dowlume.acceptance", "retained", source);
        FileIdentity identity = mover.GetIdentity(source)!;
        mover.Move(source, destination, mover.Hash(source));
        Assert.Equal("retained", RunNative("/usr/bin/xattr", "-p", "org.dowlume.acceptance", destination).Trim());
        Assert.False(File.Exists(source));
        Assert.Equal(identity, mover.GetIdentity(destination));
        Assert.Equal("content", File.ReadAllText(destination));
        Assert.Equal(modified, File.GetLastWriteTimeUtc(destination));
        Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(destination));
    }

    [MacFact]
    public void DestinationCreatedAfterPreparationIsNeverOverwritten()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt", "source");
        string destination = Path.Combine(workspace.Downloads, "target.txt");
        using var move = new MacFileMove(source, destination, mover.Hash(source), beforeRename: () => File.WriteAllText(destination, "competitor"));
        Assert.Throws<IOException>(move.Execute);
        move.Dispose();
        Assert.Equal("source", File.ReadAllText(source));
        Assert.Equal("competitor", File.ReadAllText(destination));
    }

    [MacFact]
    public void SourceReplacementAfterPreparationPreservesBothObjectsAndFailsVerification()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt", "source");
        string destination = Path.Combine(workspace.Downloads, "target.txt");
        using var move = new MacFileMove(source, destination, mover.Hash(source), beforeRename: () =>
        {
            File.Move(source, source + ".original");
            File.WriteAllText(source, "replacement");
        });
        Assert.Throws<IOException>(move.Execute);
        move.Dispose();
        Assert.Equal("source", File.ReadAllText(source + ".original"));
        Assert.Equal("replacement", File.ReadAllText(destination));
        Assert.Equal(source + ".original", move.ObservedPath);
    }

    [MacFact]
    public void DestinationAncestorSwapIsDetectedAndCannotRedirectIntoItsReplacement()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt");
        string folder = Path.Combine(workspace.Downloads, "Target");
        Directory.CreateDirectory(folder);
        using var move = new MacFileMove(source, Path.Combine(folder, "source.txt"), mover.Hash(source), beforeRename: () =>
        {
            Directory.Move(folder, folder + ".old");
            Directory.CreateDirectory(folder);
        });
        Assert.Throws<IOException>(move.Execute);
        move.Dispose();
        Assert.Empty(Directory.EnumerateFileSystemEntries(folder));
        Assert.Equal("Disposable test data", File.ReadAllText(Path.Combine(folder + ".old", "source.txt")));
        Assert.Equal(Path.Combine(folder + ".old", "source.txt"), move.ObservedPath);
    }

    [MacFact]
    public void WriterAfterRenameIsDetectedWithoutDeletingTheChangedFile()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt", "before");
        string destination = Path.Combine(workspace.Downloads, "target.txt");
        // A raw POSIX writer does not honor flock. Use the native test helper to demonstrate that boundary.
        using var move = new MacFileMove(source, destination, mover.Hash(source), afterRename: () => NativeWrite(destination, "after!"));
        Assert.Throws<IOException>(move.Execute);
        move.Dispose();
        Assert.Equal("after!", File.ReadAllText(destination));
    }

    [MacFact]
    public void SymlinkDestinationAndChangedContentAreRefusedWithoutCreatingFolders()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt");
        string link = Path.Combine(workspace.Downloads, "link");
        Directory.CreateSymbolicLink(link, workspace.Root);
        Assert.Throws<IOException>(() => mover.Move(source, Path.Combine(link, "target.txt"), mover.Hash(source)));
        string target = Path.Combine(workspace.Downloads, "new", "target.txt");
        Assert.Throws<IOException>(() => mover.Move(source, target, "incorrect hash"));
        Assert.False(Directory.Exists(Path.GetDirectoryName(target)));
        Assert.True(File.Exists(source));
    }

    [MacFact]
    public void CaseAndUnicodeCollisionsNeverOverwriteExistingObjects()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt", "source");
        string existing = workspace.CreateFile("café.txt", "existing");
        Assert.Throws<IOException>(() => mover.Move(source, Path.Combine(workspace.Downloads, "cafe\u0301.txt"), mover.Hash(source)));
        Assert.True(PathSafety.AreSame(existing, Path.Combine(workspace.Downloads, "cafe\u0301.txt")));
        Assert.Equal("existing", File.ReadAllText(existing));
        Assert.Equal("source", File.ReadAllText(source));
        string upper = workspace.CreateFile("CASE.txt", "uppercase");
        string lower = Path.Combine(workspace.Downloads, "case.txt");
        if (File.Exists(lower))
        {
            Assert.Throws<IOException>(() => mover.Move(source, lower, mover.Hash(source)));
            Assert.True(PathSafety.AreSame(upper, lower));
        }
        else
        {
            mover.Move(source, lower, mover.Hash(source));
            Assert.False(PathSafety.AreSame(upper, lower));
            Assert.Equal("source", File.ReadAllText(lower));
        }
        Assert.Equal("uppercase", File.ReadAllText(upper));
    }

    [MacFact]
    public async Task FullWorkflowUsesNativeMovesAndPersistsIdentityAcrossRestart()
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        var mover = new SafeFileMover();
        var clock = new TestClock();
        var organizer = new DownloadOrganizer(store, mover, new MacFileReadiness(TimeSpan.Zero), clock);
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        string source = workspace.CreateFile("sample.pdf", "document");
        FileIdentity identity = mover.GetIdentity(source)!;
        PreviewItem preview = Assert.Single(await organizer.PreviewAsync(settings));
        await organizer.OrganizeAsync(source, settings, expectedHash: preview.ContentHash);
        Assert.Equal(identity, Assert.Single(store.GetOperations()).SourceIdentity);
        clock.Now = clock.Now.AddYears(1);
        await organizer.ReconcileAsync(settings);
        TrackedFile review = Assert.Single(store.GetFiles());
        Assert.Equal(LifecycleStatus.InReview, review.Status);
        await organizer.SnoozeAsync(review.Id, settings);
        Assert.Equal(LifecycleStatus.Snoozed, Assert.Single(store.GetFiles()).Status);
        clock.Now = clock.Now.AddDays(8);
        await organizer.ReconcileAsync(settings);
        await organizer.KeepAsync(review.Id, settings);
        Assert.Equal(LifecycleStatus.Kept, Assert.Single(store.GetFiles()).Status);
        OperationRecord keep = store.GetOperations().Last();
        await organizer.UndoAsync(keep.Id, settings);
        string external = Path.Combine(workspace.Root, "Archive");
        Directory.CreateDirectory(external);
        await organizer.MoveAsync(review.Id, settings, external);
        OperationRecord move = store.GetOperations().Last();
        var reopened = new SqliteStateStore(workspace.Database);
        await new DownloadOrganizer(reopened, mover, new MacFileReadiness(TimeSpan.Zero), clock).UndoAsync(move.Id, settings);
        Assert.Equal(LifecycleStatus.InReview, Assert.Single(reopened.GetFiles()).Status);
        Assert.All(reopened.GetOperations(), operation => Assert.Equal(OperationStatus.Completed, operation.Status));
        Assert.Equal(identity, mover.GetIdentity(Assert.Single(reopened.GetFiles()).CurrentPath));
    }

    [MacFact]
    public async Task RecoveryRequiresIdentityNotOnlyMatchingBytes()
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt", "same bytes");
        string destination = Path.Combine(workspace.Downloads, "target.txt");
        FileIdentity identity = mover.GetIdentity(source)!;
        var file = new TrackedFile(Guid.NewGuid(), source, destination, FileCategory.Documents, DateTimeOffset.UtcNow, LifecycleStatus.Organized, mover.Hash(source));
        var pending = new OperationRecord(Guid.NewGuid(), OperationKind.Organize, OperationStatus.Pending, DateTimeOffset.UtcNow, null, file, source, destination, SourceIdentity: identity);
        store.SaveOperation(pending);
        File.Move(source, source + ".original");
        File.WriteAllText(destination, "same bytes");
        var organizer = new DownloadOrganizer(store, mover, new MacFileReadiness(TimeSpan.Zero));
        await organizer.RetryRecoveryAsync();
        Assert.Equal(OperationStatus.NeedsAttention, Assert.Single(store.GetOperations()).Status);
        File.Delete(destination);
        File.Move(source + ".original", destination);
        await organizer.RetryRecoveryAsync();
        Assert.Equal(OperationStatus.Completed, Assert.Single(store.GetOperations()).Status);
        Assert.Equal(identity, mover.GetIdentity(destination));
    }

    [MacFact]
    public void PermissionDenialRetainsTheSourceAndExistingDestination()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("sample.txt");
        string folder = Path.Combine(workspace.Root, "ReadOnly");
        Directory.CreateDirectory(folder);
        UnixFileMode mode = File.GetUnixFileMode(folder);
        try
        {
            File.SetUnixFileMode(folder, UnixFileMode.UserRead | UnixFileMode.UserExecute);
            Assert.Throws<UnauthorizedAccessException>(() => mover.Move(source, Path.Combine(folder, "sample.txt"), mover.Hash(source)));
        }
        finally { File.SetUnixFileMode(folder, mode); }
        Assert.True(File.Exists(source));
        Assert.Empty(Directory.EnumerateFiles(folder));
    }

    [MacFact]
    public async Task WatcherOrganizesCompletedDownloadsWithTheProductionMover()
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        var organizer = new DownloadOrganizer(store, new SafeFileMover(), new MacFileReadiness(TimeSpan.Zero));
        var errors = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
        await using var monitor = new Dowlume.Monitoring.DownloadMonitor(settings,
            (path, token) => organizer.OrganizeAsync(path, settings, token),
            token => organizer.ReconcileAsync(settings, token), errors.Enqueue);
        workspace.CreateFile("watched.pdf", "watcher data");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        while (store.GetFiles().Count == 0)
            await Task.Delay(50, timeout.Token);
        Assert.Equal(LifecycleStatus.Organized, Assert.Single(store.GetFiles()).Status);
        Assert.Equal("watcher data", File.ReadAllText(Assert.Single(store.GetFiles()).CurrentPath));
        Assert.Empty(errors);
    }

    [MacFact]
    public async Task HardLinksAreRejectedBeforeMutationAndReadinessDoesNotBlockOnFifos()
    {
        using var workspace = new TemporaryWorkspace();
        var mover = new SafeFileMover();
        string source = workspace.CreateFile("source.txt");
        string alias = source + ".alias";
        RunNative("/bin/ln", source, alias);
        string destination = Path.Combine(workspace.Downloads, "new", "target.txt");
        Assert.Throws<NotSupportedException>(() => mover.PrepareMove(source, destination, mover.Hash(source)));
        Assert.False(Directory.Exists(Path.GetDirectoryName(destination)));
        Assert.True(File.Exists(source) && File.Exists(alias));
        string fifo = Path.Combine(workspace.Downloads, "pipe");
        RunNative("/usr/bin/mkfifo", fifo);
        Assert.False(await new MacFileReadiness(TimeSpan.Zero).IsReadyAsync(fifo, default));
    }

    [MacVolumeFact]
    public async Task CrossVolumeRequestFailsBeforeJournalingOrCreatingDirectories()
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        string source = workspace.CreateFile("source.txt");
        string otherFolder = Path.Combine(Environment.GetEnvironmentVariable("DOWLUME_TEST_SECOND_VOLUME")!, Guid.NewGuid().ToString("N"));
        string other = Path.Combine(otherFolder, "target.txt");
        var mover = new SafeFileMover();
        Assert.Throws<NotSupportedException>(() => mover.PrepareMove(source, other, mover.Hash(source)));
        Assert.False(Directory.Exists(otherFolder));
        Directory.CreateDirectory(otherFolder);
        try
        {
            var file = new TrackedFile(Guid.NewGuid(), source, source, FileCategory.Documents, DateTimeOffset.UtcNow, LifecycleStatus.InReview, mover.Hash(source));
            store.SaveFile(file);
            var organizer = new DownloadOrganizer(store, mover, new MacFileReadiness(TimeSpan.Zero));
            await Assert.ThrowsAsync<NotSupportedException>(() => organizer.MoveAsync(file.Id, new AppSettings { DownloadsPath = workspace.Downloads }, otherFolder));
            Assert.Empty(store.GetOperations());
            Assert.Empty(Directory.EnumerateFileSystemEntries(otherFolder));
            Assert.True(File.Exists(source));
        }
        finally { Directory.Delete(otherFolder); }
    }

    [MacVolumeFact]
    public void FullDisposableVolumeNeverLosesOrOverwritesTheSource()
    {
        using var workspace = new TemporaryWorkspace();
        string volume = Environment.GetEnvironmentVariable("DOWLUME_TEST_OWNED_VOLUME")!;
        Assert.True(new DriveInfo(volume).TotalSize < 300L * 1024 * 1024);
        string source = workspace.CreateFile("source.txt", "preserve");
        string destination = Path.Combine(workspace.Downloads, "target.txt");
        var mover = new SafeFileMover();
        string hash = mover.Hash(source);
        string filler = Path.Combine(volume, "capacity-" + Guid.NewGuid().ToString("N"));
        bool exhausted = false;
        try
        {
            using (var fill = new FileStream(filler, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[1024 * 1024];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                try
                {
                    for (int index = 0; index < 300; index++)
                    {
                        fill.Write(buffer);
                        fill.Flush(true);
                    }
                }
                catch (IOException) { exhausted = true; }
            }
            Assert.True(exhausted, "The dedicated sparse image must actually run out of space.");
            try
            {
                mover.Move(source, destination, hash);
            }
            catch (IOException) { }
            Assert.True(File.Exists(source) || File.Exists(destination));
            Assert.Equal("preserve", File.ReadAllText(File.Exists(source) ? source : destination));
        }
        finally { if (File.Exists(filler)) File.Delete(filler); }
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static string RunNative(string executable, params string[] arguments)
    {
        var start = new System.Diagnostics.ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardOutput = true };
        foreach (string argument in arguments)
            start.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(start)!;
        string output = process.StandardOutput.ReadToEnd();
        Assert.True(process.WaitForExit(10000));
        Assert.Equal(0, process.ExitCode);
        return output;
    }

    private static void NativeWrite(string path, string content)
    {
        var start = new System.Diagnostics.ProcessStartInfo("/usr/bin/python3") { UseShellExecute = false };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("import os,sys; f=os.open(sys.argv[1], os.O_WRONLY|os.O_TRUNC); os.write(f,sys.argv[2].encode()); os.close(f)");
        start.ArgumentList.Add(path);
        start.ArgumentList.Add(content);
        using var process = System.Diagnostics.Process.Start(start)!;
        Assert.True(process.WaitForExit(10000));
        Assert.Equal(0, process.ExitCode);
    }
}
