using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderCopyService : BaseService<SalesTable>, ISalesOrderCopyService
{
    private readonly ISalesInventoryDemandService _inventoryDemand;
    private readonly SalesUnitConversionService _units;
    private readonly IValidator<SalesOrderCopyRequest> _validator;
    private readonly ICompanyExecutionContext _company;

    public SalesOrderCopyService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ISalesInventoryDemandService inventoryDemand,
        SalesUnitConversionService units,
        IValidator<SalesOrderCopyRequest> validator,
        ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _inventoryDemand = inventoryDemand;
        _units = units;
        _validator = validator;
        _company = company;
    }

    public SalesOrderCopyService(
        IUnitOfWork unitOfWork,
        ISalesInventoryDemandService inventoryDemand,
        SalesUnitConversionService units,
        IValidator<SalesOrderCopyRequest> validator,
        ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), inventoryDemand, units, validator, company)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public async Task<SalesOrderCopyLookupResult> GetSalesOrdersAsync(long destinationRecId, CancellationToken ct)
    {
        var destination = await FindDestinationAsync(destinationRecId, ct);
        if (destination == null)
            return new(404, Error: "Destination sales order was not found.");
        var area = destination.DataAreaId;
        var rows = await _unitOfWork.Context.Set<SalesTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.RecId != destinationRecId
                && _unitOfWork.Context.Set<SalesLine>().Any(line => line.DataAreaId == area && line.SalesId == x.SalesId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { id = x.RecId.ToString(), documentNumber = x.SalesId, account = x.CustAccount,
                accountName = x.SalesName, createdAt = x.CreatedAt, currencyCode = x.CurrencyCode,
                sourceType = "salesOrder" })
            .ToListAsync(ct);
        return new(200, rows);
    }

    public async Task<SalesOrderCopyLookupResult> GetSalesOrderLinesAsync(long destinationRecId, long sourceRecId, CancellationToken ct)
    {
        var destination = await FindDestinationAsync(destinationRecId, ct);
        if (destination == null)
            return new(404, Error: "Destination sales order was not found.");
        var area = destination.DataAreaId;
        if (destinationRecId == sourceRecId)
            return new(422, Error: "The destination order cannot be used as its own source.");
        var source = await _unitOfWork.Context.Set<SalesTable>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.RecId == sourceRecId && x.DataAreaId == area, ct);
        if (source == null) return new(404, Error: "Source sales order was not found.");
        var lines = await _unitOfWork.Context.Set<SalesLine>().AsNoTracking()
            .Where(x => x.SalesId == source.SalesId && x.DataAreaId == area)
            .ToListAsync(ct);
        lines = lines.OrderBy(x => x.LineNum).ToList();
        return new(200, await PresentLinesAsync(lines.Select(x => new CopySourceLine(
            x.RecId, x.ItemId, x.Name, x.SalesQty, x.SalesUnit, x.SalesPrice, x.PriceUnit,
            x.LineDisc, x.LinePercent, x.MultiLnDisc, x.MultiLnPercent, x.LineAmount,
            x.InventDimId, x.TaxGroup, x.TaxItemGroup, x.DlvMode, x.DlvTerm,
            x.ReceiptDateRequested, x.ShippingDateRequested, x.SalesCategory, x.SalesMarkup)).ToList(), area, ct));
    }

    public async Task<SalesOrderCopyLookupResult> GetJournalsAsync(long destinationRecId, CancellationToken ct)
    {
        var destination = await FindDestinationAsync(destinationRecId, ct);
        if (destination == null)
            return new(404, Error: "Destination sales order was not found.");
        var area = destination.DataAreaId;
        var rows = await _unitOfWork.Context.Set<CustConfirmJour>().AsNoTracking()
            .Where(x => x.DataAreaId == area
                && _unitOfWork.Context.Set<CustConfirmTrans>().Any(line => line.DataAreaId == area
                    && line.ConfirmId == x.ConfirmId && line.SalesId == x.SalesId))
            .OrderByDescending(x => x.ConfirmDate)
            .Select(x => new { id = x.RecId.ToString(), documentNumber = x.ConfirmId, account = x.OrderAccount,
                accountName = x.DeliveryName, createdAt = x.ConfirmDate, currencyCode = x.CurrencyCode,
                sourceType = "confirmation" })
            .ToListAsync(ct);
        return new(200, rows);
    }

    public async Task<SalesOrderCopyLookupResult> GetJournalLinesAsync(long destinationRecId, long sourceRecId, CancellationToken ct)
    {
        var destination = await FindDestinationAsync(destinationRecId, ct);
        if (destination == null)
            return new(404, Error: "Destination sales order was not found.");
        var area = destination.DataAreaId;
        var journal = await _unitOfWork.Context.Set<CustConfirmJour>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.RecId == sourceRecId && x.DataAreaId == area, ct);
        if (journal == null) return new(404, Error: "Confirmation journal was not found.");
        var lines = await _unitOfWork.Context.Set<CustConfirmTrans>().AsNoTracking()
            .Where(x => x.ConfirmId == journal.ConfirmId && x.SalesId == journal.SalesId
                && x.DataAreaId == area)
            .ToListAsync(ct);
        lines = lines.OrderBy(x => x.LineNum).ToList();
        return new(200, await PresentLinesAsync(lines.Select(x => new CopySourceLine(
            x.RecId, x.ItemId, x.Name, x.Qty, x.SalesUnit, x.SalesPrice, x.PriceUnit,
            x.LineDisc, x.LinePercent, x.MultiLnDisc, x.MultiLnPercent, x.LineAmount,
            x.InventDimId, x.TaxGroup, x.TaxItemGroup, string.Empty, x.DlvTerm,
            x.DlvDate, default, x.SalesCategory, x.SalesMarkup)).ToList(), area, ct));
    }

    private async Task<object> PresentLinesAsync(List<CopySourceLine> lines, string area, CancellationToken ct)
    {
        var ids = lines.Select(x => x.InventDimId).Where(x => x != string.Empty).Distinct().ToList();
        var dims = await _unitOfWork.Context.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == area && ids.Contains(x.InventDimId))
            .ToDictionaryAsync(x => x.InventDimId, ct);
        return lines.Select(x => new { id = x.Id.ToString(), x.ItemId, description = x.Name, quantity = x.Quantity,
            unit = x.Unit, unitPrice = x.UnitPrice, netAmount = x.LineAmount,
            discount = x.LineDiscount, discountPercent = x.LineDiscountPercent,
            site = dims.TryGetValue(x.InventDimId, out var d) ? d.InventSiteId : string.Empty,
            warehouse = dims.TryGetValue(x.InventDimId, out d) ? d.InventLocationId : string.Empty });
    }

    private Task<SalesTable?> FindDestinationAsync(long destinationRecId, CancellationToken ct)
        => _unitOfWork.Context.Set<SalesTable>().AsNoTracking().SingleOrDefaultAsync(
            x => x.RecId == destinationRecId && x.DataAreaId == _company.GetDataAreaId(), ct);

    public async Task<SalesOrderCopyResult> CopyAsync(long destinationRecId, SalesOrderCopyRequest? request, CancellationToken ct)
    {
        if (request?.LineIds == null)
            return new(SalesOrderCopyStatus.InvalidRequest, Error: "A copy request is required.");
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return new(SalesOrderCopyStatus.InvalidRequest, Error: validation.Errors[0].ErrorMessage);
        var lineIds = request.LineIds.Distinct().Select(long.Parse).ToList();

        return await _unitOfWork.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            var destination = await _unitOfWork.Context.Set<SalesTable>().SingleOrDefaultAsync(
                x => x.RecId == destinationRecId && x.DataAreaId == _company.GetDataAreaId(), ct);
            if (destination == null) return new SalesOrderCopyResult(SalesOrderCopyStatus.DestinationNotFound);
            var area = destination.DataAreaId;
            if (destination.SalesStatus != SalesStatus.Backorder)
                return new SalesOrderCopyResult(SalesOrderCopyStatus.DestinationClosed);

            var hasDifferentCurrency = request.Mode.Equals("fromJournal", StringComparison.OrdinalIgnoreCase)
                ? await _unitOfWork.Context.Set<CustConfirmTrans>().AsNoTracking().AnyAsync(
                    x => lineIds.Contains(x.RecId) && x.DataAreaId == area
                        && x.CurrencyCode != destination.CurrencyCode, ct)
                : await _unitOfWork.Context.Set<SalesLine>().AsNoTracking().AnyAsync(
                    x => lineIds.Contains(x.RecId) && x.DataAreaId == area
                        && x.CurrencyCode != destination.CurrencyCode, ct);
            if (hasDifferentCurrency)
                return new SalesOrderCopyResult(SalesOrderCopyStatus.InvalidRequest,
                    Error: "Source and destination currencies must match when copying sales lines.");

            List<CopySourceLine> sources;
            if (request.Mode.Equals("fromJournal", StringComparison.OrdinalIgnoreCase))
            {
                sources = await _unitOfWork.Context.Set<CustConfirmTrans>().AsNoTracking()
                    .Where(x => lineIds.Contains(x.RecId) && x.DataAreaId == area
                        && _unitOfWork.Context.Set<CustConfirmJour>().Any(journal =>
                            journal.DataAreaId == area && journal.ConfirmId == x.ConfirmId
                            && journal.SalesId == x.SalesId))
                    .Select(x => new CopySourceLine(x.RecId, x.ItemId, x.Name, x.Qty, x.SalesUnit,
                        x.SalesPrice, x.PriceUnit, x.LineDisc, x.LinePercent, x.MultiLnDisc,
                        x.MultiLnPercent, x.LineAmount, x.InventDimId, x.TaxGroup, x.TaxItemGroup,
                        string.Empty, x.DlvTerm, x.DlvDate, default, x.SalesCategory, x.SalesMarkup)).ToListAsync(ct);
            }
            else
            {
                sources = await _unitOfWork.Context.Set<SalesLine>().AsNoTracking()
                    .Where(x => lineIds.Contains(x.RecId) && x.DataAreaId == area
                        && x.SalesId != destination.SalesId
                        && _unitOfWork.Context.Set<SalesTable>().Any(order =>
                            order.DataAreaId == area && order.SalesId == x.SalesId))
                    .Select(x => new CopySourceLine(x.RecId, x.ItemId, x.Name, x.SalesQty, x.SalesUnit,
                        x.SalesPrice, x.PriceUnit, x.LineDisc, x.LinePercent, x.MultiLnDisc,
                        x.MultiLnPercent, x.LineAmount, x.InventDimId, x.TaxGroup, x.TaxItemGroup,
                        x.DlvMode, x.DlvTerm, x.ReceiptDateRequested, x.ShippingDateRequested,
                        x.SalesCategory, x.SalesMarkup)).ToListAsync(ct);
            }
            if (sources.Count != lineIds.Count)
                return new SalesOrderCopyResult(SalesOrderCopyStatus.SourceLinesNotFound);

            var dimensionIds = sources.Select(x => x.InventDimId).Where(x => x != string.Empty).Distinct().ToList();
            var dimensions = await _unitOfWork.Context.Set<InventDim>().AsNoTracking()
                .Where(x => x.DataAreaId == area && dimensionIds.Contains(x.InventDimId))
                .ToDictionaryAsync(x => x.InventDimId, ct);
            var existingLineNumbers = await _unitOfWork.Context.Set<SalesLine>()
                .Where(x => x.SalesId == destination.SalesId && x.DataAreaId == area)
                .Select(x => x.LineNum).ToListAsync(ct);
            var nextLine = existingLineNumbers.Count == 0 ? 0m : existingLineNumbers.Max();
            var created = 0;
            foreach (var source in sources.OrderBy(x => x.Id))
            {
                var sign = request.InvertSign ? -1 : 1;
                var quantity = source.Quantity * request.QuantityFactor * sign;
                if (quantity == 0) continue;
                var item = await _unitOfWork.Context.Set<InventTable>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ItemId == source.ItemId && x.DataAreaId == area, ct);
                if (item == null) return new SalesOrderCopyResult(SalesOrderCopyStatus.InvalidUnit);
                var inventoryQuantity = await _units.ToInventoryQuantityAsync(item, source.Unit, quantity, ct);
                if (inventoryQuantity == null) return new SalesOrderCopyResult(SalesOrderCopyStatus.InvalidUnit);
                var price = source.UnitPrice;
                var priceUnit = source.PriceUnit > 0 ? source.PriceUnit : 1m;
                if (request.RecalculatePrice)
                {
                    var module = await _unitOfWork.Context.Set<InventTableModule>().AsNoTracking().FirstOrDefaultAsync(
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
                    SalesMarkup = request.CopyPrecisely ? source.SalesMarkup * request.QuantityFactor : 0,
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
                _unitOfWork.Context.Set<SalesLine>().Add(line);
                destination.SmmSalesAmountTotal += line.LineAmount;
                created++;
            }
            if (created > 0 && destination.DocumentStatus == DocumentStatus.Confirmation)
                destination.DocumentStatus = DocumentStatus.None;
            await _unitOfWork.CompleteAsync(ct);
            await SalesOrderChargeRefresh.RefreshAsync(_unitOfWork.Context, destination, ct);
            await _unitOfWork.CompleteAsync(ct);
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
        long SalesCategory, decimal SalesMarkup);
