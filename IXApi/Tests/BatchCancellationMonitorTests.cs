using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using Xunit;

namespace IXApi.Tests;

public class BatchCancellationMonitorTests
{
    [Fact]
    public async Task CancellationSignalsRunningWorkAndRecordsReason()
    {
        using var execution = new CancellationTokenSource();
        var marked = false;
        var work = Task.Delay(Timeout.InfiniteTimeSpan, execution.Token);
        await BatchCancellationMonitor.RunAsync(_ => Task.FromResult(true), execution, CancellationToken.None, () => marked = true);
        Assert.True(marked);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work);
    }

    [Fact]
    public async Task StoppingMonitorDoesNotCancelSuccessfulWork()
    {
        using var execution = new CancellationTokenSource();
        using var monitoring = new CancellationTokenSource();
        var marked = false;
        var monitor = BatchCancellationMonitor.RunAsync(_ => { monitoring.Cancel(); return Task.FromResult(false); },
            execution, monitoring.Token, () => marked = true);
        await monitor;
        Assert.False(marked);
        Assert.False(execution.IsCancellationRequested);
    }
}
