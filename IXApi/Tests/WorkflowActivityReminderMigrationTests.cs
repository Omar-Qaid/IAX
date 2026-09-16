
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace IXApi.Tests;

public class WorkflowActivityReminderMigrationTests
{
    //[Fact]
    //public void AddsOnlyMissingReminderColumnsWithSafeDefaults()
    //{
    //    var migration = new AddWorkflowActivityRecurringReminders
    //    {
    //        ActiveProvider = "Microsoft.EntityFrameworkCore.SqlServer"
    //    };
    //    Assert.Equal(3, migration.UpOperations.Count);
    //    var entity = migration.TargetModel.FindEntityType("IAX.IXApi.Modules.Workflow.Activities.WfActivity")!;
    //    foreach (var operation in migration.UpOperations)
    //    {
    //        var column = Assert.IsType<AddColumnOperation>(operation);
    //        Assert.Equal("WfActivities", column.Table);
    //        Assert.False(column.IsNullable);
    //        Assert.Equal(entity.FindProperty(column.Name)!.ClrType, column.ClrType);
    //        if (column.Name == "IsRecurringReminderEnabled")
    //        {
    //            Assert.Equal("bit", column.ColumnType);
    //            Assert.Equal(false, column.DefaultValue);
    //        }
    //        else
    //        {
    //            Assert.Contains(column.Name, new[] { "MaxReminderOccurrences", "RecurringReminderIntervalHours" });
    //            Assert.Equal("tinyint", column.ColumnType);
    //            Assert.Equal((byte)0, column.DefaultValue);
    //        }
    //    }
    //}
}
