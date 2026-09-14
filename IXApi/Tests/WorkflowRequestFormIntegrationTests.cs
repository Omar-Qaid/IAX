using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Shared.Application.Contracts;
using System.Xml.Linq;
using Xunit;

namespace IAX.IXApi.Tests;

public class WorkflowRequestFormIntegrationTests
{
    [Fact]
    public void Submission_uses_matching_route_before_the_default_step()
    {
        var steps = new[]
        {
            new WfStep { RecId = 10, SortOrder = 1 },
            new WfStep { RecId = 20, SortOrder = 2 }
        };

        var selected = WfRequestService.SelectStartingStepId(
            [new WfTransition { StepId = 20, SortOrder = 1 }], steps);

        Assert.Equal(20, selected);
    }

    [Fact]
    public void Submission_uses_first_positive_order_step_when_no_route_matches()
    {
        var steps = new[]
        {
            new WfStep { RecId = 30, SortOrder = 0 },
            new WfStep { RecId = 10, SortOrder = 1 },
            new WfStep { RecId = 20, SortOrder = 2 }
        };

        var selected = WfRequestService.SelectStartingStepId([], steps);

        Assert.Equal(10, selected);
    }

    [Fact]
    public void Submitted_request_details_preserve_the_legacy_readable_snapshot_shape()
    {
        var control = new DynamicRequestControlDto
        {
            RequestControlId = 20531,
            ControlId = 6,
            Label = "Violation Type",
            LabelAr = "نوع المخالفة",
            SortOrder = 3,
            UsedAsCriteria = true,
            Options =
            [
                new DynamicRequestOptionDto
                {
                    OptionId = 1, Value = "Open branch", Label = "Open branch",
                    LabelAlias = "فتح الفرع", Score = 5, SortOrder = 1
                },
                new DynamicRequestOptionDto
                {
                    OptionId = 2, Value = "Close branch", Label = "Close branch",
                    LabelAlias = "إغلاق الفرع", Score = 7, SortOrder = 2
                }
            ]
        };

        var serialized = WfRequestService.SerializeRequestDetails(590,
            [(control, "Close branch", 7m)]);
        var saved = XDocument.Parse(serialized).Root!.Element("Control")!;

        Assert.Equal("20531", saved.Element("ControlDataId")!.Value);
        Assert.Equal("Close branch", saved.Element("ControlValue")!.Value);
        Assert.Equal("إغلاق الفرع", saved.Element("ControlValueAR")!.Value);
        Assert.Equal("Close branch", saved.Element("ControlValueEN")!.Value);
        Assert.Equal("Close branch", saved.Element("DisplayMember")!.Value);
        Assert.Equal("Close branch", saved.Element("ValueMember")!.Value);
        Assert.Equal("590", saved.Element("RelatedObjectId")!.Value);
        Assert.Equal("7", saved.Element("Weight")!.Value);
        Assert.Equal(2, saved.Element("ExtendedProperties")!.Element("Data")!.Elements("Item").Count());
    }

    [Fact]
    public void Submitted_request_details_xml_escapes_user_input()
    {
        var control = new DynamicRequestControlDto
        {
            RequestControlId = 10, ControlId = 3, Label = "Notes & comments", SortOrder = 1
        };

        var serialized = WfRequestService.SerializeRequestDetails(603,
            [(control, "A < B & C", 0m)]);

        Assert.Equal("A < B & C",
            XDocument.Parse(serialized).Root!.Element("Control")!.Element("ControlValue")!.Value);
    }

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

    [Theory]
    [InlineData(0L, null)]
    [InlineData(null, 0L)]
    [InlineData(1L, 2L)]
    public void Transition_validation_rejects_invalid_or_ambiguous_triggers(long? activityId, long? requestControlId)
    {
        var result = new WfTransitionDtoValidator().Validate(new WfTransitionDto
        {
            ProcessId = 1, VariableId = 1, OperatorId = 1, StepId = 1,
            Value = string.Empty, ActivityId = activityId, RequestControlId = requestControlId,
        });
        Assert.False(result.IsValid);
    }
}
