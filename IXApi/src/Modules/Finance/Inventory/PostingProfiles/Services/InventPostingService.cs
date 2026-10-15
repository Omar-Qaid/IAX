using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Mapster;
using InventPostingEntity = IAX.IXApi.Modules.Finance.Entities.InventPosting;
using LedgerDimensionEntity = IAX.IXApi.Modules.Finance.Entities.DimensionAttributeValueCombination;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventPostingService : BaseService<InventPostingEntity>, IInventPostingService
{
    private readonly ICompanyExecutionContext company;
    private DbContext Db => _unitOfWork.Context;

    public InventPostingService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICompanyExecutionContext company) : base(unitOfWork, currentUser) => this.company = company;
    public async Task<List<InventPostingRuleDto>?> ListAsync(int accountType, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var rows = await Db.Set<InventPostingEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId && (int)x.InventAccountType == accountType).OrderBy(x => x.ItemCode).ThenBy(x => x.ItemRelation).ToListAsync(ct);
        var ids = rows.Where(x => x.LedgerDimension != 0).Select(x => x.LedgerDimension).Distinct().ToList();
        var accounts = await Db.Set<LedgerDimensionEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId && ids.Contains(x.RecId)).ToDictionaryAsync(x => x.RecId, x => x.MainAccountValue, ct);
        return rows.Select(x => Map(x, accounts.GetValueOrDefault(x.LedgerDimension, string.Empty))).ToList();
    }

    public async Task<InventPostingLookupsDto?> LookupsAsync(CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return new InventPostingLookupsDto
        {
            Items = await Db.Set<Entities.InventTable>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.ItemId).Select(x => new InventPostingLookupDto(x.ItemId, x.ItemId + " - " + x.Name)).ToListAsync(ct),
            ItemGroups = await Db.Set<Entities.InventItemGroup>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.ItemGroupId).Select(x => new InventPostingLookupDto(x.ItemGroupId, x.ItemGroupId + " - " + x.Name)).ToListAsync(ct),
            Customers = await Db.Set<AccountsReceivable.CustTable>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.AccountNum).Select(x => new InventPostingLookupDto(x.AccountNum, x.AccountNum)).ToListAsync(ct),
            CustomerGroups = await Db.Set<AccountsReceivable.CustGroup>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.CustGroupId).Select(x => new InventPostingLookupDto(x.CustGroupId, x.CustGroupId + " - " + x.Name)).ToListAsync(ct),
            Vendors = await Db.Set<Entities.VendTable>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.AccountNum).Select(x => new InventPostingLookupDto(x.AccountNum, x.AccountNum)).ToListAsync(ct),
            VendorGroups = await Db.Set<Entities.VendGroup>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.VendGroupCode).Select(x => new InventPostingLookupDto(x.VendGroupCode, x.VendGroupCode + " - " + x.Name)).ToListAsync(ct),
            Categories = await Db.Set<Entities.EcoResCategory>().AsNoTracking().OrderBy(x => x.Name).Select(x => new InventPostingLookupDto(x.RecId.ToString(), x.Name)).ToListAsync(ct),
            TaxGroups = await Db.Set<Entities.TaxGroupHeading>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.TaxGroup).Select(x => new InventPostingLookupDto(x.TaxGroup, x.TaxGroup + " - " + x.TaxGroupName)).ToListAsync(ct),
            LedgerAccounts = await Db.Set<LedgerDimensionEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId && x.MainAccountValue != string.Empty).OrderBy(x => x.MainAccountValue).Select(x => new InventPostingLookupDto(x.RecId.ToString(), x.MainAccountValue)).ToListAsync(ct),
        };
    }

    public async Task<(InventPostingRuleDto? Rule, string? Error)> CreateAsync(InventPostingRuleDto input, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A legal entity must be selected.");
        var validation = await ValidateAsync(companyId, input, ct); if (validation.Error is not null) return (null, validation.Error);
        var row = new InventPostingEntity { DataAreaId = companyId }; Apply(row, input, validation.Category, validation.Ledger);
        Db.Add(row); await _unitOfWork.CompleteAsync(ct); return (Map(row, validation.MainAccount), null);
    }

    public async Task<(InventPostingRuleDto? Rule, string? Error)> UpdateAsync(long recId, InventPostingRuleDto input, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        var row = await Db.Set<InventPostingEntity>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (row is null) return (null, "Inventory posting rule was not found.");
        if ((int)row.InventAccountType != input.InventAccountType) return (null, "Posting type cannot be changed after creation.");
        var validation = await ValidateAsync(companyId!, input, ct); if (validation.Error is not null) return (null, validation.Error);
        Apply(row, input, validation.Category, validation.Ledger); await _unitOfWork.CompleteAsync(ct); return (Map(row, validation.MainAccount), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        var row = await Db.Set<InventPostingEntity>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (row is null) return (false, "Inventory posting rule was not found.");
        Db.Remove(row); await _unitOfWork.CompleteAsync(ct); return (true, null);
    }

    private async Task<(string? Error, long Category, long Ledger, string MainAccount)> ValidateAsync(string companyId, InventPostingRuleDto input, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(InventAccountType), input.InventAccountType)) return ("Posting type is invalid.", 0, 0, "");
        if (!Enum.IsDefined(typeof(TableGroupAll), input.ItemCode) || !Enum.IsDefined(typeof(TableGroupAll), input.CustVendCode)) return ("Code scope is invalid.", 0, 0, "");
        if ((TableGroupAll)input.ItemCode == TableGroupAll.All && input.ItemRelation.Trim().Length > 0) return ("Item relation must be blank when Item code is All.", 0, 0, "");
        if ((TableGroupAll)input.ItemCode != TableGroupAll.All && input.ItemRelation.Trim().Length == 0) return ("Item relation is required.", 0, 0, "");
        if ((TableGroupAll)input.CustVendCode == TableGroupAll.All && input.CustVendRelation.Trim().Length > 0) return ("Account relation must be blank when Account code is All.", 0, 0, "");
        if ((TableGroupAll)input.CustVendCode != TableGroupAll.All && input.CustVendRelation.Trim().Length == 0) return ("Account relation is required.", 0, 0, "");
        if (!long.TryParse(input.CategoryRelation, NumberStyles.None, CultureInfo.InvariantCulture, out var category) || category < 0) return ("Category relation is invalid.", 0, 0, "");
        if (category != 0 && !await Db.Set<Entities.EcoResCategory>().AnyAsync(x => x.RecId == category, ct)) return ("Category relation does not reference an EcoResCategory record.", 0, 0, "");
        var itemRelation = input.ItemRelation.Trim();
        if ((TableGroupAll)input.ItemCode == TableGroupAll.Table && !await Db.Set<Entities.InventTable>().AnyAsync(x => x.DataAreaId == companyId && x.ItemId == itemRelation, ct)) return ("Item relation is invalid for the selected legal entity.", 0, 0, "");
        if ((TableGroupAll)input.ItemCode == TableGroupAll.Group && !await Db.Set<Entities.InventItemGroup>().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == itemRelation, ct)) return ("Item group relation is invalid for the selected legal entity.", 0, 0, "");
        var accountRelationError = await ValidateAccountRelationAsync(companyId, input, ct);
        if (accountRelationError is not null) return (accountRelationError, 0, 0, "");
        if (!long.TryParse(input.LedgerDimension, NumberStyles.None, CultureInfo.InvariantCulture, out var ledger) || ledger <= 0) return ("Main account is required.", 0, 0, "");
        var account = await Db.Set<LedgerDimensionEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == ledger, ct);
        if (account is null) return ("Main account is invalid for the selected legal entity.", 0, 0, "");
        var mainAccountExists = account.MainAccount > 0
            ? await Db.Set<Entities.MainAccount>().AnyAsync(x => x.RecId == account.MainAccount, ct)
            : await Db.Set<Entities.MainAccount>().AnyAsync(x => x.MainAccountId == account.MainAccountValue, ct);
        if (!mainAccountExists) return ("The ledger dimension does not reference an existing MainAccount record.", 0, 0, "");
        if (input.TaxGroupId.Length > 0 && !await Db.Set<Entities.TaxGroupHeading>().AnyAsync(x => x.DataAreaId == companyId && x.TaxGroup == input.TaxGroupId, ct)) return ("Sales tax group is invalid.", 0, 0, "");
        return (null, category, ledger, account.MainAccountValue);
    }

    private async Task<string?> ValidateAccountRelationAsync(string companyId, InventPostingRuleDto input, CancellationToken ct)
    {
        var scope = (TableGroupAll)input.CustVendCode;
        if (scope == TableGroupAll.All) return null;

        var relation = input.CustVendRelation.Trim();
        var accountType = (InventAccountType)input.InventAccountType;
        if (SalesAccountTypes.Contains(accountType))
        {
            var exists = scope == TableGroupAll.Table
                ? await Db.Set<AccountsReceivable.CustTable>().AnyAsync(x => x.DataAreaId == companyId && x.AccountNum == relation, ct)
                : await Db.Set<AccountsReceivable.CustGroup>().AnyAsync(x => x.DataAreaId == companyId && x.CustGroupId == relation, ct);
            return exists ? null : "Customer relation is invalid for the selected legal entity.";
        }

        if (PurchaseAccountTypes.Contains(accountType))
        {
            var exists = scope == TableGroupAll.Table
                ? await Db.Set<Entities.VendTable>().AnyAsync(x => x.DataAreaId == companyId && x.AccountNum == relation, ct)
                : await Db.Set<Entities.VendGroup>().AnyAsync(x => x.DataAreaId == companyId && x.VendGroupCode == relation, ct);
            return exists ? null : "Vendor relation is invalid for the selected legal entity.";
        }

        return "Account code must be All for this posting type.";
    }

    private static readonly HashSet<InventAccountType> SalesAccountTypes =
    [
        InventAccountType.SalesPackingSlip,
        InventAccountType.SalesPackingSlipOffsetAccount,
        InventAccountType.SalesIssue,
        InventAccountType.SalesConsump,
        InventAccountType.SalesRevenue,
        InventAccountType.SalesCommission,
        InventAccountType.SalesDisc,
        InventAccountType.SalesCommissionOffsetAccount,
        InventAccountType.SalesPckSlpRevenue,
        InventAccountType.SalesPckSlpRevenueOffsetAccount,
        InventAccountType.SalesPackingSlipTax,
        InventAccountType.SalesAdvance,
    ];

    private static readonly HashSet<InventAccountType> PurchaseAccountTypes =
    [
        InventAccountType.PurchPackingSlip,
        InventAccountType.PurchPackingSlipOffsetAccount,
        InventAccountType.PurchReceipt,
        InventAccountType.PurchConsump,
        InventAccountType.PurchDisc,
        InventAccountType.PurchStdProfit,
        InventAccountType.PurchStdLoss,
        InventAccountType.PurchStdOffsetAccount,
        InventAccountType.PurchCharge,
        InventAccountType.PurchStockVariation,
        InventAccountType.PurchStdCostPurchasePriceVariance,
        InventAccountType.PurchReceiptFixedAsset,
        InventAccountType.PurchAdvance,
        InventAccountType.PurchExpense,
    ];

    private static void Apply(InventPostingEntity row, InventPostingRuleDto x, long category, long ledger)
    {
        row.InventAccountType = (InventAccountType)x.InventAccountType; row.ItemCode = (TableGroupAll)x.ItemCode;
        row.ItemRelation = x.ItemCode == (int)TableGroupAll.All ? string.Empty : x.ItemRelation.Trim(); row.CategoryRelation = category;
        row.CustVendCode = (TableGroupAll)x.CustVendCode; row.CustVendRelation = x.CustVendCode == (int)TableGroupAll.All ? string.Empty : x.CustVendRelation.Trim();
        row.TaxGroupId = x.TaxGroupId.Trim().ToUpperInvariant(); row.LedgerDimension = ledger;
    }
    private static InventPostingRuleDto Map(InventPostingEntity posting, string mainAccount) =>
        new InventPostingMappingSource(posting, mainAccount).Adapt<InventPostingRuleDto>();
}
