using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Dowlume.Core;
using Microsoft.Win32.SafeHandles;

namespace Dowlume.Infrastructure;

// Linux x64 ABI. No copying, unlinking, shell commands, or overwrite fallback.
[SupportedOSPlatform("linux")]
internal static class LinuxFileSystem
{
    private const int NoFollow = 0x20000, CloseOnExec = 0x80000, NonBlock = 0x800, DirectoryFlag = 0x10000;
    private const uint RequiredStats = 0x1fff; // BASIC_STATS | BTIME | MNT_ID
    internal const string VolumeMessage = "Linux file operations require local ext4 folders on the same mount, with a verifiable filesystem UUID and creation time. Choose another destination.";

    internal sealed record Snapshot(uint DeviceMajor, uint DeviceMinor, ulong MountId, ulong Inode,
        uint LinkCount, ushort Mode, ulong Size, long ModifiedSeconds, uint ModifiedNanoseconds,
        long ChangedSeconds, uint ChangedNanoseconds, long BirthSeconds, uint BirthNanoseconds);

    internal static Snapshot Inspect(SafeFileHandle handle)
    {
        byte[] data = new byte[256];
        if (StatX(handle, "", 0x1000, RequiredStats, data) != 0)
            throw Error();
        if ((BitConverter.ToUInt32(data, 0) & RequiredStats) != RequiredStats)
            throw new NotSupportedException(VolumeMessage);
        return new Snapshot(BitConverter.ToUInt32(data, 136), BitConverter.ToUInt32(data, 140),
            BitConverter.ToUInt64(data, 144), BitConverter.ToUInt64(data, 32), BitConverter.ToUInt32(data, 16),
            BitConverter.ToUInt16(data, 28), BitConverter.ToUInt64(data, 40),
            BitConverter.ToInt64(data, 112), BitConverter.ToUInt32(data, 120),
            BitConverter.ToInt64(data, 96), BitConverter.ToUInt32(data, 104),
            BitConverter.ToInt64(data, 80), BitConverter.ToUInt32(data, 88));
    }

    internal static Snapshot Capture(SafeFileHandle handle)
    {
        Snapshot snapshot = Inspect(handle);
        if ((snapshot.Mode & 0xf000) != 0x8000)
            throw new IOException("Native reads require a regular file with locally available content.");
        return snapshot;
    }

    // Walk every component relative to an open directory: O_NOFOLLOW on just the final
    // pathname would still follow symlinks in ancestors. O_NONBLOCK rejects FIFOs promptly.
    internal static SafeFileHandle OpenDirectory(string path)
    {
        RequirePlatform();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Path.IsPathFullyQualified(path) || path.Contains('\0'))
            throw new ArgumentException("An absolute path without null characters is required.");
        SafeFileHandle current = DirectoryHandle(Open("/", DirectoryFlag | NoFollow | CloseOnExec));
        try
        {
            foreach (string part in Path.GetFullPath(path).Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                SafeFileHandle next = OpenDirectoryAt(current, part);
                current.Dispose();
                current = next;
            }
            return current;
        }
        catch { current.Dispose(); throw; }
    }

    internal static SafeFileHandle OpenDirectoryAt(SafeFileHandle parent, string name) =>
        DirectoryHandle(OpenAt(parent, name, DirectoryFlag | NoFollow | CloseOnExec));

    private static SafeFileHandle DirectoryHandle(int descriptor)
    {
        SafeFileHandle handle = Wrap(descriptor);
        try
        {
            // Check every ancestor, not only the selected folder: ext4 casefold can
            // alias an ancestor even when its child no longer has the casefold flag.
            // Non-ext4 ancestors may not implement this ioctl; Volume independently
            // requires ext4 and a successful ioctl for the actual mutation parents.
            if (GetFlags(handle, 0x80086601, out ulong flags) == 0 && (flags & 0x40000000) != 0)
                throw new NotSupportedException(VolumeMessage);
            return handle;
        }
        catch { handle.Dispose(); throw; }
    }

    internal static FileStream OpenRead(string path)
    {
        string full = Path.GetFullPath(path);
        using SafeFileHandle parent = OpenDirectory(Path.GetDirectoryName(full)!);
        SafeFileHandle handle = Wrap(OpenAt(parent, Path.GetFileName(full), NoFollow | CloseOnExec | NonBlock));
        try
        {
            _ = Capture(handle);
            // Repeated verification must read the kernel, never a retained managed read buffer.
            return new FileStream(handle, FileAccess.Read, bufferSize: 1, isAsync: false);
        }
        catch { handle.Dispose(); throw; }
    }

    internal static string Volume(SafeFileHandle handle)
    {
        Snapshot stat = Inspect(handle);
        // Match the descriptor's mount, not a pathname prefix. This also excludes overlay,
        // FUSE, network filesystems, tmpfs, and ext2/3 despite their shared statfs magic.
        string? mount = File.ReadLines("/proc/self/mountinfo").SingleOrDefault(line =>
            line.StartsWith(stat.MountId.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ", StringComparison.Ordinal));
        if (mount is null || !mount.Split(" - ", StringSplitOptions.None).Last().StartsWith("ext4 ", StringComparison.Ordinal))
            throw new NotSupportedException(VolumeMessage);
        // ext4 casefold directories invalidate ordinal journal/path comparisons.
        if (GetFlags(handle, 0x80086601, out ulong flags) != 0)
            throw Error();
        if ((flags & 0x40000000) != 0)
            throw new NotSupportedException(VolumeMessage);
        // UUID survives remounts/reboots, unlike st_dev and mount ID. Never substitute a
        // device number if udev's UUID mapping is unavailable or ambiguous.
        if (!Directory.Exists("/dev/disk/by-uuid"))
            throw new NotSupportedException(VolumeMessage);
        var uuids = new List<string>();
        foreach (string entry in Directory.EnumerateFileSystemEntries("/dev/disk/by-uuid"))
        {
            if (!Guid.TryParseExact(Path.GetFileName(entry), "D", out Guid uuid))
                continue;
            byte[] data = new byte[256];
            if (StatXPath(-100, entry, 0, 0x7ff, data) != 0)
                continue;
            if ((BitConverter.ToUInt16(data, 28) & 0xf000) == 0x6000 &&
                BitConverter.ToUInt32(data, 128) == stat.DeviceMajor && BitConverter.ToUInt32(data, 132) == stat.DeviceMinor)
                uuids.Add(uuid.ToString("D"));
        }
        if (uuids.Count != 1 || Inspect(handle) != stat)
            throw new NotSupportedException(VolumeMessage);
        return "linux:ext4:" + uuids[0];
    }

    internal static FileIdentity Identity(SafeFileHandle handle)
    {
        Snapshot stat = Inspect(handle);
        return new FileIdentity(Volume(handle), stat.Inode, stat.BirthSeconds, stat.BirthNanoseconds);
    }

    internal static void VerifyUnchanged(string path, SafeFileHandle handle, Snapshot before)
    {
        using FileStream current = OpenRead(path);
        if (Capture(handle) != before || Capture(current.SafeFileHandle) != before)
            throw new IOException("The file changed after it was tracked. Refresh the preview.");
    }

    internal static void VerifyDirectory(SafeFileHandle handle, string path)
    {
        using SafeFileHandle current = OpenDirectory(path);
        Snapshot left = Inspect(handle), right = Inspect(current);
        if (left.DeviceMajor != right.DeviceMajor || left.DeviceMinor != right.DeviceMinor ||
            left.MountId != right.MountId || left.Inode != right.Inode ||
            left.BirthSeconds != right.BirthSeconds || left.BirthNanoseconds != right.BirthNanoseconds)
            throw new IOException("A folder changed during the operation.");
    }

    internal static string Hash(string path)
    {
        using FileStream stream = OpenRead(path);
        Snapshot before = Capture(stream.SafeFileHandle);
        string hash = Convert.ToHexString(SHA256.HashData(stream));
        VerifyUnchanged(path, stream.SafeFileHandle, before);
        return hash;
    }

    internal static string? GetPath(SafeFileHandle handle)
    {
        // Diagnostic only. Never used as authority for recovery or mutation.
        byte[] buffer = new byte[4096];
        long length = ReadLink($"/proc/self/fd/{handle.DangerousGetHandle().ToInt64()}", buffer, (nuint)buffer.Length);
        if (length <= 0 || length >= buffer.Length)
            return null;
        string path = Encoding.UTF8.GetString(buffer, 0, (int)length);
        return path.EndsWith(" (deleted)", StringComparison.Ordinal) ? null : path;
    }

    internal static (string Parent, string Tail) ExistingAncestor(string path)
    {
        var parts = new Stack<string>();
        string current = Path.GetFullPath(path);
        while (!Directory.Exists(current))
        {
            parts.Push(Path.GetFileName(current));
            current = Path.GetDirectoryName(current) ?? throw new IOException("No existing parent folder was found.");
        }
        return (current, parts.Count == 0 ? "" : Path.Combine(parts.ToArray()));
    }

    internal static void CreateDirectoryAt(SafeFileHandle parent, string name)
    {
        if (MkdirAt(parent, name, 0x1c0) != 0 && Marshal.GetLastPInvokeError() != 17)
            throw Error();
        FlushDirectory(parent); // Persist new directory entries before the final rename.
    }

    internal static void RenameExclusive(SafeFileHandle sourceParent, string source, SafeFileHandle destinationParent, string destination)
    {
        if (RenameAt2(sourceParent, source, destinationParent, destination, 1) != 0)
            throw Error(); // RENAME_NOREPLACE; no weaker fallback on ENOSYS/EINVAL/EXDEV.
    }

    internal static void FlushDirectory(SafeFileHandle handle)
    {
        if (FSync(handle) != 0)
            throw Error();
    }

    internal static void Lock(SafeFileHandle handle)
    {
        if (FLock(handle, 6) != 0)
            throw Error(); // Advisory LOCK_EX | LOCK_NB, supplemented by content checks.
    }

    private static void RequirePlatform()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("Native Linux file operations require Linux x64.");
    }

    private static SafeFileHandle Wrap(int fd) => fd < 0 ? throw Error() : new SafeFileHandle((IntPtr)fd, true);
    private static Exception Error()
    {
        int error = Marshal.GetLastPInvokeError();
        return error is 1 or 13
            ? new UnauthorizedAccessException("Linux denied access to the file or folder.", new Win32Exception(error))
            : new IOException("Linux could not safely complete the file operation.", new Win32Exception(error));
    }

    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int Open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAt(SafeFileHandle parent, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int flags);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
    private static extern int StatX(SafeFileHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags, uint mask, [Out] byte[] data);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
    private static extern int StatXPath(int fd, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags, uint mask, [Out] byte[] data);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    private static extern int GetFlags(SafeFileHandle handle, nuint request, out ulong flags);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
    private static extern long ReadLink([MarshalAs(UnmanagedType.LPUTF8Str)] string path, [Out] byte[] buffer, nuint length);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "mkdirat", SetLastError = true)]
    private static extern int MkdirAt(SafeFileHandle parent, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, uint mode);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "renameat2", SetLastError = true)]
    private static extern int RenameAt2(SafeFileHandle sourceParent, [MarshalAs(UnmanagedType.LPUTF8Str)] string source,
        SafeFileHandle destinationParent, [MarshalAs(UnmanagedType.LPUTF8Str)] string destination, uint flags);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int FSync(SafeFileHandle handle);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FLock(SafeFileHandle handle, int operation);
}
