using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Operators
{
    public class WfOperator : MasterEntity<byte>
    {
 
        public byte SortOrder { get; set; }
    }
}

