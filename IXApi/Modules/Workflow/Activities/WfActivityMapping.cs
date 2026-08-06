using IAX.IXApi.Modules.Workflow.Activities;
using Mapster;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WfActivity, WfActivityDto>()
                .Map(dest => dest.Name, src => src.Performer != null && src.ActivityType != null ? $"{src.Performer.Name} - {src.ActivityType.Name}" : "Activity " + src.RecId)
                .Map(dest => dest.NameAR, src => src.Performer != null && src.ActivityType != null ? $"{src.Performer.NameAR} - {src.ActivityType.NameAR}" : "نشاط " + src.RecId)
                .Map(dest => dest.IsActive, src => true);
        }
    }
}

