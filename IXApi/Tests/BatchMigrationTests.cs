using IAX.IXApi.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace IXApi.Tests;

public class BatchMigrationTests
{
    [Fact]
    public void RetirementPreservesScheduleTablesAndDoesNotReactivateOnRollback()
    {
        var migration = new RetireWorkflowProcessScheduling { ActiveProvider = "Microsoft.EntityFrameworkCore.SqlServer" };
        var operation = Assert.IsType<SqlOperation>(Assert.Single(migration.UpOperations));
        Assert.Contains("[JobKey] = N'WFProcessScheduled'", operation.Sql);
        Assert.DoesNotContain("DROP", operation.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DELETE", operation.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(migration.DownOperations);
    }
    [Fact]
    public void PriorityMigrationOnlyAddsPriorityWithNormalDefault()
    {
        var migration = new BatchJobPriority { ActiveProvider = "Microsoft.EntityFrameworkCore.SqlServer" };
        var column = Assert.IsType<AddColumnOperation>(Assert.Single(migration.UpOperations));
        Assert.Equal("BatchJobs", column.Table);
        Assert.Equal("Priority", column.Name);
        Assert.Equal(1, column.DefaultValue);
    }
    [Fact]
    public void UpgradeRenamesExistingTablesWithoutDroppingDataOrChangingWorkflowColumns()
    {
        var migration = new GenericBatchFramework { ActiveProvider = "Microsoft.EntityFrameworkCore.SqlServer" };
        Assert.DoesNotContain(migration.UpOperations, operation => operation is DropTableOperation);
        Assert.Contains(migration.UpOperations, operation => operation is RenameTableOperation rename &&
            rename.Name == "SysBackgroundJobs" && rename.NewName == "BatchJobs");
        Assert.Contains(migration.UpOperations, operation => operation is RenameTableOperation rename &&
            rename.Name == "SysBackgroundJobExecutions" && rename.NewName == "BatchJobHistory");
        Assert.Contains(migration.UpOperations, operation => operation is RenameColumnOperation rename &&
            rename.Table == "BatchJobTasks" && rename.NewName == "ServiceKey");
        Assert.Contains(migration.UpOperations, operation => operation is CreateTableOperation table && table.Name == "BatchSettings");
        Assert.DoesNotContain(migration.UpOperations, operation => operation is AddColumnOperation column && column.Table.StartsWith("Wf"));
        Assert.DoesNotContain(migration.DownOperations, operation => operation is DropColumnOperation column && column.Table.StartsWith("Wf"));
    }
}
