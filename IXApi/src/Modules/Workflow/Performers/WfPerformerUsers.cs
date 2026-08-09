using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerUsers : Entity<long>
    {
        public long PerformerId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(PerformerId))]
        public virtual WfPerformer Performer { get; set; } = null!;

        public long UserID { get; set; }
        public long RelatedField { get; set; }
        public string? ExtendedProperties { get; set; }
    }
}



