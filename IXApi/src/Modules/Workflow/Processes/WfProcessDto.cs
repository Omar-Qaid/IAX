using IAX.IXApi.Shared.Application.Contracts;
using System.Collections.Generic;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Priorities;

namespace IAX.IXApi.Modules.Workflow.Processes
{
public class WfProcessDto : WfMasterEntityDto<long>
    {
        public short CategoryId { get; set; }
        public WfCategoryDto? Category { get; set; }

        public decimal Score { get; set; }
        public bool CanRepeat { get; set; }
        public bool MandatoryDocs { get; set; }
        
        public byte PriorityId { get; set; }
        public WfPriorityDto? Priority { get; set; }

        public byte ProcessTypeId { get; set; }
        public bool SysField { get; set; }
        public byte SortOrder { get; set; }
        
        public List<WfUsersProcessDto> UsersProcesses { get; set; } = new List<WfUsersProcessDto>();
    }
}
