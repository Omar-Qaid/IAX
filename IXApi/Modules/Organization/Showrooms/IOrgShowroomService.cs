using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Organization.Showrooms
{
    public interface IOrgShowroomService : IBaseService<OrgShowroom>
    {
        /// <summary>Lists the sellers (employees) currently assigned to a showroom.</summary>
        Task<IEnumerable<ShowroomSellerDto>> GetSellersAsync(long showroomId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the full set of sellers for a showroom: employees in <paramref name="employeeIds"/> are
        /// assigned to it; employees previously assigned but not in the list are unassigned.
        /// </summary>
        Task<IEnumerable<ShowroomSellerDto>> SetSellersAsync(long showroomId, IEnumerable<long> employeeIds, CancellationToken cancellationToken = default);
    }
}

