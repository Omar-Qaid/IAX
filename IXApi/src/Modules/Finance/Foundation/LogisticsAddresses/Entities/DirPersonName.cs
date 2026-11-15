using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("DirPersonName")]
[Index(nameof(Person), nameof(ValidFrom), IsUnique = true)]
public class DirPersonName : Entity<long>
{
    public long Person { get; set; }
    [ForeignKey(nameof(Person)), InverseProperty(nameof(DirPartyTable.PersonNames))]
    public DirPartyTable PersonParty { get; set; } = null!;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    [MaxLength(FieldLengths.Name)] public string FirstName { get; set; } = "";
    [MaxLength(FieldLengths.Name)] public string MiddleName { get; set; } = "";
    [MaxLength(FieldLengths.Name)] public string LastNamePrefix { get; set; } = "";
    [MaxLength(FieldLengths.Name)] public string LastName { get; set; } = "";
}
