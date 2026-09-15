using IAX.IXApi.Shared.Application.Batch;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public class BatchServiceRegistryTests
{
    [Fact]
    public async Task AxSyncServicePassesJsonParametersToConnector()
    {
        var connector = new RecordingAxSyncProcessor();
        var service = new AxSyncBatchService(connector);

        await service.ProcessAsync(new BatchExecutionContext
        {
            ParametersJson = "{\"entity\":\"CustTable\",\"modifiedSince\":\"2026-09-01T00:00:00Z\"}"
        }, CancellationToken.None);

        Assert.Equal("CustTable", connector.Parameters?.Entity);
        Assert.Equal(DateTimeOffset.Parse("2026-09-01T00:00:00Z"), connector.Parameters?.ModifiedSince);
    }

    [Fact]
    public void FrameworkCanStartWithoutModuleServices()
    {
        using var provider = new ServiceCollection().AddBatchFramework().AddBatchFramework().BuildServiceProvider();
        Assert.Empty(provider.GetRequiredService<IBatchServiceRegistry>().Services);
        Assert.Single(provider.GetServices<IBatchServiceRegistry>());
    }

    [Fact]
    public async Task AllModulesShareOneCatalogAndProcessingContract()
    {
        var services = new ServiceCollection().AddBatchFramework();
        var modules = new[] { "Workflow", "Finance", "Inventory", "Communication", "Integration", "Identity", "Organization", "Reporting" };
        foreach (var module in modules)
            services.AddBatchService<ProcessService>($"{module}.Example", $"{module} example");
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var registry = provider.GetRequiredService<IBatchServiceRegistry>();
        Assert.Equal(modules.Length, registry.Services.Count);
        Assert.Single(provider.GetServices<IBatchServiceRegistry>());
        foreach (var descriptor in registry.Services)
        {
            var result = await registry.Resolve(descriptor.ServiceKey, scope.ServiceProvider)
                .ProcessAsync(new() { ParametersJson = descriptor.ServiceKey }, CancellationToken.None);
            Assert.Equal(descriptor.ServiceKey, result.Message);
            Assert.Equal(1, result.SuccessCount);
        }
    }

    [Fact]
    public async Task ProcessEntryPointRunsThroughRegisteredContract()
    {
        var services = new ServiceCollection().AddBatchService<ProcessService>("process", "Process example");
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = provider.GetRequiredService<IBatchServiceRegistry>().Resolve("process", scope.ServiceProvider);
        var result = await service.ProcessAsync(new() { ParametersJson = "{\"id\":1}" }, CancellationToken.None);
        Assert.Equal("{\"id\":1}", result.Message);
        Assert.Equal(1, result.SuccessCount);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ProcessAsync(new(), new CancellationToken(true)));
    }

    public sealed class ProcessService : BatchService
    {
        public override Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new BatchExecutionResult { ProcessedCount = 1, SuccessCount = 1, Message = context.ParametersJson });
        }
    }

    [Fact]
    public async Task RegistrationDiscoversMetadataAndResolvesScopedServices()
    {
        var services = new ServiceCollection().AddBatchService<TestBatchService>("example", "Example service");
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var registry = provider.GetRequiredService<IBatchServiceRegistry>();
        Assert.Equal(new BatchServiceDescriptor("example", "Example service"), Assert.Single(registry.Services));
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var service = registry.Resolve("EXAMPLE", first.ServiceProvider);
        Assert.Same(service, registry.Resolve("example", first.ServiceProvider));
        Assert.NotSame(service, registry.Resolve("example", second.ServiceProvider));
        var result = await service.ProcessAsync(new() { BatchJobTaskId = 42 }, CancellationToken.None);
        Assert.Equal(42, result.ProcessedCount);
        Assert.Throws<KeyNotFoundException>(() => registry.Resolve("missing", first.ServiceProvider));
    }

    [Fact]
    public void DuplicateAndInvalidKeysFailDuringRegistration()
    {
        var services = new ServiceCollection().AddBatchService<TestBatchService>("example", "Example");
        Assert.Throws<InvalidOperationException>(() => services.AddBatchService<TestBatchService>("EXAMPLE", "Duplicate"));
        Assert.Throws<ArgumentException>(() => services.AddBatchService<TestBatchService>(" example", "Invalid"));
    }

    public sealed class TestBatchService : IBatchService
    {
        public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
            Task.FromResult(new BatchExecutionResult { ProcessedCount = (int)context.BatchJobTaskId });
    }

    private sealed class RecordingAxSyncProcessor : IAxSyncProcessor
    {
        public AxSyncParameters? Parameters { get; private set; }

        public Task<BatchExecutionResult> ProcessAsync(AxSyncParameters parameters, CancellationToken cancellationToken)
        {
            Parameters = parameters;
            return Task.FromResult(new BatchExecutionResult());
        }
    }
}
