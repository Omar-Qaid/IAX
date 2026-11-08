using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261108105900_AddCustomerPrepaymentParameters")]
public sealed class AddCustomerPrepaymentParameters : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("PrepaymentPostingProfile", "CustParameters",
            type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<int>("TaxOnPrepayment", "CustParameters",
            type: "int", nullable: false, defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("TaxOnPrepayment", "CustParameters");
        migrationBuilder.DropColumn("PrepaymentPostingProfile", "CustParameters");
    }
}
