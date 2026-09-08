using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Categories
{
public class WfCategoryDto : WfMasterEntityDto<short>
    {
        public bool IsSystemDefined  { get; set; }
        public byte SortOrder { get; set; }
    }
}

