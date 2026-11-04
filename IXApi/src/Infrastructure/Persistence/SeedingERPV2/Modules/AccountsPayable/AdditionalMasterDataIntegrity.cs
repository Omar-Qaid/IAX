using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
internal static class AdditionalMasterDataIntegrity
{
    internal static async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        static void Check(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException("AP reference integrity: " + message);
        }
        var storageName = $"APV2-{c.Company}-STORE";
        var trackingName = $"APV2-{c.Company}-NONE";
        var storage = await c.RequireAsync<EcoResStorageDimensionGroup>(x => x.Name == storageName, ct);
        var tracking = await c.RequireAsync<EcoResTrackingDimensionGroup>(x => x.Name == trackingName, ct);
        var storageItem = await c.RequireAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == "APV2-STOCK" && x.ItemDataAreaId == c.Company, ct);
        var trackingItem = await c.RequireAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == "APV2-STOCK" && x.ItemDataAreaId == c.Company, ct);
        Check(storageItem.StorageDimensionGroup == storage.RecId && trackingItem.TrackingDimensionGroup == tracking.RecId, "item dimension-group assignments differ.");
        var storageFields = await c.Rows<EcoResStorageDimensionGroupFldSetup>().Where(x => x.StorageDimensionGroup == storage.RecId).ToListAsync(ct);
        Check(storageFields.Count == 3, "storage fields missing or unexpected.");
        foreach (var field in new[] { InventDimensionFields.Site, InventDimensionFields.Warehouse, InventDimensionFields.Location })
        {
            var row = storageFields.SingleOrDefault(x => x.DimensionFieldId == field);
            var active = field != InventDimensionFields.Location;
            Check(row != null && !row.IsDeleted && row.IsActive == active && row.IsPhysicalInventoryEnabled == active
                && row.IsFinancialInventoryEnabled == (field == InventDimensionFields.Site)
                && row.IsAllowBlankReceiptEnabled == !active && row.IsAllowBlankIssueEnabled == !active, "storage field policy differs.");
        }
        var trackingFields = await c.Rows<EcoResTrackingDimensionGroupFldSetup>().Where(x => x.TrackingDimensionGroup == tracking.RecId).ToListAsync(ct);
        Check(trackingFields.Count == 2 && trackingFields.Select(x => x.DimensionFieldId).Order().SequenceEqual(new[] { InventDimensionFields.BatchNumber, InventDimensionFields.SerialNumber })
            && trackingFields.All(x => !x.IsDeleted && !x.IsActive && !x.IsPhysicalInventoryEnabled && !x.IsFinancialInventoryEnabled
                && x.IsAllowBlankReceiptEnabled && x.IsAllowBlankIssueEnabled), "no-tracking field policy differs.");
        var dim = await c.RequireAsync<InventDim>(x => x.InventDimId == "APV2-DIM", ct);
        var purchase = await c.RequireAsync<InventItemPurchSetup>(x => x.ItemId == "APV2-STOCK" && x.ItemDataAreaId == c.Company && x.InventDimId == "", ct);
        var inventory = await c.RequireAsync<InventItemInventSetup>(x => x.ItemId == "APV2-STOCK" && x.ItemDataAreaId == c.Company && x.InventDimId == "", ct);
        Check(purchase.InventDimIdDefault == dim.InventDimId && inventory.InventDimIdDefault == dim.InventDimId
            && purchase.Stopped == 0 && inventory.Stopped == 0 && purchase.StandardQty == 100m && inventory.StandardQty == 100m,
            "order defaults differ.");
        Check((await c.RequireAsync<PurchParameters>(x => x.DataAreaId == c.Company, ct)).PurchaseType == PurchaseType.Purch,
            "default purchase type differs.");
        var charge = await c.RequireAsync<MarkupTable>(x => x.MarkupCode == "APV2-FRT", ct);
        var expense = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "681003", ct);
        await c.RequireAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == charge.TaxItemGroup, ct);
        Check(charge.ModuleType == ModuleInventPurchSales.Purchase && charge.CustType == 0 && charge.VendType == 1
            && charge.CustomerLedgerDimension == expense.RecId && charge.VendorLedgerDimension == null, "charge posting references differ.");
    }
}
