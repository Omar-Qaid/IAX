using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    public class ManagementLevelDto : MasterEntityDto<byte>
    {
        public byte Level { get; set; }
    }
}

