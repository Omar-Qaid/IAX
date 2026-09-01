using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Controls
{
public class WfControl : WfMasterEntity<byte>
    {
        [StringLength(255)]
        public string ControlType { get; set; } = null!;
        public byte SortOrder { get; set; }
    }
}

