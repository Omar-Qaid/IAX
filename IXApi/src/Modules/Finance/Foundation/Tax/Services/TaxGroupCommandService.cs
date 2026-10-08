using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxGroupCommandService
{
    private readonly IFinanceDataContext _db;
    private readonly IBaseService<TaxGroupHeading> _headers;

    public TaxGroupCommandService(IFinanceDataContext db, IBaseService<TaxGroupHeading> headers)
    {
        _db = db;
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
                    await _db.TaxGroupDatas.AddAsync(new TaxGroupData
                    {
                        DataAreaId = created.DataAreaId,
                        TaxGroup = created.TaxGroup,
                        TaxCode = lineDto.TaxCode ?? string.Empty,
                        TaxExemptCode = lineDto.TaxExemptCode ?? "NONE",
                        ExemptTax = lineDto.ExemptTax,
                        UseTax = lineDto.UseTax,
                        IntracomVat = lineDto.IntracomVat,
                        ReverseCharge_W = lineDto.ReverseCharge_W
                    }, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
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
                existingEntity = await _db.TaxGroupHeadings.FindAsync(new object[] { recId }, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.TaxGroupHeadings.FirstOrDefaultAsync(x =>
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
                var currentLines = await _db.TaxGroupDatas
                    .Where(x => x.TaxGroup == existingEntity.TaxGroup)
                    .ToListAsync(cancellationToken);

                var dtoTaxCodes = dto.Lines.Select(l => l.TaxCode).ToHashSet();
                var toRemove = currentLines.Where(l => !dtoTaxCodes.Contains(l.TaxCode)).ToList();
                if (toRemove.Any()) _db.TaxGroupDatas.RemoveRange(toRemove);

                foreach (var lineDto in dto.Lines)
                {
                    var line = currentLines.FirstOrDefault(l => l.TaxCode == lineDto.TaxCode);
                    if (line == null)
                    {
                        await _db.TaxGroupDatas.AddAsync(new TaxGroupData
                        {
                            DataAreaId = existingEntity.DataAreaId,
                            TaxGroup = existingEntity.TaxGroup,
                            TaxCode = lineDto.TaxCode ?? string.Empty,
                            TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode,
                            ExemptTax = lineDto.ExemptTax,
                            UseTax = lineDto.UseTax,
                            IntracomVat = lineDto.IntracomVat,
                            ReverseCharge_W = lineDto.ReverseCharge_W
                        }, cancellationToken);
                    }
                    else
                    {
                        line.TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode;
                        line.ExemptTax = lineDto.ExemptTax;
                        line.UseTax = lineDto.UseTax;
                        line.IntracomVat = lineDto.IntracomVat;
                        line.ReverseCharge_W = lineDto.ReverseCharge_W;
                        _db.TaxGroupDatas.Update(line);
                    }
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return updatedEntity;
        }

}
