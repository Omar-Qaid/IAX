using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendTransOpen : Entity<long>
{
    [MaxLength(20)] public string AccountNum { get; set; } = "";
    public long RefRecId { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }
}
