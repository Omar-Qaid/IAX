using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxTransConfiguration : IEntityTypeConfiguration<TaxTrans>
    {
        public void Configure(EntityTypeBuilder<TaxTrans> builder)
        {
            builder.ToTable("TaxTrans");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }
}
