using IAX.IXApi.Infrastructure.Migrations;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace IXApi.Tests;

public class ActivityControlMetadataMigrationTests
{
    [Fact]
    public void AddsAllSevenColumnsWithModelDefaultsWithoutChangingExistingColumns()
    {
        var migration = new AddActivityControlMetadata();
        var defaults = new Dictionary<string, object?>
        {
            ["CanFilter"] = true, ["CanGroup"] = true, ["CanSort"] = true,
            ["ReferenceType"] = null, ["FieldRole"] = "Dimension",
            ["DataType"] = "String", ["DefaultAggregation"] = "NONE"
        };
        Assert.Equal(defaults.Count, migration.UpOperations.Count);
        foreach (var operation in migration.UpOperations)
        {
            var column = Assert.IsType<AddColumnOperation>(operation);
            Assert.Equal("WfActivityControls", column.Table);
            Assert.Equal(defaults[column.Name], column.DefaultValue);
            Assert.Equal(column.Name == "ReferenceType", column.IsNullable);
        }
    }

    [Fact]
    public void TargetModelContainsEveryActivityControlProperty()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var actual = db.Model.FindEntityType(typeof(WfActivityControl))!;
        var target = new AddActivityControlMetadata().TargetModel.FindEntityType(typeof(WfActivityControl).FullName!)!;
        foreach (var property in actual.GetProperties())
        {
            var migrated = target.FindProperty(property.Name);
            Assert.NotNull(migrated);
            Assert.Equal(property.ClrType, migrated.ClrType);
            Assert.Equal(property.IsNullable, migrated.IsNullable);
        }
    }
}
