using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Shared.Application.Contracts;
using Xunit;

namespace IAX.IXApi.Tests;

public class WorkflowRequestFormIntegrationTests
{
    [Fact]
    public void Request_form_child_dtos_round_trip_entity_metadata()
    {
        Assert.IsAssignableFrom<EntityDto<long>>(new WfRequestControlsValidationDto());
        Assert.IsAssignableFrom<EntityDto<long>>(new WfRequestControlsOptionDto());
        Assert.IsAssignableFrom<EntityDto<long>>(new WfTransitionDto());
    }

    [Fact]
    public void Transition_validation_allows_empty_value_for_is_empty_operators()
    {
        var validator = new WfTransitionDtoValidator();
        var result = validator.Validate(new WfTransitionDto
        {
            ProcessId = 1,
            VariableId = 1,
            OperatorId = 1,
            StepId = 1,
            Value = string.Empty,
        });

        Assert.True(result.IsValid);
    }
}
