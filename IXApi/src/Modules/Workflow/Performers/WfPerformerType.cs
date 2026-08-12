using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    [DataManagement]
    public class WfPerformerType : LookupEntity<short>
    {
        public byte SortOrder { get; set; }
    }
}

