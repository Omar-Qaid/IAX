using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class SpecTransConfiguration : IEntityTypeConfiguration<SpecTrans>
    {
        public void Configure(EntityTypeBuilder<SpecTrans> builder)
        {
            builder.ToTable("SpecTrans");

        }
    }
}
