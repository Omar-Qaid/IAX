using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

/// <summary>Singleton operational switch; absent row uses configured defaults.</summary>
[Table("BatchSettings")]
public sealed class BatchSettings
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; } = 1;
    public bool Enabled { get; set; } = true;
    [Range(1, 3600)] public int PollIntervalSeconds { get; set; } = 30;
}
