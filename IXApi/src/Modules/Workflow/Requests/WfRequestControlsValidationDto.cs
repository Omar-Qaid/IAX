using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlsValidationDto : BaseEntityDto<long>
    {
        public long RequestControlId { get; set; }
        public string ValidationType { get; set; } = null!;
        public string? ValidationExpression { get; set; }
        public string? Operator { get; set; }
        public string? Value { get; set; }
        public string? MaskInput { get; set; }
        public string ErrorMessage { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
