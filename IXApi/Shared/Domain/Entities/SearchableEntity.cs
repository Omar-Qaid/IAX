namespace IAX.IXApi.Shared.Domain.Entities
{
    public abstract class SearchableEntity : AuditableEntity
    {
        public string SimilaritySearchContent { get; set; } = string.Empty;

        public string UniqueSearchContent { get; set; } = string.Empty;
    }
}
