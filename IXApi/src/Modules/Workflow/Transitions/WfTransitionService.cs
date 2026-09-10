using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Operators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;



namespace IAX.IXApi.Modules.Workflow.Transitions
{
    public class WfTransitionService : BaseService<WfTransition>, IWfTransitionService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfTransitionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfTransition entity, CancellationToken cancellationToken)
        {
            await ValidateReferencesAsync(entity, cancellationToken);
            await _sequences.EnsureCodeAsync(entity, entityName: "WfTransition", cancellationToken: cancellationToken);
        }

        public override async Task<WfTransition> UpdateAsync(WfTransition entity, CancellationToken cancellationToken = default)
        {
            await ValidateReferencesAsync(entity, cancellationToken);
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task<IEnumerable<WfTransition>> UpdateRangeAsync(IEnumerable<WfTransition> entities, CancellationToken cancellationToken = default)
        {
            var records = entities.ToArray();
            foreach (var entity in records) await ValidateReferencesAsync(entity, cancellationToken);
            return await base.UpdateRangeAsync(records, cancellationToken);
        }

        private async Task ValidateReferencesAsync(WfTransition entity, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationFailure>();
            var context = _unitOfWork.Context;
            if (!await context.Set<WfProcess>().AnyAsync(x => x.RecId == entity.ProcessId, cancellationToken))
                errors.Add(new(nameof(entity.ProcessId), "The process is unavailable."));
            if (!await context.Set<WfVariable>().AnyAsync(x => x.RecId == entity.VariableId && x.ProcessId == entity.ProcessId, cancellationToken))
                errors.Add(new(nameof(entity.VariableId), "The variable must belong to the transition process."));
            if (!await context.Set<WfStep>().AnyAsync(x => x.RecId == entity.StepId && x.ProcessId == entity.ProcessId, cancellationToken))
                errors.Add(new(nameof(entity.StepId), "The target step must belong to the transition process."));
            if (!await context.Set<WfOperator>().AnyAsync(x => x.RecId == entity.OperatorId, cancellationToken))
                errors.Add(new(nameof(entity.OperatorId), "The operator is unavailable."));
            if (entity.ActivityId.HasValue && entity.RequestControlId.HasValue)
                errors.Add(new(nameof(entity.ActivityId), "A transition can have only one trigger."));
            if (entity.RequestControlId.HasValue && !await context.Set<WfRequestControl>().AnyAsync(
                x => x.RecId == entity.RequestControlId && x.ProcessId == entity.ProcessId, cancellationToken))
                errors.Add(new(nameof(entity.RequestControlId), "The request control must belong to the transition process."));
            if (entity.ActivityId.HasValue && !await context.Set<WfActivity>().AnyAsync(
                x => x.RecId == entity.ActivityId && context.Set<WfStep>().Any(step => step.RecId == x.StepId && step.ProcessId == entity.ProcessId), cancellationToken))
                errors.Add(new(nameof(entity.ActivityId), "The activity must belong to the transition process."));
            if (entity.RecId > 0)
            {
                var savedProcessId = await context.Set<WfTransition>().AsNoTracking()
                    .Where(x => x.RecId == entity.RecId).Select(x => (long?)x.ProcessId).SingleOrDefaultAsync(cancellationToken);
                if (savedProcessId.HasValue && savedProcessId.Value != entity.ProcessId)
                    errors.Add(new(nameof(entity.ProcessId), "An existing transition cannot be moved to another process."));
            }
            if (errors.Count > 0) throw new ValidationException(errors);
        }
    }
}

