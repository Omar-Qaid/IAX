using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Which account structures the ledger uses. A legal entity has one ledger, and the structures
    /// assigned to it are the ones that validate a ledger account combination at posting.
    ///
    /// Fields are the two the standard table carries, plus the framework's own.
    /// </summary>
    [Table("LedgerStructure")]
    public class LedgerStructure : Entity<long>
    {
        //----------------------------------------- Core Identity
        // Basic Properties
        public long Ledger { get; set; }
        public long DimensionHierarchy { get; set; } // The account structure assigned to the ledger
    }
}
