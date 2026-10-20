using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
                Directory.CreateDirectory(dataDirectory);
                _instanceLock = new FileStream(Path.Combine(dataDirectory, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                var store = new SqliteStateStore(Path.Combine(dataDirectory, "dowlume.db"));
                var organizer = new DownloadOrganizer(store, new SafeFileMover(), new FileReadiness());
                var window = new MainWindow();
                text.SetLanguage(store.LoadSettings()?.Language);
                var platform = DesktopPlatform.Current;
                var model = new MainViewModel(store, organizer, new DesktopDialogs(window, text, platform.FileManager), log,
                    platform.Startup, text, platform.SuggestedDownloads);
                window.DataContext = model;
                desktop.MainWindow = window;
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
                smokeWindow.Opened += (_, _) => DispatcherTimer.RunOnce(smokeWindow.Close, TimeSpan.FromSeconds(1));
            }
            desktop.Exit += (_, _) => { _tray?.Dispose(); _instanceLock?.Dispose(); };
        }

        base.OnFrameworkInitializationCompleted();
    }

}
