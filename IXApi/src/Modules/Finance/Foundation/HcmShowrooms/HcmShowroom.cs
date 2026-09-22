using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroom : Entity<long>
{
    public long Party { get; set; }

    [ForeignKey(nameof(Party))]
    public virtual DirPartyTable PartyTable { get; set; } = null!;
}
