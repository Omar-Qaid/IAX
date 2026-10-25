using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Occupations
{
    public class HcmOccupationConfiguration : IEntityTypeConfiguration<HcmOccupation>
    {
        public void Configure(EntityTypeBuilder<HcmOccupation> builder)
        {
            builder.ToTable("HcmOccupations");
        }
    }
}
