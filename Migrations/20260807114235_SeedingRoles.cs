using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class SeedingRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2cb74073-4e7c-4bcf-b471-331659105123", null, "Student", "STUDENT" },
                    { "65bd0c9d-b400-448a-b6b0-db2570ac8c20", null, "Admin", "ADMIN" },
                    { "79e3e49c-aaa7-4902-959f-e3225027eba2", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "ec2cb1f5-9a09-440f-8aa6-301e33ddae6b", null, "HOD", "HOD" },
                    { "f0b31bb8-e286-4e7e-986d-8517344626d2", null, "Teacher", "TEACHER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2cb74073-4e7c-4bcf-b471-331659105123");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "65bd0c9d-b400-448a-b6b0-db2570ac8c20");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "79e3e49c-aaa7-4902-959f-e3225027eba2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ec2cb1f5-9a09-440f-8aa6-301e33ddae6b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0b31bb8-e286-4e7e-986d-8517344626d2");
        }
    }
}
