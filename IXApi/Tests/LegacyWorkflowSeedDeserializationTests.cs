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
        var processes = Assert.IsAssignableFrom<Array>(data!.GetType().GetProperty("Processes")?.GetValue(data));
        var process = processes.GetValue(0)!;
        Assert.Equal(true, process.GetType().GetProperty("IsRepeatable")?.GetValue(process));
        Assert.Equal((byte)0, process.GetType().GetProperty("RepeatIntervalHours")?.GetValue(process));
        Assert.Null(process.GetType().GetProperty("CanRepeat"));

        var activities = Assert.IsAssignableFrom<Array>(data.GetType().GetProperty("Activities")?.GetValue(data));
        Assert.Contains(activities.Cast<object>(), activity =>
            Equals(true, activity.GetType().GetProperty("IsAutoPassEnabled")?.GetValue(activity))
            && Equals((byte)1, activity.GetType().GetProperty("AutoPassAfterHours")?.GetValue(activity)));
        Assert.All(activities.Cast<object>(), activity =>
        {
            Assert.Equal(true, activity.GetType().GetProperty("IsSystemNotificationEnabled")?.GetValue(activity));
            Assert.Equal(true, activity.GetType().GetProperty("CanViewPreviousDocuments")?.GetValue(activity));
            Assert.Null(activity.GetType().GetProperty("AlertingBySystem"));
        });
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
