using System.Security.Cryptography;
using Dowlume.Core;

namespace Dowlume.Infrastructure;

public sealed class SafeFileMover : IFileMover, IPreparedFileMover
{
    public bool CanMove => OperatingSystem.IsWindows() ||
        (OperatingSystem.IsLinux() && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64) ||
        (OperatingSystem.IsMacOSVersionAtLeast(15) && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64);

    public IPreparedFileMove PrepareMove(string source, string destination, string expectedHash) => OperatingSystem.IsMacOS()
        ? new MacFileMove(source, destination, expectedHash)
        : OperatingSystem.IsLinux() ? new LinuxFileMove(source, destination, expectedHash)
        : new DeferredFileMove(() => Move(source, destination, expectedHash));

    public FileIdentity? GetIdentity(string path)
    {
        if (OperatingSystem.IsLinux())
        {
            using FileStream native = LinuxFileSystem.OpenRead(path);
            if (LinuxFileSystem.Capture(native.SafeFileHandle).LinkCount != 1)
                throw new IOException("The file identity changed.");
            return LinuxFileSystem.Identity(native.SafeFileHandle);
        }
        if (!OperatingSystem.IsMacOS())
            return null;
        using FileStream stream = MacFileHandle.OpenRead(path);
        MacFileSystem.RequireLocalApfs(stream.SafeFileHandle);
        if (MacFileHandle.Capture(stream.SafeFileHandle).LinkCount != 1)
            throw new IOException("The file identity changed.");
        return MacFileSystem.Identity(stream.SafeFileHandle);
    }

    public bool MatchesRecoveryFile(string path, FileIdentity identity, string expectedHash)
    {
        if (OperatingSystem.IsLinux())
        {
            using FileStream native = LinuxFileSystem.OpenRead(path);
            LinuxFileSystem.Snapshot snapshot = LinuxFileSystem.Capture(native.SafeFileHandle);
            if (snapshot.LinkCount != 1 || LinuxFileSystem.Identity(native.SafeFileHandle) != identity)
                return false;
            bool match = Convert.ToHexString(SHA256.HashData(native)) == expectedHash;
            LinuxFileSystem.VerifyUnchanged(path, native.SafeFileHandle, snapshot);
            return match;
        }
        if (!OperatingSystem.IsMacOS())
            return false;
        using FileStream stream = MacFileHandle.OpenRead(path);
        MacFileSystem.RequireLocalApfs(stream.SafeFileHandle);
        MacFileHandle.Snapshot before = MacFileHandle.Capture(stream.SafeFileHandle);
        if (before.LinkCount != 1 || MacFileSystem.Identity(stream.SafeFileHandle) != identity)
            return false;
        bool matches = Convert.ToHexString(SHA256.HashData(stream)) == expectedHash;
        MacFileHandle.VerifyUnchanged(path, stream.SafeFileHandle, before);
        return matches;
    }

    public string Hash(string path)
    {
        if (OperatingSystem.IsLinux())
            return LinuxFileSystem.Hash(path);
        if (OperatingSystem.IsMacOS())
        {
            using FileStream native = MacFileHandle.OpenRead(path);
            MacFileHandle.Snapshot before = MacFileHandle.Capture(native.SafeFileHandle);
            string hash = Convert.ToHexString(SHA256.HashData(native));
            MacFileHandle.VerifyUnchanged(path, native.SafeFileHandle, before);
            return hash;
        }
        PathSafety.RejectLinks(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.SequentialScan);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    public void Move(string source, string destination, string expectedHash)
    {
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            using IPreparedFileMove move = PrepareMove(source, destination, expectedHash);
            move.Execute();
            return;
        }
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(DownloadOrganizer.ReadOnlyMessage);
        }
        PathSafety.RejectLinks(source);
        PathSafety.RejectLinks(destination);
        MoveOnWindows(source, destination, expectedHash);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void MoveOnWindows(string source, string destination, string expectedHash)
    {
        using var directories = DirectoryLease.Acquire(source, destination);
        using FileStream verifiedSource = WindowsFileHandle.OpenForMove(source);
        VerifyHash(verifiedSource, expectedHash);
        if (WindowsFileHandle.TryRename(verifiedSource.SafeFileHandle, destination))
        {
            return;
        }
        WindowsFileHandle.CopyWithoutReplacing(source, destination);
        // Keep the verified copy locked until the original handle is closed/deleted.
        using var copy = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.Read);
        VerifyHash(copy, expectedHash);
        WindowsFileHandle.DeleteVerifiedSource(verifiedSource.SafeFileHandle);
        verifiedSource.Dispose();
    }

    private static void VerifyHash(Stream stream, string expectedHash)
    {
        if (Convert.ToHexString(SHA256.HashData(stream)) != expectedHash)
        {
            throw new IOException("The file changed after it was tracked. Review it before retrying.");
        }
    }

    public FileStream OpenVerifiedRead(string path, string expectedHash)
    {
        if (OperatingSystem.IsLinux())
        {
            FileStream native = LinuxFileSystem.OpenRead(path);
            try
            {
                LinuxFileSystem.Snapshot before = LinuxFileSystem.Capture(native.SafeFileHandle);
                VerifyHash(native, expectedHash);
                LinuxFileSystem.VerifyUnchanged(path, native.SafeFileHandle, before);
                return native;
            }
            catch { native.Dispose(); throw; }
        }
        if (OperatingSystem.IsMacOS())
        {
            FileStream native = MacFileHandle.OpenRead(path);
            try
            {
                MacFileHandle.Snapshot before = MacFileHandle.Capture(native.SafeFileHandle);
                VerifyHash(native, expectedHash);
                MacFileHandle.VerifyUnchanged(path, native.SafeFileHandle, before);
                // Verification describes this read; it does not grant a mutation lease.
                return native;
            }
            catch
            {
                native.Dispose();
                throw;
            }
        }
        PathSafety.RejectLinks(path);
        // Deny writes while processing; delete sharing permits Windows shell moves.
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920, FileOptions.SequentialScan);
        try
        {
            VerifyHash(stream, expectedHash);
            return stream;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }
}
