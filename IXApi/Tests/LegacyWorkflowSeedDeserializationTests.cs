using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using Xunit;

namespace IXApi.Tests;

public sealed class LegacyWorkflowSeedDeserializationTests
{
    [Fact]
    public async Task Missing_source_connection_uses_embedded_snapshot()
    {
        var seeder = new OthersDBWorkflowMasterFromSeeder();
        var method = typeof(OthersDBWorkflowMasterFromSeeder).GetMethod("ReadDataAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(seeder, [CancellationToken.None]));
        await task;
        var data = task.GetType().GetProperty("Result")?.GetValue(task);
        Assert.NotNull(data);
        Assert.InRange(GetArrayLength(data!, "Performers"), 1, 10);
        Assert.InRange(GetArrayLength(data!, "Processes"), 1, 10);
    }

    [Fact]
    public async Task Embedded_organization_employee_snapshot_deserializes()
    {
        var seeder = new OthersDBOrganizationEmployeeSeeder();
        var method = typeof(OthersDBOrganizationEmployeeSeeder).GetMethod("ReadAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(seeder, [CancellationToken.None]));
        await task;
        var data = task.GetType().GetProperty("Result")?.GetValue(task);
        Assert.NotNull(data);
        Assert.Equal(26, GetArrayLength(data!, "Departments"));
        Assert.Equal(106, GetArrayLength(data!, "Occupations"));
        Assert.Equal(2, GetArrayLength(data!, "Genders"));
        Assert.Equal(34, GetArrayLength(data!, "Nationalities"));
        Assert.Equal(1000, GetArrayLength(data!, "Employees"));
    }

    private static int GetArrayLength(object value, string propertyName)
    {
        var property = value.GetType().GetProperty(propertyName);
        return Assert.IsAssignableFrom<Array>(property?.GetValue(value)).Length;
    }
}
