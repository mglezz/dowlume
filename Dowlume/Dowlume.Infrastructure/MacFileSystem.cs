using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Dowlume.Core;
using Microsoft.Win32.SafeHandles;

namespace Dowlume.Infrastructure;

[SupportedOSPlatform("macos")]
internal static class MacFileSystem
{
    private const string Library = "/usr/lib/libSystem.B.dylib";
    private const int DirectoryFlags = 0x20000000 | 0x01000000 | 0x00100000 | 4;
    internal const string VolumeMessage = "macOS file operations require local APFS folders on the same volume. Choose another destination.";

    internal static SafeFileHandle OpenDirectory(string path) => Wrap(Open(path, DirectoryFlags));
    internal static SafeFileHandle OpenDirectoryAt(SafeFileHandle parent, string name) => Wrap(OpenAt(parent, name, DirectoryFlags));

    private static SafeFileHandle Wrap(int descriptor)
    {
        if (descriptor < 0)
            throw Error();
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    internal static FileIdentity Identity(SafeFileHandle handle)
    {
        MacFileHandle.Snapshot stat = MacFileHandle.Inspect(handle);
        return new FileIdentity(Volume(handle), stat.Inode, stat.BirthSeconds, stat.BirthNanoseconds);
    }

    internal static string Volume(SafeFileHandle handle)
    {
        var attributes = new AttributeList { Count = 5, Volume = 0x80040000 };
        byte[] data = new byte[64];
        if (FGetAttrList(handle, ref attributes, data, (nuint)data.Length, 0) != 0)
            throw Error();
        if (BitConverter.ToUInt32(data) != 20)
            throw new IOException("The volume identity could not be verified.");
        return Convert.ToHexString(data.AsSpan(4, 16));
    }

    internal static void RequireLocalApfs(SafeFileHandle handle)
    {
        // Darwin ARM64 statfs: f_flags at 64, f_fstypename[16] at 72.
        byte[] data = new byte[4096];
        if (FStatFs(handle, data) != 0)
            throw Error();
        string type = Encoding.ASCII.GetString(data, 72, 16).TrimEnd('\0');
        if (type != "apfs" || (BitConverter.ToUInt32(data, 64) & 0x1000) == 0)
            throw new NotSupportedException(VolumeMessage);
        string path = GetPath(handle).TrimEnd('/') + "/";
        if (path.Contains("/Library/CloudStorage/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/Library/Mobile Documents/", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Cloud-managed folders are not supported. Choose a local folder.");
        long size = FListXAttr(handle, null, 0, 0);
        if (size < 0)
            throw Error();
        if (size > 65536)
            throw new IOException("The extended metadata could not be inspected safely.");
        if (size == 0)
            return;
        byte[] names = new byte[size];
        long read = FListXAttr(handle, names, (nuint)names.Length, 0);
        if (read < 0)
            throw Error();
        if (Encoding.UTF8.GetString(names, 0, (int)read).Split('\0').Any(name =>
            name.StartsWith("com.apple.fileprovider", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("com.apple.ubiquity", StringComparison.OrdinalIgnoreCase)))
            throw new NotSupportedException("Cloud-managed folders are not supported. Choose a local folder.");
    }

    internal static string GetPath(SafeFileHandle handle)
    {
        byte[] buffer = new byte[1024];
        if (FGetPath(handle, buffer) != 0)
            throw Error();
        int end = Array.IndexOf(buffer, (byte)0);
        if (end < 0)
            throw new IOException("The native path is too long.");
        return Encoding.UTF8.GetString(buffer, 0, end);
    }

    internal static void VerifyDirectory(SafeFileHandle handle, string path)
    {
        using SafeFileHandle current = OpenDirectory(path);
        if (Identity(handle) != Identity(current))
            throw new IOException("A folder changed during the operation.");
    }

    internal static void CreateDirectoryAt(SafeFileHandle parent, string name)
    {
        if (MkdirAt(parent, name, 0x1ed) != 0 && Marshal.GetLastPInvokeError() != 17)
            throw Error();
    }

    internal static void RenameExclusive(SafeFileHandle sourceParent, string sourceName, SafeFileHandle destinationParent, string destinationName)
    {
        // Single-component names anchored at verified descriptors; no overwrite, including APFS aliases.
        if (RenameAt(sourceParent, sourceName, destinationParent, destinationName, 4) != 0)
            throw Error();
    }

    internal static void FlushDirectory(SafeFileHandle handle)
    {
        if (FSync(handle) != 0)
            throw Error();
    }

    internal static bool SamePath(string first, string second)
    {
        string a = Path.GetFullPath(first), b = Path.GetFullPath(second);
        if (a == b)
            return true;
        if (Path.Exists(a) && Path.Exists(b))
        {
            using SafeFileHandle left = OpenAny(a);
            using SafeFileHandle right = OpenAny(b);
            return Identity(left) == Identity(right);
        }
        // Missing names have no identity. Compare only as a conservative journal/path guard;
        // renameatx_np, never this comparison, decides whether a destination is unoccupied.
        (string parentA, string tailA) = ExistingAncestor(a);
        (string parentB, string tailB) = ExistingAncestor(b);
        using SafeFileHandle dirA = OpenDirectory(parentA);
        using SafeFileHandle dirB = OpenDirectory(parentB);
        return Identity(dirA) == Identity(dirB) && string.Equals(tailA.Normalize(NormalizationForm.FormD),
            tailB.Normalize(NormalizationForm.FormD), MacFileHandle.IsCaseSensitive(dirA) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsWithin(string root, string path)
    {
        (string existing, _) = ExistingAncestor(Path.GetFullPath(root));
        if (!Directory.Exists(root))
            return false;
        using SafeFileHandle rootHandle = OpenDirectory(existing);
        FileIdentity rootIdentity = Identity(rootHandle);
        string? parent = Path.GetDirectoryName(Path.GetFullPath(path));
        while (parent is not null)
        {
            if (Directory.Exists(parent))
            {
                using SafeFileHandle handle = OpenDirectory(parent);
                if (Identity(handle) == rootIdentity)
                    return true;
            }
            parent = Path.GetDirectoryName(parent);
        }
        return false;
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
        return (current, Path.Combine(parts.ToArray()));
    }

    private static SafeFileHandle OpenAny(string path) => Wrap(Open(path, 0x20000000 | 0x01000000 | 4));
    private static Exception Error()
    {
        int error = Marshal.GetLastPInvokeError();
        return error is 1 or 13
            ? new UnauthorizedAccessException("macOS denied access to the file or folder.", new Win32Exception(error))
            : new IOException("macOS could not safely complete the file operation.", new Win32Exception(error));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AttributeList
    {
        public ushort Count, Reserved;
        public uint Common, Volume, Directory, File, Fork;
    }

    [DllImport(Library, EntryPoint = "open", SetLastError = true)]
    private static extern int Open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);
    [DllImport(Library, EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAt(SafeFileHandle parent, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int flags);
    [DllImport(Library, EntryPoint = "fgetattrlist", SetLastError = true)]
    private static extern int FGetAttrList(SafeFileHandle handle, ref AttributeList attributes, [Out] byte[] buffer, nuint length, uint options);
    [DllImport(Library, EntryPoint = "fstatfs", SetLastError = true)]
    private static extern int FStatFs(SafeFileHandle handle, [Out] byte[] buffer);
    [DllImport(Library, EntryPoint = "flistxattr", SetLastError = true)]
    private static extern long FListXAttr(SafeFileHandle handle, [Out] byte[]? buffer, nuint size, int options);
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [DllImport("dowlume_native", EntryPoint = "dowlume_get_path", SetLastError = true)]
    private static extern int FGetPath(SafeFileHandle handle, [Out] byte[] buffer);
    [DllImport(Library, EntryPoint = "mkdirat", SetLastError = true)]
    private static extern int MkdirAt(SafeFileHandle parent, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, uint mode);
    [DllImport(Library, EntryPoint = "renameatx_np", SetLastError = true)]
    private static extern int RenameAt(SafeFileHandle sourceParent, [MarshalAs(UnmanagedType.LPUTF8Str)] string source,
        SafeFileHandle destinationParent, [MarshalAs(UnmanagedType.LPUTF8Str)] string destination, uint flags);
    [DllImport(Library, EntryPoint = "fsync", SetLastError = true)]
    private static extern int FSync(SafeFileHandle handle);
}
