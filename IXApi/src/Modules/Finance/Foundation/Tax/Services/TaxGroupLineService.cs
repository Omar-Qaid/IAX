using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxGroupLineService
{
    private readonly IFinanceDataContext _db;

    public TaxGroupLineService(IFinanceDataContext db) => _db = db;

        public async Task<(TaxGroupDataDto? Line, bool Updated)> AddLineAsync(string id, TaxGroupDataDto lineDto)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxGroupHeading? heading = null;
            if (long.TryParse(searchCode, out long recId))
            {
                heading = await _db.TaxGroupHeadings.FindAsync(recId);
            }
            if (heading == null)
            {
                heading = await _db.TaxGroupHeadings.FirstOrDefaultAsync(x => x.TaxGroup == searchCode || x.TaxGroup.ToUpper() == searchCode.ToUpper());
            }
            if (heading == null) return (null, false);

            var existingLine = await _db.TaxGroupDatas.FirstOrDefaultAsync(x => x.TaxGroup == heading.TaxGroup && x.TaxCode == lineDto.TaxCode);
            if (existingLine != null)
            {
                existingLine.TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode;
                existingLine.ExemptTax = lineDto.ExemptTax;
                existingLine.UseTax = lineDto.UseTax;
                existingLine.IntracomVat = lineDto.IntracomVat;
                existingLine.ReverseCharge_W = lineDto.ReverseCharge_W;

                await _db.SaveChangesAsync();
                return (existingLine.Adapt<TaxGroupDataDto>(), true);
            }

            var newLine = new TaxGroupData
            {
                DataAreaId = heading.DataAreaId,
                TaxGroup = heading.TaxGroup,
                TaxCode = lineDto.TaxCode ?? string.Empty,
                TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode,
                ExemptTax = lineDto.ExemptTax,
                UseTax = lineDto.UseTax,
                IntracomVat = lineDto.IntracomVat,
                ReverseCharge_W = lineDto.ReverseCharge_W
            };

            await _db.TaxGroupDatas.AddAsync(newLine);
            await _db.SaveChangesAsync();

            return (newLine.Adapt<TaxGroupDataDto>(), false);
        }

        public async Task<bool> DeleteLineAsync(long lineId)
        {
            var line = await _db.TaxGroupDatas.FindAsync(lineId);
            if (line == null) return false;

            _db.TaxGroupDatas.Remove(line);
            await _db.SaveChangesAsync();
            return true;
        }
}
