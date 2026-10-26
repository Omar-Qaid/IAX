namespace IAX.IXApi.Modules.Communication.Notifications.Dtos;

public class SysNotificationPreferenceDto
{
    public long RecId { get; set; }
    public string Category { get; set; } = null!;
    public bool EnableInApp { get; set; }
    public bool EnableEmail { get; set; }
    public bool EnableSms { get; set; }
    public bool EnablePush { get; set; }
}
