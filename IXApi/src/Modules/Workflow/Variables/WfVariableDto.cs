using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.Processes;
using System.Diagnostics;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfVariableDto : MasterEntityDto<long>
    {
        public byte DataTypeId { get; set; }
        public byte SortOrder { get; set; }
        public long ProcessId { get; set; }
        public WfDataTypeDto? DataType { get; set; }
        public WfProcessDto? Process { get; set; }

    }
}
