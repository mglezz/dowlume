using Avalonia;
using Dowlume.App.Services;
using Dowlume.Infrastructure;

namespace Dowlume.App;

internal static class Program
{
    internal static DownloadedUpdate? PendingInstaller
    {
        get; set;
    }
    internal static string DataDirectory { get; private set; } = DesktopPlatform.Current.DataDirectory;

    internal static bool IsCustomDataDirectory
    {
        get; private set;
    }

    internal static bool IsSmokeTest
    {
        get; private set;
    }

    [STAThread]
    public static void Main(string[] args)
    {
        if (DesktopPlatform.Current.System == DesktopSystem.Unsupported)
        {
            Console.Error.WriteLine("This build targets Windows x64, macOS Apple Silicon, or Linux x64.");
            Environment.ExitCode = 1;
            return;
        }
        if (args is ["--smoke-test", "--data-directory", var smokeDirectory])
        {
            IsCustomDataDirectory = true;
            DataDirectory = Path.GetFullPath(smokeDirectory);
            PathSafety.RejectLinks(DataDirectory);
            if (Path.Exists(DataDirectory))
                throw new IOException("Smoke testing requires a new disposable profile directory.");
            IsSmokeTest = true;
        }
        else if (args is ["--data-directory", var directory])
        {
            IsCustomDataDirectory = true;
            DataDirectory = Path.GetFullPath(directory);
        }
        else if (args.Length != 0)
        {
            Console.Error.WriteLine("Usage: Dowlume.App [--data-directory <profile>] or --smoke-test --data-directory <new-profile>");
            Environment.ExitCode = 1;
            return;
        }
        // Setup checks this marker before replacing or uninstalling running binaries.
        using (Mutex? installationGuard = OperatingSystem.IsWindows() ? new Mutex(false, @"Local\Dowlume.Desktop") : null)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        // Setup must start only after the window, profile lock, and installation marker are released.
        if (PendingInstaller is { } installer)
        {
            try
            {
                AppUpdates.LaunchInstaller(installer);
            }
            catch (Exception error)
            {
                new DiagnosticLog(Path.Combine(DataDirectory, "diagnostics.jsonl")).Write("update_launch_failed", error);
                Environment.ExitCode = 1;
            }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .WithInterFont()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .LogToTrace();
    }
}
