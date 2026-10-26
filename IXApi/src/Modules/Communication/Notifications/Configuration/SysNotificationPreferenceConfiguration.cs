using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Notifications.Configuration
{
    public class SysNotificationPreferenceConfiguration : IEntityTypeConfiguration<SysNotificationPreference>
    {
        public void Configure(EntityTypeBuilder<SysNotificationPreference> builder)
        {
            builder.ToTable("SysNotificationPreferences");
            builder.HasKey(x => x.RecId);
        }
    }
}
