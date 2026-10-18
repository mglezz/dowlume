using System.Collections.Concurrent;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

public sealed class MonitoringTests
{
    [Fact]
    public async Task LockedFileIsNotReady()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("locked.pdf");
        using var writer = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
        var readiness = new FileReadiness(TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        Assert.False(await readiness.IsReadyAsync(path, CancellationToken.None));
    }

    [Fact]
    public async Task ChangedFileIsNotReady()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("changing.pdf");
        var readiness = new FileReadiness(TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
        Task<bool> ready = readiness.IsReadyAsync(path, CancellationToken.None);
        await File.AppendAllTextAsync(path, "More content");
        Assert.False(await ready);
        Assert.True(await readiness.IsReadyAsync(path, CancellationToken.None));
    }

    [Fact]
    public async Task FullQueueEventuallyDiscoversEveryFileAndIgnoresTemporaryFiles()
    {
        using var workspace = new TemporaryWorkspace();
        var errors = new ConcurrentQueue<Exception>();
        for (int index = 0; index < 40; index++)
        {
            workspace.CreateFile($"file-{index}.pdf");
        }
        workspace.CreateFile("incomplete.crdownload");
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        var store = new SqliteStateStore(workspace.Database);
        var organizer = new DownloadOrganizer(store, new SafeFileMover(), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
        await using (var monitor = new DownloadMonitor(settings, async (path, token) =>
        {
            await Task.Delay(5, token);
            // Watcher/scanner paths are hints and may be stale. Exercise the real idempotent consumer.
            await organizer.OrganizeAsync(path, settings, token);
        }, token => organizer.ReconcileAsync(settings, token), errors.Enqueue, TimeSpan.FromMilliseconds(30), capacity: 3))
        {
            // Durable SQLite commits on shared CI storage need more time than the old in-memory counter.
            await WaitUntilAsync(() => store.GetFiles().Count == 40, TimeSpan.FromSeconds(60),
                () => $"Only {store.GetFiles().Count}/40 files completed. Background errors: {string.Join(Environment.NewLine, errors)}");
        }
        // Shutdown must finish before checking background failures and final contents.
        Assert.Empty(errors);
        Assert.Equal(40, store.GetOperations().Count);
        Assert.All(store.GetOperations(), operation => Assert.Equal(OperationStatus.Completed, operation.Status));
        Assert.All(store.GetFiles(), file => Assert.Equal("Disposable test data", File.ReadAllText(file.CurrentPath)));
        Assert.Single(Directory.GetFiles(workspace.Downloads));
        Assert.True(File.Exists(Path.Combine(workspace.Downloads, "incomplete.crdownload")));
    }

    [Fact]
    public async Task RenameAndRestartRecoverDownloads()
    {
        using var workspace = new TemporaryWorkspace();
        string temporary = workspace.CreateFile("file.crdownload");
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        var seen = new ConcurrentDictionary<string, byte>();
        Task Process(string path, CancellationToken token)
        {
            seen.TryAdd(path, 0);
            return Task.CompletedTask;
        }
        await using (var first = new DownloadMonitor(settings, Process, _ => Task.CompletedTask, _ => { }, TimeSpan.FromMilliseconds(30)))
        {
            File.Move(temporary, Path.ChangeExtension(temporary, ".pdf"));
            await WaitUntilAsync(() => seen.Count == 1);
        }
        workspace.CreateFile("offline.pdf");
        await using var second = new DownloadMonitor(settings, Process, _ => Task.CompletedTask, _ => { }, TimeSpan.FromMilliseconds(30));
        await WaitUntilAsync(() => seen.Count == 2);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeoutDuration = null, Func<string>? diagnostics = null)
    {
        using var timeout = new CancellationTokenSource(timeoutDuration ?? TimeSpan.FromSeconds(10));
        try
        {
            while (!condition())
            {
                await Task.Delay(20, timeout.Token);
            }
        }
        catch (OperationCanceledException) { Assert.Fail(diagnostics?.Invoke() ?? "The expected monitoring condition did not complete before the timeout."); }
    }
}
