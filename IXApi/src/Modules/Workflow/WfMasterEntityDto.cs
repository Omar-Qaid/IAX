using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow;

public class WfMasterEntityDto<T> : MasterEntityDto<T>
{
    public string? NameAlias { get; set; }
}

