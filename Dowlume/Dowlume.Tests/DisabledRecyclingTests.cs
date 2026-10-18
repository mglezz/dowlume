using System.Reflection;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Dowlume.Monitoring;

namespace Dowlume.Tests;

public sealed class DisabledRecyclingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AdapterAlwaysRefusesWithoutOpeningOrChangingFiles(bool changed)
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("review.txt", "Reviewed content");
        string sentinel = workspace.CreateFile("sentinel.txt", "Earlier content");
        string hash = new SafeFileMover().Hash(source);
        if (changed)
        {
            File.WriteAllText(source, "Changed content");
        }
        string expected = File.ReadAllText(source);
        IRecycleBin adapter = new WindowsRecycleBin();
        using (var locked = new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            NotSupportedException error = await Assert.ThrowsAsync<NotSupportedException>(() => adapter.RecycleAsync(source, hash));
            Assert.Equal(WindowsRecycleBin.DisabledMessage, DownloadOrganizer.FriendlyError(error));
        }
        Assert.Equal(expected, File.ReadAllText(source));
        Assert.Equal("Earlier content", File.ReadAllText(sentinel));
        await Assert.ThrowsAsync<NotSupportedException>(() => adapter.RecycleAsync("\0", "invalid hash"));
    }

    [Fact]
    public async Task InternalExecutionRefusesRecycleBeforeJournaling()
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        var organizer = new DownloadOrganizer(store, new SafeFileMover(), new FileReadiness());
        string source = workspace.CreateFile("review.txt");
        var file = new TrackedFile(Guid.NewGuid(), source, source, FileCategory.Documents,
            DateTimeOffset.UtcNow, LifecycleStatus.InReview, new SafeFileMover().Hash(source));
        MethodInfo execute = typeof(DownloadOrganizer).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => execute.Invoke(organizer,
            [OperationKind.Recycle, file, file with { Status = LifecycleStatus.Recycled }, source, null, CancellationToken.None, null]));
        Assert.IsType<NotSupportedException>(error.InnerException);
        MethodInfo decide = typeof(DownloadOrganizer).GetMethod("DecideAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await Assert.ThrowsAsync<NotSupportedException>(() => (Task)decide.Invoke(organizer,
            [file.Id, null, OperationKind.Recycle, null, CancellationToken.None])!);
        Assert.Equal(file.ContentHash, new SafeFileMover().Hash(source));
        Assert.Empty(store.GetOperations());
        Assert.Empty(store.GetFiles());
    }

    [Theory]
    [InlineData(OperationStatus.Pending, "intact", false)]
    [InlineData(OperationStatus.NeedsAttention, "intact", false)]
    [InlineData(OperationStatus.Pending, "changed", false)]
    [InlineData(OperationStatus.NeedsAttention, "missing", false)]
    [InlineData(OperationStatus.Pending, "missing", true)]
    [InlineData(OperationStatus.Completed, "missing", false)]
    public async Task HistoricalRecycleRecoveryNeverReplaysOrInventsRecoverability(OperationStatus status, string sourceState, bool hasDestination)
    {
        using var workspace = new TemporaryWorkspace();
        var store = new SqliteStateStore(workspace.Database);
        var mover = new SafeFileMover();
        string source = sourceState == "missing" ? Path.Combine(workspace.Downloads, "absent.txt") : workspace.CreateFile("review.txt", "Original");
        string reference = workspace.CreateFile("reference.txt", "Original");
        string? destination = hasDestination ? workspace.CreateFile("unexpected-copy.txt", "Original") : null;
        var before = new TrackedFile(Guid.NewGuid(), source, source, FileCategory.Documents,
            DateTimeOffset.UtcNow, LifecycleStatus.InReview, mover.Hash(reference));
        var operation = new OperationRecord(Guid.NewGuid(), OperationKind.Recycle, status, DateTimeOffset.UtcNow,
            before, before with
            {
                Status = LifecycleStatus.Recycled
            }, source, destination);
        store.SaveFile(before);
        store.SaveOperation(operation);
        if (sourceState == "changed")
        {
            File.WriteAllText(source, "Edited");
        }

        for (int attempt = 0; attempt < 2; attempt++)
        {
            // Reopen the persisted journal on each attempt to exercise upgrade/restart behavior.
            var reopened = new SqliteStateStore(workspace.Database);
            var organizer = new DownloadOrganizer(reopened, mover, new FileReadiness());
            await organizer.RetryRecoveryAsync();
            OperationRecord recovered = Assert.Single(reopened.GetOperations());
            Assert.Equal(status == OperationStatus.Completed ? OperationStatus.Completed :
                sourceState == "intact" ? OperationStatus.Failed : OperationStatus.NeedsAttention, recovered.Status);
            Assert.Equal(operation.Before, recovered.Before);
            Assert.Equal(operation.After, recovered.After);
            Assert.Equal(before, Assert.Single(reopened.GetFiles()));
            if (status == OperationStatus.Completed)
            {
                Assert.Equal(operation, recovered);
            }
            else
            {
                Assert.Contains("disabled", recovered.Error);
            }
            await Assert.ThrowsAsync<InvalidOperationException>(() => organizer.UndoAsync(operation.Id, new AppSettings { DownloadsPath = workspace.Downloads }));
        }
        if (sourceState == "missing")
        {
            Assert.False(File.Exists(source));
        }
        else
        {
            Assert.Equal(sourceState == "changed" ? "Edited" : "Original", File.ReadAllText(source));
        }
        if (destination is not null)
        {
            Assert.Equal("Original", File.ReadAllText(destination));
        }
        Assert.Equal("Original", File.ReadAllText(reference));
    }
}
