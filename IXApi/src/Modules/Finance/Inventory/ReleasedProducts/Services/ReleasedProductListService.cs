using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using InventModelGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ReleasedProductListService(
    IUnitOfWork unitOfWork,
    ICompanyExecutionContext company,
    IInventDimensionResolver dimensionResolver)
    : IReleasedProductListService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<List<ReleasedProductListDto>?> ListAsync(CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        return string.IsNullOrWhiteSpace(companyId) ? null : await LoadAsync(companyId, null, ct);
    }

    public async Task<(ReleasedProductListDto? Product, string? Error)> CreateSharedAsync(CreateSharedProductDto input, CancellationToken ct = default)
    {
        var number = input.ProductNumber.Trim().ToUpperInvariant();
        if (number.Length == 0 || string.IsNullOrWhiteSpace(input.Name)) return (null, "Product number and product name are required.");
        if (number.Length > FieldLengths.ProductNumber || input.Name.Trim().Length > FieldLengths.Name ||
            input.SearchName.Trim().Length > FieldLengths.ProductSearchName || input.Description.Trim().Length > FieldLengths.Memo)
            return (null, "One or more product fields exceed their allowed length.");
        if (await Db.Set<EcoResProduct>().AnyAsync(x => x.DisplayProductNumber == number, ct)) return (null, $"Product number '{number}' already exists.");
        var ownsTransaction = Db.Database.CurrentTransaction is null;
        if (ownsTransaction) await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var product = new EcoResProduct { DisplayProductNumber = number, SearchName = input.SearchName.Trim(), ProductType = ItemType.Item, ProductSubtype = EcoResProductSubtype.Product };
            Db.Add(product);
            await unitOfWork.CompleteAsync(ct);
            Db.Add(new EcoResDistinctProduct { RecId = product.RecId });
            Db.Add(new EcoResProductTranslation { Product = product.RecId, LanguageId = string.IsNullOrWhiteSpace(input.LanguageId) ? "en-us" : input.LanguageId.Trim().ToLowerInvariant(), Name = input.Name.Trim(), Description = input.Description.Trim() });
            await unitOfWork.CompleteAsync(ct);
            if (ownsTransaction) await unitOfWork.CommitTransactionAsync(ct);
            return (new ReleasedProductMappingSource(
                product.RecId,
                number,
                input.Name.Trim(),
                string.Empty,
                product.SearchName,
                product.ProductType.ToString(),
                product.ProductSubtype.ToString(),
                0,
                0).Adapt<ReleasedProductListDto>(), null);
        }
        catch
        {
            if (ownsTransaction) await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<(ReleasedProductListDto? Product, string? Error)> ReleaseAsync(ReleaseProductDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        if (!long.TryParse(input.ProductId, NumberStyles.None, CultureInfo.InvariantCulture, out var productId)) return (null, "The shared product identifier is invalid.");
        var product = await Db.Set<EcoResProduct>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == productId, ct);
        if (product is null) return (null, "The shared product was not found.");
        var itemId = input.ItemNumber.Trim().ToUpperInvariant();
        if (itemId.Length == 0) return (null, "Item number is required.");
        var existing = await Db.Set<ReleasedItem>().AsNoTracking().FirstOrDefaultAsync(
            x => x.DataAreaId == companyId && (x.ItemId == itemId || x.Product == productId), ct);
        if (existing is not null)
        {
            if (existing.Product == productId && existing.ItemId == itemId)
                return ((await LoadAsync(companyId, existing.RecId, ct)).Single(), null);
            return (null, "The item number or product is already released to this company with different values.");
        }
        var name = await Db.Set<EcoResProductTranslation>().Where(x => x.Product == productId).OrderBy(x => x.LanguageId == "en-us" ? 0 : 1).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? product.DisplayProductNumber;
        var item = new ReleasedItem { DataAreaId = companyId, ItemId = itemId, Product = productId, Name = name, NameAlias = string.IsNullOrWhiteSpace(input.NameAlias) ? product.SearchName : input.NameAlias.Trim(), Sku = itemId, ItemType = product.ProductType };
        Db.Add(item);
        foreach (var type in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales }) Db.Add(new InventTableModule { DataAreaId = companyId, ItemId = itemId, ModuleType = type, PriceUnit = 1 });
        await unitOfWork.CompleteAsync(ct);
        return ((await LoadAsync(companyId, item.RecId, ct)).Single(), null);
    }

    public async Task<(ReleasedProductListDto? Product, string? Error)> ConfigureAsync(long recId, ConfigureReleasedProductDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var item = await Db.Set<ReleasedItem>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (item is null) return (null, "The released product was not found.");
        if (input.PriceUnit <= 0) return (null, "Price unit must be greater than zero.");
        var units = new[] { input.InventoryUnit.Trim(), input.PurchaseUnit.Trim(), input.SalesUnit.Trim() }.Distinct().ToList();
        var unitRows = await Db.Set<UnitOfMeasure>().Where(x => units.Contains(x.Symbol)).ToListAsync(ct);
        if (units.Any(string.IsNullOrWhiteSpace) || unitRows.Count != units.Count) return (null, "All module units must reference existing units of measure.");
        if (!string.Equals(input.PurchaseUnit, input.InventoryUnit, StringComparison.OrdinalIgnoreCase) && input.PurchaseToInventoryFactor <= 0) return (null, "A positive purchase-to-inventory conversion factor is required when the units differ.");
        if (!string.Equals(input.SalesUnit, input.InventoryUnit, StringComparison.OrdinalIgnoreCase) && input.SalesToInventoryFactor <= 0) return (null, "A positive sales-to-inventory conversion factor is required when the units differ.");
        if (string.Equals(input.PurchaseUnit, input.SalesUnit, StringComparison.OrdinalIgnoreCase) && !string.Equals(input.PurchaseUnit, input.InventoryUnit, StringComparison.OrdinalIgnoreCase) && input.PurchaseToInventoryFactor != input.SalesToInventoryFactor) return (null, "Purchase and sales conversion factors must match when they use the same unit.");
        if (!await AssignmentsExistAsync(companyId, input, ct)) return (null, "Item, model, storage, and tracking groups must reference existing setup records.");
        if (!await TaxGroupsExistAsync(companyId, input, ct)) return (null, "Purchase and sales item tax groups must reference existing setup records.");
        if (input.DefaultOrderSettings is not null)
        {
            var orderSettingsError = await ValidateOrderSettingsAsync(companyId, input.DefaultOrderSettings, ct);
            if (orderSettingsError is not null) return (null, orderSettingsError);
        }
        var modules = await Db.Set<InventTableModule>().Where(x => x.DataAreaId == companyId && x.ItemId == item.ItemId).ToListAsync(ct);
        SetModule(modules, item.ItemId, companyId, ModuleInventPurchSales.Inventory, input.InventoryUnit, input.InventoryPrice, input.PriceUnit, "");
        SetModule(modules, item.ItemId, companyId, ModuleInventPurchSales.Purchase, input.PurchaseUnit, input.PurchasePrice, input.PriceUnit, input.PurchaseTaxItemGroupId);
        SetModule(modules, item.ItemId, companyId, ModuleInventPurchSales.Sales, input.SalesUnit, input.SalesPrice, input.PriceUnit, input.SalesTaxItemGroupId);
        await UpsertAsync<InventItemGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId, ItemGroupDataAreaId = companyId }, x => x.ItemGroupId = input.ItemGroupId.Trim(), ct);
        await UpsertAsync<InventModelGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId, ModelGroupDataAreaId = companyId }, x => x.ModelGroupId = input.ModelGroupId.Trim(), ct);
        await UpsertAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId }, x => x.StorageDimensionGroupId = input.StorageDimensionGroupId.Trim(), ct);
        await UpsertAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId }, x => x.TrackingDimensionGroupId = input.TrackingDimensionGroupId.Trim(), ct);
        var conversions = await Db.Set<UnitOfMeasureConversion>().Where(x => x.Product == item.Product).ToListAsync(ct);
        SetConversion(conversions, unitRows, item.Product, input.PurchaseUnit, input.InventoryUnit, input.PurchaseToInventoryFactor);
        SetConversion(conversions, unitRows, item.Product, input.SalesUnit, input.InventoryUnit, input.SalesToInventoryFactor);
        if (input.DefaultOrderSettings is not null)
            await ApplyOrderSettingsAsync(item.ItemId, companyId, input.DefaultOrderSettings, ct);
        await unitOfWork.CompleteAsync(ct);
        return ((await LoadAsync(companyId, recId, ct)).Single(), null);
    }

    public async Task<(ReleasedProductListDto? Product, string? Error)> AssignGroupsAsync(long recId, ReleasedProductGroupAssignmentDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var item = await Db.Set<ReleasedItem>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (item is null) return (null, "The released product was not found.");
        if (string.IsNullOrWhiteSpace(input.ItemGroupId) || string.IsNullOrWhiteSpace(input.ModelGroupId) ||
            string.IsNullOrWhiteSpace(input.StorageDimensionGroupId) || string.IsNullOrWhiteSpace(input.TrackingDimensionGroupId))
            return (null, "Item, model, storage, and tracking groups are required.");
        if (!await Db.Set<InventItemGroup>().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == input.ItemGroupId.Trim(), ct) ||
            !await Db.Set<InventModelGroupEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ModelGroupId == input.ModelGroupId.Trim(), ct) ||
            !await Db.Set<EcoResStorageDimensionGroup>().AnyAsync(x => x.StorageDimensionGroupId == input.StorageDimensionGroupId.Trim(), ct) ||
            !await Db.Set<EcoResTrackingDimensionGroup>().AnyAsync(x => x.TrackingDimensionGroupId == input.TrackingDimensionGroupId.Trim(), ct))
            return (null, "One or more selected groups do not reference existing setup records.");

        await UpsertAsync<InventItemGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId, ItemGroupDataAreaId = companyId }, x => x.ItemGroupId = input.ItemGroupId.Trim(), ct);
        await UpsertAsync<InventModelGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId, ModelGroupDataAreaId = companyId }, x => x.ModelGroupId = input.ModelGroupId.Trim(), ct);
        await UpsertAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId }, x => x.StorageDimensionGroupId = input.StorageDimensionGroupId.Trim(), ct);
        await UpsertAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId }, x => x.TrackingDimensionGroupId = input.TrackingDimensionGroupId.Trim(), ct);
        await unitOfWork.CompleteAsync(ct);
        return ((await LoadAsync(companyId, recId, ct)).Single(), null);
    }

    public async Task<(ReleasedProductListDto? Product, string? Error)> AssignUnitsAsync(long recId, ReleasedProductUnitAssignmentDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var item = await Db.Set<ReleasedItem>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (item is null) return (null, "The released product was not found.");
        var units = new[] { input.InventoryUnit.Trim(), input.PurchaseUnit.Trim(), input.SalesUnit.Trim() }.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var unitRows = await Db.Set<UnitOfMeasure>().Where(x => units.Contains(x.Symbol)).ToListAsync(ct);
        if (units.Any(string.IsNullOrWhiteSpace) || unitRows.Count != units.Count)
            return (null, "Inventory, purchase, and sales units must reference existing units of measure.");
        if (!string.Equals(input.PurchaseUnit, input.InventoryUnit, StringComparison.OrdinalIgnoreCase) && input.PurchaseToInventoryFactor <= 0)
            return (null, "A positive purchase-to-inventory conversion factor is required when the units differ.");
        if (!string.Equals(input.SalesUnit, input.InventoryUnit, StringComparison.OrdinalIgnoreCase) && input.SalesToInventoryFactor <= 0)
            return (null, "A positive sales-to-inventory conversion factor is required when the units differ.");
        if (string.Equals(input.PurchaseUnit, input.SalesUnit, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(input.PurchaseUnit, input.InventoryUnit, StringComparison.OrdinalIgnoreCase) &&
            input.PurchaseToInventoryFactor != input.SalesToInventoryFactor)
            return (null, "Purchase and sales conversion factors must match when they use the same unit.");
        var modules = await Db.Set<InventTableModule>().Where(x => x.DataAreaId == companyId && x.ItemId == item.ItemId).ToListAsync(ct);
        SetUnit(modules, item.ItemId, companyId, ModuleInventPurchSales.Inventory, input.InventoryUnit);
        SetUnit(modules, item.ItemId, companyId, ModuleInventPurchSales.Purchase, input.PurchaseUnit);
        SetUnit(modules, item.ItemId, companyId, ModuleInventPurchSales.Sales, input.SalesUnit);
        if (input.Groups is not null)
        {
            var groups = input.Groups;
            if (string.IsNullOrWhiteSpace(groups.ItemGroupId) || string.IsNullOrWhiteSpace(groups.ModelGroupId) || string.IsNullOrWhiteSpace(groups.StorageDimensionGroupId) || string.IsNullOrWhiteSpace(groups.TrackingDimensionGroupId) ||
                !await Db.Set<InventItemGroup>().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == groups.ItemGroupId.Trim(), ct) ||
                !await Db.Set<InventModelGroupEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ModelGroupId == groups.ModelGroupId.Trim(), ct) ||
                !await Db.Set<EcoResStorageDimensionGroup>().AnyAsync(x => x.StorageDimensionGroupId == groups.StorageDimensionGroupId.Trim(), ct) ||
                !await Db.Set<EcoResTrackingDimensionGroup>().AnyAsync(x => x.TrackingDimensionGroupId == groups.TrackingDimensionGroupId.Trim(), ct))
                return (null, "One or more selected groups do not reference existing setup records.");
            await UpsertAsync<InventItemGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId, ItemGroupDataAreaId = companyId }, x => x.ItemGroupId = groups.ItemGroupId.Trim(), ct);
            await UpsertAsync<InventModelGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId, ModelGroupDataAreaId = companyId }, x => x.ModelGroupId = groups.ModelGroupId.Trim(), ct);
            await UpsertAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId }, x => x.StorageDimensionGroupId = groups.StorageDimensionGroupId.Trim(), ct);
            await UpsertAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == companyId, () => new() { ItemId = item.ItemId, ItemDataAreaId = companyId }, x => x.TrackingDimensionGroupId = groups.TrackingDimensionGroupId.Trim(), ct);
        }
        if (input.Commercial is not null)
        {
            var commercial = input.Commercial;
            if (commercial.PurchasePrice < 0 || commercial.SalesPrice < 0 || commercial.PurchasePriceQuantity <= 0 || commercial.SalesPriceQuantity <= 0)
                return (null, "Purchase and sales prices cannot be negative, and price quantities must be greater than zero.");
            var taxGroups = new[] { commercial.PurchaseTaxItemGroupId.Trim(), commercial.SalesTaxItemGroupId.Trim() }.Where(x => x.Length > 0).Distinct().ToList();
            if (taxGroups.Count > 0 && await Db.Set<TaxItemGroupHeading>().CountAsync(x => x.DataAreaId == companyId && taxGroups.Contains(x.TaxItemGroup), ct) != taxGroups.Count)
                return (null, "Purchase and sales item tax groups must reference existing setup records.");
            SetCommercial(modules, ModuleInventPurchSales.Purchase, commercial.PurchasePrice, commercial.PurchasePriceQuantity, commercial.PurchaseTaxItemGroupId);
            SetCommercial(modules, ModuleInventPurchSales.Sales, commercial.SalesPrice, commercial.SalesPriceQuantity, commercial.SalesTaxItemGroupId);
        }
        if (input.DefaultOrderSettings is not null)
        {
            var orderSettingsError = await ValidateOrderSettingsAsync(companyId, input.DefaultOrderSettings, ct);
            if (orderSettingsError is not null) return (null, orderSettingsError);
            await ApplyOrderSettingsAsync(item.ItemId, companyId, input.DefaultOrderSettings, ct);
        }
        var conversions = await Db.Set<UnitOfMeasureConversion>().Where(x => x.Product == item.Product).ToListAsync(ct);
        SetConversion(conversions, unitRows, item.Product, input.PurchaseUnit, input.InventoryUnit, input.PurchaseToInventoryFactor);
        SetConversion(conversions, unitRows, item.Product, input.SalesUnit, input.InventoryUnit, input.SalesToInventoryFactor);
        await unitOfWork.CompleteAsync(ct);
        return ((await LoadAsync(companyId, recId, ct)).Single(), null);
    }

    public async Task<(ReleasedProductListDto? Product, string? Error)> CreateReleasedAsync(CreateReleasedProductDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        if (string.IsNullOrWhiteSpace(input.ProductNumber) || string.IsNullOrWhiteSpace(input.ItemNumber) || string.IsNullOrWhiteSpace(input.Name))
            return (null, "Product number, item number, and product name are required.");
        if (!string.IsNullOrWhiteSpace(input.VendorAccount) && !await Db.Set<VendTable>().AnyAsync(
                x => x.DataAreaId == companyId && x.AccountNum == input.VendorAccount.Trim(), ct))
            return (null, "Vendor account is invalid for the selected company.");
        if (!string.IsNullOrWhiteSpace(input.BomUnit) && !await Db.Set<UnitOfMeasure>().AnyAsync(
                x => x.Symbol == input.BomUnit.Trim(), ct))
            return (null, "BOM unit must reference an existing unit of measure.");
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var shared = await CreateSharedAsync(new CreateSharedProductDto { ProductNumber = input.ProductNumber, SearchName = input.SearchName, Name = input.Name, LanguageId = "en-us" }, ct);
            if (shared.Product is null) { await unitOfWork.RollbackTransactionAsync(ct); return (null, shared.Error); }
            if (!string.IsNullOrWhiteSpace(input.NameAlias) && long.TryParse(shared.Product.Id, out var productId))
                Db.Add(new EcoResProductTranslation { Product = productId, LanguageId = "ar", Name = input.NameAlias.Trim() });
            var released = await ReleaseAsync(new ReleaseProductDto { ProductId = shared.Product.Id, ItemNumber = input.ItemNumber, NameAlias = input.ItemNameAlias }, ct);
            if (released.Product is null) { await unitOfWork.RollbackTransactionAsync(ct); return released; }
            if (input.Configuration is not null)
            {
                var configured = await ConfigureAsync(long.Parse(released.Product.Id, CultureInfo.InvariantCulture), input.Configuration, ct);
                if (configured.Product is null) { await unitOfWork.RollbackTransactionAsync(ct); return configured; }
            }
            var item = await Db.Set<ReleasedItem>().SingleAsync(x => x.RecId == long.Parse(released.Product.Id, CultureInfo.InvariantCulture) && x.DataAreaId == companyId, ct);
            item.Sku = string.IsNullOrWhiteSpace(input.Sku) ? item.ItemId : input.Sku.Trim();
            item.ItemBuyerGroupId = input.BuyerGroupId.Trim(); item.PrimaryVendorID = input.VendorAccount.Trim(); item.BomUnitId = input.BomUnit.Trim();
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return ((await LoadAsync(companyId, item.RecId, ct)).Single(), null);
        }
        catch { await unitOfWork.RollbackTransactionAsync(ct); throw; }
    }

    public async Task<ReleasedProductLookupDto?> GetLookupsAsync(CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return new ReleasedProductLookupDto
        {
            Units = await Db.Set<UnitOfMeasure>().AsNoTracking().OrderBy(x => x.Symbol).Select(x => new ReleasedProductLookupOptionDto(x.Symbol, x.Symbol)).ToListAsync(ct),
            ItemGroups = await Db.Set<InventItemGroup>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.ItemGroupId).Select(x => new ReleasedProductLookupOptionDto(x.ItemGroupId, x.ItemGroupId + " - " + x.Name)).ToListAsync(ct),
            ModelGroups = await Db.Set<InventModelGroupEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.ModelGroupId).Select(x => new ReleasedProductLookupOptionDto(x.ModelGroupId, x.ModelGroupId + " - " + x.Name)).ToListAsync(ct),
            StorageDimensionGroups = await Db.Set<EcoResStorageDimensionGroup>().AsNoTracking().OrderBy(x => x.StorageDimensionGroupId).Select(x => new ReleasedProductLookupOptionDto(x.StorageDimensionGroupId, x.StorageDimensionGroupId + " - " + x.Name)).ToListAsync(ct),
            TrackingDimensionGroups = await Db.Set<EcoResTrackingDimensionGroup>().AsNoTracking().OrderBy(x => x.TrackingDimensionGroupId).Select(x => new ReleasedProductLookupOptionDto(x.TrackingDimensionGroupId, x.TrackingDimensionGroupId + " - " + x.Name)).ToListAsync(ct),
            TaxItemGroups = await Db.Set<TaxItemGroupHeading>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.TaxItemGroup).Select(x => new ReleasedProductLookupOptionDto(x.TaxItemGroup, x.TaxItemGroup + " - " + x.Name)).ToListAsync(ct),
            Vendors = await Db.Set<VendTable>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.AccountNum).Select(x => new ReleasedProductLookupOptionDto(x.AccountNum, x.AccountNum)).ToListAsync(ct),
            Sites = await Db.Set<InventSite>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.SiteId).Select(x => new ReleasedProductLookupOptionDto(x.SiteId, x.SiteId + " - " + x.Name)).ToListAsync(ct),
            Warehouses = await Db.Set<InventLocation>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.InventLocationId).Select(x => new ReleasedProductLookupOptionDto(x.InventLocationId, x.InventLocationId + " - " + x.Name)).ToListAsync(ct),
        };
    }

    public async Task<ReleasedProductValidationDto?> ValidateAsync(long recId, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var item = await Db.Set<ReleasedItem>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (item is null) return null;
        var errors = new List<string>();
        var warnings = new List<string>();
        var product = await Db.Set<EcoResProduct>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == item.Product, ct);
        if (product is null) errors.Add("The released item does not reference an existing shared product.");
        else
        {
            if (product.ProductType != ItemType.Item) errors.Add("Product type must be Item.");
            if (product.ProductSubtype != EcoResProductSubtype.Product) errors.Add("Product subtype must be Product for an item without variants.");
            if (!await Db.Set<EcoResDistinctProduct>().AnyAsync(x => x.RecId == product.RecId, ct))
                errors.Add("The distinct-product record is missing.");
            if (!await Db.Set<EcoResProductTranslation>().AnyAsync(x => x.Product == product.RecId && x.Name != string.Empty, ct))
                errors.Add("A product name translation is required.");
        }

        var modules = await Db.Set<InventTableModule>().AsNoTracking().Where(x => x.DataAreaId == companyId && x.ItemId == item.ItemId).ToListAsync(ct);
        var units = await Db.Set<UnitOfMeasure>().AsNoTracking().ToListAsync(ct);
        var unitSymbols = units.Select(x => x.Symbol).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var type in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
        {
            var module = modules.SingleOrDefault(x => x.ModuleType == type);
            if (module is null || string.IsNullOrWhiteSpace(module.UnitId)) errors.Add($"{type} unit is required.");
            else if (!unitSymbols.Contains(module.UnitId)) errors.Add($"{type} unit does not reference an existing unit of measure.");
            if (module is not null && type != ModuleInventPurchSales.Inventory && module.PriceUnit <= 0)
                errors.Add($"{type} price quantity must be greater than zero.");
            if (module is not null && !string.IsNullOrWhiteSpace(module.TaxItemGroupId) &&
                !await Db.Set<TaxItemGroupHeading>().AnyAsync(x => x.DataAreaId == companyId && x.TaxItemGroup == module.TaxItemGroupId, ct))
                errors.Add($"{type} item sales tax group does not reference an existing setup record.");
        }

        var inventoryUnit = modules.SingleOrDefault(x => x.ModuleType == ModuleInventPurchSales.Inventory)?.UnitId ?? string.Empty;
        foreach (var type in new[] { ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
        {
            var transactionUnit = modules.SingleOrDefault(x => x.ModuleType == type)?.UnitId ?? string.Empty;
            if (product is not null && transactionUnit.Length > 0 && inventoryUnit.Length > 0 &&
                !string.Equals(transactionUnit, inventoryUnit, StringComparison.OrdinalIgnoreCase))
            {
                var conversions = await Db.Set<UnitOfMeasureConversion>().AsNoTracking().Where(x => x.Product == product.RecId).ToListAsync(ct);
                if (ConversionFactor(conversions, units, product.RecId, transactionUnit, inventoryUnit) <= 0)
                    errors.Add($"A positive {type.ToString().ToLowerInvariant()}-to-inventory unit conversion is required.");
            }
        }
        var itemGroupId = await Db.Set<InventItemGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId).Select(x => x.ItemGroupId).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(itemGroupId)) errors.Add("Item group is required.");
        else if (!await Db.Set<InventItemGroup>().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == itemGroupId, ct)) errors.Add("Item group does not reference an existing setup record.");
        if (!await Db.Set<InventModelGroupItem>().AnyAsync(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId, ct)) errors.Add("Item model group is required.");
        var storageGroupId = await Db.Set<EcoResStorageDimensionGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId).Select(x => x.StorageDimensionGroupId).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(storageGroupId)) errors.Add("Storage dimension group is required.");
        else if (!await Db.Set<EcoResStorageDimensionGroup>().AnyAsync(x => x.StorageDimensionGroupId == storageGroupId, ct)) errors.Add("Storage dimension group does not reference an existing setup record.");
        var trackingGroupId = await Db.Set<EcoResTrackingDimensionGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId).Select(x => x.TrackingDimensionGroupId).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(trackingGroupId)) errors.Add("Tracking dimension group is required.");
        else if (!await Db.Set<EcoResTrackingDimensionGroup>().AnyAsync(x => x.TrackingDimensionGroupId == trackingGroupId, ct)) errors.Add("Tracking dimension group does not reference an existing setup record.");
        var modelGroupId = await Db.Set<InventModelGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId).Select(x => x.ModelGroupId).FirstOrDefaultAsync(ct);
        if (!string.IsNullOrWhiteSpace(modelGroupId) && !await Db.Set<InventModelGroupEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ModelGroupId == modelGroupId && x.StockedProduct == NoYes.Yes, ct)) errors.Add("The item model group must be configured as a stocked product.");
        var purchaseSetup = await Db.Set<InventItemPurchSetup>().AsNoTracking().FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId && x.InventDimId == string.Empty, ct);
        var salesSetup = await Db.Set<InventItemSalesSetup>().AsNoTracking().FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId && x.InventDimId == string.Empty, ct);
        var inventorySetup = await Db.Set<InventItemInventSetup>().AsNoTracking().FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId && x.InventDimId == string.Empty, ct);
        if (purchaseSetup is null) errors.Add("Purchase default order settings are required.");
        else await ValidatePersistedOrderSettingsAsync("Purchase", companyId, purchaseSetup.InventDimIdDefault, purchaseSetup.LowestQty, purchaseSetup.HighestQty, purchaseSetup.MultipleQty, purchaseSetup.StandardQty, purchaseSetup.LeadTime, purchaseSetup.Stopped, errors, ct);
        if (salesSetup is null) errors.Add("Sales default order settings are required.");
        else await ValidatePersistedOrderSettingsAsync("Sales", companyId, salesSetup.InventDimIdDefault, salesSetup.LowestQty, salesSetup.HighestQty, salesSetup.MultipleQty, salesSetup.StandardQty, salesSetup.LeadTime, salesSetup.Stopped, errors, ct);
        if (inventorySetup is null) errors.Add("Inventory default order settings are required.");
        else await ValidatePersistedOrderSettingsAsync("Inventory", companyId, inventorySetup.InventDimIdDefault, inventorySetup.LowestQty, inventorySetup.HighestQty, inventorySetup.MultipleQty, inventorySetup.StandardQty, inventorySetup.LeadTime, inventorySetup.Stopped, errors, ct);
        if (!await Db.Set<InventItemSetupSupplyType>().AnyAsync(x => x.ItemDataAreaId == companyId && x.ItemId == item.ItemId, ct)) errors.Add("Default order type is required.");
        if (!await HasApplicablePostingRuleAsync(companyId, item.ItemId, itemGroupId,
                [InventAccountType.PurchPackingSlip, InventAccountType.PurchReceipt], ct))
            warnings.Add("No general purchase inventory posting rule was found for this item, its item group, or all items.");
        if (!await HasApplicablePostingRuleAsync(companyId, item.ItemId, itemGroupId,
                [InventAccountType.SalesPackingSlip, InventAccountType.SalesIssue, InventAccountType.SalesRevenue], ct))
            warnings.Add("No general sales inventory posting rule was found for this item, its item group, or all items.");
        return new ReleasedProductValidationDto { Errors = errors, Warnings = warnings };
    }

    private async Task<bool> HasApplicablePostingRuleAsync(string companyId, string itemId, string? itemGroupId,
        IReadOnlyCollection<InventAccountType> accountTypes, CancellationToken ct) =>
        await (from posting in Db.Set<InventPosting>().AsNoTracking()
               join ledger in Db.Set<DimensionAttributeValueCombination>().AsNoTracking()
                   on posting.LedgerDimension equals ledger.RecId
               where posting.DataAreaId == companyId && ledger.DataAreaId == companyId &&
                     posting.CustVendCode == TableGroupAll.All && accountTypes.Contains(posting.InventAccountType) &&
                     (posting.ItemCode == TableGroupAll.All ||
                      posting.ItemCode == TableGroupAll.Table && posting.ItemRelation == itemId ||
                      posting.ItemCode == TableGroupAll.Group && posting.ItemRelation == itemGroupId)
               select posting.RecId).AnyAsync(ct);

    private async Task ValidatePersistedOrderSettingsAsync(string label, string companyId, string? defaultDimensionId,
        decimal? minimum, decimal? maximum, decimal? multiple, decimal? standard, int? leadTime, int? stopped,
        List<string> errors, CancellationToken ct)
    {
        if (stopped == 1) errors.Add($"{label} transactions are stopped.");
        if (minimum < 0 || maximum < 0 || multiple < 0 || standard < 0 || leadTime < 0)
            errors.Add($"{label} order quantities and lead time cannot be negative.");
        if (maximum.GetValueOrDefault() > 0 && minimum.GetValueOrDefault() > maximum.GetValueOrDefault())
            errors.Add($"{label} minimum quantity cannot exceed maximum quantity.");
        if (string.IsNullOrWhiteSpace(defaultDimensionId)) return;
        var dimension = await Db.Set<InventDim>().AsNoTracking().FirstOrDefaultAsync(
            x => x.DataAreaId == companyId && x.InventDimId == defaultDimensionId, ct);
        if (dimension is null)
        {
            errors.Add($"{label} default dimensions do not reference an existing inventory dimension.");
            return;
        }
        if (string.IsNullOrWhiteSpace(dimension.InventSiteId) || string.IsNullOrWhiteSpace(dimension.InventLocationId))
        {
            errors.Add($"{label} default site and warehouse must be supplied together.");
            return;
        }
        if (!await Db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == companyId &&
            x.InventLocationId == dimension.InventLocationId && x.InventSiteId == dimension.InventSiteId, ct))
            errors.Add($"{label} default warehouse does not belong to its default site.");
    }

    private async Task<List<ReleasedProductListDto>> LoadAsync(string companyId, long? itemRecId, CancellationToken ct)
    {
        var rows = await (from item in Db.Set<ReleasedItem>().AsNoTracking() join product in Db.Set<EcoResProduct>().AsNoTracking() on item.Product equals product.RecId where item.DataAreaId == companyId && (!itemRecId.HasValue || item.RecId == itemRecId) orderby item.ItemId select new { ItemRecId = item.RecId, item.ItemId, item.Name, item.NameAlias, ProductRecId = product.RecId, product.ProductType, product.ProductSubtype }).ToListAsync(ct);
        var productIds = rows.Select(x => x.ProductRecId).Distinct().ToList();
        var itemIds = rows.Select(x => x.ItemId).ToList();
        var translations = await Db.Set<EcoResProductTranslation>().AsNoTracking().Where(x => productIds.Contains(x.Product)).ToListAsync(ct);
        var modules = await Db.Set<InventTableModule>().AsNoTracking().Where(x => x.DataAreaId == companyId && itemIds.Contains(x.ItemId)).ToListAsync(ct);
        var itemGroups = await Db.Set<InventItemGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId)).ToDictionaryAsync(x => x.ItemId, x => x.ItemGroupId, ct);
        var modelGroups = await Db.Set<InventModelGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId)).ToDictionaryAsync(x => x.ItemId, x => x.ModelGroupId, ct);
        var storageGroups = await Db.Set<EcoResStorageDimensionGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId)).ToDictionaryAsync(x => x.ItemId, x => x.StorageDimensionGroupId, ct);
        var trackingGroups = await Db.Set<EcoResTrackingDimensionGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId)).ToDictionaryAsync(x => x.ItemId, x => x.TrackingDimensionGroupId, ct);
        var unitRows = await Db.Set<UnitOfMeasure>().AsNoTracking().ToListAsync(ct);
        var conversions = await Db.Set<UnitOfMeasureConversion>().AsNoTracking().Where(x => productIds.Contains(x.Product)).ToListAsync(ct);
        var purchaseSetups = await Db.Set<InventItemPurchSetup>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId) && x.InventDimId == string.Empty).ToListAsync(ct);
        var salesSetups = await Db.Set<InventItemSalesSetup>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId) && x.InventDimId == string.Empty).ToListAsync(ct);
        var inventorySetups = await Db.Set<InventItemInventSetup>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId) && x.InventDimId == string.Empty).ToListAsync(ct);
        var supplyTypes = await Db.Set<InventItemSetupSupplyType>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && itemIds.Contains(x.ItemId)).ToListAsync(ct);
        var defaultDimensionIds = purchaseSetups.Select(x => x.InventDimIdDefault).Concat(salesSetups.Select(x => x.InventDimIdDefault)).Concat(inventorySetups.Select(x => x.InventDimIdDefault)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        var defaultDimensions = await Db.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == companyId && defaultDimensionIds.Contains(x.InventDimId)).ToDictionaryAsync(x => x.InventDimId, ct);
        return rows.Select(x => new ReleasedProductMappingSource(
                x.ItemRecId,
                x.ItemId,
                translations.FirstOrDefault(t => t.Product == x.ProductRecId && t.LanguageId == "en-us")?.Name ?? x.Name,
                translations.FirstOrDefault(t => t.Product == x.ProductRecId && t.LanguageId.StartsWith("ar"))?.Name ?? string.Empty,
                x.NameAlias,
                x.ProductType.ToString(),
                x.ProductSubtype.ToString(),
                Price(modules, x.ItemId, ModuleInventPurchSales.Sales),
                Price(modules, x.ItemId, ModuleInventPurchSales.Purchase),
                Price(modules, x.ItemId, ModuleInventPurchSales.Inventory),
                PriceQuantity(modules, x.ItemId, ModuleInventPurchSales.Sales),
                PriceQuantity(modules, x.ItemId, ModuleInventPurchSales.Purchase),
                TaxItemGroup(modules, x.ItemId, ModuleInventPurchSales.Sales),
                TaxItemGroup(modules, x.ItemId, ModuleInventPurchSales.Purchase),
                itemGroups.GetValueOrDefault(x.ItemId, string.Empty),
                modelGroups.GetValueOrDefault(x.ItemId, string.Empty),
                storageGroups.GetValueOrDefault(x.ItemId, string.Empty),
                trackingGroups.GetValueOrDefault(x.ItemId, string.Empty),
                Unit(modules, x.ItemId, ModuleInventPurchSales.Inventory),
                Unit(modules, x.ItemId, ModuleInventPurchSales.Purchase),
                Unit(modules, x.ItemId, ModuleInventPurchSales.Sales),
                ConversionFactor(conversions, unitRows, x.ProductRecId, Unit(modules, x.ItemId, ModuleInventPurchSales.Purchase), Unit(modules, x.ItemId, ModuleInventPurchSales.Inventory)),
                ConversionFactor(conversions, unitRows, x.ProductRecId, Unit(modules, x.ItemId, ModuleInventPurchSales.Sales), Unit(modules, x.ItemId, ModuleInventPurchSales.Inventory)),
                OrderSettings(x.ItemId, purchaseSetups, salesSetups, inventorySetups, supplyTypes, defaultDimensions)))
            .Adapt<List<ReleasedProductListDto>>();
    }

    private static InventTableModule? Module(IEnumerable<InventTableModule> rows, string itemId, ModuleInventPurchSales type) => rows.Where(x => x.ItemId == itemId && x.ModuleType == type).OrderByDescending(x => x.PriceDate).ThenByDescending(x => x.RecId).FirstOrDefault();
    private static decimal Price(IEnumerable<InventTableModule> rows, string itemId, ModuleInventPurchSales type) => Module(rows, itemId, type)?.Price ?? 0;
    private static decimal PriceQuantity(IEnumerable<InventTableModule> rows, string itemId, ModuleInventPurchSales type) { var value = Module(rows, itemId, type)?.PriceUnit ?? 1; return value > 0 ? value : 1; }
    private static string TaxItemGroup(IEnumerable<InventTableModule> rows, string itemId, ModuleInventPurchSales type) => Module(rows, itemId, type)?.TaxItemGroupId ?? string.Empty;
    private static string Unit(IEnumerable<InventTableModule> rows, string itemId, ModuleInventPurchSales type) => rows.FirstOrDefault(x => x.ItemId == itemId && x.ModuleType == type)?.UnitId ?? string.Empty;
    private void SetUnit(List<InventTableModule> modules, string itemId, string companyId, ModuleInventPurchSales type, string unit) { var row = modules.SingleOrDefault(x => x.ModuleType == type) ?? new InventTableModule { ItemId = itemId, DataAreaId = companyId, ModuleType = type, PriceUnit = 1 }; if (Db.Entry(row).State == EntityState.Detached) Db.Add(row); row.UnitId = unit.Trim(); }
    private static void SetCommercial(List<InventTableModule> modules, ModuleInventPurchSales type, decimal price, decimal priceQuantity, string taxItemGroupId) { var row = modules.Single(x => x.ModuleType == type); row.Price = price; row.PriceUnit = priceQuantity; row.TaxItemGroupId = taxItemGroupId.Trim(); row.PriceDate = DateTime.UtcNow; }
    private void SetConversion(List<UnitOfMeasureConversion> rows, List<UnitOfMeasure> units, long product, string fromSymbol, string toSymbol, decimal factor)
    {
        if (string.Equals(fromSymbol, toSymbol, StringComparison.OrdinalIgnoreCase)) return;
        var from = units.Single(x => string.Equals(x.Symbol, fromSymbol, StringComparison.OrdinalIgnoreCase));
        var to = units.Single(x => string.Equals(x.Symbol, toSymbol, StringComparison.OrdinalIgnoreCase));
        var row = rows.FirstOrDefault(x => (x.FromUnitOfMeasure == from.RecId && x.ToUnitOfMeasure == to.RecId) || (x.FromUnitOfMeasure == to.RecId && x.ToUnitOfMeasure == from.RecId));
        if (row is null) { row = new UnitOfMeasureConversion { Product = product }; rows.Add(row); Db.Add(row); }
        row.FromUnitOfMeasure = from.RecId; row.ToUnitOfMeasure = to.RecId; row.Factor = factor; row.Numerator = 0; row.Denominator = 0; row.InnerOffset = 0; row.OuterOffset = 0;
    }
    private static decimal ConversionFactor(List<UnitOfMeasureConversion> rows, List<UnitOfMeasure> units, long product, string fromSymbol, string toSymbol)
    {
        if (string.IsNullOrWhiteSpace(fromSymbol) || string.IsNullOrWhiteSpace(toSymbol) || string.Equals(fromSymbol, toSymbol, StringComparison.OrdinalIgnoreCase)) return 1;
        var from = units.FirstOrDefault(x => string.Equals(x.Symbol, fromSymbol, StringComparison.OrdinalIgnoreCase)); var to = units.FirstOrDefault(x => string.Equals(x.Symbol, toSymbol, StringComparison.OrdinalIgnoreCase));
        if (from is null || to is null) return 0;
        var row = rows.FirstOrDefault(x => x.Product == product && ((x.FromUnitOfMeasure == from.RecId && x.ToUnitOfMeasure == to.RecId) || (x.FromUnitOfMeasure == to.RecId && x.ToUnitOfMeasure == from.RecId)));
        if (row is null) return 0; var factor = row.Factor > 0 ? row.Factor : row.Numerator > 0 && row.Denominator > 0 ? (decimal)row.Numerator / row.Denominator : 0;
        return row.FromUnitOfMeasure == from.RecId ? factor : factor > 0 ? 1 / factor : 0;
    }
    private void SetModule(List<InventTableModule> modules, string itemId, string companyId, ModuleInventPurchSales type, string unit, decimal price, decimal priceUnit, string taxGroup) { var row = modules.SingleOrDefault(x => x.ModuleType == type) ?? new InventTableModule { ItemId = itemId, DataAreaId = companyId, ModuleType = type }; if (Db.Entry(row).State == EntityState.Detached) Db.Add(row); row.UnitId = unit.Trim(); row.Price = price; row.PriceUnit = priceUnit; row.TaxItemGroupId = taxGroup.Trim(); row.PriceDate = DateTime.UtcNow; }
    private async Task<bool> AssignmentsExistAsync(string companyId, ConfigureReleasedProductDto input, CancellationToken ct) =>
        !string.IsNullOrWhiteSpace(input.ItemGroupId) && !string.IsNullOrWhiteSpace(input.ModelGroupId) &&
        !string.IsNullOrWhiteSpace(input.StorageDimensionGroupId) && !string.IsNullOrWhiteSpace(input.TrackingDimensionGroupId) &&
        await Db.Set<InventItemGroup>().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == input.ItemGroupId.Trim(), ct) &&
        await Db.Set<InventModelGroupEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ModelGroupId == input.ModelGroupId.Trim(), ct) &&
        await Db.Set<EcoResStorageDimensionGroup>().AnyAsync(x => x.StorageDimensionGroupId == input.StorageDimensionGroupId.Trim(), ct) &&
        await Db.Set<EcoResTrackingDimensionGroup>().AnyAsync(x => x.TrackingDimensionGroupId == input.TrackingDimensionGroupId.Trim(), ct);
    private async Task UpsertAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, Func<T> create, Action<T> update, CancellationToken ct) where T : class { var row = await Db.Set<T>().FirstOrDefaultAsync(predicate, ct) ?? create(); update(row); if (Db.Entry(row).State == EntityState.Detached) Db.Add(row); }

    private async Task<bool> TaxGroupsExistAsync(string companyId, ConfigureReleasedProductDto input, CancellationToken ct)
    {
        var groups = new[] { input.PurchaseTaxItemGroupId.Trim(), input.SalesTaxItemGroupId.Trim() }
            .Where(value => value.Length > 0).Distinct().ToList();
        return groups.Count == 0 || await Db.Set<TaxItemGroupHeading>().CountAsync(
            row => row.DataAreaId == companyId && groups.Contains(row.TaxItemGroup), ct) == groups.Count;
    }

    private async Task<string?> ValidateOrderSettingsAsync(string companyId, ReleasedProductDefaultOrderSettingsDto settings, CancellationToken ct)
    {
        if (settings.DefaultOrderType < 0) return "Default order type is invalid.";
        foreach (var module in new[] { settings.Purchase, settings.Sales, settings.Inventory })
        {
            if (module.StandardQuantity < 0 || module.MinimumQuantity < 0 || module.MaximumQuantity < 0 || module.MultipleQuantity < 0 || module.LeadTimeDays < 0)
                return "Default order quantities and lead time cannot be negative.";
            if (module.MaximumQuantity > 0 && module.MinimumQuantity > module.MaximumQuantity)
                return "Default minimum quantity cannot exceed maximum quantity.";
            if (string.IsNullOrWhiteSpace(module.SiteId) && string.IsNullOrWhiteSpace(module.WarehouseId)) continue;
            if (string.IsNullOrWhiteSpace(module.SiteId) || string.IsNullOrWhiteSpace(module.WarehouseId))
                return "Default site and warehouse must be supplied together.";
            var site = module.SiteId.Trim();
            var warehouse = module.WarehouseId.Trim();
            if (!await Db.Set<InventLocation>().AnyAsync(row => row.DataAreaId == companyId && row.InventLocationId == warehouse && row.InventSiteId == site, ct))
                return $"Warehouse '{warehouse}' was not found in site '{site}'.";
        }
        return null;
    }

    private async Task ApplyOrderSettingsAsync(string itemId, string companyId, ReleasedProductDefaultOrderSettingsDto settings, CancellationToken ct)
    {
        var purchaseDimension = await ResolveDefaultDimensionAsync(companyId, settings.Purchase, ct);
        var salesDimension = await ResolveDefaultDimensionAsync(companyId, settings.Sales, ct);
        var inventoryDimension = await ResolveDefaultDimensionAsync(companyId, settings.Inventory, ct);

        await UpsertAsync<InventItemPurchSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == companyId && x.InventDimId == string.Empty,
            () => new() { ItemId = itemId, ItemDataAreaId = companyId, InventDimId = string.Empty },
            row => ApplyOrderModule(row, purchaseDimension, settings.Purchase), ct);
        await UpsertAsync<InventItemSalesSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == companyId && x.InventDimId == string.Empty,
            () => new() { ItemId = itemId, ItemDataAreaId = companyId, InventDimId = string.Empty },
            row => ApplyOrderModule(row, salesDimension, settings.Sales), ct);
        await UpsertAsync<InventItemInventSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == companyId && x.InventDimId == string.Empty,
            () => new() { ItemId = itemId, ItemDataAreaId = companyId, InventDimId = string.Empty },
            row => ApplyOrderModule(row, inventoryDimension, settings.Inventory), ct);
        await UpsertAsync<InventItemSetupSupplyType>(x => x.ItemId == itemId && x.ItemDataAreaId == companyId,
            () => new() { ItemId = itemId, ItemDataAreaId = companyId },
            row => row.DefaultOrderType = settings.DefaultOrderType, ct);
    }

    private static ReleasedProductDefaultOrderSettingsDto OrderSettings(string itemId, List<InventItemPurchSetup> purchaseRows,
        List<InventItemSalesSetup> salesRows, List<InventItemInventSetup> inventoryRows,
        List<InventItemSetupSupplyType> supplyRows, Dictionary<string, InventDim> dimensions)
    {
        var purchase = purchaseRows.FirstOrDefault(x => x.ItemId == itemId);
        var sales = salesRows.FirstOrDefault(x => x.ItemId == itemId);
        var inventory = inventoryRows.FirstOrDefault(x => x.ItemId == itemId);
        return new ReleasedProductDefaultOrderSettingsDto
        {
            DefaultOrderType = supplyRows.FirstOrDefault(x => x.ItemId == itemId)?.DefaultOrderType ?? 1,
            Purchase = ToOrderSettings(purchase?.InventDimIdDefault, purchase?.StandardQty, purchase?.LowestQty, purchase?.HighestQty, purchase?.MultipleQty, purchase?.LeadTime, purchase?.Stopped, dimensions),
            Sales = ToOrderSettings(sales?.InventDimIdDefault, sales?.StandardQty, sales?.LowestQty, sales?.HighestQty, sales?.MultipleQty, sales?.LeadTime, sales?.Stopped, dimensions),
            Inventory = ToOrderSettings(inventory?.InventDimIdDefault, inventory?.StandardQty, inventory?.LowestQty, inventory?.HighestQty, inventory?.MultipleQty, inventory?.LeadTime, inventory?.Stopped, dimensions),
        };
    }

    private static ReleasedProductOrderModuleSettingsDto ToOrderSettings(string? dimensionId, decimal? standard, decimal? minimum,
        decimal? maximum, decimal? multiple, int? leadTime, int? stopped, Dictionary<string, InventDim> dimensions)
    {
        var dimension = !string.IsNullOrWhiteSpace(dimensionId) && dimensions.TryGetValue(dimensionId, out var value) ? value : null;
        return new ReleasedProductOrderModuleSettingsDto
        {
            SiteId = dimension?.InventSiteId ?? string.Empty, WarehouseId = dimension?.InventLocationId ?? string.Empty,
            StandardQuantity = standard ?? 0, MinimumQuantity = minimum ?? 0, MaximumQuantity = maximum ?? 0,
            MultipleQuantity = multiple ?? 0, LeadTimeDays = leadTime ?? 0, Stopped = stopped == 1,
        };
    }

    private async Task<string?> ResolveDefaultDimensionAsync(string companyId, ReleasedProductOrderModuleSettingsDto settings, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(settings.SiteId)) return null;
        var dimension = await dimensionResolver.ResolveAsync(companyId, settings.SiteId, settings.WarehouseId, ct);
        return dimension.InventDimId;
    }

    private static void ApplyOrderModule(InventItemPurchSetup row, string? dimensionId, ReleasedProductOrderModuleSettingsDto settings)
    {
        row.InventDimIdDefault = dimensionId; row.StandardQty = settings.StandardQuantity; row.MultipleQty = settings.MultipleQuantity;
        row.LowestQty = settings.MinimumQuantity; row.HighestQty = settings.MaximumQuantity > 0 ? settings.MaximumQuantity : null;
        row.LeadTime = settings.LeadTimeDays; row.Stopped = settings.Stopped ? 1 : 0;
    }
    private static void ApplyOrderModule(InventItemSalesSetup row, string? dimensionId, ReleasedProductOrderModuleSettingsDto settings)
    {
        row.InventDimIdDefault = dimensionId; row.StandardQty = settings.StandardQuantity; row.MultipleQty = settings.MultipleQuantity;
        row.LowestQty = settings.MinimumQuantity; row.HighestQty = settings.MaximumQuantity > 0 ? settings.MaximumQuantity : null;
        row.LeadTime = settings.LeadTimeDays; row.Stopped = settings.Stopped ? 1 : 0;
    }
    private static void ApplyOrderModule(InventItemInventSetup row, string? dimensionId, ReleasedProductOrderModuleSettingsDto settings)
    {
        row.InventDimIdDefault = dimensionId; row.StandardQty = settings.StandardQuantity; row.MultipleQty = settings.MultipleQuantity;
        row.LowestQty = settings.MinimumQuantity; row.HighestQty = settings.MaximumQuantity > 0 ? settings.MaximumQuantity : null;
        row.LeadTime = settings.LeadTimeDays; row.Stopped = settings.Stopped ? 1 : 0;
    }
}
