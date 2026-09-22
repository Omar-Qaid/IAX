using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public interface IHcmShowroomService : IBaseService<HcmShowroom>
{
    Task<HcmShowroom> AddShowroomAsync(
        HcmShowroom showroom,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default);

    Task<HcmShowroom> UpdateShowroomAsync(
        HcmShowroom showroom,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default);
}
