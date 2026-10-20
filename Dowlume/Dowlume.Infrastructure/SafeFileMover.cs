using System.Security.Cryptography;
using Dowlume.Core;

namespace Dowlume.Infrastructure;

public sealed class SafeFileMover : IFileMover
{
    public bool CanMove => OperatingSystem.IsWindows();

    public string Hash(string path)
    {
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
            throw new IOException("The file changed after it was tracked. Review it in Explorer before retrying.");
        }
    }

    public FileStream OpenVerifiedRead(string path, string expectedHash)
    {
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
