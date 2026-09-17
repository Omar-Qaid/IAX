using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public sealed class HcmWorkerConfiguration : IEntityTypeConfiguration<HcmWorker>
{
    public void Configure(EntityTypeBuilder<HcmWorker> builder)
    {
        builder.ToTable("HcmWorker");
        builder.Property(x => x.PersonnelNumber).HasMaxLength(25).IsRequired();

        builder.HasOne(x => x.Gender)
            .WithMany()
            .HasForeignKey(x => x.GenderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Nationality)
            .WithMany()
            .HasForeignKey(x => x.NationalityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
