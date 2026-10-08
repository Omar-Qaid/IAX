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

    public TaxTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ITaxCodeRateService rates, ILogger<TaxTableService> logger) : base(unitOfWork, currentUser)
    {
        _rates = rates;
        _logger = logger;
    }
        public async Task<IEnumerable<TaxTableDto>> GetDetailsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("[TaxTable] - Fetching all sales tax codes with rate values");
                var entities = await GetAllAsync(cancellationToken: cancellationToken);
                var dtos = entities.Adapt<List<TaxTableDto>>();

                try
                {
                    if (_unitOfWork.Context.Set<TaxData>() != null)
                    {
                        var rates = await _unitOfWork.Context.Set<TaxData>().AsNoTracking().ToListAsync(cancellationToken);
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
                entity = await _unitOfWork.Context.Set<TaxTable>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }

            if (entity == null)
            {
                entity = await _unitOfWork.Context.Set<TaxTable>().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
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
                    var rate = await _unitOfWork.Context.Set<TaxData>().AsNoTracking().FirstOrDefaultAsync(r => r.TaxCode == dto.TaxCode, cancellationToken);
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

        public async Task<TaxTableDto?> CreateFromDtoAsync(TaxTableDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<TaxTable>();
            if (string.IsNullOrEmpty(entity.DataAreaId)) entity.DataAreaId = "dat";

            TaxCodeRateService.SanitizeEntity(entity);
            var created = await AddAsync(entity, cancellationToken);

            await _rates.SyncTaxDataRateAsync(created.TaxCode, dto.TaxValue, cancellationToken);

            return await GetDetailAsync(created.TaxCode, cancellationToken);
        }

        public async Task<TaxTableDto?> UpdateFromDtoAsync(string id, TaxTableDto dto, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxTable? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _unitOfWork.Context.Set<TaxTable>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _unitOfWork.Context.Set<TaxTable>().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return null;
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxCode;

            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
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
                existingEntity = await _unitOfWork.Context.Set<TaxTable>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _unitOfWork.Context.Set<TaxTable>().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return false;
            }

            await RemoveAsync(existingEntity, cancellationToken);
            return true;
        }

}
