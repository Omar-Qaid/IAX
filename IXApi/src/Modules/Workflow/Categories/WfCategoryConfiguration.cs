using IAX.IXApi.Modules.Workflow.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Categories
{
    public class WfCategoryConfiguration : IEntityTypeConfiguration<WfCategory>
    {
        public void Configure(EntityTypeBuilder<WfCategory> builder)
        {
            builder.ToTable("WfCategories");

            builder.Property(x => x.RecId);

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}

