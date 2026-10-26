using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Entities
{
    [Table("SysDataSeedLogs")]
    public class SysDataSeedLog
    {
        [Key]
        public int RecId { get; set; }

        /// <summary>
        /// Legacy seeder name or composite key "{SeederId}:v{Version}:{Scope}".
        /// Preserved for backward-compatibility with existing queries.
        /// </summary>
        public string TableName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string? SeederId { get; set; }

        [NotMapped]
        public int Version { get; set; } = 1;

        [NotMapped]
        public string? Scope { get; set; }

        [NotMapped]
        public string Outcome { get; set; } = "Success"; // "Success", "Failed", "Skipped"

        [NotMapped]
        public long DurationMs { get; set; }

        [NotMapped]
        public string? Error { get; set; }
    }
}
