using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxCodeRateService : ITaxCodeRateService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;
    private readonly ILogger<TaxCodeRateService> _logger;
    private readonly ICurrentUserService _currentUser;

    public TaxCodeRateService(IUnitOfWork unitOfWork, ILogger<TaxCodeRateService> logger, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task SyncTaxDataRateAsync(string taxCode, decimal taxValue, CancellationToken cancellationToken)
    {
        var area = _currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) throw new InvalidOperationException("A company must be selected before changing tax rates.");
        if (string.IsNullOrWhiteSpace(taxCode) || _db == null || _db.Set<TaxData>() == null) return;

        try
        {
            var existingData = await _db.Set<TaxData>().FirstOrDefaultAsync(td => td.DataAreaId == area && td.TaxCode == taxCode, cancellationToken);
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
                    DataAreaId = area
                });
            }
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "[TaxTable] - Error syncing TaxData rate for {TaxCode}", taxCode);
            throw;
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
    }
}
