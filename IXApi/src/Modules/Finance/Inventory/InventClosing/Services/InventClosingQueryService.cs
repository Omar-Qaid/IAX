using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventClosingQueryService
{
    Task<IReadOnlyList<InventClosingRecordDto>> ListAsync(CancellationToken ct);
    Task<InventClosingRecordDto?> GetAsync(long recId, CancellationToken ct);
}

public sealed class InventClosingQueryService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    : IInventClosingQueryService
{
    public async Task<IReadOnlyList<InventClosingRecordDto>> ListAsync(CancellationToken ct)
    {
        var area = RequireCompany();
        var rows = await unitOfWork.Context.Set<InventClosing>().AsNoTracking()
            .Where(x => x.DataAreaId == area)
            .OrderByDescending(x => x.TransDate).ThenByDescending(x => x.RunNum).ThenByDescending(x => x.RecId)
            .ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<InventClosingRecordDto?> GetAsync(long recId, CancellationToken ct)
    {
        var area = RequireCompany();
        var row = await unitOfWork.Context.Set<InventClosing>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        return row is null ? null : Map(row);
    }

    private static InventClosingRecordDto Map(InventClosing row) => new()
    {
        RecId = row.RecId, TransDate = row.TransDate, Executed = row.Executed,
        Voucher = row.Voucher, PeriodCode = row.PeriodCode, RunNum = row.RunNum,
        AdjustmentType = row.AdjustmentType, InventCostStatus = row.InventCostStatus,
        Cancellation = row.Cancellation == NoYes.Yes,
        CancelClosing = row.CancelClosing == NoYes.Yes,
        RunRecalculation = row.RunRecalculation == NoYes.Yes, Notes = row.Notes
    };

    private string RequireCompany() => company.GetDataAreaId()
        ?? throw new InvalidOperationException("A company must be selected.");
}
