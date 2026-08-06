using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustLedger")]
    public class CustLedger : Entity<long>
    {
        //----------------------------------------- Core Properties

        [Required]
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        //----------------------------------------- Posting Options
        public NoYes CollectionLetter { get; set; }

        public NoYes Interest { get; set; }

        public NoYes Settlement { get; set; }


        //----------------------------------------- Navigation Properties (Single)
        #region Navigation Properties Row

        #endregion
        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List
//         public virtual ICollection<CustTable> CustTables { get; set; } = new List<CustTable>();
        #endregion
    }
}
