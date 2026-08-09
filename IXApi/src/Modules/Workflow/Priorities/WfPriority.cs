using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Workflow.Priorities
{
    [DataManagement]
    public class WfPriority : MasterEntity<byte>
    {
        public byte SortOrder { get; set; }
    }
}

