using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration;

public sealed class SysBackgroundJobMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SysBackgroundJob, SysBackgroundJobDto>()
            .Map(destination => destination.RecurrenceCount,
                source => source.RecurrenceCounts.Sum(count => count.RecurrenceCount));
        config.NewConfig<SysBackgroundJobExecution, SysBackgroundJobExecutionDto>()
            .Map(destination => destination.JobCaption, source => source.Job == null ? null : source.Job.Caption)
            .Map(destination => destination.CreatedAt, source => source.CreatedAt ?? DateTime.UtcNow);
    }
}
