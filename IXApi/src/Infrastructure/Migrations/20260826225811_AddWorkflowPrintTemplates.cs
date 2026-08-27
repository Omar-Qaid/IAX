using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowPrintTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfPrintTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<long>(type: "bigint", nullable: false),
                    PageSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfPrintTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_WfPrintTemplates_WfProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "WfProcesses",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfPrintTemplateVersions",
                columns: table => new
                {
                    TemplateVersionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    TemplateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfPrintTemplateVersions", x => x.TemplateVersionId);
                    table.ForeignKey(
                        name: "FK_WfPrintTemplateVersions_WfPrintTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WfPrintTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfRequestPrintVersions",
                columns: table => new
                {
                    RequestPrintVersionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateVersionId = table.Column<long>(type: "bigint", nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelectedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfRequestPrintVersions", x => x.RequestPrintVersionId);
                    table.ForeignKey(
                        name: "FK_WfRequestPrintVersions_WfPrintTemplateVersions_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalTable: "WfPrintTemplateVersions",
                        principalColumn: "TemplateVersionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfRequestPrintVersions_WfPrintTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WfPrintTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfRequestPrintVersions_WfRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "WfRequests",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfPrintTemplates_CurrentVersionId",
                table: "WfPrintTemplates",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_WfPrintTemplates_DataAreaId_ProcessId_Code",
                table: "WfPrintTemplates",
                columns: new[] { "DataAreaId", "ProcessId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfPrintTemplates_DataAreaId_ProcessId_IsDefault",
                table: "WfPrintTemplates",
                columns: new[] { "DataAreaId", "ProcessId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WfPrintTemplates_ProcessId",
                table: "WfPrintTemplates",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WfPrintTemplateVersions_DataAreaId_TemplateId_VersionNo",
                table: "WfPrintTemplateVersions",
                columns: new[] { "DataAreaId", "TemplateId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfPrintTemplateVersions_TemplateId",
                table: "WfPrintTemplateVersions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestPrintVersions_DataAreaId_RequestId_TemplateId",
                table: "WfRequestPrintVersions",
                columns: new[] { "DataAreaId", "RequestId", "TemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestPrintVersions_RequestId",
                table: "WfRequestPrintVersions",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestPrintVersions_TemplateId",
                table: "WfRequestPrintVersions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestPrintVersions_TemplateVersionId",
                table: "WfRequestPrintVersions",
                column: "TemplateVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_WfPrintTemplates_WfPrintTemplateVersions_CurrentVersionId",
                table: "WfPrintTemplates",
                column: "CurrentVersionId",
                principalTable: "WfPrintTemplateVersions",
                principalColumn: "TemplateVersionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WfPrintTemplates_WfPrintTemplateVersions_CurrentVersionId",
                table: "WfPrintTemplates");

            migrationBuilder.DropTable(
                name: "WfRequestPrintVersions");

            migrationBuilder.DropTable(
                name: "WfPrintTemplateVersions");

            migrationBuilder.DropTable(
                name: "WfPrintTemplates");
        }
    }
}
