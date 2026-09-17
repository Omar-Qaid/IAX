using IAX.IXMcp.Configuration;
using IAX.IXMcp.Protocol;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Tests;

public sealed class PerUserConcurrencyLimiterTests
{
    [Fact]
    public async Task Limits_each_user_independently_and_releases_on_dispose()
    {
        var limiter = new PerUserConcurrencyLimiter(Options.Create(new IXMcpOptions
        {
            ContractDirectory = "unused",
            IXApiBaseUrl = "https://ixapi.test",
            MaximumConcurrentCallsPerUser = 1
        }));
        await using var firstUserLease = limiter.TryAcquire("user-a");
        await using var secondUserLease = limiter.TryAcquire("user-b");

        Assert.NotNull(firstUserLease);
        Assert.NotNull(secondUserLease);
        Assert.Null(limiter.TryAcquire("user-a"));

        await firstUserLease.DisposeAsync();
        await using var replacement = limiter.TryAcquire("user-a");
        Assert.NotNull(replacement);
    }
}
