using Mapster;

namespace IAX.IXApi.Modules.Organization.HcmWorkers
{
    public class HcmWorkerMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<HcmWorker, HcmWorkerDto>();
        }
    }
}

