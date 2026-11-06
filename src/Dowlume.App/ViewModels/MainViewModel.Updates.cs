using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dowlume.App.Services;

namespace Dowlume.App.ViewModels;

public partial class MainViewModel
{
    private readonly IAppUpdates _updates;
    private AvailableUpdate? _availableUpdate;
    private string _updateMessage = "Updates are checked only when you ask. GitHub receives a network request, but no file names or paths.";
    private object[] _updateArguments = [];
    [ObservableProperty] private bool _hasUpdate;
    public event Action? UpdateInstallationReady;
    public static string CurrentVersion => typeof(MainViewModel).Assembly.GetName().Version!.ToString(3);
    public string UpdateMessage => Text.Format(_updateMessage, _updateArguments);
    public string UpdateActionLabel => Text.Translate(_availableUpdate?.InstallerUrl is not null && _updates.CanInstall
        ? "Download and install update…" : "Open release and instructions");

    private void SetUpdateMessage(string message, params object[] arguments)
    {
        _updateMessage = message;
        _updateArguments = arguments;
        OnPropertyChanged(nameof(UpdateMessage));
    }

    [RelayCommand]
    private Task CheckForUpdatesAsync() => RunAsync(async () =>
    {
        HasUpdate = false;
        _availableUpdate = null;
        SetUpdateMessage("Checking for updates…");
        try
        {
            _availableUpdate = await _updates.CheckAsync(Version.Parse(CurrentVersion), _shutdown.Token);
            HasUpdate = _availableUpdate is not null;
            OnPropertyChanged(nameof(UpdateActionLabel));
            if (_availableUpdate is { } update)
                SetUpdateMessage("Dowlume {0} is available. Installed version: {1}.", update.Version, CurrentVersion);
            else
                SetUpdateMessage("No newer stable release is available. Current version: {0}.", CurrentVersion);
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { }
        catch (Exception error)
        {
            _log.Write("update_check_failed", error);
            SetUpdateMessage("Could not check for updates. Check your connection or try again later.");
        }
    });

    [RelayCommand]
    private Task ApplyUpdateAsync() => RunAsync(async () =>
    {
        if (_availableUpdate is not { } update)
            return;
        if (!_updates.CanInstall || update.InstallerUrl is null)
        {
            _updates.OpenRelease(update);
            return;
        }
        if (!await _dialogs.ConfirmAsync(Text.Translate("Update Dowlume?"),
            Text.Format("Download Dowlume {0} from GitHub and verify its checksum? Dowlume will pause organization and close before opening the installer. Install over the existing copy; your profile and organized files are retained. The installer is currently unsigned and Windows may ask for confirmation.", update.Version),
            Text.Translate("Download and install update…")))
            return;
        try
        {
            SetUpdateMessage("Downloading and verifying the update…");
            DownloadedUpdate installer = await _updates.DownloadAsync(update, _shutdown.Token);
            _shutdown.Token.ThrowIfCancellationRequested();
            await StopMonitorAsync();
            _shutdown.Token.ThrowIfCancellationRequested();
            _updates.InstallAfterExit(installer);
            SetUpdateMessage("Update verified. Closing Dowlume to open the installer…");
            // Post the close request so shutdown can await this command without a cycle.
            Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateInstallationReady?.Invoke());
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { }
        catch (Exception error)
        {
            _log.Write("update_download_failed", error);
            SetUpdateMessage("The update could not be prepared or verified. Nothing was installed. Try again later.");
        }
    });
}
