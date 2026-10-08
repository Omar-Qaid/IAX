using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxItemGroupLineService
{
    private readonly IFinanceDataContext _db;

    public TaxItemGroupLineService(IFinanceDataContext db) => _db = db;

        public async Task<TaxOnItemDto?> AddLineAsync(string id, TaxOnItemDto lineDto)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxItemGroupHeading? heading = null;
            if (long.TryParse(searchCode, out long recId))
            {
                heading = await _db.Set<TaxItemGroupHeading>().FindAsync(recId);
            }
            if (heading == null)
            {
                heading = await _db.Set<TaxItemGroupHeading>().FirstOrDefaultAsync(x => x.TaxItemGroup == searchCode || x.TaxItemGroup.ToUpper() == searchCode.ToUpper());
            }
            if (heading == null) return null;

            var existingLine = await _db.TaxOnItems.FirstOrDefaultAsync(x => x.TaxItemGroup == heading.TaxItemGroup && x.TaxCode == lineDto.TaxCode);
            if (existingLine != null)
            {
                existingLine.TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode;
                _db.TaxOnItems.Update(existingLine);
                await _db.SaveChangesAsync();
                lineDto.RecId = existingLine.RecId;
            }
            else
            {
                var line = new TaxOnItem
                {
                    DataAreaId = string.IsNullOrEmpty(heading.DataAreaId) ? "dat" : heading.DataAreaId,
                    TaxItemGroup = heading.TaxItemGroup,
                    TaxCode = lineDto.TaxCode,
                    TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode
                };
                await _db.TaxOnItems.AddAsync(line);
                await _db.SaveChangesAsync();
                lineDto.RecId = line.RecId;
            }

            lineDto.TaxItemGroup = heading.TaxItemGroup;
            return lineDto;
        }

        public async Task<bool> DeleteLineAsync(long lineId)
        {
            var line = await _db.TaxOnItems.FindAsync(lineId);
            if (line == null) return false;

            _db.TaxOnItems.Remove(line);
            await _db.SaveChangesAsync();
            return true;
        }
}
