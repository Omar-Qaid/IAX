using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderDeliveryAddressService : ISalesOrderDeliveryAddressService
{
    private readonly IUnitOfWork _dbContext;
    private readonly ICompanyExecutionContext _company;
    private readonly ILocationService _locationService;
    private readonly IPostalAddressService _postalAddressService;

    public SalesOrderDeliveryAddressService(IUnitOfWork dbContext, ICompanyExecutionContext company,
        ILocationService locationService, IPostalAddressService postalAddressService)
    {
        _dbContext = dbContext;
        _company = company;
        _locationService = locationService;
        _postalAddressService = postalAddressService;
    }

    public async Task<SalesDeliveryAddressResult> CreateAsync(long recId, string? lineIdText,
        AddressInfoDto address, CancellationToken cancellationToken)
    {
        return await _dbContext.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var area = _company.GetDataAreaId();
            var order = await _dbContext.Context.Set<SalesTable>().FirstOrDefaultAsync(
                row => row.RecId == recId && row.DataAreaId == area, cancellationToken);
            if (order == null) return new SalesDeliveryAddressResult(SalesDeliveryAddressStatus.OrderNotFound);
            if (order.SalesStatus != SalesStatus.Backorder)
                return new SalesDeliveryAddressResult(SalesDeliveryAddressStatus.NotOpen);
            SalesLine? line = null;
            if (!string.IsNullOrWhiteSpace(lineIdText))
            {
                if (!long.TryParse(lineIdText, System.Globalization.NumberStyles.None,
                        System.Globalization.CultureInfo.InvariantCulture, out var lineId))
                    return new SalesDeliveryAddressResult(SalesDeliveryAddressStatus.BadLineId);
                line = await _dbContext.Context.Set<SalesLine>().FirstOrDefaultAsync(row => row.RecId == lineId
                    && row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken);
                if (line == null) return new SalesDeliveryAddressResult(SalesDeliveryAddressStatus.LineNotFound);
                if (line.SalesStatus != SalesStatus.Backorder || line.RemainSalesPhysical != line.SalesQty
                    || line.RemainSalesFinancial != line.SalesQty)
                    return new SalesDeliveryAddressResult(SalesDeliveryAddressStatus.ProcessedLine);
            }

            var previousAddressId = line?.DeliveryPostalAddress ?? order.DeliveryPostalAddress;
            var location = await _locationService.CreateLocationAsync(address.Description.Trim(), true, cancellationToken);
            var postalAddress = await _postalAddressService.CreatePostalAddressAsync(location.RecId, address, cancellationToken);
            postalAddress.DataAreaId = order.DataAreaId;
            if (line == null) order.DeliveryPostalAddress = postalAddress.RecId;
            else line.DeliveryPostalAddress = postalAddress.RecId;
            if (order.DocumentStatus == DocumentStatus.Confirmation)
                order.DocumentStatus = DocumentStatus.None;
            if (previousAddressId != postalAddress.RecId)
                await SalesDeliveryAddressCleanup.CleanupAsync(_dbContext.Context,
                    previousAddressId, order.DataAreaId, line?.RecId ?? 0,
                    line == null ? order.RecId : 0, cancellationToken);
            await _dbContext.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SalesDeliveryAddressResult(SalesDeliveryAddressStatus.Created,
                postalAddress.RecId, location.Description, postalAddress.Address);
        });
    }
}
