using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Entities;

/// <summary>
/// Cash discount: the code, the period within which the discount may be taken, the percentage, and
/// the ledger account the taken discount posts to.
/// </summary>
/// <remarks>
/// The code is referenced by <c>SalesTable.CashDisc</c>, <c>PurchTable.CashDisc</c> and
/// <c>CustTrans.CashDiscCode</c>, which existed before this table did.
///
/// <para>
/// <b>Field shape is not documented in this repository.</b> <c>ARTables.md</c> names
/// <c>CashDisc</c> only as a field on the order and the invoice, never as a table with a field
/// list. The period is therefore modelled on <see cref="PaymTerm"/>'s own
/// <c>NumOfMonths</c>/<c>NumOfDays</c> pair, which is the in-repo precedent for an offset from a
/// base date, and nothing beyond the irreducible minimum was added. Confirm against target D365
/// metadata before relying on this shape; standard <c>CashDisc</c> also carries chained
/// next-discount codes and a comparison basis, which are out of basic scope and absent here.
/// </para>
/// </remarks>
[Table("CashDisc")]
public class CashDisc : Entity<long>
{
    [StringLength(FieldLengths.CashDiscCode)]
    public string CashDiscCode { get; set; } = string.Empty;

    [StringLength(FieldLengths.Description)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Months added to the discount base date.</summary>
    public int NumOfMonths { get; set; }

    /// <summary>Days added to the discount base date.</summary>
    public int NumOfDays { get; set; }

    /// <summary>The discount percentage taken when payment falls on or before the discount date.</summary>
    public decimal DiscPct { get; set; }

    /// <summary>
    /// The ledger account combination the taken discount posts to. A discount cannot be taken
    /// without it, and no default is assumed.
    /// </summary>
    public long CashDiscLedgerDimension { get; set; }
}
