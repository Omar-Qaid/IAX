namespace IAX.IXApi.Modules.Organization.OrganizationUnits
{
    public class OrganizationUnit : MasterEntity<long>
    {
        public DateOnly ValidFrom { get; set; } = DateOnly.MinValue;
        public DateOnly? ValidTo { get; set; }
        public byte OrganizationUnitType { get; set; }
        public long? ParentOrganizationUnitId { get; set; }

        public virtual OrganizationUnit? ParentOrganizationUnit { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; } = new List<OrganizationUnit>();
        public virtual ICollection<IAX.IXApi.Modules.Organization.Structure.OrganizationHierarchyNode> HierarchyNodes { get; set; } = new List<IAX.IXApi.Modules.Organization.Structure.OrganizationHierarchyNode>();
        public virtual ICollection<IAX.IXApi.Modules.Organization.Structure.HcmPosition> Positions { get; set; } = new List<IAX.IXApi.Modules.Organization.Structure.HcmPosition>();
        public virtual ICollection<IAX.IXApi.Modules.Organization.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment> WorkerOrganizationAssignments { get; set; } = new List<IAX.IXApi.Modules.Organization.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment>();
    }
}
