namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    /// <summary>
    /// OrgEmployeeCategory is now a BaseEntity (Id/Code/Name/NameAR/IsActive), so the DTO
    /// uses the standard field names and maps automatically via Mapster — no custom
    /// IRegister needed. The Department/Occupation/OrgEmployeeGroup links moved to the
    /// OrgEmployeeCategoryGroup child entity and are managed separately.
    /// </summary>
    public class OrgEmployeeCategoryDto
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
        public List<OrgEmployeeCategoryGroupDto>? Groups { get; set; }
    }

    /// <summary>One linkage row: exactly one of Department / Occupation / OrgEmployeeGroup is set.</summary>
    public class OrgEmployeeCategoryGroupDto
    {
        public long RecId { get; set; }
        public short? DepartmentID { get; set; }
        public short? OccupationID { get; set; }
        public long? UserGroupID { get; set; }
    }
}


