using IAX.IXApi.Modules.Workflow.Execution;
using Xunit;

namespace IAX.IXApi.Tests;

public class WorkflowConditionEvaluatorTests
{
    [Theory]
    [InlineData("INT", "GT", "11", "10", true)]
    [InlineData("INT", "GTE", "10", "10", true)]
    [InlineData("INT", "LT", "11", "10", false)]
    [InlineData("STR", "EQ", "001", "1", false)]
    [InlineData("STR", "EQ", "Finance", "finance", true)]
    [InlineData("STR", "CONTAINS", "Finance team", "TEAM", true)]
    [InlineData("BOOL", "NEQ", "true", "false", true)]
    [InlineData("DT", "EQ", "2026-09-10T03:00:00+03:00", "2026-09-10T00:00:00Z", true)]
    [InlineData("DT", "GTE", "2026-09-10", "2026-09-10", true)]
    [InlineData("INT", "BETWEEN", "10", "[\"10\",\"20\"]", true)]
    [InlineData("INT", "BETWEEN", "21", "[\"10\",\"20\"]", false)]
    [InlineData("INT", "EQ", "", "0", false)]
    [InlineData("STR", "ISEMPTY", "", "", true)]
    public void Evaluates_using_declared_types(string type, string op, string actual, string expected, bool result)
        => Assert.Equal(result, WorkflowConditionEvaluator.Evaluate(type, op, actual, expected));

    [Theory]
    [InlineData("INT", "EQ", "1.5", "1")]
    [InlineData("BOOL", "EQ", "1", "true")]
    [InlineData("DT", "EQ", "2026-09-10T10:00:00", "2026-09-10")]
    [InlineData("STR", "GT", "10", "2")]
    [InlineData("OBJECT", "EQ", "1", "1")]
    [InlineData("INT", "UNKNOWN", "1", "1")]
    [InlineData("INT", "BETWEEN", "10", "[\"20\",\"10\"]")]
    [InlineData("INT", "EQ", "", "invalid")]
    public void Rejects_invalid_values_and_configuration(string type, string op, string actual, string expected)
        => Assert.ThrowsAny<Exception>(() => WorkflowConditionEvaluator.Evaluate(type, op, actual, expected));
}
