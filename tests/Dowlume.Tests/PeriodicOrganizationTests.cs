using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

public sealed class PeriodicOrganizationTests
{
    [Fact]
    public async Task FolderTimerWaitsAtLeastTwentyMinutesAndCancelsWithoutOverlap()
    {
        using var workspace = new TemporaryWorkspace();
        var settings = new AppSettings { DownloadsPath = workspace.Downloads, OrganizeFolders = true };
        var clock = new ManualClock();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bool cancelled = false;
        int calls = 0;
        var monitor = new FolderMonitor(settings, async token =>
        {
            calls++;
            started.SetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, token);
            }
            finally { cancelled = token.IsCancellationRequested; }
        }, error => throw error, clock);
        ManualTimer timer = await clock.NextAsync();
        Assert.Equal(TimeSpan.FromMinutes(20), timer.DueTime);
        Assert.Equal(0, calls);
        timer.Fire();
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(clock.HasPendingTimer);
        await monitor.DisposeAsync();
        Assert.True(cancelled);
        Assert.Equal(1, calls);
        Assert.Throws<ArgumentException>(() => new FolderMonitor(settings with { OrganizeFolders = false }, _ => Task.CompletedTask, _ => { }));
        Assert.Throws<ArgumentException>(() => new FolderMonitor(settings with { FolderIntervalMinutes = 19 }, _ => Task.CompletedTask, _ => { }));
    }

    [Fact]
    public void OlderProfilesRemainOptInAndScheduleChangesInvalidateConsent()
    {
        AppSettings settings = JsonSerializer.Deserialize<AppSettings>("{}")!;
        Assert.False(settings.UsePeriodicOrganization);
        Assert.Equal(10, settings.OrganizationIntervalMinutes);
        Assert.False(settings.HasSameValuesAs(settings with
        {
            UsePeriodicOrganization = true
        }));
        Assert.False(settings.HasSameValuesAs(settings with
        {
            OrganizationIntervalMinutes = 20
        }));
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        settings = settings with
        {
            DownloadsPath = workspace.Downloads,
            UsePeriodicOrganization = true,
            OrganizationIntervalMinutes = 20
        };
        store.SaveSettings(settings);
        Assert.True(settings.HasSameValuesAs(new SqliteStateStore(workspace.Database).LoadSettings()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public void InvalidIntervalsAreRejected(int minutes)
    {
        using var workspace = new TemporaryWorkspace();
        Assert.Throws<ArgumentException>(() => new AppSettings
        {
            DownloadsPath = workspace.Downloads,
            OrganizationIntervalMinutes = minutes
        }.Validate());
    }

    [Fact]
    public async Task PassesWaitSkipExcludedFilesRecoverFromErrorsAndStopOnPause()
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("report.pdf");
        workspace.CreateFile("unfinished.crdownload");
        workspace.CreateFile("ignored.log");
        workspace.CreateFile("subfolder/untouched.pdf");
        var settings = new AppSettings { DownloadsPath = workspace.Downloads, UsePeriodicOrganization = true, ExcludedExtensions = [".log"] };
        var clock = new ManualClock();
        var seen = new ConcurrentQueue<string>();
        var errors = new ConcurrentQueue<Exception>();
        int reconciliations = 0;
        var monitor = new PeriodicDownloadMonitor(settings, (path, _) =>
        {
            seen.Enqueue(path);
            return Task.CompletedTask;
        }, _ => Interlocked.Increment(ref reconciliations) == 1
            ? Task.FromException(new IOException("Transient failure")) : Task.CompletedTask, errors.Enqueue, clock);
        try
        {
            ManualTimer first = await clock.NextAsync();
            Assert.Equal(TimeSpan.FromMinutes(10), first.DueTime);
            Assert.Empty(seen);
            Assert.Equal(0, reconciliations);
            string later = workspace.CreateFile("later.txt");
            Assert.Empty(seen);
            first.Fire();
            ManualTimer second = await clock.NextAsync();
            Assert.Equal(new[] { later, source }.Order(), seen.Order());
            Assert.Single(errors);
            second.Fire();
            ManualTimer third = await clock.NextAsync();
            Assert.Equal(4, seen.Count);
            Assert.Equal(2, reconciliations);
            await monitor.DisposeAsync();
            monitor = null!;
            third.Fire();
            Assert.Equal(4, seen.Count);
        }
        finally { if (monitor is not null) await monitor.DisposeAsync(); }
    }

    [Fact]
    public async Task SlowPassDoesNotScheduleAnotherPassAndPauseCancelsActiveWork()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.CreateFile("slow.pdf");
        var clock = new ManualClock();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bool cancelled = false;
        var monitor = new PeriodicDownloadMonitor(new AppSettings { DownloadsPath = workspace.Downloads, UsePeriodicOrganization = true },
            async (_, token) =>
            {
                started.SetResult();
                try
                {
                    await Task.Delay(Timeout.Infinite, token);
                }
                finally { cancelled = token.IsCancellationRequested; }
            }, _ => Task.CompletedTask, _ => { }, clock);
        (await clock.NextAsync()).Fire();
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(clock.HasPendingTimer);
        await monitor.DisposeAsync();
        Assert.True(cancelled);
    }

    [Fact]
    public async Task ScheduledMovesRespectReadinessCollisionsAndUndo()
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("report.pdf", "new content");
        string existing = workspace.CreateFile("Documents/report.pdf", "existing content");
        string incomplete = workspace.CreateFile("unfinished.part");
        string busy = workspace.CreateFile("busy.pdf");
        var settings = new AppSettings { DownloadsPath = workspace.Downloads, UsePeriodicOrganization = true };
        var store = new SqliteStateStore(workspace.Database);
        var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new FileReadiness(TimeSpan.Zero, TimeSpan.Zero));
        var clock = new ManualClock();
        var errors = new ConcurrentQueue<Exception>();
        await using var monitor = new PeriodicDownloadMonitor(settings,
            (path, token) => organizer.OrganizeAsync(path, settings, token),
            token => organizer.ReconcileAsync(settings, token), errors.Enqueue, clock);
        using (var held = new FileStream(busy, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            (await clock.NextAsync()).Fire();
            ManualTimer next = await clock.NextAsync();
            OperationRecord operation = Assert.Single(store.GetOperations());
            Assert.Equal(OperationStatus.Completed, operation.Status);
            Assert.NotEqual(existing, operation.DestinationPath);
            Assert.Equal("existing content", File.ReadAllText(existing));
            Assert.Equal("new content", File.ReadAllText(operation.DestinationPath!));
            Assert.True(File.Exists(incomplete));
            Assert.True(File.Exists(busy));
            await organizer.UndoAsync(operation.Id, settings);
            next.Fire();
            await clock.NextAsync();
            Assert.Equal(2, store.GetOperations().Count);
            Assert.Equal("new content", File.ReadAllText(source));
            Assert.Empty(errors);
        }
    }

    private sealed class ManualClock : TimeProvider
    {
        private readonly Channel<ManualTimer> _timers = Channel.CreateUnbounded<ManualTimer>();
        public bool HasPendingTimer => _timers.Reader.TryPeek(out _);
        public async Task<ManualTimer> NextAsync() => await _timers.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(callback, state, dueTime);
            _timers.Writer.TryWrite(timer);
            return timer;
        }
    }

    private sealed class ManualTimer(TimerCallback callback, object? state, TimeSpan dueTime) : ITimer
    {
        private bool _disposed;
        public TimeSpan DueTime { get; } = dueTime;
        public void Fire()
        {
            if (!_disposed)
                callback(state);
        }
        public bool Change(TimeSpan dueTime, TimeSpan period) => throw new NotSupportedException();
        public void Dispose() => _disposed = true;
        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
