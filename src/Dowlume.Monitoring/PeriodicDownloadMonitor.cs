using Dowlume.Core;

namespace Dowlume.Monitoring;

/// <summary>Runs sequential passes, with a full quiet interval before each pass.</summary>
public sealed class PeriodicDownloadMonitor : IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _worker;

    public PeriodicDownloadMonitor(AppSettings settings, Func<string, CancellationToken, Task> process,
        Func<CancellationToken, Task> reconcile, Action<Exception> reportError, TimeProvider? timeProvider = null)
    {
        settings.Validate();
        if (!settings.UsePeriodicOrganization)
        {
            throw new ArgumentException("Periodic organization must be explicitly enabled.", nameof(settings));
        }
        _worker = Task.Run(async () =>
        {
            CancellationToken token = _shutdown.Token;
            try
            {
                while (true)
                {
                    // Delay after completion too: slow passes never overlap or trigger catch-up bursts.
                    await Task.Delay(TimeSpan.FromMinutes(settings.OrganizationIntervalMinutes), timeProvider ?? TimeProvider.System, token);
                    try
                    {
                        await reconcile(token);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
                    catch (Exception error) { reportError(error); }
                    try
                    {
                        foreach (string path in Directory.EnumerateFiles(settings.DownloadsPath))
                        {
                            token.ThrowIfCancellationRequested();
                            if (FileClassifier.IsExcluded(path, settings))
                                continue;
                            try
                            {
                                await process(path, token);
                            }
                            catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
                            catch (Exception error) { reportError(error); }
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
                    catch (Exception error) { reportError(error); }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _shutdown.CancelAsync();
        await _worker;
        _shutdown.Dispose();
    }
}
