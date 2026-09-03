using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class GeneralizeReportDocumentTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_WfPrintTemplates_WfPrintTemplateVersions_CurrentVersionId", "WfPrintTemplates");
        migrationBuilder.DropForeignKey("FK_WfPrintTemplateVersions_WfPrintTemplates_TemplateId", "WfPrintTemplateVersions");
        migrationBuilder.DropForeignKey("FK_WfRequestPrintVersions_WfPrintTemplateVersions_TemplateVersionId", "WfRequestPrintVersions");
        migrationBuilder.DropForeignKey("FK_WfRequestPrintVersions_WfPrintTemplates_TemplateId", "WfRequestPrintVersions");
        migrationBuilder.DropForeignKey("FK_WfRequestPrintVersions_WfRequests_RequestId", "WfRequestPrintVersions");
        migrationBuilder.DropForeignKey("FK_WfPrintTemplates_WfProcesses_ProcessId", "WfPrintTemplates");
        migrationBuilder.DropIndex("IX_WfPrintTemplates_DataAreaId_ProcessId_Code", "WfPrintTemplates");
        migrationBuilder.DropIndex("IX_WfPrintTemplates_DataAreaId_ProcessId_IsDefault", "WfPrintTemplates");
        migrationBuilder.DropIndex("IX_WfPrintTemplates_ProcessId", "WfPrintTemplates");
        migrationBuilder.DropIndex("IX_WfRequestPrintVersions_DataAreaId_RequestId_TemplateId", "WfRequestPrintVersions");
        migrationBuilder.DropIndex("IX_WfRequestPrintVersions_RequestId", "WfRequestPrintVersions");

        migrationBuilder.RenameTable("WfPrintTemplates", newName: "ReportTemplates");
        migrationBuilder.RenameTable("WfPrintTemplateVersions", newName: "ReportTemplateVersions");
        migrationBuilder.RenameTable("WfRequestPrintVersions", newName: "ReportEntityVersions");
        migrationBuilder.RenameColumn("ProcessId", "ReportTemplates", "RefRecId");
        migrationBuilder.RenameColumn("RequestId", "ReportEntityVersions", "RefRecId");
        migrationBuilder.RenameColumn("RequestPrintVersionId", "ReportEntityVersions", "ReportEntityVersionId");
        migrationBuilder.AddColumn<int>("RefTableId", "ReportTemplates", "int", nullable: false, defaultValue: 476793887);
        migrationBuilder.AddColumn<int>("RefTableId", "ReportEntityVersions", "int", nullable: false, defaultValue: 1001);

        migrationBuilder.Sql("EXEC sp_rename N'[PK_WfPrintTemplates]', N'PK_ReportTemplates', N'OBJECT';");
        migrationBuilder.Sql("EXEC sp_rename N'[PK_WfPrintTemplateVersions]', N'PK_ReportTemplateVersions', N'OBJECT';");
        migrationBuilder.Sql("EXEC sp_rename N'[PK_WfRequestPrintVersions]', N'PK_ReportEntityVersions', N'OBJECT';");
        migrationBuilder.RenameIndex("IX_WfPrintTemplates_CurrentVersionId", "IX_ReportTemplates_CurrentVersionId", table: "ReportTemplates");
        migrationBuilder.RenameIndex("IX_WfPrintTemplateVersions_DataAreaId_TemplateId_VersionNo", "IX_ReportTemplateVersions_DataAreaId_TemplateId_VersionNo", table: "ReportTemplateVersions");
        migrationBuilder.RenameIndex("IX_WfPrintTemplateVersions_TemplateId", "IX_ReportTemplateVersions_TemplateId", table: "ReportTemplateVersions");
        migrationBuilder.RenameIndex("IX_WfRequestPrintVersions_TemplateId", "IX_ReportEntityVersions_TemplateId", table: "ReportEntityVersions");
        migrationBuilder.RenameIndex("IX_WfRequestPrintVersions_TemplateVersionId", "IX_ReportEntityVersions_TemplateVersionId", table: "ReportEntityVersions");

        migrationBuilder.CreateIndex("IX_ReportTemplates_DataAreaId_RefTableId_RefRecId_Code", "ReportTemplates", new[] { "DataAreaId", "RefTableId", "RefRecId", "Code" }, unique: true);
        migrationBuilder.CreateIndex("IX_ReportTemplates_DataAreaId_RefTableId_RefRecId_IsDefault", "ReportTemplates", new[] { "DataAreaId", "RefTableId", "RefRecId", "IsDefault" }, unique: true, filter: "[IsDefault] = 1 AND [IsDeleted] = 0 AND [IsActive] = 1");
        migrationBuilder.CreateIndex("IX_ReportEntityVersions_DataAreaId_RefTableId_RefRecId_TemplateId", "ReportEntityVersions", new[] { "DataAreaId", "RefTableId", "RefRecId", "TemplateId" }, unique: true);
        migrationBuilder.AddForeignKey("FK_ReportTemplateVersions_ReportTemplates_TemplateId", "ReportTemplateVersions", "TemplateId", "ReportTemplates", principalColumn: "TemplateId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ReportEntityVersions_ReportTemplateVersions_TemplateVersionId", "ReportEntityVersions", "TemplateVersionId", "ReportTemplateVersions", principalColumn: "TemplateVersionId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ReportEntityVersions_ReportTemplates_TemplateId", "ReportEntityVersions", "TemplateId", "ReportTemplates", principalColumn: "TemplateId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ReportTemplates_ReportTemplateVersions_CurrentVersionId", "ReportTemplates", "CurrentVersionId", "ReportTemplateVersions", principalColumn: "TemplateVersionId", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_ReportTemplates_ReportTemplateVersions_CurrentVersionId", "ReportTemplates");
        migrationBuilder.DropForeignKey("FK_ReportTemplateVersions_ReportTemplates_TemplateId", "ReportTemplateVersions");
        migrationBuilder.DropForeignKey("FK_ReportEntityVersions_ReportTemplateVersions_TemplateVersionId", "ReportEntityVersions");
        migrationBuilder.DropForeignKey("FK_ReportEntityVersions_ReportTemplates_TemplateId", "ReportEntityVersions");
        migrationBuilder.DropIndex("IX_ReportTemplates_DataAreaId_RefTableId_RefRecId_Code", "ReportTemplates");
        migrationBuilder.DropIndex("IX_ReportTemplates_DataAreaId_RefTableId_RefRecId_IsDefault", "ReportTemplates");
        migrationBuilder.DropIndex("IX_ReportEntityVersions_DataAreaId_RefTableId_RefRecId_TemplateId", "ReportEntityVersions");
        migrationBuilder.DropColumn("RefTableId", "ReportTemplates");
        migrationBuilder.DropColumn("RefTableId", "ReportEntityVersions");
        migrationBuilder.RenameColumn("RefRecId", "ReportTemplates", "ProcessId");
        migrationBuilder.RenameColumn("RefRecId", "ReportEntityVersions", "RequestId");
        migrationBuilder.RenameColumn("ReportEntityVersionId", "ReportEntityVersions", "RequestPrintVersionId");
        migrationBuilder.RenameIndex("IX_ReportTemplates_CurrentVersionId", "IX_WfPrintTemplates_CurrentVersionId", table: "ReportTemplates");
        migrationBuilder.RenameIndex("IX_ReportTemplateVersions_DataAreaId_TemplateId_VersionNo", "IX_WfPrintTemplateVersions_DataAreaId_TemplateId_VersionNo", table: "ReportTemplateVersions");
        migrationBuilder.RenameIndex("IX_ReportTemplateVersions_TemplateId", "IX_WfPrintTemplateVersions_TemplateId", table: "ReportTemplateVersions");
        migrationBuilder.RenameIndex("IX_ReportEntityVersions_TemplateId", "IX_WfRequestPrintVersions_TemplateId", table: "ReportEntityVersions");
        migrationBuilder.RenameIndex("IX_ReportEntityVersions_TemplateVersionId", "IX_WfRequestPrintVersions_TemplateVersionId", table: "ReportEntityVersions");
        migrationBuilder.RenameTable("ReportTemplates", newName: "WfPrintTemplates");
        migrationBuilder.RenameTable("ReportTemplateVersions", newName: "WfPrintTemplateVersions");
        migrationBuilder.RenameTable("ReportEntityVersions", newName: "WfRequestPrintVersions");

        migrationBuilder.Sql("EXEC sp_rename N'[PK_ReportTemplates]', N'PK_WfPrintTemplates', N'OBJECT';");
        migrationBuilder.Sql("EXEC sp_rename N'[PK_ReportTemplateVersions]', N'PK_WfPrintTemplateVersions', N'OBJECT';");
        migrationBuilder.Sql("EXEC sp_rename N'[PK_ReportEntityVersions]', N'PK_WfRequestPrintVersions', N'OBJECT';");
        migrationBuilder.CreateIndex("IX_WfPrintTemplates_DataAreaId_ProcessId_Code", "WfPrintTemplates", new[] { "DataAreaId", "ProcessId", "Code" }, unique: true);
        migrationBuilder.CreateIndex("IX_WfPrintTemplates_DataAreaId_ProcessId_IsDefault", "WfPrintTemplates", new[] { "DataAreaId", "ProcessId", "IsDefault" }, unique: true, filter: "[IsDefault] = 1 AND [IsDeleted] = 0 AND [IsActive] = 1");
        migrationBuilder.CreateIndex("IX_WfPrintTemplates_ProcessId", "WfPrintTemplates", "ProcessId");
        migrationBuilder.CreateIndex("IX_WfRequestPrintVersions_DataAreaId_RequestId_TemplateId", "WfRequestPrintVersions", new[] { "DataAreaId", "RequestId", "TemplateId" }, unique: true);
        migrationBuilder.CreateIndex("IX_WfRequestPrintVersions_RequestId", "WfRequestPrintVersions", "RequestId");
        migrationBuilder.AddForeignKey("FK_WfPrintTemplateVersions_WfPrintTemplates_TemplateId", "WfPrintTemplateVersions", "TemplateId", "WfPrintTemplates", principalColumn: "TemplateId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_WfRequestPrintVersions_WfPrintTemplateVersions_TemplateVersionId", "WfRequestPrintVersions", "TemplateVersionId", "WfPrintTemplateVersions", principalColumn: "TemplateVersionId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_WfRequestPrintVersions_WfPrintTemplates_TemplateId", "WfRequestPrintVersions", "TemplateId", "WfPrintTemplates", principalColumn: "TemplateId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_WfRequestPrintVersions_WfRequests_RequestId", "WfRequestPrintVersions", "RequestId", "WfRequests", principalColumn: "RECID", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_WfPrintTemplates_WfProcesses_ProcessId", "WfPrintTemplates", "ProcessId", "WfProcesses", principalColumn: "RECID", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_WfPrintTemplates_WfPrintTemplateVersions_CurrentVersionId", "WfPrintTemplates", "CurrentVersionId", "WfPrintTemplateVersions", principalColumn: "TemplateVersionId", onDelete: ReferentialAction.Restrict);
    }
}
