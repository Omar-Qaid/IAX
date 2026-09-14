namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

/// <summary>Cooperatively stops active work when persisted cancellation is observed.</summary>
public static class BatchCancellationMonitor
{
    public static async Task RunAsync(Func<CancellationToken, Task<bool>> cancellationRequested,
        CancellationTokenSource execution, CancellationToken stopMonitoring, Action onCancellation)
    {
        try
        {
            while (!stopMonitoring.IsCancellationRequested)
            {
                if (await cancellationRequested(stopMonitoring))
                {
                    onCancellation();
                    await execution.CancelAsync();
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(2), stopMonitoring);
            }
        }
        catch (OperationCanceledException) when (stopMonitoring.IsCancellationRequested) { }
    }
}
