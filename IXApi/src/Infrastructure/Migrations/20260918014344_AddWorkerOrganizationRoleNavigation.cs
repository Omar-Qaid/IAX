using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerOrganizationRoleNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrganizationRoleId",
                table: "HcmWorkerOrganizationAssignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE assignment
                SET OrganizationRoleId = position.RoleId
                FROM dbo.HcmWorkerOrganizationAssignments AS assignment
                INNER JOIN dbo.HcmPositions AS position ON position.RECID = assignment.PositionId
                INNER JOIN dbo.OrganizationRoles AS role ON role.RECID = position.RoleId
                WHERE assignment.OrganizationRoleId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignments_DataAreaId_OrganizationRoleId_ValidFrom_ValidTo",
                table: "HcmWorkerOrganizationAssignments",
                columns: new[] { "DataAreaId", "OrganizationRoleId", "ValidFrom", "ValidTo" });

            migrationBuilder.Sql("""
                UPDATE assignment
                SET OrganizationRoleId = position.RoleId
                FROM dbo.HcmWorkerOrganizationAssignments AS assignment
                INNER JOIN dbo.HcmPositions AS position ON position.RECID = assignment.PositionId
                INNER JOIN dbo.OrganizationRoles AS role ON role.RECID = position.RoleId
                WHERE assignment.OrganizationRoleId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignments_OrganizationRoleId",
                table: "HcmWorkerOrganizationAssignments",
                column: "OrganizationRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_HcmWorkerOrganizationAssignments_OrganizationRoles_OrganizationRoleId",
                table: "HcmWorkerOrganizationAssignments",
                column: "OrganizationRoleId",
                principalTable: "OrganizationRoles",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HcmWorkerOrganizationAssignments_OrganizationRoles_OrganizationRoleId",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropIndex(
                name: "IX_HcmWorkerOrganizationAssignments_DataAreaId_OrganizationRoleId_ValidFrom_ValidTo",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropIndex(
                name: "IX_HcmWorkerOrganizationAssignments_OrganizationRoleId",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "OrganizationRoleId",
                table: "HcmWorkerOrganizationAssignments");
        }
    }
}
