using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventLocation")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventLocationController : ControllerBase
{
    private readonly IFinanceDataContext _db;
    private readonly ICompanyExecutionContext _company;
    public InventLocationController(IFinanceDataContext db, ICompanyExecutionContext company) { _db = db; _company = company; }

    public sealed class LocationInput
    {
        [Required, StringLength(FieldLengths.InventLocationId)] public string InventLocationId { get; set; } = string.Empty;
        [Required, StringLength(FieldLengths.Name)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(FieldLengths.InventSiteId)] public string InventSiteId { get; set; } = string.Empty;
        public int InventLocationType { get; set; }
        public int InventLocationLevel { get; set; }
        public string InventLocationIdTransit { get; set; } = string.Empty;
        public string InventLocationIdQuarantine { get; set; } = string.Empty;
        public string InventLocationIdReqMain { get; set; } = string.Empty;
        public string ItmInventLocationIdGit { get; set; } = string.Empty;
        public string ItmInventLocationIdUnder { get; set; } = string.Empty;
        public string VendAccount { get; set; } = string.Empty;
        public bool WorkflowApproval { get; set; }
        public bool Manual { get; set; }
        public bool ReqRefill { get; set; }
        public string WmsLocationIdDefaultReceipt { get; set; } = string.Empty;
        public string WmsLocationIdDefaultIssue { get; set; } = string.Empty;
        public string DefaultProductionInputLocation { get; set; } = string.Empty;
        public string DefaultProductionFinishGoodsLocation { get; set; } = string.Empty;
        public string DefaultKanbanFinishedGoodsLocation { get; set; } = string.Empty;
        public string DefaultReturnCreditOnlyLocation { get; set; } = string.Empty;
        public string DefaultStatusId { get; set; } = string.Empty;
        public string WmsRackFormat { get; set; } = string.Empty;
        public string WmsLevelFormat { get; set; } = string.Empty;
        public string WmsPositionFormat { get; set; } = string.Empty;
        public bool WhsEnabled { get; set; }
        public bool WarehouseAutoReleaseReservation { get; set; }
        public bool AutoUpdateShipment { get; set; }
        public bool ReserveAtLoadPost { get; set; }
        public bool DecrementLoadLine { get; set; }
        public bool PrintBolBeforeShipConfirm { get; set; }
        public bool CycleCountAllowPalletMove { get; set; }
        public bool AllowLaborStandards { get; set; }
        public bool AllowMarkingReservationRemoval { get; set; }
        public bool UseWmsOrders { get; set; }
        public bool WmsAisleNameActive { get; set; }
        public bool WmsRackNameActive { get; set; }
        public bool WmsLevelNameActive { get; set; }
        public bool WmsPositionNameActive { get; set; }
        public bool UniqueCheckDigits { get; set; }
        public bool EnableQualityManagement { get; set; }
        public bool RemoveInventBlockingOnStatusChange { get; set; }
        public bool ProdReserveOnlyWhse { get; set; }
        public bool WhsProdOrderBackflushMustUseReservedQty { get; set; }
        public bool FshStore { get; set; }
        public bool ConsolidateShipAtRtw { get; set; }
        public bool RetailInventNegPhysical { get; set; }
        public bool RetailInventNegFinancial { get; set; }
        public bool EnableExternalWarehouse { get; set; }
        public int MaxPickingRouteTime { get; set; }
        public int PickingLineTime { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var locations = await _db.Set<InventLocation>().AsNoTracking()
            .OrderBy(x => x.InventLocationId).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(locations.Select(Map)));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct)
    {
        var sites = await _db.Set<InventSite>().AsNoTracking().OrderBy(x => x.SiteId)
            .Select(x => new { id = x.SiteId, code = x.SiteId, name = x.Name }).ToListAsync(ct);
        var warehouses = await _db.Set<InventLocation>().AsNoTracking().OrderBy(x => x.InventLocationId)
            .Select(x => new { id = x.InventLocationId, code = x.InventLocationId, name = x.Name, siteId = x.InventSiteId }).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(new { sites, warehouses }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(LocationInput input, CancellationToken ct)
    {
        var code = input.InventLocationId.Trim().ToUpperInvariant();
        Normalize(input);
        if (await _db.Set<InventLocation>().AnyAsync(x => x.InventLocationId == code, ct))
            return Conflict(APIResponse<object>.Fail("A warehouse with this ID already exists."));
        if (!await _db.Set<InventSite>().AnyAsync(x => x.SiteId == input.InventSiteId, ct))
            return UnprocessableEntity(APIResponse<object>.Fail("The selected site was not found."));
        var referenceError = await ValidateWarehouseReferences(input, code, ct);
        if (referenceError != null) return UnprocessableEntity(APIResponse<object>.Fail(referenceError));
        var location = Apply(new InventLocation { InventLocationId = code, DataAreaId = _company.GetDataAreaId() ?? "dat" }, input);
        _db.Set<InventLocation>().Add(location);
        await _db.SaveChangesAsync(ct);
        return Ok(APIResponse<object>.Ok(Map(location)));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, LocationInput input, CancellationToken ct)
    {
        var location = await _db.Set<InventLocation>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (location == null) return NotFound(APIResponse<object>.Fail("Warehouse was not found."));
        Normalize(input);
        if (!string.Equals(location.InventLocationId, input.InventLocationId.Trim(), StringComparison.OrdinalIgnoreCase))
            return UnprocessableEntity(APIResponse<object>.Fail("Warehouse ID cannot be changed after creation."));
        if (!await _db.Set<InventSite>().AnyAsync(x => x.SiteId == input.InventSiteId, ct))
            return UnprocessableEntity(APIResponse<object>.Fail("The selected site was not found."));
        var referenceError = await ValidateWarehouseReferences(input, location.InventLocationId, ct);
        if (referenceError != null) return UnprocessableEntity(APIResponse<object>.Fail(referenceError));
        Apply(location, input); await _db.SaveChangesAsync(ct);
        return Ok(APIResponse<object>.Ok(Map(location)));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var location = await _db.Set<InventLocation>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (location == null) return NotFound(APIResponse<object>.Fail("Warehouse was not found."));
        var code = location.InventLocationId;
        if (await _db.Set<InventLocation>().AnyAsync(x => x.RecId != recId &&
            (x.InventLocationIdTransit == code || x.InventLocationIdQuarantine == code ||
             x.InventLocationIdReqMain == code || x.ItmInventLocationIdGit == code ||
             x.ItmInventLocationIdUnder == code), ct))
            return Conflict(APIResponse<object>.Fail("This warehouse is referenced by another warehouse."));
        _db.Set<InventLocation>().Remove(location); await _db.SaveChangesAsync(ct);
        return Ok(APIResponse<bool>.Ok(true));
    }

    private static InventLocation Apply(InventLocation x, LocationInput i)
    {
        x.Name=i.Name.Trim(); x.InventSiteId=i.InventSiteId.Trim(); x.InventLocationType=(InventLocationType)i.InventLocationType;
        x.InventLocationLevel=i.InventLocationLevel; x.InventLocationIdTransit=i.InventLocationIdTransit.Trim();
        x.InventLocationIdQuarantine=i.InventLocationIdQuarantine.Trim(); x.InventLocationIdReqMain=i.InventLocationIdReqMain.Trim();
        x.ItmInventLocationIdGit=i.ItmInventLocationIdGit.Trim(); x.ItmInventLocationIdUnder=i.ItmInventLocationIdUnder.Trim(); x.VendAccount=i.VendAccount.Trim();
        x.WorkflowApproval=i.WorkflowApproval?NoYes.Yes:NoYes.No; x.Manual=i.Manual?NoYes.Yes:NoYes.No; x.ReqRefill=i.ReqRefill?NoYes.Yes:NoYes.No;
        x.WmsLocationIdDefaultReceipt=i.WmsLocationIdDefaultReceipt.Trim(); x.WmsLocationIdDefaultIssue=i.WmsLocationIdDefaultIssue.Trim();
        x.DefaultProductionInputLocation=i.DefaultProductionInputLocation.Trim(); x.DefaultProductionFinishGoodsLocation=i.DefaultProductionFinishGoodsLocation.Trim();
        x.DefaultKanbanFinishedGoodsLocation=i.DefaultKanbanFinishedGoodsLocation.Trim(); x.DefaultReturnCreditOnlyLocation=i.DefaultReturnCreditOnlyLocation.Trim();
        x.DefaultStatusID=i.DefaultStatusId.Trim(); x.WmsRackFormat=i.WmsRackFormat.Trim(); x.WmsLevelFormat=i.WmsLevelFormat.Trim(); x.WmsPositionFormat=i.WmsPositionFormat.Trim();
        x.WhsEnabled=Yes(i.WhsEnabled); x.WarehouseAutoReleaseReservation=Yes(i.WarehouseAutoReleaseReservation); x.AutoUpdateShipment=Yes(i.AutoUpdateShipment);
        x.ReserveAtLoadPost=Yes(i.ReserveAtLoadPost); x.DecrementLoadLine=Yes(i.DecrementLoadLine); x.PrintBolBeforeShipConfirm=Yes(i.PrintBolBeforeShipConfirm);
        x.CycleCountAllowPalletMove=Yes(i.CycleCountAllowPalletMove); x.AllowLaborStandards=Yes(i.AllowLaborStandards); x.AllowMarkingReservationRemoval=Yes(i.AllowMarkingReservationRemoval);
        x.UseWmsOrders=Yes(i.UseWmsOrders); x.WmsAisleNameActive=Yes(i.WmsAisleNameActive); x.WmsRackNameActive=Yes(i.WmsRackNameActive);
        x.WmsLevelNameActive=Yes(i.WmsLevelNameActive); x.WmsPositionNameActive=Yes(i.WmsPositionNameActive); x.UniqueCheckDigits=Yes(i.UniqueCheckDigits);
        x.EnableQualityManagement=Yes(i.EnableQualityManagement); x.RemoveInventBlockingOnStatusChange=Yes(i.RemoveInventBlockingOnStatusChange);
        x.ProdReserveOnlyWhse=Yes(i.ProdReserveOnlyWhse); x.WhsProdOrderBackflushMustUseReservedQty=Yes(i.WhsProdOrderBackflushMustUseReservedQty);
        x.FshStore=Yes(i.FshStore); x.ConsolidateShipAtRtw=Yes(i.ConsolidateShipAtRtw); x.RetailInventNegPhysical=Yes(i.RetailInventNegPhysical);
        x.RetailInventNegFinancial=Yes(i.RetailInventNegFinancial); x.EnableExternalWarehouse=Yes(i.EnableExternalWarehouse);
        x.MaxPickingRouteTime=i.MaxPickingRouteTime; x.PickingLineTime=i.PickingLineTime;
        return x;
    }

    private static void Normalize(LocationInput input)
    {
        input.InventSiteId = input.InventSiteId.Trim().ToUpperInvariant();
        input.InventLocationIdTransit = input.InventLocationIdTransit.Trim().ToUpperInvariant();
        input.InventLocationIdQuarantine = input.InventLocationIdQuarantine.Trim().ToUpperInvariant();
        input.InventLocationIdReqMain = input.InventLocationIdReqMain.Trim().ToUpperInvariant();
        input.ItmInventLocationIdGit = input.ItmInventLocationIdGit.Trim().ToUpperInvariant();
        input.ItmInventLocationIdUnder = input.ItmInventLocationIdUnder.Trim().ToUpperInvariant();
    }

    private async Task<string?> ValidateWarehouseReferences(LocationInput input, string currentCode, CancellationToken ct)
    {
        var references = new[]
        {
            input.InventLocationIdTransit,
            input.InventLocationIdQuarantine,
            input.InventLocationIdReqMain,
            input.ItmInventLocationIdGit,
            input.ItmInventLocationIdUnder
        }.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        if (references.Any(x => string.Equals(x, currentCode, StringComparison.OrdinalIgnoreCase)))
            return "A warehouse cannot reference itself.";
        if (references.Length == 0) return null;

        var existing = await _db.Set<InventLocation>().AsNoTracking()
            .Where(x => references.Contains(x.InventLocationId) && x.InventSiteId == input.InventSiteId)
            .Select(x => x.InventLocationId).ToListAsync(ct);
        var missing = references.Except(existing, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        return missing == null ? null : $"Warehouse '{missing}' was not found in site '{input.InventSiteId}'.";
    }

    private static NoYes Yes(bool value) => value ? NoYes.Yes : NoYes.No;
    private static object Map(InventLocation x) => new { id=x.RecId.ToString(), x.RecId, x.InventLocationId, x.Name, x.InventSiteId,
        inventLocationType=(int)x.InventLocationType, x.InventLocationLevel, x.InventLocationIdTransit, x.InventLocationIdQuarantine,
        x.InventLocationIdReqMain, x.ItmInventLocationIdGit, x.ItmInventLocationIdUnder, x.VendAccount,
        workflowApproval=x.WorkflowApproval==NoYes.Yes, manual=x.Manual==NoYes.Yes, reqRefill=x.ReqRefill==NoYes.Yes,
        x.WmsLocationIdDefaultReceipt, x.WmsLocationIdDefaultIssue, x.DefaultProductionInputLocation,
        x.DefaultProductionFinishGoodsLocation, x.DefaultKanbanFinishedGoodsLocation, x.DefaultReturnCreditOnlyLocation,
        defaultStatusId=x.DefaultStatusID, x.WmsRackFormat, x.WmsLevelFormat, x.WmsPositionFormat,
        whsEnabled=x.WhsEnabled==NoYes.Yes, warehouseAutoReleaseReservation=x.WarehouseAutoReleaseReservation==NoYes.Yes,
        autoUpdateShipment=x.AutoUpdateShipment==NoYes.Yes, reserveAtLoadPost=x.ReserveAtLoadPost==NoYes.Yes,
        decrementLoadLine=x.DecrementLoadLine==NoYes.Yes, printBolBeforeShipConfirm=x.PrintBolBeforeShipConfirm==NoYes.Yes,
        cycleCountAllowPalletMove=x.CycleCountAllowPalletMove==NoYes.Yes, allowLaborStandards=x.AllowLaborStandards==NoYes.Yes,
        allowMarkingReservationRemoval=x.AllowMarkingReservationRemoval==NoYes.Yes, useWmsOrders=x.UseWmsOrders==NoYes.Yes,
        wmsAisleNameActive=x.WmsAisleNameActive==NoYes.Yes, wmsRackNameActive=x.WmsRackNameActive==NoYes.Yes,
        wmsLevelNameActive=x.WmsLevelNameActive==NoYes.Yes, wmsPositionNameActive=x.WmsPositionNameActive==NoYes.Yes,
        uniqueCheckDigits=x.UniqueCheckDigits==NoYes.Yes, enableQualityManagement=x.EnableQualityManagement==NoYes.Yes,
        removeInventBlockingOnStatusChange=x.RemoveInventBlockingOnStatusChange==NoYes.Yes, prodReserveOnlyWhse=x.ProdReserveOnlyWhse==NoYes.Yes,
        whsProdOrderBackflushMustUseReservedQty=x.WhsProdOrderBackflushMustUseReservedQty==NoYes.Yes,
        fshStore=x.FshStore==NoYes.Yes, consolidateShipAtRtw=x.ConsolidateShipAtRtw==NoYes.Yes,
        retailInventNegPhysical=x.RetailInventNegPhysical==NoYes.Yes, retailInventNegFinancial=x.RetailInventNegFinancial==NoYes.Yes,
        enableExternalWarehouse=x.EnableExternalWarehouse==NoYes.Yes, x.MaxPickingRouteTime, x.PickingLineTime };
}
