using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/TaxPeriodHead")]
    [Route("api/TaxPeriodHead")]
    [Route("api/v1/TaxPeriod")]
    [Route("api/TaxPeriod")]
    public class TaxPeriodHeadController : BaseController<TaxPeriodHead, TaxPeriodHeadDto>
    {
        private readonly ApplicationDbContext _db;

        public TaxPeriodHeadController(IBaseService<TaxPeriodHead> service, ApplicationDbContext db, ILogger<TaxPeriodHeadController> logger)
            : base(service, logger)
        {
            _db = db;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxPeriodHeadDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            var headings = await _db.Set<TaxPeriodHead>().AsNoTracking().ToListAsync(cancellationToken);
            var allIntervals = await _db.Set<TaxReportPeriod>().AsNoTracking().ToListAsync(cancellationToken);

            var dtos = headings.Select(heading =>
            {
                var dto = heading.Adapt<TaxPeriodHeadDto>();
                dto.Intervals = allIntervals
                    .Where(i => i.TaxPeriod == heading.TaxPeriod)
                    .Select(i => i.Adapt<TaxReportPeriodDto>())
                    .ToList();
                return dto;
            }).ToList();

            return Ok(APIResponse<IEnumerable<TaxPeriodHeadDto>>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxPeriodHeadDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxPeriodHead? entity = null;
            if (long.TryParse(searchCode, out var recId))
            {
                entity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }

            if (entity == null)
            {
                entity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.TaxPeriod == searchCode || x.TaxPeriod.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }

            if (entity == null)
            {
                return NotFound(APIResponse<TaxPeriodHeadDto>.Fail("TaxPeriodHead not found"));
            }

            var dto = entity.Adapt<TaxPeriodHeadDto>();
            var intervals = await _db.Set<TaxReportPeriod>()
                .AsNoTracking()
                .Where(i => i.TaxPeriod == dto.TaxPeriod)
                .ProjectToType<TaxReportPeriodDto>()
                .ToListAsync(cancellationToken);
            dto.Intervals = intervals;

            return Ok(APIResponse<TaxPeriodHeadDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxPeriodHeadDto>>> Create([FromBody] TaxPeriodHeadDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<TaxPeriodHead>();
            if (string.IsNullOrEmpty(entity.DataAreaId)) entity.DataAreaId = "dat";

            SanitizeEntity(entity);
            var created = await _service.AddAsync(entity, cancellationToken);

            if (dto.Intervals != null && dto.Intervals.Any())
            {
                foreach (var intervalDto in dto.Intervals)
                {
                    var intervalEntity = intervalDto.Adapt<TaxReportPeriod>();
                    intervalEntity.TaxPeriod = created.TaxPeriod;
                    if (string.IsNullOrEmpty(intervalEntity.DataAreaId)) intervalEntity.DataAreaId = created.DataAreaId;
                    await _db.Set<TaxReportPeriod>().AddAsync(intervalEntity, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return await GetById(created.TaxPeriod, cancellationToken);
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxPeriodHeadDto>>> Update(string id, [FromBody] TaxPeriodHeadDto dto, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxPeriodHead? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.TaxPeriod == searchCode || x.TaxPeriod.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return NotFound(APIResponse<TaxPeriodHeadDto>.Fail("Sales tax settlement period not found"));
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxPeriod;

            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            if (!string.IsNullOrEmpty(originalCode))
            {
                existingEntity.TaxPeriod = originalCode;
            }

            SanitizeEntity(existingEntity);
            var updatedEntity = await _service.UpdateAsync(existingEntity, cancellationToken);

            if (dto.Intervals != null)
            {
                var currentIntervals = await _db.Set<TaxReportPeriod>()
                    .Where(x => x.TaxPeriod == existingEntity.TaxPeriod)
                    .ToListAsync(cancellationToken);

                _db.Set<TaxReportPeriod>().RemoveRange(currentIntervals);

                foreach (var intervalDto in dto.Intervals)
                {
                    var intervalEntity = intervalDto.Adapt<TaxReportPeriod>();
                    intervalEntity.TaxPeriod = existingEntity.TaxPeriod;
                    intervalEntity.RecId = 0; // reset key for new insert
                    if (string.IsNullOrEmpty(intervalEntity.DataAreaId)) intervalEntity.DataAreaId = existingEntity.DataAreaId;
                    await _db.Set<TaxReportPeriod>().AddAsync(intervalEntity, cancellationToken);
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            return await GetById(existingEntity.TaxPeriod, cancellationToken);
        }

        [HttpDelete("{id}")]
        public override async Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxPeriodHead? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxPeriodHead>().FirstOrDefaultAsync(x => x.TaxPeriod == searchCode || x.TaxPeriod.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return NotFound(APIResponse<bool>.Fail("Sales tax settlement period not found"));
            }

            var intervals = await _db.Set<TaxReportPeriod>()
                .Where(x => x.TaxPeriod == existingEntity.TaxPeriod)
                .ToListAsync(cancellationToken);
            if (intervals.Any())
            {
                _db.Set<TaxReportPeriod>().RemoveRange(intervals);
                await _db.SaveChangesAsync(cancellationToken);
            }

            await _service.RemoveAsync(existingEntity, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true));
        }

        private static void SanitizeEntity(TaxPeriodHead entity)
        {
            entity.TaxPeriod = entity.TaxPeriod?.Trim() ?? string.Empty;
            entity.Name = entity.Name?.Trim() ?? string.Empty;
            entity.TaxAuthority = entity.TaxAuthority?.Trim() ?? string.Empty;
            entity.PaymentCode ??= string.Empty;
            entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? "dat" : entity.DataAreaId;
        }
    }
}

