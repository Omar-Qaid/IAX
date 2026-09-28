using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
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

        public override async Task<ActionResult<APIResponse<IEnumerable<WfPerformerDto>>>> GetAll(
            CancellationToken cancellationToken = default)
        {
            var performers = (await _service.GetAllAsync(cancellationToken: cancellationToken)).ToList();
            var performerIds = performers.Select(item => item.RecId).ToList();
            var usersByPerformer = await _unitOfWork.Repository<WfPerformerUsers>()
                .GetQueryable()
                .AsNoTracking()
                .Where(item => performerIds.Contains(item.PerformerId))
                .GroupBy(item => item.PerformerId)
                .ToDictionaryAsync(
                    group => group.Key,
                    group => group.Select(item => item.UserID).ToList(),
                    cancellationToken);
            var workerIds = usersByPerformer.Values.SelectMany(ids => ids).Distinct().ToList();
            var workerOptions = await LoadWorkerOptionsAsync(workerIds, cancellationToken);

            var result = performers.Select(entity =>
            {
                var dto = entity.Adapt<WfPerformerDto>();
                dto.UserIds = usersByPerformer.GetValueOrDefault(entity.RecId) ?? [];
                dto.UserOptions = dto.UserIds
                    .Where(workerOptions.ContainsKey)
                    .Select(workerId => workerOptions[workerId])
                    .ToList();
                return dto;
            });
            return Ok(APIResponse<IEnumerable<WfPerformerDto>>.Ok(result));
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
            dto.UserOptions = (await LoadWorkerOptionsAsync(dto.UserIds, cancellationToken)).Values.ToList();

            return Ok(APIResponse<WfPerformerDto>.Ok(dto));
        }

        public override async Task<ActionResult<APIResponse<WfPerformerDto>>> Create([FromBody] WfPerformerDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<WfPerformer>();
            var created = await _service.AddAsync(entity, cancellationToken);

            await SyncUsersAsync(created.RecId, dto.UserIds, cancellationToken);

            var resultDto = created.Adapt<WfPerformerDto>();
            resultDto.UserIds = dto.UserIds ?? new();
            resultDto.UserOptions = (await LoadWorkerOptionsAsync(resultDto.UserIds, cancellationToken)).Values.ToList();
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
            resultDto.UserOptions = (await LoadWorkerOptionsAsync(resultDto.UserIds, cancellationToken)).Values.ToList();
            return Ok(APIResponse<WfPerformerDto>.Ok(resultDto, "Updated successfully"));
        }

        [HttpGet("sql-schema")]
        public ActionResult<APIResponse<Dictionary<string, string[]>>> GetSqlSchema()
        {
            var commonFields = new[]
            {
                "RECID", "Code", "Name", "NameAlias", "Description", "IsActive", "DataAreaId"
            };
            var schema = new Dictionary<string, string[]>
            {
                ["HcmDepartments"] = commonFields,
                ["HcmOccupations"] = commonFields,
                ["WfProcesses"] = commonFields.Concat(new[]
                {
                    "CategoryId", "Score", "IsRepeatable", "RepeatIntervalHours",
                    "MandatoryDocuments", "PriorityId", "ProcessTypeId", "IsSystemDefined", "SortOrder"
                }).ToArray(),
                ["WfRequestControls"] = commonFields.Concat(new[]
                {
                    "ProcessId", "ControlId", "Score", "SortOrder", "CanFilter", "CanGroup",
                    "CanSort", "ReferenceType", "FieldRole", "DataType", "DefaultAggregation"
                }).ToArray(),
                ["WfActivityControls"] = commonFields.Concat(new[]
                {
                    "ActivityId", "ProcessId", "ControlId", "Score", "SortOrder", "CanFilter",
                    "CanGroup", "CanSort", "ReferenceType", "FieldRole", "DataType", "DefaultAggregation"
                }).ToArray()
            };

            return Ok(APIResponse<Dictionary<string, string[]>>.Ok(schema));
        }

        [HttpGet("{performerId:long}/users")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<WfPerformerUserDto>>>> GetUsers(
            long performerId,
            CancellationToken cancellationToken = default)
        {
            var users = await _unitOfWork.Repository<WfPerformerUsers>()
                .GetQueryable()
                .AsNoTracking()
                .Where(item => item.PerformerId == performerId)
                .OrderBy(item => item.RecId)
                .Select(item => new WfPerformerUserDto(
                    item.RecId,
                    item.PerformerId,
                    item.UserID,
                    item.RelatedField,
                    item.ExtendedProperties))
                .ToListAsync(cancellationToken);

            return Ok(APIResponse<IReadOnlyList<WfPerformerUserDto>>.Ok(users));
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

        private async Task<Dictionary<long, WfPerformerUserOptionDto>> LoadWorkerOptionsAsync(
            IEnumerable<long> workerIds,
            CancellationToken cancellationToken)
        {
            var ids = workerIds.Distinct().ToList();
            if (ids.Count == 0) return [];

            return await _unitOfWork.Repository<HcmWorker>()
                .GetQueryable()
                .AsNoTracking()
                .Where(worker => ids.Contains(worker.RecId))
                .Select(worker => new WfPerformerUserOptionDto(
                    worker.RecId,
                    worker.PersonnelNumber,
                    worker.Party.Name,
                    worker.Party.NameAlias))
                .ToDictionaryAsync(worker => worker.Id, cancellationToken);
        }
    }
}
