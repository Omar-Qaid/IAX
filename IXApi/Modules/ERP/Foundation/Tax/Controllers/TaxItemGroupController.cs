using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mapster;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    [ApiController]
    [Route("api/v1/TaxItemGroup")]
    [Route("api/TaxItemGroup")]
    [Route("api/v1/ItemSalesTaxGroup")]
    [Route("api/ItemSalesTaxGroup")]
    public class TaxItemGroupController : BaseController<TaxItemGroupHeading, TaxItemGroupDto>
    {
        private readonly ApplicationDbContext _db;

        public TaxItemGroupController(IBaseService<TaxItemGroupHeading> service, ApplicationDbContext db, ILogger<TaxItemGroupController> logger)
            : base(service, logger)
        {
            _db = db;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxItemGroupDto>>>> GetAll(CancellationToken cancellationToken = default)
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

            return Ok(APIResponse<IEnumerable<TaxItemGroupDto>>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxItemGroupDto>>> GetById(string id, CancellationToken cancellationToken = default)
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
            if (heading == null) return NotFound(APIResponse<TaxItemGroupDto>.Fail("Item sales tax group not found"));

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

            return Ok(APIResponse<TaxItemGroupDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxItemGroupDto>>> Create([FromBody] TaxItemGroupDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<TaxItemGroupHeading>();
            if (string.IsNullOrEmpty(entity.DataAreaId)) entity.DataAreaId = "dat";

            await OnBeforeCreateAsync(entity);
            var created = await _service.AddAsync(entity, cancellationToken);

            if (dto.Lines != null && dto.Lines.Any())
            {
                foreach (var lineDto in dto.Lines)
                {
                    if (string.IsNullOrWhiteSpace(lineDto.TaxCode)) continue;

                    await _db.TaxOnItems.AddAsync(new TaxOnItem
                    {
                        DataAreaId = string.IsNullOrEmpty(created.DataAreaId) ? "dat" : created.DataAreaId,
                        TaxItemGroup = created.TaxItemGroup,
                        TaxCode = lineDto.TaxCode,
                        TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode
                    }, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return await GetById(created.TaxItemGroup, cancellationToken);
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxItemGroupDto>>> Update(string id, [FromBody] TaxItemGroupDto dto, CancellationToken cancellationToken = default)
        {
            var searchCode = System.Uri.UnescapeDataString(id).Trim();
            TaxItemGroupHeading? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxItemGroupHeading>().FindAsync(new object[] { recId }, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxItemGroupHeading>().FirstOrDefaultAsync(x => x.TaxItemGroup == searchCode || x.TaxItemGroup.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return NotFound(APIResponse<TaxItemGroupDto>.Fail("Item sales tax group not found"));
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxItemGroup;

            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            if (!string.IsNullOrEmpty(originalCode))
            {
                existingEntity.TaxItemGroup = originalCode;
            }

            await OnBeforeUpdateAsync(existingEntity);
            var updatedEntity = await _service.UpdateAsync(existingEntity, cancellationToken);

            if (dto.Lines != null)
            {
                var currentLines = await _db.TaxOnItems
                    .Where(x => x.TaxItemGroup == existingEntity.TaxItemGroup)
                    .ToListAsync(cancellationToken);

                var dtoTaxCodes = dto.Lines
                    .Select(l => l.TaxCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .ToHashSet();

                var toRemove = currentLines.Where(l => !dtoTaxCodes.Contains(l.TaxCode)).ToList();
                if (toRemove.Any()) _db.TaxOnItems.RemoveRange(toRemove);

                foreach (var lineDto in dto.Lines)
                {
                    if (string.IsNullOrWhiteSpace(lineDto.TaxCode)) continue;

                    var line = currentLines.FirstOrDefault(l => l.TaxCode == lineDto.TaxCode);
                    if (line == null)
                    {
                        await _db.TaxOnItems.AddAsync(new TaxOnItem
                        {
                            DataAreaId = string.IsNullOrEmpty(existingEntity.DataAreaId) ? "dat" : existingEntity.DataAreaId,
                            TaxItemGroup = existingEntity.TaxItemGroup,
                            TaxCode = lineDto.TaxCode,
                            TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode
                        }, cancellationToken);
                    }
                    else
                    {
                        line.TaxExemptCode = string.IsNullOrWhiteSpace(lineDto.TaxExemptCode) ? "NONE" : lineDto.TaxExemptCode;
                        _db.TaxOnItems.Update(line);
                    }
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return await GetById(existingEntity.TaxItemGroup, cancellationToken);
        }

        [HttpPost("{id}/lines")]
        public async Task<IActionResult> AddLine(string id, [FromBody] TaxOnItemDto lineDto)
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
            if (heading == null) return NotFound("Item sales tax group not found");

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
            return Ok(APIResponse<TaxOnItemDto>.Ok(lineDto));
        }

        [HttpDelete("lines/{lineId}")]
        public async Task<IActionResult> DeleteLine(long lineId)
        {
            var line = await _db.TaxOnItems.FindAsync(lineId);
            if (line == null) return NotFound();

            _db.TaxOnItems.Remove(line);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
