using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class FeeSchemaUpdated2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1dbaf08c-a636-4b50-ad45-be0102d9b203");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5afd286e-6386-4660-9496-88c8f093df06");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "84e0a274-5d70-4261-ba81-52758e3241e0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a6641622-2cbd-444c-b2db-e89a863cb170");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cb83e2e3-d65a-4143-9adc-a7a491f80cca");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ApplicableFees",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "Status",
                table: "ApplicableFees");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1dbaf08c-a636-4b50-ad45-be0102d9b203", null, "Student", "STUDENT" },
                    { "5afd286e-6386-4660-9496-88c8f093df06", null, "HOD", "HOD" },
                    { "84e0a274-5d70-4261-ba81-52758e3241e0", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "a6641622-2cbd-444c-b2db-e89a863cb170", null, "Admin", "ADMIN" },
                    { "cb83e2e3-d65a-4143-9adc-a7a491f80cca", null, "Teacher", "TEACHER" }
                });
        }
    }
}
