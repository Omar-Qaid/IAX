using Mapster;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxGroupQueryService : ITaxGroupQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;

    public TaxGroupQueryService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<TaxGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var headings = await _db.Set<TaxGroupHeading>().AsNoTracking().ToListAsync(cancellationToken);
            var allLines = await _db.Set<TaxGroupData>()
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .ToListAsync(cancellationToken);

            var taxDataList = await _db.Set<TaxData>().AsNoTracking().ToListAsync(cancellationToken);

            var dtos = headings.Select(heading =>
            {
                var dto = new TaxGroupReadSource(heading).Adapt<TaxGroupDto>();
                dto.Lines = allLines
                    .Where(l => l.TaxGroup == heading.TaxGroup)
                    .Select(l => new TaxGroupLineReadSource(l, taxDataList.Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0).Adapt<TaxGroupDataDto>()).ToList();
                return dto;
            }).ToList();

            return dtos;
        }

        public async Task<TaxGroupDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxGroupHeading? heading = null;
            if (long.TryParse(searchCode, out long recId))
            {
                heading = await _db.Set<TaxGroupHeading>().FindAsync(new object[] { recId }, cancellationToken);
            }
            if (heading == null)
            {
                heading = await _db.Set<TaxGroupHeading>().FirstOrDefaultAsync(x =>
                    x.TaxGroup == searchCode ||
                    x.TaxGroup.ToUpper() == searchCode.ToUpper() ||
                    (searchCode.Equals("Export", StringComparison.OrdinalIgnoreCase) && (x.TaxGroup == "EXP" || x.TaxGroup == "EXPORT")) ||
                    (searchCode.Equals("EXP", StringComparison.OrdinalIgnoreCase) && (x.TaxGroup == "EXPORT" || x.TaxGroup == "EXP")), cancellationToken);
            }
            if (heading == null) return null;

            var dto = new TaxGroupReadSource(heading).Adapt<TaxGroupDto>();

            var lines = await _db.Set<TaxGroupData>()
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .Where(x => x.TaxGroup == heading.TaxGroup)
                .ToListAsync(cancellationToken);

            dto.Lines = lines.Select(l => new TaxGroupLineReadSource(l, _db.Set<TaxData>().Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0).Adapt<TaxGroupDataDto>()).ToList();

            return dto;
        }

}
