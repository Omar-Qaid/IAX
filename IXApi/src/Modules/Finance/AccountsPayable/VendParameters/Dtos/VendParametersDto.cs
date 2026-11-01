using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendParametersDto : EntityDto<long>
    {
        public int Key { get; set; }

        [StringLength(FieldLengths.PostingProfile)]
        public string VendPostingProfile { get; set; } = string.Empty;

        public long Partition { get; set; }
    }
}
