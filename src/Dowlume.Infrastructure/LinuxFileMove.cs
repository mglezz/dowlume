using System.Runtime.Versioning;
using System.Security.Cryptography;
using Dowlume.Core;
using Microsoft.Win32.SafeHandles;

namespace Dowlume.Infrastructure;

// Same-volume atomic rename only: never copies, unlinks, overwrites, or rolls back by pathname.
[SupportedOSPlatform("linux")]
internal sealed class LinuxFileMove : IPreparedFileMove
{
    private readonly string _source, _destination, _expectedHash, _sourceFolder, _ancestor, _tail;
    private FileStream? _stream;
    private SafeFileHandle? _sourceParent, _destinationAncestor;
    private readonly List<SafeFileHandle> _createdParents = [];
    private readonly Action? _beforeRename, _afterRename;
    private bool _executed;
    private string? _observedPath;
    public FileIdentity SourceIdentity
    {
        get;
    }
    public string? ObservedPath
    {
        get
        {
            try
            {
                return _stream is null ? _observedPath : LinuxFileSystem.GetPath(_stream.SafeFileHandle);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return null; }
        }
    }

    internal LinuxFileMove(string source, string destination, string expectedHash, Action? beforeRename = null, Action? afterRename = null)
    {
        _source = Path.GetFullPath(source);
        _destination = Path.GetFullPath(destination);
        _expectedHash = expectedHash;
        _beforeRename = beforeRename;
        _afterRename = afterRename;
        _sourceFolder = Path.GetDirectoryName(_source)!;
        (_ancestor, _tail) = LinuxFileSystem.ExistingAncestor(Path.GetDirectoryName(_destination)!);
        try
        {
            PathSafety.RejectLinks(_source);
            PathSafety.RejectLinks(_destination);
            _sourceParent = LinuxFileSystem.OpenDirectory(_sourceFolder);
            _destinationAncestor = LinuxFileSystem.OpenDirectory(_ancestor);
            _stream = LinuxFileSystem.OpenRead(_source);
            _ = LinuxFileSystem.Volume(_sourceParent);
            _ = LinuxFileSystem.Volume(_destinationAncestor);
            SourceIdentity = LinuxFileSystem.Identity(_stream.SafeFileHandle);
            if (SourceIdentity.Volume != LinuxFileSystem.Volume(_destinationAncestor) ||
                LinuxFileSystem.Inspect(_sourceParent).MountId != LinuxFileSystem.Inspect(_destinationAncestor).MountId ||
                LinuxFileSystem.Inspect(_sourceParent).MountId != LinuxFileSystem.Inspect(_stream.SafeFileHandle).MountId)
                throw new NotSupportedException(LinuxFileSystem.VolumeMessage);
            if (LinuxFileSystem.Capture(_stream.SafeFileHandle).LinkCount != 1)
                throw new NotSupportedException("Files with multiple hard links cannot be organized. Choose an independent copy.");
            // Cooperative locks are useful but not a guarantee against POSIX writers.
            LinuxFileSystem.Lock(_stream.SafeFileHandle);
            VerifyContent();
            LinuxFileSystem.VerifyUnchanged(_source, _stream.SafeFileHandle, LinuxFileSystem.Capture(_stream.SafeFileHandle));
            if (Path.Exists(_destination))
                throw new IOException("The destination already exists.");
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Execute()
    {
        if (_executed)
            throw new InvalidOperationException("A prepared move can only execute once.");
        _executed = true;
        ObjectDisposedException.ThrowIf(_stream is null, this);
        LinuxFileSystem.VerifyDirectory(_sourceParent!, _sourceFolder);
        LinuxFileSystem.VerifyDirectory(_destinationAncestor!, _ancestor);
        VerifyContent();
        using (FileStream current = LinuxFileSystem.OpenRead(_source))
        {
            if (LinuxFileSystem.Identity(current.SafeFileHandle) != SourceIdentity)
                throw new IOException("The source was replaced before moving.");
        }
        SafeFileHandle destinationParent = _destinationAncestor!;
        foreach (string component in _tail.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            LinuxFileSystem.CreateDirectoryAt(destinationParent, component);
            SafeFileHandle next = LinuxFileSystem.OpenDirectoryAt(destinationParent, component);
            _createdParents.Add(next);
            if (LinuxFileSystem.Volume(next) != SourceIdentity.Volume ||
                LinuxFileSystem.Inspect(next).MountId != LinuxFileSystem.Inspect(_sourceParent!).MountId)
                throw new IOException("A destination mount changed.");
            destinationParent = next;
        }
        LinuxFileSystem.VerifyDirectory(destinationParent, Path.GetDirectoryName(_destination)!);
        _beforeRename?.Invoke();
        LinuxFileSystem.VerifyDirectory(_sourceParent!, _sourceFolder);
        LinuxFileSystem.VerifyDirectory(destinationParent, Path.GetDirectoryName(_destination)!);
        LinuxFileSystem.VerifyUnchanged(_source, _stream!.SafeFileHandle, LinuxFileSystem.Capture(_stream.SafeFileHandle));
        VerifyContent();
        LinuxFileSystem.FlushDirectory(_stream.SafeFileHandle);
        LinuxFileSystem.VerifyUnchanged(_source, _stream.SafeFileHandle, LinuxFileSystem.Capture(_stream.SafeFileHandle));
        LinuxFileSystem.RenameExclusive(_sourceParent!, Path.GetFileName(_source), destinationParent, Path.GetFileName(_destination));
        _afterRename?.Invoke();
        LinuxFileSystem.FlushDirectory(destinationParent);
        LinuxFileSystem.FlushDirectory(_sourceParent!);
        LinuxFileSystem.VerifyDirectory(_sourceParent!, _sourceFolder);
        LinuxFileSystem.VerifyDirectory(destinationParent, Path.GetDirectoryName(_destination)!);
        using FileStream result = LinuxFileSystem.OpenRead(_destination);
        if (LinuxFileSystem.Identity(result.SafeFileHandle) != SourceIdentity || Path.Exists(_source))
            throw new IOException("A file path changed during the move. Inspect the retained files.");
        LinuxFileSystem.Snapshot finalSnapshot = LinuxFileSystem.Capture(_stream.SafeFileHandle);
        VerifyContent();
        LinuxFileSystem.VerifyUnchanged(_destination, _stream.SafeFileHandle, finalSnapshot);
    }

    private void VerifyContent()
    {
        FileStream stream = _stream!;
        LinuxFileSystem.Snapshot before = LinuxFileSystem.Capture(stream.SafeFileHandle);
        if (before.LinkCount != 1 || LinuxFileSystem.Identity(stream.SafeFileHandle) != SourceIdentity)
            throw new IOException("The file identity changed.");
        stream.Position = 0;
        if (Convert.ToHexString(SHA256.HashData(stream)) != _expectedHash || LinuxFileSystem.Capture(stream.SafeFileHandle) != before)
            throw new IOException("The file changed after it was tracked. Refresh the preview.");
    }

    public void Dispose()
    {
        _observedPath = ObservedPath;
        _stream?.Dispose();
        _stream = null;
        foreach (SafeFileHandle handle in _createdParents)
            handle.Dispose();
        _destinationAncestor?.Dispose();
        _sourceParent?.Dispose();
    }

}
