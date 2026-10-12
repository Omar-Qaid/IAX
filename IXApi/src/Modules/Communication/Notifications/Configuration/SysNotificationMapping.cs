using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Communication.Notifications.Configuration;

public sealed class SysNotificationMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SysNotification, SysNotificationDto>()
            .Ignore(destination => destination.Recipients);

        config.NewConfig<SysNotificationRecipient, SysNotificationDto>()
            .Map(destination => destination.IsRead, source => source.IsRead)
            .Map(destination => destination.ReadDate, source => source.ReadDate)
            .Map(destination => destination.IsArchived, source => source.IsArchived)
            .IgnoreNonMapped(true);
    }
}
