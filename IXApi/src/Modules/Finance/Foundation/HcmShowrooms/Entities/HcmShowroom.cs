using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroom : Entity<long>
{
    [Required, StringLength(25)]
    public string PersonnelNumber { get; set; } = string.Empty;

    public long Party { get; set; }

    [ForeignKey(nameof(Party))]
    public virtual DirPartyTable PartyTable { get; set; } = null!;

    public virtual ICollection<IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments.HcmWorkerShowroomAssignment> WorkerShowroomAssignments { get; set; } = [];
}
