using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxCodeRateService : ITaxCodeRateService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;
    private readonly ILogger<TaxCodeRateService> _logger;

    public TaxCodeRateService(IUnitOfWork unitOfWork, ILogger<TaxCodeRateService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

        public async Task SyncTaxDataRateAsync(string taxCode, decimal taxValue, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taxCode) || _db == null || _db.Set<TaxData>() == null) return;

            try
            {
                var existingData = await _db.Set<TaxData>().FirstOrDefaultAsync(td => td.TaxCode == taxCode, cancellationToken);
                if (existingData != null)
                {
                    existingData.TaxValue = taxValue;
                    _db.Set<TaxData>().Update(existingData);
                }
                else
                {
                    _db.Set<TaxData>().Add(new TaxData
                    {
                        TaxCode = taxCode,
                        TaxValue = taxValue,
                        TaxFromDate = System.DateTime.UtcNow,
                        TaxToDate = System.DateTime.UtcNow.AddYears(10),
                        DataAreaId = "dat"
                    });
                }
                await _unitOfWork.CompleteAsync(cancellationToken);
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
