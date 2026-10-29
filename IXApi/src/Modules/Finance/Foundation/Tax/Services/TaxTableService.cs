using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxTableService : BaseService<TaxTable>, ITaxTableService
{
    private readonly ITaxCodeRateService _rates;
    private readonly ILogger<TaxTableService> _logger;

    private string SelectedCompany() => !string.IsNullOrWhiteSpace(_currentUser.GetDataAreaId())
        ? _currentUser.GetDataAreaId() : throw new InvalidOperationException("A company must be selected before accessing tax codes.");

    protected override IQueryable<TaxTable> ApplyDomainFilters(IQueryable<TaxTable> query)
    {
        var area = SelectedCompany();
        return query.Where(x => x.DataAreaId == area);
    }

    private IQueryable<TaxTable> CompanyTaxCodes()
    {
        var area = SelectedCompany();
        return _unitOfWork.Context.Set<TaxTable>().Where(x => x.DataAreaId == area);
    }

    public TaxTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ITaxCodeRateService rates, ILogger<TaxTableService> logger) : base(unitOfWork, currentUser)
    {
        _rates = rates;
        _logger = logger;
    }
    public async Task<IEnumerable<TaxTableDto>> GetDetailsAsync(CancellationToken cancellationToken = default)
    {
        var area = SelectedCompany();
        try
        {
            _logger.LogInformation("[TaxTable] - Fetching all sales tax codes with rate values");
            var entities = await GetAllAsync(cancellationToken: cancellationToken);
            var dtos = entities.Adapt<List<TaxTableDto>>();

            try
            {
                if (_unitOfWork.Context.Set<TaxData>() != null)
                {
                    var rates = await _unitOfWork.Context.Set<TaxData>().AsNoTracking().Where(x => x.DataAreaId == area).ToListAsync(cancellationToken);
                    foreach (var dto in dtos)
                    {
                        var match = rates.FirstOrDefault(r => r.TaxCode == dto.TaxCode);
                        if (match != null)
                        {
                            dto.TaxValue = match.TaxValue;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "[TaxTable] - Could not load TaxData rate values, returning base tax codes.");
            }

            return dtos;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "[TaxTable] - Error in GetAll, returning empty list fallback.");
            return new List<TaxTableDto>();
        }
    }

    public async Task<TaxTableDto?> GetDetailAsync(string id, CancellationToken cancellationToken = default)
    {
        var searchCode = Uri.UnescapeDataString(id).Trim();
        TaxTable? entity = null;
        if (long.TryParse(searchCode, out var recId))
        {
            entity = await CompanyTaxCodes().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
        }

        if (entity == null)
        {
            entity = await CompanyTaxCodes().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
        }

        if (entity == null)
        {
            return null;
        }

        var dto = entity.Adapt<TaxTableDto>();
        try
        {
            if (_unitOfWork.Context.Set<TaxData>() != null)
            {
                var rate = await _unitOfWork.Context.Set<TaxData>().AsNoTracking().FirstOrDefaultAsync(r => r.DataAreaId == entity.DataAreaId && r.TaxCode == dto.TaxCode, cancellationToken);
                if (rate != null)
                {
                    dto.TaxValue = rate.TaxValue;
                }
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "[TaxTable] - Could not load TaxData rate for {TaxCode}", dto.TaxCode);
        }

        return dto;
    }

    public Task<TaxTableDto?> CreateFromDtoAsync(TaxTableDto dto, CancellationToken cancellationToken = default) =>
        TaxCodeSaveTransaction.RunAsync(_unitOfWork.Context, () => CreateCoreAsync(dto, cancellationToken), cancellationToken);

    private async Task<TaxTableDto?> CreateCoreAsync(TaxTableDto dto, CancellationToken cancellationToken)
    {
        var entity = dto.Adapt<TaxTable>();
        entity.DataAreaId = SelectedCompany();

        TaxCodeRateService.SanitizeEntity(entity);
        var created = await AddAsync(entity, cancellationToken);

        await _rates.SyncTaxDataRateAsync(created.TaxCode, dto.TaxValue, cancellationToken);

        return await GetDetailAsync(created.TaxCode, cancellationToken);
    }

    public Task<TaxTableDto?> UpdateFromDtoAsync(string id, TaxTableDto dto, CancellationToken cancellationToken = default) =>
        TaxCodeSaveTransaction.RunAsync(_unitOfWork.Context, () => UpdateCoreAsync(id, dto, cancellationToken), cancellationToken);

    private async Task<TaxTableDto?> UpdateCoreAsync(string id, TaxTableDto dto, CancellationToken cancellationToken)
    {
        var searchCode = Uri.UnescapeDataString(id).Trim();
        TaxTable? existingEntity = null;
        if (long.TryParse(searchCode, out long recId))
        {
            existingEntity = await CompanyTaxCodes().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
        }
        if (existingEntity == null)
        {
            existingEntity = await CompanyTaxCodes().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
        }
        if (existingEntity == null)
        {
            return null;
        }

        var originalRecId = existingEntity.RecId;
        var originalCode = existingEntity.TaxCode;
        var originalArea = existingEntity.DataAreaId;

        dto.Adapt(existingEntity);
        existingEntity.RecId = originalRecId;
        existingEntity.DataAreaId = originalArea;
        if (!string.IsNullOrEmpty(originalCode))
        {
            existingEntity.TaxCode = originalCode;
        }

        TaxCodeRateService.SanitizeEntity(existingEntity);
        var updatedEntity = await UpdateAsync(existingEntity, cancellationToken);

        await _rates.SyncTaxDataRateAsync(existingEntity.TaxCode, dto.TaxValue, cancellationToken);

        return await GetDetailAsync(existingEntity.TaxCode, cancellationToken);
    }

    public async Task<bool> DeleteByCodeAsync(string id, CancellationToken cancellationToken = default)
    {
        var searchCode = Uri.UnescapeDataString(id).Trim();
        TaxTable? existingEntity = null;
        if (long.TryParse(searchCode, out long recId))
        {
            existingEntity = await CompanyTaxCodes().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
        }
        if (existingEntity == null)
        {
            existingEntity = await CompanyTaxCodes().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
        }
        if (existingEntity == null)
        {
            return false;
        }

        await RemoveAsync(existingEntity, cancellationToken);
        return true;
    }

}
