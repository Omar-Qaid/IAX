using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Shared.Features;

[ApiController]
[Route("api/v1/MarkupTrans")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class MarkupTransController : ControllerBase
{
    // Keep aligned with IXApp shared document table registry.
    private const int SalesTableId = 2002;
    private const int SalesLineId = 2003;

    private readonly IFinanceDataContext _db;
    private readonly ICompanyExecutionContext _company;

    public MarkupTransController(IFinanceDataContext db, ICompanyExecutionContext company)
    {
        _db = db;
        _company = company;
    }

    [HttpGet("document/{documentType}/{documentRecId:long}")]
    public async Task<IActionResult> GetDocumentCharges(
        string documentType,
        long documentRecId,
        [FromQuery] string level = "header",
        CancellationToken cancellationToken = default)
    {
        var moduleType = ParseDocumentType(documentType);
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (moduleType == null)
            return BadRequest(APIResponse<object>.Fail("Document type must be sales or purchase."));
        var transTableId = ResolveTransTableId(moduleType.Value, level);
        if (transTableId == null)
            return BadRequest(APIResponse<object>.Fail("Charge level must be header or line."));

        var targetError = await ValidateTarget(moduleType.Value, transTableId.Value, documentRecId, dataAreaId, cancellationToken);
        if (targetError != null) return NotFound(APIResponse<object>.Fail(targetError));

        var entities = await _db.Set<MarkupTrans>()
            .AsNoTracking()
            .Where(x => x.TransRecId == documentRecId
                && (moduleType == MarkupModuleType.Customer
                    ? x.ModuleType == MarkupModuleType.Customer || x.ModuleType == MarkupModuleType.Sales
                    : x.ModuleType == moduleType)
                && (x.TransTableId == transTableId
                    || (transTableId == SalesTableId && x.TransTableId == 0))
                && x.DataAreaId == dataAreaId)
            .OrderBy(x => x.LineNum)
            .ToListAsync(cancellationToken);
        var records = entities.Select(ToDto).ToList();

        return Ok(APIResponse<IEnumerable<MarkupTransDto>>.Ok(records));
    }

    [HttpGet("codes/{documentType}")]
    public async Task<IActionResult> GetChargeCodes(string documentType, CancellationToken cancellationToken = default)
    {
        var moduleType = ParseDocumentType(documentType);
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (moduleType == null)
            return BadRequest(APIResponse<object>.Fail("Document type must be sales or purchase."));

        var setupModule = moduleType == MarkupModuleType.Customer
            ? ModuleInventPurchSales.Sales
            : ModuleInventPurchSales.Purchase;
        var codes = await _db.Set<MarkupTable>().AsNoTracking()
            .Where(x => x.ModuleType == setupModule && x.DataAreaId == dataAreaId)
            .OrderBy(x => x.MarkupCode)
            .Select(x => new { x.MarkupCode, x.Txt, x.TaxItemGroup, x.McrBrokerContractFee })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(codes));
    }

    [HttpPost("document/{documentType}/{documentRecId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Create(
        string documentType,
        long documentRecId,
        [FromBody] MarkupTransDto input,
        [FromQuery] string level = "header",
        CancellationToken cancellationToken = default)
    {
        var moduleType = ParseDocumentType(documentType);
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (moduleType == null)
            return BadRequest(APIResponse<MarkupTransDto>.Fail("Document type must be sales or purchase."));
        var transTableId = ResolveTransTableId(moduleType.Value, level);
        if (transTableId == null)
            return BadRequest(APIResponse<MarkupTransDto>.Fail("Charge level must be header or line."));
        var validation = await ValidateInput(moduleType.Value, transTableId.Value, documentRecId, input, cancellationToken);
        if (validation != null) return validation;

        var nextLine = await _db.Set<MarkupTrans>()
            .Where(x => x.TransRecId == documentRecId
                && (moduleType == MarkupModuleType.Customer
                    ? x.ModuleType == MarkupModuleType.Customer || x.ModuleType == MarkupModuleType.Sales
                    : x.ModuleType == moduleType)
                && (x.TransTableId == transTableId
                    || (transTableId == SalesTableId && x.TransTableId == 0))
                && x.DataAreaId == dataAreaId)
            .Select(x => (decimal?)x.LineNum).MaxAsync(cancellationToken) ?? 0;
        var entity = new MarkupTrans
        {
            DataAreaId = dataAreaId,
            TransRecId = documentRecId,
            TransTableId = transTableId.Value,
            LineNum = nextLine + 1,
            TransDate = input.TransDate == default ? DateTime.UtcNow.Date : input.TransDate,
            ModuleType = moduleType.Value
        };
        ApplyInput(entity, input);
        entity.CalculatedAmount = await CalculateAmount(entity, cancellationToken);
        await _db.Set<MarkupTrans>().AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(APIResponse<MarkupTransDto>.Ok(ToDto(entity)));
    }

    [HttpPut("{id:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] MarkupTransDto input, CancellationToken cancellationToken = default)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        var entity = await _db.Set<MarkupTrans>().FirstOrDefaultAsync(
            x => x.RecId == id && x.DataAreaId == dataAreaId,
            cancellationToken);
        if (entity == null) return NotFound(APIResponse<MarkupTransDto>.Fail("Charge transaction not found."));
        var validation = await ValidateInput(entity.ModuleType, entity.TransTableId, entity.TransRecId, input, cancellationToken);
        if (validation != null) return validation;
        ApplyInput(entity, input);
        entity.CalculatedAmount = await CalculateAmount(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(APIResponse<MarkupTransDto>.Ok(ToDto(entity)));
    }

    [HttpDelete("{id:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken = default)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        var entity = await _db.Set<MarkupTrans>().FirstOrDefaultAsync(
            x => x.RecId == id && x.DataAreaId == dataAreaId,
            cancellationToken);
        if (entity == null) return NotFound(APIResponse<bool>.Fail("Charge transaction not found."));
        _db.Set<MarkupTrans>().Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(APIResponse<bool>.Ok(true));
    }

    private async Task<ActionResult?> ValidateInput(MarkupModuleType moduleType, int transTableId, long documentRecId, MarkupTransDto input, CancellationToken ct)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (string.IsNullOrWhiteSpace(input.MarkupCode))
            return BadRequest(APIResponse<MarkupTransDto>.Fail("Charges code is required."));
        if (input.Value < 0)
            return BadRequest(APIResponse<MarkupTransDto>.Fail("Charges value cannot be negative."));
        if (input.MarkupCategory == MarkupCategory.Percent && input.Value > 100)
            return BadRequest(APIResponse<MarkupTransDto>.Fail("Percentage charges value cannot exceed 100."));
        var isSalesCharge = moduleType is MarkupModuleType.Customer or MarkupModuleType.Sales;
        var setupModule = isSalesCharge ? ModuleInventPurchSales.Sales : ModuleInventPurchSales.Purchase;
        if (!await _db.Set<MarkupTable>().AsNoTracking().AnyAsync(
                x => x.MarkupCode == input.MarkupCode
                    && x.ModuleType == setupModule
                    && x.DataAreaId == dataAreaId,
                ct))
            return BadRequest(APIResponse<MarkupTransDto>.Fail("The selected charges code is not valid for this document type."));
        var targetError = await ValidateTarget(moduleType, transTableId, documentRecId, dataAreaId, ct);
        if (targetError != null)
            return NotFound(APIResponse<MarkupTransDto>.Fail(targetError));
        return null;
    }

    private async Task<decimal> CalculateAmount(MarkupTrans charge, CancellationToken ct)
    {
        if (charge.MarkupCategory == MarkupCategory.Fixed)
            return charge.Value;

        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (charge.TransTableId == SalesLineId)
        {
            var line = await _db.Set<SalesLine>().AsNoTracking()
                .Where(x => x.RecId == charge.TransRecId && x.DataAreaId == dataAreaId)
                .Select(x => new { x.SalesQty, x.LineAmount, x.LineDisc, x.MultiLnDisc })
                .SingleAsync(ct);
            if (charge.MarkupCategory == MarkupCategory.Pcs)
                return decimal.Round(charge.Value * line.SalesQty, 2, MidpointRounding.AwayFromZero);

            var netAmount = Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc);
            return decimal.Round(netAmount * charge.Value / 100m, 2, MidpointRounding.AwayFromZero);
        }

        if (charge.TransTableId == SalesTableId || charge.TransTableId == 0)
        {
            var order = await _db.Set<SalesTable>().AsNoTracking()
                .Where(x => x.RecId == charge.TransRecId && x.DataAreaId == dataAreaId)
                .Select(x => x.SalesId)
                .SingleAsync(ct);
            var lines = await _db.Set<SalesLine>().AsNoTracking()
                .Where(x => x.SalesId == order && x.DataAreaId == dataAreaId)
                .Select(x => new { x.SalesQty, x.LineAmount, x.LineDisc, x.MultiLnDisc })
                .ToListAsync(ct);
            if (charge.MarkupCategory == MarkupCategory.Pcs)
                return decimal.Round(charge.Value * lines.Sum(x => x.SalesQty), 2, MidpointRounding.AwayFromZero);

            var netAmount = lines.Sum(x => Math.Max(0m, x.LineAmount - x.LineDisc - x.MultiLnDisc));
            return decimal.Round(netAmount * charge.Value / 100m, 2, MidpointRounding.AwayFromZero);
        }

        return charge.Value;
    }

    private async Task<string?> ValidateTarget(
        MarkupModuleType moduleType,
        int transTableId,
        long documentRecId,
        string dataAreaId,
        CancellationToken ct)
    {
        if (moduleType is not (MarkupModuleType.Customer or MarkupModuleType.Sales)) return null;

        if (transTableId == SalesLineId)
            return await _db.Set<SalesLine>().AsNoTracking().AnyAsync(
                x => x.RecId == documentRecId && x.DataAreaId == dataAreaId,
                ct)
                ? null
                : "Sales order line not found.";

        return await _db.Set<SalesTable>().AsNoTracking().AnyAsync(
            x => x.RecId == documentRecId && x.DataAreaId == dataAreaId,
            ct)
            ? null
            : "Sales order not found.";
    }

    private static void ApplyInput(MarkupTrans entity, MarkupTransDto input)
    {
        entity.MarkupCode = input.MarkupCode.Trim();
        entity.Txt = input.Txt?.Trim() ?? string.Empty;
        entity.MarkupCategory = input.MarkupCategory;
        entity.CurrencyCode = input.CurrencyCode?.Trim().ToUpperInvariant() ?? string.Empty;
        entity.Value = input.Value;
        entity.Keep = input.Keep;
        entity.McrBrokerContractFee = input.McrBrokerContractFee;
        entity.TaxGroup = input.TaxGroup?.Trim().ToUpperInvariant() ?? string.Empty;
        entity.TaxItemGroup = input.TaxItemGroup?.Trim().ToUpperInvariant() ?? string.Empty;
        entity.Voucher = input.Voucher?.Trim() ?? string.Empty;
        entity.CalculatedAmount = input.CalculatedAmount;
        entity.IntercompanyRefRecId = input.IntercompanyRefRecId;
        entity.IntercompanyMarkupValue = input.IntercompanyMarkupValue;
        entity.IsModified = NoYes.Yes;
    }

    private static MarkupModuleType? ParseDocumentType(string value) => value.ToLowerInvariant() switch
    {
        "sales" => MarkupModuleType.Customer,
        "purchase" => MarkupModuleType.Vendor,
        _ => null
    };

    private static int? ResolveTransTableId(MarkupModuleType moduleType, string level) => (moduleType, level.ToLowerInvariant()) switch
    {
        (MarkupModuleType.Customer or MarkupModuleType.Sales, "header") => SalesTableId,
        (MarkupModuleType.Customer or MarkupModuleType.Sales, "line") => SalesLineId,
        (MarkupModuleType.Vendor, "header") => 0,
        _ => null
    };

    private static MarkupTransDto ToDto(MarkupTrans x) => new()
    {
        RecId = x.RecId,
        DataAreaId = x.DataAreaId,
        MarkupCode = x.MarkupCode,
        LineNum = x.LineNum,
        TransDate = x.TransDate,
        Txt = x.Txt,
        Voucher = x.Voucher,
        MarkupCategory = x.MarkupCategory,
        ModuleCategory = x.ModuleCategory,
        ModuleType = x.ModuleType,
        TransRecId = x.TransRecId,
        TransTableId = x.TransTableId,
        OrigRecId = x.OrigRecId,
        OrigTableId = x.OrigTableId,
        CurrencyCode = x.CurrencyCode,
        Value = x.Value,
        Posted = x.Posted,
        PreviousValue = x.PreviousValue,
        CalculatedAmount = x.CalculatedAmount,
        CalculatedProratedAmount = x.CalculatedProratedAmount,
        FromAmount = x.FromAmount,
        ToAmount = x.ToAmount,
        Keep = x.Keep,
        IsCompound = x.IsCompound,
        IsAutoCharge = x.IsAutoCharge,
        IsTieredCharge = x.IsTieredCharge,
        TaxGroup = x.TaxGroup,
        TaxItemGroup = x.TaxItemGroup,
        TaxAmount = x.TaxAmount,
        OverrideSalesTax = x.OverrideSalesTax,
        TaxAutoGenerated = x.TaxAutoGenerated,
        McrBrokerContractFee = x.McrBrokerContractFee,
        IntercompanyRefRecId = x.IntercompanyRefRecId,
        IntercompanyMarkupValue = x.IntercompanyMarkupValue,
        DocumentStatus = x.DocumentStatus
    };
}
