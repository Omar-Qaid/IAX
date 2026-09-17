namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits
{
    public class OrganizationUnit : MasterEntity<long>
    {
        public DateOnly ValidFrom { get; set; } = DateOnly.MinValue;
        public DateOnly? ValidTo { get; set; }
        public byte OrganizationUnitType { get; set; }
        public long? ParentOrganizationUnitId { get; set; }

        public virtual OrganizationUnit? ParentOrganizationUnit { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; } = new List<OrganizationUnit>();
        public virtual ICollection<IAX.IXApi.Modules.Finance.Foundation.Structure.OrganizationHierarchyNode> HierarchyNodes { get; set; } = new List<IAX.IXApi.Modules.Finance.Foundation.Structure.OrganizationHierarchyNode>();
        public virtual ICollection<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition> Positions { get; set; } = new List<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>();
        public virtual ICollection<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment> WorkerOrganizationAssignments { get; set; } = new List<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment>();
    }
}
