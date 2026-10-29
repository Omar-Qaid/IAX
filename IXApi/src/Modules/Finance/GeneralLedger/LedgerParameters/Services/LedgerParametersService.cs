using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ParametersEntity = IAX.IXApi.Modules.Finance.Entities.LedgerParameters;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters;

/// <summary>
/// Reads and writes the company's general ledger parameters. The record is created with its
/// defaults on first access, as the parameters page does.
/// </summary>
public sealed class LedgerParametersService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : ILedgerParametersService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<(LedgerParametersDto? Result, string? Error)> GetAsync(CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        return (Map(await FindOrCreateAsync(area, ct)), null);
    }

    public async Task<(LedgerParametersDto? Result, string? Error)> UpdateAsync(LedgerParametersDto input, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        if (input == null) return (null, "Parameter values are required.");
        if (input.MaxRoundingDifferenceMst < 0m) return (null, "The rounding tolerance cannot be negative.");

        var parameters = await FindOrCreateAsync(area, ct);
        parameters.AbortErrorAccount = input.AbortErrorAccount;
        parameters.CheckContinuous = input.CheckContinuous;
        parameters.CheckVoucher = input.CheckVoucher;
        parameters.CheckJournalizing = input.CheckJournalizing;
        parameters.AllowMultipleTransactionsWithinOneVoucher = input.AllowMultipleTransactionsWithinOneVoucher;
        parameters.ReverseOppositeSignAmounts = input.ReverseOppositeSignAmounts;
        parameters.TransactionReversalCorrection = input.TransactionReversalCorrection;
        parameters.MaxRoundingDifferenceMst = input.MaxRoundingDifferenceMst;
        parameters.LimitedOpenFiscalPeriods = input.LimitedOpenFiscalPeriods;
        parameters.PeriodClosingPosting = input.PeriodClosingPosting;
        parameters.DeleteOpening = input.DeleteOpening;
        parameters.OpeningVoucher = input.OpeningVoucher;
        parameters.YearClosed = input.YearClosed;
        parameters.EnableLedgerSettlement = input.EnableLedgerSettlement;
        parameters.EnableLedgerSettlementPartialSettlement = input.EnableLedgerSettlementPartialSettlement;
        await unitOfWork.CompleteAsync(ct);
        return (Map(parameters), null);
    }

    private async Task<ParametersEntity> FindOrCreateAsync(string area, CancellationToken ct)
    {
        var parameters = await Db.Set<ParametersEntity>().FirstOrDefaultAsync(x => x.DataAreaId == area, ct);
        if (parameters != null) return parameters;
        parameters = new ParametersEntity { DataAreaId = area, Key_ = 0 };
        Db.Add(parameters);
        await unitOfWork.CompleteAsync(ct);
        return parameters;
    }

    private static LedgerParametersDto Map(ParametersEntity x) => new(
        x.AbortErrorAccount, x.CheckContinuous, x.CheckVoucher, x.CheckJournalizing,
        x.AllowMultipleTransactionsWithinOneVoucher, x.ReverseOppositeSignAmounts,
        x.TransactionReversalCorrection, x.MaxRoundingDifferenceMst, x.LimitedOpenFiscalPeriods,
        x.PeriodClosingPosting, x.DeleteOpening, x.OpeningVoucher, x.YearClosed,
        x.EnableLedgerSettlement, x.EnableLedgerSettlementPartialSettlement);

    /// <summary>
    /// Reads one company's parameters without creating them. Posting paths use this: a company
    /// that has never opened the parameters page must not have a record written during posting.
    /// </summary>
    internal static Task<ParametersEntity?> PeekAsync(DbContext db, string area, CancellationToken ct) =>
        db.Set<ParametersEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area, ct);
}
