using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PostingProfile;

public sealed class CustLedgerAccountsService : BaseService<CustLedgerAccounts>, ICustLedgerAccountsService
{
    private readonly ICompanyExecutionContext _company;
    protected override IQueryable<CustLedgerAccounts> ApplyDomainFilters(IQueryable<CustLedgerAccounts> query) =>
        query.Where(x => x.DataAreaId == _company.GetDataAreaId());
    public CustLedgerAccountsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICompanyExecutionContext company) : base(unitOfWork, currentUser) { _company = company; }

    public async Task<object> LedgerAccountsAsync(CancellationToken ct) =>
        await _unitOfWork.Context.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == _company.GetDataAreaId() && x.MainAccountValue != "")
            .OrderBy(x => x.MainAccountValue).ThenBy(x => x.RecId)
            .Select(x => new { id = x.RecId, code = x.MainAccountValue, name = x.DisplayValue })
            .ToListAsync(ct);

    protected override Task OnBeforeAddAsync(CustLedgerAccounts entity, CancellationToken ct) => ValidateAsync(entity, ct);

    public override async Task<CustLedgerAccounts> UpdateAsync(CustLedgerAccounts entity, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(entity, cancellationToken);
        return await base.UpdateAsync(entity, cancellationToken);
    }

    private async Task ValidateAsync(CustLedgerAccounts entity, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area) || entity.DataAreaId != area)
            throw new ValidationException("Select the company that owns this posting rule.");
        var db = _unitOfWork.Context;
        if (!await db.Set<CustLedger>().AnyAsync(x => x.DataAreaId == area && x.PostingProfile == entity.PostingProfile, ct))
            throw new ValidationException("The customer posting profile was not found in the selected company.");
        entity.Num = entity.Num.Trim();
        if (!Enum.IsDefined(entity.AccountCode)) throw new ValidationException("Select Table, Group, or All for Account code.");
        if (entity.AccountCode == AccountCode.All) entity.Num = string.Empty;
        else if (entity.Num.Length == 0
            || (entity.AccountCode == AccountCode.Table && !await db.Set<CustTable>().AnyAsync(x => x.DataAreaId == area && x.AccountNum == entity.Num, ct))
            || (entity.AccountCode == AccountCode.Group && !await db.Set<CustGroup>().AnyAsync(x => x.DataAreaId == area && x.CustGroupId == entity.Num, ct)))
            throw new ValidationException("Select a customer or customer group in the selected company.");
        if (await db.Set<CustLedgerAccounts>().AnyAsync(x => x.DataAreaId == area && x.RecId != entity.RecId
            && x.PostingProfile == entity.PostingProfile && x.AccountCode == entity.AccountCode && x.Num == entity.Num, ct))
            throw new ValidationException("A rule already exists for this account scope in the posting profile.");
        var ids = new[] { entity.SummaryLedgerDimension, entity.ClearingLedgerDimension,
            entity.VatPrepaymentsLedgerDimension, entity.LiabilitiesForDiscountLedgerDimension,
            entity.DepositLedgerDimension, entity.EndorseLedgerDimension, entity.ExportSalesLedgerDimension, entity.WriteOffLedgerDimension };
        var requested = ids.Where(x => x > 0).Distinct().ToList();
        if (ids.Any(x => x < 0) || await db.Set<DimensionAttributeValueCombination>()
            .CountAsync(x => x.DataAreaId == area && requested.Contains(x.RecId) && x.MainAccountValue != "", ct) != requested.Count)
            throw new ValidationException("Select ledger accounts from the selected company's account lookup.");
    }
}
