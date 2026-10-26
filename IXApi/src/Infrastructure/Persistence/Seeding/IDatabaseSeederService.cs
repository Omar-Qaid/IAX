using System;
using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding
{
    public interface IDatabaseSeederService
    {
        Task SeedAsync(CancellationToken ct = default);
        Task RunOnceAsync(string seedName, Func<Task> action, CancellationToken ct = default);
    }
}
