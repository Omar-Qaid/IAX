using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountStructureTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DimensionConstraintTree",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionHierarchy = table.Column<long>(type: "bigint", nullable: false),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActivating = table.Column<int>(type: "int", nullable: false),
                    ActivationBatchId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionConstraintTree", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionConstraintTree_DimensionHierarchy_DimensionHierarchy",
                        column: x => x.DimensionHierarchy,
                        principalTable: "DimensionHierarchy",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionHierarchyLevel",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionAttribute = table.Column<long>(type: "bigint", nullable: false),
                    DimensionHierarchy = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    TemporaryDimensionHierarchyLevel = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DimensionHierarchyLevel", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionHierarchyLevel_DimensionAttribute_DimensionAttribute",
                        column: x => x.DimensionAttribute,
                        principalTable: "DimensionAttribute",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DimensionHierarchyLevel_DimensionHierarchy_DimensionHierarchy",
                        column: x => x.DimensionHierarchy,
                        principalTable: "DimensionHierarchy",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LedgerStructure",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ledger = table.Column<long>(type: "bigint", nullable: false),
                    DimensionHierarchy = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_LedgerStructure", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_LedgerStructure_DimensionHierarchy_DimensionHierarchy",
                        column: x => x.DimensionHierarchy,
                        principalTable: "DimensionHierarchy",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerStructure_Ledger_Ledger",
                        column: x => x.Ledger,
                        principalTable: "Ledger",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionConstraintNode",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionConstraintTree = table.Column<long>(type: "bigint", nullable: false),
                    DimensionHierarchyLevel = table.Column<long>(type: "bigint", nullable: false),
                    ParentConstraintNode = table.Column<long>(type: "bigint", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IsOptional = table.Column<int>(type: "int", nullable: false),
                    IsOverlapping = table.Column<int>(type: "int", nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TemporaryConstraintNode = table.Column<long>(type: "bigint", nullable: false),
                    HasErrorInChildren = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionConstraintNode", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionConstraintNode_DimensionConstraintTree_DimensionConstraintTree",
                        column: x => x.DimensionConstraintTree,
                        principalTable: "DimensionConstraintTree",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DimensionConstraintNode_DimensionHierarchyLevel_DimensionHierarchyLevel",
                        column: x => x.DimensionHierarchyLevel,
                        principalTable: "DimensionHierarchyLevel",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionConstraintNodeCriteria",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionConstraintNode = table.Column<long>(type: "bigint", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    RangeFrom = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RangeTo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsFromOpen = table.Column<int>(type: "int", nullable: false),
                    IsToOpen = table.Column<int>(type: "int", nullable: false),
                    WildCardString = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TemporaryOrdinal = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DimensionConstraintNodeCriteria", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_DimensionConstraintNodeCriteria_DimensionConstraintNode_DimensionConstraintNode",
                        column: x => x.DimensionConstraintNode,
                        principalTable: "DimensionConstraintNode",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintNode_DataAreaId_DimensionConstraintTree_ParentConstraintNode_Ordinal",
                table: "DimensionConstraintNode",
                columns: new[] { "DataAreaId", "DimensionConstraintTree", "ParentConstraintNode", "Ordinal" });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintNode_DimensionConstraintTree",
                table: "DimensionConstraintNode",
                column: "DimensionConstraintTree");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintNode_DimensionHierarchyLevel",
                table: "DimensionConstraintNode",
                column: "DimensionHierarchyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintNodeCriteria_DataAreaId_DimensionConstraintNode_Ordinal",
                table: "DimensionConstraintNodeCriteria",
                columns: new[] { "DataAreaId", "DimensionConstraintNode", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintNodeCriteria_DimensionConstraintNode",
                table: "DimensionConstraintNodeCriteria",
                column: "DimensionConstraintNode");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintTree_DataAreaId_DimensionHierarchy",
                table: "DimensionConstraintTree",
                columns: new[] { "DataAreaId", "DimensionHierarchy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionConstraintTree_DimensionHierarchy",
                table: "DimensionConstraintTree",
                column: "DimensionHierarchy");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionHierarchyLevel_DataAreaId_DimensionHierarchy_DimensionAttribute",
                table: "DimensionHierarchyLevel",
                columns: new[] { "DataAreaId", "DimensionHierarchy", "DimensionAttribute" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionHierarchyLevel_DataAreaId_DimensionHierarchy_Level",
                table: "DimensionHierarchyLevel",
                columns: new[] { "DataAreaId", "DimensionHierarchy", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionHierarchyLevel_DimensionAttribute",
                table: "DimensionHierarchyLevel",
                column: "DimensionAttribute");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionHierarchyLevel_DimensionHierarchy",
                table: "DimensionHierarchyLevel",
                column: "DimensionHierarchy");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerStructure_DimensionHierarchy",
                table: "LedgerStructure",
                column: "DimensionHierarchy");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerStructure_Ledger_DimensionHierarchy",
                table: "LedgerStructure",
                columns: new[] { "Ledger", "DimensionHierarchy" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DimensionConstraintNodeCriteria");

            migrationBuilder.DropTable(
                name: "LedgerStructure");

            migrationBuilder.DropTable(
                name: "DimensionConstraintNode");

            migrationBuilder.DropTable(
                name: "DimensionConstraintTree");

            migrationBuilder.DropTable(
                name: "DimensionHierarchyLevel");
        }
    }
}
