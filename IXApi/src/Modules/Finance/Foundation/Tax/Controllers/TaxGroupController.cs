using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mapster;


namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/TaxGroup")]
    [Route("api/TaxGroup")]
    [Route("api/v1/SalesTaxGroup")]
    [Route("api/SalesTaxGroup")]
    [DomainPermission("Tax", "TaxGroups")]
    public class TaxGroupController : BaseController<TaxGroupHeading, TaxGroupDto>
    {
        private readonly IFinanceDataContext _db;

        public TaxGroupController(IBaseService<TaxGroupHeading> service, IFinanceDataContext db, ILogger<TaxGroupController> logger)
            : base(service, logger)
        {
            _db = db;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxGroupDto>>>> GetAll(CancellationToken cancellationToken = default)
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

            return Ok(APIResponse<IEnumerable<TaxGroupDto>>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxGroupDto>>> GetById(string id, CancellationToken cancellationToken = default)
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
            if (heading == null) return NotFound(APIResponse<TaxGroupDto>.Fail("Tax group not found"));

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

            return Ok(APIResponse<TaxGroupDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxGroupDto>>> Create([FromBody] TaxGroupDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<TaxGroupHeading>();
            await OnBeforeCreateAsync(entity);
            var created = await _service.AddAsync(entity, cancellationToken);

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

            var resultDto = created.Adapt<TaxGroupDto>();
            return Ok(APIResponse<TaxGroupDto>.Ok(resultDto, "Created successfully"));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxGroupDto>>> Update(string id, [FromBody] TaxGroupDto dto, CancellationToken cancellationToken = default)
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
                return NotFound(APIResponse<TaxGroupDto>.Fail("Sales tax group not found"));
            }

            var originalRecId = existingEntity.RecId;
            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            await OnBeforeUpdateAsync(existingEntity);

            var updatedEntity = await _service.UpdateAsync(existingEntity, cancellationToken);

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

            var resultDto = updatedEntity.Adapt<TaxGroupDto>();
            return Ok(APIResponse<TaxGroupDto>.Ok(resultDto, "Updated successfully"));
        }

        [HttpPost("{id}/lines")]
        public async Task<IActionResult> AddLine(string id, [FromBody] TaxGroupDataDto lineDto)
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
            if (heading == null) return NotFound("Tax group not found");

            var existingLine = await _db.TaxGroupDatas.FirstOrDefaultAsync(x => x.TaxGroup == heading.TaxGroup && x.TaxCode == lineDto.TaxCode);
            if (existingLine != null)
            {
                existingLine.TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode;
                existingLine.ExemptTax = lineDto.ExemptTax;
                existingLine.UseTax = lineDto.UseTax;
                existingLine.IntracomVat = lineDto.IntracomVat;
                existingLine.ReverseCharge_W = lineDto.ReverseCharge_W;
                
                await _db.SaveChangesAsync();
                return Ok(APIResponse<TaxGroupDataDto>.Ok(existingLine.Adapt<TaxGroupDataDto>(), "Line updated successfully"));
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

            return Ok(APIResponse<TaxGroupDataDto>.Ok(newLine.Adapt<TaxGroupDataDto>(), "Line added successfully"));
        }

        [HttpDelete("lines/{lineId}")]
        public async Task<IActionResult> DeleteLine(long lineId)
        {
            var line = await _db.TaxGroupDatas.FindAsync(lineId);
            if (line == null) return NotFound();

            _db.TaxGroupDatas.Remove(line);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}

