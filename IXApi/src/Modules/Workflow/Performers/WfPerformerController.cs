using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Performers")]
    public class WfPerformerController : BaseController<WfPerformer, WfPerformerDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public WfPerformerController(IWfPerformerService service, IUnitOfWork unitOfWork, ILogger<WfPerformerController> logger) : base(service, logger)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ActionResult<APIResponse<WfPerformerDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _service.GetByIdAsync(id, include: null!, cancellationToken: cancellationToken);
            if (entity == null)
            {
                return NotFound(APIResponse<WfPerformerDto>.Fail($"{_entityName} not found"));
            }

            var dto = entity.Adapt<WfPerformerDto>();
            dto.UserIds = await _unitOfWork.Repository<WfPerformerUsers>()
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.PerformerId == entity.RecId)
                .Select(x => x.UserID)
                .ToListAsync(cancellationToken);

            return Ok(APIResponse<WfPerformerDto>.Ok(dto));
        }

        public override async Task<ActionResult<APIResponse<WfPerformerDto>>> Create([FromBody] WfPerformerDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<WfPerformer>();
            var created = await _service.AddAsync(entity, cancellationToken);

            await SyncUsersAsync(created.RecId, dto.UserIds, cancellationToken);

            var resultDto = created.Adapt<WfPerformerDto>();
            resultDto.UserIds = dto.UserIds ?? new();
            return Ok(APIResponse<WfPerformerDto>.Ok(resultDto, "Created successfully"));
        }

        public override async Task<ActionResult<APIResponse<WfPerformerDto>>> Update(string id, [FromBody] WfPerformerDto dto, CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (existing == null)
            {
                return NotFound(APIResponse<WfPerformerDto>.Fail($"{_entityName} not found"));
            }

            dto.Adapt(existing);
            var updated = await _service.UpdateAsync(existing, cancellationToken);

            await SyncUsersAsync(updated.RecId, dto.UserIds, cancellationToken);

            var resultDto = updated.Adapt<WfPerformerDto>();
            resultDto.UserIds = dto.UserIds ?? new();
            return Ok(APIResponse<WfPerformerDto>.Ok(resultDto, "Updated successfully"));
        }

        [HttpGet("sql-schema")]
        public ActionResult<APIResponse<Dictionary<string, string[]>>> GetSqlSchema()
        {
            var schema = new Dictionary<string, string[]>
            {
                ["OrgEmployees"] = new[] { "EmployeeId", "EmployeeName", "EmployeeNameAR", "DepartmentId", "JobId", "Activated" },
                ["Departments"] = new[] { "DepartmentId", "DepartmentName", "DepartmentNameAR", "ParentId" },
                ["OrgJobs"] = new[] { "JobId", "JobName", "JobNameAR" },
                ["OrgCompanies"] = new[] { "CompanyId", "CompanyName", "CompanyNameAR" },
                ["WfPerformers"] = new[] { "PerformerId", "PerformerName", "PerformerType" },
            };
            return Ok(APIResponse<Dictionary<string, string[]>>.Ok(schema));
        }

        private async Task SyncUsersAsync(long performerId, List<long>? userIds, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<WfPerformerUsers>();
            var existing = await repo.GetQueryable()
                .Where(x => x.PerformerId == performerId)
                .ToListAsync(cancellationToken);

            var desired = (userIds ?? new()).Distinct().ToList();
            var existingIds = existing.Select(x => x.UserID).ToHashSet();
            var desiredSet = desired.ToHashSet();

            foreach (var row in existing.Where(r => !desiredSet.Contains(r.UserID)))
            {
                await repo.RemoveAsync(row);
            }

            foreach (var userId in desired.Where(u => !existingIds.Contains(u)))
            {
                await repo.AddAsync(new WfPerformerUsers
                {
                    PerformerId = performerId,
                    UserID = userId
                }, cancellationToken);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}
