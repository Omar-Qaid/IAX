using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class AppPermissionConfiguration : IEntityTypeConfiguration<AppPermission>
    {
        public void Configure(EntityTypeBuilder<AppPermission> builder)
        {
            builder.ToTable("AspNetPermissions");
            builder.HasKey(x => x.RecId);
            builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Resource).HasMaxLength(100).IsRequired().HasDefaultValue("");
            builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(250);
            builder.HasIndex(x => new { x.Module, x.Resource, x.Action }).IsUnique();
            builder.Ignore(x => x.Key);
        }
    }
}