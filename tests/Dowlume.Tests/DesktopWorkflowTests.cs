using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using Dowlume.App.Services;
using Dowlume.App.ViewModels;
using Dowlume.App.Views;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

[Collection("Desktop")]
public sealed class DesktopWorkflowTests
{
    [Fact]
    public async Task UpdateCheckIsManualAndInstallationRequiresConsentAndPausesOrganization()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads, UsePeriodicOrganization = true });
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness());
            var updates = new TestUpdates();
            var dialogs = new TestDialogs();
            var model = new MainViewModel(store, organizer, dialogs, new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")), updates: updates);
            try
            {
                Assert.Equal(0, updates.Checks);
                Assert.False(model.HasUpdate);
                await model.CheckForUpdatesCommand.ExecuteAsync(null);
                Assert.Equal(1, updates.Checks);
                Assert.True(model.HasUpdate);
                Assert.Contains("0.6.0", model.UpdateMessage);
                updates.CanInstall = false;
                await model.ApplyUpdateCommand.ExecuteAsync(null);
                Assert.Equal(1, updates.OpenedReleases);
                Assert.Equal(0, updates.Downloads);
                updates.CanInstall = true;
                await model.ApplyUpdateCommand.ExecuteAsync(null);
                Assert.Equal(0, updates.Downloads);
                Assert.False(updates.Prepared);
                dialogs.Confirm = true;
                await model.PreviewCommand.ExecuteAsync(null);
                await model.StartCommand.ExecuteAsync(null);
                Assert.True(model.IsMonitoring);
                updates.FailDownload = true;
                await model.ApplyUpdateCommand.ExecuteAsync(null);
                Assert.False(updates.Prepared);
                Assert.True(model.IsMonitoring);
                Assert.Contains("Nothing was installed", model.UpdateMessage);
                updates.FailDownload = false;
                await model.ApplyUpdateCommand.ExecuteAsync(null);
                Assert.True(updates.Prepared);
                Assert.False(model.IsMonitoring);
                Assert.Empty(store.GetOperations());
                updates.FailCheck = true;
                await model.CheckForUpdatesCommand.ExecuteAsync(null);
                Assert.False(model.HasUpdate);
                Assert.Contains("Could not check", model.UpdateMessage);
            }
            finally { await model.ShutdownAsync(); }
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task PeriodicOrganizationRequiresConsentWaitsAndRestartsPaused()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            string source = workspace.CreateFile("scheduled.pdf");
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads });
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
            var dialogs = new TestDialogs();
            var log = new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl"));
            var model = new MainViewModel(store, organizer, dialogs, log);
            try
            {
                await model.PreviewCommand.ExecuteAsync(null);
                model.UsePeriodicOrganization = true;
                await model.StartCommand.ExecuteAsync(null);
                Assert.Equal(0, dialogs.ConfirmationCount);
                await model.PreviewCommand.ExecuteAsync(null);
                await model.StartCommand.ExecuteAsync(null);
                Assert.False(model.IsMonitoring);
                Assert.Equal(1, dialogs.ConfirmationCount);
                dialogs.Confirm = true;
                await model.StartCommand.ExecuteAsync(null);
                Assert.True(model.IsMonitoring);
                Assert.False(model.CanConfigure);
                Assert.Contains("every 10 minutes", model.MonitoringLabel);
                Assert.True(File.Exists(source));
                Assert.Empty(store.GetOperations());
                await model.PauseCommand.ExecuteAsync(null);
                Assert.False(model.IsMonitoring);
                Assert.True(model.CanConfigure);
                await model.StartCommand.ExecuteAsync(null);
                Assert.False(model.IsMonitoring);
            }
            finally { await model.ShutdownAsync(); }
            var restarted = new MainViewModel(store, organizer, dialogs, log);
            Assert.True(restarted.UsePeriodicOrganization);
            Assert.False(restarted.IsMonitoring);
            Assert.False(restarted.HasPreview);
            await restarted.ShutdownAsync();
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task ReviewSearchPreservesTotalsAndClearsHiddenSelectionsWithoutChangingFiles()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads });
            string source = workspace.CreateFile("Report [100%].pdf");
            var report = new TrackedFile(Guid.NewGuid(), source, source, FileCategory.Documents,
                DateTimeOffset.UtcNow, LifecycleStatus.InReview, "fixture");
            var photo = report with
            {
                Id = Guid.NewGuid(),
                CurrentPath = workspace.CreateFile("Photo.jpg"),
                Category = FileCategory.Images
            };
            store.SaveFile(report);
            store.SaveFile(photo);
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness());
            var model = new MainViewModel(store, organizer, new TestDialogs(), new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
            var window = new MainWindow { DataContext = model };
            window.Show();
            try
            {
                await model.RefreshAsync();
                model.SelectedReview = report;
                model.ReviewSearch = "  REPORT [100%]  ";
                Assert.Equal(report, Assert.Single(model.ReviewFiles));
                Assert.Equal(report, model.SelectedReview);
                Assert.Equal(2, model.ReviewCount);
                Assert.Equal("Showing 1 of 2 files awaiting review", model.ReviewResultsLabel);
                model.SelectedLanguage = MainViewModel.Languages.Single(language => language.Code == "es");
                Assert.Contains("1 de 2", model.ReviewResultsLabel);
                Assert.Equal(report, Assert.Single(model.ReviewFiles));
                model.ReviewSearch = "photo";
                Assert.Null(model.SelectedReview);
                Assert.False(model.CanReviewSelectedFile);
                await model.KeepCommand.ExecuteAsync(null);
                Assert.Empty(store.GetOperations());
                Assert.Equal(LifecycleStatus.InReview, store.GetFiles().Single(file => file.Id == report.Id).Status);
                model.SelectedReview = photo;
                store.SaveFile(photo with
                {
                    Status = LifecycleStatus.Kept
                });
                await model.RefreshAsync();
                Assert.True(model.IsReviewEmpty);
                Assert.Null(model.SelectedReview);
                Assert.Equal(1, model.ReviewCount);
                model.ClearReviewSearchCommand.Execute(null);
                Assert.Equal(report, Assert.Single(model.ReviewFiles));
                model.ReviewSearch = workspace.Downloads;
                Assert.Single(model.ReviewFiles);
                Assert.True(File.Exists(source));
                Assert.True(File.Exists(photo.CurrentPath));

                window.GetVisualDescendants().OfType<TabControl>().Single().SelectedIndex = 2;
                window.Width = window.MinWidth;
                window.Height = window.MinHeight;
                window.UpdateLayout();
                TextBox search = window.GetVisualDescendants().OfType<TextBox>().Single(box => box.IsEffectivelyVisible);
                search.Text = "no matching file";
                Assert.True(model.IsReviewEmpty);
                Assert.Contains("coincidencias", model.ReviewEmptyMessage);
                store.SaveFile(report with
                {
                    Status = LifecycleStatus.Kept
                });
                await model.RefreshAsync();
                Assert.Equal(0, model.ReviewCount);
                Assert.True(model.IsReviewEmpty);
                Assert.DoesNotContain("coincidencias", model.ReviewEmptyMessage);
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
            }
            finally { await DesktopWindowTest.CloseAsync(window); }
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task ClosingDuringConfirmationCancelsWithoutOrganizingFiles()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            string source = workspace.CreateFile("consent.pdf");
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads });
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
            var window = new MainWindow();
            var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            window.Closed += (_, _) => closed.TrySetResult();
            var model = new MainViewModel(store, organizer, new DesktopDialogs(window),
                new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
            window.DataContext = model;
            window.Show();
            await model.PreviewCommand.ExecuteAsync(null);
            Task confirmation = model.StartCommand.ExecuteAsync(null);
            Assert.Single(window.OwnedWindows);
            window.Close();
            await confirmation.WaitAsync(TimeSpan.FromSeconds(3));
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.True(File.Exists(source));
            Assert.Empty(store.GetOperations());
            Assert.False(model.IsMonitoring);
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task TrayRemainsAttachedWhileHiddenAndBothOpenActionsRestoreTheWindow()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(() =>
        {
            var window = new MainWindow();
            window.Show();
            using (var tray = new DesktopTray(Application.Current!, window))
            {
                TrayIcon icon = Assert.Single(TrayIcon.GetIcons(Application.Current!)!);
                Assert.True(icon.IsVisible);
                window.Hide();
                Assert.False(window.IsVisible);
                icon.Command!.Execute(null);
                Assert.True(window.IsVisible);
                window.WindowState = WindowState.Minimized;
                window.Hide();
                var open = Assert.IsType<NativeMenuItem>(icon.Menu!.Items[0]);
                open.Command!.Execute(null);
                Assert.True(window.IsVisible);
                Assert.Equal(WindowState.Normal, window.WindowState);
                var exit = Assert.IsType<NativeMenuItem>(icon.Menu.Items[2]);
                exit.Command!.Execute(null);
                Assert.False(window.IsVisible);
            }
            Assert.Null(TrayIcon.GetIcons(Application.Current!));
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task CustomRulesCanBeAddedEditedAndRemovedWithFreshPreviewAndLocalizedConflicts()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            string source = workspace.CreateFile("contrato.pdf");
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads });
            var model = new MainViewModel(store,
                new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero)),
                new TestDialogs(), new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
            model.AddRuleCommand.Execute(null);
            CategoryRuleViewModel custom = model.Rules.Last();
            custom.FolderName = "Contratos";
            custom.MatchKind = RuleMatchKind.NameContains;
            custom.NamePattern = "contrato";
            await model.SaveSettingsCommand.ExecuteAsync(null);
            Assert.Empty(model.ErrorMessage);
            Assert.Equal(custom.Id, store.LoadSettings()!.Categories.Last().Id);
            await model.PreviewCommand.ExecuteAsync(null);
            Assert.Equal("Contratos", Assert.Single(model.PreviewItems).CustomFolder);
            custom.NamePattern = "ABC";
            await model.StartCommand.ExecuteAsync(null);
            Assert.Contains("Preview the current settings", model.ErrorMessage);
            Assert.False(model.IsMonitoring);
            custom.NamePattern = "contrato";
            model.SelectedLanguage = MainViewModel.Languages.Single(language => language.Code == "es");
            model.AddRuleCommand.Execute(null);
            CategoryRuleViewModel duplicate = model.Rules.Last();
            duplicate.FolderName = "Duplicados";
            duplicate.MatchKind = RuleMatchKind.NameStartsWith;
            duplicate.NamePattern = "CONTRATO";
            await model.SaveSettingsCommand.ExecuteAsync(null);
            Assert.Contains("está asignado", model.ErrorMessage);
            Assert.Contains("Contratos", model.ErrorMessage);
            Assert.Equal(7, store.LoadSettings()!.Categories.Count);
            model.RemoveRuleCommand.Execute(duplicate);
            model.RemoveRuleCommand.Execute(custom);
            model.RemoveRuleCommand.Execute(model.Rules[0]);
            Assert.Equal(6, model.Rules.Count);
            await model.SaveSettingsCommand.ExecuteAsync(null);
            Assert.Empty(model.ErrorMessage);
            Assert.Equal(6, store.LoadSettings()!.Categories.Count);
            Assert.True(File.Exists(source));
            Assert.Empty(store.GetOperations());
            await model.ShutdownAsync();
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task StartupToggleIsOptInAndReportsFailuresWithoutClaimingSuccess()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness());
            var startup = new TestStartup();
            var model = new MainViewModel(store, organizer, new TestDialogs(),
                new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")), startup);
            Assert.False(model.StartAtLogin);
            Assert.Equal(0, startup.Changes);
            await model.ToggleLoginStartupCommand.ExecuteAsync(null);
            Assert.True(model.StartAtLogin);
            Assert.False(model.IsMonitoring);
            startup.Fail = true;
            await model.ToggleLoginStartupCommand.ExecuteAsync(null);
            Assert.True(model.StartAtLogin);
            Assert.Contains("Access denied", model.ErrorMessage);
            startup.Fail = false;
            await model.ToggleLoginStartupCommand.ExecuteAsync(null);
            Assert.False(model.StartAtLogin);
            await model.ShutdownAsync();
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task WindowsLoginPreferencePersistsWithoutSavingUnrelatedEditsAndHiddenWindowCanExit()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            var saved = new AppSettings { DownloadsPath = workspace.Downloads };
            store.SaveSettings(saved);
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness());
            var model = new MainViewModel(store, organizer, new TestDialogs(),
                new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")), new TestStartup());
            Assert.False(model.ShowWindowAtWindowsLogin);
            model.DownloadsPath = "unsaved folder edit";
            await model.ToggleWindowsLoginWindowCommand.ExecuteAsync(null);
            Assert.True(store.LoadSettings()!.ShowWindowAtWindowsLogin);
            Assert.Equal(saved.DownloadsPath, store.LoadSettings()!.DownloadsPath);
            Assert.False(model.StartAtLogin);
            Assert.False(model.IsMonitoring);
            var reopened = new MainViewModel(store, organizer, new TestDialogs(),
                new DiagnosticLog(Path.Combine(workspace.Root, "reopened.jsonl")), new TestStartup());
            Assert.True(reopened.ShowWindowAtWindowsLogin);
            await reopened.SaveSettingsCommand.ExecuteAsync(null);
            Assert.True(store.LoadSettings()!.ShowWindowAtWindowsLogin);
            await reopened.ToggleWindowsLoginWindowCommand.ExecuteAsync(null);
            Assert.False(store.LoadSettings()!.ShowWindowAtWindowsLogin);
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={workspace.Database};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "CREATE TRIGGER refuse_setting_update BEFORE UPDATE ON settings BEGIN SELECT RAISE(ABORT, 'Fixture persistence failure'); END;";
                command.ExecuteNonQuery();
                bool restored = false;
                reopened.PropertyChanged += (_, args) => restored |= args.PropertyName == nameof(MainViewModel.ShowWindowAtWindowsLogin);
                await reopened.ToggleWindowsLoginWindowCommand.ExecuteAsync(null);
                Assert.False(reopened.ShowWindowAtWindowsLogin);
                Assert.False(store.LoadSettings()!.ShowWindowAtWindowsLogin);
                Assert.NotEmpty(reopened.ErrorMessage);
                Assert.True(restored);
            }
            var window = new MainWindow { DataContext = reopened };
            Assert.False(window.IsVisible);
            await DesktopWindowTest.CloseAsync(window);
            Assert.True(window.IsShutdownComplete);
            await model.ShutdownAsync();
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task ActivitySearchPagingAndUndoWorkWithOlderHistory()
    {
        using var workspace = new TemporaryWorkspace();
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        var store = new SqliteStateStore(workspace.Database);
        store.SaveSettings(settings);
        var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
        string source = workspace.CreateFile("Older report.pdf");
        await organizer.OrganizeAsync(source, settings);
        OperationRecord original = Assert.Single(store.GetOperations());
        // Durable fixture writes can be slow on hosted disks; the UI deadline starts after setup.
        for (int index = 0; index < 205; index++)
        {
            store.SaveOperation(original with
            {
                Id = Guid.NewGuid(),
                SourcePath = Path.Combine(workspace.Downloads, $"other-{index}.pdf"),
                Status = OperationStatus.Failed,
                Error = "Disposable history fixture."
            });
        }
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            var dialogs = new TestDialogs { Confirm = true };
            var model = new MainViewModel(store, organizer, dialogs, new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
            var window = new MainWindow { DataContext = model };
            window.Show();
            try
            {
                await model.RefreshAsync();
                Assert.Equal(50, model.Activity.Count);
                Assert.True(model.HasNextActivityPage);
                Guid firstPageId = model.Activity[0].Id;
                await model.NextActivityPageCommand.ExecuteAsync(null);
                Assert.True(model.HasPreviousActivityPage);
                Assert.DoesNotContain(model.Activity, operation => operation.Id == firstPageId);
                model.SelectedOperation = model.Activity[0];
                Guid olderPageId = model.SelectedOperation.Id;
                store.SaveOperation(original with
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.Failed
                });
                await model.RefreshAsync();
                Assert.Equal(olderPageId, model.SelectedOperation?.Id);
                Assert.Equal(olderPageId, model.Activity[0].Id);

                model.ActivitySearch = "older REPORT";
                model.SelectedActivityKind = model.ActivityKinds.Single(filter => filter.Value == OperationKind.Organize);
                model.SelectedActivityStatus = model.ActivityStatuses.Single(filter => filter.Value == OperationStatus.Completed);
                await model.SearchActivityCommand.ExecuteAsync(null);
                Assert.Equal(original.Id, Assert.Single(model.Activity).Id);
                Assert.False(model.HasPreviousActivityPage);
                Assert.False(model.HasNextActivityPage);
                Assert.Null(model.SelectedOperation);
                model.SelectedOperation = model.Activity[0];
                await model.UndoCommand.ExecuteAsync(null);
                Assert.True(File.Exists(source), model.ErrorMessage);
                Assert.Equal(LifecycleStatus.Kept, Assert.Single(store.GetFiles()).Status);

                model.ActivitySearch = "no such file";
                await model.SearchActivityCommand.ExecuteAsync(null);
                Assert.True(model.IsActivityEmpty);
                Assert.Null(model.SelectedOperation);
                await model.ClearActivityFiltersCommand.ExecuteAsync(null);
                Assert.Equal(50, model.Activity.Count);
                Assert.False(model.IsActivityEmpty);
                Assert.False(model.HasPreviousActivityPage);
                Assert.Equal(string.Empty, model.ErrorMessage);

                TabControl tabs = window.GetVisualDescendants().OfType<TabControl>().Single();
                tabs.SelectedIndex = 3;
                window.Width = window.MinWidth;
                window.Height = window.MinHeight;
                window.UpdateLayout();
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                Assert.Equal(2, window.GetVisualDescendants().OfType<ComboBox>().Count(combo => combo.IsEffectivelyVisible));
            }
            finally { await DesktopWindowTest.CloseAsync(window); }
            return true;
        }, timeout.Token);
    }

    [Fact]
    public async Task PreviewConsentMonitoringAndReviewWorkThroughTheDesktop()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var settings = new AppSettings { DownloadsPath = workspace.Downloads };
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(settings);
            var clock = new TestClock();
            var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.FromMilliseconds(5), TimeSpan.Zero), clock);
            var dialogs = new TestDialogs();
            var model = new MainViewModel(store, organizer, dialogs, new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
            var window = new MainWindow { DataContext = model };
            window.Show();
            string source = workspace.CreateFile("A sample report.pdf");

            await model.PreviewCommand.ExecuteAsync(null);
            Assert.Single(model.PreviewItems);
            Assert.True(File.Exists(source));
            Assert.False(model.IsMonitoring);

            int previewedReviewDays = model.Rules[0].ReviewAfterDays;
            model.Rules[0].ReviewAfterDays++;
            await model.StartCommand.ExecuteAsync(null);
            Assert.Contains("Preview the current settings", model.ErrorMessage);
            Assert.Equal(0, dialogs.ConfirmationCount);
            Assert.True(File.Exists(source));
            Assert.Empty(store.GetOperations());
            model.Rules[0].ReviewAfterDays = previewedReviewDays;

            await model.StartCommand.ExecuteAsync(null);
            Assert.False(model.IsMonitoring);
            Assert.True(File.Exists(source));

            dialogs.Confirm = true;
            await model.StartCommand.ExecuteAsync(null);
            Assert.True(model.IsMonitoring, model.ErrorMessage);
            await model.PauseCommand.ExecuteAsync(null);
            Assert.False(model.IsMonitoring);
            clock.Now = clock.Now.AddDays(31);
            await organizer.ReconcileAsync(settings);
            await model.RefreshAsync();
            model.SelectedReview = Assert.Single(model.ReviewFiles);
            TrackedFile reviewed = model.SelectedReview;
            OperationRecord[] beforeRecycle = store.GetOperations().ToArray();
            TabControl reviewTabs = window.GetVisualDescendants().OfType<TabControl>().Single();
            reviewTabs.SelectedIndex = 2;
            window.UpdateLayout();
            Button recycle = window.GetVisualDescendants().OfType<Button>().Single(button => button.Command == model.RecycleCommand);
            Assert.True(recycle.IsEffectivelyVisible);
            Assert.False(recycle.IsEffectivelyEnabled);
            Assert.False(model.RecycleCommand.CanExecute(null));
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => text.IsEffectivelyVisible && text.Text == model.RecyclingNotice);
            int confirmations = dialogs.ConfirmationCount;
            // Direct command execution must also refuse, even if a caller ignores CanExecute.
            await model.RecycleCommand.ExecuteAsync(null);
            Assert.Equal(model.RecyclingNotice, model.ErrorMessage);
            Assert.Equal(confirmations, dialogs.ConfirmationCount);
            Assert.Equal(beforeRecycle, store.GetOperations());
            Assert.Equal(reviewed.ContentHash, FixtureFileMover.Create(workspace).Hash(reviewed.CurrentPath));
            Assert.Equal(reviewed, Assert.Single(model.ReviewFiles));
            await model.KeepCommand.ExecuteAsync(null);
            Assert.Empty(model.ReviewFiles);
            Assert.Equal(LifecycleStatus.Kept, Assert.Single(model.ManagedFiles).Status);

            TabControl tabs = window.GetVisualDescendants().OfType<TabControl>().Single();
            for (int index = 0; index < 5; index++)
            {
                tabs.SelectedIndex = index;
                window.UpdateLayout();
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                string? captureDirectory = Environment.GetEnvironmentVariable("DOWLUME_TEST_SCREENSHOTS");
                if (captureDirectory is not null)
                {
                    Directory.CreateDirectory(captureDirectory);
                    frame.Save(Path.Combine(captureDirectory, $"screen-{index}.png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                }
            }
            await DesktopWindowTest.CloseAsync(window);
            return true;
        }, timeout.Token);
    }

    public sealed class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
            .WithInterFont().UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }

    public sealed class TestApplication : Dowlume.App.App
    {
        public override void OnFrameworkInitializationCompleted()
        {
        }
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class TestDialogs : IDesktopDialogs
    {
        public bool Confirm
        {
            get; set;
        }
        public Task<string?> ChooseFolderAsync(string title) => Task.FromResult<string?>(null);
        public int ConfirmationCount
        {
            get; private set;
        }
        public Task<bool> ConfirmAsync(string title, string message, string action)
        {
            ConfirmationCount++;
            return Task.FromResult(Confirm);
        }
        public void RevealFile(string path)
        {
        }
    }


    private sealed class TestUpdates : IAppUpdates
    {
        public bool CanInstall { get; set; } = true;
        public int OpenedReleases
        {
            get; private set;
        }
        public int Checks
        {
            get; private set;
        }
        public int Downloads
        {
            get; private set;
        }
        public bool Prepared
        {
            get; private set;
        }
        public bool FailCheck
        {
            get; set;
        }
        public bool FailDownload
        {
            get; set;
        }
        public Task<AvailableUpdate?> CheckAsync(Version currentVersion, CancellationToken cancellationToken)
        {
            Checks++;
            if (FailCheck)
                throw new HttpRequestException();
            return Task.FromResult<AvailableUpdate?>(new AvailableUpdate(new Version(0, 6, 0), new Uri("https://github.com/mglezz/dowlume/releases/tag/v0.6.0"),
                new Uri("https://github.com/mglezz/dowlume/releases/download/v0.6.0/Dowlume-0.6.0-win-x64-setup.exe"), new string('a', 64), 10));
        }
        public Task<DownloadedUpdate> DownloadAsync(AvailableUpdate update, CancellationToken cancellationToken)
        {
            Downloads++;
            if (FailDownload)
                throw new IOException();
            return Task.FromResult(new DownloadedUpdate("inert fixture", new string('a', 64)));
        }
        public void InstallAfterExit(DownloadedUpdate update) => Prepared = true;
        public void OpenRelease(AvailableUpdate update) => OpenedReleases++;
    }

    private sealed class TestStartup : IStartupRegistration
    {
        public bool IsSupported => true;
        public bool IsEnabled
        {
            get; private set;
        }
        public bool Fail
        {
            get; set;
        }
        public int Changes
        {
            get; private set;
        }

        public void SetEnabled(bool enabled)
        {
            if (Fail)
            {
                throw new UnauthorizedAccessException();
            }
            Changes++;
            IsEnabled = enabled;
        }
    }
}
