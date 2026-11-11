using Dowlume.Core;

namespace Dowlume.Monitoring;

/// <summary>Independent, non-overlapping folder passes; never runs immediately at startup.</summary>
public sealed class FolderMonitor : IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _worker;

    public FolderMonitor(AppSettings settings, Func<CancellationToken, Task> scan, Action<Exception> reportError,
        TimeProvider? clock = null)
    {
        settings.Validate();
        if (!settings.OrganizeFolders)
            throw new ArgumentException("Folder organization must be explicitly enabled.");
        _worker = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(settings.FolderIntervalMinutes), clock ?? TimeProvider.System, _shutdown.Token);
                    try
                    {
                        await scan(_shutdown.Token);
                    }
                    catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { return; }
                    catch (Exception error) { reportError(error); }
                }
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { }
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _shutdown.CancelAsync();
        await _worker;
        _shutdown.Dispose();
    }
}
