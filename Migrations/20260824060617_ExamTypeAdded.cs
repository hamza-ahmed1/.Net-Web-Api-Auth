using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class ExamTypeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "013bc1bd-7704-4ac4-9b2d-8d5a5cd4d411");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "37598927-ab79-44c5-a02f-64f35c16a973");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "682017c3-adbc-483a-b03f-a85c5ac75b70");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a154072b-834e-433e-9abd-63ced36b177a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c9a408a9-c73e-444a-b612-ec6908e45f7c");

            migrationBuilder.CreateTable(
                name: "ExamTypes",
                columns: table => new
                {
                    ExamTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTypes", x => x.ExamTypeId);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "17728553-1455-4147-bed6-725033a3b59d", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "71e341ae-5ce7-4a49-91f8-2d63d01cd263", null, "Teacher", "TEACHER" },
                    { "89510b54-ec4d-4033-9855-875769e5035d", null, "Student", "STUDENT" },
                    { "ab9f9681-48a8-4553-80e2-b338a4082a08", null, "HOD", "HOD" },
                    { "feec76a5-90ab-47c1-8564-96b872c43379", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamTypes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "17728553-1455-4147-bed6-725033a3b59d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "71e341ae-5ce7-4a49-91f8-2d63d01cd263");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "89510b54-ec4d-4033-9855-875769e5035d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ab9f9681-48a8-4553-80e2-b338a4082a08");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "feec76a5-90ab-47c1-8564-96b872c43379");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "013bc1bd-7704-4ac4-9b2d-8d5a5cd4d411", null, "Student", "STUDENT" },
                    { "37598927-ab79-44c5-a02f-64f35c16a973", null, "Teacher", "TEACHER" },
                    { "682017c3-adbc-483a-b03f-a85c5ac75b70", null, "Admin", "ADMIN" },
                    { "a154072b-834e-433e-9abd-63ced36b177a", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "c9a408a9-c73e-444a-b612-ec6908e45f7c", null, "HOD", "HOD" }
                });
        }
    }
}
