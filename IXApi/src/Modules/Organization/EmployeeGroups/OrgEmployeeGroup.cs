using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroup : MasterEntity<long>
    {
        public virtual ICollection<OrgEmployeeGroupDetail> OrgEmployeeGroupDetails { get; set; } = new List<OrgEmployeeGroupDetail>();
    }
}


