using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Avalonia.Controls;
using Dowlume.App.ViewModels;
using Dowlume.App.Views;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.App.Services;

// Runs only after --smoke-test has required a new, isolated profile. No personal folders or login jobs.
internal static class NativePackageAcceptance
{
    internal static async Task RunAsync(MainWindow window, string profile)
    {
        if (!Program.IsSmokeTest || !Program.IsCustomDataDirectory)
            throw new InvalidOperationException("Package acceptance requires an isolated smoke profile.");

        (IFileReadiness readiness, string reportName) = SelectNativePlatform();
        var elapsed = Stopwatch.StartNew();
        var model = (MainViewModel)window.DataContext!;
        string databasePath = Path.Combine(profile, "dowlume.db");
        var store = new SqliteStateStore(databasePath);
        string downloads = Path.Combine(profile, "Samples");
        Directory.CreateDirectory(downloads);
        var settings = new AppSettings { DownloadsPath = downloads };
        store.SaveSettings(settings);
        model.DownloadsPath = downloads;

        await VerifyLanguagesAsync(window, model, store);
        await Task.Run(() => VerifyFileWorkflowAsync(profile, databasePath, store, settings, readiness));
        await model.RefreshAsync();
        Require(model.ManagedCount == 3 && model.ErrorMessage.Length == 0, "Desktop refresh");
        await WriteReportAsync(Path.Combine(profile, reportName), elapsed.ElapsedMilliseconds);
    }

    private static (IFileReadiness Readiness, string ReportName) SelectNativePlatform()
    {
        if (OperatingSystem.IsWindows())
            return (new FileReadiness(TimeSpan.Zero, TimeSpan.Zero), "WINDOWS-WORKFLOW.json");
        if (OperatingSystem.IsMacOS())
            return (new MacFileReadiness(TimeSpan.Zero), "MACOS-WORKFLOW.json");
        if (OperatingSystem.IsLinux())
            return (new LinuxFileReadiness(TimeSpan.Zero), "LINUX-WORKFLOW.json");
        throw new PlatformNotSupportedException("Native package acceptance requires a supported desktop platform.");
    }

    private static async Task VerifyLanguagesAsync(MainWindow window, MainViewModel model, IStateStore store)
    {
        TabControl tabs = window.FindControl<TabControl>("MainTabs")!;
        Require(tabs.ItemCount > 0, "Navigation tabs");
        foreach (var language in MainViewModel.Languages)
        {
            model.SelectedLanguage = language;
            Require(store.LoadSettings()!.Language == language.Code && model.Text.Language == language.Code, "Language persistence");
            for (int tab = 0; tab < tabs.ItemCount; tab++)
            {
                tabs.SelectedIndex = tab;
                await Task.Delay(30);
            }
        }
    }

    private static async Task VerifyFileWorkflowAsync(string profile, string databasePath, IStateStore store,
        AppSettings settings, IFileReadiness readiness)
    {
        var mover = new SafeFileMover();
        var clock = new AcceptanceClock();
        var organizer = new DownloadOrganizer(store, mover, readiness, clock);
        string sample = Path.Combine(settings.DownloadsPath, "sample café.pdf");
        await File.WriteAllTextAsync(sample, "Disposable package acceptance data");
        FileIdentity identity = mover.GetIdentity(sample)!;
        PreviewItem preview = (await organizer.PreviewAsync(settings)).Single();
        await organizer.OrganizeAsync(sample, settings, expectedHash: preview.ContentHash);
        clock.Now = clock.Now.AddYears(1);
        await organizer.ReconcileAsync(settings);
        TrackedFile file = store.GetFiles().Single();
        Require(file.Status == LifecycleStatus.InReview, "Review");
        await organizer.SnoozeAsync(file.Id, settings);
        clock.Now = clock.Now.AddDays(8);
        await organizer.ReconcileAsync(settings);
        await organizer.KeepAsync(file.Id, settings);
        await organizer.UndoAsync(store.GetOperations().Last().Id, settings);

        string archive = Path.Combine(profile, "Archive");
        Directory.CreateDirectory(archive);
        await organizer.MoveAsync(file.Id, settings, archive);
        var reopened = new SqliteStateStore(databasePath);
        await new DownloadOrganizer(reopened, mover, readiness, clock)
            .UndoAsync(store.GetOperations().Last().Id, settings);
        Require(mover.GetIdentity(reopened.GetFiles().Single().CurrentPath) == identity, "Identity after restart and Undo");
        Require(reopened.GetOperations().All(operation => operation.Status == OperationStatus.Completed), "Completed journal");
        await VerifyMonitoringAsync(organizer, store, settings);
        await VerifyPeriodicOrganizationAsync(organizer, store, settings);
    }

    private static async Task VerifyPeriodicOrganizationAsync(DownloadOrganizer organizer, IStateStore store, AppSettings settings)
    {
        settings = settings with
        {
            UsePeriodicOrganization = true,
            OrganizationIntervalMinutes = 1
        };
        store.SaveSettings(settings);
        string source = Path.Combine(settings.DownloadsPath, "scheduled.txt");
        string partial = Path.Combine(settings.DownloadsPath, "unfinished.crdownload");
        await File.WriteAllTextAsync(source, "Periodic package acceptance");
        await File.WriteAllTextAsync(partial, "Unfinished sample");
        var errors = new ConcurrentQueue<Exception>();
        await using (var monitor = new PeriodicDownloadMonitor(settings,
            (path, token) => organizer.OrganizeAsync(path, settings, token),
            token => organizer.ReconcileAsync(settings, token), errors.Enqueue))
        {
            await Task.Delay(1000);
            Require(File.Exists(source) && store.GetFiles().Count == 2, "Periodic initial delay");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(80));
            while (store.GetFiles().Count < 3)
                await Task.Delay(100, timeout.Token);
        }
        Require(errors.IsEmpty && File.Exists(partial), "Periodic completed files only and drain");
        Require(await File.ReadAllTextAsync(Path.Combine(settings.DownloadsPath, "Documents", "scheduled.txt")) == "Periodic package acceptance", "Periodic content preservation");
        string paused = Path.Combine(settings.DownloadsPath, "paused.txt");
        await File.WriteAllTextAsync(paused, "Paused sample");
        await Task.Delay(500);
        Require(File.Exists(paused) && store.LoadSettings()!.UsePeriodicOrganization, "Periodic pause and saved preference");
    }

    private static async Task VerifyMonitoringAsync(DownloadOrganizer organizer, IStateStore store, AppSettings settings)
    {
        var errors = new ConcurrentQueue<Exception>();
        await using (var monitor = new DownloadMonitor(settings,
            (path, token) => organizer.OrganizeAsync(path, settings, token),
            token => organizer.ReconcileAsync(settings, token), errors.Enqueue))
        {
            await File.WriteAllTextAsync(Path.Combine(settings.DownloadsPath, "watched.txt"), "Watcher acceptance");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            while (store.GetFiles().Count < 2)
                await Task.Delay(50, timeout.Token);
        }
        Require(errors.IsEmpty, "Monitoring and shutdown");
    }

    private static Task WriteReportAsync(string path, long elapsedMilliseconds)
    {
        var report = new
        {
            Passed = true,
            EnglishAndSpanishTabs = true,
            LanguagePersistence = true,
            NativeOrganizeReviewSnoozeKeepMoveUndo = true,
            ReopenedJournal = true,
            NativeWatcherAndDrain = true,
            NativePeriodicDelayOrganizationAndDrain = true,
            ElapsedMilliseconds = elapsedMilliseconds,
            ManualDesktopAcceptance = false,
            ActualLoginAcceptance = false
        };
        return File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void Require(bool condition, string step)
    {
        if (!condition)
            throw new InvalidOperationException($"Package acceptance failed: {step}.");
    }

    private sealed class AcceptanceClock : TimeProvider
    {
        internal DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
