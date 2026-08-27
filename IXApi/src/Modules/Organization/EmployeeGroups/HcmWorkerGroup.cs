using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    public class HcmWorkerGroup : MasterEntity<long>
    {
        public virtual ICollection<HcmWorkerGroupDetail> HcmWorkerGroupDetails { get; set; } = new List<HcmWorkerGroupDetail>();
    }
}


