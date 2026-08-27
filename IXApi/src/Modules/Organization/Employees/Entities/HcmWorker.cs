using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Occupations;
using IAX.IXApi.Modules.Organization.Genders;
using IAX.IXApi.Modules.Organization.Nationalities;
using IAX.IXApi.Modules.Organization.Showrooms;
using IAX.IXApi.Modules.Organization.HcmWorkerManagers;
using IAX.IXApi.Modules.Identity.Users;

namespace IAX.IXApi.Modules.Organization.Employees.Entities
{
    [Table("HcmWorker")]
    public class HcmWorker : Entity<long>
    {
        //----------------------------------------- Core Identity & Global Directory Links
        // Basic Properties
        [Required]
        [StringLength(25)]
        public string PersonnelNumber { get; set; } = string.Empty; // Unique corporate worker ID key code (e.g., "EMP-000412")

        public long Person { get; set; } // Foreign Key link pointing directly to the DirPartyTable record representing this individual


        public short DepartmentId { get; set; }
        public short OccupationId { get; set; }  
        public DateTime? HireDate { get; set; }  
        public DateTime? BirthDate { get; set; }
        public byte GenderId { get; set; }
        public short NationalityId { get; set; }
        public long? ShowroomId { get; set; }

        #region Navigation Properties Row

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;
        [ForeignKey(nameof(OccupationId))]
        public virtual Occupation Occupation { get; set; } = null!;
        [ForeignKey(nameof(GenderId))]
        public virtual Gender Gender { get; set; } = null!;
        [ForeignKey(nameof(NationalityId))]
        public virtual Nationality Nationality { get; set; } = null!;
        [ForeignKey(nameof(ShowroomId))]
        public virtual Showroom? Showroom { get; set; }
        
        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }
        public virtual AspNetUser? User { get; set; }
        public virtual ICollection<HcmWorkerManager> Managers { get; set; } = new List<HcmWorkerManager>();

        #endregion
    }
}

