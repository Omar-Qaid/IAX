using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalService : IInventJournalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;
    private readonly IInventDimensionResolver _dimensions;

    public InventJournalService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ISysNumberSequenceService numbers, IInventDimensionResolver dimensions)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _numbers = numbers;
        _dimensions = dimensions;
    }

    private DbContext Db => _unitOfWork.Context;

    public async Task<List<InventJournalTableDto>> ListAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        var headers = await Db.Set<InventJournalTable>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId)
            .OrderByDescending(x => x.RecId).ToListAsync(ct);
        return headers.Select(MapHeader).ToList();
    }

    public async Task<InventJournalTableDto?> GetAsync(string journalId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var header = await Db.Set<InventJournalTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalId == Normalize(journalId), ct);
        return header == null ? null : await MapWithLinesAsync(header, companyId, ct);
    }

    public async Task<InventJournalLookupsDto> LookupsAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        return new InventJournalLookupsDto
        {
            JournalNames = await Db.Set<InventJournalName>().AsNoTracking().Where(x => x.DataAreaId == companyId)
                .OrderBy(x => x.JournalNameId).Select(x => new InventJournalLookupDto(x.JournalNameId, x.Description)).ToListAsync(ct),
            Items = await Db.Set<ReleasedItem>().AsNoTracking().Where(x => x.DataAreaId == companyId)
                .OrderBy(x => x.ItemId).Select(x => new InventJournalLookupDto(x.ItemId, x.Name)).ToListAsync(ct),
            Sites = await Db.Set<InventSite>().AsNoTracking().Where(x => x.DataAreaId == companyId)
                .OrderBy(x => x.SiteId).Select(x => new InventJournalLookupDto(x.SiteId, x.Name)).ToListAsync(ct),
            Warehouses = await Db.Set<InventLocation>().AsNoTracking().Where(x => x.DataAreaId == companyId)
                .OrderBy(x => x.InventLocationId).Select(x => new InventJournalLookupDto(x.InventLocationId, x.Name)).ToListAsync(ct),
            Units = await Db.Set<UnitOfMeasure>().IgnoreQueryFilters().AsNoTracking().Where(x => !x.IsDeleted)
                .OrderBy(x => x.Symbol).Select(x => new InventJournalLookupDto(x.Symbol, x.Symbol)).ToListAsync(ct)
        };
    }

    public async Task<(InventJournalTableDto? Journal, string? Error)> CreateAsync(InventJournalCreateDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var journalNameId = Normalize(input.JournalNameId);
        if (journalNameId.Length == 0) return (null, "Journal name is required.");
        var name = await Db.Set<InventJournalName>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalNameId == journalNameId, ct);
        if (name == null) return (null, "The selected journal name does not exist in the selected company.");
        var dimensionError = await ValidateSiteWarehouseAsync(companyId, input.InventSiteId, input.InventLocationId, false, ct);
        if (dimensionError != null) return (null, dimensionError);

        var journalId = (await _numbers.NextAsync(nameof(InventJournalTable), companyId, ct)).Code;
        var header = new InventJournalTable
        {
            DataAreaId = companyId,
            JournalId = journalId,
            JournalNameId = journalNameId,
            Description = input.Description.Trim(),
            JournalType = name.JournalType,
            JournalOriginType = InventJournalOriginType.Standard,
            InventSiteId = Normalize(input.InventSiteId),
            InventLocationId = Normalize(input.InventLocationId),
            VoucherNumberSequenceTable = name.VoucherNumberSequenceTable,
            DetailSummary = name.DetailSummary,
            DeletePostedLines = name.DeletePostedLines,
            Reservation = name.Reservation,
            Source = string.Empty,
            JournalIdOrignal = string.Empty,
            Posted = NoYes.No,
            PostedUserId = string.Empty
        };
        Db.Add(header);
        await _unitOfWork.CompleteAsync(ct);
        return (MapHeader(header), null);
    }

    public async Task<(InventJournalTableDto? Journal, string? Error)> AddLineAsync(string journalId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var header = await EditableHeaderAsync(companyId, journalId, ct);
        if (header == null) return (null, "The journal was not found or has already been posted.");
        var validation = await ValidateLineAsync(companyId, header, input, ct);
        if (validation.Error != null) return (null, validation.Error);
        var lineNumbers = await Db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId)
            .Select(x => x.LineNum).ToListAsync(ct);
        var lastLine = lineNumbers.Count == 0 ? 0m : lineNumbers.Max();
        var line = new InventJournalTrans { DataAreaId = companyId, JournalId = header.JournalId, LineNum = lastLine + 1m };
        ApplyLine(line, header, input, validation.FromDimension!, validation.ToDimension);
        Db.Add(line);
        header.NumOfLines++;
        await _unitOfWork.CompleteAsync(ct);
        return (await MapWithLinesAsync(header, companyId, ct), null);
    }

    public async Task<(InventJournalTableDto? Journal, string? Error)> UpdateLineAsync(string journalId, long lineRecId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var header = await EditableHeaderAsync(companyId, journalId, ct);
        if (header == null) return (null, "The journal was not found or has already been posted.");
        var line = await Db.Set<InventJournalTrans>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalId == header.JournalId && x.RecId == lineRecId, ct);
        if (line == null) return (null, "The journal line was not found.");
        var validation = await ValidateLineAsync(companyId, header, input, ct);
        if (validation.Error != null) return (null, validation.Error);
        ApplyLine(line, header, input, validation.FromDimension!, validation.ToDimension);
        await _unitOfWork.CompleteAsync(ct);
        return (await MapWithLinesAsync(header, companyId, ct), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteLineAsync(string journalId, long lineRecId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var header = await EditableHeaderAsync(companyId, journalId, ct);
        if (header == null) return (false, "The journal was not found or has already been posted.");
        var line = await Db.Set<InventJournalTrans>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalId == header.JournalId && x.RecId == lineRecId, ct);
        if (line == null) return (false, "The journal line was not found.");
        Db.Remove(line);
        header.NumOfLines = Math.Max(0, header.NumOfLines - 1);
        await _unitOfWork.CompleteAsync(ct);
        return (true, null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(string journalId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var header = await EditableHeaderAsync(companyId, journalId, ct);
        if (header == null) return (false, "The journal was not found or has already been posted.");
        var lines = await Db.Set<InventJournalTrans>().Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId).ToListAsync(ct);
        Db.RemoveRange(lines);
        Db.Remove(header);
        await _unitOfWork.CompleteAsync(ct);
        return (true, null);
    }

    private async Task<(string? Error, InventDim? FromDimension, InventDim? ToDimension)> ValidateLineAsync(
        string companyId, InventJournalTable header, InventJournalLineInputDto input, CancellationToken ct)
    {
        var itemId = Normalize(input.ItemId);
        if (itemId.Length == 0) return ("Item number is required.", null, null);
        if (!await Db.Set<ReleasedItem>().AnyAsync(x => x.DataAreaId == companyId && x.ItemId == itemId, ct))
            return ("The selected released product does not exist in the selected company.", null, null);
        var fromError = await ValidateSiteWarehouseAsync(companyId, input.InventSiteId, input.InventLocationId, true, ct);
        if (fromError != null) return (fromError, null, null);
        if (string.IsNullOrWhiteSpace(input.Unit) || !await Db.Set<UnitOfMeasure>().IgnoreQueryFilters().AnyAsync(x => !x.IsDeleted && x.Symbol == input.Unit.Trim(), ct))
            return ("A valid unit of measure is required.", null, null);
        if (header.JournalType == InventJournalType.Counting)
        {
            if (input.Counted < 0) return ("Counted quantity cannot be negative.", null, null);
        }
        else if (input.Qty == 0)
            return ("Quantity must not be zero.", null, null);
        if (header.JournalType == InventJournalType.Transfer && input.Qty <= 0)
            return ("Transfer quantity must be greater than zero.", null, null);
        if (header.JournalType == InventJournalType.Movement && input.LedgerDimension == 0)
            return ("Movement journal lines require an offset ledger account.", null, null);

        var trackingError = await ValidateTrackingDimensionsAsync(companyId, itemId, input, ct);
        if (trackingError != null) return (trackingError, null, null);

        if (header.JournalType == InventJournalType.Transfer)
        {
            var toError = await ValidateSiteWarehouseAsync(companyId, input.ToInventSiteId, input.ToInventLocationId, true, ct);
            if (toError != null) return (toError, null, null);
            if (Normalize(input.InventSiteId) == Normalize(input.ToInventSiteId) && Normalize(input.InventLocationId) == Normalize(input.ToInventLocationId))
                return ("Transfer source and destination dimensions must be different.", null, null);
        }
        var trackingTemplate = new InventDim
        {
            InventBatchId = Normalize(input.InventBatchId),
            InventSerialId = Normalize(input.InventSerialId)
        };
        var from = await _dimensions.ResolveAsync(companyId, Normalize(input.InventSiteId), Normalize(input.InventLocationId), ct, trackingTemplate);
        InventDim? to = null;
        if (header.JournalType == InventJournalType.Transfer)
            to = await _dimensions.ResolveAsync(companyId, Normalize(input.ToInventSiteId), Normalize(input.ToInventLocationId), ct, trackingTemplate);
        return (null, from, to);
    }

    private async Task<string?> ValidateTrackingDimensionsAsync(string companyId, string itemId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var assignment = await Db.Set<EcoResTrackingDimensionGroupItem>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == itemId, ct);
        if (assignment == null) return "A tracking dimension group is required before entering inventory journal lines.";

        var setup = await Db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
            .Where(x => x.TrackingDimensionGroup == assignment.TrackingDimensionGroup &&
                (x.DimensionFieldId == InventDimensionFields.BatchNumber || x.DimensionFieldId == InventDimensionFields.SerialNumber))
            .ToListAsync(ct);
        var batchSetup = setup.FirstOrDefault(x => x.DimensionFieldId == InventDimensionFields.BatchNumber);
        var serialSetup = setup.FirstOrDefault(x => x.DimensionFieldId == InventDimensionFields.SerialNumber);
        var batchId = Normalize(input.InventBatchId);
        var serialId = Normalize(input.InventSerialId);

        if (batchSetup?.IsPhysicalInventoryEnabled == true && batchId.Length == 0 &&
            !batchSetup.IsAllowBlankReceiptEnabled && !batchSetup.IsAllowBlankIssueEnabled)
            return "A batch number is required by the item's tracking dimension group.";
        if (serialSetup?.IsPhysicalInventoryEnabled == true && serialId.Length == 0 &&
            !serialSetup.IsAllowBlankReceiptEnabled && !serialSetup.IsAllowBlankIssueEnabled)
            return "A serial number is required by the item's tracking dimension group.";
        if (batchId.Length > 0 && !await Db.Set<InventBatch>().AnyAsync(x => x.ItemId == itemId && x.InventBatchId == batchId, ct))
            return $"Batch '{batchId}' is not defined for item '{itemId}'.";
        if (serialId.Length > 0 && !await Db.Set<InventSerial>().AnyAsync(x => x.ItemId == itemId && x.InventSerialId == serialId, ct))
            return $"Serial number '{serialId}' is not defined for item '{itemId}'.";
        if (serialSetup?.IsPhysicalInventoryEnabled == true &&
            (Math.Abs(input.Qty) > 1m || input.Counted > 1m))
            return "A serial-tracked journal line cannot contain more than one serial-numbered unit.";
        return null;
    }

    private async Task<string?> ValidateSiteWarehouseAsync(string companyId, string siteId, string warehouseId, bool required, CancellationToken ct)
    {
        siteId = Normalize(siteId);
        warehouseId = Normalize(warehouseId);
        if (!required && siteId.Length == 0 && warehouseId.Length == 0) return null;
        if (siteId.Length == 0 || warehouseId.Length == 0) return "Site and warehouse must be entered together.";
        var valid = await Db.Set<InventLocation>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId &&
            x.InventLocationId == warehouseId && x.InventSiteId == siteId, ct);
        return valid ? null : $"Warehouse '{warehouseId}' does not belong to site '{siteId}' in the selected company.";
    }

    private Task<InventJournalTable?> EditableHeaderAsync(string companyId, string journalId, CancellationToken ct) =>
        Db.Set<InventJournalTable>().FirstOrDefaultAsync(x => x.DataAreaId == companyId &&
            x.JournalId == Normalize(journalId) && x.Posted == NoYes.No, ct);

    private static void ApplyLine(InventJournalTrans line, InventJournalTable header, InventJournalLineInputDto input, InventDim from, InventDim? to)
    {
        line.TransDate = input.TransDate == default ? DateTime.Today : input.TransDate.Date;
        line.ItemId = Normalize(input.ItemId);
        line.JournalType = header.JournalType;
        line.InventDimId = from.InventDimId;
        line.ToInventDimId = to?.InventDimId ?? string.Empty;
        line.Qty = input.Qty;
        line.Counted = input.Counted;
        line.Unit = input.Unit.Trim();
        line.UnitQty = 1m;
        line.CostPrice = input.CostPrice;
        line.PriceUnit = 1m;
        line.CostAmount = header.JournalType == InventJournalType.Counting ? 0m : input.Qty * input.CostPrice;
        line.LedgerDimension = input.LedgerDimension;
        line.InventRefType = header.JournalType == InventJournalType.Counting
            ? InventRefType.PhysicalinventoryCount : InventRefType.InventoryJournal;
        line.Voucher = string.Empty;
        line.InventTransId = string.Empty;
        line.ToInventTransId = string.Empty;
        line.InventTransIdFather = string.Empty;
        line.ProjId = string.Empty;
        line.ProjCategoryId = string.Empty;
        line.ProjLinePropertyId = string.Empty;
        line.ProjSalesCurrencyId = string.Empty;
        line.ProjUnitId = string.Empty;
        line.ProjTransId = string.Empty;
        line.ProjTaxGroupId = string.Empty;
        line.ProjTaxItemGroupId = string.Empty;
        line.ProdGroupId = string.Empty;
    }

    private async Task<InventJournalTableDto> MapWithLinesAsync(InventJournalTable header, string companyId, CancellationToken ct)
    {
        var lines = (await Db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId).ToListAsync(ct))
            .OrderBy(x => x.LineNum).ToList();
        var dimIds = lines.SelectMany(x => new[] { x.InventDimId, x.ToInventDimId }).Where(x => x != string.Empty).Distinct().ToList();
        var dims = await Db.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == companyId && dimIds.Contains(x.InventDimId))
            .ToDictionaryAsync(x => x.InventDimId, ct);
        var dto = MapHeader(header);
        dto.Lines = lines.Select(x => MapLine(x, dims)).ToList();
        return dto;
    }

    private static InventJournalTableDto MapHeader(InventJournalTable x) => new()
    {
        RecId = x.RecId, JournalId = x.JournalId, JournalNameId = x.JournalNameId, Description = x.Description,
        JournalType = x.JournalType, NumOfLines = x.NumOfLines, Posted = x.Posted == NoYes.Yes,
        PostedDateTime = x.Posted == NoYes.Yes ? x.PostedDateTime : null,
        InventSiteId = x.InventSiteId, InventLocationId = x.InventLocationId
    };

    private static InventJournalTransDto MapLine(InventJournalTrans x, IReadOnlyDictionary<string, InventDim> dims)
    {
        dims.TryGetValue(x.InventDimId, out var from);
        dims.TryGetValue(x.ToInventDimId, out var to);
        return new InventJournalTransDto
        {
            RecId = x.RecId, LineNum = x.LineNum, TransDate = x.TransDate, ItemId = x.ItemId,
            InventDimId = x.InventDimId, ToInventDimId = x.ToInventDimId,
            InventSiteId = from?.InventSiteId ?? string.Empty, InventLocationId = from?.InventLocationId ?? string.Empty,
            ToInventSiteId = to?.InventSiteId ?? string.Empty, ToInventLocationId = to?.InventLocationId ?? string.Empty,
            Qty = x.Qty, InventOnHand = x.InventOnHand, Counted = x.Counted, Unit = x.Unit,
            InventBatchId = from?.InventBatchId ?? string.Empty, InventSerialId = from?.InventSerialId ?? string.Empty,
            CostPrice = x.CostPrice, CostAmount = x.CostAmount, LedgerDimension = x.LedgerDimension
        };
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private string RequireCompany() => _company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
}
