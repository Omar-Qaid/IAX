using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderLookupService : ISalesOrderLookupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;

    public SalesOrderLookupService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    {
        _unitOfWork = unitOfWork;
        _company = company;
    }

    public async Task<object> UnitsAsync(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _unitOfWork.Context.Set<UnitOfMeasure>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(unit => unit.Symbol.Contains(search));
        var totalRecords = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(unit => unit.Symbol).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(unit => new { symbol = unit.Symbol }).ToListAsync(cancellationToken);
        return (new { data, pageNumber, totalRecords, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) });
    }

    public async Task<object> ItemsAsync(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _unitOfWork.Context.Set<InventTable>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(item => item.ItemId.Contains(search) || item.NameAlias.Contains(search));
        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.ItemId).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var itemIds = items.Select(item => item.ItemId).ToList();
        var modules = await _unitOfWork.Context.Set<InventTableModule>().AsNoTracking()
            .Where(module => itemIds.Contains(module.ItemId) && module.ModuleType == ModuleInventPurchSales.Sales).ToListAsync(cancellationToken);
        var data = items.Select(item => {
            var module = modules.FirstOrDefault(row => row.ItemId == item.ItemId);
            return new { itemNumber = item.ItemId, name = item.NameAlias, itemType = item.ItemType.ToString(),
                unit = module?.UnitId ?? string.Empty, unitPrice = module == null ? 0 : module.Price / (module.PriceUnit > 0 ? module.PriceUnit : 1) };
        }).ToList();
        return (new { data, pageNumber, totalRecords, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) });
    }

    public async Task<object> InventoryDimensionsAsync(CancellationToken cancellationToken = default)
    {
        var dataAreaId = _company.GetDataAreaId();
        var sites = await _unitOfWork.Context.Set<InventSite>().AsNoTracking()
            .Where(site => site.DataAreaId == dataAreaId)
            .OrderBy(site => site.SiteId)
            .Select(site => new { id = site.SiteId, code = site.SiteId, name = site.Name })
            .ToListAsync(cancellationToken);
        var warehouses = await _unitOfWork.Context.Set<InventLocation>().AsNoTracking()
            .Where(location => location.DataAreaId == dataAreaId)
            .OrderBy(location => location.InventLocationId)
            .Select(location => new {
                id = location.InventLocationId,
                code = location.InventLocationId,
                name = location.Name,
                siteId = location.InventSiteId
            })
            .ToListAsync(cancellationToken);
        return (new { sites, warehouses });
    }

    public async Task<object> TaxGroupsAsync(CancellationToken cancellationToken = default)
    {
        var salesTaxGroups = await _unitOfWork.Context.Set<TaxGroupHeading>().AsNoTracking()
            .OrderBy(group => group.TaxGroup)
            .Select(group => new {
                id = group.TaxGroup,
                code = group.TaxGroup,
                name = group.TaxGroupName
            })
            .ToListAsync(cancellationToken);
        var itemSalesTaxGroups = await _unitOfWork.Context.Set<TaxItemGroupHeading>().AsNoTracking()
            .OrderBy(group => group.TaxItemGroup)
            .Select(group => new {
                id = group.TaxItemGroup,
                code = group.TaxItemGroup,
                name = group.Name
            })
            .ToListAsync(cancellationToken);
        return (new { salesTaxGroups, itemSalesTaxGroups });
    }

    public async Task<object> LedgerDimensionsAsync(string? search = null, int pageNumber = 1,
        int pageSize = 50, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _unitOfWork.Context.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(row => row.DataAreaId == dataAreaId && row.MainAccountValue != string.Empty);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(row => row.MainAccountValue.Contains(term) || row.DisplayValue.Contains(term));
        }
        var totalRecords = await query.CountAsync(cancellationToken);
        var dimensions = await query
            .OrderBy(row => row.MainAccountValue)
            .ThenBy(row => row.RecId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new { id = row.RecId, code = row.MainAccountValue, name = row.DisplayValue })
            .ToListAsync(cancellationToken);
        return (new
        {
            data = dimensions,
            pageNumber,
            totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        });
    }
    public async Task<object> ReturnLotsAsync(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _unitOfWork.Context.Set<InventTransOrigin>().AsNoTracking()
            .Where(origin => origin.DataAreaId == dataAreaId && origin.ItemId == itemNumber
                && _unitOfWork.Context.Set<InventTrans>().Any(transaction =>
                    transaction.DataAreaId == dataAreaId && transaction.InventTransOrigin == origin.RecId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(origin => origin.InventTransId.Contains(term)
                || origin.ReferenceId.Contains(term));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var lots = await query.OrderByDescending(origin => origin.RecId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(origin => new { id = origin.InventTransId, code = origin.InventTransId,
                name = origin.ReferenceId })
            .ToListAsync(cancellationToken);
        return (new
        {
            data = lots,
            pageNumber,
            totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        });
    }

    public async Task<object> BatchNumbersAsync(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _unitOfWork.Context.Set<InventBatch>().AsNoTracking()
            .Where(batch => batch.DataAreaId == dataAreaId && batch.ItemId == itemNumber);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(batch => batch.InventBatchId.Contains(term));
        }
        var totalRecords = await query.CountAsync(cancellationToken);
        var rows = await query.OrderBy(batch => batch.InventBatchId)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(batch => new { id = batch.InventBatchId, code = batch.InventBatchId, name = batch.InventBatchId })
            .ToListAsync(cancellationToken);
        return (new { data = rows, pageNumber, totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) });
    }

    public async Task<object> SerialNumbersAsync(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _unitOfWork.Context.Set<InventSerial>().AsNoTracking()
            .Where(serial => serial.DataAreaId == dataAreaId && serial.ItemId == itemNumber);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(serial => serial.InventSerialId.Contains(term));
        }
        var totalRecords = await query.CountAsync(cancellationToken);
        var rows = await query.OrderBy(serial => serial.InventSerialId)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(serial => new { id = serial.InventSerialId, code = serial.InventSerialId, name = serial.InventSerialId })
            .ToListAsync(cancellationToken);
        return (new { data = rows, pageNumber, totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) });
    }
}
