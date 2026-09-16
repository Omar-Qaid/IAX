namespace IAX.IXApi.Modules.Organization.OrganizationUnits
{
    public class OrganizationUnit : WfMasterEntity<long>
    {
        public DateOnly ValidFrom { get; set; } = DateOnly.MinValue;
        public DateOnly? ValidTo { get; set; }
        public byte OrganizationUnitType { get; set; }
        public long? ParentOrganizationUnitId { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual OrganizationUnit? ParentOrganizationUnit { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; } = new List<OrganizationUnit>();
    }
}
