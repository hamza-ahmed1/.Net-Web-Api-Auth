using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class Feeschemachanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "269c3e5f-dbf8-474a-9b1e-b4e217803399");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "76a48a6a-f93f-4ac9-85b6-9a9d16d2c458");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dc2e2f31-58de-4a6f-8fd2-ac2d4f692b98");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f252ee12-d722-4356-957b-d78d42efc702");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f374c236-277a-4e8f-9715-51dec90584ee");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Invoices");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5638449b-7598-4cd2-ac08-37087d399dbd", null, "Student", "STUDENT" },
                    { "70c2f42d-f4d8-4f22-a8bd-777bb52de465", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "8f1e615f-8ed1-4899-9e71-666a9e60de64", null, "Admin", "ADMIN" },
                    { "e1bbcb30-e6c0-4fdb-a641-bff6c5390db1", null, "Teacher", "TEACHER" },
                    { "e43c279c-0b01-4c8a-9100-410469bfdf98", null, "HOD", "HOD" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5638449b-7598-4cd2-ac08-37087d399dbd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "70c2f42d-f4d8-4f22-a8bd-777bb52de465");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8f1e615f-8ed1-4899-9e71-666a9e60de64");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e1bbcb30-e6c0-4fdb-a641-bff6c5390db1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e43c279c-0b01-4c8a-9100-410469bfdf98");

            migrationBuilder.AddColumn<bool>(
                name: "status",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "269c3e5f-dbf8-474a-9b1e-b4e217803399", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "76a48a6a-f93f-4ac9-85b6-9a9d16d2c458", null, "Admin", "ADMIN" },
                    { "dc2e2f31-58de-4a6f-8fd2-ac2d4f692b98", null, "Teacher", "TEACHER" },
                    { "f252ee12-d722-4356-957b-d78d42efc702", null, "HOD", "HOD" },
                    { "f374c236-277a-4e8f-9715-51dec90584ee", null, "Student", "STUDENT" }
                });
        }
    }
}
