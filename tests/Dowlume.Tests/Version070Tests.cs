using System.Text.Json;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Styling;
using Dowlume.App.Services;
using Dowlume.App.ViewModels;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

[Collection("Desktop")]
public sealed class Version070Tests
{
    [Fact]
    public void OldProfilesRemainOptInAndFolderConfigurationRequiresSeparateDestination()
    {
        AppSettings old = JsonSerializer.Deserialize<AppSettings>("{}")!;
        Assert.False(old.ResumeMonitoringOnStartup);
        Assert.False(old.MonitoringConsentGranted);
        Assert.False(old.OrganizeFolders);
        Assert.Equal("System", old.Theme);
        using var workspace = new TemporaryWorkspace();
        var settings = old with
        {
            DownloadsPath = workspace.Downloads
        };
        settings.Validate();
        Assert.Throws<ArgumentException>(() => (settings with { FolderIntervalMinutes = 19 }).Validate());
        Assert.Throws<ArgumentException>(() => (settings with { OrganizeFolders = true, CollectedFoldersName = "documents" }).Validate());
        Assert.Throws<ArgumentException>(() => (settings with { CollectedFoldersName = "../Folders" }).Validate());
        Assert.False(settings.HasSameValuesAs(settings with
        {
            OrganizeFolders = true
        }));
        Assert.True(settings.HasSameValuesAs(settings with
        {
            Theme = "Dark",
            ResumeMonitoringOnStartup = true
        }));
    }

    [Fact]
    public async Task StartupNeedsConfirmedConsentPersistsThemeAndExplicitOffDisablesRestart()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            var store = new SqliteStateStore(workspace.Database);
            var log = new DiagnosticLog(Path.Combine(workspace.Root, "log.jsonl"));
            var dialogs = new Dialogs();
            MainViewModel Model() => new(store, new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness()), dialogs, log,
                suggestedDownloads: workspace.Downloads);
            var model = Model();
            try
            {
                model.ResumeMonitoringOnStartup = true;
                model.UsePeriodicOrganization = true;
                await model.SaveSettingsCommand.ExecuteAsync(null);
                await model.ResumeOnStartupAsync();
                Assert.False(model.IsMonitoring);
                await model.ToggleMonitoringCommand.ExecuteAsync(null);
                Assert.False(model.IsMonitoring);
                await model.PreviewCommand.ExecuteAsync(null);
                model.SelectedTheme = "Dark";
                Assert.Equal(ThemeVariant.Dark, Application.Current!.RequestedThemeVariant);
                Assert.Equal("Dark", store.LoadSettings()!.Theme);
                await model.StartCommand.ExecuteAsync(null);
                Assert.True(model.IsMonitoring);
                Assert.True(store.LoadSettings()!.MonitoringConsentGranted);
            }
            finally { await model.ShutdownAsync(); }

            model = Model();
            try
            {
                Assert.Equal("Dark", model.SelectedTheme);
                await model.ResumeOnStartupAsync();
                Assert.True(model.IsMonitoring);
                await model.ResumeOnStartupAsync();
                Assert.Equal(1, dialogs.Confirmations);
                Assert.Empty(store.GetOperations());
                await model.ToggleMonitoringCommand.ExecuteAsync(null);
                Assert.False(model.IsMonitoring);
                Assert.False(store.LoadSettings()!.ResumeMonitoringOnStartup);
                model.ResumeMonitoringOnStartup = true;
                model.Rules[0].FolderName = "New documents";
                await model.SaveSettingsCommand.ExecuteAsync(null);
                Assert.False(store.LoadSettings()!.MonitoringConsentGranted);
            }
            finally { await model.ShutdownAsync(); }
            model = Model();
            try
            {
                await model.ResumeOnStartupAsync();
                Assert.False(model.IsMonitoring);
            }
            finally { await model.ShutdownAsync(); }
            return true;
        }, timeout.Token);
    }

    [WindowsFact]
    public async Task FolderPassWaitsForStabilityPreservesContentsAndSkipsUnsafeOrManagedFolders()
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        var settings = new AppSettings { DownloadsPath = workspace.Downloads, OrganizeFolders = true };
        var clock = new Clock();
        var organizer = new FolderOrganizer(store, clock);
        string Folder(string name, string file = "data.txt")
        {
            string path = Path.Combine(workspace.Downloads, name);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, file), name);
            File.SetLastWriteTimeUtc(Path.Combine(path, file), clock.Now.AddHours(-1).UtcDateTime);
            Directory.SetLastWriteTimeUtc(path, clock.Now.AddHours(-1).UtcDateTime);
            return path;
        }
        string stable = Folder("Extracted");
        string changing = Folder("Changing");
        string partial = Folder("Partial", "archive.crdownload");
        string documents = Folder("Documents");
        string busy = Folder("Busy");
        string collision = Folder("Duplicate");
        Directory.CreateDirectory(Path.Combine(workspace.Downloads, "Folders", "Duplicate"));
        using var writer = new FileStream(Path.Combine(busy, "data.txt"), FileMode.Open, FileAccess.Write, FileShare.None);
        await organizer.ScanAsync(settings);
        Assert.True(Directory.Exists(stable));
        Assert.Empty(store.GetOperations());
        clock.Now = clock.Now.AddMinutes(19);
        await organizer.ScanAsync(settings);
        Assert.Empty(store.GetOperations());
        File.AppendAllText(Path.Combine(changing, "data.txt"), " changed");
        clock.Now = clock.Now.AddMinutes(2);
        await organizer.ScanAsync(settings);
        Assert.False(Directory.Exists(stable));
        Assert.Equal("Extracted", File.ReadAllText(Path.Combine(workspace.Downloads, "Folders", "Extracted", "data.txt")));
        Assert.True(Directory.Exists(changing));
        Assert.True(Directory.Exists(partial));
        Assert.True(Directory.Exists(documents));
        Assert.True(Directory.Exists(collision));
        Assert.True(Directory.Exists(busy));
        OperationRecord moved = Assert.Single(store.GetOperations());
        Assert.Equal(OperationKind.OrganizeFolder, moved.Kind);
        Assert.Equal(OperationStatus.Completed, moved.Status);
        Assert.False(moved.CanUndo);
        Assert.Empty(store.GetFiles());
        // Interrupted folder actions never enter file recovery or replay automatically.
        store.SaveOperation(moved with
        {
            Id = Guid.NewGuid(),
            Status = OperationStatus.Pending,
            SourcePath = changing
        });
        var files = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness());
        await files.RetryRecoveryAsync();
        Assert.Contains(store.GetOperations(), operation => operation.Status == OperationStatus.NeedsAttention);
        Assert.DoesNotContain(changing, organizer.Preview(settings));
        Assert.True(Directory.Exists(changing));
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Dialogs : IDesktopDialogs
    {
        public int Confirmations
        {
            get; private set;
        }
        public Task<string?> ChooseFolderAsync(string title) => Task.FromResult<string?>(null);
        public Task<bool> ConfirmAsync(string title, string message, string action)
        {
            Confirmations++;
            return Task.FromResult(true);
        }
        public void RevealFile(string path)
        {
        }
    }
}
