using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IAX.IXApi.Modules.Workflow.Persistence;

public interface IWorkflowDataContext
{
    DatabaseFacade Database { get; }
    IModel Model { get; }
    DbSet<WfRequestControlsValidation> WfRequestControlsValidations { get; }
    DbSet<WfRequestControl> WfRequestControls { get; }
    DbSet<WfRequestDetail> WfRequestDetails { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
