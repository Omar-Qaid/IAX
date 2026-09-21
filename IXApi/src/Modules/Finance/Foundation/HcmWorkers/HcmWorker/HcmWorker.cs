using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Identity.Users;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public class HcmWorker : Entity<long>
{
    [Required, StringLength(25)]
    public string PersonnelNumber { get; set; } = string.Empty;

    public long Person { get; set; }
    public short OccupationId { get; set; }
    public byte GenderId { get; set; }
    public short NationalityId { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? UserId { get; set; }

    public virtual DirPartyTable Party { get; set; } = null!;
    public virtual Occupation Occupation { get; set; } = null!;
    public virtual Gender Gender { get; set; } = null!;
    public virtual Nationality Nationality { get; set; } = null!;
    public virtual AspNetUser? User { get; set; }
    public virtual ICollection<HcmWorkerOrganizationAssignment> WorkerOrganizationAssignments { get; set; } = [];
}
