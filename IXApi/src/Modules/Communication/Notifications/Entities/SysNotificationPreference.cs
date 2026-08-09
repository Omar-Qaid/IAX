using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// User preferences configuration. Allows toggling individual delivery channels
    /// (InApp, Email, SMS, Push) per notification category (e.g. Workflow, HR, Finance, System).
    /// </summary>
    [Table("SysNotificationPreferences")]
    public class SysNotificationPreference
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = null!;

        public bool EnableInApp { get; set; } = true;

        public bool EnableEmail { get; set; } = true;

        public bool EnableSms { get; set; } = false;

        public bool EnablePush { get; set; } = true;
    }
}

