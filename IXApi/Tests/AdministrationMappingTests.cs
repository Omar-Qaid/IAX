using IAX.IXApi.Modules.Administration.BackgroundJobs;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AdministrationMappingTests
{
    [Fact]
    public void Batch_setup_mapping_preserves_record_identity_and_concurrency_token()
    {
        var config = new TypeAdapterConfig();
        new BatchSetupMapping().Register(config);
        var period = new SysBackgroundJobActivePeriod { RecId = 17, Code = "DAY", Name = "Day" };
        var periodDto = period.Adapt<BatchJobActivePeriodDto>(config);
        Assert.Equal("DAY", periodDto.Id);
        new BatchJobActivePeriodDto { RecId = 999, Id = "EVE", Name = "Evening" }.Adapt(period, config);
        Assert.Equal(17, period.RecId);
        Assert.Equal("EVE", period.Code);

        var rowVersion = new byte[] { 1, 2, 3 };
        var group = new SysBackgroundJobGroup { RecId = 25, GroupCode = "A", RowVersion = rowVersion };
        new SysBackgroundJobGroupDto { RecId = 999, GroupCode = "B", RowVersion = [8] }.Adapt(group, config);
        Assert.Equal(25, group.RecId);
        Assert.Same(rowVersion, group.RowVersion);
        Assert.Equal("B", group.GroupCode);
    }

    [Fact]
    public void Job_mapping_preserves_aggregate_and_execution_fields()
    {
        var config = new TypeAdapterConfig();
        new SysBackgroundJobMapping().Register(config);
        var job = new SysBackgroundJob
        {
            RecId = 41, Caption = "Close", JobKey = "close",
            RecurrenceCounts = [new SysBackgroundJobRecurrenceCount { RecurrenceCount = 2 },
                                new SysBackgroundJobRecurrenceCount { RecurrenceCount = 3 }]
        };
        var dto = job.Adapt<SysBackgroundJobDto>(config);
        Assert.Equal(41, dto.RecId);
        Assert.Equal(5, dto.RecurrenceCount);

        var execution = new SysBackgroundJobExecution { RecId = 7, Job = job, CreatedAt = null };
        var executionDto = execution.Adapt<SysBackgroundJobExecutionDto>(config);
        Assert.Equal("Close", executionDto.JobCaption);
        Assert.Equal(7, executionDto.RecId);
        Assert.NotEqual(default, executionDto.CreatedAt);
    }
}
