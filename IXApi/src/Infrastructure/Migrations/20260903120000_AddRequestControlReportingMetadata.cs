using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260903120000_AddRequestControlReportingMetadata")]
    public partial class AddRequestControlReportingMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "CanFilter", table: "WfRequestControls", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<bool>(name: "CanGroup", table: "WfRequestControls", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<bool>(name: "CanSort", table: "WfRequestControls", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<string>(name: "ReferenceType", table: "WfRequestControls", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "FieldRole", table: "WfRequestControls", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Dimension");
            migrationBuilder.AddColumn<string>(name: "DataType", table: "WfRequestControls", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "String");
            migrationBuilder.AddColumn<string>(name: "DefaultAggregation", table: "WfRequestControls", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NONE");

            migrationBuilder.AddCheckConstraint(name: "CK_WfRequestControls_ReferenceType", table: "WfRequestControls", sql: "[ReferenceType] IS NULL OR [ReferenceType] IN (N'Lookup',N'Employee',N'Showroom',N'Branch',N'Company',N'Department',N'BusinessUnit',N'Area',N'City',N'Country',N'Location',N'Customer',N'Vendor',N'Item',N'ItemGroup',N'Category',N'Warehouse',N'PaymentMethod',N'ViolationType',N'Invoice',N'PurchaseOrder',N'SalesOrder',N'Process',N'User')");
            migrationBuilder.AddCheckConstraint(name: "CK_WfRequestControls_FieldRole", table: "WfRequestControls", sql: "[FieldRole] IN (N'Dimension',N'Measure',N'Both')");
            migrationBuilder.AddCheckConstraint(name: "CK_WfRequestControls_DataType", table: "WfRequestControls", sql: "[DataType] IN (N'String',N'Integer',N'Decimal',N'Date',N'DateTime',N'Time',N'Boolean')");
            migrationBuilder.AddCheckConstraint(name: "CK_WfRequestControls_DefaultAggregation", table: "WfRequestControls", sql: "[DefaultAggregation] IN (N'NONE',N'SUM',N'COUNT',N'COUNT_DISTINCT',N'AVG',N'MIN',N'MAX')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_WfRequestControls_ReferenceType", table: "WfRequestControls");
            migrationBuilder.DropCheckConstraint(name: "CK_WfRequestControls_FieldRole", table: "WfRequestControls");
            migrationBuilder.DropCheckConstraint(name: "CK_WfRequestControls_DataType", table: "WfRequestControls");
            migrationBuilder.DropCheckConstraint(name: "CK_WfRequestControls_DefaultAggregation", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "CanFilter", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "CanGroup", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "CanSort", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "ReferenceType", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "FieldRole", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "DataType", table: "WfRequestControls");
            migrationBuilder.DropColumn(name: "DefaultAggregation", table: "WfRequestControls");
        }
    }
}
