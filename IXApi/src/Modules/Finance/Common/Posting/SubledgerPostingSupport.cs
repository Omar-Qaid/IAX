using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace IAX.IXApi.Modules.Finance.Common;

/// <summary>Raised when subledger posting configuration or state prevents posting; mapped to a 422.</summary>
internal sealed class SubledgerPostingException(string message) : Exception(message);

internal sealed record SubledgerLedgerContext(Ledger Ledger, FiscalCalendarPeriod Period);

/// <summary>Which side of the trade a physical (pre-invoice) document posts for.</summary>
internal enum PhysicalPostingSide
{
    /// <summary>Product receipt against a purchase order.</summary>
    Purchase,
    /// <summary>Packing slip against a sales order.</summary>
    Revenue
}

/// <summary>
/// Shared AR/AP subledger posting resolution (sales invoice, credit note, customer payment, vendor
/// invoice and vendor return): ledger period, customer/vendor/inventory/tax posting accounts, sales tax codes, voucher lines
/// and the concurrency lock. Accounts are always read from configuration (D365 posting profiles).
/// </summary>
internal sealed class SubledgerPostingSupport(DbContext db)
{
    /// <summary>One application-lock namespace for every finance posting key, so unrelated keys never alias.</summary>
    private const string LockResourcePrefix = "FinancePosting:";

    public async Task<SubledgerLedgerContext> LoadLedgerContextAsync(string area, DateTime date, CancellationToken ct)
    {
        var ledger = await db.Set<Ledger>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area, ct)
            ?? throw new SubledgerPostingException("A ledger is required for the selected company.");
        if (string.IsNullOrWhiteSpace(ledger.AccountingCurrency))
            throw new SubledgerPostingException("The ledger accounting currency is required.");
        var period = await db.Set<FiscalCalendarPeriod>().AsNoTracking().FirstOrDefaultAsync(
            x => x.FiscalCalendar == ledger.FiscalCalendar && x.StartDate <= date && x.EndDate >= date && x.Type == FiscalPeriodType.Operating, ct)
            ?? throw new SubledgerPostingException($"No operating fiscal period contains {date:d}.");
        var ledgerPeriod = await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().FirstOrDefaultAsync(
            x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId, ct);
        if (ledgerPeriod == null || ledgerPeriod.Status != FiscalPeriodStatus.Open)
            throw new SubledgerPostingException($"The fiscal period for {date:d} is not open.");
        return new SubledgerLedgerContext(ledger, period);
    }

    public GeneralJournalEntry NewEntry(string area, string journalNumber, string voucher, DateTime date, SubledgerLedgerContext context) => new()
    {
        DataAreaId = area, JournalNumber = journalNumber, Ledger = context.Ledger.RecId,
        JournalCategory = LedgerJournalCategory.General, PostingLayer = CurrentToOperationsPostingLayer.Current,
        SubledgerVoucher = voucher, SubledgerVoucherDataAreaId = area, DocumentNumber = journalNumber,
        AccountingDate = date, DocumentDate = date,
        FiscalCalendarPeriod = context.Period.RecId, FiscalCalendarYear = context.Period.FiscalCalendarYear
    };

    /// <param name="paymentReference">
    /// The source line's payment reference. Carried onto the posted entry because a main account may
    /// require one, and because the field exists on both sides and was being dropped.
    /// </param>
    public static GeneralJournalAccountEntry AccountEntry(string area, long entryId, DimensionAttributeValueCombination dimension,
        LedgerPostingType type, decimal amount, string text, decimal qty, string currency,
        string paymentReference = "") => new()
    {
        DataAreaId = area, GeneralJournalEntry = entryId, LedgerDimension = dimension.RecId, MainAccount = dimension.MainAccount,
        LedgerAccount = dimension.DisplayValue.Length == 0 ? dimension.MainAccountValue : dimension.DisplayValue,
        Text = text, PostingType = type, TransactionCurrencyCode = currency, TransactionCurrencyAmount = amount,
        AccountingCurrencyAmount = amount, ReportingCurrencyAmount = amount, Quantity = qty,
        IsCredit = amount < 0 ? NoYes.Yes : NoYes.No, PaymentReference = paymentReference
    };

    /// <summary>Customer posting profile: Table (customer) -> Group (customer group) -> All, within the profile.</summary>
    public async Task<DimensionAttributeValueCombination> ResolveCustomerSummaryAsync(string area, string? postingProfile,
        CustTable customer, CancellationToken ct)
    {
        var profile = postingProfile;
        if (string.IsNullOrWhiteSpace(profile))
            profile = await db.Set<CustParameters>().AsNoTracking().Where(x => x.DataAreaId == area)
                .Select(x => x.CustPostingProfile).FirstOrDefaultAsync(ct) ?? string.Empty;
        var rules = await db.Set<CustLedgerAccounts>().AsNoTracking().Where(x => x.DataAreaId == area).ToListAsync(ct);
        if (!string.IsNullOrWhiteSpace(profile) && rules.Any(x => x.PostingProfile == profile))
            rules = rules.Where(x => x.PostingProfile == profile).ToList();
        var rule = rules.Where(x => x.AccountCode == AccountCode.Table && x.Num == customer.AccountNum).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.AccountCode == AccountCode.Group && x.Num == customer.CustGroupId).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.AccountCode == AccountCode.All).OrderBy(x => x.RecId).FirstOrDefault();
        if (rule == null || rule.SummaryLedgerDimension == 0)
            throw new SubledgerPostingException($"Customer posting profile summary account is missing for customer '{customer.AccountNum}'.");
        return await LoadDimensionAsync(area, rule.SummaryLedgerDimension, "customer summary", ct);
    }

    /// <summary>Vendor posting profile: Table (vendor) -> Group (vendor group) -> All.</summary>
    public async Task<DimensionAttributeValueCombination> ResolveVendorSummaryAsync(string area, VendTable vendor, CancellationToken ct)
    {
        var rules = await db.Set<VendLedgerAccounts>().AsNoTracking().Where(x => x.DataAreaId == area).ToListAsync(ct);
        var rule = rules.Where(x => x.AccountCode == AccountCode.Table && x.Num == vendor.AccountNum).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.AccountCode == AccountCode.Group && x.Num == vendor.VendGroup).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.AccountCode == AccountCode.All).OrderBy(x => x.RecId).FirstOrDefault();
        if (rule == null || rule.SummaryLedgerDimension == 0)
            throw new SubledgerPostingException($"Vendor posting profile summary account is missing for vendor '{vendor.AccountNum}'.");
        return await LoadDimensionAsync(area, rule.SummaryLedgerDimension, "vendor summary", ct);
    }

    /// <summary>
    /// Item model group ledger policy for an item, or null when the item has no model group.
    /// The model group decides whether a document posts to the ledger at all (D365 policy fields).
    /// </summary>
    public async Task<InventModelGroup?> ResolveItemModelPolicyAsync(string area, string itemId, CancellationToken ct) =>
        await (from assignment in db.Set<InventModelGroupItem>()
               join policy in db.Set<InventModelGroup>()
                   on new { assignment.ModelGroupId, DataAreaId = assignment.ModelGroupDataAreaId }
                   equals new { policy.ModelGroupId, policy.DataAreaId }
               where assignment.ItemDataAreaId == area && assignment.ItemId == itemId
               select policy).SingleOrDefaultAsync(ct);

    /// <summary>
    /// Whether a physical document (packing slip, product receipt) posts to the ledger for this item:
    /// the model group must opt in to physical posting for the document's side, and physical on-hand
    /// posting must be enabled, because the physical entry is reversed against on-hand value.
    /// </summary>
    public async Task<bool> PostsPhysicalAsync(string area, string itemId, PhysicalPostingSide side, CancellationToken ct)
    {
        var model = await ResolveItemModelPolicyAsync(area, itemId, ct);
        var opted = side == PhysicalPostingSide.Purchase ? model?.PostPhysicalPurchase : model?.PostPhysicalRevenue;
        if (opted != NoYes.Yes) return false;
        if (model!.PostOnHandPhysical != NoYes.Yes)
            throw new SubledgerPostingException($"{side} physical posting requires physical inventory ledger posting on the item model group.");
        return true;
    }

    /// <summary>Inventory posting: item (Table) -> item group (Group) -> All.</summary>
    /// <param name="itemGroupId">The item's group when the caller already loaded it; resolved here otherwise.</param>
    public async Task<DimensionAttributeValueCombination> ResolveInventPostingAsync(string area, string itemId,
        InventAccountType type, CancellationToken ct, string? itemGroupId = null)
    {
        var itemGroup = itemGroupId ?? await db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == area && x.ItemId == itemId).Select(x => x.ItemGroupId).FirstOrDefaultAsync(ct) ?? string.Empty;
        var rules = await db.Set<InventPosting>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.InventAccountType == type && x.CustVendCode == TableGroupAll.All).ToListAsync(ct);
        var rule = rules.Where(x => x.ItemCode == TableGroupAll.Table && x.ItemRelation == itemId).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.ItemCode == TableGroupAll.Group && x.ItemRelation == itemGroup).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.ItemCode == TableGroupAll.All).OrderBy(x => x.RecId).FirstOrDefault();
        if (rule == null || rule.LedgerDimension == 0)
            throw new SubledgerPostingException($"Inventory posting setup is missing for {type} and item '{itemId}'.");
        return await LoadDimensionAsync(area, rule.LedgerDimension, type.ToString(), ct);
    }

    /// <summary>Sales tax payable account for a tax code: TaxTable.TaxAccountGroup -> TaxLedgerAccountGroup.</summary>
    public async Task<DimensionAttributeValueCombination> ResolveTaxPayableAsync(string area, string taxCode, CancellationToken ct)
    {
        var accountGroup = await db.Set<TaxTable>().AsNoTracking().Where(x => x.DataAreaId == area && x.TaxCode == taxCode)
            .Select(x => x.TaxAccountGroup).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(accountGroup))
            throw new SubledgerPostingException($"Sales tax code '{taxCode}' has no ledger posting group.");
        var group = await db.Set<TaxLedgerAccountGroup>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.TaxAccountGroup == accountGroup, ct);
        if (group?.TaxOutgoingLedgerDimension is not > 0)
            throw new SubledgerPostingException($"Sales tax payable account is missing for ledger posting group '{accountGroup}'.");
        return await LoadDimensionAsync(area, group.TaxOutgoingLedgerDimension.Value, "sales tax payable", ct);
    }

    /// <summary>Tax codes that apply when the customer tax group and item tax group share a code, at the rate valid on the date.</summary>
    public async Task<List<(string Code, decimal Rate)>> ApplicableTaxCodesAsync(string area, string taxGroup,
        string taxItemGroup, DateTime date, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(taxGroup) || string.IsNullOrWhiteSpace(taxItemGroup)) return [];
        var groupCodes = await db.Set<TaxGroupData>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.TaxGroup == taxGroup && x.ExemptTax != NoYes.Yes)
            .Select(x => x.TaxCode).ToListAsync(ct);
        var itemCodes = await db.Set<TaxOnItem>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.TaxItemGroup == taxItemGroup).Select(x => x.TaxCode).ToListAsync(ct);
        var codes = groupCodes.Intersect(itemCodes).Distinct().OrderBy(x => x).ToList();
        var rates = await db.Set<TaxData>().AsNoTracking()
            .Where(x => x.DataAreaId == area && codes.Contains(x.TaxCode)).ToListAsync(ct);
        return codes.Select(code => (code, rates.Where(x => x.TaxCode == code
                && (x.TaxFromDate == default || x.TaxFromDate.Date <= date)
                && (x.TaxToDate == default || x.TaxToDate.Date >= date))
            .OrderByDescending(x => x.TaxFromDate).FirstOrDefault()?.TaxValue ?? 0m))
            .Where(x => x.Item2 != 0m).ToList();
    }

    /// <summary>Splits a line's tax across its codes so the parts always add up to the rounded total.</summary>
    public static List<(string Code, decimal Tax)> AllocateTax(List<(string Code, decimal Rate)> codes, decimal lineTax)
    {
        var rate = codes.Sum(x => x.Rate);
        var result = new List<(string, decimal)>();
        var allocated = 0m;
        for (var i = 0; i < codes.Count; i++)
        {
            var part = i == codes.Count - 1 ? lineTax - allocated
                : decimal.Round(lineTax * codes[i].Rate / rate, 2, MidpointRounding.AwayFromZero);
            allocated += part;
            result.Add((codes[i].Code, part));
        }
        return result;
    }

    /// <summary>
    /// Validated once per posting operation, so a journal with many lines loads the ledger's
    /// account structures once rather than per line.
    /// </summary>
    private AccountStructureValidator? _structures;

    public async Task<DimensionAttributeValueCombination> LoadDimensionAsync(string area, long recId, string purpose, CancellationToken ct)
    {
        var dimension = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct)
            ?? throw new SubledgerPostingException($"The {purpose} account references an invalid ledger dimension.");
        var valid = dimension.MainAccount > 0
            ? await db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.RecId == dimension.MainAccount, ct)
            : await db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.MainAccountId == dimension.MainAccountValue, ct);
        if (!valid) throw new SubledgerPostingException($"Ledger dimension '{dimension.DisplayValue}' does not reference a valid main account.");

        // The account structures assigned to the ledger decide which dimension combinations may
        // post. A company with none assigned is not validated, and posts as it did before.
        _structures ??= new AccountStructureValidator(db);
        if (await _structures.ValidateAsync(area, dimension, ct) is { } refusal)
            throw new SubledgerPostingException($"The {purpose} account is not valid: {refusal}");
        return dimension;
    }

    public async Task LockAsync(string key, CancellationToken ct)
    {
        if (db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) != true) return;
        var resource = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = "DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000; SELECT @result;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.Value = LockResourcePrefix + resource;
        command.Parameters.Add(parameter);
        if (await command.ExecuteScalarAsync(ct) is not int code || code < 0)
            throw new SubledgerPostingException("Posting could not acquire its concurrency lock.");
    }
}
