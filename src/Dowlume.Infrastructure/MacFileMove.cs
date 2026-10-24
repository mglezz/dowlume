using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using Dowlume.Core;
using Microsoft.Win32.SafeHandles;

namespace Dowlume.Infrastructure;

// Same-volume atomic rename only: never copies, unlinks, overwrites, or rolls back by pathname.
[SupportedOSPlatform("macos")]
internal sealed class MacFileMove : IPreparedFileMove
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
                return _stream is null ? _observedPath : MacFileSystem.GetPath(_stream.SafeFileHandle);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return null; }
        }
    }

    internal MacFileMove(string source, string destination, string expectedHash, Action? beforeRename = null, Action? afterRename = null)
    {
        _source = Path.GetFullPath(source);
        _destination = Path.GetFullPath(destination);
        _expectedHash = expectedHash;
        _beforeRename = beforeRename;
        _afterRename = afterRename;
        _sourceFolder = Path.GetDirectoryName(_source)!;
        (_ancestor, _tail) = MacFileSystem.ExistingAncestor(Path.GetDirectoryName(_destination)!);
        try
        {
            PathSafety.RejectLinks(_source);
            PathSafety.RejectLinks(_destination);
            _sourceParent = MacFileSystem.OpenDirectory(_sourceFolder);
            _destinationAncestor = MacFileSystem.OpenDirectory(_ancestor);
            _stream = MacFileHandle.OpenRead(_source);
            MacFileSystem.RequireLocalApfs(_sourceParent);
            MacFileSystem.RequireLocalApfs(_destinationAncestor);
            MacFileSystem.RequireLocalApfs(_stream.SafeFileHandle);
            SourceIdentity = MacFileSystem.Identity(_stream.SafeFileHandle);
            if (SourceIdentity.Volume != MacFileSystem.Volume(_destinationAncestor))
                throw new NotSupportedException(MacFileSystem.VolumeMessage);
            if (MacFileHandle.Capture(_stream.SafeFileHandle).LinkCount != 1)
                throw new NotSupportedException("Files with multiple hard links cannot be organized. Choose an independent copy.");
            // Cooperative locks are useful but not a guarantee against POSIX writers.
            if (FLock(_stream.SafeFileHandle, 6) != 0)
                throw new IOException("The file is busy or still changing. Try again after it finishes.");
            VerifyContent();
            MacFileHandle.VerifyUnchanged(_source, _stream.SafeFileHandle, MacFileHandle.Capture(_stream.SafeFileHandle));
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
        MacFileSystem.VerifyDirectory(_sourceParent!, _sourceFolder);
        MacFileSystem.VerifyDirectory(_destinationAncestor!, _ancestor);
        VerifyContent();
        using (FileStream current = MacFileHandle.OpenRead(_source))
        {
            if (MacFileSystem.Identity(current.SafeFileHandle) != SourceIdentity)
                throw new IOException("The source was replaced before moving.");
        }
        SafeFileHandle destinationParent = _destinationAncestor!;
        foreach (string component in _tail.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            MacFileSystem.CreateDirectoryAt(destinationParent, component);
            SafeFileHandle next = MacFileSystem.OpenDirectoryAt(destinationParent, component);
            _createdParents.Add(next);
            MacFileSystem.RequireLocalApfs(next);
            if (MacFileSystem.Volume(next) != SourceIdentity.Volume)
                throw new IOException("A destination mount changed.");
            destinationParent = next;
        }
        MacFileSystem.VerifyDirectory(destinationParent, Path.GetDirectoryName(_destination)!);
        _beforeRename?.Invoke();
        MacFileSystem.RenameExclusive(_sourceParent!, Path.GetFileName(_source), destinationParent, Path.GetFileName(_destination));
        _afterRename?.Invoke();
        MacFileSystem.FlushDirectory(destinationParent);
        MacFileSystem.FlushDirectory(_sourceParent!);
        MacFileSystem.VerifyDirectory(_sourceParent!, _sourceFolder);
        MacFileSystem.VerifyDirectory(destinationParent, Path.GetDirectoryName(_destination)!);
        using FileStream result = MacFileHandle.OpenRead(_destination);
        if (MacFileSystem.Identity(result.SafeFileHandle) != SourceIdentity || Path.Exists(_source))
            throw new IOException("A file path changed during the move. Inspect the retained files.");
        VerifyContent();
    }

    private void VerifyContent()
    {
        FileStream stream = _stream!;
        MacFileHandle.Snapshot before = MacFileHandle.Capture(stream.SafeFileHandle);
        if (before.LinkCount != 1 || MacFileSystem.Identity(stream.SafeFileHandle) != SourceIdentity)
            throw new IOException("The file identity changed.");
        stream.Position = 0;
        if (Convert.ToHexString(SHA256.HashData(stream)) != _expectedHash || MacFileHandle.Capture(stream.SafeFileHandle) != before)
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

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "flock", SetLastError = true)]
    private static extern int FLock(SafeFileHandle handle, int operation);
}
