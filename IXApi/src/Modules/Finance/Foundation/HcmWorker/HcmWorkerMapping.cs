using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers
{
    public class HcmWorkerMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<HcmWorker, HcmWorkerDto>();
        }
    }
}

