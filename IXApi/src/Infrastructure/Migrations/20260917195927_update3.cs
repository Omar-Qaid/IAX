using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WfRequestControls_ReferenceType",
                table: "WfRequestControls");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "HcmWorker",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "HcmWorker",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "HcmWorker",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "HcmWorker",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_WfRequestControls_ReferenceType",
                table: "WfRequestControls",
                sql: "[ReferenceType] IS NULL OR [ReferenceType] IN (N'Lookup',N'Employee',N'Branch',N'Company',N'Department',N'BusinessUnit',N'Area',N'City',N'Country',N'Location',N'Customer',N'Vendor',N'Item',N'ItemGroup',N'Category',N'Warehouse',N'PaymentMethod',N'ViolationType',N'Invoice',N'PurchaseOrder',N'SalesOrder',N'Process',N'User',N'Showroom')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WfRequestControls_ReferenceType",
                table: "WfRequestControls");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "HcmWorker");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "HcmWorker");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "HcmWorker");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "HcmWorker");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WfRequestControls_ReferenceType",
                table: "WfRequestControls",
                sql: "[ReferenceType] IS NULL OR [ReferenceType] IN (N'Lookup',N'Employee',N'Branch',N'Company',N'Department',N'BusinessUnit',N'Area',N'City',N'Country',N'Location',N'Customer',N'Vendor',N'Item',N'ItemGroup',N'Category',N'Warehouse',N'PaymentMethod',N'ViolationType',N'Invoice',N'PurchaseOrder',N'SalesOrder',N'Process',N'User')");
        }
    }
}
