using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class Feeschemachanged2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ApplicableFees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "299a4818-87b7-4fd4-af76-2dba6675144a", null, "Teacher", "TEACHER" },
                    { "9e9a4f6f-864b-440e-ab96-a1d76fc0e4f5", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "a5130439-ad2e-4761-82c7-55e3137eb634", null, "Student", "STUDENT" },
                    { "c76ba92c-2e29-4934-adc1-fd4d2a9f5dc2", null, "Admin", "ADMIN" },
                    { "eb401576-03dc-47f4-8537-f9619c7e092c", null, "HOD", "HOD" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "299a4818-87b7-4fd4-af76-2dba6675144a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9e9a4f6f-864b-440e-ab96-a1d76fc0e4f5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a5130439-ad2e-4761-82c7-55e3137eb634");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c76ba92c-2e29-4934-adc1-fd4d2a9f5dc2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "eb401576-03dc-47f4-8537-f9619c7e092c");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ApplicableFees",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
    }
}
