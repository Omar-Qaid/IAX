using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesQuotation
{
    public class SalesQuotationLineConfiguration : IEntityTypeConfiguration<SalesQuotationLine>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationLine> builder)
        {
            builder.ToTable("SalesQuotationLine");

        

            
        }
    }
}


