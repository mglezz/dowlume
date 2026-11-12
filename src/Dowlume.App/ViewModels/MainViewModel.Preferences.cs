using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.App.ViewModels;

public partial class MainViewModel
{
    public static IReadOnlyList<string> Themes { get; } = ["System", "Light", "Dark"];
    [ObservableProperty] private string _selectedTheme = "System";
    private string _savedTheme = "System";
    [ObservableProperty] private bool _resumeMonitoringOnStartup;
    [ObservableProperty] private bool _organizeFolders;
    [ObservableProperty] private int _folderIntervalMinutes = 20;
    [ObservableProperty] private string _collectedFoldersName = "Folders";
    private IAsyncDisposable? _folderMonitor;
    private bool _startupAttempted;
    public System.Collections.ObjectModel.ObservableCollection<string> FolderPreview { get; } = [];

    partial void OnSelectedThemeChanged(string value)
    {
        try
        {
            AppSettings saved = _store.LoadSettings() ?? new AppSettings { DownloadsPath = _suggestedDownloads };
            _store.SaveSettings(saved with
            {
                Theme = value
            });
            _savedTheme = value;
            ApplyTheme(value);
        }
        catch (Exception error)
        {
            // A failed read must not trigger another database access inside error handling.
            _selectedTheme = _savedTheme;
            OnPropertyChanged(nameof(SelectedTheme));
            ShowError(error);
        }
    }

    private static void ApplyTheme(string theme)
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = theme switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                _ => ThemeVariant.Default
            };
    }

    [RelayCommand]
    private async Task ToggleMonitoringAsync()
    {
        try
        {
            if (IsMonitoring)
                await PauseAsync();
            else
                await StartAsync();
        }
        finally { OnPropertyChanged(nameof(IsMonitoring)); }
    }

    // Called once after desktop initialization, including when no window is shown at login.
    public Task ResumeOnStartupAsync() => RunAsync(async () =>
    {
        if (_startupAttempted || IsMonitoring)
            return;
        _startupAttempted = true;
        AppSettings? settings = _store.LoadSettings();
        if (settings is not { ResumeMonitoringOnStartup: true, MonitoringConsentGranted: true })
            return;
        _organizer.RequireFileOperations();
        settings.Validate();
        PathSafety.ValidateRoot(settings.DownloadsPath);
        await BeginMonitoringAsync(settings);
    });

    private async Task BeginMonitoringAsync(AppSettings settings)
    {
        _shutdown.Token.ThrowIfCancellationRequested();
        _monitor = settings.UsePeriodicOrganization
            ? new PeriodicDownloadMonitor(settings,
                (path, token) => _organizer.OrganizeAsync(path, settings, token),
                token => _organizer.ReconcileAsync(settings, token), ReportBackgroundError)
            : new DownloadMonitor(settings,
                (path, token) => _organizer.OrganizeAsync(path, settings, token),
                token => _organizer.ReconcileAsync(settings, token), ReportBackgroundError);
        if (settings.OrganizeFolders)
        {
            var folders = new FolderOrganizer(_store);
            _folderMonitor = new FolderMonitor(settings, token => folders.ScanAsync(settings, token), ReportBackgroundError);
        }
        IsMonitoring = true;
        InvalidatePreview();
        if (settings.UsePeriodicOrganization)
            SetStatus("Periodic organization enabled. The first check starts in {0} minutes. Pause to stop future checks.", settings.OrganizationIntervalMinutes);
        else
            StatusMessage = "Monitoring completed downloads. You can pause at any time.";
        await RefreshAsync();
    }
}
