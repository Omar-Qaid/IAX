using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Categories
{
public class WfCategoryDto : MasterEntityDto<short>
    {
        public bool IsSystemDefined  { get; set; }
        public byte SortOrder { get; set; }
    }
}

