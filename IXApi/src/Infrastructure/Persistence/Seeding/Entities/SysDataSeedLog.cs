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
        public string TableName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

