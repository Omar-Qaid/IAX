using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace IAX.IXApi.Modules.Workflow.Categories
{
    [DataManagement]
public class WfCategory : WfMasterEntity<short>
    {
        public bool IsSystemDefined  { get; set; }
        public byte SortOrder { get; set; }
    }
}


