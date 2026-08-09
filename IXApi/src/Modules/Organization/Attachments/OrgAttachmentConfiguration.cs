using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachmentConfiguration : IEntityTypeConfiguration<OrgAttachment>
    {
        public void Configure(EntityTypeBuilder<OrgAttachment> builder)
        {
            builder.ToTable("OrgAttachments");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("AttachmentId")
                .ValueGeneratedOnAdd();


  
        }
    }
}
