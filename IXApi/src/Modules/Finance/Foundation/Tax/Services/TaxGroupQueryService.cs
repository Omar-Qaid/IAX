using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxGroupQueryService
{
    private readonly IFinanceDataContext _db;

    public TaxGroupQueryService(IFinanceDataContext db) => _db = db;

        public async Task<IEnumerable<TaxGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var headings = await _db.TaxGroupHeadings.AsNoTracking().ToListAsync(cancellationToken);
            var allLines = await _db.TaxGroupDatas
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .ToListAsync(cancellationToken);

            var taxDataList = await _db.TaxData.AsNoTracking().ToListAsync(cancellationToken);

            var dtos = headings.Select(heading =>
            {
                var dto = new TaxGroupDto
                {
                    RecId = heading.RecId,
                    DataAreaId = heading.DataAreaId,
                    TaxGroup = heading.TaxGroup,
                    TaxGroupName = heading.TaxGroupName,
                    TaxGroupSetup = heading.TaxGroupSetup,
                    Source = heading.Source,
                    TaxGroupRounding = heading.TaxGroupRounding,
                    TaxReverseOnCashDisc = heading.TaxReverseOnCashDisc,
                    EuTrade_W = heading.EuTrade_W,
                    MandatorySalesDate_W = heading.MandatorySalesDate_W,
                    FillSalesDate_W = heading.FillSalesDate_W,
                    FillVatDueDatePeriodNumber = heading.FillVatDueDatePeriodNumber,
                    FillVatDueDate_W = heading.FillVatDueDate_W,
                    FillVatDueDateBasedOn = heading.FillVatDueDateBasedOn,
                    FillVatDueDatePeriod = heading.FillVatDueDatePeriod,
                    TaxPrintDetail = heading.TaxPrintDetail
                };
                dto.Lines = allLines
                    .Where(l => l.TaxGroup == heading.TaxGroup)
                    .Select(l => new TaxGroupDataDto
                    {
                        RecId = l.RecId,
                        DataAreaId = l.DataAreaId,
                        TaxGroup = l.TaxGroup,
                        TaxCode = l.TaxCode,
                        TaxExemptCode = l.TaxExemptCode,
                        ExemptTax = l.ExemptTax,
                        UseTax = l.UseTax,
                        IntracomVat = l.IntracomVat,
                        ReverseCharge_W = l.ReverseCharge_W,
                        TaxCodeName = l.TaxTable?.TaxName,
                        TaxValue = taxDataList.Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0
                    }).ToList();
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
                heading = await _db.TaxGroupHeadings.FindAsync(new object[] { recId }, cancellationToken);
            }
            if (heading == null)
            {
                heading = await _db.TaxGroupHeadings.FirstOrDefaultAsync(x =>
                    x.TaxGroup == searchCode ||
                    x.TaxGroup.ToUpper() == searchCode.ToUpper() ||
                    (searchCode.Equals("Export", StringComparison.OrdinalIgnoreCase) && (x.TaxGroup == "EXP" || x.TaxGroup == "EXPORT")) ||
                    (searchCode.Equals("EXP", StringComparison.OrdinalIgnoreCase) && (x.TaxGroup == "EXPORT" || x.TaxGroup == "EXP")), cancellationToken);
            }
            if (heading == null) return null;

            var dto = new TaxGroupDto
            {
                RecId = heading.RecId,
                DataAreaId = heading.DataAreaId,
                TaxGroup = heading.TaxGroup,
                TaxGroupName = heading.TaxGroupName,
                TaxGroupSetup = heading.TaxGroupSetup,
                Source = heading.Source,
                TaxGroupRounding = heading.TaxGroupRounding,
                TaxReverseOnCashDisc = heading.TaxReverseOnCashDisc,
                EuTrade_W = heading.EuTrade_W,
                MandatorySalesDate_W = heading.MandatorySalesDate_W,
                FillSalesDate_W = heading.FillSalesDate_W,
                FillVatDueDatePeriodNumber = heading.FillVatDueDatePeriodNumber,
                FillVatDueDate_W = heading.FillVatDueDate_W,
                FillVatDueDateBasedOn = heading.FillVatDueDateBasedOn,
                FillVatDueDatePeriod = heading.FillVatDueDatePeriod,
                TaxPrintDetail = heading.TaxPrintDetail
            };

            var lines = await _db.TaxGroupDatas
                .AsNoTracking()
                .Include(x => x.TaxTable)
                .Where(x => x.TaxGroup == heading.TaxGroup)
                .ToListAsync(cancellationToken);

            dto.Lines = lines.Select(l => new TaxGroupDataDto
            {
                RecId = l.RecId,
                DataAreaId = l.DataAreaId,
                TaxGroup = l.TaxGroup,
                TaxCode = l.TaxCode,
                TaxExemptCode = l.TaxExemptCode,
                ExemptTax = l.ExemptTax,
                UseTax = l.UseTax,
                IntracomVat = l.IntracomVat,
                ReverseCharge_W = l.ReverseCharge_W,
                TaxCodeName = l.TaxTable?.TaxName,
                TaxValue = _db.TaxData.Where(td => td.TaxCode == l.TaxCode).Select(td => (decimal?)td.TaxValue).FirstOrDefault() ?? 0
            }).ToList();

            return dto;
        }

}
