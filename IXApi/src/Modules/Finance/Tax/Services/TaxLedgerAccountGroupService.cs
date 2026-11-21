using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Tax;

public sealed class TaxLedgerAccountGroupService : BaseService<TaxLedgerAccountGroup>, ITaxLedgerAccountGroupService
{
    private readonly ICompanyExecutionContext _company;
    public TaxLedgerAccountGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICompanyExecutionContext company) : base(unitOfWork, currentUser) { _company = company; }
    protected override IQueryable<TaxLedgerAccountGroup> ApplyDomainFilters(IQueryable<TaxLedgerAccountGroup> query) =>
        query.Where(x => x.DataAreaId == _company.GetDataAreaId());
    public async Task<object> LedgerAccountsAsync(CancellationToken ct) =>
        await _unitOfWork.Context.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == _company.GetDataAreaId() && x.MainAccountValue != "")
            .OrderBy(x => x.MainAccountValue).ThenBy(x => x.RecId)
            .Select(x => new { id = x.RecId, code = x.MainAccountValue, name = x.DisplayValue })
            .ToListAsync(ct);
    protected override Task OnBeforeAddAsync(TaxLedgerAccountGroup entity, CancellationToken ct) => ValidateAsync(entity, ct);
    public override async Task<TaxLedgerAccountGroup> UpdateAsync(TaxLedgerAccountGroup entity, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(entity, cancellationToken);
        return await base.UpdateAsync(entity, cancellationToken);
    }
    private async Task ValidateAsync(TaxLedgerAccountGroup entity, CancellationToken ct)
    {
        Normalize(entity);
        var area = _company.GetDataAreaId();
        if (entity.DataAreaId != area) throw new ValidationException("Select the company that owns this ledger posting group.");
        if (await _unitOfWork.Context.Set<TaxLedgerAccountGroup>().AnyAsync(x => x.DataAreaId == area
            && x.TaxAccountGroup == entity.TaxAccountGroup && x.RecId != entity.RecId, ct))
            throw new ValidationException("The ledger posting group already exists in this company.");
        if (entity.RecId > 0)
        {
            var original = await _unitOfWork.Context.Set<TaxLedgerAccountGroup>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == entity.RecId && x.DataAreaId == area, ct);
            if (original != null && original.TaxAccountGroup != entity.TaxAccountGroup
                && await _unitOfWork.Context.Set<TaxTable>().AnyAsync(x => x.DataAreaId == area && x.TaxAccountGroup == original.TaxAccountGroup, ct))
                throw new ValidationException("A ledger posting group used by sales tax codes cannot be renamed.");
        }
        var ids = typeof(TaxLedgerAccountGroup).GetProperties()
            .Where(x => x.Name.Contains("LedgerDimension") && x.PropertyType == typeof(long?))
            .Select(x => (long?)x.GetValue(entity)).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Any(x => x <= 0) || await _unitOfWork.Context.Set<DimensionAttributeValueCombination>()
            .CountAsync(x => x.DataAreaId == area && ids.Contains(x.RecId) && x.MainAccountValue != "", ct) != ids.Count)
            throw new ValidationException("Select ledger accounts from the selected company's account lookup.");
    }
    public override async Task RemoveAsync(TaxLedgerAccountGroup entity, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Context.Set<TaxTable>().AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.TaxAccountGroup == entity.TaxAccountGroup, cancellationToken))
            throw new ValidationException("A ledger posting group used by sales tax codes cannot be deleted.");
        await base.RemoveAsync(entity, cancellationToken);
    }
    public void Normalize(TaxLedgerAccountGroup entity)
    {
        entity.TaxAccountGroup = entity.TaxAccountGroup?.Trim() ?? string.Empty;
        entity.Name = entity.Name?.Trim() ?? string.Empty;
        entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _company.GetDataAreaId() : entity.DataAreaId;
    }
}
