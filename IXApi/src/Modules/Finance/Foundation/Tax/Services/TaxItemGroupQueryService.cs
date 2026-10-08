using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxItemGroupQueryService
{
    private readonly IFinanceDataContext _db;

    public TaxItemGroupQueryService(IFinanceDataContext db) => _db = db;

        public async Task<IEnumerable<TaxItemGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var headings = await _db.Set<TaxItemGroupHeading>().AsNoTracking().ToListAsync(cancellationToken);
            var allLines = await _db.TaxOnItems
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .ToListAsync(cancellationToken);

            var taxDataList = await _db.TaxData.AsNoTracking().ToListAsync(cancellationToken);

            var dtos = headings.Select(heading =>
            {
                var dto = new TaxItemGroupDto
                {
                    RecId = heading.RecId,
                    DataAreaId = heading.DataAreaId,
                    TaxItemGroup = heading.TaxItemGroup,
                    Name = heading.Name,
                    Source = heading.Source,
                    EuSalesListType = heading.EuSalesListType
                };
                dto.Lines = allLines
                    .Where(l => l.TaxItemGroup == heading.TaxItemGroup)
                    .Select(l => new TaxOnItemDto
                    {
                        RecId = l.RecId,
                        DataAreaId = l.DataAreaId,
                        TaxItemGroup = l.TaxItemGroup,
                        TaxCode = l.TaxCode,
                        TaxExemptCode = l.TaxExemptCode,
                        TaxCodeName = l.TaxTable?.TaxName,
                        TaxValue = taxDataList.Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0
                    }).ToList();
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

            var dto = new TaxItemGroupDto
            {
                RecId = heading.RecId,
                DataAreaId = heading.DataAreaId,
                TaxItemGroup = heading.TaxItemGroup,
                Name = heading.Name,
                Source = heading.Source,
                EuSalesListType = heading.EuSalesListType
            };

            var lines = await _db.TaxOnItems
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .Where(x => x.TaxItemGroup == heading.TaxItemGroup)
                .ToListAsync(cancellationToken);

            dto.Lines = lines.Select(l => new TaxOnItemDto
            {
                RecId = l.RecId,
                DataAreaId = l.DataAreaId,
                TaxItemGroup = l.TaxItemGroup,
                TaxCode = l.TaxCode,
                TaxExemptCode = l.TaxExemptCode,
                TaxCodeName = l.TaxTable?.TaxName,
                TaxValue = _db.TaxData.Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0
            }).ToList();

            return dto;
        }

}
