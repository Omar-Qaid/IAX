using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxItemGroupHeadingConfiguration : IEntityTypeConfiguration<TaxItemGroupHeading>
    {
        public void Configure(EntityTypeBuilder<TaxItemGroupHeading> builder)
        {
            builder.ToTable("TaxItemGroupHeading");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }
}
