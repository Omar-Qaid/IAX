using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Occupations;
using UserGroupEntity = IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup.HcmWorkerGroup;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    /// <summary>
    /// [Unused / Not Needed] Category mapping entity.
    /// Kept for backward compatibility.
    /// </summary>
    public class HcmWorkerCategoryGroup : MasterEntity<long>
    {
        public long UserCategoriesID { get; set; }
        [ForeignKey(nameof(UserCategoriesID))]
        public virtual HcmWorkerCategory HcmWorkerCategory { get; set; } = null!;

        public short? DepartmentID { get; set; }
        [ForeignKey(nameof(DepartmentID))]
        public virtual Department? Department { get; set; }

        public short? OccupationID { get; set; }
        [ForeignKey(nameof(OccupationID))]
        public virtual Occupation? Occupation { get; set; }

        public long? UserGroupID { get; set; }
        [ForeignKey(nameof(UserGroupID))]
        public virtual UserGroupEntity? HcmWorkerGroup { get; set; }
    }
}



