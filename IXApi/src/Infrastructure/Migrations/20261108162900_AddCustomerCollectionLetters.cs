using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261108162900_AddCustomerCollectionLetters")]
public sealed class AddCustomerCollectionLetters : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CustCollectionLetterTable",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CollectionLetterCourse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                PaymTermId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustCollectionLetterTable", x => x.RECID));
        migrationBuilder.CreateIndex(
            name: "IX_CustCollectionLetterTable_DataAreaId_CollectionLetterCourse",
            table: "CustCollectionLetterTable",
            columns: new[] { "DataAreaId", "CollectionLetterCourse" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "CustCollectionLetterLine",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CollectionLetterCourse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CollectionLetterCode = table.Column<int>(type: "int", nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                NumOfDays = table.Column<int>(type: "int", nullable: false),
                MinAmount = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                Fee = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                Block = table.Column<int>(type: "int", nullable: false),
                Txt = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustCollectionLetterLine", x => x.RECID));
        migrationBuilder.CreateIndex(
            name: "IX_CustCollectionLetterLine_DataAreaId_CollectionLetterCourse_CurrencyCode_CollectionLetterCode",
            table: "CustCollectionLetterLine",
            columns: new[] { "DataAreaId", "CollectionLetterCourse", "CurrencyCode", "CollectionLetterCode" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "CustCollectionLetterJour",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CollectionLetterNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CollectionLetterCode = table.Column<int>(type: "int", nullable: false),
                CollectionLetterDate = table.Column<DateTime>(type: "date", nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Status = table.Column<int>(type: "int", nullable: true),
                Updated = table.Column<DateTime>(type: "date", nullable: true),
                Canceled = table.Column<DateTime>(type: "date", nullable: true),
                Fee = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                TaxAmount = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                LedgerVoucher = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                PrintDate = table.Column<DateTime>(type: "date", nullable: true),
                PaymTermId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                DueDate = table.Column<DateTime>(type: "date", nullable: true),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustCollectionLetterJour", x => x.RECID));
        migrationBuilder.CreateIndex(
            name: "IX_CustCollectionLetterJour_DataAreaId_CollectionLetterNum",
            table: "CustCollectionLetterJour",
            columns: new[] { "DataAreaId", "CollectionLetterNum" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "CustCollectionLetterTrans",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CollectionLetterNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CollectionLetterCode = table.Column<int>(type: "int", nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                CustTransId = table.Column<long>(type: "bigint", nullable: false),
                CustTransOpenRecId = table.Column<long>(type: "bigint", nullable: false),
                DueDate = table.Column<DateTime>(type: "date", nullable: false),
                TransDate = table.Column<DateTime>(type: "date", nullable: false),
                Invoice = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Voucher = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                RemainAmount = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                CalculationAmount = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                Fee = table.Column<decimal>(type: "decimal(28, 8)", nullable: false),
                CollectionLetterIssued = table.Column<int>(type: "int", nullable: false),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CustCollectionLetterTrans", x => x.RECID));
        migrationBuilder.CreateIndex(
            name: "IX_CustCollectionLetterTrans_DataAreaId_CollectionLetterNum_CustTransId",
            table: "CustCollectionLetterTrans",
            columns: new[] { "DataAreaId", "CollectionLetterNum", "CustTransId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CustCollectionLetterTrans");
        migrationBuilder.DropTable(name: "CustCollectionLetterJour");
        migrationBuilder.DropTable(name: "CustCollectionLetterLine");
        migrationBuilder.DropTable(name: "CustCollectionLetterTable");
    }
}
