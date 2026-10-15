using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

/// <summary>
/// Removes the earlier non-standard projection table. Item-group posting accounts
/// are stored only in the standard D365 F&amp;O InventPosting table.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261015190000_RemoveNonStandardInventItemGroupForm")]
public partial class RemoveNonStandardInventItemGroupForm : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("DROP TABLE IF EXISTS [InventItemGroupForm];");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally empty: a rollback must not recreate a non-standard ERP table.
    }
}
