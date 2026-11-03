using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>Basic vendor bank identity; no payment-file or bank-integration behavior.</summary>
/// <remarks>Field names follow Microsoft's VendBankAccount Common Data Model metadata.</remarks>
[Table("VendBankAccount")]
public sealed class VendBankAccount : Entity<long>
{
    [Required, StringLength(10)] public string AccountID { get; set; } = string.Empty;
    [Required, StringLength(20)] public string VendAccount { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(48)] public string AccountNum { get; set; } = string.Empty;
    [Required, StringLength(3)] public string CurrencyCode { get; set; } = string.Empty;
}
