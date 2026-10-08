using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public record SalesOrderCopyRequest(string Mode, List<string> LineIds, decimal QuantityFactor = 1,
    bool InvertSign = false, bool RecalculatePrice = false, bool CopyPrecisely = true);

public enum SalesOrderCopyStatus { Success, DestinationNotFound, DestinationClosed, SourceLinesNotFound, InvalidUnit }
public sealed record SalesOrderCopyResult(SalesOrderCopyStatus Status, int CopiedLineCount = 0);

public sealed class SalesOrderCopyService
{
    private readonly IFinanceDataContext _db;
    private readonly ISalesInventoryDemandService _inventoryDemand;
    private readonly SalesUnitConversionService _units;

    public SalesOrderCopyService(IFinanceDataContext db, ISalesInventoryDemandService inventoryDemand,
        SalesUnitConversionService units)
    {
        _db = db;
        _inventoryDemand = inventoryDemand;
        _units = units;
    }

    public async Task<SalesOrderCopyResult> CopyAsync(long destinationRecId, SalesOrderCopyRequest request, List<long> lineIds, CancellationToken ct)
    {
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            var destination = await _db.Set<SalesTable>().SingleOrDefaultAsync(
                x => x.RecId == destinationRecId, ct);
            if (destination == null) return new SalesOrderCopyResult(SalesOrderCopyStatus.DestinationNotFound);
            var area = destination.DataAreaId;
            if (destination.SalesStatus != SalesStatus.Backorder)
                return new SalesOrderCopyResult(SalesOrderCopyStatus.DestinationClosed);

            List<CopySourceLine> sources;
            if (request.Mode.Equals("fromJournal", StringComparison.OrdinalIgnoreCase))
            {
                sources = await _db.Set<CustConfirmTrans>().AsNoTracking()
                    .Where(x => lineIds.Contains(x.RecId) && x.DataAreaId == area)
                    .Select(x => new CopySourceLine(x.RecId, x.ItemId, x.Name, x.Qty, x.SalesUnit,
                        x.SalesPrice, x.PriceUnit, x.LineDisc, x.LinePercent, x.MultiLnDisc,
                        x.MultiLnPercent, x.LineAmount, x.InventDimId, x.TaxGroup, x.TaxItemGroup,
                        string.Empty, x.DlvTerm, x.DlvDate, default, x.SalesCategory)).ToListAsync(ct);
            }
            else
            {
                sources = await _db.Set<SalesLine>().AsNoTracking()
                    .Where(x => lineIds.Contains(x.RecId) && x.DataAreaId == area
                        && x.SalesId != destination.SalesId)
                    .Select(x => new CopySourceLine(x.RecId, x.ItemId, x.Name, x.SalesQty, x.SalesUnit,
                        x.SalesPrice, x.PriceUnit, x.LineDisc, x.LinePercent, x.MultiLnDisc,
                        x.MultiLnPercent, x.LineAmount, x.InventDimId, x.TaxGroup, x.TaxItemGroup,
                        x.DlvMode, x.DlvTerm, x.ReceiptDateRequested, x.ShippingDateRequested,
                        x.SalesCategory)).ToListAsync(ct);
            }
            if (sources.Count != lineIds.Count)
                return new SalesOrderCopyResult(SalesOrderCopyStatus.SourceLinesNotFound);

            var dimensionIds = sources.Select(x => x.InventDimId).Where(x => x != string.Empty).Distinct().ToList();
            var dimensions = await _db.Set<InventDim>().AsNoTracking()
                .Where(x => x.DataAreaId == area && dimensionIds.Contains(x.InventDimId))
                .ToDictionaryAsync(x => x.InventDimId, ct);
            var nextLine = await _db.Set<SalesLine>().Where(x => x.SalesId == destination.SalesId && x.DataAreaId == area)
                .MaxAsync(x => (decimal?)x.LineNum, ct) ?? 0;
            var created = 0;
            foreach (var source in sources.OrderBy(x => x.Id))
            {
                var sign = request.InvertSign ? -1 : 1;
                var quantity = source.Quantity * request.QuantityFactor * sign;
                if (quantity == 0) continue;
                var item = await _db.Set<InventTable>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ItemId == source.ItemId && x.DataAreaId == area, ct);
                if (item == null) return new SalesOrderCopyResult(SalesOrderCopyStatus.InvalidUnit);
                var inventoryQuantity = await _units.ToInventoryQuantityAsync(item, source.Unit, quantity, ct);
                if (inventoryQuantity == null) return new SalesOrderCopyResult(SalesOrderCopyStatus.InvalidUnit);
                var price = source.UnitPrice;
                var priceUnit = source.PriceUnit > 0 ? source.PriceUnit : 1m;
                if (request.RecalculatePrice)
                {
                    var module = await _db.Set<InventTableModule>().AsNoTracking().FirstOrDefaultAsync(
                        x => x.ItemId == source.ItemId && x.DataAreaId == area && x.ModuleType == ModuleInventPurchSales.Sales, ct);
                    if (module != null && string.Equals(module.UnitId, source.Unit, StringComparison.OrdinalIgnoreCase))
                    {
                        price = module.Price;
                        priceUnit = module.PriceUnit > 0 ? module.PriceUnit : 1m;
                    }
                }
                var line = new SalesLine
                {
                    SalesId = destination.SalesId, LineNum = ++nextLine, ItemId = source.ItemId,
                    Name = source.Name, CustAccount = destination.CustAccount, CustGroupId = destination.CustGroup,
                    CurrencyCode = destination.CurrencyCode, SalesQty = quantity, QtyOrdered = inventoryQuantity.Value,
                    RemainSalesPhysical = quantity, RemainSalesFinancial = quantity,
                    SalesUnit = source.Unit, PriceUnit = priceUnit,
                    SalesPrice = price, LineAmount = quantity * price / priceUnit,
                    LineDisc = request.CopyPrecisely ? source.LineDiscount * request.QuantityFactor * sign : 0,
                    LinePercent = request.CopyPrecisely ? source.LineDiscountPercent : 0,
                    MultiLnDisc = request.CopyPrecisely ? source.MultiLineDiscount * request.QuantityFactor * sign : 0,
                    MultiLnPercent = request.CopyPrecisely ? source.MultiLineDiscountPercent : 0,
                    TaxGroup = source.TaxGroup, TaxItemGroup = source.TaxItemGroup,
                    DlvMode = source.DeliveryMode, DlvTerm = source.DeliveryTerms,
                    ReceiptDateRequested = source.DeliveryDate == default ? destination.ReceiptDateRequested : source.DeliveryDate,
                    ShippingDateRequested = source.ShippingDate == default ? destination.ShippingDateRequested : source.ShippingDate,
                    SalesCategory = source.SalesCategory, SalesStatus = SalesStatus.Backorder,
                    SalesType = destination.SalesType ?? SalesType.Sales, DataAreaId = area
                };
                dimensions.TryGetValue(source.InventDimId, out var dimension);
                await _inventoryDemand.CreateAsync(destination, line,
                    dimension?.InventSiteId ?? destination.InventSiteId,
                    dimension?.InventLocationId ?? destination.InventLocationId, ct);
                _db.Set<SalesLine>().Add(line);
                destination.SmmSalesAmountTotal += line.LineAmount;
                created++;
            }
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return new SalesOrderCopyResult(SalesOrderCopyStatus.Success, created);
        });
    }
}

internal sealed record CopySourceLine(long Id, string ItemId, string Name, decimal Quantity,
        string Unit, decimal UnitPrice, decimal PriceUnit, decimal LineDiscount,
        decimal LineDiscountPercent, decimal MultiLineDiscount, decimal MultiLineDiscountPercent,
        decimal LineAmount, string InventDimId, string TaxGroup, string TaxItemGroup,
        string DeliveryMode, string DeliveryTerms, DateTime DeliveryDate, DateTime ShippingDate,
        long SalesCategory);
