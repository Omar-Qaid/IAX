using IAX.IXApi.Modules.Workflow.Execution;
using Xunit;

namespace IAX.IXApi.Tests;

public class WorkflowVariableBindingsTests
{
    [Fact]
    public void Maps_visible_normalized_values_and_keeps_hidden_and_unmapped_variables_empty()
    {
        var result = WorkflowVariableBindings.Bind([1, 2, 3, 4],
            new Dictionary<long, string> { [10] = "001", [20] = "hidden client value" },
            new HashSet<long> { 10 }, [(10, 1), (20, 2), (10, 4)]);
        Assert.Equal("001", result[1]);
        Assert.Equal(string.Empty, result[2]);
        Assert.Equal(string.Empty, result[3]);
        Assert.Equal("001", result[4]);
    }

    [Theory]
    [InlineData(99, 1)]
    [InlineData(10, 99)]
    public void Rejects_foreign_or_unavailable_references(long control, long variable)
        => Assert.Throws<InvalidOperationException>(() => WorkflowVariableBindings.Bind([1],
            new Dictionary<long, string> { [10] = "value" }, new HashSet<long> { 10 }, [(control, variable)]));

    [Fact]
    public void Rejects_conflicting_mappings_even_when_one_source_is_hidden()
        => Assert.Throws<InvalidOperationException>(() => WorkflowVariableBindings.Bind([1],
            new Dictionary<long, string> { [10] = "first", [20] = "second" }, new HashSet<long> { 10 }, [(10, 1), (20, 1)]));
}
