using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IAX.IXApi.Modules.Finance.Persistence;

public interface IFinanceDataContext
{
    DatabaseFacade Database { get; }
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
    DbSet<HcmWorker> HcmWorkers { get; }
    DbSet<HcmDepartment> HcmDepartments { get; }
    DbSet<HcmShowroom> HcmShowrooms { get; }
    DbSet<OrganizationUnit> OrganizationUnits { get; }
    DbSet<HcmWorkerOrganizationAssignment> HcmWorkerOrganizationAssignments { get; }
    DbSet<IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments.HcmWorkerShowroomAssignment> HcmWorkerShowroomAssignments { get; }
    DbSet<OrganizationRole> OrganizationRoles { get; }
    DbSet<OrganizationHierarchy> OrganizationHierarchies { get; }
    DbSet<OrganizationHierarchyNode> OrganizationHierarchyNodes { get; }
    DbSet<HcmPosition> HcmPositions { get; }
    DbSet<HcmReportingHierarchy> HcmReportingHierarchies { get; }
    DbSet<HcmPositionReportingLine> HcmPositionReportingLines { get; }
    DbSet<TaxData> TaxData { get; }
    DbSet<TaxGroupHeading> TaxGroupHeadings { get; }
    DbSet<TaxGroupData> TaxGroupDatas { get; }
    DbSet<TaxOnItem> TaxOnItems { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
