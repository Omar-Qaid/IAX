using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHcmWorkerOrganizationAssignmentV1AndShowroomAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HcmWorker_Occupations_OccupationId",
                table: "HcmWorker");

            migrationBuilder.DropForeignKey(
                name: "FK_HcmWorker_OrgNationalities_NationalityId",
                table: "HcmWorker");

            migrationBuilder.DropForeignKey(
                name: "FK_WfUsersProcesses_Occupations_OccupationId",
                table: "WfUsersProcesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Occupations",
                table: "Occupations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgNationalities",
                table: "OrgNationalities");

            migrationBuilder.RenameTable(
                name: "Occupations",
                newName: "HcmOccupations");

            migrationBuilder.RenameTable(
                name: "OrgNationalities",
                newName: "HcmNationalities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HcmOccupations",
                table: "HcmOccupations",
                column: "RECID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HcmNationalities",
                table: "HcmNationalities",
                column: "RECID");

            migrationBuilder.CreateTable(
                name: "HcmDepartments",
                columns: table => new
                {
                    RECID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameAlias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HcmDepartments", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "HcmShowroom",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Party = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_HcmShowroom", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_HcmShowroom_DirPartyTable_Party",
                        column: x => x.Party,
                        principalTable: "DirPartyTable",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HcmWorkerOrganizationAssignmentsV1",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HcmWorkerId = table.Column<long>(type: "bigint", nullable: false),
                    HcmManagerWorkerId = table.Column<long>(type: "bigint", nullable: false),
                    DepartmentId = table.Column<short>(type: "smallint", nullable: true),
                    OccupationId = table.Column<short>(type: "smallint", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "SYSUTCDATETIME()"),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HcmWorkerOrganizationAssignmentsV1", x => x.RECID);
                    table.CheckConstraint("CK_HcmWorkerOrganizationAssignmentsV1_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]");
                    table.ForeignKey(
                        name: "FK_HcmWorkerOrganizationAssignmentsV1_HcmDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "HcmDepartments",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorkerOrganizationAssignmentsV1_HcmOccupations_OccupationId",
                        column: x => x.OccupationId,
                        principalTable: "HcmOccupations",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorkerOrganizationAssignmentsV1_HcmWorker_HcmManagerWorkerId",
                        column: x => x.HcmManagerWorkerId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorkerOrganizationAssignmentsV1_HcmWorker_HcmWorkerId",
                        column: x => x.HcmWorkerId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HcmWorkerShowroomAssignments",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HcmWorkerId = table.Column<long>(type: "bigint", nullable: false),
                    HcmShowroomId = table.Column<long>(type: "bigint", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "SYSUTCDATETIME()"),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HcmWorkerShowroomAssignments", x => x.RECID);
                    table.CheckConstraint("CK_HcmWorkerShowroomAssignments_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]");
                    table.ForeignKey(
                        name: "FK_HcmWorkerShowroomAssignments_HcmShowroom_HcmShowroomId",
                        column: x => x.HcmShowroomId,
                        principalTable: "HcmShowroom",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HcmWorkerShowroomAssignments_HcmWorker_HcmWorkerId",
                        column: x => x.HcmWorkerId,
                        principalTable: "HcmWorker",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HcmShowroom_Party",
                table: "HcmShowroom",
                column: "Party");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_DataAreaId_DepartmentId_ValidFrom_ValidTo",
                table: "HcmWorkerOrganizationAssignmentsV1",
                columns: new[] { "DataAreaId", "DepartmentId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_DataAreaId_HcmManagerWorkerId_ValidFrom_ValidTo",
                table: "HcmWorkerOrganizationAssignmentsV1",
                columns: new[] { "DataAreaId", "HcmManagerWorkerId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_DataAreaId_HcmWorkerId_IsPrimary_ValidFrom_ValidTo",
                table: "HcmWorkerOrganizationAssignmentsV1",
                columns: new[] { "DataAreaId", "HcmWorkerId", "IsPrimary", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_DataAreaId_OccupationId_ValidFrom_ValidTo",
                table: "HcmWorkerOrganizationAssignmentsV1",
                columns: new[] { "DataAreaId", "OccupationId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_DepartmentId",
                table: "HcmWorkerOrganizationAssignmentsV1",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_HcmManagerWorkerId",
                table: "HcmWorkerOrganizationAssignmentsV1",
                column: "HcmManagerWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_HcmWorkerId",
                table: "HcmWorkerOrganizationAssignmentsV1",
                column: "HcmWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerOrganizationAssignmentsV1_OccupationId",
                table: "HcmWorkerOrganizationAssignmentsV1",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerShowroomAssignments_DataAreaId_HcmWorkerId_IsPrimary_ValidFrom_ValidTo",
                table: "HcmWorkerShowroomAssignments",
                columns: new[] { "DataAreaId", "HcmWorkerId", "IsPrimary", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerShowroomAssignments_HcmShowroomId_ValidFrom_ValidTo",
                table: "HcmWorkerShowroomAssignments",
                columns: new[] { "HcmShowroomId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_HcmWorkerShowroomAssignments_HcmWorkerId_ValidFrom_ValidTo",
                table: "HcmWorkerShowroomAssignments",
                columns: new[] { "HcmWorkerId", "ValidFrom", "ValidTo" });

            migrationBuilder.AddForeignKey(
                name: "FK_HcmWorker_HcmNationalities_NationalityId",
                table: "HcmWorker",
                column: "NationalityId",
                principalTable: "HcmNationalities",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HcmWorker_HcmOccupations_OccupationId",
                table: "HcmWorker",
                column: "OccupationId",
                principalTable: "HcmOccupations",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfUsersProcesses_HcmOccupations_OccupationId",
                table: "WfUsersProcesses",
                column: "OccupationId",
                principalTable: "HcmOccupations",
                principalColumn: "RECID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HcmWorker_HcmNationalities_NationalityId",
                table: "HcmWorker");

            migrationBuilder.DropForeignKey(
                name: "FK_HcmWorker_HcmOccupations_OccupationId",
                table: "HcmWorker");

            migrationBuilder.DropForeignKey(
                name: "FK_WfUsersProcesses_HcmOccupations_OccupationId",
                table: "WfUsersProcesses");

            migrationBuilder.DropTable(
                name: "HcmWorkerOrganizationAssignmentsV1");

            migrationBuilder.DropTable(
                name: "HcmWorkerShowroomAssignments");

            migrationBuilder.DropTable(
                name: "HcmDepartments");

            migrationBuilder.DropTable(
                name: "HcmShowroom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HcmOccupations",
                table: "HcmOccupations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HcmNationalities",
                table: "HcmNationalities");

            migrationBuilder.RenameTable(
                name: "HcmOccupations",
                newName: "Occupations");

            migrationBuilder.RenameTable(
                name: "HcmNationalities",
                newName: "OrgNationalities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Occupations",
                table: "Occupations",
                column: "RECID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgNationalities",
                table: "OrgNationalities",
                column: "RECID");

            migrationBuilder.AddForeignKey(
                name: "FK_HcmWorker_Occupations_OccupationId",
                table: "HcmWorker",
                column: "OccupationId",
                principalTable: "Occupations",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HcmWorker_OrgNationalities_NationalityId",
                table: "HcmWorker",
                column: "NationalityId",
                principalTable: "OrgNationalities",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfUsersProcesses_Occupations_OccupationId",
                table: "WfUsersProcesses",
                column: "OccupationId",
                principalTable: "Occupations",
                principalColumn: "RECID");
        }
    }
}
