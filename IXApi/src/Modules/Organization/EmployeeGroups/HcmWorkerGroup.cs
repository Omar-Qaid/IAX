using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    /// <summary>
    /// [Unused / Not Needed] Arbitrary worker group entity.
    /// Kept for backward compatibility; optional grouping mechanism.
    /// </summary>
    public class HcmWorkerGroup : MasterEntity<long>
    {
        public virtual ICollection<HcmWorkerGroupDetail> HcmWorkerGroupDetails { get; set; } = new List<HcmWorkerGroupDetail>();
    }
}


