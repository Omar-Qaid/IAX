using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
   [Table("LogisticsLocationRole")]
    public class LogisticsLocationRole : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(40)]
        public string Name { get; set; } = string.Empty; // Unique human-readable system role identifier name (e.g., "Invoice", "Home")

        // Enum Properties
        public LogisticsLocationRoleType Type { get; set; } // Core system enumeration fallback type mapping baseline behaviors

        // ==========================================================
        // Structural Context Assignment Matrix
        // ==========================================================
        // Enum Properties
        public NoYes IsPostalAddress { get; set; } // Flag explicitly validating if this role can be linked to physical mail layouts
        public NoYes IsContactInfo { get; set; }   // Flag explicitly validating if this role can be linked to digital links (Phone, Email)

        // ==========================================================
        // Human Resources Governance & Self-Service Restrictions
        // ==========================================================
        // Enum Properties
        public NoYes DisableAddOrEditInEmployeeSelfService { get; set; } // Hard security block lock preventing modifications via ESS portals
    }
}
