using System.Runtime.Versioning;

namespace Dowlume.Infrastructure;

internal static class SafeFolderMove
{
    internal static void Move(string source, string destination)
    {
        PathSafety.RejectLinks(source);
        PathSafety.RejectLinks(destination);
        if (OperatingSystem.IsWindows())
        {
            using var parents = DirectoryLease.Acquire(source, destination);
            using var folder = WindowsFileHandle.OpenDirectoryForMove(source);
            PathSafety.RejectLinks(source);
            if (!WindowsFileHandle.TryRename(folder, destination))
                throw new IOException("Folders can only be collected on the same volume.");
        }
        else if (OperatingSystem.IsLinux())
            MoveLinux(source, destination);
        else if (OperatingSystem.IsMacOS())
            MoveMac(source, destination);
        else
            throw new PlatformNotSupportedException(DownloadOrganizer.ReadOnlyMessage);
    }

    [SupportedOSPlatform("linux")]
    private static void MoveLinux(string source, string destination)
    {
        string parentPath = Path.GetDirectoryName(source)!;
        string collection = Path.GetDirectoryName(destination)!;
        using var parent = LinuxFileSystem.OpenDirectory(parentPath);
        using var folder = LinuxFileSystem.OpenDirectory(source);
        _ = LinuxFileSystem.Volume(parent);
        _ = LinuxFileSystem.Volume(folder);
        LinuxFileSystem.CreateDirectoryAt(parent, Path.GetFileName(collection));
        using var target = LinuxFileSystem.OpenDirectoryAt(parent, Path.GetFileName(collection));
        if (LinuxFileSystem.Volume(parent) != LinuxFileSystem.Volume(target) ||
            LinuxFileSystem.Inspect(parent).MountId != LinuxFileSystem.Inspect(target).MountId ||
            LinuxFileSystem.Inspect(parent).MountId != LinuxFileSystem.Inspect(folder).MountId)
            throw new IOException("Folders can only be collected on the same volume.");
        LinuxFileSystem.VerifyDirectory(parent, parentPath);
        LinuxFileSystem.VerifyDirectory(folder, source);
        LinuxFileSystem.VerifyDirectory(target, collection);
        LinuxFileSystem.RenameExclusive(parent, Path.GetFileName(source), target, Path.GetFileName(destination));
        LinuxFileSystem.FlushDirectory(target);
        LinuxFileSystem.FlushDirectory(parent);
        LinuxFileSystem.VerifyDirectory(folder, destination);
    }

    [SupportedOSPlatform("macos")]
    private static void MoveMac(string source, string destination)
    {
        string parentPath = Path.GetDirectoryName(source)!;
        string collection = Path.GetDirectoryName(destination)!;
        using var parent = MacFileSystem.OpenDirectory(parentPath);
        using var folder = MacFileSystem.OpenDirectory(source);
        MacFileSystem.RequireLocalApfs(parent);
        MacFileSystem.RequireLocalApfs(folder);
        MacFileSystem.CreateDirectoryAt(parent, Path.GetFileName(collection));
        using var target = MacFileSystem.OpenDirectoryAt(parent, Path.GetFileName(collection));
        MacFileSystem.RequireLocalApfs(target);
        if (MacFileSystem.Volume(parent) != MacFileSystem.Volume(target) || MacFileSystem.Volume(parent) != MacFileSystem.Volume(folder))
            throw new IOException("Folders can only be collected on the same volume.");
        MacFileSystem.VerifyDirectory(parent, parentPath);
        MacFileSystem.VerifyDirectory(folder, source);
        MacFileSystem.VerifyDirectory(target, collection);
        MacFileSystem.RenameExclusive(parent, Path.GetFileName(source), target, Path.GetFileName(destination));
        MacFileSystem.FlushDirectory(target);
        MacFileSystem.FlushDirectory(parent);
        MacFileSystem.VerifyDirectory(folder, destination);
    }
}
