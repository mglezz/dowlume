using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Dowlume.Infrastructure;

// Read-only Darwin adapter. None of these checks excludes a concurrent writer.
[SupportedOSPlatform("macos")]
internal static class MacFileHandle
{
    private const string LibSystem = "/usr/lib/libSystem.B.dylib";
    // O_NOFOLLOW_ANY | O_CLOEXEC | O_NONBLOCK. Darwin rejects combining
    // O_NOFOLLOW_ANY with the redundant final-component O_NOFOLLOW flag.
    private const int ReadFlags = 0x20000000 | 0x01000000 | 0x00000004;
    private const ushort FileTypeMask = 0xf000;
    private const ushort RegularFile = 0x8000;
    private const uint Dataless = 0x40000000;

    internal static FileStream OpenRead(string path)
    {
        if (!OperatingSystem.IsMacOSVersionAtLeast(15) || RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            throw new PlatformNotSupportedException("Native macOS reads require macOS 15 or later on Apple Silicon.");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Path.IsPathFullyQualified(path) || path.Contains('\0'))
        {
            throw new ArgumentException("An absolute path without null characters is required.", nameof(path));
        }
        // O_NOFOLLOW_ANY checks every component during kernel resolution, closing the
        // gap between a managed link check and open. O_NONBLOCK avoids waiting on FIFOs.
        int descriptor;
        do
        {
            descriptor = Open(path, ReadFlags);
        } while (descriptor == -1 && Marshal.GetLastPInvokeError() == 4);
        if (descriptor == -1)
        {
            throw NativeError(Marshal.GetLastPInvokeError());
        }
        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            _ = Capture(handle);
            return new FileStream(handle, FileAccess.Read, 81920, isAsync: false);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static Snapshot Capture(SafeFileHandle handle)
    {
        Snapshot snapshot = Inspect(handle);
        if ((snapshot.Mode & FileTypeMask) != RegularFile || (snapshot.Flags & Dataless) != 0)
        {
            throw new IOException("Native reads require a regular file with locally available content.");
        }
        return snapshot;
    }

    internal static Snapshot Inspect(SafeFileHandle handle)
    {
        if (FStat(handle, out DarwinStat stat) != 0)
        {
            throw NativeError(Marshal.GetLastPInvokeError());
        }
        return new Snapshot(stat.Device, stat.Inode, stat.LinkCount, stat.Mode, stat.Size,
            stat.ModifiedSeconds, stat.ModifiedNanoseconds, stat.ChangedSeconds, stat.ChangedNanoseconds,
            stat.BirthSeconds, stat.BirthNanoseconds, stat.Flags);
    }

    internal static bool IsCaseSensitive(SafeFileHandle handle)
    {
        // Darwin _PC_CASE_SENSITIVE; query the opened volume instead of assuming an OS policy.
        long result = FPathConf(handle, 11);
        return result switch
        {
            0 => false,
            1 => true,
            _ => throw new IOException("The filesystem did not report its case-sensitivity policy.")
        };
    }

    internal static void VerifyUnchanged(string path, SafeFileHandle handle, Snapshot before)
    {
        if (Capture(handle) != before)
        {
            throw new IOException("The file changed while it was being read. Refresh the preview.");
        }
        using FileStream current = OpenRead(path);
        if (Capture(current.SafeFileHandle) != before)
        {
            throw new IOException("The file path changed while it was being read. Refresh the preview.");
        }
    }

    private static Exception NativeError(int error) => error is 1 or 13
        ? new UnauthorizedAccessException("macOS denied access to the file.", new Win32Exception(error))
        : new IOException("macOS could not safely open or inspect the file.", new Win32Exception(error));

    internal readonly record struct Snapshot(int Device, ulong Inode, ushort LinkCount, ushort Mode, long Size,
        long ModifiedSeconds, long ModifiedNanoseconds, long ChangedSeconds, long ChangedNanoseconds,
        long BirthSeconds, long BirthNanoseconds, uint Flags);

    // Darwin ARM64 stat ABI from the macOS SDK sys/stat.h (64-bit inode layout).
    // Access time is intentionally excluded from Snapshot: the read itself can update it.
    [StructLayout(LayoutKind.Explicit, Size = 144)]
    private struct DarwinStat
    {
        [FieldOffset(0)] public int Device;
        [FieldOffset(4)] public ushort Mode;
        [FieldOffset(6)] public ushort LinkCount;
        [FieldOffset(8)] public ulong Inode;
        [FieldOffset(48)] public long ModifiedSeconds;
        [FieldOffset(56)] public long ModifiedNanoseconds;
        [FieldOffset(64)] public long ChangedSeconds;
        [FieldOffset(72)] public long ChangedNanoseconds;
        [FieldOffset(80)] public long BirthSeconds;
        [FieldOffset(88)] public long BirthNanoseconds;
        [FieldOffset(96)] public long Size;
        [FieldOffset(116)] public uint Flags;
    }

    [DllImport(LibSystem, EntryPoint = "open", SetLastError = true)]
    private static extern int Open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);

    [DllImport(LibSystem, EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStat(SafeFileHandle descriptor, out DarwinStat stat);

    [DllImport(LibSystem, EntryPoint = "fpathconf", SetLastError = true)]
    private static extern long FPathConf(SafeFileHandle descriptor, int name);
}
