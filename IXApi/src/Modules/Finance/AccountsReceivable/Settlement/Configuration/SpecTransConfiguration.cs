using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement
{
    public class SpecTransConfiguration : IEntityTypeConfiguration<SpecTrans>
    {
        public void Configure(EntityTypeBuilder<SpecTrans> builder)
        {
            builder.ToTable("SpecTrans");

        }
    }
}

