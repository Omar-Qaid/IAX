using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustTransOpenConfiguration : IEntityTypeConfiguration<CustTransOpen>
    {
        public void Configure(EntityTypeBuilder<CustTransOpen> builder)
        {
            builder.ToTable("CustTransOpen");

            builder.Property(t => t.AmountCur).HasPrecision(18, 4);
            builder.Property(t => t.AmountMst).HasPrecision(18, 4);
            builder.Property(t => t.ExchAdjUnrealized).HasPrecision(18, 4);
            builder.Property(t => t.PossibleCashDisc).HasPrecision(18, 4);

            builder.Property(t => t.AccountNum)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(t => t.RefRecId);
            builder.HasIndex(t => t.AccountNum);
            builder.HasIndex(t => t.DueDate);

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);


        }
    }
}

