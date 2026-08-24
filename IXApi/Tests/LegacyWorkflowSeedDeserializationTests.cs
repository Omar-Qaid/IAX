using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using Xunit;

namespace IXApi.Tests;

public sealed class LegacyWorkflowSeedDeserializationTests
{
    [Fact]
    public async Task Embedded_snapshot_deserializes()
    {
        var method = typeof(LegacyWorkflowMasterDataSeeder).GetMethod("ReadDataAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(null, [CancellationToken.None]));
        await task;
        Assert.NotNull(task.GetType().GetProperty("Result")?.GetValue(task));
    }

    [Fact]
    public async Task Embedded_organization_employee_snapshot_deserializes()
    {
        var method = typeof(LegacyOrganizationEmployeeSeeder).GetMethod("ReadAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(null, [CancellationToken.None]));
        await task;
        Assert.NotNull(task.GetType().GetProperty("Result")?.GetValue(task));
    }
}
