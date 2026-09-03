using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Shared.Domain.Reporting;
using IAX.IXApi.Modules.Workflow.Steps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IAX.IXApi.Modules.Workflow.Persistence;

public interface IWorkflowDataContext
{
    DatabaseFacade Database { get; }
    IModel Model { get; }
    DbSet<WfRequestControlsValidation> WfRequestControlsValidations { get; }
    DbSet<WfRequestControlsOption> WfRequestControlsOptions { get; }
    DbSet<WfRequestControl> WfRequestControls { get; }
    DbSet<WfRequestDetail> WfRequestDetails { get; }
    DbSet<WfActivityDetail> WfActivityDetails { get; }
    DbSet<WfRequest> WfRequests { get; }
    DbSet<WfControl> WfControls { get; }
    DbSet<WfProcess> WfProcesses { get; }
    DbSet<WfStep> WfSteps { get; }
    DbSet<ReportTemplate> ReportTemplates { get; }
    DbSet<ReportTemplateVersion> ReportTemplateVersions { get; }
    DbSet<ReportEntityVersion> ReportEntityVersions { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
