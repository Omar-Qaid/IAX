using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;

public interface ILedgerPeriodModuleAccessControlService : IBaseService<LedgerPeriodModuleAccessControl> { }

public sealed class LedgerPeriodModuleAccessControlService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : BaseService<LedgerPeriodModuleAccessControl>(unitOfWork, currentUser), ILedgerPeriodModuleAccessControlService
{
    protected override async Task OnBeforeAddAsync(LedgerPeriodModuleAccessControl entity, CancellationToken ct)
    {
        if (!await _unitOfWork.Context.Set<LedgerFiscalCalendarPeriod>().AnyAsync(
                x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.LedgerFiscalCalendarPeriod, ct))
            throw new InvalidOperationException("The ledger fiscal period was not found.");
        if (await _unitOfWork.Context.Set<LedgerPeriodModuleAccessControl>().AnyAsync(
                x => x.DataAreaId == entity.DataAreaId && x.LedgerFiscalCalendarPeriod == entity.LedgerFiscalCalendarPeriod, ct))
            throw new InvalidOperationException("Module access is already configured for this ledger period.");
    }
}
