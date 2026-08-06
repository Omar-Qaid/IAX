using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Infrastructure.Persistence.Configuration;

public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : class
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Standard configurations, e.g. mapping table names by type name convention if needed
        // builder.ToTable(typeof(T).Name);
    }
}
