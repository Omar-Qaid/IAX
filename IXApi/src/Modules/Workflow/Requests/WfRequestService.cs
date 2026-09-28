using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Persistence;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public partial class WfRequestService : BaseService<WfRequest>, IWfRequestService
    {
        private readonly ISysNumberSequenceService _sequences;
        private readonly IWorkflowDataContext _context;
        private readonly ISysNotificationService _notifications;
        private readonly IAppPermissionService _permissions;
        private readonly IAX.IXApi.Modules.Organization.DocumentManagement.Services.IDocumentService _documents;
        private readonly IAX.IXApi.Modules.Organization.DocumentManagement.Storage.IFileStorageProvider _fileStorage;

        public WfRequestService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences, IWorkflowDataContext context, ISysNotificationService notifications, IAppPermissionService permissions, IAX.IXApi.Modules.Organization.DocumentManagement.Services.IDocumentService documents, IAX.IXApi.Modules.Organization.DocumentManagement.Storage.IFileStorageProvider fileStorage) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
            _context = context;
            _notifications = notifications;
            _permissions = permissions;
            _documents = documents;
            _fileStorage = fileStorage;
        }

        protected override async Task OnBeforeAddAsync(WfRequest entity, CancellationToken cancellationToken)
        {
            if (!entity.EmployeeId.HasValue)
            {
                var currentUserId = _currentUser.GetCurrentUserId();
                if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId != "sys")
                {
                    entity.EmployeeId = await _context.Set<HcmWorker>().AsNoTracking()
                        .Where(worker => worker.UserId == currentUserId && worker.IsActive && !worker.IsDeleted)
                        .Select(worker => (long?)worker.RecId)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }
            await _sequences.EnsureCodeAsync(entity, entityName: "WfRequest", cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<WfRequestDto>> GetRequestListAsync(CancellationToken cancellationToken = default)
        {
            var requests = (await GetAllAsync(cancellationToken: cancellationToken)).ToList();
            if (!await CanViewAllRequestsAsync(cancellationToken))
            {
                var userId = _currentUser.GetCurrentUserId();
                var employeeId = await GetCurrentEmployeeIdAsync(userId, cancellationToken);
                var assignedRequestIds = employeeId.HasValue
                    ? await _context.Set<WfAssignment>().AsNoTracking()
                        .Where(item => item.UserId == employeeId.Value)
                        .Select(item => item.RequestId)
                        .Distinct()
                        .ToListAsync(cancellationToken)
                    : [];
                requests = requests.Where(item =>
                    item.CreatedBy == userId
                    || employeeId.HasValue && item.EmployeeId == employeeId.Value
                    || assignedRequestIds.Contains(item.RecId)).ToList();
            }
            var employeeIds = requests
                .Where(item => item.EmployeeId.HasValue)
                .Select(item => item.EmployeeId!.Value)
                .Distinct()
                .ToList();

            var workers = employeeIds.Count == 0
                ? []
                : await _context.Set<HcmWorker>().AsNoTracking()
                    .Where(item => employeeIds.Contains(item.RecId))
                    .ToListAsync(cancellationToken);
            var partyIds = workers.Select(item => item.Person).Distinct().ToList();
            var parties = partyIds.Count == 0
                ? new Dictionary<long, MailPartyLookup>()
                : await _context.Database.SqlQueryRaw<MailPartyLookup>(
                        "SELECT RECID AS PartyId, COALESCE(NULLIF(RFullName, ''), NULLIF(Name, ''), PartyNumber) AS DisplayName FROM dbo.DirPartyTable")
                    .Where(item => partyIds.Contains(item.PartyId))
                    .ToDictionaryAsync(item => item.PartyId, cancellationToken);
            var workersById = workers.ToDictionary(item => item.RecId);

            return requests.Select(request =>
            {
                var dto = request.Adapt<WfRequestDto>();
                if (request.EmployeeId.HasValue && workersById.TryGetValue(request.EmployeeId.Value, out var worker))
                {
                    parties.TryGetValue(worker.Person, out var party);
                    dto.RequesterName = FirstText(party?.DisplayName, worker.PersonnelNumber);
                }
                return dto;
            }).ToList();
        }

        public async Task<DynamicRequestFormDto?> GetFormDefinitionAsync(long processId, CancellationToken cancellationToken = default)
        {
            var process = await _context.WfProcesses.AsNoTracking()
                .Where(item => item.RecId == processId && item.IsActive)
                .Select(item => new { item.RecId, item.Name, item.Description, item.MandatoryDocuments })
                .FirstOrDefaultAsync(cancellationToken);
            if (process == null) return null;

            var controls = await _context.WfRequestControls.AsNoTracking()
                .Where(item => item.ProcessId == processId && item.IsActive)
                .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                .Select(item => new
                {
                    item.RecId, item.ProcessId,
                    ControlId = (byte?)item.ControlId,
                    Score = (decimal?)item.Score,
                    SortOrder = (byte?)item.SortOrder,
                    item.Code, item.Name, item.NameAlias,
                    item.ReferenceType, item.ExtendedProperties,
                    ControlCode = item.Control.Code,
                    ControlName = item.Control.Name,
                    RuntimeControlType = item.Control.ControlType
                })
                .ToListAsync(cancellationToken);
            var ids = controls.Select(item => item.RecId).ToList();
            var options = await _context.WfRequestControlsOptions.AsNoTracking()
                .Where(item => ids.Contains(item.RequestControlId) && item.IsActive)
                .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                .Select(item => new
                {
                    item.RecId, item.RequestControlId, item.Value, item.Name, item.NameAlias,
                    Score = (decimal?)item.Score,
                    SortOrder = (int?)item.SortOrder,
                    item.ExtendedProperties
                })
                .ToListAsync(cancellationToken);
            var validations = await _context.WfRequestControlsValidations.AsNoTracking()
                .Where(item => ids.Contains(item.RequestControlId) && item.IsActive)
                .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                .Select(item => new
                {
                    item.RecId, item.RequestControlId, item.ValidationType,
                    item.ValidationExpression, item.Operator, item.Value, item.MaskInput,
                    item.ErrorMessage, item.ErrorMessageAlias, item.Severity,
                    SortOrder = (int?)item.SortOrder
                })
                .ToListAsync(cancellationToken);
            var referenceTypes = controls.Select(item => item.ReferenceType)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var referenceOptions = new Dictionary<long, List<DynamicRequestOptionDto>>();
            if (referenceTypes.Contains("Employee"))
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var employees = await _context.Set<HcmWorker>().AsNoTracking()
                    .Where(item => item.IsActive && !item.IsDeleted)
                    .OrderBy(item => item.PersonnelNumber).ThenBy(item => item.RecId)
                    .Select(item => new
                    {
                        item.RecId, item.Person, item.PersonnelNumber,
                        Name = item.Party.Name,
                        item.Party.NameAlias
                    })
                    .ToListAsync(cancellationToken);
                var employeeIds = employees.Select(item => item.RecId).ToList();
                var assignments = await _context.Set<HcmWorkerOrganizationAssignmentV1>().AsNoTracking()
                    .Where(item => employeeIds.Contains(item.HcmWorkerId) && item.IsPrimary && item.IsActive
                        && !item.IsDeleted && item.ValidFrom <= today
                        && (!item.ValidTo.HasValue || item.ValidTo.Value >= today))
                    .OrderByDescending(item => item.ValidFrom).ThenByDescending(item => item.RecId)
                    .ToListAsync(cancellationToken);
                var assignmentByWorker = assignments.GroupBy(item => item.HcmWorkerId)
                    .ToDictionary(group => group.Key, group => group.First());
                var currentEmployeeId = await GetCurrentEmployeeIdAsync(
                    _currentUser.GetCurrentUserId(), cancellationToken);
                var managerChain = new Dictionary<int, long>();
                var chainWorkerId = currentEmployeeId;
                for (var level = 1; level <= 3 && chainWorkerId.HasValue; level++)
                {
                    if (!assignmentByWorker.TryGetValue(chainWorkerId.Value, out var assignment)) break;
                    managerChain[level] = assignment.HcmManagerWorkerId;
                    chainWorkerId = assignment.HcmManagerWorkerId;
                }
                foreach (var control in controls.Where(item =>
                    string.Equals(item.ReferenceType, "Employee", StringComparison.OrdinalIgnoreCase)))
                {
                    var filter = ParseProperties(control.ExtendedProperties).ReferenceFilter;
                    var filtered = employees.Where(employee =>
                    {
                        assignmentByWorker.TryGetValue(employee.RecId, out var assignment);
                        if (filter.DepartmentId.HasValue && assignment?.DepartmentId != filter.DepartmentId) return false;
                        if (filter.OccupationId.HasValue && assignment?.OccupationId != filter.OccupationId) return false;
                        if (filter.ManagerLevel is >= 1 and <= 3 &&
                            (!managerChain.TryGetValue(filter.ManagerLevel.Value, out var managerId)
                                || employee.RecId != managerId)) return false;
                        return true;
                    }).ToList();
                    referenceOptions[control.RecId] = filtered.Select((item, index) =>
                    {
                        var label = FirstText(item.Name, item.PersonnelNumber,
                            item.RecId.ToString(CultureInfo.InvariantCulture));
                        return new DynamicRequestOptionDto
                        {
                            OptionId = item.RecId, Value = item.RecId.ToString(CultureInfo.InvariantCulture),
                            Label = label,
                            LabelAlias = FirstText(item.NameAlias, item.Name, item.PersonnelNumber),
                            SortOrder = index
                        };
                    }).ToList();
                }
            }
            if (referenceTypes.Contains("Showroom"))
            {
                var showrooms = await _context.Set<HcmShowroom>().AsNoTracking()
                    .Where(item => item.IsActive && !item.IsDeleted)
                    .OrderBy(item => item.PersonnelNumber).ThenBy(item => item.RecId)
                    .Select(item => new
                    {
                        item.RecId, item.PersonnelNumber,
                        Name = item.PartyTable.Name,
                        item.PartyTable.NameAlias
                    }).ToListAsync(cancellationToken);
                var showroomOptions = showrooms.Select((item, index) => new DynamicRequestOptionDto
                {
                    OptionId = item.RecId, Value = item.RecId.ToString(CultureInfo.InvariantCulture),
                    Label = FirstText(item.Name, item.PersonnelNumber),
                    LabelAlias = FirstText(item.NameAlias, item.Name, item.PersonnelNumber), SortOrder = index
                }).ToList();
                foreach (var control in controls.Where(item =>
                    string.Equals(item.ReferenceType, "Showroom", StringComparison.OrdinalIgnoreCase)))
                    referenceOptions[control.RecId] = showroomOptions;
            }

            return new DynamicRequestFormDto
            {
                ProcessId = process.RecId,
                ProcessName = process.Name ?? string.Empty,
                ProcessDescription = process.Description,
                MandatoryDocuments = process.MandatoryDocuments,
                Controls = controls.Select(control =>
                {
                    var properties = ParseProperties(control.ExtendedProperties);
                    var configuredOptions = options.Where(item => item.RequestControlId == control.RecId)
                        .Select((item, optionIndex) => new DynamicRequestOptionDto
                        {
                            OptionId = item.RecId, Value = item.Value, Label = item.Name,
                            LabelAlias = item.NameAlias,
                            Score = item.Score ?? 0m, SortOrder = item.SortOrder ?? optionIndex,
                            ExtendedProperties = item.ExtendedProperties,
                            FeatureConfiguration = ParseOptionFeatures(
                                item.ExtendedProperties,
                                properties.OptionFeatureConfigurations.ElementAtOrDefault(optionIndex))
                        }).ToList();
                    var runtimeOptions = configuredOptions.Count > 0
                        ? configuredOptions
                        : referenceOptions.TryGetValue(control.RecId, out var lookupOptions)
                            ? lookupOptions
                            : [];
                    return new DynamicRequestControlDto
                    {
                        RequestControlId = control.RecId,
                        ControlId = control.ControlId ?? 0,
                        Code = control.Code ?? $"control_{control.RecId}",
                        Label = control.Name ?? control.Code ?? $"Field {control.RecId}",
                        LabelAr = control.NameAlias ?? properties.LabelAr,
                        LabelColor = properties.LabelColor,
                        ControlType = ResolveRuntimeControlType(
                            control.ControlCode,
                            control.ControlName,
                            control.RuntimeControlType),
                        ReferenceType = control.ReferenceType,
                        SortOrder = control.SortOrder ?? 0,
                        ColumnSpan = properties.ColumnSpan,
                        Score = control.Score ?? 0m,
                        Required = properties.Required,
                        ReadOnly = properties.ReadOnly,
                        UniqueKey = properties.UniqueKey,
                        UsedAsCriteria = properties.UsedAsCriteria,
                        DefaultValue = properties.DefaultValue,
                        VisibilityCondition = properties.VisibilityCondition,
                        Options = runtimeOptions,
                        Validations = validations.Where(item => item.RequestControlId == control.RecId).Select(item => new DynamicRequestValidationDto
                        {
                            ValidationId = item.RecId, Type = item.ValidationType,
                            Expression = item.ValidationExpression, Operator = item.Operator,
                            Value = item.Value, Mask = item.MaskInput, ErrorMessage = item.ErrorMessage,
                            ErrorMessageAlias = item.ErrorMessageAlias,
                            Severity = item.Severity, SortOrder = item.SortOrder ?? 0
                        }).ToList()
                    };
                }).ToList()
            };
        }

        public async Task<DynamicRequestLookupPageDto?> GetReferenceOptionsAsync(
            long processId, long requestControlId, int pageNumber, int pageSize,
            string? search, CancellationToken cancellationToken = default)
        {
            var control = await _context.WfRequestControls.AsNoTracking()
                .Where(item => item.RecId == requestControlId && item.ProcessId == processId && item.IsActive)
                .Select(item => new { item.ReferenceType, item.ExtendedProperties })
                .FirstOrDefaultAsync(cancellationToken);
            if (control == null || control.ReferenceType is not ("Employee" or "Showroom")) return null;

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 10, 100);
            var term = search?.Trim();

            if (control.ReferenceType == "Showroom")
            {
                var query = _context.Set<HcmShowroom>().AsNoTracking()
                    .Where(item => item.IsActive && !item.IsDeleted);
                query = ApplyReferenceRules(query, ParseProperties(control.ExtendedProperties).ReferenceFilter.Rules);
                if (!string.IsNullOrWhiteSpace(term))
                    query = query.Where(item => item.PersonnelNumber.Contains(term)
                        || item.PartyTable.Name.Contains(term)
                        || item.PartyTable.NameAlias != null && item.PartyTable.NameAlias.Contains(term));
                var total = await query.CountAsync(cancellationToken);
                var rows = await query.OrderBy(item => item.PersonnelNumber).ThenBy(item => item.RecId)
                    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                    .Select(item => new DynamicRequestOptionDto
                    {
                        OptionId = item.RecId,
                        Value = item.RecId.ToString(),
                        Label = item.PartyTable.Name ?? item.PersonnelNumber,
                        LabelAlias = item.PartyTable.NameAlias ?? item.PartyTable.Name ?? item.PersonnelNumber
                    }).ToListAsync(cancellationToken);
                return new(rows, pageNumber, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)), total);
            }

            var filter = ParseProperties(control.ExtendedProperties).ReferenceFilter;
            long? managerId = null;
            if (filter.ManagerLevel is >= 1 and <= 3)
            {
                var current = await GetCurrentEmployeeIdAsync(_currentUser.GetCurrentUserId(), cancellationToken);
                for (var level = 1; level <= filter.ManagerLevel && current.HasValue; level++)
                    current = await _context.Set<HcmWorkerOrganizationAssignmentV1>().AsNoTracking()
                        .Where(item => item.HcmWorkerId == current && item.IsPrimary && item.IsActive && !item.IsDeleted)
                        .OrderByDescending(item => item.ValidFrom).ThenByDescending(item => item.RecId)
                        .Select(item => (long?)item.HcmManagerWorkerId).FirstOrDefaultAsync(cancellationToken);
                managerId = current;
            }
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var workers = _context.Set<HcmWorker>().AsNoTracking()
                .Where(item => item.IsActive && !item.IsDeleted);
            workers = ApplyReferenceRules(workers, filter.Rules);
            if (filter.ManagerLevel.HasValue)
                workers = managerId.HasValue ? workers.Where(item => item.RecId == managerId.Value) : workers.Where(_ => false);
            if (filter.DepartmentId.HasValue || filter.OccupationId.HasValue)
                workers = workers.Where(worker => worker.WorkerOrganizationAssignmentsV1.Any(assignment =>
                    assignment.IsPrimary && assignment.IsActive && !assignment.IsDeleted
                    && assignment.ValidFrom <= today && (!assignment.ValidTo.HasValue || assignment.ValidTo.Value >= today)
                    && (!filter.DepartmentId.HasValue || assignment.DepartmentId == filter.DepartmentId)
                    && (!filter.OccupationId.HasValue || assignment.OccupationId == filter.OccupationId)));
            if (!string.IsNullOrWhiteSpace(term))
                workers = workers.Where(item => item.PersonnelNumber.Contains(term)
                    || item.Party.Name.Contains(term)
                    || item.Party.NameAlias != null && item.Party.NameAlias.Contains(term));
            var workerTotal = await workers.CountAsync(cancellationToken);
            var workerRows = await workers.OrderBy(item => item.PersonnelNumber).ThenBy(item => item.RecId)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(item => new DynamicRequestOptionDto
                {
                    OptionId = item.RecId,
                    Value = item.RecId.ToString(),
                    Label = item.Party.Name ?? item.PersonnelNumber,
                    LabelAlias = item.Party.NameAlias ?? item.Party.Name ?? item.PersonnelNumber
                }).ToListAsync(cancellationToken);
            return new(workerRows, pageNumber,
                Math.Max(1, (int)Math.Ceiling(workerTotal / (double)pageSize)), workerTotal);
        }

        public async Task<MailRequestDetailsDto?> GetMailDetailsAsync(long requestId, CancellationToken cancellationToken = default)
        {
            if (!await CanAccessRequestAsync(requestId, cancellationToken)) return null;

            var request = await _context.WfRequests.AsNoTracking()
                .Include(item => item.Process)
                .SingleOrDefaultAsync(item => item.RecId == requestId, cancellationToken);
            if (request == null) return null;

            // Ignore the child soft-delete filter without ever widening the request's tenant boundary.
            var detailRows = await _context.WfRequestDetails.IgnoreQueryFilters().AsNoTracking()
                .Where(item => item.RequestId == requestId
                    && item.DataAreaId == request.DataAreaId
                    && !item.IsDeleted)
                .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                .ToListAsync(cancellationToken);
            var detailControlDataIds = detailRows.Where(item => item.ControlDataId.HasValue)
                .Select(item => item.ControlDataId!.Value).Distinct().ToList();
            var requestControlsById = await _context.WfRequestControls.AsNoTracking()
                .Where(item => item.ProcessId == request.ProcessId && detailControlDataIds.Contains(item.RecId))
                .ToDictionaryAsync(item => item.RecId, cancellationToken);
            var databaseRows = detailRows.Select(item => new MailFieldSource(
                    item.RecId, item.ControlId, item.ControlDataId,
                    !string.IsNullOrEmpty(item.Name) ? item.Name : item.ControlDataId.HasValue && requestControlsById.TryGetValue(item.ControlDataId.Value, out var requestControl)
                        ? requestControl.Name ?? string.Empty : string.Empty,
                    !string.IsNullOrEmpty(item.NameAlias) ? item.NameAlias : item.ControlDataId.HasValue && requestControlsById.TryGetValue(item.ControlDataId.Value, out requestControl)
                        ? requestControl.NameAlias ?? string.Empty : string.Empty,
                    item.ControlValue ?? string.Empty, item.ValueAlias ?? string.Empty, item.Value ?? string.Empty, item.SortOrder)).ToList();
            var parsedRows = MergeMailFieldSources(databaseRows, ParseSerializedRequestDetails(request.RequestDetails));

            var controlIds = parsedRows.Where(item => item.ControlId.HasValue)
                .Select(item => item.ControlId!.Value).Distinct().ToList();
            var controls = await _context.WfControls.AsNoTracking()
                .Where(item => controlIds.Contains(item.RecId))
                .ToDictionaryAsync(item => item.RecId, cancellationToken);
            var fields = parsedRows.OrderBy(item => item.Order).ThenBy(item => item.DetailId).Select(item =>
            {
                controls.TryGetValue(item.ControlId ?? 0, out var control);
                return new MailRequestFieldDto
                {
                    DetailId = item.DetailId,
                    ControlId = item.ControlId,
                    ControlDataId = item.ControlDataId,
                    Label = FirstText(item.Label, item.LabelAr, $"Field {item.ControlDataId}"),
                    LabelAr = FirstText(item.LabelAr, item.Label, $"Field {item.ControlDataId}"),
                    Value = FirstText(item.ValueAr, item.Value, item.ValueEn),
                    ValueAr = item.ValueAr,
                    ValueEn = item.ValueEn,
                    ControlType = ResolveRuntimeControlType(control?.Code, control?.Name, control?.ControlType),
                    ControlOrder = item.Order
                };
            }).ToList();

            var assignments = await _context.Set<WfAssignment>().AsNoTracking()
                .Include(item => item.Activity).ThenInclude(item => item.Step)
                .Where(item => item.RequestId == requestId)
                .OrderBy(item => item.AssignDate).ThenBy(item => item.RecId)
                .ToListAsync(cancellationToken);
            var assignmentIds = assignments.Select(item => item.RecId).ToList();
            var activityDetails = assignmentIds.Count == 0
                ? []
                : await _context.WfActivityDetails.AsNoTracking()
                    .Where(item => assignmentIds.Contains(item.AssignmentID))
                    .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                    .ToListAsync(cancellationToken);
            var activityControlDataIds = activityDetails.Select(item => item.ControlDataId).Distinct().ToList();
            var activityControlsById = await _context.Set<WfActivityControl>().AsNoTracking()
                .Where(item => activityControlDataIds.Contains(item.RecId))
                .ToDictionaryAsync(item => item.RecId, cancellationToken);
            var activityIds = assignments.Select(item => item.ActivityId).Distinct().ToList();
            var configuredActivityControls = activityIds.Count == 0
                ? []
                : await _context.Set<WfActivityControl>().AsNoTracking()
                    .Include(item => item.Control)
                    .Where(item => activityIds.Contains(item.ActivityId) && item.IsActive)
                    .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                    .ToListAsync(cancellationToken);
            var configuredControlIds = configuredActivityControls.Select(item => item.RecId).ToList();
            var activityControlOptions = configuredControlIds.Count == 0
                ? []
                : await _context.Set<WfActivityControlsOption>().AsNoTracking()
                    .Where(item => configuredControlIds.Contains(item.ActivityControlId) && item.IsActive)
                    .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId)
                    .ToListAsync(cancellationToken);

            var employeeIds = assignments.Select(item => item.UserId)
                .Append(request.EmployeeId ?? 0).Where(item => item > 0).Distinct().ToList();
            var requesterUserId = request.EmployeeId.HasValue ? null : request.CreatedBy;
            var workers = await _context.Set<HcmWorker>().AsNoTracking()
                .Where(item => employeeIds.Contains(item.RecId)
                    || requesterUserId != null && item.UserId == requesterUserId)
                .ToListAsync(cancellationToken);
            var partyIds = workers.Select(item => item.Person).Distinct().ToList();
            var parties = partyIds.Count == 0
                ? new Dictionary<long, MailPartyLookup>()
                : await _context.Database.SqlQueryRaw<MailPartyLookup>(
                        "SELECT RECID AS PartyId, COALESCE(NULLIF(RFullName, ''), NULLIF(Name, ''), PartyNumber) AS DisplayName FROM dbo.DirPartyTable")
                    .Where(item => partyIds.Contains(item.PartyId))
                    .ToDictionaryAsync(item => item.PartyId, cancellationToken);
            string EmployeeDisplay(long? id)
            {
                var worker = workers.FirstOrDefault(item => item.RecId == id);
                if (worker == null) return id.HasValue ? $"Employee {id.Value}" : "Workflow queue";
                parties.TryGetValue(worker.Person, out var party);
                return FirstText(party?.DisplayName, worker.PersonnelNumber);
            }

            var latest = assignments.LastOrDefault();
            var history = assignments.OrderByDescending(item => item.AssignDate).Select(item =>
            {
                var notes = activityDetails.Where(detail => detail.AssignmentID == item.RecId)
                    .Select(detail => new { Detail = detail, Control = activityControlsById.GetValueOrDefault(detail.ControlDataId) })
                    .Where(detail => !IsSignature(detail.Control?.Name, detail.Control?.NameAlias, detail.Detail.ControlValue))
                    .Select(detail => (Label: FirstText(detail.Control?.NameAlias, detail.Control?.Name), Value: detail.Detail.ControlValue))
                    .Where(detail => !string.IsNullOrWhiteSpace(detail.Value) && !LooksSerialized(detail.Value))
                    .Select(detail => $"{detail.Label}: {detail.Value}").ToList();
                var assignmentDetails = activityDetails
                    .Where(detail => detail.AssignmentID == item.RecId)
                    .ToList();
                return new MailTrackingEntryDto
                {
                    AssignmentId = item.RecId,
                    Title = FirstText(item.Activity?.Name, item.Activity?.Code, "Workflow activity"),
                    Stage = FirstText(item.Activity?.Step?.Name, item.Activity?.Step?.Code, "Workflow stage"),
                    Responsible = EmployeeDisplay(item.UserId),
                    Action = item.IsFinished ? item.Automatically == true ? "Passed automatically" : "Completed" : "In progress",
                    Date = item.FinishedDate ?? item.AssignDate,
                    Notes = notes.Count > 0 ? string.Join(" · ", notes) : "—",
                    IsCurrent = !request.IsFinished && !request.IsStopped && !item.IsFinished && item.RecId == latest?.RecId,
                    IsCompleted = item.IsFinished,
                    Controls = configuredActivityControls
                        .Where(control => control.ActivityId == item.ActivityId)
                        .Select(control =>
                        {
                            var detail = assignmentDetails.FirstOrDefault(value => value.ControlDataId == control.RecId);
                            return new MailActivityControlDto
                            {
                                ActivityControlId = control.RecId,
                                Label = FirstText(control.Name, control.NameAlias, control.Control?.Name, control.Code),
                                LabelAr = FirstText(control.NameAlias, control.Name, control.Control?.Name, control.Code),
                                ControlType = ResolveRuntimeControlType(control.Control?.Code, control.Control?.Name, control.Control?.ControlType),
                                Value = detail?.ControlValue ?? string.Empty,
                                SortOrder = control.SortOrder,
                                Options = activityControlOptions
                                    .Where(option => option.ActivityControlId == control.RecId)
                                    .Select(option => new MailActivityControlOptionDto
                                    {
                                        Value = option.Value,
                                        Label = FirstText(option.Name, option.NameAlias, option.Value),
                                        LabelAr = FirstText(option.NameAlias, option.Name, option.Value)
                                    }).ToList()
                            };
                        }).ToList()
                };
            }).ToList();

            var employee = request.EmployeeId.HasValue
                ? workers.FirstOrDefault(item => item.RecId == request.EmployeeId)
                : workers.FirstOrDefault(item => item.UserId == requesterUserId);
            var accountDisplayName = !string.IsNullOrWhiteSpace(request.CreatedBy)
                ? await _context.Set<AspNetUser>().AsNoTracking()
                    .Where(user => user.Id == request.CreatedBy)
                    .Select(user => user.UserName)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;
            var requesterDisplayName = FirstText(
                accountDisplayName,
                employee == null ? null : EmployeeDisplay(employee.RecId),
                "Unknown requester");
            return new MailRequestDetailsDto
            {
                RequestId = request.RecId,
                ProcessName = FirstText(request.Process?.Name, request.Process?.Code, $"Process {request.ProcessId}"),
                ProcessCode = request.Process?.Code ?? string.Empty,
                CreatedBy = request.CreatedBy ?? string.Empty,
                CreatedDate = request.CreatedAt,
                SubmittedBy = requesterDisplayName,
                SubmissionDate = request.RequestDate,
                Status = request.IsStopped ? "Stopped" : request.IsFinished ? "Completed" : "In progress",
                RequestDate = request.RequestDate,
                EmployeeName = requesterDisplayName,
                EmployeeNumber = employee?.PersonnelNumber ?? request.EmployeeId?.ToString(CultureInfo.InvariantCulture) ?? "—",
                TransactionType = request.IsStopped ? "Request stopped" : request.IsFinished ? "Request completed" : FirstText(latest?.Activity?.Name, request.Name, "Workflow request"),
                TransactionTime = request.RequestDate,
                TransactionEndTime = request.FinishedDate ?? request.StoppedDate,
                ResponsibleEmployee = latest == null ? null : EmployeeDisplay(latest.UserId),
                Fields = fields,
                History = history
            };
        }

        public async Task<SubmitDynamicRequestResultDto> SubmitDynamicAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken = default)
        {
            return await WorkflowSubmissionTransaction.RunAsync(_unitOfWork.Context, _fileStorage, async storedPaths =>
            {
                await LockSubmissionAsync(submission.ProcessId, cancellationToken);
                var prepared = await PrepareSubmissionAsync(submission, cancellationToken, verifyUploads: true);
                if (prepared.Errors.Any(error => error.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)))
                    throw new DynamicRequestValidationException(prepared.Errors);
                var (form, values, visible, featureValues, selectedOptionContexts, _) = prepared;

                var scored = visible.Select(control =>
                {
                    values.TryGetValue(control.RequestControlId, out var value);
                    value ??= control.DefaultValue ?? string.Empty;
                    var selected = SelectedValues(value);
                    var score = control.Options.Count > 0
                        ? control.Options.Where(option => selected.Contains(option.Value, StringComparer.Ordinal)).Sum(option => option.Score)
                        : IsEmpty(value) || Normalize(control.ControlType) == "checkbox" && !IsTrue(value) ? 0 : control.Score;
                    return new { Control = control, Value = value, Score = score };
                }).ToList();

                var request = new WfRequest
                {
                    Name = form.ProcessName,
                    RequestDetails = SerializeRequestDetails(submission.ProcessId, scored.Select(item =>
                        (item.Control, item.Value, item.Score))),
                    RequestDate = DateTime.UtcNow, ProcessId = submission.ProcessId,
                    Score = scored.Sum(item => item.Score), Progress = 0, IsActive = true,
                    DataAreaId = _currentUser.GetDataAreaId()
                };
                request.EmployeeId = await GetCurrentEmployeeIdAsync(_currentUser.GetCurrentUserId(), cancellationToken);
                var execution = await BuildWorkflowStartPlanAsync(request, prepared, cancellationToken);
                await OnBeforeAddAsync(request, cancellationToken);
                await _unitOfWork.Repository<WfRequest>().AddAsync(request, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                var controlDetails = scored.Select(item =>
                {
                    var (valueAr, valueEn, _) = LocalizedControlValues(item.Control, item.Value);
                    return new
                    {
                        item.Control.RequestControlId,
                        Detail = new WfRequestDetail
                        {
                            ProcessId = submission.ProcessId, RequestId = request.RecId,
                            ControlId = item.Control.ControlId, ControlDataId = item.Control.RequestControlId,
                            ControlValue = item.Value,
                            Name = item.Control.Label,
                            NameAlias = item.Control.LabelAr ?? string.Empty,
                            ValueAlias = valueAr,
                            Value = valueEn,
                            SortOrder = item.Control.SortOrder,
                            Score = item.Score,
                            EarnedScore = item.Score,
                            DataAreaId = request.DataAreaId
                        }
                    };
                }).ToList();
                var optionDetails = selectedOptionContexts
                    .Where(item => item.Option.FeatureConfiguration.RequireFileUpload &&
                        featureValues.TryGetValue(item.Option.OptionId, out var fileValue) && !IsEmpty(fileValue))
                    .Select(item =>
                    {
                        var fileValue = featureValues[item.Option.OptionId];
                        return new
                        {
                            item.Control.RequestControlId,
                            item.Option.OptionId,
                            Detail = new WfRequestDetail
                            {
                                ProcessId = submission.ProcessId, RequestId = request.RecId,
                                ControlId = item.Control.ControlId, ControlDataId = item.Control.RequestControlId,
                                ControlValue = fileValue,
                                Name = item.Control.Label,
                                NameAlias = item.Control.LabelAr ?? string.Empty,
                                SortOrder = item.Control.SortOrder,
                                Score = 0,
                                EarnedScore = 0,
                                DataAreaId = request.DataAreaId
                            }
                        };
                    }).ToList();
                var details = controlDetails.Select(item => item.Detail)
                    .Concat(optionDetails.Select(item => item.Detail)).ToList();
                await _unitOfWork.Repository<WfRequestDetail>().AddRangeAsync(details, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                var requestControlIds = controlDetails.Select(item => item.RequestControlId).Distinct().ToList();
                var requestOptions = await _context.WfRequestControlsOptions.AsNoTracking()
                    .Where(option => requestControlIds.Contains(option.RequestControlId) && option.IsActive)
                    .OrderBy(option => option.SortOrder).ThenBy(option => option.RecId)
                    .ToListAsync(cancellationToken);
                request.RequestDetails = SerializeRequestDetails(
                    submission.ProcessId,
                    details,
                    requestOptions);
                foreach (var variable in execution.Variables) variable.RequestId = request.RecId;
                foreach (var assignment in execution.Assignments) assignment.RequestId = request.RecId;
                await _unitOfWork.Repository<IAX.IXApi.Modules.Workflow.Processes.WfProcessVariable>().AddRangeAsync(execution.Variables, cancellationToken);
                await _unitOfWork.Repository<WfAssignment>().AddRangeAsync(execution.Assignments, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                var alertOptions = selectedOptionContexts
                    .Where(item => item.Option.FeatureConfiguration.SendAlertMessage &&
                        !string.IsNullOrWhiteSpace(item.Option.FeatureConfiguration.AlertMessage) &&
                        item.Option.FeatureConfiguration.PerformerIds.Count > 0)
                    .ToList();
                if (alertOptions.Count > 0)
                {
                    var performerIds = alertOptions.SelectMany(item => item.Option.FeatureConfiguration.PerformerIds).Distinct().ToList();
                    var performerUsers = await _context.Set<WfPerformerUsers>().AsNoTracking()
                        .Where(item => performerIds.Contains(item.PerformerId))
                        .Select(item => new { item.PerformerId, item.UserID })
                        .ToListAsync(cancellationToken);
                    var alertEmployeeIds = performerUsers.Select(x => x.UserID).Distinct().ToList();
                    var alertAccounts = await _context.Set<HcmWorker>().AsNoTracking()
                        .Where(x => alertEmployeeIds.Contains(x.RecId) && x.IsActive && !x.IsDeleted && x.UserId != null)
                        .ToDictionaryAsync(x => x.RecId, x => x.UserId!, cancellationToken);
                    foreach (var item in alertOptions)
                    {
                        var recipients = performerUsers
                            .Where(user => item.Option.FeatureConfiguration.PerformerIds.Contains(user.PerformerId))
                            .Where(user => alertAccounts.ContainsKey(user.UserID))
                            .Select(user => alertAccounts[user.UserID])
                            .Distinct()
                            .ToList();
                        if (recipients.Count == 0) continue;
                        foreach (var recipient in recipients)
                            _context.Set<IAX.IXApi.Modules.Communication.Notifications.Entities.SysScheduledNotification>().Add(
                                NotificationIntent(request, recipient, $"Workflow request {request.Code ?? request.RecId.ToString(CultureInfo.InvariantCulture)}",
                                    item.Option.FeatureConfiguration.AlertMessage));
                    }
                }
                await QueueAssignmentNotificationsAsync(request, execution.Assignments, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                var attachmentOwners = controlDetails.Select(item => new DynamicRequestAttachmentOwnerDto
                {
                    RequestControlId = item.RequestControlId, DetailRecId = item.Detail.RecId
                }).Concat(optionDetails.Select(item => new DynamicRequestAttachmentOwnerDto
                {
                    RequestControlId = item.RequestControlId, OptionId = item.OptionId, DetailRecId = item.Detail.RecId
                })).ToList();
                await SaveSubmissionFilesAsync(submission, request.RecId, attachmentOwners, storedPaths, cancellationToken);
                return new SubmitDynamicRequestResultDto
                {
                    RequestId = request.RecId,
                    Code = request.Code,
                    Score = request.Score,
                    StartingStepId = execution.StepId,
                    AssignmentIds = execution.Assignments.Select(item => item.RecId).ToList(),
                    AttachmentOwners = attachmentOwners
                };

            }, cancellationToken);
        }

        internal static string SerializeRequestDetails(
            long processId,
            IEnumerable<(DynamicRequestControlDto Control, string Value, decimal Score)> controls)
        {
            var details = new XElement("Details",
                controls
                .OrderBy(item => item.Control.SortOrder)
                .ThenBy(item => item.Control.RequestControlId)
                .Select(item =>
                {
                    var (valueAr, valueEn, selectedOptions) = LocalizedControlValues(item.Control, item.Value);
                    var serializedOptions = new XElement("WfRequestControlsOptions",
                        item.Control.Options
                                .OrderBy(option => option.SortOrder)
                                .ThenBy(option => option.OptionId)
                                .Select(option => new XElement("WfRequestControlsOption",
                                    new XElement("RecId", option.OptionId),
                                    new XElement("RequestControlId", item.Control.RequestControlId),
                                    new XElement("Value", option.Value),
                                    new XElement("Name", option.Label),
                                    new XElement("NameAlias", option.LabelAlias ?? string.Empty),
                                    new XElement("Score", option.Score),
                                    new XElement("SortOrder", option.SortOrder),
                                    new XElement("ExtendedProperties", option.ExtendedProperties ?? string.Empty))));

                    return new XElement("Control",
                        new XElement("RecId", 0),
                        new XElement("ProcessId", processId),
                        new XElement("RequestId", 0),
                        new XElement("ControlId", item.Control.ControlId),
                        new XElement("ControlDataId", item.Control.RequestControlId),
                        new XElement("Name", item.Control.Label),
                        new XElement("NameAlias", item.Control.LabelAr ?? string.Empty),
                        new XElement("ControlValue", item.Value),
                        new XElement("SortOrder", item.Control.SortOrder),
                        new XElement("ValueAlias", valueAr),
                        new XElement("Value", valueEn),
                        new XElement("Score", item.Score),
                        new XElement("EarnedScore", item.Score),
                        serializedOptions);
                }));

            return details.ToString(SaveOptions.DisableFormatting);
        }

        public static string SerializeRequestDetails(
            long processId,
            IEnumerable<WfRequestDetail> details,
            IEnumerable<WfRequestControlsOption>? options = null)
        {
            var optionsByControl = (options ?? [])
                .OrderBy(option => option.SortOrder).ThenBy(option => option.RecId)
                .GroupBy(option => option.RequestControlId)
                .ToDictionary(group => group.Key, group => group.ToList());
            return new XElement("Details", details
                .OrderBy(detail => detail.SortOrder)
                .ThenBy(detail => detail.ControlDataId)
                .ThenBy(detail => detail.RecId)
                .Select(detail => new XElement("Control",
                    new XElement("RecId", detail.RecId),
                    new XElement("ProcessId", detail.ProcessId ?? processId),
                    new XElement("RequestId", detail.RequestId),
                    new XElement("ControlId", detail.ControlId),
                    new XElement("ControlDataId", detail.ControlDataId),
                    new XElement("Name", detail.Name ?? string.Empty),
                    new XElement("NameAlias", detail.NameAlias ?? string.Empty),
                    new XElement("ControlValue", detail.ControlValue ?? string.Empty),
                    new XElement("SortOrder", detail.SortOrder),
                    new XElement("ValueAlias", FirstText(detail.ValueAlias, detail.ControlValue)),
                    new XElement("Value", FirstText(detail.Value, detail.ControlValue)),
                    new XElement("Score", detail.Score),
                    new XElement("EarnedScore", detail.EarnedScore),
                    new XElement("WfRequestControlsOptions",
                        detail.ControlDataId.HasValue && optionsByControl.TryGetValue(detail.ControlDataId.Value, out var detailOptions)
                            ? detailOptions.Select(option => new XElement("WfRequestControlsOption",
                                new XElement("RecId", option.RecId),
                                new XElement("RequestControlId", option.RequestControlId),
                                new XElement("Value", option.Value),
                                new XElement("Name", option.Name),
                                new XElement("NameAlias", option.NameAlias ?? string.Empty),
                                new XElement("Score", option.Score),
                                new XElement("SortOrder", option.SortOrder),
                                new XElement("ExtendedProperties", option.ExtendedProperties ?? string.Empty)))
                            : []))))
                .ToString(SaveOptions.DisableFormatting);
        }

        public static void ApplyRequestDetailSnapshotValues(
            string requestDetailsXml,
            IEnumerable<WfRequestDetail> details)
        {
            if (string.IsNullOrWhiteSpace(requestDetailsXml)) return;
            var controls = XDocument.Parse(requestDetailsXml).Root?.Elements("Control")
                .Select(control => new
                {
                    Id = long.TryParse(control.Element("ControlDataId")?.Value, out var id) ? id : (long?)null,
                    ValueAlias = control.Element("ValueAlias")?.Value ?? control.Element("ControlValueAR")?.Value,
                    Value = control.Element("Value")?.Value ?? control.Element("ControlValueEN")?.Value,
                    Score = decimal.TryParse(control.Element("Score")?.Value ?? control.Element("Weight")?.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var score) ? score : 0m,
                    EarnedScore = decimal.TryParse(control.Element("EarnedScore")?.Value ?? control.Element("TargetWeight")?.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var earnedScore) ? earnedScore : 0m
                })
                .Where(control => control.Id.HasValue)
                .GroupBy(control => control.Id!.Value)
                .ToDictionary(group => group.Key, group => group.First());
            if (controls == null) return;

            foreach (var detail in details)
            {
                if (!detail.ControlDataId.HasValue ||
                    !controls.TryGetValue(detail.ControlDataId.Value, out var control)) continue;
                detail.ValueAlias = FirstText(control.ValueAlias, detail.ControlValue);
                detail.Value = FirstText(control.Value, detail.ControlValue);
                detail.Score = control.Score;
                detail.EarnedScore = control.EarnedScore;
            }
        }

        private static (string ValueAr, string ValueEn, List<DynamicRequestOptionDto> SelectedOptions)
            LocalizedControlValues(DynamicRequestControlDto control, string value)
        {
            var selected = SelectedValues(value);
            var selectedOptions = control.Options
                .Where(option => selected.Contains(option.Value, StringComparer.Ordinal))
                .OrderBy(option => option.SortOrder)
                .ThenBy(option => option.OptionId)
                .ToList();
            var optionValueAr = string.Join(", ", selectedOptions
                .Select(option => option.LabelAlias ?? option.Label)
                .Where(optionLabel => !string.IsNullOrWhiteSpace(optionLabel)));
            var optionValueEn = string.Join(", ", selectedOptions
                .Select(option => option.Label)
                .Where(optionLabel => !string.IsNullOrWhiteSpace(optionLabel)));
            return (FirstText(optionValueAr, value), FirstText(optionValueEn, value), selectedOptions);
        }

        private sealed record PreparedSubmission(
            DynamicRequestFormDto Form,
            Dictionary<long, string> Values,
            List<DynamicRequestControlDto> Visible,
            Dictionary<long, string> FeatureValues,
            List<(DynamicRequestControlDto Control, DynamicRequestOptionDto Option)> SelectedOptions,
            List<ValidationResult> Errors);

        public async Task<List<ValidationResult>> ValidateSubmissionAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken = default)
        {
            try
            {
                return (await PrepareSubmissionAsync(submission, cancellationToken)).Errors;
            }
            catch (DynamicRequestValidationException exception)
            {
                return exception.Errors;
            }
        }

        private async Task<PreparedSubmission> PrepareSubmissionAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken, bool verifyUploads = false)
        {
            var form = await GetFormDefinitionAsync(submission.ProcessId, cancellationToken)
                ?? throw new KeyNotFoundException("The workflow process was not found or is inactive.");
            var duplicate = submission.Values.GroupBy(item => item.RequestControlId).FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new DynamicRequestValidationException([Error(duplicate.Key, "Request", "A field was submitted more than once.")]);
            var allowedIds = form.Controls.Select(item => item.RequestControlId).ToHashSet();
            var unknown = submission.Values.FirstOrDefault(item => !allowedIds.Contains(item.RequestControlId));
            if (unknown != null)
                throw new DynamicRequestValidationException([Error(unknown.RequestControlId, "Request", "A submitted field is not configured for this process.")]);

            if (verifyUploads) NormalizeUploadedFiles(form, submission);
            var values = DynamicRequestValues.Normalize(form.Controls, submission.Values);
            var optionControlledIds = form.Controls.SelectMany(control => control.Options)
                .Where(option => option.FeatureConfiguration.ShowOtherControls)
                .SelectMany(option => option.FeatureConfiguration.VisibleControlIds).ToHashSet();
            var visibleIds = form.Controls.Where(control =>
                    control.VisibilityCondition == null && !optionControlledIds.Contains(control.RequestControlId))
                .Select(control => control.RequestControlId).ToHashSet();
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var control in form.Controls.Where(control => control.VisibilityCondition != null && !visibleIds.Contains(control.RequestControlId)))
                {
                    var condition = control.VisibilityCondition!;
                    if (visibleIds.Contains(condition.SourceControlId) && IsVisible(condition, values))
                        changed = visibleIds.Add(control.RequestControlId) || changed;
                }
                foreach (var control in form.Controls.Where(control => visibleIds.Contains(control.RequestControlId)))
                {
                    values.TryGetValue(control.RequestControlId, out var selectedValue);
                    var selected = SelectedValues(selectedValue ?? control.DefaultValue ?? string.Empty);
                    foreach (var features in control.Options
                        .Where(option => selected.Contains(option.Value, StringComparer.Ordinal))
                        .Select(option => option.FeatureConfiguration))
                    {
                        if (features.ShowOtherControls)
                            foreach (var visibleControlId in features.VisibleControlIds)
                                changed = visibleIds.Add(visibleControlId) || changed;
                    }
                }
            }
            var visible = form.Controls.Where(control =>
                visibleIds.Contains(control.RequestControlId) && Normalize(control.ControlType) != "label").ToList();
            var errors = new List<ValidationResult>();
            var duplicateFeature = submission.OptionFeatureValues.GroupBy(item => item.OptionId).FirstOrDefault(group => group.Count() > 1);
            if (duplicateFeature != null)
                errors.Add(Error(0, "Option feature", "An option feature was submitted more than once."));
            var featureValues = submission.OptionFeatureValues
                .GroupBy(item => item.OptionId)
                .ToDictionary(group => group.Key, group => group.First().FileValue?.Trim() ?? string.Empty);
            var selectedOptionContexts = visible.SelectMany(control =>
            {
                values.TryGetValue(control.RequestControlId, out var selectedValue);
                var selected = SelectedValues(selectedValue ?? control.DefaultValue ?? string.Empty);
                return control.Options
                    .Where(option => selected.Contains(option.Value, StringComparer.Ordinal))
                    .Select(option => (Control: control, Option: option));
            }).ToList();
            var selectedOptionIds = selectedOptionContexts.Select(item => item.Option.OptionId).ToHashSet();
            if (featureValues.Keys.Any(optionId => !selectedOptionIds.Contains(optionId)))
                errors.Add(Error(0, "Option feature", "A feature value was submitted for an option that is not selected."));
            foreach (var item in selectedOptionContexts.Where(item => item.Option.FeatureConfiguration.RequireFileUpload))
            {
                featureValues.TryGetValue(item.Option.OptionId, out var fileValue);
                if (IsEmpty(fileValue) || FileValues(fileValue ?? string.Empty).Count == 0)
                    errors.Add(Error(item.Control.RequestControlId, item.Control.Label, $"{item.Option.Label} file upload is required."));
                else if ((fileValue ?? string.Empty).Length > SubmissionValueLimit)
                    errors.Add(Error(item.Control.RequestControlId, item.Control.Label, $"The uploaded file metadata cannot exceed {SubmissionValueLimit} characters."));
            }
            foreach (var control in visible)
            {
                values.TryGetValue(control.RequestControlId, out var value);
                value ??= control.DefaultValue ?? string.Empty;
                if (value.Length > SubmissionValueLimit)
                    errors.Add(Error(control.RequestControlId, control.Label, $"The value cannot exceed {SubmissionValueLimit} characters."));
                if (control.Required && IsEmpty(value))
                    errors.Add(Error(control.RequestControlId, control.Label, $"{control.Label} is required."));
                errors.AddRange(ValidateRules(control, value, form.Controls, values));
                if (!SelectionIsValid(control, value))
                    errors.Add(Error(control.RequestControlId, control.Label, "The selected value is not a configured option."));
            }
            var requiresApplicantUniqueness = visible.Any(control => control.Validations.Any(rule =>
                Normalize(rule.Type) == "uniqueperapplicant"));
            var currentUserId = _currentUser.GetCurrentUserId();
            var currentEmployeeId = requiresApplicantUniqueness
                ? await GetCurrentEmployeeIdAsync(currentUserId, cancellationToken)
                : null;
            foreach (var control in visible)
            {
                values.TryGetValue(control.RequestControlId, out var value);
                if (IsEmpty(value)) continue;
                var validationTypes = control.Validations.Select(rule => Normalize(rule.Type)).ToHashSet();
                var uniqueGlobal = control.UniqueKey || validationTypes.Contains("unique") ||
                    validationTypes.Contains("uniqueglobal");
                if (uniqueGlobal && await _context.WfRequestDetails.AnyAsync(
                    detail => detail.ControlDataId == control.RequestControlId && detail.ControlValue == value,
                    cancellationToken))
                {
                    errors.Add(Error(control.RequestControlId, control.Label,
                        RuleMessage(control, "uniqueglobal", $"{control.Label} must be unique across all requests.")));
                    continue;
                }
                if (validationTypes.Contains("uniqueperapplicant"))
                {
                    var usedByApplicant = await _context.WfRequestDetails.AnyAsync(detail =>
                        detail.ControlDataId == control.RequestControlId && detail.ControlValue == value &&
                        _context.WfRequests.Any(request => request.RecId == detail.RequestId &&
                            (currentEmployeeId.HasValue
                                ? request.EmployeeId == currentEmployeeId.Value
                                : request.CreatedBy == currentUserId)), cancellationToken);
                    if (usedByApplicant)
                        errors.Add(Error(control.RequestControlId, control.Label,
                            RuleMessage(control, "uniqueperapplicant", $"{control.Label} must be unique for this applicant.")));
                }
            }
            if (verifyUploads) ValidateUploadTargets(submission, visible, selectedOptionContexts);
            return new PreparedSubmission(form, values, visible, featureValues, selectedOptionContexts, errors);

        }

        private static string RuleMessage(DynamicRequestControlDto control, string normalizedType, string fallback) =>
            control.Validations.FirstOrDefault(rule => Normalize(rule.Type) == normalizedType)?.ErrorMessage
            is { Length: > 0 } message ? message : fallback;

        internal static IEnumerable<ValidationResult> ValidateRules(
            DynamicRequestControlDto control,
            string value,
            IReadOnlyCollection<DynamicRequestControlDto> controls,
            IReadOnlyDictionary<long, string> values)
        {
            foreach (var rule in control.Validations)
            {
                var type = Normalize(rule.Type);
                if (!SubmissionRuleTypes.Contains(type))
                {
                    yield return Error(control.RequestControlId, control.Label, $"Unsupported validation rule: {rule.Type}.");
                    continue;
                }
                if (IsEmpty(value) && type != "required") continue;
                var operand = rule.Value ?? rule.Expression ?? string.Empty;
                var valid = type switch
                {
                    "required" => !IsEmpty(value),
                    "minlength" => int.TryParse(operand, out var minLength) && value.Length >= minLength,
                    "maxlength" => int.TryParse(operand, out var maxLength) && value.Length <= maxLength,
                    "exactlength" or "length" => int.TryParse(operand, out var length) && value.Length == length,
                    "minvalue" => TryDecimal(value, out var minValue) && TryDecimal(operand, out var minimum) && minValue >= minimum,
                    "mindate" or "maxdate" => DateBoundRule.IsValid(type, value, operand, DateOnly.FromDateTime(DateTime.UtcNow)),
                    "maxvalue" => TryDecimal(value, out var maxValue) && TryDecimal(operand, out var maximum) && maxValue <= maximum,
                    "range" => TryDecimal(value, out var rangeValue) && TryDecimal(rule.Value, out var from) && TryDecimal(rule.Expression, out var to) && rangeValue >= from && rangeValue <= to,
                    "regex" or "pattern" => SafeRegex(value, rule.Expression),
                    "mask" or "inputmask" => InputMaskRule.IsValid(value, rule.Mask),
                    "unique" or "uniqueglobal" or "uniqueperapplicant" => true, // Checked against persisted answers.
                    "email" => SafeRegex(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
                    "url" => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                    "phone" => SafeRegex(value, @"^[+0-9\s()-]{7,20}$"),
                    "saudimobile" => SafeRegex(value, @"^(?:\+966|00966|966|0)?5\d{8}$"),
                    "saudinationalid" => SafeRegex(value, @"^[12]\d{9}$"),
                    "saudiiban" => SafeRegex(value.Replace(" ", ""), @"^SA\d{22}$"),
                    "taxnumber" => SafeRegex(value, @"^\d{15}$"),
                    "passport" => SafeRegex(value, @"^[A-Za-z0-9]{6,12}$"),
                    "startswith" => value.StartsWith(operand, StringComparison.OrdinalIgnoreCase),
                    "endswith" => value.EndsWith(operand, StringComparison.OrdinalIgnoreCase),
                    "contains" => value.Contains(operand, StringComparison.OrdinalIgnoreCase),
                    "fileextensions" or "fileextension" or "allowedextensions" or "allowedtypes" => FileExtensionsValid(value, operand),
                    "filesize" or "maxfilesize" => FileSizesValid(value, operand),
                    "minselected" => int.TryParse(operand, out var minSelected) && SelectionCount(value) >= minSelected,
                    "maxselected" or "maxfiles" => int.TryParse(operand, out var maxSelected) && SelectionCount(value) <= maxSelected,
                    "compare" or "comparison" or "crossfield" or "expression" or "custom" or "customexpression" or "conditional" => EvaluateConfiguredRule(rule, value, controls, values),
                    _ => false
                };
                if (!valid) yield return new ValidationResult
                {
                    RequestControlId = control.RequestControlId, ControlName = control.Label,
                    ErrorMessage = rule.ErrorMessage, Severity = rule.Severity
                };
            }
        }

        private static bool EvaluateConfiguredRule(
            DynamicRequestValidationDto rule,
            string currentValue,
            IReadOnlyCollection<DynamicRequestControlDto> controls,
            IReadOnlyDictionary<long, string> values)
        {
            var expression = rule.Expression;
            if (!string.IsNullOrWhiteSpace(expression))
            {
                expression = expression.Replace("{value}", currentValue, StringComparison.OrdinalIgnoreCase);
                foreach (var control in controls)
                {
                    values.TryGetValue(control.RequestControlId, out var otherValue);
                    expression = expression.Replace($"{{{control.Code}}}", otherValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                    expression = expression.Replace($"{{{control.RequestControlId}}}", otherValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                }
                foreach (var comparisonOperator in new[] { ">=", "<=", "!=", "==", "=", ">", "<" })
                {
                    var index = expression.IndexOf(comparisonOperator, StringComparison.Ordinal);
                    if (index < 0) continue;
                    return Compare(expression[..index].Trim(), expression[(index + comparisonOperator.Length)..].Trim(), comparisonOperator);
                }
                return false;
            }
            return Compare(currentValue, rule.Value ?? string.Empty, rule.Operator ?? "=");
        }

        private static bool Compare(string left, string right, string comparisonOperator)
        {
            left = left.Trim(' ', '\'', '"'); right = right.Trim(' ', '\'', '"');
            if (TryDecimal(left, out var leftNumber) && TryDecimal(right, out var rightNumber))
                return comparisonOperator switch
                {
                    ">" => leftNumber > rightNumber, "<" => leftNumber < rightNumber,
                    ">=" => leftNumber >= rightNumber, "<=" => leftNumber <= rightNumber,
                    "!=" or "<>" => leftNumber != rightNumber, _ => leftNumber == rightNumber
                };
            return comparisonOperator switch
            {
                "!=" or "<>" => !left.Equals(right, StringComparison.OrdinalIgnoreCase),
                "contains" => left.Contains(right, StringComparison.OrdinalIgnoreCase),
                _ => left.Equals(right, StringComparison.OrdinalIgnoreCase)
            };
        }

        private static bool IsVisible(DynamicRequestConditionDto? condition, IReadOnlyDictionary<long, string> values)
        {
            if (condition == null) return true;
            values.TryGetValue(condition.SourceControlId, out var actual);
            actual ??= string.Empty;
            var expected = condition.Value ?? string.Empty;
            return condition.Operator switch
            {
                "!=" or "<>" => !actual.Equals(expected, StringComparison.OrdinalIgnoreCase),
                ">" => TryDecimal(actual, out var left) && TryDecimal(expected, out var right) && left > right,
                "<" => TryDecimal(actual, out var left) && TryDecimal(expected, out var right) && left < right,
                ">=" => TryDecimal(actual, out var left) && TryDecimal(expected, out var right) && left >= right,
                "<=" => TryDecimal(actual, out var left) && TryDecimal(expected, out var right) && left <= right,
                "contains" => SelectedValues(actual).Contains(expected, StringComparer.OrdinalIgnoreCase) || actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
                "isEmpty" => IsEmpty(actual),
                _ => actual.Equals(expected, StringComparison.OrdinalIgnoreCase) || SelectedValues(actual).Contains(expected, StringComparer.OrdinalIgnoreCase)
            };
        }

        private static RuntimeProperties ParseProperties(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                var properties = new RuntimeProperties
                {
                    LabelAr = GetString(root, "labelAR"), LabelColor = GetString(root, "labelColor"), DefaultValue = GetString(root, "defaultValue"),
                    Required = GetBoolean(root, "required"), ReadOnly = GetBoolean(root, "readOnly"),
                    UniqueKey = GetBoolean(root, "uniqueKey"), UsedAsCriteria = GetBoolean(root, "usedAsCriteria"),
                    ColumnSpan = (byte)Math.Clamp(GetLong(root, "columnSpan"), 1, 3)
                };
                if (root.TryGetProperty("referenceFilter", out var referenceFilter)
                    && referenceFilter.ValueKind == JsonValueKind.Object)
                {
                    var departmentId = GetLong(referenceFilter, "departmentId");
                    var occupationId = GetLong(referenceFilter, "occupationId");
                    var managerLevel = GetLong(referenceFilter, "managerLevel");
                    properties.ReferenceFilter = new RuntimeReferenceFilter
                    {
                        DepartmentId = departmentId > 0 ? checked((short)departmentId) : null,
                        OccupationId = occupationId > 0 ? checked((short)occupationId) : null,
                        ManagerLevel = managerLevel is >= 1 and <= 3 ? checked((byte)managerLevel) : null,
                        Rules = ReadReferenceFilterRules(referenceFilter)
                    };
                }
                if (root.TryGetProperty("visibilityCondition", out var condition) && condition.ValueKind == JsonValueKind.Object)
                {
                    var source = GetLong(condition, "sourceControlId");
                    if (source > 0) properties.VisibilityCondition = new DynamicRequestConditionDto
                    {
                        SourceControlId = source, Operator = GetString(condition, "operator") ?? "=",
                        Value = GetString(condition, "value") ?? string.Empty
                    };
                }
                if (root.TryGetProperty("optionFeatureConfigurations", out var optionFeatures) && optionFeatures.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in optionFeatures.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object) continue;
                        properties.OptionFeatureConfigurations.Add(ParseOptionFeatures(item));
                    }
                }
                return properties;
            }
            catch (JsonException) { return new(); }
        }

        private static DynamicRequestOptionFeatureDto ParseOptionFeatures(
            string? json,
            DynamicRequestOptionFeatureDto? fallback = null)
        {
            if (string.IsNullOrWhiteSpace(json)) return fallback ?? new();
            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.ValueKind == JsonValueKind.Object
                    ? ParseOptionFeatures(document.RootElement)
                    : fallback ?? new();
            }
            catch (JsonException) { return fallback ?? new(); }
        }

        private static DynamicRequestOptionFeatureDto ParseOptionFeatures(JsonElement item)
        {
            var visibleControlIds = ReadLongArray(item, "visibleControlIds");
            return new DynamicRequestOptionFeatureDto
            {
                RequireFileUpload = GetBoolean(item, "requireFileUpload") || GetBoolean(item, "allowFileUpload"),
                SendAlertMessage = GetBoolean(item, "sendAlertMessage") || GetBoolean(item, "sendAlert"),
                AlertMessage = GetString(item, "alertMessage") ?? string.Empty,
                PerformerIds = ReadLongArray(item, "performerIds"),
                ShowOtherControls = GetBoolean(item, "showOtherControls") || visibleControlIds.Count > 0,
                VisibleControlIds = visibleControlIds,
            };
        }

        private static List<long> ReadLongArray(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var items) || items.ValueKind != JsonValueKind.Array) return [];
            var result = new List<long>();
            foreach (var value in items.EnumerateArray())
            {
                long parsed = 0;
                if (value.ValueKind == JsonValueKind.String) long.TryParse(value.GetString(), out parsed);
                else if (value.ValueKind == JsonValueKind.Number) value.TryGetInt64(out parsed);
                if (parsed > 0) result.Add(parsed);
            }
            return result.Distinct().ToList();
        }

        private static string? GetString(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        private static bool GetBoolean(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
        private static long GetLong(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var value)) return 0;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var result)) return result;
            return value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out var parsed) ? parsed : 0;
        }
        private static List<MailFieldSource> ParseSerializedRequestDetails(string? serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized) || !serialized.TrimStart().StartsWith('<')) return [];
            try
            {
                using var text = new StringReader(serialized);
                using var reader = XmlReader.Create(text, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
                var document = XDocument.Load(reader, LoadOptions.None);
                return document.Descendants("Control").Select((control, index) =>
                {
                    byte? controlId = byte.TryParse(control.Element("ControlId")?.Value, out var parsedControlId) ? parsedControlId : null;
                    long? controlDataId = long.TryParse(control.Element("ControlDataId")?.Value, out var parsedDataId) ? parsedDataId : null;
                    var order = byte.TryParse(control.Element("SortOrder")?.Value ?? control.Element("ControlOrder")?.Value, out var parsedOrder) ? parsedOrder : (byte)Math.Min(index, byte.MaxValue);
                    return new MailFieldSource(
                        index + 1, controlId, controlDataId,
                        control.Element("Name")?.Value ?? control.Element("ControlLabel")?.Value ?? string.Empty,
                        control.Element("NameAlias")?.Value ?? control.Element("ControlLabelAR")?.Value ?? string.Empty,
                        control.Element("ControlValue")?.Value ?? string.Empty,
                        control.Element("ValueAlias")?.Value ?? control.Element("ControlValueAR")?.Value ?? string.Empty,
                        control.Element("Value")?.Value ?? control.Element("ControlValueEN")?.Value ?? string.Empty,
                        order);
                }).ToList();
            }
            catch (XmlException) { return []; }
        }
        private static List<MailFieldSource> MergeMailFieldSources(List<MailFieldSource> databaseRows, List<MailFieldSource> serializedRows)
        {
            static string Key(MailFieldSource item) => item.ControlDataId is > 0
                ? $"data:{item.ControlDataId.Value}"
                : $"control:{item.ControlId}:{item.Order}:{Normalize(FirstText(item.LabelAr, item.Label))}";

            var serializedByKey = serializedRows.GroupBy(Key).ToDictionary(group => group.Key, group => group.First());
            var result = databaseRows.Select(database =>
            {
                if (!serializedByKey.Remove(Key(database), out var serialized)) return database;
                return database with
                {
                    ControlId = database.ControlId ?? serialized.ControlId,
                    ControlDataId = database.ControlDataId ?? serialized.ControlDataId,
                    Label = FirstText(database.Label, serialized.Label),
                    LabelAr = FirstText(database.LabelAr, serialized.LabelAr),
                    Value = FirstText(database.Value, serialized.Value),
                    ValueAr = FirstText(database.ValueAr, serialized.ValueAr),
                    ValueEn = FirstText(database.ValueEn, serialized.ValueEn)
                };
            }).ToList();
            result.AddRange(serializedRows.Where(serialized => serializedByKey.ContainsKey(Key(serialized))));
            return result.OrderBy(item => item.Order).ThenBy(item => item.DetailId).ToList();
        }
        private static string FirstText(params string?[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
        private static bool LooksSerialized(string value) => value.TrimStart().StartsWith("<Details", StringComparison.OrdinalIgnoreCase);
        private static bool IsSignature(string? label, string? labelAr, string? value)
        {
            var metadata = Normalize($"{label} {labelAr}");
            var raw = value?.TrimStart() ?? string.Empty;
            return metadata.Contains("signature") || (labelAr?.Contains("توقيع", StringComparison.OrdinalIgnoreCase) ?? false)
                || raw.StartsWith("sig:", StringComparison.OrdinalIgnoreCase)
                || raw.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase);
        }
        private static string Normalize(string value) => Regex.Replace(value ?? string.Empty, "[^a-z0-9]", string.Empty, RegexOptions.IgnoreCase).ToLowerInvariant();
        private static string ResolveRuntimeControlType(string? code, string? name, string? controlType)
        {
            var codeKey = Normalize(code ?? string.Empty);
            var metadata = Normalize($"{code} {name} {controlType}");
            if (codeKey is "number" or "digits" || metadata.Contains("digits")) return "number";
            if (codeKey is "textarea" or "longtext" || metadata.Contains("longtext")) return "longtext";
            if (codeKey == "date" || metadata.Contains("calendar")) return "date";
            if (codeKey == "time") return "time";
            if (codeKey == "url") return "url";
            if (metadata.Contains("checkboxlist")) return "checkboxlist";
            if (metadata.Contains("radiobutton") || codeKey == "radio") return "radio";
            if (metadata.Contains("dropdown") || codeKey == "select") return "select";
            if (metadata.Contains("checkbox")) return "checkbox";
            if (metadata.Contains("employeesearch")) return "employeesearch";
            if (metadata.Contains("employeeid")) return "employeeid";
            foreach (var type in new[] { "file", "signature", "table", "label", "showroom", "location", "advertiser" })
                if (metadata.Contains(type)) return type;
            return "text";
        }
        private static bool IsEmpty(string? value) => string.IsNullOrWhiteSpace(value) || value == "[]";
        private static bool IsTrue(string value) => value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1";
        private static bool TryDecimal(string? value, out decimal result) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        private static bool SafeRegex(string value, string? pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;
            try { return Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(250)); }
            catch (ArgumentException) { return false; }
            catch (RegexMatchTimeoutException) { return false; }
        }
        private static List<string> SelectedValues(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return [];
            if (value.TrimStart().StartsWith('['))
            {
                try { return JsonSerializer.Deserialize<List<string>>(value) ?? []; }
                catch (JsonException) { return []; }
            }
            return [value];
        }
        private static List<FileValue> FileValues(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith('[')) return [];
            try
            {
                using var document = JsonDocument.Parse(value);
                if (document.RootElement.ValueKind != JsonValueKind.Array) return [];
                return document.RootElement.EnumerateArray().Select(item =>
                {
                    var name = GetString(item, "n") ?? GetString(item, "name") ?? string.Empty;
                    var sizeElement = item.TryGetProperty("s", out var compactSize) ? compactSize
                        : item.TryGetProperty("size", out var fullSize) ? fullSize : default;
                    var type = GetString(item, "t") ?? GetString(item, "type") ?? string.Empty;
                    return new FileValue(name, sizeElement.ValueKind == JsonValueKind.Number && sizeElement.TryGetInt64(out var size) ? size : -1, type);
                }).Where(item => !string.IsNullOrWhiteSpace(item.Name) && item.Size >= 0).ToList();
            }
            catch (JsonException) { return []; }
        }
        private static int SelectionCount(string value)
        {
            var files = FileValues(value);
            return files.Count > 0 ? files.Count : SelectedValues(value).Count;
        }
        private static bool FileExtensionsValid(string value, string operand)
        {
            var files = FileValues(value);
            if (files.Count == 0) return false;
            var allowed = operand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => item.TrimStart('.')).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return allowed.Count > 0 && files.All(file => allowed.Contains(Path.GetExtension(file.Name).TrimStart('.')) || allowed.Contains(file.Type));
        }
        private static bool FileSizesValid(string value, string operand)
        {
            var files = FileValues(value);
            if (files.Count == 0 || !TryFileSize(operand, out var maximum)) return false;
            return files.All(file => file.Size <= maximum);
        }
        private static bool TryFileSize(string value, out decimal bytes)
        {
            bytes = 0;
            var match = Regex.Match(value.Trim(), @"^(\d+(?:\.\d+)?)\s*(b|kb|mb|gb)?$", RegexOptions.IgnoreCase);
            if (!match.Success || !decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)) return false;
            var multiplier = match.Groups[2].Value.ToLowerInvariant() switch
            {
                "b" => 1m, "kb" => 1024m, "gb" => 1024m * 1024m * 1024m, _ => 1024m * 1024m
            };
            bytes = amount * multiplier;
            return true;
        }
        private static ValidationResult Error(long id, string name, string message) => new() { RequestControlId = id, ControlName = name, ErrorMessage = message, Severity = "Error" };
        private sealed class MailPartyLookup { public long PartyId { get; set; } public string DisplayName { get; set; } = string.Empty; }
        private sealed record MailFieldSource(long DetailId, byte? ControlId, long? ControlDataId, string Label, string LabelAr, string Value, string ValueAr, string ValueEn, byte Order);
        private sealed record FileValue(string Name, long Size, string Type);
        private sealed class RuntimeProperties
        {
            public string? LabelAr { get; set; }
            public string? LabelColor { get; set; }
            public string? DefaultValue { get; set; }
            public bool Required { get; set; }
            public bool ReadOnly { get; set; }
            public bool UniqueKey { get; set; }
            public bool UsedAsCriteria { get; set; }
            public byte ColumnSpan { get; set; } = 1;
            public DynamicRequestConditionDto? VisibilityCondition { get; set; }
            public RuntimeReferenceFilter ReferenceFilter { get; set; } = new();
            public List<DynamicRequestOptionFeatureDto> OptionFeatureConfigurations { get; set; } = [];
        }
        private sealed class RuntimeReferenceFilter
        {
            public short? DepartmentId { get; set; }
            public short? OccupationId { get; set; }
            public byte? ManagerLevel { get; set; }
            public List<RuntimeReferenceFilterRule> Rules { get; set; } = [];
        }
        private sealed record RuntimeReferenceFilterRule(string Field, string Operator, string Value);
    }
}
