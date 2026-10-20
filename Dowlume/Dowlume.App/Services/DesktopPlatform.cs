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
        Startup = system == DesktopSystem.Windows ? new WindowsStartupRegistration() : new UnavailableStartupRegistration();
        FileManager = new NativeFileManager(system);
        SuggestedDownloads = DownloadsFolder.Suggest();
        // Retain existing profiles; ports use separate preview profiles until migration is accepted.
        string name = system == DesktopSystem.Windows ? "Dowlume" : "Dowlume.Preview";
        DataDirectory = system == DesktopSystem.MacOS
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", name)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name);
    }

    public static DesktopPlatform Current { get; } = new(Detect());

    private static DesktopSystem Detect()
    {
        if (OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            return DesktopSystem.Windows;
        if (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
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
