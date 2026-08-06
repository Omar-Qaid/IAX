using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
   [Table("DirPartyLocationRole")]
    public class DirPartyLocationRole : Entity<long>
    {
        //----------------------------------------- Core Identity & Intersection Coordinates
        // Basic Properties
        public long PartyLocation { get; set; } // Foreign Key link pointing directly to the specific DirPartyLocation relationship anchor

        public long LocationRole { get; set; }  // Foreign Key link pointing directly to the master LogisticsLocationRole framework definition


        #region Navigation Properties Row

        [ForeignKey(nameof(PartyLocation))]
        public virtual DirPartyLocation? AssociatedPartyLocationContext { get; set; }

        [ForeignKey(nameof(LocationRole))]
        public virtual LogisticsLocationRole? AssignedRoleDetails { get; set; }

        #endregion
    }
}
