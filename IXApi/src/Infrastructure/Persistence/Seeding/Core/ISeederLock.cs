using System;
using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    public interface ISeederLock
    {
        /// <summary>
        /// Acquires a distributed startup synchronization lock across server instances.
        /// Disposing the returned object releases the lock.
        /// </summary>
        Task<IAsyncDisposable> AcquireLockAsync(int timeoutSeconds, CancellationToken cancellationToken);
    }
}
