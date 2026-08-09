using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupDetail : MasterEntity<long>
    {
        public long UserGroupID { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(450)]
        public string UserID { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(UserGroupID))]
        public virtual OrgEmployeeGroup OrgEmployeeGroup { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(UserID))]
        public virtual IAX.IXApi.Modules.Identity.Users.AspNetUser User { get; set; } = null!;
    }
}



