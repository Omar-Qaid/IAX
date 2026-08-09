using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerType : LookupEntity<short>
    {
        public byte SortOrder { get; set; }
    }
}

