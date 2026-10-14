using System.Globalization;
using System.Data;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

public sealed class PackingSlipJournalService(
    IUnitOfWork unitOfWork,
    ICompanyExecutionContext company,
    ISysNumberSequenceService numberSequences) : IPackingSlipJournalService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<PackingSlipJournalResult> ListAsync(long salesOrderRecId, CancellationToken ct)
    {
        var order = await FindOrderAsync(salesOrderRecId, ct);
        if (order is null) return new(404, null, "Sales order was not found in the selected company.");

        var journals = await Db.Set<CustPackingSlipJour>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && row.SalesId == order.SalesId)
            .OrderByDescending(row => row.DeliveryDate)
            .ThenByDescending(row => row.RecId)
            .ToListAsync(ct);
        return new(200, journals.Select(ToSummary).ToList());
    }

    public async Task<PackingSlipJournalResult> GetAsync(
        long salesOrderRecId,
        long packingSlipRecId,
        CancellationToken ct)
    {
        var order = await FindOrderAsync(salesOrderRecId, ct);
        if (order is null) return new(404, null, "Sales order was not found in the selected company.");

        var journal = await Db.Set<CustPackingSlipJour>().AsNoTracking().SingleOrDefaultAsync(
            row => row.RecId == packingSlipRecId
                && row.DataAreaId == order.DataAreaId
                && row.SalesId == order.SalesId,
            ct);
        if (journal is null) return new(404, null, "Packing slip was not found.");

        var lines = await Db.Set<CustPackingSlipTrans>().AsNoTracking()
            .Where(row => row.DataAreaId == journal.DataAreaId
                && row.SalesId == journal.SalesId
                && row.PackingSlipId == journal.PackingSlipId)
            .OrderBy(row => row.LineNum)
            .ToListAsync(ct);
        var dimensionIds = lines.Select(row => row.InventDimId)
            .Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        var dimensions = await Db.Set<InventDim>().AsNoTracking()
            .Where(row => row.DataAreaId == journal.DataAreaId && dimensionIds.Contains(row.InventDimId))
            .ToDictionaryAsync(row => row.InventDimId, ct);

        var presentedLines = lines.Select(line =>
        {
            var dto = ToLine(line);
            if (dimensions.TryGetValue(line.InventDimId, out var dimension))
            {
                dto.SiteId = dimension.InventSiteId;
                dto.WarehouseId = dimension.InventLocationId;
                dto.BatchNumber = dimension.InventBatchId;
                dto.SerialNumber = dimension.InventSerialId;
            }
            return dto;
        }).ToList();

        return new(200, new { header = ToSummary(journal), lines = presentedLines });
    }

    public async Task<PackingSlipJournalResult> PreviewPostingAsync(
        long salesOrderRecId, string quantityMode, CancellationToken ct)
    {
        if (!IsValidQuantityMode(quantityMode))
            return new(400, null, "Quantity must be All, Deliver now, or Picked.");
        var order = await FindOrderAsync(salesOrderRecId, ct);
        if (order is null) return new(404, null, "Sales order was not found in the selected company.");
        if (order.SalesStatus != SalesStatus.Backorder)
            return new(422, null, "Only an open sales order can be shipped.");

        var salesLines = await Db.Set<SalesLine>().AsNoTracking()
            .Where(line => line.DataAreaId == order.DataAreaId && line.SalesId == order.SalesId
                && line.RemainSalesPhysical > 0)
            .OrderBy(line => line.LineNum).ToListAsync(ct);
        var mode = NormalizeQuantityMode(quantityMode);
        var proposedQuantities = await ProposedQuantitiesAsync(order, salesLines, mode, ct);
        var dimensionsById = await GetDimensionsAsync(order.DataAreaId, salesLines.Select(line => line.InventDimId), ct);
        var partyId = await Db.Set<CustTable>().AsNoTracking()
            .Where(customer => customer.DataAreaId == order.DataAreaId && customer.AccountNum == order.CustAccount)
            .Select(customer => customer.Party).SingleOrDefaultAsync(ct);
        var customerName = partyId > 0
            ? await Db.Set<DirPartyTable>().AsNoTracking().Where(party => party.RecId == partyId)
                .Select(party => party.Name).SingleOrDefaultAsync(ct)
            : null;
        var lines = salesLines.Select(line => PostingLine(line, proposedQuantities.GetValueOrDefault(line.RecId),
            dimensionsById.GetValueOrDefault(line.InventDimId))).Where(line => line.Quantity > 0).ToList();
        return new(200, new PackingSlipPostingPreviewDto
        {
            Header = new PackingSlipPostingHeaderDto
            {
                Id = order.RecId.ToString(CultureInfo.InvariantCulture),
                SalesId = order.SalesId,
                CustomerName = customerName ?? order.CustAccount,
                CurrencyCode = order.CurrencyCode,
                SiteId = order.InventSiteId,
            },
            Lines = lines,
        });
    }

    public async Task<PackingSlipJournalResult> PostAsync(
        long salesOrderRecId, PostPackingSlipRequest? request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RequestId)
            || request.RequestId.Length > FieldLengths.ParmId || request.Lines is null)
            return new(400, null, "A request ID and at least one valid posting line are required.");
        if (!IsValidQuantityMode(request.QuantityMode))
            return new(400, null, "Quantity must be All, Deliver now, or Picked.");
        if (!request.Posting) return await PreviewPostingAsync(salesOrderRecId, request.QuantityMode, ct);
        if (request.Lines.Count == 0 || request.Lines.Any(line => line.Quantity <= 0
            || !long.TryParse(line.SalesLineId, NumberStyles.None, CultureInfo.InvariantCulture, out _)))
            return new(422, null, "Enter a positive delivery quantity for at least one sales line.");

        return await Db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var order = await FindOrderAsync(salesOrderRecId, ct);
            if (order is null) return new PackingSlipJournalResult(404, null, "Sales order was not found in the selected company.");
            var previous = await Db.Set<CustPackingSlipJour>().AsNoTracking().SingleOrDefaultAsync(
                row => row.DataAreaId == order.DataAreaId && row.SalesId == order.SalesId && row.ParmId == request.RequestId, ct);
            if (previous is not null)
                return new PackingSlipJournalResult(200, ToSummary(previous), "Packing slip posted.");
            if (order.SalesStatus != SalesStatus.Backorder)
                return new PackingSlipJournalResult(422, null, "Only an open sales order can be shipped.");

            var requested = new Dictionary<long, decimal>();
            foreach (var entry in request.Lines)
            {
                if (!long.TryParse(entry.SalesLineId, NumberStyles.None, CultureInfo.InvariantCulture, out var lineId)
                    || requested.TryGetValue(lineId, out _))
                    return new PackingSlipJournalResult(422, null, "The selected sales lines are invalid or duplicated.");
                requested[lineId] = entry.Quantity;
            }
            var lines = await Db.Set<SalesLine>().Where(line => line.DataAreaId == order.DataAreaId
                && line.SalesId == order.SalesId && requested.Keys.Contains(line.RecId))
                .OrderBy(line => line.LineNum).ToListAsync(ct);
            if (lines.Count != requested.Count)
                return new PackingSlipJournalResult(422, null, "A selected sales line no longer belongs to this order.");
            if (lines.Any(line => line.SalesStatus != SalesStatus.Backorder || line.StockedProduct != NoYes.Yes
                || line.RemainSalesPhysical <= 0 || requested[line.RecId] > line.RemainSalesPhysical))
                return new PackingSlipJournalResult(422, null,
                    "Delivery quantities must be positive and cannot exceed the remaining physical quantity on stocked open lines.");

            var dimensions = await GetDimensionsAsync(order.DataAreaId, lines.Select(line => line.InventDimId), ct);
            if (lines.Any(line => string.IsNullOrWhiteSpace(line.InventTransId)
                || !dimensions.ContainsKey(line.InventDimId)))
                return new PackingSlipJournalResult(422, null, "Every selected line must have a valid inventory transaction and dimensions.");

            var packingSlipId = await NextPackingSlipIdAsync(order.DataAreaId, ct);
            var deliveryDate = request.DeliveryDate?.Date ?? DateTime.UtcNow.Date;
            var journal = new CustPackingSlipJour
            {
                PackingSlipId = packingSlipId,
                DeliveryDate = deliveryDate,
                DocumentDate = deliveryDate,
                SalesId = order.SalesId,
                SalesType = order.SalesType ?? SalesType.Sales,
                OrderAccount = order.CustAccount,
                InvoiceAccount = string.IsNullOrWhiteSpace(order.InvoiceAccount) ? order.CustAccount : order.InvoiceAccount,
                DeliveryPostalAddress = order.DeliveryPostalAddress,
                DeliveryName = order.DeliveryName,
                DlvMode = order.DlvMode,
                DlvTerm = order.DlvTerm,
                ShipCarrierBlindShipment = order.ShipCarrierBlindShipment,
                ParmId = request.RequestId,
                PostedState = NoYes.Yes,
                DataAreaId = order.DataAreaId,
            };
            Db.Set<CustPackingSlipJour>().Add(journal);
            await Db.SaveChangesAsync(ct);

            foreach (var line in lines)
            {
                var salesQuantity = requested[line.RecId];
                var inventQuantity = ToInventoryQuantity(line, salesQuantity);
                if (inventQuantity <= 0 || inventQuantity > line.RemainInventPhysical)
                    return new PackingSlipJournalResult(422, null,
                        $"The inventory quantity exceeds the remaining physical quantity for item {line.ItemId}.");
                var dimension = dimensions[line.InventDimId];
                var inventory = await Db.Set<InventSum>().SingleOrDefaultAsync(row => row.DataAreaId == order.DataAreaId
                    && row.ItemId == line.ItemId && row.InventDimId == line.InventDimId, ct);
                if (inventory is null)
                    return new PackingSlipJournalResult(422, null, $"Inventory summary is missing for item {line.ItemId}.");
                if (inventory.PhysicalInvent < inventQuantity)
                    return new PackingSlipJournalResult(422, null, $"Physical on-hand is insufficient for item {line.ItemId}.");
                if (line.Complete != 0 && salesQuantity != line.RemainSalesPhysical)
                    return new PackingSlipJournalResult(422, null, $"Item {line.ItemId} requires complete delivery on this order line.");

                var origin = await Db.Set<InventTransOrigin>().SingleOrDefaultAsync(row => row.DataAreaId == order.DataAreaId
                    && row.InventTransId == line.InventTransId && row.ReferenceId == order.SalesId
                    && row.ReferenceCategory == InventRefType.SalesTable, ct);
                if (origin is null || origin.ItemId != line.ItemId || origin.ItemInventDimId != line.InventDimId)
                    return new PackingSlipJournalResult(422, null, $"Inventory origin is invalid for item {line.ItemId}.");
                var sourceTransactions = await Db.Set<InventTrans>().Where(row => row.DataAreaId == order.DataAreaId
                    && row.InventTransOrigin == origin.RecId
                    && (row.StatusIssue == StatusIssue.Ordered || row.StatusIssue == StatusIssue.Picked))
                    .OrderBy(row => row.RecId).ToListAsync(ct);
                if (sourceTransactions.Any(row => row.ItemId != line.ItemId || row.InventDimId != line.InventDimId)
                    || sourceTransactions.Sum(row => -row.Qty) < inventQuantity)
                    return new PackingSlipJournalResult(422, null, $"Open inventory demand does not cover the delivery quantity for item {line.ItemId}.");

                var quantityLeft = inventQuantity;
                decimal postedCatchWeightQuantity = 0;
                foreach (var source in sourceTransactions)
                {
                    if (quantityLeft <= 0) break;
                    var sourceQuantity = -source.Qty;
                    if (sourceQuantity <= 0) continue;
                    var postingQuantity = Math.Min(sourceQuantity, quantityLeft);
                    var priorStatus = source.StatusIssue;
                    if (priorStatus == StatusIssue.Ordered && inventory.OnOrder < postingQuantity
                        || priorStatus == StatusIssue.Picked && inventory.Picked < postingQuantity)
                        return new PackingSlipJournalResult(422, null, $"Inventory summary does not match open demand for item {line.ItemId}.");
                    if (postingQuantity < sourceQuantity)
                    {
                        var originalCatchWeight = source.PdscwQty;
                        var remaining = CopyOpenTransaction(source, sourceQuantity - postingQuantity);
                        if (sourceQuantity > 0 && originalCatchWeight > 0)
                        {
                            remaining.PdscwQty = originalCatchWeight * (sourceQuantity - postingQuantity) / sourceQuantity;
                            source.PdscwQty = originalCatchWeight * postingQuantity / sourceQuantity;
                        }
                        Db.Set<InventTrans>().Add(remaining);
                    }
                    var catchWeightForPosting = sourceQuantity > 0 && source.PdscwQty > 0
                        ? source.PdscwQty * postingQuantity / sourceQuantity : 0m;
                    source.Qty = -postingQuantity;
                    source.StatusIssue = StatusIssue.Deducted;
                    source.DateStatus = deliveryDate;
                    source.DatePhysical = deliveryDate;
                    source.DateInvent = deliveryDate;
                    source.PackingSlipId = packingSlipId;
                    source.PdscwQty = catchWeightForPosting;
                    if (priorStatus == StatusIssue.Ordered)
                    {
                        inventory.OnOrder -= postingQuantity;
                        inventory.PdscwOnOrder -= catchWeightForPosting;
                    }
                    else
                    {
                        inventory.Picked -= postingQuantity;
                        inventory.PdscwPicked -= catchWeightForPosting;
                    }
                    inventory.Deducted += postingQuantity;
                    inventory.PdscwDeducted += catchWeightForPosting;
                    inventory.PhysicalInvent -= postingQuantity;
                    inventory.PdscwPhysicalInvent -= catchWeightForPosting;
                    inventory.AvailPhysical -= postingQuantity;
                    inventory.PdscwAvailPhysical -= catchWeightForPosting;
                    inventory.LastUpdDatePhysical = deliveryDate;
                    postedCatchWeightQuantity += catchWeightForPosting;
                    quantityLeft -= postingQuantity;
                }
                if (quantityLeft > 0)
                    return new PackingSlipJournalResult(422, null, $"Inventory demand could not be allocated for item {line.ItemId}.");

                var lineAmount = line.PriceUnit > 0 ? salesQuantity * line.SalesPrice / line.PriceUnit : 0m;
                Db.Set<CustPackingSlipTrans>().Add(new CustPackingSlipTrans
                {
                    PackingSlipId = packingSlipId,
                    DeliveryDate = deliveryDate,
                    SalesId = order.SalesId,
                    OrigSalesId = order.SalesId,
                    LineNum = line.LineNum,
                    Name = line.Name,
                    ItemId = line.ItemId,
                    SalesUnit = line.SalesUnit,
                    StockedProduct = line.StockedProduct,
                    InventDimId = line.InventDimId,
                    InventTransId = line.InventTransId,
                    InventRefId = line.InventRefId,
                    InventRefTransId = line.InventRefTransId,
                    InventRefType = line.InventRefType,
                    Qty = salesQuantity,
                    Ordered = line.SalesQty,
                    InventQty = inventQuantity,
                    Remain = Math.Max(0m, line.RemainSalesPhysical - salesQuantity),
                    RemainInvent = Math.Max(0m, line.RemainInventPhysical - inventQuantity),
                    PdsCwQty = postedCatchWeightQuantity,
                    DeliveryPostalAddress = line.DeliveryPostalAddress,
                    SalesLineShippingDateRequested = line.ShippingDateRequested,
                    SalesLineShippingDateConfirmed = line.ShippingDateConfirmed,
                    DeliveryType = line.DeliveryType,
                    AmountCur = lineAmount,
                    DefaultDimension = line.DefaultDimension,
                    SalesCategory = line.SalesCategory,
                    ParentRecId = journal.RecId,
                    FullyMatched = NoYes.No,
                    Scrap = line.Scrap != 0 ? NoYes.Yes : NoYes.No,
                    DataAreaId = order.DataAreaId,
                });
                line.RemainSalesPhysical = Math.Max(0m, line.RemainSalesPhysical - salesQuantity);
                line.RemainInventPhysical = Math.Max(0m, line.RemainInventPhysical - inventQuantity);
                line.SalesDeliverNow = 0;
                line.InventDeliverNow = 0;
                line.PdsCwRemainInventPhysical = Math.Max(0m, line.PdsCwRemainInventPhysical - postedCatchWeightQuantity);
                line.PdsCwInventDeliverNow = 0;
                if (line.RemainSalesPhysical == 0) line.SalesStatus = SalesStatus.Delivered;
                journal.Qty += salesQuantity;
            }
            journal.InvoiceIssueDueDate_W = deliveryDate;
            var otherOpenLinesRemain = await Db.Set<SalesLine>().AnyAsync(line => line.DataAreaId == order.DataAreaId
                && line.SalesId == order.SalesId && !requested.Keys.Contains(line.RecId)
                && line.RemainSalesPhysical > 0 && line.SalesStatus == SalesStatus.Backorder, ct);
            if (!otherOpenLinesRemain && lines.All(line => line.RemainSalesPhysical <= 0))
                order.SalesStatus = SalesStatus.Delivered;
            order.DocumentStatus = DocumentStatus.PackingSlip;
            await unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return new PackingSlipJournalResult(200, ToSummary(journal), "Packing slip posted.");
        });
    }

    private async Task<Dictionary<long, decimal>> ProposedQuantitiesAsync(
        SalesTable order, List<SalesLine> lines, string mode, CancellationToken ct)
    {
        if (mode != "picked") return lines.ToDictionary(line => line.RecId, line =>
            mode == "deliverNow" ? Math.Min(Math.Max(0m, line.SalesDeliverNow), line.RemainSalesPhysical)
                : line.RemainSalesPhysical);
        var ids = lines.Select(line => line.InventTransId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        var origins = await Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(origin => origin.DataAreaId == order.DataAreaId && ids.Contains(origin.InventTransId))
            .ToDictionaryAsync(origin => origin.InventTransId, ct);
        var originIds = origins.Values.Select(origin => origin.RecId).ToList();
        var picked = await Db.Set<InventTrans>().AsNoTracking()
            .Where(trans => trans.DataAreaId == order.DataAreaId && originIds.Contains(trans.InventTransOrigin)
                && trans.StatusIssue == StatusIssue.Picked)
            .GroupBy(trans => trans.InventTransOrigin)
            .Select(group => new { Origin = group.Key, Qty = -group.Sum(trans => trans.Qty) })
            .ToDictionaryAsync(group => group.Origin, group => group.Qty, ct);
        return lines.ToDictionary(line => line.RecId, line =>
        {
            if (!origins.TryGetValue(line.InventTransId, out var origin)) return 0m;
            var inventoryQuantity = picked.GetValueOrDefault(origin.RecId);
            return line.QtyOrdered > 0 ? Math.Min(line.RemainSalesPhysical,
                inventoryQuantity * line.SalesQty / line.QtyOrdered) : 0m;
        });
    }

    private async Task<Dictionary<string, InventDim>> GetDimensionsAsync(
        string area, IEnumerable<string> ids, CancellationToken ct)
    {
        var keys = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        return await Db.Set<InventDim>().AsNoTracking()
            .Where(row => row.DataAreaId == area && keys.Contains(row.InventDimId))
            .ToDictionaryAsync(row => row.InventDimId, ct);
    }

    private static PackingSlipPostingLineDto PostingLine(SalesLine line, decimal quantity, InventDim? dimension) => new()
    {
        Id = line.RecId.ToString(CultureInfo.InvariantCulture),
        LineNumber = line.LineNum,
        ItemId = line.ItemId,
        Name = line.Name,
        SalesCategory = line.SalesCategory.ToString(CultureInfo.InvariantCulture),
        ProductNumber = line.ItemId,
        InventSiteId = dimension?.InventSiteId ?? string.Empty,
        InventLocationId = dimension?.InventLocationId ?? string.Empty,
        WmsLocationId = dimension?.WmsLocationId ?? string.Empty,
        CatchWeightQuantity = line.PdsCwQty > 0 && line.SalesQty > 0 ? line.PdsCwQty * quantity / line.SalesQty : 0m,
        Quantity = quantity,
        NetAmount = line.PriceUnit > 0 ? quantity * line.SalesPrice / line.PriceUnit : 0m,
    };

    private static string NormalizeQuantityMode(string? mode) => mode?.Trim().ToLowerInvariant() switch
    {
        "delivernow" => "deliverNow",
        "picked" => "picked",
        _ => "all",
    };

    private static bool IsValidQuantityMode(string? mode) =>
        mode?.Trim().ToLowerInvariant() is "all" or "delivernow" or "picked";

    private static decimal ToInventoryQuantity(SalesLine line, decimal salesQuantity) =>
        line.SalesQty > 0 ? salesQuantity * line.QtyOrdered / line.SalesQty : 0m;

    private async Task<string> NextPackingSlipIdAsync(string area, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 1000; attempt++)
        {
            var candidate = (await numberSequences.NextAsync("CustPackingSlipJour", area, ct)).Code;
            if (!await Db.Set<CustPackingSlipJour>().IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(row => row.DataAreaId == area && row.PackingSlipId == candidate, ct)) return candidate;
        }
        throw new InvalidOperationException("The packing slip number sequence could not produce an unused PackingSlipId.");
    }

    private static InventTrans CopyOpenTransaction(InventTrans source, decimal quantity) => new()
    {
        ItemId = source.ItemId,
        InventDimId = source.InventDimId,
        InventTransOrigin = source.InventTransOrigin,
        InventDimFixed = source.InventDimFixed,
        StatusIssue = source.StatusIssue,
        StatusReceipt = source.StatusReceipt,
        ValueOpen = source.ValueOpen,
        Qty = -quantity,
        QtySettled = source.QtySettled,
        PdscwQty = source.PdscwQty,
        PdscwSettled = source.PdscwSettled,
        CurrencyCode = source.CurrencyCode,
        CostAmountPosted = source.CostAmountPosted,
        CostAmountPhysical = source.CostAmountPhysical,
        CostAmountAdjustment = source.CostAmountAdjustment,
        CostAmountSettled = source.CostAmountSettled,
        CostAmountStd = source.CostAmountStd,
        CostAmountOperations = source.CostAmountOperations,
        RevenueAmountPhysical = source.RevenueAmountPhysical,
        TaxAmountPhysical = source.TaxAmountPhysical,
        Voucher = source.Voucher,
        VoucherPhysical = source.VoucherPhysical,
        InvoiceId = source.InvoiceId,
        PackingSlipId = source.PackingSlipId,
        InvoiceReturned = source.InvoiceReturned,
        PackingSlipReturned = source.PackingSlipReturned,
        DateStatus = source.DateStatus,
        DatePhysical = source.DatePhysical,
        DateFinancial = source.DateFinancial,
        DateClosed = source.DateClosed,
        DateExpected = source.DateExpected,
        TimeExpected = source.TimeExpected,
        DateInvent = source.DateInvent,
        ShippingDateRequested = source.ShippingDateRequested,
        ShippingDateConfirmed = source.ShippingDateConfirmed,
        ProjId = source.ProjId,
        ProjCategoryId = source.ProjCategoryId,
        ProjAdjustRefId = source.ProjAdjustRefId,
        PickingRouteId = source.PickingRouteId,
        TransChildRefId = source.TransChildRefId,
        TransChildType = source.TransChildType,
        IntercompanyInventDimTransferred = source.IntercompanyInventDimTransferred,
        MarkingRefInventTransOrigin = source.MarkingRefInventTransOrigin,
        ReturnInventTransOrigin = source.ReturnInventTransOrigin,
        NonFinancialTransferInventClosing = source.NonFinancialTransferInventClosing,
        DataAreaId = source.DataAreaId,
    };

    private Task<SalesTable?> FindOrderAsync(long recId, CancellationToken ct) =>
        Db.Set<SalesTable>().AsNoTracking().SingleOrDefaultAsync(
            row => row.RecId == recId && row.DataAreaId == company.GetDataAreaId(), ct);

    private static PackingSlipJournalSummaryDto ToSummary(CustPackingSlipJour row) => new()
    {
        Id = row.RecId.ToString(CultureInfo.InvariantCulture),
        SalesId = row.SalesId,
        PackingSlipId = row.PackingSlipId,
        Version = row.InternalPackingSlipId,
        DeliveryDate = row.DeliveryDate,
        InvoiceDueDate = row.InvoiceIssueDueDate_W,
        Terms = row.DlvTerm,
        ModeOfDelivery = row.DlvMode,
        BlindShipment = row.ShipCarrierBlindShipment == NoYes.Yes,
        IntercompanyCompanyId = row.IntercompanyCompanyId,
        IntercompanyPosted = row.IntercompanyPosted == NoYes.Yes,
        Printed = row.Printed == NoYes.Yes,
        OwnerId = row.TransportationDeliveryOwner.ToString(CultureInfo.InvariantCulture),
        ContractorId = row.TransportationDeliveryContractor.ToString(CultureInfo.InvariantCulture),
    };

    private static PackingSlipJournalLineDto ToLine(CustPackingSlipTrans row) => new()
    {
        Id = row.RecId.ToString(CultureInfo.InvariantCulture),
        PackingSlipId = row.PackingSlipId,
        SalesId = row.SalesId,
        OriginalSalesId = row.OrigSalesId,
        LineNumber = row.LineNum,
        ItemId = row.ItemId,
        Description = row.Name,
        SalesUnit = row.SalesUnit,
        Quantity = row.Qty,
        InventQuantity = row.InventQty,
        CatchWeightQuantity = row.PdsCwQty,
        InventoryTransactionId = row.InventTransId,
        RequestedShipDate = row.SalesLineShippingDateRequested,
        ConfirmedShipDate = row.SalesLineShippingDateConfirmed,
        DeliveryType = (int)row.DeliveryType,
        Amount = row.AmountCur,
        AccountingAmount = row.ValueMst,
        FullyMatched = row.FullyMatched == NoYes.Yes,
        Scrap = row.Scrap == NoYes.Yes,
    };
}
