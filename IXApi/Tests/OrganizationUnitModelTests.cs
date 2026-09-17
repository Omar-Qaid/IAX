using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

namespace IXApi.Tests;

public class OrganizationUnitModelTests
{
    [Fact]
    public void SharedOrganizationTablesAreCompanyFilteredAndMigrationMatchesModel()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var types = new[] { typeof(OrganizationUnit), typeof(HcmWorkerOrganizationAssignment),
            typeof(OrganizationRole), typeof(OrganizationHierarchy),
            typeof(OrganizationHierarchyNode), typeof(HcmPosition) };
        foreach (var type in types)
        {
            var entity = db.Model.FindEntityType(type)!;
            Assert.NotNull(entity.GetQueryFilter());
            Assert.Contains("DataAreaId", entity.GetQueryFilter()!.ToString());
            Assert.Equal(4, entity.FindProperty("DataAreaId")!.GetMaxLength());
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        }
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void OrganizationContextUsesExistingWorkerAndPreservesOptionalRequestLinks()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var unit = db.Model.FindEntityType(typeof(OrganizationUnit))!;
        Assert.Equal("OrganizationUnits", unit.GetTableName());
        Assert.Equal(50, unit.FindProperty(nameof(OrganizationUnit.Code))!.GetMaxLength());
        Assert.Contains(unit.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(new[] { "DataAreaId", "Code" }));
        var parent = Assert.Single(unit.GetForeignKeys());
        Assert.False(parent.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, parent.DeleteBehavior);

        var assignment = db.Model.FindEntityType(typeof(HcmWorkerOrganizationAssignment))!;
        var worker = Assert.Single(assignment.GetForeignKeys(), x => x.Properties.Single().Name == "HcmWorkerId");
        Assert.Equal(typeof(HcmWorker), worker.PrincipalEntityType.ClrType);
        Assert.Equal("RecId", Assert.Single(worker.PrincipalKey.Properties).Name);
        Assert.Equal("date", assignment.FindProperty("ValidFrom")!.GetColumnType());

        var request = db.Model.FindEntityType(typeof(WfRequest))!;
        foreach (var name in new[] { "RequestForHcmWorkerId", "OrganizationUnitId", "HcmWorkerAssignmentId" })
        {
            var relationship = Assert.Single(request.GetForeignKeys(), x => x.Properties.Single().Name == name);
            Assert.False(relationship.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, relationship.DeleteBehavior);
        }
    }
}
