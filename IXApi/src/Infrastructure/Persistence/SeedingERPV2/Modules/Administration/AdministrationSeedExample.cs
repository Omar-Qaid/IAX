namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// Stable business references described by the Administration manifest and checked against persisted
/// seeders. The sequences listed are the ones the finance cycle examples allocate their document
/// numbers from; the full set is owned by <see cref="AdministrationNumberSequenceCatalog"/>.
/// </summary>
public static class AdministrationSeedExample
{
    public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
    [
        ("SysSettings","AppName","HBMC ERP"),
        ("SysNumberSequence","NumberSequence","Voucher"),
        ("SysNumberSequence","NumberSequence","LedgerJournalTable"),
        ("SysNumberSequence","NumberSequence","PurchTable"),
        ("SysNumberSequence","NumberSequence","VendInvoiceJour"),
        ("SysNumberSequence","NumberSequence","SalesTable"),
        ("SysNumberSequence","NumberSequence","CustInvoiceJour"),
        ("SysBackgroundJob","JobKey","WfActivityAutoPass")
    ];
}
