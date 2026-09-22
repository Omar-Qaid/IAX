using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroomDto : MasterEntityDto<long>
{
    public long Party { get; set; }
}
