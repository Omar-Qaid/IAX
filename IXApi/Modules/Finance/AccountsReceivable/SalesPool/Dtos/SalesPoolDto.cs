using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class SalesPoolDto : EntityDto<int>
    {

        [Required]
        [StringLength(FieldLengths.SalesPoolId)]
        public string SalesPoolId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
    }
}


