using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace IAX.IXApi.Modules.Finance.Common;

/// <summary>Raised when subledger posting configuration or state prevents posting; mapped to a 422.</summary>
internal sealed class SubledgerPostingException(string message) : Exception(message);

internal sealed record SubledgerLedgerContext(Ledger Ledger, FiscalCalendarPeriod Period);

/// <summary>
/// Shared AR/AP subledger posting resolution (sales invoice, credit note, customer payment, vendor
/// invoice and vendor return): ledger period, customer/vendor/inventory/tax posting accounts, sales tax codes, voucher lines
/// and the concurrency lock. Accounts are always read from configuration (D365 posting profiles).
/// </summary>
internal sealed class SubledgerPostingSupport(DbContext db)
{
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

    public static GeneralJournalAccountEntry AccountEntry(string area, long entryId, DimensionAttributeValueCombination dimension,
        LedgerPostingType type, decimal amount, string text, decimal qty, string currency) => new()
    {
        DataAreaId = area, GeneralJournalEntry = entryId, LedgerDimension = dimension.RecId, MainAccount = dimension.MainAccount,
        LedgerAccount = dimension.DisplayValue.Length == 0 ? dimension.MainAccountValue : dimension.DisplayValue,
        Text = text, PostingType = type, TransactionCurrencyCode = currency, TransactionCurrencyAmount = amount,
        AccountingCurrencyAmount = amount, ReportingCurrencyAmount = amount, Quantity = qty,
        IsCredit = amount < 0 ? NoYes.Yes : NoYes.No, PaymentReference = string.Empty
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

    /// <summary>Inventory posting: item (Table) -> item group (Group) -> All.</summary>
    public async Task<DimensionAttributeValueCombination> ResolveInventPostingAsync(string area, string itemId,
        InventAccountType type, CancellationToken ct)
    {
        var itemGroup = await db.Set<InventItemGroupItem>().AsNoTracking()
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

    public async Task<DimensionAttributeValueCombination> LoadDimensionAsync(string area, long recId, string purpose, CancellationToken ct)
    {
        var dimension = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct)
            ?? throw new SubledgerPostingException($"The {purpose} account references an invalid ledger dimension.");
        var valid = dimension.MainAccount > 0
            ? await db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.RecId == dimension.MainAccount, ct)
            : await db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.MainAccountId == dimension.MainAccountValue, ct);
        if (!valid) throw new SubledgerPostingException($"Ledger dimension '{dimension.DisplayValue}' does not reference a valid main account.");
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
        parameter.Value = "SalesPosting:" + resource;
        command.Parameters.Add(parameter);
        if (await command.ExecuteScalarAsync(ct) is not int code || code < 0)
            throw new SubledgerPostingException("Sales posting could not acquire its concurrency lock.");
    }
}
