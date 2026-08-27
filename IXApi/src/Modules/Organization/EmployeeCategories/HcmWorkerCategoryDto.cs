namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    public class HcmWorkerCategoryDto
    {
        public long RecId { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string NameAR { get; set; } = null!;
        public string? Description { get; set; }
        public string? DescriptionAR { get; set; }
        public bool IsActive { get; set; }
        public bool? ForAll { get; set; }
        public bool? Manager1 { get; set; }
        public bool? Manager2 { get; set; }
        public bool? Manager3 { get; set; }
        public bool? Manager4 { get; set; }

        /// <summary>
        /// Linkage rows. NULL means "not provided — leave existing groups untouched"
        /// (so callers that don't manage groups, e.g. the classification panel, are safe).
        /// A non-null list (even empty) replaces the set.
        /// </summary>
        public List<HcmWorkerCategoryGroupDto>? Groups { get; set; }
    }
}