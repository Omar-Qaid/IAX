using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfDataType : MasterEntity<byte>
    {
        public byte SortOrder { get; set; }
    }
}

