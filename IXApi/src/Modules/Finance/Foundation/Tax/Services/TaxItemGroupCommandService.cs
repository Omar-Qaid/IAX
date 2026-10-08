using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxItemGroupCommandService
{
    private readonly IFinanceDataContext _db;
    private readonly IBaseService<TaxItemGroupHeading> _headers;

    public TaxItemGroupCommandService(IFinanceDataContext db, IBaseService<TaxItemGroupHeading> headers)
    {
        _db = db;
        _headers = headers;
    }

        public Task<TaxItemGroupHeading> CreateAsync(TaxItemGroupDto dto, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var created = await CreateCoreAsync(dto, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return created;
            });

        private async Task<TaxItemGroupHeading> CreateCoreAsync(TaxItemGroupDto dto, CancellationToken cancellationToken)
        {
            var entity = dto.Adapt<TaxItemGroupHeading>();
            if (string.IsNullOrEmpty(entity.DataAreaId)) entity.DataAreaId = "dat";

            var created = await _headers.AddAsync(entity, cancellationToken);

            if (dto.Lines != null && dto.Lines.Any())
            {
                foreach (var lineDto in dto.Lines)
                {
                    if (string.IsNullOrWhiteSpace(lineDto.TaxCode)) continue;

                    await _db.TaxOnItems.AddAsync(new TaxOnItem
                    {
                        DataAreaId = string.IsNullOrEmpty(created.DataAreaId) ? "dat" : created.DataAreaId,
                        TaxItemGroup = created.TaxItemGroup,
                        TaxCode = lineDto.TaxCode,
                        TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode
                    }, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return created;
        }

        public Task<TaxItemGroupHeading?> UpdateAsync(string id, TaxItemGroupDto dto, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var updated = await UpdateCoreAsync(id, dto, cancellationToken);
                if (updated != null) await transaction.CommitAsync(cancellationToken);
                return updated;
            });

        private async Task<TaxItemGroupHeading?> UpdateCoreAsync(string id, TaxItemGroupDto dto, CancellationToken cancellationToken)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxItemGroupHeading? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxItemGroupHeading>().FindAsync(new object[] { recId }, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxItemGroupHeading>().FirstOrDefaultAsync(x => x.TaxItemGroup == searchCode || x.TaxItemGroup.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return null;
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxItemGroup;

            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            if (!string.IsNullOrEmpty(originalCode))
            {
                existingEntity.TaxItemGroup = originalCode;
            }

            var updatedEntity = await _headers.UpdateAsync(existingEntity, cancellationToken);

            if (dto.Lines != null)
            {
                var currentLines = await _db.TaxOnItems
                    .Where(x => x.TaxItemGroup == existingEntity.TaxItemGroup)
                    .ToListAsync(cancellationToken);

                var dtoTaxCodes = dto.Lines
                    .Select(l => l.TaxCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .ToHashSet();

                var toRemove = currentLines.Where(l => !dtoTaxCodes.Contains(l.TaxCode)).ToList();
                if (toRemove.Any()) _db.TaxOnItems.RemoveRange(toRemove);

                foreach (var lineDto in dto.Lines)
                {
                    if (string.IsNullOrWhiteSpace(lineDto.TaxCode)) continue;

                    var line = currentLines.FirstOrDefault(l => l.TaxCode == lineDto.TaxCode);
                    if (line == null)
                    {
                        await _db.TaxOnItems.AddAsync(new TaxOnItem
                        {
                            DataAreaId = string.IsNullOrEmpty(existingEntity.DataAreaId) ? "dat" : existingEntity.DataAreaId,
                            TaxItemGroup = existingEntity.TaxItemGroup,
                            TaxCode = lineDto.TaxCode,
                            TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode
                        }, cancellationToken);
                    }
                    else
                    {
                        line.TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode;
                        _db.TaxOnItems.Update(line);
                    }
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return updatedEntity;
        }

}
