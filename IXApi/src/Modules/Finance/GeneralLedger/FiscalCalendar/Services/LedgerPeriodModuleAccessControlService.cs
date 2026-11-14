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
        ValidateAccessLevels(entity);
        if (string.IsNullOrWhiteSpace(entity.DataAreaId))
            entity.DataAreaId = _currentUser.GetDataAreaId();
        await ValidatePeriodAsync(entity, ct);
        if (_unitOfWork.Context.ChangeTracker.Entries<LedgerPeriodModuleAccessControl>().Any(x =>
                x.State is EntityState.Added or EntityState.Modified && !ReferenceEquals(x.Entity, entity)
                && (string.IsNullOrWhiteSpace(x.Entity.DataAreaId) ? _currentUser.GetDataAreaId() : x.Entity.DataAreaId) == entity.DataAreaId
                && x.Entity.LedgerFiscalCalendarPeriod == entity.LedgerFiscalCalendarPeriod)
            || await _unitOfWork.Context.Set<LedgerPeriodModuleAccessControl>().AnyAsync(
                x => x.DataAreaId == entity.DataAreaId && x.LedgerFiscalCalendarPeriod == entity.LedgerFiscalCalendarPeriod, ct))
            throw new InvalidOperationException("Module access is already configured for this ledger period.");
    }

    public override async Task<IEnumerable<LedgerPeriodModuleAccessControl>> AddRangeAsync(
        IEnumerable<LedgerPeriodModuleAccessControl> entities, CancellationToken cancellationToken = default)
    {
        var rows = entities.ToList();
        foreach (var row in rows)
            if (string.IsNullOrWhiteSpace(row.DataAreaId)) row.DataAreaId = _currentUser.GetDataAreaId();
        if (rows.GroupBy(x => new { x.DataAreaId, x.LedgerFiscalCalendarPeriod }).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Module access is already configured for this ledger period.");
        return await base.AddRangeAsync(rows, cancellationToken);
    }

    public override async Task<LedgerPeriodModuleAccessControl> UpdateAsync(
        LedgerPeriodModuleAccessControl entity, CancellationToken cancellationToken = default)
    {
        ValidateAccessLevels(entity);
        await ValidatePeriodAsync(entity, cancellationToken);
        return await base.UpdateAsync(entity, cancellationToken);
    }

    public override async Task<IEnumerable<LedgerPeriodModuleAccessControl>> UpdateRangeAsync(
        IEnumerable<LedgerPeriodModuleAccessControl> entities, CancellationToken cancellationToken = default)
    {
        var rows = entities.ToList();
        foreach (var row in rows)
        {
            ValidateAccessLevels(row);
            await ValidatePeriodAsync(row, cancellationToken);
        }
        return await base.UpdateRangeAsync(rows, cancellationToken);
    }

    private async Task ValidatePeriodAsync(LedgerPeriodModuleAccessControl entity, CancellationToken ct)
    {
        if (!await _unitOfWork.Context.Set<LedgerFiscalCalendarPeriod>().AnyAsync(
                x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.LedgerFiscalCalendarPeriod, ct))
            throw new InvalidOperationException("The ledger fiscal period was not found.");
    }

    private static void ValidateAccessLevels(LedgerPeriodModuleAccessControl entity)
    {
        if (!Enum.IsDefined(entity.LedgerAccessLevel) || !Enum.IsDefined(entity.TaxAccessLevel)
            || !Enum.IsDefined(entity.BankAccessLevel) || !Enum.IsDefined(entity.CustAccessLevel)
            || !Enum.IsDefined(entity.VendAccessLevel) || !Enum.IsDefined(entity.InventAccessLevel))
            throw new InvalidOperationException("Module access levels must be All, None, or UserGroup.");
    }
}
