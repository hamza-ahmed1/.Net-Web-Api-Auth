using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class Feeschemachanged1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "061be4e3-96ca-4f31-9fbc-1862d8d30a03", null, "Admin", "ADMIN" },
                    { "0839f28a-62ed-42b0-b372-ec182c1cd647", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "0bb0298b-5f71-4134-bd0d-7de8aaf223f8", null, "Student", "STUDENT" },
                    { "8b37da2a-b519-485b-bcfd-47b0f2bd2a28", null, "Teacher", "TEACHER" },
                    { "e1234570-5100-4063-8ade-8220f51c4346", null, "HOD", "HOD" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "061be4e3-96ca-4f31-9fbc-1862d8d30a03");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0839f28a-62ed-42b0-b372-ec182c1cd647");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0bb0298b-5f71-4134-bd0d-7de8aaf223f8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8b37da2a-b519-485b-bcfd-47b0f2bd2a28");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e1234570-5100-4063-8ade-8220f51c4346");

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
    }
}
