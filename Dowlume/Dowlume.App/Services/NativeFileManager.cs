using System.Diagnostics;

namespace Dowlume.App.Services;

internal sealed class NativeFileManager(DesktopSystem system) : IFileManager
{
    public void Reveal(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
        {
            throw new FileNotFoundException("The selected file is no longer available.");
        }
        using Process? process = Process.Start(CreateStartInfo(system, fullPath));
    }

    internal static ProcessStartInfo CreateStartInfo(DesktopSystem system, string fullPath)
    {
        if (!Path.IsPathFullyQualified(fullPath))
            throw new ArgumentException("An absolute file path is required.");
        string executable = system switch
        {
            DesktopSystem.Windows => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
            DesktopSystem.MacOS => "/usr/bin/open",
            DesktopSystem.Linux => "/usr/bin/xdg-open",
            _ => throw new PlatformNotSupportedException("File manager integration is unavailable on this platform.")
        };
        var start = new ProcessStartInfo(executable) { UseShellExecute = false };
        if (system == DesktopSystem.Windows)
            start.ArgumentList.Add("/select,");
        if (system == DesktopSystem.MacOS)
            start.ArgumentList.Add("-R");
        // xdg-open opens the containing folder; file selection is not portable across Linux file managers.
        start.ArgumentList.Add(system == DesktopSystem.Linux ? Path.GetDirectoryName(fullPath)! : fullPath);
        return start;
    }
}
