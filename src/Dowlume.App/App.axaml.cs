using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dowlume.App.Localization;
using Dowlume.App.Services;
using Dowlume.App.ViewModels;
using Dowlume.App.Views;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.App;

public partial class App : Application
{
    private FileStream? _instanceLock;
    private DesktopTray? _tray;
    private MacDesktopIntegration? _macDesktop;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string dataDirectory = Program.DataDirectory;
            var log = new DiagnosticLog(Path.Combine(dataDirectory, "diagnostics.jsonl"));
            var text = new Localizer();
            try
            {
                if (OperatingSystem.IsMacOS() && !Program.IsCustomDataDirectory)
                    MacProfileMigration.ImportPreview(Path.Combine(Path.GetDirectoryName(dataDirectory)!, "Dowlume.Preview"), dataDirectory);
                Directory.CreateDirectory(dataDirectory);
                _instanceLock = new FileStream(Path.Combine(dataDirectory, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                var store = new SqliteStateStore(Path.Combine(dataDirectory, "dowlume.db"));
                var organizer = new DownloadOrganizer(store, new SafeFileMover(), OperatingSystem.IsMacOS() ? new MacFileReadiness() : OperatingSystem.IsLinux() ? new LinuxFileReadiness() : new FileReadiness());
                var window = new MainWindow();
                text.SetLanguage(store.LoadSettings()?.Language);
                var platform = DesktopPlatform.Current;
                var model = new MainViewModel(store, organizer, new DesktopDialogs(window, text, platform.FileManager), log,
                    Program.IsCustomDataDirectory && platform.System != DesktopSystem.Windows ? new UnavailableStartupRegistration() : platform.Startup, text, platform.SuggestedDownloads);
                window.DataContext = model;
                desktop.MainWindow = window;
                if (platform.System == DesktopSystem.MacOS)
                    _macDesktop = new MacDesktopIntegration(this, desktop, window, text);
                if (platform.HasAcceptedTray)
                {
                    try
                    {
                        _tray = new DesktopTray(this, window, text);
                        window.ConfigureTray(true);
                    }
                    catch (Exception error) { log.Write("tray_unavailable", error); }
                }
                log.Write("application_started");
            }
            catch (Exception error)
            {
                if (OperatingSystem.IsMacOS() && !Program.IsCustomDataDirectory && !File.Exists(Path.Combine(dataDirectory, "dowlume.db")))
                    new DiagnosticLog(Path.Combine(Path.GetDirectoryName(dataDirectory)!, "Dowlume.migration.jsonl")).Write("startup_failed", error);
                else
                    log.Write("startup_failed", error);
                if (Program.IsSmokeTest)
                    Environment.ExitCode = 1;
                desktop.MainWindow = new Window
                {
                    Title = text.Translate("Dowlume — unable to start"),
                    Width = 520,
                    Height = 220,
                    Content = new TextBlock
                    {
                        Margin = new Thickness(28),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Text = text.Translate("Dowlume could not open its local data. Another instance may already be running. Close it and try again. If the problem persists, back up your local database before repairing or reinstalling.")
                    }
                };
            }
            if (Program.IsSmokeTest)
            {
                Window smokeWindow = desktop.MainWindow!;
                smokeWindow.Opened += async (_, _) =>
                {
                    try
                    {
                        if (OperatingSystem.IsMacOS() && smokeWindow is MainWindow main)
                            await MacPackageAcceptance.RunAsync(main, dataDirectory);
                        else if (OperatingSystem.IsLinux() && smokeWindow is MainWindow linuxMain)
                            await LinuxPackageAcceptance.RunAsync(linuxMain, dataDirectory);
                        else
                            await Task.Delay(1000);
                    }
                    catch (Exception error)
                    {
                        log.Write("package_acceptance_failed", error);
                        Console.Error.WriteLine(error);
                        Environment.ExitCode = 1;
                    }
                    finally { smokeWindow.Close(); }
                };
            }
            desktop.Exit += (_, _) => { _macDesktop?.Dispose(); _tray?.Dispose(); _instanceLock?.Dispose(); };
        }

        base.OnFrameworkInitializationCompleted();
    }

}
