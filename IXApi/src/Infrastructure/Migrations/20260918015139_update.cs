using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "HcmWorkerOrganizationAssignments",
                newName: "RECID");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HcmWorkerOrganizationAssignments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "HcmWorkerOrganizationAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HcmWorkerOrganizationAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "HcmWorkerOrganizationAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "HcmWorkerOrganizationAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerAccountId",
                table: "HcmWorkerOrganizationAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecVersion",
                table: "HcmWorkerOrganizationAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "HcmWorkerOrganizationAssignments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "OwnerAccountId",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "RecVersion",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "HcmWorkerOrganizationAssignments");

            migrationBuilder.RenameColumn(
                name: "RECID",
                table: "HcmWorkerOrganizationAssignments",
                newName: "AssignmentId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "HcmWorkerOrganizationAssignments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
