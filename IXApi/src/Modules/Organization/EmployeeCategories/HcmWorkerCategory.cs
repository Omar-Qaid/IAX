using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    public class HcmWorkerCategory : MasterEntity<long>
    {
        public bool? ForAll { get; set; }
        public bool? Manager1 { get; set; }
        public bool? Manager2 { get; set; }
        public bool? Manager3 { get; set; }
        public bool? Manager4 { get; set; }

        /// <summary>
        /// Linkage rows â€” each pairs the category with one of Department / Occupation / EmployeeGroup.
        /// Inverse side of EmployeeCategoryGroup.UserCategoriesID; no schema change (FK already exists).
        /// </summary>
        public virtual ICollection<HcmWorkerCategoryGroup> Groups { get; set; } = new List<HcmWorkerCategoryGroup>();
    }
}


