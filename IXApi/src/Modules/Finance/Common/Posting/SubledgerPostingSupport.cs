using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Modules.Identity.Users;

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

internal enum LedgerPostingModule { Ledger, Tax, Bank, Customer, Vendor, Inventory }

/// <summary>
/// Shared AR/AP subledger posting resolution (sales invoice, credit note, customer payment, vendor
/// invoice and vendor return): ledger period, customer/vendor/inventory/tax posting accounts, sales tax codes, voucher lines
/// and the concurrency lock. Accounts are always read from configuration (D365 posting profiles).
/// </summary>
internal sealed class SubledgerPostingSupport(DbContext db)
{
    /// <summary>One application-lock namespace for every finance posting key, so unrelated keys never alias.</summary>
    private const string LockResourcePrefix = "FinancePosting:";

    public async Task<SubledgerLedgerContext> LoadLedgerContextAsync(string area, DateTime date,
        LedgerPostingModule module, string userId, CancellationToken ct)
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
        if (db.Model.FindEntityType(typeof(LedgerPeriodModuleAccessControl)) != null)
        {
            var access = await db.Set<LedgerPeriodModuleAccessControl>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.LedgerFiscalCalendarPeriod == ledgerPeriod.RecId, ct);
            if (access != null)
            {
                var rule = module switch
                {
                    LedgerPostingModule.Ledger => (access.LedgerAccessLevel, access.LedgerUserGroupInfo),
                    LedgerPostingModule.Tax => (access.TaxAccessLevel, access.TaxUserGroupInfo),
                    LedgerPostingModule.Bank => (access.BankAccessLevel, access.BankUserGroupInfo),
                    LedgerPostingModule.Customer => (access.CustAccessLevel, access.CustUserGroupInfo),
                    LedgerPostingModule.Vendor => (access.VendAccessLevel, access.VendUserGroupInfo),
                    LedgerPostingModule.Inventory => (access.InventAccessLevel, access.InventUserGroupInfo),
                    _ => throw new SubledgerPostingException($"Unsupported fiscal-period posting module '{module}'.")
                };
                if (rule.Item1 == LedgerPeriodAccessLevel.None)
                    throw new SubledgerPostingException($"The {module} module is closed for the fiscal period containing {date:d}.");
                if (rule.Item1 == LedgerPeriodAccessLevel.UserGroup &&
                    (string.IsNullOrWhiteSpace(rule.Item2) || !await db.Set<AspNetUserRole>().AsNoTracking()
                        .AnyAsync(x => x.UserId == userId && x.RoleId == rule.Item2, ct)))
                    throw new SubledgerPostingException($"User '{userId}' is not in the group allowed to post {module} transactions in this period.");
            }
        }
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
        string paymentReference = "") => AccountEntry(area, entryId, dimension, type, amount, amount, amount,
            text, qty, currency, paymentReference);

    public static GeneralJournalAccountEntry AccountEntry(string area, long entryId, DimensionAttributeValueCombination dimension,
        LedgerPostingType type, decimal transactionAmount, decimal accountingAmount, decimal reportingAmount,
        string text, decimal qty, string currency, string paymentReference = "") => new()
    {
        DataAreaId = area, GeneralJournalEntry = entryId, LedgerDimension = dimension.RecId, MainAccount = dimension.MainAccount,
        LedgerAccount = dimension.DisplayValue.Length == 0 ? dimension.MainAccountValue : dimension.DisplayValue,
        Text = text, PostingType = type, TransactionCurrencyCode = currency, TransactionCurrencyAmount = transactionAmount,
        AccountingCurrencyAmount = accountingAmount, ReportingCurrencyAmount = reportingAmount, Quantity = qty,
        IsCredit = accountingAmount < 0 ? NoYes.Yes : NoYes.No, PaymentReference = paymentReference
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

    /// <summary>Vendor posting profile: Table (vendor) -> Group (vendor group) -> All, within the profile.</summary>
    public async Task<DimensionAttributeValueCombination> ResolveVendorSummaryAsync(string area, VendTable vendor, CancellationToken ct)
    {
        // The company's default profile from VendParameters, the payable mirror of the customer
        // path. Without it every rule in the company is a candidate and two posting profiles mix.
        var profile = await db.Set<VendParameters>().AsNoTracking().Where(x => x.DataAreaId == area)
            .Select(x => x.VendPostingProfile).FirstOrDefaultAsync(ct) ?? string.Empty;
        var rules = await db.Set<VendLedgerAccounts>().AsNoTracking().Where(x => x.DataAreaId == area).ToListAsync(ct);
        if (!string.IsNullOrWhiteSpace(profile) && rules.Any(x => x.PostingProfile == profile))
            rules = rules.Where(x => x.PostingProfile == profile).ToList();
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

    /// <summary>
    /// Inventory posting setup. A rule is scoped on two axes: the item (Table -> Group -> All) and
    /// the customer or vendor (Table -> Group -> All). The most specific rule matching both axes
    /// wins, item axis first, which is how the setup page is organised.
    ///
    /// A caller that has no customer or vendor in context - an inventory journal or the inventory
    /// close - passes neither, and then only rules scoped to All customers/vendors can match.
    /// </summary>
    /// <param name="itemGroupId">The item's group when the caller already loaded it; resolved here otherwise.</param>
    /// <param name="custVendAccount">The vendor or customer account the document posts for, when there is one.</param>
    /// <param name="custVendGroup">That vendor's or customer's group, when the caller already loaded it.</param>
    public async Task<DimensionAttributeValueCombination> ResolveInventPostingAsync(string area, string itemId,
        InventAccountType type, CancellationToken ct, string? itemGroupId = null,
        string? custVendAccount = null, string? custVendGroup = null)
    {
        var itemGroup = itemGroupId ?? await db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == area && x.ItemId == itemId).Select(x => x.ItemGroupId).FirstOrDefaultAsync(ct) ?? string.Empty;
        var rules = await db.Set<InventPosting>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.InventAccountType == type).ToListAsync(ct);

        // Rank: Table beats Group beats All on each axis. -1 means the rule does not match at all.
        static int Rank(TableGroupAll code, string relation, string? specific, string? group) => code switch
        {
            TableGroupAll.Table => !string.IsNullOrEmpty(specific) && relation == specific ? 2 : -1,
            TableGroupAll.Group => !string.IsNullOrEmpty(group) && relation == group ? 1 : -1,
            _ => 0
        };

        var rule = rules
            .Select(x => new
            {
                Rule = x,
                ItemRank = Rank(x.ItemCode, x.ItemRelation, itemId, itemGroup),
                CustVendRank = Rank(x.CustVendCode, x.CustVendRelation, custVendAccount, custVendGroup)
            })
            .Where(x => x.ItemRank >= 0 && x.CustVendRank >= 0)
            .OrderByDescending(x => x.ItemRank).ThenByDescending(x => x.CustVendRank).ThenBy(x => x.Rule.RecId)
            .Select(x => x.Rule)
            .FirstOrDefault();
        if (rule == null || rule.LedgerDimension == 0)
            throw new SubledgerPostingException($"Inventory posting setup is missing for {type} and item '{itemId}'.");
        return await LoadDimensionAsync(area, rule.LedgerDimension, type.ToString(), ct);
    }

    /// <summary>
    /// The vendor scope for inventory posting resolution: the account and its vendor group, so a
    /// posting rule scoped to one vendor or to a vendor group can match.
    /// </summary>
    public async Task<(string Account, string Group)> VendorScopeAsync(string area, string vendAccount, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vendAccount)) return (string.Empty, string.Empty);
        var group = await db.Set<VendTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.AccountNum == vendAccount)
            .Select(x => x.VendGroup).FirstOrDefaultAsync(ct);
        return (vendAccount, group ?? string.Empty);
    }

    /// <summary>The customer scope for inventory posting resolution: the account and its customer group.</summary>
    public async Task<(string Account, string Group)> CustomerScopeAsync(string area, string custAccount, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(custAccount)) return (string.Empty, string.Empty);
        var group = await db.Set<CustTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.AccountNum == custAccount)
            .Select(x => x.CustGroupId).FirstOrDefaultAsync(ct);
        return (custAccount, group ?? string.Empty);
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

    /// <summary>
    /// Tax codes that apply when the customer tax group and item tax group share a code, at the rate
    /// valid on the date.
    /// <para>
    /// A code is dropped only when it has <b>no</b> <see cref="TaxData"/> row covering the date, which
    /// means it is not in force then. A code whose rate row says zero is <b>kept, at zero</b>: a
    /// zero-rated code is an ordinary configuration, and its taxable base has to be recorded so the
    /// turnover can be reported under that code. Callers must therefore expect a zero rate and must
    /// not treat it as "no tax applies" - see the zero-tax handling where each flow writes its
    /// <c>TaxTrans</c> rows.
    /// </para>
    /// </summary>
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
        return codes.Select(code => (Code: code, Row: rates.Where(x => x.TaxCode == code
                && (x.TaxFromDate == default || x.TaxFromDate.Date <= date)
                && (x.TaxToDate == default || x.TaxToDate.Date >= date))
            .OrderByDescending(x => x.TaxFromDate).FirstOrDefault()))
            .Where(x => x.Row != null)
            .Select(x => (x.Code, x.Row!.TaxValue)).ToList();
    }

    /// <summary>
    /// Splits a line's tax across its codes so the parts always add up to the rounded total.
    /// When every code is zero-rated the total is zero and each code takes zero, so a zero-rated
    /// line still yields one entry per code for its <c>TaxTrans</c> row.
    /// </summary>
    public static List<(string Code, decimal Tax)> AllocateTax(List<(string Code, decimal Rate)> codes, decimal lineTax)
    {
        var rate = codes.Sum(x => x.Rate);
        var result = new List<(string, decimal)>();
        // Nothing to apportion, and apportioning by a zero rate would divide by zero.
        if (rate == 0m) return codes.Select(x => (x.Code, 0m)).ToList();
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

    /// <summary>
    /// The realized exchange gain or loss account for a currency. D365 looks for a
    /// currency-specific `CurrencyLedgerGainLossAccount` row first and falls back to the ledger's
    /// own `LedgerGainLossAccount` row. A missing account is refused, never defaulted.
    /// </summary>
    public async Task<DimensionAttributeValueCombination> ResolveRealizedGainLossAsync(string area,
        long ledgerRecId, string currencyCode, bool isGain, CancellationToken ct)
    {
        var type = isGain ? CurrencyGainLossAccountType.RealizedGain : CurrencyGainLossAccountType.RealizedLoss;
        var dimension = await db.Set<CurrencyLedgerGainLossAccount>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.Ledger == ledgerRecId
                && x.CurrencyCode == currencyCode && x.AccountType == type && x.LedgerDimension != 0)
            .Select(x => x.LedgerDimension).FirstOrDefaultAsync(ct);
        if (dimension == 0)
            dimension = await db.Set<LedgerGainLossAccount>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.Ledger == ledgerRecId
                    && x.AccountType == type && x.LedgerDimension != 0)
                .Select(x => x.LedgerDimension).FirstOrDefaultAsync(ct);
        if (dimension == 0)
            throw new SubledgerPostingException(
                $"No realized exchange {(isGain ? "gain" : "loss")} account is configured for currency " +
                $"'{currencyCode}'. Configure it on the currency gain/loss accounts or on the ledger.");
        return await LoadDimensionAsync(area, dimension, $"realized exchange {(isGain ? "gain" : "loss")}", ct);
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
