using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxGroupLineService : ITaxGroupLineService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;

    public TaxGroupLineService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<(TaxGroupDataDto? Line, bool Updated)> AddLineAsync(string id, TaxGroupDataDto lineDto)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxGroupHeading? heading = null;
            if (long.TryParse(searchCode, out long recId))
            {
                heading = await _db.Set<TaxGroupHeading>().FindAsync(recId);
            }
            if (heading == null)
            {
                heading = await _db.Set<TaxGroupHeading>().FirstOrDefaultAsync(x => x.TaxGroup == searchCode || x.TaxGroup.ToUpper() == searchCode.ToUpper());
            }
            if (heading == null) return (null, false);

            var existingLine = await _db.Set<TaxGroupData>().FirstOrDefaultAsync(x => x.TaxGroup == heading.TaxGroup && x.TaxCode == lineDto.TaxCode);
            if (existingLine != null)
            {
                new TaxGroupLineWriteSource(lineDto).Adapt(existingLine);

                await _unitOfWork.CompleteAsync();
                return (existingLine.Adapt<TaxGroupDataDto>(), true);
            }

            var newLine = new TaxGroupLineCreateSource(lineDto, heading.DataAreaId, heading.TaxGroup, false).Adapt<TaxGroupData>();

            await _db.Set<TaxGroupData>().AddAsync(newLine);
            await _unitOfWork.CompleteAsync();

            return (newLine.Adapt<TaxGroupDataDto>(), false);
        }

        public async Task<bool> DeleteLineAsync(long lineId)
        {
            var line = await _db.Set<TaxGroupData>().FindAsync(lineId);
            if (line == null) return false;

            _db.Set<TaxGroupData>().Remove(line);
            await _unitOfWork.CompleteAsync();
            return true;
        }
}
