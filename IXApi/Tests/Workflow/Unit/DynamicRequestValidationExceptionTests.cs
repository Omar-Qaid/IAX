using IAX.IXApi.Modules.Workflow.Requests;
using Xunit;

namespace IAX.IXApi.Tests;

public class DynamicRequestValidationExceptionTests
{
    [Fact]
    public void MessageExposesWorkflowFailureAndPreservesStructuredErrors()
    {
        List<ValidationResult> errors = [new()
        {
            ControlName = "Workflow configuration",
            ErrorMessage = "No starting transition matches the submitted values.",
            Severity = "Error"
        }];
        var exception = new DynamicRequestValidationException(errors);
        Assert.Equal("Workflow configuration: No starting transition matches the submitted values.", exception.Message);
        Assert.Same(errors, exception.Errors);
    }

    [Fact]
    public void EmptyErrorsRetainFallbackMessage() => Assert.Equal(
        "The request contains invalid values.", new DynamicRequestValidationException([]).Message);
}
