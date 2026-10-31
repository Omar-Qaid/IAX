using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

public sealed class LedgerJournalSetupCheck(DbContext db)
{
    public async Task<LedgerJournalSetupCheckDto?> CheckAsync(string area, long recId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(area)) return null;
        var header = await db.Set<Entities.LedgerJournalTable>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (header == null) return null;
        var errors = new List<string>();
        var unverified = new HashSet<string>(StringComparer.Ordinal)
        {
            "Account structures, dimension defaults and legal-entity account overrides.",
            "Workflow approval, posting permissions and module-level period access.",
            "Tax, reversal, posting layer and rounding rules."
        };
        var ledgers = await db.Set<Entities.Ledger>().AsNoTracking().Where(x => x.DataAreaId == area).ToListAsync(ct);
        if (ledgers.Count != 1)
            return new(new[] { "Exactly one ledger is required for the selected company." }, unverified.ToList());
        var ledger = ledgers[0];
        var names = await db.Set<LedgerJournalName>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.JournalName == header.JournalName).ToListAsync(ct);
        if (names.Count != 1) errors.Add("The journal name must resolve uniquely in the selected company.");
        else if (names[0].JournalType != header.JournalType) errors.Add("Journal type differs from the journal name setup.");
        if (string.IsNullOrWhiteSpace(ledger.AccountingCurrency)) errors.Add("Ledger accounting currency is required.");
        if (header.Posted == NoYes.Yes) errors.Add("The journal is already posted.");
        if (header.SystemBlocked == NoYes.Yes) errors.Add("The journal is system blocked.");
        var lines = await db.Set<LedgerJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.JournalNum == header.JournalNum).ToListAsync(ct);
        if (lines.Count == 0) errors.Add("The journal has no lines.");
        var periods = await db.Set<FiscalCalendarPeriod>().AsNoTracking()
            .Where(x => x.FiscalCalendar == ledger.FiscalCalendar && x.Type == FiscalPeriodType.Operating).ToListAsync(ct);
        var access = await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking()
            .Where(x => x.Ledger == ledger.RecId).ToListAsync(ct);
        foreach (var date in lines.Select(x => x.TransDate.Date).Distinct())
        {
            var matches = periods.Where(x => x.StartDate.Date <= date && x.EndDate.Date >= date).ToList();
            if (matches.Count != 1) errors.Add($"{date:yyyy-MM-dd}: no unique operating period in the ledger calendar.");
            else
            {
                var statuses = access.Where(x => x.FiscalCalendarPeriod == matches[0].RecId).ToList();
                if (statuses.Count != 1 || statuses[0].Status != FiscalPeriodStatus.Open)
                    errors.Add($"{date:yyyy-MM-dd}: the ledger period is not open or its status is ambiguous.");
            }
        }
        var ids = lines.SelectMany(x => new[] { x.LedgerDimension, x.OffsetLedgerDimension }).Where(x => x > 0).Distinct().ToList();
        var dimensions = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == area && ids.Contains(x.RecId)).ToListAsync(ct);
        var accountIds = dimensions.Select(x => x.MainAccount).Distinct().ToList();
        // Do not treat MainAccount as universally company-specific: its chart establishes ownership.
        var accounts = await db.Set<MainAccount>().AsNoTracking().Where(x => accountIds.Contains(x.RecId)).ToListAsync(ct);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.CurrencyCode)) errors.Add($"Line {line.LineNum}: currency is required.");
            if (line.CurrencyCode != ledger.AccountingCurrency ||
                (!string.IsNullOrWhiteSpace(ledger.ReportingCurrency) && ledger.ReportingCurrency != ledger.AccountingCurrency))
                unverified.Add("Effective exchange rates, conversion factors and accounting/reporting currency amounts.");
            if ((!string.IsNullOrEmpty(line.Company) && line.Company != area) ||
                (!string.IsNullOrEmpty(line.OffsetCompany) && line.OffsetCompany != area))
            {
                unverified.Add("Intercompany setup and destination-company accounting validation.");
                continue;
            }
            CheckAccount(line.AccountType, line.LedgerDimension, $"Line {line.LineNum}");
            if (line.OffsetLedgerDimension != 0) CheckAccount(line.OffsetAccountType, line.OffsetLedgerDimension, $"Line {line.LineNum} offset");
        }
        var rates = await new LedgerExchangeRateCheck(db).CheckAsync(area, ledger, lines, ct);
        errors.AddRange(rates.Errors);
        unverified.UnionWith(rates.Unverified);

        // Posting controls the company configured that this journal path does not apply. A
        // switched-on restriction is an error here, because posting refuses it rather than
        // ignoring it; the rest are reported so the result never implies they are in force.
        var parameters = await LedgerParameters.LedgerParametersService.PeekAsync(db, area, ct);
        if (LedgerJournalParameterControls.Refusal(parameters) is { } refusal) errors.Add(refusal);
        unverified.UnionWith(LedgerJournalParameterControls.Unenforced(parameters));

        return new(errors.Distinct().ToList(), unverified.OrderBy(x => x).ToList());

        void CheckAccount(LedgerJournalACType type, long id, string label)
        {
            if (type != LedgerJournalACType.Ledger)
            {
                unverified.Add("Customer/vendor/bank/asset dynamic account identity and subledger posting setup.");
                return;
            }
            var dimension = dimensions.SingleOrDefault(x => x.RecId == id);
            var account = accounts.SingleOrDefault(x => x.RecId == dimension?.MainAccount);
            if (dimension == null || account == null) errors.Add($"{label}: ledger combination or its main account is missing.");
            else if (ledger.ChartOfAccounts == 0 || account.LedgerChartOfAccounts != ledger.ChartOfAccounts)
                errors.Add($"{label}: main account does not belong to the ledger chart of accounts.");
        }
    }
}
