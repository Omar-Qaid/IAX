using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public interface IValidationEngine
    {
        Task<List<ValidationResult>> ValidateRequestAsync(long processId, long? requestId, List<WfRequestDetail> requestDetails);
    }
}