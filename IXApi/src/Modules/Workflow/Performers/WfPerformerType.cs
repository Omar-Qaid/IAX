using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    [DataManagement]
    public class WfPerformerType : LookupEntity<short>
    {
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? NameAlias { get; set; }
        public byte SortOrder { get; set; }
    }
}

