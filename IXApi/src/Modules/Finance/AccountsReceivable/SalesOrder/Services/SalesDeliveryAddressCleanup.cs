using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

internal static class SalesDeliveryAddressCleanup
{
    public static async Task CleanupAsync(DbContext db, long addressId, string dataAreaId,
        long excludingLineId, long excludingOrderId, CancellationToken cancellationToken)
    {
        if (addressId <= 0) return;
        var address = await db.Set<LogisticsPostalAddress>().FirstOrDefaultAsync(
            row => row.RecId == addressId && row.DataAreaId == dataAreaId, cancellationToken);
        if (address == null) return;

        // A customer, another document, or a confirmation may still need this address.
        if (await db.Set<DirPartyLocation>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.Location == address.Location, cancellationToken)
            || await db.Set<DirPartyTable>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.PrimaryAddressLocation == address.Location, cancellationToken)
            || await db.Set<SalesTable>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.RecId != excludingOrderId
                    && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<SalesLine>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.RecId != excludingLineId
                    && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<CustConfirmJour>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<SalesQuotationTable>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<SalesQuotationLine>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<CustQuotationJour>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<CustPackingSlipJour>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && (row.DeliveryPostalAddress == addressId || row.InvoicePostalAddress == addressId), cancellationToken)
            || await db.Set<CustPackingSlipTrans>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.DeliveryPostalAddress == addressId, cancellationToken)
            || await db.Set<CustInvoiceTable>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && (row.DeliveryPostalAddress == addressId || row.PostalAddress == addressId), cancellationToken)
            || await db.Set<CustInvoiceJour>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && (row.DeliveryPostalAddress == addressId || row.InvoicePostalAddress == addressId), cancellationToken)
            || await db.Set<CustInvoiceTrans>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.DeliveryPostalAddress == addressId, cancellationToken))
            return;

        db.Set<LogisticsPostalAddress>().Remove(address);
        var location = await db.Set<LogisticsLocation>().FirstOrDefaultAsync(
            row => row.RecId == address.Location, cancellationToken);
        if (location == null) return;

        var locationStillUsed = await db.Set<LogisticsPostalAddress>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.RecId != addressId
                    && row.Location == location.RecId, cancellationToken)
            || await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.Location == location.RecId, cancellationToken)
            || await db.Set<LogisticsLocation>().IgnoreQueryFilters().AnyAsync(
                row => !row.IsDeleted && row.ParentLocation == location.RecId, cancellationToken);
        if (!locationStillUsed)
            db.Set<LogisticsLocation>().Remove(location);
    }
}
