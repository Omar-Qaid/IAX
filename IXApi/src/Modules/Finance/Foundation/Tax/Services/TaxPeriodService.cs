using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxPeriodService
{
    private readonly IFinanceDataContext _db;
    private readonly IBaseService<TaxPeriodHead> _headers;

    public TaxPeriodService(IFinanceDataContext db, IBaseService<TaxPeriodHead> headers)
    {
        _db = db;
        _headers = headers;
    }

        public async Task<IEnumerable<TaxPeriodHeadDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var headings = await _db.Set<TaxPeriodHead>().AsNoTracking().ToListAsync(cancellationToken);
            var allIntervals = await _db.Set<TaxReportPeriod>().AsNoTracking().ToListAsync(cancellationToken);

            var dtos = headings.Select(heading =>
            {
                var dto = heading.Adapt<TaxPeriodHeadDto>();
                dto.Intervals = allIntervals
                    .Where(i => i.TaxPeriod == heading.TaxPeriod)
                    .Select(i => i.Adapt<TaxReportPeriodDto>())
                    .ToList();
                return dto;
            }).ToList();

            return dtos;
        }

        public async Task<TaxPeriodHeadDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxPeriodHead? entity = null;
            if (long.TryParse(searchCode, out var recId))
            {
                entity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }

            if (entity == null)
            {
                entity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.TaxPeriod == searchCode || x.TaxPeriod.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }

            if (entity == null)
            {
                return null;
            }

            var dto = entity.Adapt<TaxPeriodHeadDto>();
            var intervals = await _db.Set<TaxReportPeriod>()
                .AsNoTracking()
                .Where(i => i.TaxPeriod == dto.TaxPeriod)
                .ProjectToType<TaxReportPeriodDto>()
                .ToListAsync(cancellationToken);
            dto.Intervals = intervals;

            return dto;
        }

        public Task<TaxPeriodHead> CreateAsync(TaxPeriodHeadDto dto, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var created = await CreateCoreAsync(dto, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return created;
            });

        private async Task<TaxPeriodHead> CreateCoreAsync(TaxPeriodHeadDto dto, CancellationToken cancellationToken)
        {
            var entity = dto.Adapt<TaxPeriodHead>();
            if (string.IsNullOrEmpty(entity.DataAreaId)) entity.DataAreaId = "dat";

            SanitizeEntity(entity);
            var created = await _headers.AddAsync(entity, cancellationToken);

            if (dto.Intervals != null && dto.Intervals.Any())
            {
                foreach (var intervalDto in dto.Intervals)
                {
                    var intervalEntity = intervalDto.Adapt<TaxReportPeriod>();
                    intervalEntity.TaxPeriod = created.TaxPeriod;
                    if (string.IsNullOrEmpty(intervalEntity.DataAreaId)) intervalEntity.DataAreaId = created.DataAreaId;
                    await _db.Set<TaxReportPeriod>().AddAsync(intervalEntity, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return created;
        }

        public Task<TaxPeriodHead?> UpdateAsync(string id, TaxPeriodHeadDto dto, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var updated = await UpdateCoreAsync(id, dto, cancellationToken);
                if (updated != null) await transaction.CommitAsync(cancellationToken);
                return updated;
            });

        private async Task<TaxPeriodHead?> UpdateCoreAsync(string id, TaxPeriodHeadDto dto, CancellationToken cancellationToken)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxPeriodHead? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.TaxPeriod == searchCode || x.TaxPeriod.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return null;
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxPeriod;

            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            if (!string.IsNullOrEmpty(originalCode))
            {
                existingEntity.TaxPeriod = originalCode;
            }

            SanitizeEntity(existingEntity);
            var updatedEntity = await _headers.UpdateAsync(existingEntity, cancellationToken);

            if (dto.Intervals != null)
            {
                var currentIntervals = await _db.Set<TaxReportPeriod>()
                    .Where(x => x.TaxPeriod == existingEntity.TaxPeriod)
                    .ToListAsync(cancellationToken);

                _db.Set<TaxReportPeriod>().RemoveRange(currentIntervals);

                foreach (var intervalDto in dto.Intervals)
                {
                    var intervalEntity = intervalDto.Adapt<TaxReportPeriod>();
                    intervalEntity.TaxPeriod = existingEntity.TaxPeriod;
                    intervalEntity.RecId = 0; // reset key for new insert
                    if (string.IsNullOrEmpty(intervalEntity.DataAreaId)) intervalEntity.DataAreaId = existingEntity.DataAreaId;
                    await _db.Set<TaxReportPeriod>().AddAsync(intervalEntity, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return updatedEntity;
        }

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var deleted = await DeleteCoreAsync(id, cancellationToken);
                if (deleted) await transaction.CommitAsync(cancellationToken);
                return deleted;
            });

        private async Task<bool> DeleteCoreAsync(string id, CancellationToken cancellationToken)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxPeriodHead? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.TaxPeriod == searchCode || x.TaxPeriod.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return false;
            }

            var intervals = await _db.Set<TaxReportPeriod>()
                .Where(x => x.TaxPeriod == existingEntity.TaxPeriod)
                .ToListAsync(cancellationToken);
            if (intervals.Any())
            {
                _db.Set<TaxReportPeriod>().RemoveRange(intervals);
                await _db.SaveChangesAsync(cancellationToken);
            }

            await _headers.RemoveAsync(existingEntity, cancellationToken);
            return true;
        }

        private static void SanitizeEntity(TaxPeriodHead entity)
        {
            entity.TaxPeriod = entity.TaxPeriod?.Trim() ?? string.Empty;
            entity.Name = entity.Name?.Trim() ?? string.Empty;
            entity.TaxAuthority = entity.TaxAuthority?.Trim() ?? string.Empty;
            entity.PaymentCode ??= string.Empty;
            entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? "dat" : entity.DataAreaId;
        }
}
