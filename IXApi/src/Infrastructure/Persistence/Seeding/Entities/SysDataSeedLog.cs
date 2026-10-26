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

        public string? SeederId { get; set; }

        public int Version { get; set; } = 1;

        public string Scope { get; set; } = "global";

        public string Outcome { get; set; } = "Success"; // "Success", "Failed", "Skipped"

        public string Policy { get; set; } = "Legacy";

        public long DurationMs { get; set; }

        public string? Error { get; set; }
    }
}
