using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Entities;

public sealed class EcoResCategoryConfiguration : IEntityTypeConfiguration<EcoResCategory>
{
    public void Configure(EntityTypeBuilder<EcoResCategory> builder) =>
        builder.HasIndex(category => new { category.CategoryHierarchy, category.Name });
}
