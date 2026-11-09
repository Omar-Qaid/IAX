using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record PurchPrepaymentDefinitionDto(decimal Amount, long CategoryRecId, string Description);

public sealed partial class PurchOrderService
{
    private const int FixedPrepaymentType = 1;

    public Task<PurchResult> DefinePrepaymentAsync(long recId, PurchPrepaymentDefinitionDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => DefinePrepaymentCoreAsync(recId, input, ct));

    private async Task<PurchResult> DefinePrepaymentCoreAsync(long recId, PurchPrepaymentDefinitionDto input, CancellationToken ct)
    {
        var description = input.Description?.Trim();
        if (input.Amount <= 0 || input.Amount > 99999999999999.9999m
            || decimal.Round(input.Amount, 4) != input.Amount || input.CategoryRecId <= 0 || string.IsNullOrEmpty(description))
            return new(400, null, "A valid fixed prepayment amount, category and description are required.");

        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var purchaseId = await Db.Set<PurchTable>().Where(x => x.DataAreaId == area && x.RecId == recId)
                .Select(x => x.PurchId).SingleOrDefaultAsync(ct);
            if (purchaseId == null) return await Fail(404, "Purchase order was not found.", ct);
            await new SubledgerPostingSupport(Db).LockAsync($"PurchDocument:{area}:{purchaseId}", ct);
            var order = await Db.Set<PurchTable>().AsNoTracking().SingleAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (order.PurchaseType != PurchaseType.Purch || order.PurchStatus is PurchStatus.Canceled or PurchStatus.Invoiced)
                return await Fail(422, "Only an active purchase order can have a prepayment definition.", ct);

            var existing = await Db.Set<PurchPrepayTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.PurchTable == purchaseId, ct);
            if (existing != null && existing.Type == FixedPrepaymentType && existing.Value == input.Amount && existing.PrepayCategoryId == input.CategoryRecId
                && existing.Description == description)
            {
                await _unitOfWork.CommitTransactionAsync(ct);
                return new(200, existing, "Purchase order prepayment is already defined.");
            }
            if (await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area && x.PurchId == purchaseId, ct))
                return await Fail(422, "A prepayment definition cannot be changed after invoicing.", ct);
            if (!await Db.Set<EcoResCategory>().AnyAsync(x => x.RecId == input.CategoryRecId && x.IsActive && !x.IsDeleted, ct))
                return await Fail(422, "The prepayment category is not active.", ct);

            var amounts = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == purchaseId
                && x.PurchStatus != PurchStatus.Canceled).Select(x => x.LineAmount).ToListAsync(ct);
            var limit = amounts.Sum();
            if (input.Amount > limit || limit > 99999999999999.9999m)
                return await Fail(422, "The prepayment amount exceeds the supported purchase order value.", ct);

            var prepayment = existing ?? new PurchPrepayTable { DataAreaId = area, PurchTable = purchaseId };
            prepayment.Type = FixedPrepaymentType;
            prepayment.Currency = order.CurrencyCode;
            prepayment.Description = description;
            prepayment.PrepayCategoryId = input.CategoryRecId;
            prepayment.Value = input.Amount;
            prepayment.PrepayLimit = limit;
            prepayment.PrepayAvailable = input.Amount;
            if (existing == null) Db.Add(prepayment);
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, prepayment, "Purchase order prepayment defined.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }
}
