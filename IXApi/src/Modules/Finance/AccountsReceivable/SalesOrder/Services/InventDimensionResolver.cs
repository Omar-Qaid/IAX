using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;

public sealed class InventDimensionResolver : IInventDimensionResolver
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ISalesInventoryNumberService _numbers;

    public InventDimensionResolver(IFinanceDataContext dbContext, ISalesInventoryNumberService numbers)
    {
        _dbContext = dbContext;
        _numbers = numbers;
    }

    public async Task<InventDim> ResolveAsync(string dataAreaId, string inventSiteId,
        string inventLocationId, CancellationToken cancellationToken = default, InventDim? template = null)
    {
        dataAreaId = dataAreaId.Trim();
        inventSiteId = inventSiteId.Trim();
        inventLocationId = inventLocationId.Trim();
        var configId = template?.ConfigId ?? string.Empty;
        var sizeId = template?.InventSizeId ?? string.Empty;
        var colorId = template?.InventColorId ?? string.Empty;
        var styleId = template?.InventStyleId ?? string.Empty;
        var versionId = template?.InventVersionId ?? string.Empty;
        var batchId = template?.InventBatchId ?? string.Empty;
        var serialId = template?.InventSerialId ?? string.Empty;
        var statusId = template?.InventStatusId ?? string.Empty;
        var locationId = template?.WmsLocationId ?? string.Empty;
        var licensePlateId = template?.LicensePlateId ?? string.Empty;
        var dimension10 = template?.InventDimension10 ?? 0m;
        var dimension9 = template?.InventDimension9 ?? default;
        var dimension9TzId = template?.InventDimension9TzId ?? 0;
        var existing = await _dbContext.Set<InventDim>().FirstOrDefaultAsync(item =>
            item.DataAreaId == dataAreaId && item.InventSiteId == inventSiteId
            && item.InventLocationId == inventLocationId && item.ConfigId == configId
            && item.InventSizeId == sizeId && item.InventColorId == colorId
            && item.InventStyleId == styleId && item.InventVersionId == versionId
            && item.InventBatchId == batchId && item.InventSerialId == serialId
            && item.InventStatusId == statusId && item.WmsLocationId == locationId
            && item.LicensePlateId == licensePlateId && item.InventDimension10 == dimension10
            && item.InventDimension9 == dimension9 && item.InventDimension9TzId == dimension9TzId,
            cancellationToken);
        if (existing != null) return existing;

        var canonical = string.Join('|',
            dataAreaId,
            inventSiteId,
            inventLocationId,
            configId, sizeId, colorId, styleId, versionId,
            batchId, serialId, statusId, locationId, licensePlateId,
            dimension10.ToString(CultureInfo.InvariantCulture),
            dimension9.Ticks.ToString(CultureInfo.InvariantCulture),
            dimension9TzId.ToString(CultureInfo.InvariantCulture));
        var canonicalBytes = Encoding.UTF8.GetBytes(canonical);
        var inventDimId = await _numbers.NextInventDimIdAsync(cancellationToken);
        var dimension = new InventDim
        {
            InventDimId = inventDimId,
            InventSiteId = inventSiteId,
            InventLocationId = inventLocationId,
            ConfigId = configId,
            InventSizeId = sizeId,
            InventColorId = colorId,
            InventStyleId = styleId,
            InventVersionId = versionId,
            InventBatchId = batchId,
            InventSerialId = serialId,
            InventStatusId = statusId,
            WmsLocationId = locationId,
            LicensePlateId = licensePlateId,
            InventDimension10 = dimension10,
            InventDimension9 = dimension9,
            InventDimension9TzId = dimension9TzId,
            Sha1HashHex = Convert.ToHexString(SHA1.HashData(canonicalBytes)),
            Sha3HashHex = Convert.ToHexString(SHA3_384.HashData(canonicalBytes)),
            DataAreaId = dataAreaId
        };
        _dbContext.Set<InventDim>().Add(dimension);
        return dimension;
    }
}
