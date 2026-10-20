using System.Runtime.InteropServices;

namespace Dowlume.Infrastructure;

public static class DownloadsFolder
{
    public static string Suggest()
    {
        if (OperatingSystem.IsWindows())
        {
            Guid downloadsId = new("374DE290-123F-4565-9164-39C4925E467B");
            int result = SHGetKnownFolderPath(ref downloadsId, 0, IntPtr.Zero, out IntPtr path);
            try
            {
                if (result == 0)
                {
                    return Marshal.PtrToStringUni(path)!;
                }
            }
            finally { Marshal.FreeCoTaskMem(path); }
        }
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (OperatingSystem.IsLinux())
        {
            string? configured = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            string configHome = configured is not null && Path.IsPathFullyQualified(configured)
                ? configured : Path.Combine(home, ".config");
            string file = Path.Combine(configHome, "user-dirs.dirs");
            try
            {
                if (File.Exists(file))
                {
                    using var reader = File.OpenText(file);
                    char[] buffer = new char[65537];
                    int length = reader.ReadBlock(buffer, 0, buffer.Length);
                    string? downloads = XdgUserDirectories.ParseDownloads(new string(buffer, 0, length), home);
                    if (downloads is not null)
                        return downloads;
                }
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
        return Path.Combine(home, "Downloads");
    }

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath(ref Guid folderId, uint flags, IntPtr token, out IntPtr path);
}
