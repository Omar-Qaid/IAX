using Mapster;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Shared.Domain.Entities
{
    public abstract class BaseEntity<T> : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("RECID")]
        public T RecId { get; set; } = default(T)!;

        [Timestamp] 
        public byte[] RowVersion { get; set; } = default!;
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int RecVersion { get; set; } = 1;

        [Required]
        [MaxLength(4)]
        public string DataAreaId { get; set; } = "dat";

    }
}
