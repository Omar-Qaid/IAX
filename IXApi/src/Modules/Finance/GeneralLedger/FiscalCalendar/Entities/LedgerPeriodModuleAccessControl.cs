using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("LedgerPeriodModuleAccessControl")]
public class LedgerPeriodModuleAccessControl : Entity<long>
{
    public long LedgerFiscalCalendarPeriod { get; set; }
    public LedgerPeriodAccessLevel LedgerAccessLevel { get; set; }
    [StringLength(FieldLengths.Name)] public string LedgerUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel TaxAccessLevel { get; set; }
    [StringLength(FieldLengths.Name)] public string TaxUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel BankAccessLevel { get; set; }
    [StringLength(FieldLengths.Name)] public string BankUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel CustAccessLevel { get; set; }
    [StringLength(FieldLengths.Name)] public string CustUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel VendAccessLevel { get; set; }
    [StringLength(FieldLengths.Name)] public string VendUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel InventAccessLevel { get; set; }
    [StringLength(FieldLengths.Name)] public string InventUserGroupInfo { get; set; } = string.Empty;
}
