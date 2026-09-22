using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public sealed class HcmShowroomConfiguration : IEntityTypeConfiguration<HcmShowroom>
{
    public void Configure(EntityTypeBuilder<HcmShowroom> builder)
    {
        builder.ToTable("HcmShowroom");
        builder.HasOne(x => x.PartyTable)
            .WithMany()
            .HasForeignKey(x => x.Party)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
