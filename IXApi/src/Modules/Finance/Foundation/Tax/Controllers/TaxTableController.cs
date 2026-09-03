using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/SalesTaxCode")]
    [Route("api/SalesTaxCode")]
    [DomainPermission("Tax", "TaxCodes")]
    public class TaxTableController : BaseController<TaxTable, TaxTableDto>
    {
        private readonly IFinanceDataContext _db;

        public TaxTableController(IBaseService<TaxTable> service, IFinanceDataContext db, ILogger<TaxTableController> logger)
            : base(service, logger)
        {
            _db = db;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxTableDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("[TaxTable] - Fetching all sales tax codes with rate values");
                var entities = await _service.GetAllAsync(cancellationToken: cancellationToken);
                var dtos = entities.Adapt<List<TaxTableDto>>();

                try
                {
                    if (_db != null && _db.TaxData != null)
                    {
                        var rates = await _db.TaxData.AsNoTracking().ToListAsync(cancellationToken);
                        foreach (var dto in dtos)
                        {
                            var match = rates.FirstOrDefault(r => r.TaxCode == dto.TaxCode);
                            if (match != null)
                            {
                                dto.TaxValue = match.TaxValue;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "[TaxTable] - Could not load TaxData rate values, returning base tax codes.");
                }

                return Ok(APIResponse<IEnumerable<TaxTableDto>>.Ok(dtos));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "[TaxTable] - Error in GetAll, returning empty list fallback.");
                return Ok(APIResponse<IEnumerable<TaxTableDto>>.Ok(new List<TaxTableDto>()));
            }
        }

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxTableDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxTable? entity = null;
            if (long.TryParse(searchCode, out var recId))
            {
                entity = await _db.Set<TaxTable>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }

            if (entity == null)
            {
                entity = await _db.Set<TaxTable>().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }

            if (entity == null)
            {
                return NotFound(APIResponse<TaxTableDto>.Fail("Sales tax code not found"));
            }

            var dto = entity.Adapt<TaxTableDto>();
            try
            {
                if (_db != null && _db.TaxData != null)
                {
                    var rate = await _db.TaxData.AsNoTracking().FirstOrDefaultAsync(r => r.TaxCode == dto.TaxCode, cancellationToken);
                    if (rate != null)
                    {
                        dto.TaxValue = rate.TaxValue;
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "[TaxTable] - Could not load TaxData rate for {TaxCode}", dto.TaxCode);
            }

            return Ok(APIResponse<TaxTableDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxTableDto>>> Create([FromBody] TaxTableDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<TaxTable>();
            if (string.IsNullOrEmpty(entity.DataAreaId)) entity.DataAreaId = "dat";

            SanitizeEntity(entity);
            var created = await _service.AddAsync(entity, cancellationToken);

            await SyncTaxDataRateAsync(created.TaxCode, dto.TaxValue, cancellationToken);

            return await GetById(created.TaxCode, cancellationToken);
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxTableDto>>> Update(string id, [FromBody] TaxTableDto dto, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxTable? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxTable>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxTable>().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return NotFound(APIResponse<TaxTableDto>.Fail("Sales tax code not found"));
            }

            var originalRecId = existingEntity.RecId;
            var originalCode = existingEntity.TaxCode;

            dto.Adapt(existingEntity);
            existingEntity.RecId = originalRecId;
            if (!string.IsNullOrEmpty(originalCode))
            {
                existingEntity.TaxCode = originalCode;
            }

            SanitizeEntity(existingEntity);
            var updatedEntity = await _service.UpdateAsync(existingEntity, cancellationToken);

            await SyncTaxDataRateAsync(existingEntity.TaxCode, dto.TaxValue, cancellationToken);

            return await GetById(existingEntity.TaxCode, cancellationToken);
        }

        [HttpDelete("{id}")]
        public override async Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default)
        {
            var searchCode = Uri.UnescapeDataString(id).Trim();
            TaxTable? existingEntity = null;
            if (long.TryParse(searchCode, out long recId))
            {
                existingEntity = await _db.Set<TaxTable>().FirstOrDefaultAsync(x => x.RecId == recId, cancellationToken);
            }
            if (existingEntity == null)
            {
                existingEntity = await _db.Set<TaxTable>().FirstOrDefaultAsync(x => x.TaxCode == searchCode || x.TaxCode.ToUpper() == searchCode.ToUpper(), cancellationToken);
            }
            if (existingEntity == null)
            {
                return NotFound(APIResponse<bool>.Fail("Sales tax code not found"));
            }

            await _service.RemoveAsync(existingEntity, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true));
        }

        private async Task SyncTaxDataRateAsync(string taxCode, decimal taxValue, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taxCode) || _db == null || _db.TaxData == null) return;

            try
            {
                var existingData = await _db.TaxData.FirstOrDefaultAsync(td => td.TaxCode == taxCode, cancellationToken);
                if (existingData != null)
                {
                    existingData.TaxValue = taxValue;
                    _db.TaxData.Update(existingData);
                }
                else
                {
                    _db.TaxData.Add(new TaxData
                    {
                        TaxCode = taxCode,
                        TaxValue = taxValue,
                        TaxFromDate = System.DateTime.UtcNow,
                        TaxToDate = System.DateTime.UtcNow.AddYears(10),
                        DataAreaId = "dat"
                    });
                }
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "[TaxTable] - Error syncing TaxData rate for {TaxCode}", taxCode);
            }
        }

        private static void SanitizeEntity(TaxTable entity)
        {
            entity.TaxCode = entity.TaxCode?.Trim() ?? string.Empty;
            entity.TaxName = entity.TaxName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(entity.TaxPeriod)) entity.TaxPeriod = "Monthly";
            if (string.IsNullOrWhiteSpace(entity.TaxAccountGroup)) entity.TaxAccountGroup = "STANDARD";
            if (string.IsNullOrWhiteSpace(entity.TaxCurrencyCode)) entity.TaxCurrencyCode = "SAR";
            entity.TaxOnTax ??= string.Empty;
            entity.TaxUnit ??= string.Empty;
            entity.PrintCode ??= string.Empty;
            entity.PaymentTaxCode ??= string.Empty;
            entity.TaxJurisdictionCode ??= string.Empty;
            entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? "dat" : entity.DataAreaId;
        }
    }
}

