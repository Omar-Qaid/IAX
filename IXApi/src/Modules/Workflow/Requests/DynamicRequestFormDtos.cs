namespace IAX.IXApi.Modules.Workflow.Requests;

public sealed class DynamicRequestFormDto
{
    public long ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? ProcessDescription { get; set; }
    public List<DynamicRequestControlDto> Controls { get; set; } = [];
}

public sealed class DynamicRequestControlDto
{
    public long RequestControlId { get; set; }
    public byte ControlId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public string? LabelColor { get; set; }
    public string ControlType { get; set; } = string.Empty;
    public byte SortOrder { get; set; }
    public byte ColumnSpan { get; set; } = 1;
    public decimal Score { get; set; }
    public bool Required { get; set; }
    public bool ReadOnly { get; set; }
    public bool UniqueKey { get; set; }
    public bool UsedAsCriteria { get; set; }
    public string? DefaultValue { get; set; }
    public DynamicRequestConditionDto? VisibilityCondition { get; set; }
    public List<DynamicRequestOptionDto> Options { get; set; } = [];
    public List<DynamicRequestValidationDto> Validations { get; set; } = [];
}

public sealed class DynamicRequestOptionDto
{
    public long OptionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int SortOrder { get; set; }
    public DynamicRequestOptionFeatureDto FeatureConfiguration { get; set; } = new();
}

public sealed class DynamicRequestOptionFeatureDto
{
    public bool RequireFileUpload { get; set; }
    public bool SendAlertMessage { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
    public List<long> PerformerIds { get; set; } = [];
    public bool ShowOtherControls { get; set; }
    public List<long> VisibleControlIds { get; set; } = [];
}

public sealed class DynamicRequestValidationDto
{
    public long ValidationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Expression { get; set; }
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public string? Mask { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
    public int SortOrder { get; set; }
}

public sealed class DynamicRequestConditionDto
{
    public long SourceControlId { get; set; }
    public string Operator { get; set; } = "=";
    public string Value { get; set; } = string.Empty;
}

public sealed class SubmitDynamicRequestDto
{
    public long ProcessId { get; set; }
    public List<DynamicRequestValueDto> Values { get; set; } = [];
    public List<DynamicRequestOptionFeatureValueDto> OptionFeatureValues { get; set; } = [];
}

public sealed class DynamicRequestValueDto
{
    public long RequestControlId { get; set; }
    public string? Value { get; set; }
}

public sealed class DynamicRequestOptionFeatureValueDto
{
    public long OptionId { get; set; }
    public string? FileValue { get; set; }
}

public sealed class SubmitDynamicRequestResultDto
{
    public long RequestId { get; set; }
    public string? Code { get; set; }
    public decimal Score { get; set; }
    public List<DynamicRequestAttachmentOwnerDto> AttachmentOwners { get; set; } = [];
}

public sealed class DynamicRequestAttachmentOwnerDto
{
    public long RequestControlId { get; set; }
    public long? OptionId { get; set; }
    public long DetailRecId { get; set; }
}

public sealed class DynamicRequestValidationException(List<ValidationResult> errors)
    : Exception("The request contains invalid values.")
{
    public List<ValidationResult> Errors { get; } = errors;
}
