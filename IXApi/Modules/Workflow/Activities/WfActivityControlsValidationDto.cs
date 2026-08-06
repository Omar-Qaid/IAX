using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsValidationDto : EntityDto<long>
    {
        public long ActivityControlId { get; set; }
        public string ValidationType { get; set; } = null!;
        public string? ValidationExpression { get; set; }
        public string? Operator { get; set; }
        public string? Value { get; set; }
        public string? MaskInput { get; set; }
        public string ErrorMessageAr { get; set; } = null!;
        public string ErrorMessageEn { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
