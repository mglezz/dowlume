using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dowlume.App.Localization;
using Dowlume.App.Services;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IStateStore _store;
    private readonly DownloadOrganizer _organizer;
    private readonly IDesktopDialogs _dialogs;
    private readonly DiagnosticLog _log;
    private readonly IStartupRegistration? _startup;
    private readonly string _suggestedDownloads;
    private readonly CancellationTokenSource _shutdown = new();
    private IAsyncDisposable? _monitor;
    private AppSettings? _previewSettings;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private Task _activeWork = Task.CompletedTask;

    [ObservableProperty] private string _downloadsPath = string.Empty;
    [ObservableProperty] private string _excludedExtensions = string.Empty;
    [ObservableProperty] private bool _usePeriodicOrganization;
    [ObservableProperty] private int _organizationIntervalMinutes = 10;
    private string _statusMessage = "Choose a folder, preview the changes, then start organizing.";
    private object[] _statusArguments = [];
    public string StatusMessage
    {
        get => Text.Format(_statusMessage, _statusArguments);
        set => SetStatus(value);
    }

    private void SetStatus(string message, params object[] arguments)
    {
        _statusMessage = message;
        _statusArguments = arguments;
        OnPropertyChanged(nameof(StatusMessage));
    }
    private string _errorMessage = string.Empty;
    private object[] _errorArguments = [];
    public string ErrorMessage
    {
        get => _errorArguments.Length == 0 ? Text.Translate(_errorMessage) : Text.Format(_errorMessage, _errorArguments);
        private set
        {
            _errorMessage = value;
            _errorArguments = [];
            OnPropertyChanged();
        }
    }
    public Localizer Text
    {
        get;
    }
    public static IReadOnlyList<LanguageOption> Languages { get; } = [new("en", "English"), new("es", "Español")];
    [ObservableProperty] private LanguageOption _selectedLanguage = Languages[0];

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        try
        {
            // Persist only the language, retaining saved rules and any unsaved edits in the form.
            AppSettings saved = _store.LoadSettings() ?? new AppSettings { DownloadsPath = _suggestedDownloads };
            _store.SaveSettings(saved with
            {
                Language = value.Code
            });
            Text.SetLanguage(value.Code);
            if (_previewSettings is not null)
            {
                _previewSettings = _previewSettings with
                {
                    Language = value.Code
                };
            }
            RefreshLanguage();
        }
        catch (Exception error)
        {
            _selectedLanguage = Languages.Single(language => language.Code == Text.Language);
            OnPropertyChanged(nameof(SelectedLanguage));
            ShowError(error);
        }
    }

    private void RefreshLanguage()
    {
        foreach (SettingChoice option in ThemeOptions)
            option.Refresh();
        foreach (CategoryRuleViewModel rule in Rules)
            rule.RefreshLabels();
        OnPropertyChanged(nameof(UpdateMessage));
        OnPropertyChanged(nameof(UpdateActionLabel));
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(ErrorMessage));
        OnPropertyChanged(nameof(MonitoringLabel));
        OnPropertyChanged(nameof(RecyclingNotice));
        OnPropertyChanged(nameof(PlatformNotice));
        OnPropertyChanged(nameof(PrivacyNotice));
        OnPropertyChanged(nameof(ActivityPageLabel));
        RefreshReviewLabels();
        OperationKind? kind = SelectedActivityKind.Value;
        OperationStatus? status = SelectedActivityStatus.Value;
        OnPropertyChanged(nameof(ActivityKinds));
        OnPropertyChanged(nameof(ActivityStatuses));
        SelectedActivityKind = ActivityKinds.Single(filter => filter.Value == kind);
        SelectedActivityStatus = ActivityStatuses.Single(filter => filter.Value == status);
    }
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isMonitoring;
    [ObservableProperty] private bool _hasPreview;
    [ObservableProperty] private int _managedCount;
    [ObservableProperty] private int _reviewCount;
    [ObservableProperty] private int _attentionCount;
    [ObservableProperty] private bool _startAtLogin;
    [ObservableProperty] private bool _showWindowAtWindowsLogin;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanReviewSelectedFile))] private TrackedFile? _selectedReview;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanUndoSelectedOperation))] private OperationRecord? _selectedOperation;

    public bool IsIdle => !IsBusy;
    public bool SupportsLoginStartup => _startup?.IsSupported == true;
    public bool SupportsFileOperations => _organizer.CanMutateFiles;
    public bool IsPreviewPlatform => !SupportsFileOperations;
    public static bool IsMacOS => OperatingSystem.IsMacOS();
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsWindows => OperatingSystem.IsWindows();
    public string PlatformNotice => Text.Translate(DownloadOrganizer.ReadOnlyMessage);
    public string PrivacyNotice => Text[IsPreviewPlatform ? "PreviewPrivacyNotice" : IsMacOS ? "MacPrivacyNotice" : IsLinux ? "LinuxPrivacyNotice" : "LocalonlyNoaccountcloudsyncortelemetryClosing"];
    public bool CanReviewSelectedFile => SupportsFileOperations && IsIdle && SelectedReview is not null;
    public bool CanUndoSelectedOperation => SupportsFileOperations && IsIdle && SelectedOperation?.CanUndo == true;
    public bool CanConfigure => !IsBusy && !IsMonitoring;
    public string MonitoringLabel => IsMonitoring && UsePeriodicOrganization
        ? Text.Format("Periodic organization is on · every {0} minutes", OrganizationIntervalMinutes)
        : Text.Translate(IsMonitoring ? "Monitoring is on" : "Monitoring is paused");
    public ObservableCollection<CategoryRuleViewModel> Rules { get; } = [];
    public ObservableCollection<PreviewItem> PreviewItems { get; } = [];
    public ObservableCollection<TrackedFile> ReviewFiles { get; } = [];
    public ObservableCollection<TrackedFile> ManagedFiles { get; } = [];
    public ObservableCollection<OperationRecord> Activity { get; } = [];
    public ObservableCollection<CategorySummary> Categories { get; } = [];

    public MainViewModel(IStateStore store, DownloadOrganizer organizer, IDesktopDialogs dialogs, DiagnosticLog log,
        IStartupRegistration? startup = null, Localizer? localizer = null, string? suggestedDownloads = null, IAppUpdates? updates = null)
    {
        Text = localizer ?? new Localizer();
        ThemeOptions = Themes.Select(theme => new SettingChoice(theme, Text, theme)).ToArray();
        _updates = updates ?? new AppUpdates();
        _store = store;
        _organizer = organizer;
        if (IsPreviewPlatform)
            _statusMessage = "Choose a folder in Settings, then inspect its preview. File operations are unavailable on this platform.";
        _dialogs = dialogs;
        _log = log;
        _startup = startup;
        _suggestedDownloads = suggestedDownloads ?? DesktopPlatform.Current.SuggestedDownloads;
        try
        {
            StartAtLogin = startup?.IsEnabled == true;
        }
        catch (Exception error) { ShowError(error); }
        AppSettings settings = store.LoadSettings() ?? new AppSettings { DownloadsPath = _suggestedDownloads };
        ShowWindowAtWindowsLogin = settings.ShowWindowAtWindowsLogin;
        _selectedTheme = settings.Theme;
        _savedTheme = settings.Theme;
        ApplyTheme(settings.Theme);
        ResumeMonitoringOnStartup = settings.ResumeMonitoringOnStartup;
        OrganizeFolders = settings.OrganizeFolders;
        FolderIntervalMinutes = settings.FolderIntervalMinutes;
        CollectedFoldersName = settings.CollectedFoldersName;
        Text.SetLanguage(settings.Language);
        _selectedLanguage = Languages.Single(language => language.Code == Text.Language);
        RefreshLanguage();
        DownloadsPath = settings.DownloadsPath;
        UsePeriodicOrganization = settings.UsePeriodicOrganization;
        OrganizationIntervalMinutes = settings.OrganizationIntervalMinutes;
        ExcludedExtensions = string.Join(", ", settings.ExcludedExtensions);
        foreach (CategoryRule rule in settings.Categories)
        {
            Rules.Add(new CategoryRuleViewModel(rule, Text));
        }
    }

    [RelayCommand]
    private Task ToggleWindowsLoginWindowAsync() => RunAsync(() =>
    {
        try
        {
            AppSettings saved = _store.LoadSettings() ?? new AppSettings { DownloadsPath = _suggestedDownloads };
            _store.SaveSettings(saved with
            {
                ShowWindowAtWindowsLogin = !ShowWindowAtWindowsLogin
            });
            ShowWindowAtWindowsLogin = !ShowWindowAtWindowsLogin;
        }
        finally
        {
            // Restore the one-way checkbox if persistence fails.
            OnPropertyChanged(nameof(ShowWindowAtWindowsLogin));
        }
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task ToggleLoginStartupAsync() => RunAsync(() =>
    {
        if (_startup is null || !_startup.IsSupported)
        {
            throw new InvalidOperationException("Login startup is unavailable on this platform.");
        }
        try
        {
            _startup.SetEnabled(!StartAtLogin);
            StartAtLogin = _startup.IsEnabled;
            StatusMessage = StartAtLogin
                ? "Dowlume will start when you sign in. Monitoring follows your saved automatic-start preference."
                : "Dowlume will no longer start automatically from this location.";
        }
        finally
        {
            // Restore the checkbox if the platform refused the requested change.
            OnPropertyChanged(nameof(StartAtLogin));
        }
        return Task.CompletedTask;
    });

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsIdle));
        OnPropertyChanged(nameof(CanReviewSelectedFile));
        OnPropertyChanged(nameof(CanUndoSelectedOperation));
        OnPropertyChanged(nameof(CanConfigure));
    }

    partial void OnIsMonitoringChanged(bool value)
    {
        OnPropertyChanged(nameof(CanConfigure));
        OnPropertyChanged(nameof(MonitoringLabel));
    }

    [RelayCommand]
    private Task ChooseFolderAsync() => RunAsync(async () =>
    {
        string? folder = await _dialogs.ChooseFolderAsync(Text.Translate("Choose your Downloads folder"));
        if (folder is not null)
        {
            DownloadsPath = folder;
            InvalidatePreview();
        }
    });

    [RelayCommand]
    private Task SaveSettingsAsync() => RunAsync(() =>
    {
        // Changed organization settings revoke automatic-start consent until previewed again.
        AppSettings settings = ReadSettings();
        AppSettings? previous = _store.LoadSettings();
        _store.SaveSettings(settings with
        {
            MonitoringConsentGranted = previous?.MonitoringConsentGranted == true && settings.HasSameValuesAs(previous)
        });
        InvalidatePreview();
        StatusMessage = IsPreviewPlatform
            ? "Settings saved. You can inspect the folder in Preview. File operations remain unavailable."
            : "Settings saved. Preview the folder before starting monitoring.";
        return Task.CompletedTask;
    });

    [RelayCommand]
    private void AddRule()
    {
        if (!CanConfigure)
            return;
        Rules.Add(new CategoryRuleViewModel(new CategoryRule(FileCategory.Other, string.Empty, [], 30) { Id = Guid.NewGuid() }, Text) { IsExpanded = true });
        InvalidatePreview();
    }

    [RelayCommand]
    private void RemoveRule(CategoryRuleViewModel rule)
    {
        if (!CanConfigure || !rule.IsCustom)
            return;
        Rules.Remove(rule);
        InvalidatePreview();
    }

    [RelayCommand]
    private Task PreviewAsync() => RunAsync(async () =>
    {
        InvalidatePreview();
        AppSettings settings = ReadSettings();
        StatusMessage = "Checking completed downloads. Large files may take a moment.";
        IReadOnlyList<PreviewItem> items = await Task.Run(() => _organizer.PreviewAsync(settings, _shutdown.Token));
        IReadOnlyList<string> folders = await Task.Run(() => new FolderOrganizer(_store).Preview(settings));
        Replace(PreviewItems, items);
        Replace(FolderPreview, folders.Select(path => $"{Path.GetFileName(path)} → {settings.CollectedFoldersName}"));
        _previewSettings = settings;
        HasPreview = true;
        SetStatus("{0} completed files are ready. Busy, recent, excluded, and already tracked files are skipped. Nothing has moved.", items.Count);
    });

    [RelayCommand(CanExecute = nameof(SupportsFileOperations))]
    private Task StartAsync() => RunAsync(async () =>
    {
        _organizer.RequireFileOperations();
        AppSettings settings = ReadSettings();
        if (!settings.HasSameValuesAs(_previewSettings))
        {
            throw new InvalidOperationException("Preview the current settings before starting.");
        }
        string confirmation = settings.UsePeriodicOrganization
            ? Text.Format("Check this folder every {0} minutes?\n\n{1}\n\nThe first pass waits for the interval. Each pass organizes completed downloads, including files added after this preview, and moves aging files to Expiring Files. Busy and unfinished downloads are skipped. No files will be deleted automatically. You can turn monitoring off at any time. Automatic startup requires your saved opt-in preference.", settings.OrganizationIntervalMinutes, settings.DownloadsPath)
            : Text.Format("Organize {0} previewed files and monitor this folder for completed downloads?\n\n{1}\n\nFuture and currently unfinished downloads will also be organized. Aging files will move to Expiring Files for your review. No files will be deleted automatically.", PreviewItems.Count, settings.DownloadsPath);
        if (settings.OrganizeFolders)
        {
            confirmation += "\n\n" + Text.Format("Stable folders will be collected in {0}, with checks every {1} minutes. Classification folders, links and incomplete downloads are skipped. Folders are never merged or deleted; folder moves are recorded but have no automatic Undo.", settings.CollectedFoldersName, settings.FolderIntervalMinutes);
            confirmation += "\n\n" + Text["FolderMoveWarning"];
        }
        if (settings.ResumeMonitoringOnStartup)
            confirmation += "\n\n" + Text.Translate("Monitoring will resume automatically when Dowlume opens, including at login. Turning monitoring off also disables automatic startup.");
        if (!await _dialogs.ConfirmAsync(Text.Translate("Start organizing downloads?"), confirmation, Text.Translate("Organize and monitor")))
        {
            return;
        }
        _store.SaveSettings(settings with
        {
            MonitoringConsentGranted = false
        });
        if (!settings.UsePeriodicOrganization)
        {
            foreach (PreviewItem item in PreviewItems.ToArray())
            {
                await Task.Run(() => _organizer.OrganizeAsync(item.SourcePath, settings, _shutdown.Token, item.ContentHash));
            }
        }
        await BeginMonitoringAsync(settings);
        try
        {
            _store.SaveSettings(settings with
            {
                MonitoringConsentGranted = true
            });
        }
        catch { await StopMonitorAsync(); throw; }
    });

    [RelayCommand]
    private Task PauseAsync() => RunAsync(async () =>
    {
        await StopMonitorAsync();
        // Explicit Off also prevents a surprising restart after login.
        _store.SaveSettings(SavedSettings() with
        {
            ResumeMonitoringOnStartup = false
        });
        ResumeMonitoringOnStartup = false;
        StatusMessage = "Monitoring paused. Preview again when you are ready to resume.";
    });

    [RelayCommand]
    private Task KeepAsync() => ReviewActionAsync((file, settings) => _organizer.KeepAsync(file.Id, settings, _shutdown.Token));

    [RelayCommand]
    private Task SnoozeAsync() => ReviewActionAsync((file, settings) => _organizer.SnoozeAsync(file.Id, settings, _shutdown.Token));

    [RelayCommand]
    private Task MoveAsync() => RunAsync(async () =>
    {
        TrackedFile file = RequireReviewSelection();
        string? folder = await _dialogs.ChooseFolderAsync(Text.Translate("Choose where to move this file"));
        if (folder is null)
        {
            return;
        }
        if (!await _dialogs.ConfirmAsync(Text.Translate("Move file?"), Text.Format("Move {0} to:\n\n{1}\n\nDowlume will stop monitoring this file. Existing files will not be overwritten.", file.Name, folder), Text.Translate("Move file")))
        {
            return;
        }
        await Task.Run(() => _organizer.MoveAsync(file.Id, SavedSettings(), folder, _shutdown.Token));
        await RefreshAsync();
    });

    public string RecyclingNotice => Text.Translate(WindowsRecycleBin.DisabledMessage);

    private static bool CanRecycle() => false;

    [RelayCommand(CanExecute = nameof(CanRecycle))]
    private Task RecycleAsync() => RunAsync(() => Task.FromException(new NotSupportedException(WindowsRecycleBin.DisabledMessage)));

    [RelayCommand]
    private Task UndoAsync() => RunAsync(async () =>
    {
        OperationRecord operation = SelectedOperation ?? throw new InvalidOperationException("Select an action in Activity first.");
        if (!await _dialogs.ConfirmAsync(Text.Translate("Undo this action?"), Text.Format("Undo {0}?\n\nThe original location must be available and the file must be unchanged. Undoing initial organization keeps the file in place permanently. Undoing a review move snoozes the file for seven days.", Text.Describe(operation)), Text.Translate("Undo action")))
        {
            return;
        }
        await Task.Run(() => _organizer.UndoAsync(operation.Id, SavedSettings(), _shutdown.Token));
        await RefreshAsync();
    });

    [RelayCommand(CanExecute = nameof(SupportsFileOperations))]
    private Task RetryRecoveryAsync() => RunAsync(async () =>
    {
        await Task.Run(() => _organizer.RetryRecoveryAsync(_shutdown.Token));
        await RefreshAsync();
        StatusMessage = "Recovery checked. Inspect any NeedsAttention entries in Activity; both paths are shown there.";
    });

    [RelayCommand]
    private Task RevealReviewAsync() => RunAsync(() =>
    {
        _dialogs.RevealFile(RequireReviewSelection().CurrentPath);
        return Task.CompletedTask;
    });

    private Task ReviewActionAsync(Func<TrackedFile, AppSettings, Task> action) => RunAsync(async () =>
    {
        TrackedFile file = RequireReviewSelection();
        AppSettings settings = SavedSettings();
        await Task.Run(() => action(file, settings));
        await RefreshAsync();
    });

    public async Task RefreshAsync(bool skipIfRefreshing = false)
    {
        if (_shutdown.IsCancellationRequested)
        {
            return;
        }
        if (!await _refreshGate.WaitAsync(skipIfRefreshing ? 0 : Timeout.Infinite))
        {
            return;
        }
        try
        {
            if (_shutdown.IsCancellationRequested)
            {
                return;
            }
            OperationHistoryQuery query = _historyQuery;
            var snapshot = await Task.Run(() => (Settings: _store.LoadSettings(), Files: _store.GetFiles(), History: _store.GetOperationHistory(query),
                AttentionCount: _store.CountOperationsRequiringAttention()));
            string root = snapshot.Settings?.DownloadsPath ?? DownloadsPath;
            TrackedFile[] files = snapshot.Files.Where(file => PathSafety.IsWithin(root, file.CurrentPath)).ToArray();
            UpdateReviewQueue(files);
            Replace(ManagedFiles, files.Where(file => file.IsManaged || file.Status == LifecycleStatus.Missing));
            if (query == _historyQuery)
            {
                ShowActivityPage(snapshot.History);
            }
            var summaries = Enum.GetValues<FileCategory>().Select(category =>
                new CategorySummary(category, files.Count(file => file.IsManaged && file.CustomRuleId is null && file.Category == category))).ToList();
            AppSettings? saved = snapshot.Settings;
            var customIds = (saved?.Categories.Where(rule => rule.Id is not null).Select(rule => rule.Id!.Value) ?? [])
                .Concat(files.Where(file => file.IsManaged && file.CustomRuleId is not null).Select(file => file.CustomRuleId!.Value)).Distinct();
            foreach (Guid id in customIds)
            {
                string folder = saved?.Categories.SingleOrDefault(rule => rule.Id == id)?.FolderName ?? files.First(file => file.CustomRuleId == id).AssignedFolder!;
                summaries.Add(new CategorySummary(FileCategory.Other, files.Count(file => file.IsManaged && file.CustomRuleId == id), folder));
            }
            Replace(Categories, summaries);
            ManagedCount = files.Count(file => file.IsManaged);
            AttentionCount = files.Count(file => file.Status == LifecycleStatus.Missing) +
                snapshot.AttentionCount;
        }
        catch (Exception error) { ShowError(error); }
        finally { _refreshGate.Release(); }
    }

    private AppSettings ReadSettings()
    {
        if (IsMonitoring)
        {
            throw new InvalidOperationException("Pause monitoring before changing settings or previewing files.");
        }
        if (string.IsNullOrWhiteSpace(DownloadsPath) || !Path.IsPathFullyQualified(DownloadsPath.Trim()))
        {
            throw new ArgumentException("Choose an absolute Downloads folder path.");
        }
        var settings = new AppSettings
        {
            Language = Text.Language,
            Theme = SelectedTheme,
            ResumeMonitoringOnStartup = ResumeMonitoringOnStartup,
            OrganizeFolders = OrganizeFolders,
            FolderIntervalMinutes = FolderIntervalMinutes,
            CollectedFoldersName = CollectedFoldersName.Trim(),
            ShowWindowAtWindowsLogin = ShowWindowAtWindowsLogin,
            UsePeriodicOrganization = UsePeriodicOrganization,
            OrganizationIntervalMinutes = OrganizationIntervalMinutes,
            DownloadsPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(DownloadsPath.Trim())),
            Categories = Rules.Select(rule => rule.ToRule()).ToList(),
            ExcludedExtensions = CategoryRuleViewModel.ParseExtensions(ExcludedExtensions)
        };
        settings.Validate();
        PathSafety.ValidateRoot(settings.DownloadsPath);
        return settings;
    }

    private AppSettings SavedSettings() => _store.LoadSettings() ?? throw new InvalidOperationException("Save and preview a Downloads folder first.");
    private TrackedFile RequireReviewSelection() => SelectedReview ?? throw new InvalidOperationException("Select a file in the review list first.");

    private Task RunAsync(Func<Task> action)
    {
        if (IsBusy || _shutdown.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        _activeWork = RunGuardedAsync(action);
        return _activeWork;
    }

    private async Task RunGuardedAsync(Func<Task> action)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await action();
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { }
        catch (Exception error) { ShowError(error); }
        finally { IsBusy = false; }
    }

    private void ShowError(Exception error)
    {
        _log.Write("operation_failed", error);
        ErrorMessage = DownloadOrganizer.FriendlyError(error);
        if (error is RuleConflictException conflict)
        {
            _errorArguments = conflict.Details.Cast<object>().ToArray();
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    private void ReportBackgroundError(Exception error) => Dispatcher.UIThread.Post(() => ShowError(error));

    private void InvalidatePreview()
    {
        _previewSettings = null;
        HasPreview = false;
        PreviewItems.Clear();
        FolderPreview.Clear();
    }

    private async Task StopMonitorAsync()
    {
        if (_folderMonitor is not null)
        {
            await _folderMonitor.DisposeAsync();
            _folderMonitor = null;
        }
        if (_monitor is not null)
        {
            await _monitor.DisposeAsync();
            _monitor = null;
        }
        IsMonitoring = false;
    }

    public async Task ShutdownAsync()
    {
        await _shutdown.CancelAsync();
        await _activeWork;
        await StopMonitorAsync();
        await _refreshGate.WaitAsync();
        _refreshGate.Release();
        _shutdown.Dispose();
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        T[] snapshot = values.ToArray();
        if (target.SequenceEqual(snapshot))
        {
            return;
        }
        target.Clear();
        foreach (T item in snapshot)
        {
            target.Add(item);
        }
    }
}
