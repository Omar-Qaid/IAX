using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Nationalities
{
    public class HcmNationalityConfiguration : IEntityTypeConfiguration<HcmNationality>
    {
        public void Configure(EntityTypeBuilder<HcmNationality> builder)
        {
            builder.ToTable("HcmNationalities");
        }
    }
}
