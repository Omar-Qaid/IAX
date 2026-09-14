using IAX.IXApi.Modules.Workflow;
using IAX.IXApi.Modules.Workflow.Jobs;
using IAX.IXApi.Shared.Application.Batch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public class WorkflowRequestSubmissionBatchServiceTests
{
    [Fact]
    public void WorkflowJobsUseUnifiedNamesAndProcessEntryPoint()
    {
        var jobs = typeof(WorkflowRequestSubmissionBatchService).Assembly.GetTypes()
            .Where(type => type.Namespace == "IAX.IXApi.Modules.Workflow.Jobs" && type.IsClass && !type.IsAbstract).ToList();
        Assert.NotEmpty(jobs);
        foreach (var job in jobs.Where(type => type.IsPublic))
        {
            Assert.EndsWith("BatchService", job.Name);
            Assert.True(typeof(IBatchService).IsAssignableFrom(job));
            Assert.NotNull(job.GetMethod(nameof(IBatchService.ProcessAsync)));
            Assert.Null(job.GetMethod("ExecuteAsync"));
        }
    }

    [Fact]
    public void ModuleExposesSubmissionServiceInRegistry()
    {
        var services = new ServiceCollection();
        services.AddWorkflowModule(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var registry = provider.GetRequiredService<IBatchServiceRegistry>();
        Assert.Contains(registry.Services, service => service.ServiceKey == WorkflowRequestSubmissionBatchService.ServiceKey);
        Assert.Contains(registry.Services, service => service.ServiceKey == WorkflowActivityAutoPassBatchService.ServiceKey);
        Assert.DoesNotContain(registry.Services, service => service.ServiceKey == "WFProcessScheduled");
        Assert.IsType<WorkflowRequestSubmissionBatchService>(registry.Resolve(
            WorkflowRequestSubmissionBatchService.ServiceKey, scope.ServiceProvider));
    }

    [Fact]
    public async Task ScaffoldDoesNotReportSuccessfulSubmission()
    {
        var service = new WorkflowRequestSubmissionBatchService();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.ProcessAsync(new(), CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ProcessAsync(new(), new CancellationToken(true)));
    }
}
