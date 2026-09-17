using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Identity.Users;

using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers
{
    public class HcmWorker : Entity<long>
    {
        //----------------------------------------- Core Identity & Global Directory Links
        // Basic Properties
        [Required]
        [StringLength(25)]
        public string PersonnelNumber { get; set; } = string.Empty; // Unique corporate worker ID key code (e.g., "EMP-000412")
        public long Person { get; set; } // Foreign Key link pointing directly to the DirPartyTable record representing this individual
        public DateTime? HireDate { get; set; }  
        public DateTime? BirthDate { get; set; }
        public byte GenderId { get; set; }
        public short NationalityId { get; set; }

        #region Navigation Properties Row
        [ForeignKey(nameof(Person))]
        public virtual DirPartyTable? DirPartyTable { get; set; }

        public virtual Gender Gender { get; set; } = null!;
        public virtual Nationality Nationality { get; set; } = null!;
        
        public string? UserId { get; set; }
        public virtual AspNetUser? User { get; set; }
        public virtual ICollection<HcmWorkerOrganizationAssignment> WorkerOrganizationAssignments { get; set; } = new List<HcmWorkerOrganizationAssignment>();

        #endregion
    }
}
