using System.Collections.Concurrent;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Protocol;

public sealed class PerUserConcurrencyLimiter(IOptions<IXMcpOptions> options)
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> entries = new(StringComparer.Ordinal);

    public IAsyncDisposable? TryAcquire(string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        var semaphore = entries.GetOrAdd(
            subject,
            _ => new SemaphoreSlim(
                options.Value.MaximumConcurrentCallsPerUser,
                options.Value.MaximumConcurrentCallsPerUser));
        return semaphore.Wait(0) ? new Lease(semaphore) : null;
    }

    private sealed class Lease(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        private int disposed;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                semaphore.Release();
            }

            return ValueTask.CompletedTask;
        }
    }
}
