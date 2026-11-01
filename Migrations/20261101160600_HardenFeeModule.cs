using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class HardenFeeModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionHistories_Invoices_InvoiceId",
                table: "TransactionHistories");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_StudentId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_FeeTypes_FeeCategoryId",
                table: "FeeTypes");

            migrationBuilder.DropIndex(
                name: "IX_ApplicableFees_StudentId",
                table: "ApplicableFees");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ApplicableFees");

            migrationBuilder.CreateSequence(
                name: "InvoiceNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "TransactionHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "TransactionHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TransactionHistories",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordedByUserId",
                table: "TransactionHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversesTransactionId",
                table: "TransactionHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "Invoices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Month",
                table: "Invoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNum",
                table: "Invoices",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Invoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LateFee",
                table: "Invoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invoices",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "Invoices",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidedByUserId",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Per",
                table: "FeeTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FeeTypes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "FeeTypes",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AcademicTerm",
                table: "FeeTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FeeCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "InvoiceAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemoveReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceAdjustments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Every invoice gets its own number, oldest first, and new numbers continue after them
            migrationBuilder.Sql(@"WITH numbered AS (
    SELECT InvoiceNum, CreatedAt, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS n FROM Invoices
)
UPDATE numbered
SET InvoiceNum = 'INV-' + CONVERT(char(8), CreatedAt, 112) + '-' + RIGHT('000000' + CAST(n AS varchar(20)), 6);");

            // Sequence restart
            migrationBuilder.Sql(@"DECLARE @next bigint = (SELECT COUNT(*) FROM Invoices) + 1;
DECLARE @sql nvarchar(200) = N'ALTER SEQUENCE [InvoiceNumbers] RESTART WITH ' + CAST(@next AS nvarchar(20));
EXEC (@sql);");

            // Old mark-unpaid rows all shared one reference; keep them, but make each unique
            migrationBuilder.Sql(@"WITH repeats AS (
    SELECT TransactionReference, TransactionId,
           ROW_NUMBER() OVER (PARTITION BY TransactionReference ORDER BY PaidAt, TransactionId) AS n
    FROM TransactionHistories
    WHERE TransactionReference <> ''
)
UPDATE repeats
SET TransactionReference = LEFT(TransactionReference, 80) + '-' + LEFT(CONVERT(varchar(36), TransactionId), 8)
WHERE n > 1;");

            // Cancel unpaid repeat bills for the same student, fee and period; a paid one is kept first
            migrationBuilder.Sql(@"WITH repeats AS (
    SELECT IsVoided, VoidedAt, VoidReason, AmountPaid,
           ROW_NUMBER() OVER (PARTITION BY StudentId, FeeTypeId, [Month], [Year]
                              ORDER BY CASE WHEN AmountPaid > 0 THEN 0 ELSE 1 END, CreatedAt, Id) AS n
    FROM Invoices
)
UPDATE repeats
SET IsVoided = 1, VoidedAt = SYSUTCDATETIME(), VoidReason = 'Duplicate cancelled by the fee module upgrade.'
WHERE n > 1 AND AmountPaid = 0;");

            // Rename repeated names so the unique rules can be added
            migrationBuilder.Sql(@"WITH repeats AS (
    SELECT Name, ROW_NUMBER() OVER (PARTITION BY FeeCategoryId, Name ORDER BY FeeTypeId) AS n FROM FeeTypes
)
UPDATE repeats SET Name = LEFT(Name, 140) + ' (' + CAST(n AS varchar(10)) + ')' WHERE n > 1;

WITH repeats AS (
    SELECT Name, ROW_NUMBER() OVER (PARTITION BY Name ORDER BY FeeCategoryId) AS n FROM FeeCategories
)
UPDATE repeats SET Name = LEFT(Name, 90) + ' (' + CAST(n AS varchar(10)) + ')' WHERE n > 1;");

            // A repeated fee assignment is an exact copy with nothing linked to it
            migrationBuilder.Sql(@"WITH repeats AS (
    SELECT ROW_NUMBER() OVER (PARTITION BY StudentId, FeeTypeId ORDER BY CreatedAt, AfId) AS n FROM ApplicableFees
)
DELETE FROM repeats WHERE n > 1;");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_ReversesTransactionId",
                table: "TransactionHistories",
                column: "ReversesTransactionId",
                unique: true,
                filter: "[ReversesTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_TransactionReference",
                table: "TransactionHistories",
                column: "TransactionReference",
                unique: true,
                filter: "[TransactionReference] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNum",
                table: "Invoices",
                column: "InvoiceNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StudentId_FeeTypeId_Month_Year",
                table: "Invoices",
                columns: new[] { "StudentId", "FeeTypeId", "Month", "Year" },
                unique: true,
                filter: "[IsVoided] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FeeTypes_FeeCategoryId_Name",
                table: "FeeTypes",
                columns: new[] { "FeeCategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeeCategories_Name",
                table: "FeeCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicableFees_StudentId_FeeTypeId",
                table: "ApplicableFees",
                columns: new[] { "StudentId", "FeeTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceAdjustments_InvoiceId",
                table: "InvoiceAdjustments",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionHistories_Invoices_InvoiceId",
                table: "TransactionHistories",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionHistories_Invoices_InvoiceId",
                table: "TransactionHistories");

            migrationBuilder.DropTable(
                name: "InvoiceAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_TransactionHistories_ReversesTransactionId",
                table: "TransactionHistories");

            migrationBuilder.DropIndex(
                name: "IX_TransactionHistories_TransactionReference",
                table: "TransactionHistories");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceNum",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_StudentId_FeeTypeId_Month_Year",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_FeeTypes_FeeCategoryId_Name",
                table: "FeeTypes");

            migrationBuilder.DropIndex(
                name: "IX_FeeCategories_Name",
                table: "FeeCategories");

            migrationBuilder.DropIndex(
                name: "IX_ApplicableFees_StudentId_FeeTypeId",
                table: "ApplicableFees");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "RecordedByUserId",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "ReversesTransactionId",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LateFee",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VoidedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VoidedByUserId",
                table: "Invoices");

            migrationBuilder.DropSequence(
                name: "InvoiceNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "TransactionHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Month",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNum",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Per",
                table: "FeeTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FeeTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "FeeTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "AcademicTerm",
                table: "FeeTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FeeCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ApplicableFees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StudentId",
                table: "Invoices",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeTypes_FeeCategoryId",
                table: "FeeTypes",
                column: "FeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicableFees_StudentId",
                table: "ApplicableFees",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionHistories_Invoices_InvoiceId",
                table: "TransactionHistories",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
