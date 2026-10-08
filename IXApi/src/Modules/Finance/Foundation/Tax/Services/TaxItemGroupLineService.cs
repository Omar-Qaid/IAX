using Mapster;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxItemGroupLineService : ITaxItemGroupLineService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;

    public TaxItemGroupLineService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

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

            var existingLine = await _db.Set<TaxOnItem>().FirstOrDefaultAsync(x => x.TaxItemGroup == heading.TaxItemGroup && x.TaxCode == lineDto.TaxCode);
            if (existingLine != null)
            {
                new TaxItemGroupLineWriteSource(lineDto).Adapt(existingLine);
                _db.Set<TaxOnItem>().Update(existingLine);
                await _unitOfWork.CompleteAsync();
                lineDto.RecId = existingLine.RecId;
            }
            else
            {
                var line = new TaxItemGroupLineCreateSource(lineDto, string.IsNullOrEmpty(heading.DataAreaId) ? "dat" : heading.DataAreaId, heading.TaxItemGroup).Adapt<TaxOnItem>();
                await _db.Set<TaxOnItem>().AddAsync(line);
                await _unitOfWork.CompleteAsync();
                lineDto.RecId = line.RecId;
            }

            lineDto.TaxItemGroup = heading.TaxItemGroup;
            return lineDto;
        }

        public async Task<bool> DeleteLineAsync(long lineId)
        {
            var line = await _db.Set<TaxOnItem>().FindAsync(lineId);
            if (line == null) return false;

            _db.Set<TaxOnItem>().Remove(line);
            await _unitOfWork.CompleteAsync();
            return true;
        }
}
