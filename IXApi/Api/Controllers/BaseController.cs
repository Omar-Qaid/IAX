using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace IAX.IXApi.Api.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController<TEntity, TDto> : ControllerBase
        where TEntity : class
        where TDto : class
    {
        protected readonly IBaseService<TEntity> _service;
        protected readonly ILogger<BaseController<TEntity, TDto>> _logger;
        protected readonly string _entityName;

        protected BaseController(IBaseService<TEntity> service, ILogger<BaseController<TEntity, TDto>> logger)
        {
            _service = service;
            _logger = logger;
            _entityName = typeof(TEntity).Name;
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        [HttpGet]
        public virtual async Task<ActionResult<APIResponse<IEnumerable<TDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Fetching all records", _entityName);

            // Eager-load the same default navigations as the paged endpoint so DTO flattening
            // (e.g. DepartmentName <- Department.Name over non-nullable navs) doesn't dereference null.
            var defaults = GetDefaultIncludes();
            var entities = defaults is { Length: > 0 }
                ? await _service.GetAllAsync(defaults, cancellationToken)
                : await _service.GetAllAsync(cancellationToken: cancellationToken);

            // Materialize the mapping (Mapster's Adapt to IEnumerable is deferred): keeps any mapping
            // error inside the action (handled -> clean 500) instead of crashing mid-serialization.
            var dtos = entities.Adapt<List<TDto>>();
            return Ok(APIResponse<IEnumerable<TDto>>.Ok(dtos));
        }

        /// <summary>
        /// Gets paged entities based on filter parameters.
        /// </summary>
        [HttpGet("paged")]
        public virtual async Task<ActionResult<APIResponse<IEnumerable<TDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Fetching paged records", _entityName);
            
            // Apply default includes if none provided
            if (paginationParams.Includes == null || !paginationParams.Includes.Any())
            {
                var defaults = GetDefaultIncludes();
                if (defaults != null) paginationParams.Includes = defaults.ToList();
            }

            var pagedEntities = await _service.GetPagedAsync(paginationParams, cancellationToken: cancellationToken);
            var dtos = pagedEntities.Items.Adapt<List<TDto>>();
            
            var response = APIResponse<IEnumerable<TDto>>.Ok(dtos);
            response.Pagination = new PaginationMetadata(pagedEntities.PageNumber, pagedEntities.PageSize, pagedEntities.TotalCount);
            
            return Ok(response);
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<APIResponse<TDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Fetching record with ID: {Id}", _entityName, id);
            
            // By passing null to the lambda version, we ensure that any Service-level overrides 
            // that use the lambda signature are correctly triggered.
            var entity = await _service.GetByIdAsync(id, include: null!, cancellationToken: cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("[{EntityName}] - Record with ID: {Id} not found", _entityName, id);
                return NotFound(APIResponse<TDto>.Fail($"{_entityName} not found"));
            }

            var dto = entity.Adapt<TDto>();
            await OnAfterGetAsync(dto);
            return Ok(APIResponse<TDto>.Ok(dto));
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        [HttpPost]
        public virtual async Task<ActionResult<APIResponse<TDto>>> Create([FromBody] TDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Creating new record", _entityName);
            
            var entity = dto.Adapt<TEntity>();
            await OnBeforeCreateAsync(entity);

            var createdEntity = await _service.AddAsync(entity, cancellationToken);
            // Re-load with default includes so the returned DTO is fully populated and flattening
            // (e.g. DepartmentName <- Department.Name) doesn't dereference an un-loaded navigation.
            var createdId = typeof(TEntity).GetProperty("Id")?.GetValue(createdEntity);
            var resultDto = (await ReloadWithDefaultsAsync(createdId, cancellationToken) ?? createdEntity).Adapt<TDto>();

            await OnAfterCreateAsync(resultDto);
            return Ok(APIResponse<TDto>.Ok(resultDto, "Created successfully"));
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        [HttpPut("{id}")]
        public virtual async Task<ActionResult<APIResponse<TDto>>> Update(string id, [FromBody] TDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Updating record with ID: {Id}", _entityName, id);
            
            var existingEntity = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (existingEntity == null)
            {
                return NotFound(APIResponse<TDto>.Fail($"{_entityName} not found"));
            }

            dto.Adapt(existingEntity);
            await OnBeforeUpdateAsync(existingEntity);
            
            var updatedEntity = await _service.UpdateAsync(existingEntity, cancellationToken);
            // Re-load with default includes so the returned DTO is fully populated and flattening
            // (e.g. DepartmentName <- Department.Name) doesn't dereference an un-loaded navigation.
            var resultDto = (await ReloadWithDefaultsAsync(id, cancellationToken) ?? updatedEntity).Adapt<TDto>();

            await OnAfterUpdateAsync(resultDto);
            return Ok(APIResponse<TDto>.Ok(resultDto, "Updated successfully"));
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Deleting record with ID: {Id}", _entityName, id);
            
            var entity = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (entity == null)
            {
                return NotFound(APIResponse<bool>.Fail($"{_entityName} not found"));
            }

            await OnBeforeDeleteAsync(entity);
            await _service.RemoveAsync(entity, cancellationToken);
            await OnAfterDeleteAsync(id);
            
            return Ok(APIResponse<bool>.Ok(true, "Deleted successfully"));
        }

        /// <summary>
        /// Creates a range of entities.
        /// </summary>
        [HttpPost("range")]
        public virtual async Task<ActionResult<APIResponse<IEnumerable<TDto>>>> CreateRange([FromBody] IEnumerable<TDto> dtos, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Creating range of records", _entityName);
            
            var entities = dtos.Adapt<IEnumerable<TEntity>>();
            foreach (var entity in entities)
            {
                await OnBeforeCreateAsync(entity);
            }
            
            var createdEntities = await _service.AddRangeAsync(entities, cancellationToken);
            var resultDtos = createdEntities.Adapt<List<TDto>>();
            
            foreach (var resultDto in resultDtos)
            {
                await OnAfterCreateAsync(resultDto);
            }

            return Ok(APIResponse<IEnumerable<TDto>>.Ok(resultDtos, "Created successfully"));
        }

        /// <summary>
        /// Updates a range of entities.
        /// </summary>
        [HttpPut("range")]
        public virtual async Task<ActionResult<APIResponse<IEnumerable<TDto>>>> UpdateRange([FromBody] IEnumerable<TDto> dtos, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Updating range of records", _entityName);
            
            var entities = dtos.Adapt<IEnumerable<TEntity>>().ToList();
            var dtosList = dtos.ToList();
            
            var dtoIdProp = typeof(TDto).GetProperty("Id");
            var entityIdProp = typeof(TEntity).GetProperty("Id");
            
            if (dtoIdProp != null && entityIdProp != null)
            {
                for (int i = 0; i < dtosList.Count; i++)
                {
                    var idValue = dtoIdProp.GetValue(dtosList[i]);
                    entityIdProp.SetValue(entities[i], idValue);
                }
            }

            foreach (var entity in entities)
            {
                await OnBeforeUpdateAsync(entity);
            }
            
            var updatedEntities = await _service.UpdateRangeAsync(entities, cancellationToken);
            var resultDtos = updatedEntities.Adapt<List<TDto>>();
            
            foreach (var resultDto in resultDtos)
            {
                await OnAfterUpdateAsync(resultDto);
            }

            return Ok(APIResponse<IEnumerable<TDto>>.Ok(resultDtos, "Updated successfully"));
        }

        /// <summary>
        /// Deletes a range of entities by ids.
        /// </summary>
        [HttpDelete("range")]
        public virtual async Task<ActionResult<APIResponse<bool>>> DeleteRange([FromBody] IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Deleting range of records", _entityName);
            
            var entities = new List<TEntity>();
            foreach (var id in ids)
            {
                var entity = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);
                if (entity != null)
                {
                    await OnBeforeDeleteAsync(entity);
                    entities.Add(entity);
                }
            }

            if (entities.Any())
            {
                await _service.RemoveRangeAsync(entities, cancellationToken);
                foreach (var id in ids)
                {
                    await OnAfterDeleteAsync(id);
                }
            }
            
            return Ok(APIResponse<bool>.Ok(true, "Deleted successfully"));
        }

        #region Hooks

        protected virtual Task OnBeforeCreateAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnAfterCreateAsync(TDto dto) => Task.CompletedTask;
        protected virtual Task OnBeforeUpdateAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnAfterUpdateAsync(TDto dto) => Task.CompletedTask;
        protected virtual Task OnBeforeDeleteAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnAfterDeleteAsync(string id) => Task.CompletedTask;
        protected virtual Task OnAfterGetAsync(TDto dto) => Task.CompletedTask;

        #endregion

        /// <summary>
        /// Override to specify default includes for the GetById and GetPaged operations.
        /// </summary>
        protected virtual string[]? GetDefaultIncludes() => null;

        /// <summary>
        /// Re-loads an entity with the same default navigations the read endpoints use, so a freshly
        /// created/updated entity (whose navigations the write path didn't eager-load) maps to a
        /// complete DTO without dereferencing a null navigation during flattening. Returns null when
        /// there are no default includes (callers fall back to the entity they already hold).
        /// </summary>
        protected async Task<TEntity?> ReloadWithDefaultsAsync(object? id, CancellationToken cancellationToken)
        {
            var defaults = GetDefaultIncludes();
            if (id is null || defaults is not { Length: > 0 })
                return null;
            return await _service.GetByIdAsync(id, defaults, cancellationToken: cancellationToken);
        }
    }
}
