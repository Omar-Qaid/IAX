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
    public void Submitted_request_details_use_entity_field_names_for_details_and_options()
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
        Assert.Equal("إغلاق الفرع", saved.Element("ValueAlias")!.Value);
        Assert.Equal("Close branch", saved.Element("Value")!.Value);
        Assert.Equal("Violation Type", saved.Element("Name")!.Value);
        Assert.Equal("590", saved.Element("ProcessId")!.Value);
        Assert.Equal("7", saved.Element("Score")!.Value);
        var options = saved.Element("WfRequestControlsOptions")!.Elements("WfRequestControlsOption").ToList();
        Assert.Equal(2, options.Count);
        Assert.Equal("2", options[1].Element("RecId")!.Value);
        Assert.Equal("20531", options[1].Element("RequestControlId")!.Value);
        Assert.Equal("Close branch", options[1].Element("Name")!.Value);
        Assert.Equal("7", options[1].Element("Score")!.Value);
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

        var saved = XDocument.Parse(serialized).Root!.Element("Control")!;
        Assert.Equal("A < B & C", saved.Element("ControlValue")!.Value);
        Assert.Equal("A < B & C", saved.Element("ValueAlias")!.Value);
        Assert.Equal("A < B & C", saved.Element("Value")!.Value);
    }

    [Fact]
    public void Submitted_request_details_are_serialized_in_configured_control_order()
    {
        var second = new DynamicRequestControlDto
        {
            RequestControlId = 20, ControlId = 3, Label = "Second", SortOrder = 2
        };
        var first = new DynamicRequestControlDto
        {
            RequestControlId = 10, ControlId = 3, Label = "First", SortOrder = 1
        };

        var serialized = WfRequestService.SerializeRequestDetails(652,
            [(second, "B", 0m), (first, "A", 0m)]);
        var ids = XDocument.Parse(serialized).Root!.Elements("Control")
            .Select(control => control.Element("ControlDataId")!.Value)
            .ToList();

        Assert.Equal(["10", "20"], ids);
    }

    [Fact]
    public void Seeded_request_details_serialize_every_persisted_snapshot_field()
    {
        var serialized = WfRequestService.SerializeRequestDetails(590,
        [
            new WfRequestDetail
            {
                RecId = 90,
                ProcessId = 590,
                RequestId = 100,
                ControlId = 6,
                ControlDataId = 20531,
                Name = "Violation Type",
                NameAlias = "نوع المخالفة",
                ControlValue = "close",
                SortOrder = 3,
                ValueAlias = "إغلاق الفرع",
                Value = "Close branch",
                Score = 7,
                EarnedScore = 5,
            }
        ],
        [
            new WfRequestControlsOption
            {
                RecId = 700,
                RequestControlId = 20531,
                Value = "close",
                Name = "Close branch",
                NameAlias = "إغلاق الفرع",
                Score = 7,
                SortOrder = 2,
                ExtendedProperties = "{\"sendAlertMessage\":true}",
            }
        ]);

        var saved = XDocument.Parse(serialized).Root!.Element("Control")!;
        Assert.Equal("90", saved.Element("RecId")!.Value);
        Assert.Equal("20531", saved.Element("ControlDataId")!.Value);
        Assert.Equal("Violation Type", saved.Element("Name")!.Value);
        Assert.Equal("نوع المخالفة", saved.Element("NameAlias")!.Value);
        Assert.Equal("close", saved.Element("ControlValue")!.Value);
        Assert.Equal("6", saved.Element("ControlId")!.Value);
        Assert.Equal("590", saved.Element("ProcessId")!.Value);
        Assert.Equal("100", saved.Element("RequestId")!.Value);
        Assert.Equal("3", saved.Element("SortOrder")!.Value);
        Assert.Equal("إغلاق الفرع", saved.Element("ValueAlias")!.Value);
        Assert.Equal("Close branch", saved.Element("Value")!.Value);
        Assert.Equal("7", saved.Element("Score")!.Value);
        Assert.Equal("5", saved.Element("EarnedScore")!.Value);
        var option = saved.Element("WfRequestControlsOptions")!.Element("WfRequestControlsOption")!;
        Assert.Equal("700", option.Element("RecId")!.Value);
        Assert.Equal("{\"sendAlertMessage\":true}", option.Element("ExtendedProperties")!.Value);
    }

    [Fact]
    public void Imported_request_snapshot_populates_seeded_detail_values_and_scores()
    {
        const string snapshot = """
            <Details><Control><ControlDataId>20531</ControlDataId><ControlValueAR>إغلاق الفرع</ControlValueAR><ControlValueEN>Close branch</ControlValueEN><Weight>7.5</Weight><TargetWeight>5.25</TargetWeight></Control></Details>
            """;
        var detail = new WfRequestDetail
        {
            ControlDataId = 20531,
            ControlValue = "close",
        };

        WfRequestService.ApplyRequestDetailSnapshotValues(snapshot, [detail]);

        Assert.Equal("إغلاق الفرع", detail.ValueAlias);
        Assert.Equal("Close branch", detail.Value);
        Assert.Equal(7.5m, detail.Score);
        Assert.Equal(5.25m, detail.EarnedScore);
    }

    [Fact]
    public void Request_form_child_dtos_round_trip_entity_metadata()
    {
        Assert.IsAssignableFrom<EntityDto<long>>(new WfRequestControlsValidationDto());
        Assert.IsAssignableFrom<EntityDto<long>>(new WfRequestControlsOptionDto());
        Assert.IsAssignableFrom<EntityDto<long>>(new WfTransitionDto());
    }

    [Fact]
    public void Request_control_option_validation_requires_its_parent_value_and_name()
    {
        var result = new WfRequestControlsOptionDtoValidator().Validate(
            new WfRequestControlsOptionDto());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(WfRequestControlsOptionDto.RequestControlId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(WfRequestControlsOptionDto.Value));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(WfRequestControlsOptionDto.Name));
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
