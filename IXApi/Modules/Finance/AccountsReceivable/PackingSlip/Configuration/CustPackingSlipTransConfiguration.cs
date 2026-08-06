 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustPackingSlipTransConfiguration : IEntityTypeConfiguration<CustPackingSlipTrans>
    {
        public void Configure(EntityTypeBuilder<CustPackingSlipTrans> builder)
        {
            builder.HasIndex(x => x.PackingSlipId);
            builder.HasIndex(x => x.InventTransId);

       
           


        }
    }
}


