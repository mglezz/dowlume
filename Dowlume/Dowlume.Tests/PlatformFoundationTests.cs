using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Dowlume.App.Localization;
using Dowlume.App.Services;
using Dowlume.App.ViewModels;
using Dowlume.App.Views;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

[Collection("Desktop")]
public sealed class PlatformFoundationTests
{
    [Fact]
    public async Task PreviewPlatformCannotMutateFilesHistoryOrReplayPendingOperations()
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("sample.pdf", "Preserve this content");
        string destination = Path.Combine(workspace.Downloads, "Documents", "sample.pdf");
        var store = new SqliteStateStore(workspace.Database);
        var settings = new AppSettings { DownloadsPath = workspace.Downloads, Language = "es" };
        store.SaveSettings(settings);
        var reader = new SafeFileMover();
        var file = new TrackedFile(Guid.NewGuid(), source, destination, FileCategory.Documents,
            DateTimeOffset.UtcNow, LifecycleStatus.Organized, reader.Hash(source));
        var operation = new OperationRecord(Guid.NewGuid(), OperationKind.Organize, OperationStatus.Pending,
            DateTimeOffset.UtcNow, null, file, source, destination);
        store.SaveOperation(operation);
        var mover = new ReadOnlyMover();
        var organizer = new DownloadOrganizer(store, mover, new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
        await organizer.PreviewAsync(settings);
        Assert.Equal(operation, Assert.Single(store.GetOperations()));
        Assert.Empty(store.GetFiles());
        Func<Task>[] requests =
        [
            () => organizer.OrganizeAsync(source, settings),
            () => organizer.ReconcileAsync(settings),
            () => organizer.KeepAsync(file.Id, settings),
            () => organizer.SnoozeAsync(file.Id, settings),
            () => organizer.MoveAsync(file.Id, settings, workspace.Root),
            () => organizer.UndoAsync(operation.Id, settings),
            () => organizer.RetryRecoveryAsync()
        ];
        foreach (Func<Task> request in requests)
        {
            await Assert.ThrowsAsync<PlatformNotSupportedException>(request);
        }
        Assert.Equal(0, mover.MoveCalls);
        Assert.Equal(operation, Assert.Single(store.GetOperations()));
        Assert.Empty(store.GetFiles());
        Assert.Equal("es", store.LoadSettings()!.Language);
        Assert.Equal("Preserve this content", File.ReadAllText(source));
        Assert.False(Directory.Exists(Path.GetDirectoryName(destination)));
    }

    [Fact]
    public void ProductionMoverRefusesUnacceptedNativeBackendsBeforeCreatingDirectories()
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("original.txt");
        string destination = Path.Combine(workspace.Downloads, "new", "copy.txt");
        var mover = new SafeFileMover();
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            Assert.True(mover.CanMove);
            mover.Move(source, destination, mover.Hash(source));
            Assert.False(File.Exists(source));
            Assert.Equal("Disposable test data", File.ReadAllText(destination));
        }
        else
        {
            Assert.False(mover.CanMove);
            Assert.Throws<PlatformNotSupportedException>(() => mover.Move(source, destination, mover.Hash(source)));
            Assert.True(File.Exists(source));
            Assert.False(Directory.Exists(Path.GetDirectoryName(destination)));
        }
    }

    [Fact]
    public async Task PreviewDesktopExplainsRestrictionsAndCannotHideWithoutATray()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await session.Dispatch(async () =>
        {
            using var workspace = new TemporaryWorkspace();
            string source = workspace.CreateFile("sample.pdf");
            var store = new SqliteStateStore(workspace.Database);
            store.SaveSettings(new AppSettings { DownloadsPath = workspace.Downloads, Language = "es" });
            var organizer = new DownloadOrganizer(store, new ReadOnlyMover(), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
            var window = new MainWindow();
            var model = new MainViewModel(store, organizer, new DesktopDialogs(window), new DiagnosticLog(Path.Combine(workspace.Root, "diagnostics.jsonl")));
            window.DataContext = model;
            window.Show();
            Assert.True(model.IsPreviewPlatform);
            Assert.Contains("vista previa", model.PlatformNotice);
            Assert.False(model.StartCommand.CanExecute(null));
            Assert.False(model.RetryRecoveryCommand.CanExecute(null));
            await model.PreviewCommand.ExecuteAsync(null);
            Assert.Single(model.PreviewItems);
            await model.StartCommand.ExecuteAsync(null);
            Assert.False(model.IsMonitoring);
            Assert.Empty(store.GetOperations());
            Assert.True(File.Exists(source));
            Button hide = window.FindControl<Button>("HideToTrayButton")!;
            Assert.False(hide.IsVisible);
            hide.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(window.IsVisible);
            await DesktopWindowTest.CloseAsync(window);
            return true;
        }, timeout.Token);
    }

    [Theory]
    [InlineData(DesktopSystem.Windows)]
    [InlineData(DesktopSystem.MacOS)]
    [InlineData(DesktopSystem.Linux)]
    public void FileManagerUsesLiteralArgumentsWithoutAShell(DesktopSystem system)
    {
        using var workspace = new TemporaryWorkspace();
        string path = Path.Combine(workspace.Downloads, "folder with spaces", "$(touch injected);file.txt");
        ProcessStartInfo start = NativeFileManager.CreateStartInfo(system, path);
        Assert.False(start.UseShellExecute);
        Assert.Empty(start.Arguments);
        Assert.Equal(system == DesktopSystem.Linux ? Path.GetDirectoryName(path) : path, start.ArgumentList.Last());
        Assert.Equal(system == DesktopSystem.Linux ? 1 : 2, start.ArgumentList.Count);
    }

    private sealed class ReadOnlyMover : IFileMover
    {
        public bool CanMove => false;
        public int MoveCalls
        {
            get; private set;
        }
        public string Hash(string path) => new SafeFileMover().Hash(path);
        public void Move(string source, string destination, string expectedHash)
        {
            MoveCalls++;
            throw new InvalidOperationException("A read-only mover must never be called.");
        }
    }
}
