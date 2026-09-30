using System.Security.Cryptography;
using System.Text;
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
        string inventLocationId, CancellationToken cancellationToken = default)
    {
        dataAreaId = dataAreaId.Trim();
        inventSiteId = inventSiteId.Trim();
        inventLocationId = inventLocationId.Trim();
        var existing = await _dbContext.Set<InventDim>().FirstOrDefaultAsync(item =>
            item.DataAreaId == dataAreaId && item.InventSiteId == inventSiteId
            && item.InventLocationId == inventLocationId && item.ConfigId == string.Empty
            && item.InventSizeId == string.Empty && item.InventColorId == string.Empty
            && item.InventStyleId == string.Empty && item.InventVersionId == string.Empty
            && item.InventBatchId == string.Empty && item.InventSerialId == string.Empty
            && item.InventStatusId == string.Empty && item.WmsLocationId == string.Empty
            && item.LicensePlateId == string.Empty, cancellationToken);
        if (existing != null) return existing;

        var canonical = string.Join('|',
            dataAreaId,
            inventSiteId,
            inventLocationId,
            string.Empty, // ConfigId
            string.Empty, // InventSizeId
            string.Empty, // InventColorId
            string.Empty, // InventStyleId
            string.Empty, // InventVersionId
            string.Empty, // InventBatchId
            string.Empty, // InventSerialId
            string.Empty, // InventStatusId
            string.Empty, // WmsLocationId
            string.Empty); // LicensePlateId
        var canonicalBytes = Encoding.UTF8.GetBytes(canonical);
        var inventDimId = await _numbers.NextInventDimIdAsync(cancellationToken);
        var dimension = new InventDim
        {
            InventDimId = inventDimId,
            InventSiteId = inventSiteId,
            InventLocationId = inventLocationId,
            Sha1HashHex = Convert.ToHexString(SHA1.HashData(canonicalBytes)),
            Sha3HashHex = Convert.ToHexString(SHA3_384.HashData(canonicalBytes)),
            DataAreaId = dataAreaId
        };
        _dbContext.Set<InventDim>().Add(dimension);
        return dimension;
    }
}
