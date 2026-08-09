using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Identity.Users
{
    public class AspNetUser : IdentityUser<string>
    {
        [MaxLength(256)]
        public override string Id { get => base.Id; set => base.Id = value; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime LastLoginDate { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? LastLockoutDate { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? AccountExpirationDate { get; set; }
        public string ? PhotoUrl { get; set; }

    
        public long? OrgEntityId { get; set; }

        [ForeignKey(nameof(OrgEntityId))]
        public virtual OrgEntity? OrgEntity { get; set; }
    }

    /*
     var entity = user.OrgEntity;   // قد يكون معرضاً أو موظفاً
switch (entity)
{
    case OrgShowroom showroom:   // حساب معرض
        // ... صلاحيات/شاشات المعرض، البائعون: showroom.Sellers
        break;
    case OrgEmployee employee:   // حساب موظف
        // ... شاشات الموظف
        break;
}
     */
}




