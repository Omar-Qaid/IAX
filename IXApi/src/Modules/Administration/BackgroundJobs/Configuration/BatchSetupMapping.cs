using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration;

public sealed class BatchSetupMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SysBackgroundJobActivePeriod, BatchJobActivePeriodDto>()
            .Map(destination => destination.Id, source => source.Code);
        config.NewConfig<BatchJobActivePeriodDto, SysBackgroundJobActivePeriod>()
            .Map(destination => destination.Code, source => source.Id)
            .Ignore(destination => destination.RecId);
        config.NewConfig<SysBackgroundJobGroup, SysBackgroundJobGroupDto>();
        config.NewConfig<SysBackgroundJobGroupDto, SysBackgroundJobGroup>()
            .Ignore(destination => destination.RecId)
            .Ignore(destination => destination.RowVersion!);
        config.NewConfig<SysBackgroundJobTask, SysBackgroundJobTaskDto>();
        config.NewConfig<SysBackgroundJobTaskDto, SysBackgroundJobTask>()
            .Ignore(destination => destination.RecId);
    }
}
