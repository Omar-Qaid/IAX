using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicInventoryCategoriesAndProductStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductLifecycleStateId",
                table: "InventTable",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EcoResCategoryHierarchy",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("PK_EcoResCategoryHierarchy", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResProductLifecycleState",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IsDefaultOnProductRelease = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EcoResProductLifecycleState", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResProductCategory",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<long>(type: "bigint", nullable: false),
                    CategoryHierarchy = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EcoResProductCategory", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_EcoResProductCategory_EcoResCategoryHierarchy_CategoryHierarchy",
                        column: x => x.CategoryHierarchy,
                        principalTable: "EcoResCategoryHierarchy",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EcoResProductCategory_EcoResCategory_Category",
                        column: x => x.Category,
                        principalTable: "EcoResCategory",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EcoResProductCategory_EcoResProduct_Product",
                        column: x => x.Product,
                        principalTable: "EcoResProduct",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EcoResCategoryHierarchy_Name",
                table: "EcoResCategoryHierarchy",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProductCategory_Category",
                table: "EcoResProductCategory",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProductCategory_CategoryHierarchy",
                table: "EcoResProductCategory",
                column: "CategoryHierarchy");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProductCategory_Product_Category",
                table: "EcoResProductCategory",
                columns: new[] { "Product", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProductLifecycleState_IsDefaultOnProductRelease",
                table: "EcoResProductLifecycleState",
                column: "IsDefaultOnProductRelease",
                unique: true,
                filter: "[IsDefaultOnProductRelease] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProductLifecycleState_StateId",
                table: "EcoResProductLifecycleState",
                column: "StateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EcoResProductCategory");

            migrationBuilder.DropTable(
                name: "EcoResProductLifecycleState");

            migrationBuilder.DropTable(
                name: "EcoResCategoryHierarchy");

            migrationBuilder.DropColumn(
                name: "ProductLifecycleStateId",
                table: "InventTable");
        }
    }
}
