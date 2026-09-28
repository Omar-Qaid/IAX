using System.Reflection;
using IAX.IXApi.Modules.Workflow.Requests;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class WorkflowControlPropertiesTests
{
    [Theory]
    [InlineData("null", null)]
    [InlineData("12", 12)]
    [InlineData("\"12\"", 12)]
    [InlineData("false", null)]
    [InlineData("{}", null)]
    [InlineData("[]", null)]
    public void Employee_filters_accept_empty_values_and_numeric_ids(string department, int? expected)
    {
        var properties = Parse("{\"labelAR\":\"Employee\",\"referenceFilter\":{\"departmentId\":" + department
            + ",\"occupationId\":null,\"managerLevel\":null}}");
        Assert.Equal("Employee", Property(properties, "LabelAr"));
        var filter = Property(properties, "ReferenceFilter")!;
        Assert.Equal(expected, Property(filter, "DepartmentId") is short id ? (int?)id : null);
        Assert.Null(Property(filter, "OccupationId"));
        Assert.Null(Property(filter, "ManagerLevel"));
    }

    [Fact]
    public void Option_ids_skip_non_numeric_values_and_preserve_valid_ids()
    {
        var properties = Parse("""
            {"optionFeatureConfigurations":[{"performerIds":[null,false,{},[],12,"13",12],"visibleControlIds":[null,"14"]}]}
            """);
        var options = Assert.IsType<List<DynamicRequestOptionFeatureDto>>(Property(properties, "OptionFeatureConfigurations"));
        var option = Assert.Single(options);
        Assert.Equal(new long[] { 12, 13 }, option.PerformerIds);
        Assert.Equal(new long[] { 14 }, option.VisibleControlIds);
    }

    private static object Parse(string json) => typeof(WfRequestService)
        .GetMethod("ParseProperties", BindingFlags.NonPublic | BindingFlags.Static)!
        .Invoke(null, [json])!;

    private static object? Property(object value, string name) => value.GetType().GetProperty(name)!.GetValue(value);
}
