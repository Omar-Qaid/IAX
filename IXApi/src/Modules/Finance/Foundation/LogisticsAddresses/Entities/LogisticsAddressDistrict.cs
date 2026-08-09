using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LogisticsAddressDistrict")]
    public class LogisticsAddressDistrict : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(60)]
        public string Name { get; set; } = string.Empty; // Official localized naming convention for the district or sector

        [Required]
        [StringLength(60)]
        public string Description { get; set; } = string.Empty; // Alternative description, mailing label name, or search shortcut

        // ==========================================================
        // Parent Municipal Hierarchy Mappings
        // ==========================================================
        // Basic Properties
        public long City { get; set; } // Foreign Key link pointing directly to the parent LogisticsAddressCity record


        #region Navigation Properties Row

        [ForeignKey(nameof(City))]
        public virtual LogisticsAddressCity? LogisticsAddressCityTable { get; set; }

        #endregion
      
    }
}
