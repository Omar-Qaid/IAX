using IAX.IXApi.Modules.Workflow.Requests;
using Xunit;

namespace IAX.IXApi.Tests;

public class DynamicRequestValuesTests
{
    [Fact]
    public void Omitted_editable_defaults_are_available_to_all_submission_consumers()
    {
        var values = DynamicRequestValues.Normalize([
            new() { RequestControlId = 1, DefaultValue = "Finance", UniqueKey = true },
            new() { RequestControlId = 2 },
        ], []);
        Assert.Equal("Finance", values[1]);
        Assert.Equal(string.Empty, values[2]);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("  Legal  ", "Legal")]
    public void Explicit_editable_answers_override_defaults(string? answer, string expected)
    {
        var values = DynamicRequestValues.Normalize([
            new() { RequestControlId = 1, DefaultValue = "Finance" },
        ], [new() { RequestControlId = 1, Value = answer }]);
        Assert.Equal(expected, values[1]);
    }

    [Fact]
    public void Read_only_configuration_overrides_client_values_without_changing_default_format()
    {
        var values = DynamicRequestValues.Normalize([
            new() { RequestControlId = 1, ReadOnly = true, DefaultValue = " Finance " },
        ], [new() { RequestControlId = 1, Value = "Legal" }]);
        Assert.Equal(" Finance ", values[1]);
    }
}
