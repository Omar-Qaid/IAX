using Mapster;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxItemGroupQueryService : ITaxItemGroupQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;

    public TaxItemGroupQueryService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<TaxItemGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var headings = await _db.Set<TaxItemGroupHeading>().AsNoTracking().ToListAsync(cancellationToken);
            var allLines = await _db.Set<TaxOnItem>()
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .ToListAsync(cancellationToken);

            var taxDataList = await _db.Set<TaxData>().AsNoTracking().ToListAsync(cancellationToken);

            var dtos = headings.Select(heading =>
            {
                var dto = new TaxItemGroupReadSource(heading).Adapt<TaxItemGroupDto>();
                dto.Lines = allLines
                    .Where(l => l.TaxItemGroup == heading.TaxItemGroup)
                    .Select(l => new TaxItemGroupLineReadSource(l, taxDataList.Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0).Adapt<TaxOnItemDto>()).ToList();
                return dto;
            }).ToList();

            return dtos;
        }

        public async Task<TaxItemGroupDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxItemGroupHeading? heading = null;
            if (long.TryParse(searchCode, out long recId))
            {
                heading = await _db.Set<TaxItemGroupHeading>().FindAsync(new object[] { recId }, cancellationToken);
            }
            if (heading == null)
            {
                heading = await _db.Set<TaxItemGroupHeading>().AsNoTracking().FirstOrDefaultAsync(x => x.TaxItemGroup == searchCode || x.TaxItemGroup.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (heading == null) return null;

            var dto = new TaxItemGroupReadSource(heading).Adapt<TaxItemGroupDto>();

            var lines = await _db.Set<TaxOnItem>()
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .Where(x => x.TaxItemGroup == heading.TaxItemGroup)
                .ToListAsync(cancellationToken);

            dto.Lines = lines.Select(l => new TaxItemGroupLineReadSource(l, _db.Set<TaxData>().Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0).Adapt<TaxOnItemDto>()).ToList();

            return dto;
        }

}
