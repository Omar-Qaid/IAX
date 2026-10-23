using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;

public sealed class InventDimensionResolver : IInventDimensionResolver
{
    private readonly IUnitOfWork _dbContext;
    private readonly ISalesInventoryNumberService _numbers;

    public InventDimensionResolver(IUnitOfWork dbContext, ISalesInventoryNumberService numbers)
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
        if (dataAreaId.Length == 0 || inventSiteId.Length == 0 || inventLocationId.Length == 0)
            throw new InvalidOperationException("Company, site, and warehouse are required for basic inventory dimensions.");
        if (!await _dbContext.Context.Set<InventSite>().AsNoTracking().AnyAsync(x =>
                x.DataAreaId == dataAreaId && x.SiteId == inventSiteId, cancellationToken) ||
            !await _dbContext.Context.Set<InventLocation>().AsNoTracking().AnyAsync(x =>
                x.DataAreaId == dataAreaId && x.InventSiteId == inventSiteId && x.InventLocationId == inventLocationId, cancellationToken))
            throw new InvalidOperationException("The warehouse does not belong to the selected site and company.");
        if (template is not null && (template.ConfigId.Length > 0 || template.InventSizeId.Length > 0 ||
            template.InventColorId.Length > 0 || template.InventStyleId.Length > 0 || template.InventVersionId.Length > 0 ||
            template.InventBatchId.Length > 0 || template.InventSerialId.Length > 0 || template.InventStatusId.Length > 0 ||
            template.WmsLocationId.Length > 0 || template.LicensePlateId.Length > 0 || template.InventDimension10 != 0 ||
            template.InventDimension9 != default || template.InventDimension9TzId != 0))
            throw new InvalidOperationException("Only site and warehouse dimensions are supported in the basic inventory scope.");
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
        var sha1 = Convert.ToHexString(SHA1.HashData(canonicalBytes));
        var ownsTransaction = _dbContext.Context.Database.IsRelational()
            && _dbContext.Context.Database.CurrentTransaction is null;
        if (ownsTransaction) await _dbContext.BeginTransactionAsync(cancellationToken);
        try
        {
        if (_dbContext.Context.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
            await LockCombinationAsync("InventDim:" + sha1, cancellationToken);
        var existing = await _dbContext.Context.Set<InventDim>().FirstOrDefaultAsync(item =>
            item.DataAreaId == dataAreaId && item.InventSiteId == inventSiteId
            && item.InventLocationId == inventLocationId && item.ConfigId == configId
            && item.InventSizeId == sizeId && item.InventColorId == colorId
            && item.InventStyleId == styleId && item.InventVersionId == versionId
            && item.InventBatchId == batchId && item.InventSerialId == serialId
            && item.InventStatusId == statusId && item.WmsLocationId == locationId
            && item.LicensePlateId == licensePlateId && item.InventDimension10 == dimension10
            && item.InventDimension9 == dimension9 && item.InventDimension9TzId == dimension9TzId,
            cancellationToken);
        if (existing != null)
        {
            if (ownsTransaction) await _dbContext.CommitTransactionAsync(cancellationToken);
            return existing;
        }

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
            Sha1HashHex = sha1,
            // SHA-3 is only available on newer OS builds (e.g. Windows 11 24H2+). Guard with
            // IsSupported so dimension creation does not throw PlatformNotSupportedException on
            // hosts without SHA-3; the column is auxiliary and no lookup depends on it.
            Sha3HashHex = SHA3_384.IsSupported
                ? Convert.ToHexString(SHA3_384.HashData(canonicalBytes))
                : string.Empty,
            DataAreaId = dataAreaId
        };
        _dbContext.Context.Set<InventDim>().Add(dimension);
        await _dbContext.CompleteAsync(cancellationToken);
        if (ownsTransaction) await _dbContext.CommitTransactionAsync(cancellationToken);
        return dimension;
        }
        catch
        {
            if (ownsTransaction)
            {
                await _dbContext.RollbackTransactionAsync(cancellationToken);
                _dbContext.Context.ChangeTracker.Clear();
            }
            throw;
        }
    }

    private async Task LockCombinationAsync(string resource, CancellationToken ct)
    {
        var database = _dbContext.Context.Database;
        await using var command = database.GetDbConnection().CreateCommand();
        command.Transaction = database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = "DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000; SELECT @result;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.Value = resource;
        command.Parameters.Add(parameter);
        var result = await command.ExecuteScalarAsync(ct);
        if (result is not int code || code < 0)
            throw new InvalidOperationException("Could not protect inventory dimension creation against a concurrent request.");
    }
}
