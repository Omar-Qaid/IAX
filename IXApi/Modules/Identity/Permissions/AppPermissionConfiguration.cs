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

    public class AppRolePermissionConfiguration : IEntityTypeConfiguration<AppRolePermission>
    {
        public void Configure(EntityTypeBuilder<AppRolePermission> builder)
        {
            builder.ToTable("AspNetRolePermissions");
            builder.HasKey(x => new { x.RoleId, x.PermissionId });

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

