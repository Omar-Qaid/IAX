using DocumentFormat.OpenXml.Office2010.Excel;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    [ScopedService]
    public class WfProcessService : BaseService<WfProcess>, IWfProcessService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfProcessService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfProcess entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfProcess", cancellationToken: cancellationToken);
        }


        public override async Task<PagedResultDto<WfProcess>> GetPagedAsync(QueryFilterDto paginationParams, Func<IQueryable<WfProcess>, IIncludableQueryable<WfProcess, object>>? include = null, CancellationToken cancellationToken = default)
        {
            return await base.GetPagedAsync(paginationParams, include: q => q.Include(x => x.Category).Include(x=>x.Priority), cancellationToken: cancellationToken);

        }

        public override async Task<WfProcess?> GetByIdAsync(object id, Func<IQueryable<WfProcess>, IIncludableQueryable<WfProcess, object>> include, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, include: q => {
                var query = include != null ? include(q) : q;
                return query.Include(x => x.UsersProcesses);
            }, cancellationToken: cancellationToken);
        }

      
        public override async Task<WfProcess> AddAsync(WfProcess entity, CancellationToken cancellationToken = default)
        {
            var incoming = entity.UsersProcesses?.ToList() ?? new List<WfUsersProcess>();
            entity.UsersProcesses = new List<WfUsersProcess>();
            foreach (var up in incoming)
            {
                entity.UsersProcesses.Add(new WfUsersProcess
                {
                    EmployeeId = up.EmployeeId,
                    DepartmentId = up.DepartmentId,
                    OccupationId = up.OccupationId
                });
            }
            return await base.AddAsync(entity, cancellationToken);
        }

        public override async Task<WfProcess> UpdateAsync(WfProcess entity, CancellationToken cancellationToken = default)
        {
            // Capture BEFORE re-fetching: EF Core's identity map returns the same instance,
            // so entity and existing are the same object — without this snapshot, Clear() would
            // also empty the source list and SyncUsersProcesses would insert nothing.
            var incomingUsersProcesses = entity.UsersProcesses?.ToList() ?? new List<WfUsersProcess>();

            var existing = await _repository.GetByIdAsync(entity.RecId, include: q => q.Include(x => x.UsersProcesses), cancellationToken: cancellationToken);
            if (existing != null)
            {
                _unitOfWork.Context.Entry(existing).CurrentValues.SetValues(entity);

                _unitOfWork.Context.RemoveRange(existing.UsersProcesses.ToList());
                existing.UsersProcesses.Clear();

                foreach (var up in incomingUsersProcesses)
                {
                    existing.UsersProcesses.Add(new WfUsersProcess
                    {
                        ProcessId = existing.RecId,
                        EmployeeId = up.EmployeeId,
                        DepartmentId = up.DepartmentId,
                        OccupationId = up.OccupationId
                    });
                }

                return await base.UpdateAsync(existing, cancellationToken);
            }
            return await base.UpdateAsync(entity, cancellationToken);
        }
    }
}
