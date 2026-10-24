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
[System.Runtime.Versioning.SupportedOSPlatform("macos")]
internal static class MacPackageAcceptance
{
    internal static async Task RunAsync(MainWindow window, string profile)
    {
        if (!Program.IsSmokeTest || !Program.IsCustomDataDirectory || !OperatingSystem.IsMacOS())
            throw new InvalidOperationException("Package acceptance requires an isolated macOS smoke profile.");
        var elapsed = Stopwatch.StartNew();
        var model = (MainViewModel)window.DataContext!;
        var store = new SqliteStateStore(Path.Combine(profile, "dowlume.db"));
        string downloads = Path.Combine(profile, "Samples");
        Directory.CreateDirectory(downloads);
        var settings = new AppSettings { DownloadsPath = downloads };
        store.SaveSettings(settings);
        model.DownloadsPath = downloads;
        foreach (var language in MainViewModel.Languages)
        {
            model.SelectedLanguage = language;
            Require(store.LoadSettings()!.Language == language.Code && model.Text.Language == language.Code, "Language persistence");
            for (int tab = 0; tab < 5; tab++)
            {
                window.FindControl<TabControl>("MainTabs")!.SelectedIndex = tab;
                await Task.Delay(30);
            }
        }
        await Task.Run(async () =>
        {
            var mover = new SafeFileMover();
            var clock = new AcceptanceClock();
            var organizer = new DownloadOrganizer(store, mover, new MacFileReadiness(TimeSpan.Zero), clock);
            string sample = Path.Combine(downloads, "sample café.pdf");
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
            var reopened = new SqliteStateStore(Path.Combine(profile, "dowlume.db"));
            await new DownloadOrganizer(reopened, mover, new MacFileReadiness(TimeSpan.Zero), clock)
                .UndoAsync(store.GetOperations().Last().Id, settings);
            Require(mover.GetIdentity(reopened.GetFiles().Single().CurrentPath) == identity, "Identity after restart and Undo");
            Require(reopened.GetOperations().All(operation => operation.Status == OperationStatus.Completed), "Completed journal");
            var errors = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
            await using (var monitor = new DownloadMonitor(settings,
                (path, token) => organizer.OrganizeAsync(path, settings, token),
                token => organizer.ReconcileAsync(settings, token), errors.Enqueue))
            {
                await File.WriteAllTextAsync(Path.Combine(downloads, "watched.txt"), "Watcher acceptance");
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                while (store.GetFiles().Count < 2)
                    await Task.Delay(50, timeout.Token);
            }
            Require(errors.IsEmpty, "Monitoring and shutdown");
        });
        await model.RefreshAsync();
        Require(model.ManagedCount == 2 && model.ErrorMessage.Length == 0, "Desktop refresh");
        var report = new
        {
            Passed = true,
            EnglishAndSpanishTabs = true,
            LanguagePersistence = true,
            NativeOrganizeReviewSnoozeKeepMoveUndo = true,
            ReopenedJournal = true,
            NativeWatcherAndDrain = true,
            ElapsedMilliseconds = elapsed.ElapsedMilliseconds,
            ManualDesktopAcceptance = false,
            ActualLoginAcceptance = false
        };
        await File.WriteAllTextAsync(Path.Combine(profile, "MACOS-WORKFLOW.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
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
