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
        var data = task.GetType().GetProperty("Result")?.GetValue(task);
        Assert.NotNull(data);
        Assert.InRange(GetArrayLength(data!, "Performers"), 1, 10);
        Assert.InRange(GetArrayLength(data!, "Processes"), 1, 10);
    }

    [Fact]
    public async Task Embedded_organization_employee_snapshot_deserializes()
    {
        var method = typeof(LegacyOrganizationEmployeeSeeder).GetMethod("ReadAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(null, [CancellationToken.None]));
        await task;
        var data = task.GetType().GetProperty("Result")?.GetValue(task);
        Assert.NotNull(data);
        Assert.InRange(GetArrayLength(data!, "Employees"), 1, 10);
    }

    private static int GetArrayLength(object value, string propertyName)
    {
        var property = value.GetType().GetProperty(propertyName);
        return Assert.IsAssignableFrom<Array>(property?.GetValue(value)).Length;
    }
}
