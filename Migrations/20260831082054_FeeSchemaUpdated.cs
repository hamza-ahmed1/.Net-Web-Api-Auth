using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class FeeSchemaUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "25ed5d4a-cc21-4988-ad1b-da936f2e446c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "72c9a394-1a65-49b2-9f6d-e7be62da1582");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a59c447a-998c-4511-baeb-b36b24628c4e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "db62c8f2-036c-49cb-944c-fbb1dc22bc25");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f5fc7f5f-f24d-4a63-8a4d-0ffff34f10bf");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ApplicableFees");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "status",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TransactionHistories",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistories", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_TransactionHistories_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "06f70fab-c89e-422e-87b7-7d9f9f4c3c58", null, "Teacher", "TEACHER" },
                    { "1aab365e-2ba3-4dd3-9544-8f92431eacc5", null, "Admin", "ADMIN" },
                    { "a5ec2ba0-4487-4e92-a58e-b783db21a093", null, "HOD", "HOD" },
                    { "a7383e51-3e60-41e2-8975-0b39c98daa03", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "bac307ec-a046-4643-8d86-66ed35141ba7", null, "Student", "STUDENT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_InvoiceId",
                table: "TransactionHistories",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionHistories");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06f70fab-c89e-422e-87b7-7d9f9f4c3c58");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1aab365e-2ba3-4dd3-9544-8f92431eacc5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a5ec2ba0-4487-4e92-a58e-b783db21a093");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a7383e51-3e60-41e2-8975-0b39c98daa03");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bac307ec-a046-4643-8d86-66ed35141ba7");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByAdminId",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ApplicableFees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicableFeeAfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AmountSnapshot = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_ApplicableFees_ApplicableFeeAfId",
                        column: x => x.ApplicableFeeAfId,
                        principalTable: "ApplicableFees",
                        principalColumn: "AfId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceItemItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedByAdminId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_InvoiceItems_InvoiceItemItemId",
                        column: x => x.InvoiceItemItemId,
                        principalTable: "InvoiceItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "25ed5d4a-cc21-4988-ad1b-da936f2e446c", null, "HOD", "HOD" },
                    { "72c9a394-1a65-49b2-9f6d-e7be62da1582", null, "Teacher", "TEACHER" },
                    { "a59c447a-998c-4511-baeb-b36b24628c4e", null, "Student", "STUDENT" },
                    { "db62c8f2-036c-49cb-944c-fbb1dc22bc25", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "f5fc7f5f-f24d-4a63-8a4d-0ffff34f10bf", null, "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ApplicableFeeAfId",
                table: "InvoiceItems",
                column: "ApplicableFeeAfId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceItemItemId",
                table: "Payments",
                column: "InvoiceItemItemId");
        }
    }
}
