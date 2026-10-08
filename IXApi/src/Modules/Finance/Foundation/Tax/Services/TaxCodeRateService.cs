using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxCodeRateService
{
    private readonly IFinanceDataContext _db;
    private readonly ILogger<TaxCodeRateService> _logger;

    public TaxCodeRateService(IFinanceDataContext db, ILogger<TaxCodeRateService> logger)
    {
        _db = db;
        _logger = logger;
    }

        public async Task SyncTaxDataRateAsync(string taxCode, decimal taxValue, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taxCode) || _db == null || _db.TaxData == null) return;

            try
            {
                var existingData = await _db.TaxData.FirstOrDefaultAsync(td => td.TaxCode == taxCode, cancellationToken);
                if (existingData != null)
                {
                    existingData.TaxValue = taxValue;
                    _db.TaxData.Update(existingData);
                }
                else
                {
                    _db.TaxData.Add(new TaxData
                    {
                        TaxCode = taxCode,
                        TaxValue = taxValue,
                        TaxFromDate = System.DateTime.UtcNow,
                        TaxToDate = System.DateTime.UtcNow.AddYears(10),
                        DataAreaId = "dat"
                    });
                }
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "[TaxTable] - Error syncing TaxData rate for {TaxCode}", taxCode);
            }
        }

        public static void SanitizeEntity(TaxTable entity)
        {
            entity.TaxCode = entity.TaxCode?.Trim() ?? string.Empty;
            entity.TaxName = entity.TaxName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(entity.TaxPeriod)) entity.TaxPeriod = "Monthly";
            if (string.IsNullOrWhiteSpace(entity.TaxAccountGroup)) entity.TaxAccountGroup = "STANDARD";
            if (string.IsNullOrWhiteSpace(entity.TaxCurrencyCode)) entity.TaxCurrencyCode = "SAR";
            entity.TaxOnTax ??= string.Empty;
            entity.TaxUnit ??= string.Empty;
            entity.PrintCode ??= string.Empty;
            entity.PaymentTaxCode ??= string.Empty;
            entity.TaxJurisdictionCode ??= string.Empty;
            entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? "dat" : entity.DataAreaId;
        }
}
