using System.Runtime.InteropServices;
using Dowlume.Infrastructure;

namespace Dowlume.App.Services;

public enum DesktopSystem
{
    Windows, MacOS, Linux, Unsupported
}

// Select native services once; unsupported capabilities remain unavailable.
public sealed class DesktopPlatform
{
    public DesktopSystem System
    {
        get;
    }
    public IStartupRegistration Startup
    {
        get;
    }
    public IFileManager FileManager
    {
        get;
    }
    public string SuggestedDownloads
    {
        get;
    }
    public string DataDirectory
    {
        get;
    }
    public bool HasAcceptedTray => System == DesktopSystem.Windows;

    private DesktopPlatform(DesktopSystem system)
    {
        System = system;
        Startup = system switch
        {
            DesktopSystem.Windows => new WindowsStartupRegistration(),
            DesktopSystem.MacOS => new MacStartupRegistration(),
            _ => new UnavailableStartupRegistration()
        };
        FileManager = new NativeFileManager(system);
        SuggestedDownloads = DownloadsFolder.Suggest();
        // macOS imports a closed preview profile on first launch; Linux remains isolated.
        string name = system == DesktopSystem.Linux ? "Dowlume.Preview" : "Dowlume";
        DataDirectory = system == DesktopSystem.MacOS
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", name)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name);
    }

    public static DesktopPlatform Current { get; } = new(Detect());

    private static DesktopSystem Detect()
    {
        if (OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            return DesktopSystem.Windows;
        if (OperatingSystem.IsMacOSVersionAtLeast(15) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            return DesktopSystem.MacOS;
        if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            return DesktopSystem.Linux;
        return DesktopSystem.Unsupported;
    }
}

internal sealed class UnavailableStartupRegistration : IStartupRegistration
{
    public bool IsSupported => false;
    public bool IsEnabled => false;
    public void SetEnabled(bool enabled) => throw new PlatformNotSupportedException("Login startup is unavailable on this platform.");
}
