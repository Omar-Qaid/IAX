using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachmentDetailConfiguration : IEntityTypeConfiguration<OrgAttachmentDetail>
    {
        public void Configure(EntityTypeBuilder<OrgAttachmentDetail> builder)
        {
            builder.ToTable("OrgAttachmentDetails");

            builder
         .HasKey(x => x.RecId);

            builder
                .Property(x => x.FileId)
                .ValueGeneratedNever();

            builder.Property(x => x.FileName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.FileType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .IsRequired();

            // Configure one-to-many relationship with Attachment
            builder.HasOne(x => x.Attachment)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.AttachmentId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(ijt => ijt.Attachment)
         .WithMany()
         .HasForeignKey(ijt => ijt.AttachmentId)
         .HasPrincipalKey(tg => tg.RecId)
         .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

