using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class StudentTableUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5b4ca498-36c7-48af-ac81-e9c121901414");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5b94f4da-ab3b-4f66-a481-1d4dde414898");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6e351b83-3df3-4383-b0ac-e9bc2be33198");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7d34a400-5bb6-4946-af9e-6527f1726864");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d4c55d3b-a516-46a5-a27e-f4f8f83774b4");

            migrationBuilder.AddColumn<string>(
                name: "CNIC",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "053481f8-b93d-429c-b243-81cdd78a8547", null, "Teacher", "TEACHER" },
                    { "48ebecdf-99d4-44b3-b825-be2a4ad8b63e", null, "HOD", "HOD" },
                    { "524e8d4c-6c7a-4fd6-a993-1c8cf68a4d30", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "7a22e9fa-854b-443f-b176-ac0d3ef1289e", null, "Student", "STUDENT" },
                    { "dd4702e8-07d0-410e-901b-e3c7642d1a09", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "053481f8-b93d-429c-b243-81cdd78a8547");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48ebecdf-99d4-44b3-b825-be2a4ad8b63e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "524e8d4c-6c7a-4fd6-a993-1c8cf68a4d30");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7a22e9fa-854b-443f-b176-ac0d3ef1289e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dd4702e8-07d0-410e-901b-e3c7642d1a09");

            migrationBuilder.DropColumn(
                name: "CNIC",
                table: "Students");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5b4ca498-36c7-48af-ac81-e9c121901414", null, "Admin", "ADMIN" },
                    { "5b94f4da-ab3b-4f66-a481-1d4dde414898", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "6e351b83-3df3-4383-b0ac-e9bc2be33198", null, "Teacher", "TEACHER" },
                    { "7d34a400-5bb6-4946-af9e-6527f1726864", null, "HOD", "HOD" },
                    { "d4c55d3b-a516-46a5-a27e-f4f8f83774b4", null, "Student", "STUDENT" }
                });
        }
    }
}
