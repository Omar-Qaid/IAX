using Mapster;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Shared.Application.Organization;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

public sealed class OrganizationStructureMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateOrganizationHierarchyWithRootNode, OrganizationHierarchyNode>().IgnoreNonMapped(true)
            .Map(d => d.OrganizationUnitId, s => s.OrganizationUnitId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo);
        config.NewConfig<OrganizationUnit, OrganizationUnitInfo>().MapWith(x => new OrganizationUnitInfo(x.RecId, x.Code, x.Name, x.OrganizationUnitType, x.ParentOrganizationUnitId, x.ValidFrom, x.ValidTo, x.NameAlias));

        config.NewConfig<OrganizationRole, OrganizationRoleInfo>().MapWith(x => new OrganizationRoleInfo(x.RecId, x.Code, x.Name, x.NameAlias));

        config.NewConfig<OrganizationHierarchy, OrganizationHierarchyInfo>().MapWith(x => new OrganizationHierarchyInfo(x.RecId, x.Code, x.Name, x.Purpose, x.NameAlias));

        config.NewConfig<OrganizationHierarchyNode, OrganizationNodeInfo>().MapWith(x => new OrganizationNodeInfo(x.RecId, x.HierarchyId, x.OrganizationUnitId, x.ParentNodeId, x.ValidFrom, x.ValidTo));

        config.NewConfig<HcmPosition, PositionInfo>().MapWith(x => new PositionInfo(x.RecId, x.Code, x.Name, x.OrganizationUnitId, x.RoleId, x.ValidFrom, x.ValidTo, x.NameAlias));

        config.NewConfig<HcmWorkerOrganizationAssignment, WorkerAssignmentInfo>().MapWith(x => new WorkerAssignmentInfo(x.RecId, x.HcmWorkerId,
            x.PositionId, x.OrganizationUnitId, x.OrganizationRoleId, x.OrganizationRole == null ? null : x.OrganizationRole.Code, x.IsPrimary, x.ValidFrom, x.ValidTo));

        config.NewConfig<CreateOrganizationUnit, OrganizationUnit>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.ParentOrganizationUnitId, s => s.ParentOrganizationUnitId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.OrganizationUnitType, s => s.Type);

        config.NewConfig<UpdateOrganizationUnit, OrganizationUnit>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.ParentOrganizationUnitId, s => s.ParentOrganizationUnitId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.OrganizationUnitType, s => s.Type);

        config.NewConfig<CreateOrganizationRole, OrganizationRole>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias);

        config.NewConfig<UpdateOrganizationRole, OrganizationRole>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias);

        config.NewConfig<CreateOrganizationHierarchy, OrganizationHierarchy>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.Purpose, s => s.Purpose);

        config.NewConfig<CreateOrganizationHierarchyWithRootNode, OrganizationHierarchy>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.Purpose, s => s.Purpose);

        config.NewConfig<UpdateOrganizationHierarchy, OrganizationHierarchy>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.Purpose, s => s.Purpose);

        config.NewConfig<CreateOrganizationNode, OrganizationHierarchyNode>().IgnoreNonMapped(true)
            .Map(d => d.HierarchyId, s => s.HierarchyId)
            .Map(d => d.OrganizationUnitId, s => s.OrganizationUnitId)
            .Map(d => d.ParentNodeId, s => s.ParentNodeId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo);

        config.NewConfig<UpdateOrganizationNode, OrganizationHierarchyNode>().IgnoreNonMapped(true)
            .Map(d => d.OrganizationUnitId, s => s.OrganizationUnitId)
            .Map(d => d.ParentNodeId, s => s.ParentNodeId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo);

        config.NewConfig<CreatePosition, HcmPosition>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.OrganizationUnitId, s => s.OrganizationUnitId)
            .Map(d => d.RoleId, s => s.RoleId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo);

        config.NewConfig<UpdatePosition, HcmPosition>().IgnoreNonMapped(true)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.NameAlias, s => s.NameAlias)
            .Map(d => d.OrganizationUnitId, s => s.OrganizationUnitId)
            .Map(d => d.RoleId, s => s.RoleId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo);

        config.NewConfig<AssignWorker, HcmWorkerOrganizationAssignment>().IgnoreNonMapped(true)
            .Map(d => d.PositionId, s => s.PositionId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.IsPrimary, s => s.IsPrimary)
            .Map(d => d.HcmWorkerId, s => s.WorkerId);

        config.NewConfig<UpdateWorkerAssignment, HcmWorkerOrganizationAssignment>().IgnoreNonMapped(true)
            .Map(d => d.PositionId, s => s.PositionId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.IsPrimary, s => s.IsPrimary);
    }
}
