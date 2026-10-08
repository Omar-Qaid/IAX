using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxGroupCommandService : ITaxGroupCommandService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;
    private readonly IBaseService<TaxGroupHeading> _headers;

    public TaxGroupCommandService(IUnitOfWork unitOfWork, IBaseService<TaxGroupHeading> headers)
    {
        _unitOfWork = unitOfWork;
        _headers = headers;
    }

        public Task<TaxGroupHeading> CreateAsync(TaxGroupDto dto, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var created = await CreateCoreAsync(dto, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return created;
            });

        private async Task<TaxGroupHeading> CreateCoreAsync(TaxGroupDto dto, CancellationToken cancellationToken)
        {
            var entity = dto.Adapt<TaxGroupHeading>();

            var created = await _headers.AddAsync(entity, cancellationToken);

            if (dto.Lines != null && dto.Lines.Any())
            {
                foreach (var lineDto in dto.Lines)
                {
                    await _db.Set<TaxGroupData>().AddAsync(new TaxGroupLineCreateSource(lineDto, created.DataAreaId, created.TaxGroup, true).Adapt<TaxGroupData>(), cancellationToken);
                }
                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            return created;
        }

        public Task<TaxGroupHeading?> UpdateAsync(string id, TaxGroupDto dto, CancellationToken cancellationToken = default)
            => _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var updated = await UpdateCoreAsync(id, dto, cancellationToken);
                if (updated != null) await transaction.CommitAsync(cancellationToken);
                return updated;
            });

        private async Task<TaxGroupHeading?> UpdateCoreAsync(string id, TaxGroupDto dto, CancellationToken cancellationToken)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxGroupHeading? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxGroupHeading>().FindAsync(new object[] { recId }, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxGroupHeading>().FirstOrDefaultAsync(x =>
                    x.TaxGroup == searchCode ||
                    x.TaxGroup.ToUpper() == searchCode.ToUpper() ||
                    (searchCode.Equals("Export", StringComparison.OrdinalIgnoreCase) && (x.TaxGroup == "EXP" || x.TaxGroup == "EXPORT")) ||
                    (searchCode.Equals("EXP", StringComparison.OrdinalIgnoreCase) && (x.TaxGroup == "EXPORT" || x.TaxGroup == "EXP")), cancellationToken);
            }
            if (existingEntity == null)
            {
                return null;
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxGroup;
            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            existingEntity.TaxGroup = originalCode;

            var updatedEntity = await _headers.UpdateAsync(existingEntity, cancellationToken);

            if (dto.Lines != null)
            {
                var currentLines = await _db.Set<TaxGroupData>()
                    .Where(x => x.TaxGroup == existingEntity.TaxGroup)
                    .ToListAsync(cancellationToken);

                var dtoTaxCodes = dto.Lines.Select(l => l.TaxCode).ToHashSet();
                var toRemove = currentLines.Where(l => !dtoTaxCodes.Contains(l.TaxCode)).ToList();
                if (toRemove.Any()) _db.Set<TaxGroupData>().RemoveRange(toRemove);

                foreach (var lineDto in dto.Lines)
                {
                    var line = currentLines.FirstOrDefault(l => l.TaxCode == lineDto.TaxCode);
                    if (line == null)
                    {
                        await _db.Set<TaxGroupData>().AddAsync(new TaxGroupLineCreateSource(lineDto, existingEntity.DataAreaId, existingEntity.TaxGroup, false).Adapt<TaxGroupData>(), cancellationToken);
                    }
                    else
                    {
                        new TaxGroupLineWriteSource(lineDto).Adapt(line);
                        _db.Set<TaxGroupData>().Update(line);
                    }
                }
                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            return updatedEntity;
        }

}
