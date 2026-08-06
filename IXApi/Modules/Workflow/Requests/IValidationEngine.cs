using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public interface IValidationEngine
    {
        Task<List<ValidationResult>> ValidateRequestAsync(long processId, long? requestId, List<WfRequestDetail> requestDetails);
    }

    public class ValidationResult
    {
        public long RequestControlId { get; set; }
        public string ControlName { get; set; } = null!;
        public string ErrorMessageAr { get; set; } = null!;
        public string ErrorMessageEn { get; set; } = null!;
        public string Severity { get; set; } = null!; // Error, Warning, Information
    }
}
