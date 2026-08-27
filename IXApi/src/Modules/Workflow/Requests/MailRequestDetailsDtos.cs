namespace IAX.IXApi.Modules.Workflow.Requests;

public sealed class MailRequestDetailsDto
{
    public long RequestId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ProcessCode { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionTime { get; set; }
    public DateTime? TransactionEndTime { get; set; }
    public string? ResponsibleEmployee { get; set; }
    public List<MailRequestFieldDto> Fields { get; set; } = [];
    public List<MailTrackingEntryDto> History { get; set; } = [];
}

public sealed class MailRequestFieldDto
{
    public long DetailId { get; set; }
    public byte? ControlId { get; set; }
    public long? ControlDataId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string LabelAr { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueAr { get; set; } = string.Empty;
    public string ValueEn { get; set; } = string.Empty;
    public string ControlType { get; set; } = "text";
    public byte ControlOrder { get; set; }
}

public sealed class MailTrackingEntryDto
{
    public long AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool IsCompleted { get; set; }
}
