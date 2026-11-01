using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

/// <summary>
/// Settle and post sales tax for one settlement period interval.
///
/// Tax posts to the input tax account (incoming, a receivable) and the output tax account
/// (outgoing, a payable) as documents are invoiced, and those accounts only ever grow until they
/// are settled. Settlement clears each tax account's balance for the interval and moves the net to
/// the sales tax settlement account (TaxLedgerAccountGroup.TaxReportLedgerDimension), which is
/// what is then owed to or recoverable from the authority.
///
/// Signs: TaxTrans.TaxAmount carries the same sign as the ledger entry that posted it - incoming
/// tax is positive (debit) and outgoing tax is negative (credit). Clearing therefore posts the
/// negation of each account's balance and the settlement account takes the total, so the voucher
/// balances to zero by construction rather than by a computed offset.
///
/// Scope: this is the "Original" settlement of an interval, which D365 allows once per interval.
/// Corrective runs over an already settled interval are rejected rather than approximated.
/// </summary>
public sealed class TaxSettlementService : ITaxSettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private DbContext Db => _unitOfWork.Context;
    private SubledgerPostingSupport Support => new(Db);

    public TaxSettlementService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
    }

    public Task<(TaxSettlementResultDto? Result, string? Error)> PreviewAsync(
        TaxSettlementInputDto input, CancellationToken ct = default) => RunAsync(input, post: false, ct);

    public Task<(TaxSettlementResultDto? Result, string? Error)> SettleAndPostAsync(
        TaxSettlementInputDto input, CancellationToken ct = default) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => RunAsync(input, post: true, ct));

    private async Task<(TaxSettlementResultDto? Result, string? Error)> RunAsync(
        TaxSettlementInputDto input, bool post, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var periodId = input.TaxPeriod?.Trim() ?? string.Empty;
        if (periodId.Length == 0) return (null, "A settlement period is required.");

        var head = await Db.Set<TaxPeriodHead>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.TaxPeriod == periodId, ct);
        if (head == null) return (null, $"Settlement period '{periodId}' was not found.");

        var fromDate = input.FromDate.ToDateTime(TimeOnly.MinValue);
        var interval = await Db.Set<TaxReportPeriod>()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.TaxPeriod == periodId && x.FromDate == fromDate, ct);
        if (interval == null)
            return (null, $"Settlement period '{periodId}' has no interval starting on {input.FromDate:d}.");
        if (interval.Closed == NoYes.Yes)
            return (null, $"The interval {interval.FromDate:d} to {interval.ToDate:d} is closed.");
        if (interval.VersionNum > 0)
            return (null, $"The interval {interval.FromDate:d} to {interval.ToDate:d} is already settled. " +
                          "Corrective settlement of a settled interval is outside the basic scope.");

        var transDate = input.TransDate.ToDateTime(TimeOnly.MinValue);
        if (transDate < interval.ToDate)
            return (null, $"The settlement date must not precede the end of the interval ({interval.ToDate:d}).");

        // Unsettled tax in the interval. A settled row carries its settlement period, so an
        // interval never picks up tax that an earlier run already cleared.
        var exclusiveEnd = interval.ToDate.Date.AddDays(1);
        var transactions = await Db.Set<TaxTrans>()
            .Where(x => x.DataAreaId == companyId && x.TaxPeriod == string.Empty
                && x.TransDate >= interval.FromDate.Date && x.TransDate < exclusiveEnd)
            .ToListAsync(ct);
        if (transactions.Count == 0)
            return (null, $"No unsettled sales tax was found between {interval.FromDate:d} and {interval.ToDate:d}.");

        // Each tax code's account group decides which account its tax sits on, so clearing is
        // grouped by the resolved account rather than by the code.
        var accounts = new Dictionary<long, (DimensionAttributeValueCombination Account, TaxDirection Direction, decimal Balance)>();
        DimensionAttributeValueCombination? settlementAccount = null;
        try
        {
            foreach (var group in transactions.GroupBy(x => (x.TaxCode, x.TaxDirection)))
            {
                var (account, report) = await ResolveAsync(companyId, group.Key.TaxCode, group.Key.TaxDirection, ct);
                settlementAccount ??= report;
                if (report.RecId != settlementAccount.RecId)
                    return (null, "The tax codes in this interval point at different sales tax settlement accounts, " +
                                  "so the interval cannot be settled as one voucher.");
                var balance = group.Sum(x => x.TaxAmount);
                accounts[account.RecId] = accounts.TryGetValue(account.RecId, out var existing)
                    ? (account, existing.Direction, existing.Balance + balance)
                    : (account, group.Key.TaxDirection, balance);
            }
        }
        catch (SubledgerPostingException ex) { return (null, ex.Message); }

        var settlementAmount = accounts.Values.Sum(x => x.Balance);
        var outgoing = accounts.Values.Where(x => x.Direction == TaxDirection.Outgoing).Sum(x => x.Balance);
        var incoming = accounts.Values.Where(x => x.Direction == TaxDirection.Incoming).Sum(x => x.Balance);
        var offsetsPosted = head.NotGenerateOffsetTaxTrans != NoYes.Yes;
        // Captured before the interval is incremented: the result is built lazily, so reading
        // interval.VersionNum inside it would report the value after the increment.
        var version = interval.VersionNum + 1;

        var result = (string voucher) => new TaxSettlementResultDto(
            periodId, DateOnly.FromDateTime(interval.FromDate), DateOnly.FromDateTime(interval.ToDate),
            version, voucher, input.TransDate, transactions.Count,
            outgoing, incoming, settlementAmount, offsetsPosted,
            settlementAccount!.DisplayValue.Length == 0 ? settlementAccount.MainAccountValue : settlementAccount.DisplayValue,
            accounts.Values
                .Select(x => new TaxSettlementAccountDto(
                    x.Account.DisplayValue.Length == 0 ? x.Account.MainAccountValue : x.Account.DisplayValue,
                    x.Direction.ToString(), x.Balance))
                .OrderBy(x => x.LedgerAccount, StringComparer.Ordinal).ToList());

        if (!post) return (result(string.Empty), null);

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var context = await Support.LoadLedgerContextAsync(companyId, transDate,
                LedgerPostingModule.Tax, _user.GetCurrentUserId(), ct);
            if (!string.IsNullOrWhiteSpace(context.Ledger.ReportingCurrency) &&
                !string.Equals(context.Ledger.ReportingCurrency, context.Ledger.AccountingCurrency, StringComparison.OrdinalIgnoreCase))
                throw new SubledgerPostingException(
                    "Sales tax settlement with a different reporting currency requires exchange-rate processing, which is not supported.");

            var voucher = (await _numbers.NextAsync("TaxSettlement", companyId, ct)).Code;
            var currency = context.Ledger.AccountingCurrency;
            var entry = Support.NewEntry(companyId, voucher, voucher, transDate, context);
            Db.Add(entry);
            await _unitOfWork.CompleteAsync(ct); // Persists the voucher key inside this transaction.

            foreach (var (_, scope) in accounts)
            {
                if (scope.Balance == 0m) continue;
                Db.Add(SubledgerPostingSupport.AccountEntry(companyId, entry.RecId, scope.Account,
                    LedgerPostingType.Tax, -scope.Balance,
                    $"Sales tax settlement {periodId} {interval.FromDate:d}-{interval.ToDate:d}", 0m, currency));
            }
            Db.Add(SubledgerPostingSupport.AccountEntry(companyId, entry.RecId, settlementAccount!,
                LedgerPostingType.Tax, settlementAmount,
                $"Sales tax settlement {periodId} {interval.FromDate:d}-{interval.ToDate:d}", 0m, currency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = companyId, GeneralJournalEntry = entry.RecId, Voucher = voucher,
                VoucherDataAreaId = companyId, AccountingDate = transDate
            });

            foreach (var trans in transactions)
            {
                // The offset transaction is what the sales tax / ledger reconciliation reads, so
                // the settled tax nets to zero in the tax inquiry as well as in the ledger.
                if (offsetsPosted) Db.Add(Offset(trans, voucher, transDate, periodId, version));
                trans.TaxPeriod = periodId;
                trans.TaxRepCounter = version;
            }

            interval.VersionNum = version;
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (result(voucher), null);
        }
        catch (SubledgerPostingException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return (null, ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// The tax account the code posts to, and the settlement account it clears into. Incoming tax
    /// uses the input tax account and outgoing tax the payable account, which is the same pair the
    /// purchase and sales postings used when the tax was created.
    /// </summary>
    private async Task<(DimensionAttributeValueCombination Account, DimensionAttributeValueCombination Report)> ResolveAsync(
        string area, string taxCode, TaxDirection direction, CancellationToken ct)
    {
        var accountGroup = await Db.Set<TaxTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.TaxCode == taxCode)
            .Select(x => x.TaxAccountGroup).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(accountGroup))
            throw new SubledgerPostingException($"Sales tax code '{taxCode}' has no ledger posting group.");
        var group = await Db.Set<TaxLedgerAccountGroup>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.TaxAccountGroup == accountGroup, ct);
        if (group == null)
            throw new SubledgerPostingException($"Ledger posting group '{accountGroup}' was not found.");

        var dimension = direction == TaxDirection.Outgoing
            ? group.TaxOutgoingLedgerDimension
            : group.TaxIncomingLedgerDimension;
        if (dimension is not > 0)
            throw new SubledgerPostingException(
                $"The {(direction == TaxDirection.Outgoing ? "sales tax payable" : "purchase input tax")} account is missing for ledger posting group '{accountGroup}'.");
        if (group.TaxReportLedgerDimension is not > 0)
            throw new SubledgerPostingException(
                $"The sales tax settlement account is missing for ledger posting group '{accountGroup}'.");

        return (await Support.LoadDimensionAsync(area, dimension.Value, "tax", ct),
                await Support.LoadDimensionAsync(area, group.TaxReportLedgerDimension.Value, "sales tax settlement", ct));
    }

    /// <summary>The offsetting tax transaction for one settled transaction: the same row negated.</summary>
    private static TaxTrans Offset(TaxTrans source, string voucher, DateTime transDate, string periodId, int version) => new()
    {
        DataAreaId = source.DataAreaId, TransDate = transDate, Voucher = voucher,
        TaxCode = source.TaxCode, TaxGroup = source.TaxGroup, TaxItemGroup = source.TaxItemGroup,
        TaxDirection = source.TaxDirection, TaxAccountType = source.TaxAccountType,
        SourceCurrencyCode = source.SourceCurrencyCode, CurrencyCode = source.CurrencyCode,
        TaxValue = source.TaxValue,
        SourceBaseAmountCur = -source.SourceBaseAmountCur, SourceTaxAmountCur = -source.SourceTaxAmountCur,
        TaxBaseAmountCur = -source.TaxBaseAmountCur, TaxAmountCur = -source.TaxAmountCur,
        TaxBaseAmount = -source.TaxBaseAmount, TaxAmount = -source.TaxAmount,
        TaxBaseAmountRep = -source.TaxBaseAmountRep, TaxAmountRep = -source.TaxAmountRep,
        TaxOrigin = source.TaxOrigin, TaxPeriod = periodId, TaxRepCounter = version,
        OrigTaxTransRecId = source.RecId, TaxAutoGenerated = NoYes.Yes
    };
}
