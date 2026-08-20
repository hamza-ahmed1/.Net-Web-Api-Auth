using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class ExamTablesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1235745d-12ab-42d9-bef9-3a9b4ef5c9e0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "792cd491-bfc1-4cb6-b7bb-2c2492b7ddeb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd1f72f9-bd7e-4bc9-8534-5106e139c05e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d2c817f3-ce65-4919-ade5-7316f488cdd7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e0797b13-092f-486a-9295-29c0045bb7fa");

            migrationBuilder.CreateTable(
                name: "Exam",
                columns: table => new
                {
                    ExamID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalMarks = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeacherSectionCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam", x => x.ExamID);
                    table.ForeignKey(
                        name: "FK_Exam_TeacherSectionCourses_TeacherSectionCourseId",
                        column: x => x.TeacherSectionCourseId,
                        principalTable: "TeacherSectionCourses",
                        principalColumn: "TeacherSectionCourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResult",
                columns: table => new
                {
                    ExamResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObtainMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsAbsent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResult", x => x.ExamResultId);
                    table.ForeignKey(
                        name: "FK_ExamResult_Exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exam",
                        principalColumn: "ExamID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResult_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "11365bab-9d6e-4c55-80fc-e421eeb19026", null, "HOD", "HOD" },
                    { "880519df-a364-42f2-b0bb-9d7099b844c2", null, "Admin", "ADMIN" },
                    { "d51d456c-979e-45bb-bbf6-45c296c47254", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "f172cc4f-9941-4886-963c-b0bfe7b71a18", null, "Student", "STUDENT" },
                    { "fea55e56-c74b-4dfa-8d6b-fa4dda551fdd", null, "Teacher", "TEACHER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exam_TeacherSectionCourseId",
                table: "Exam",
                column: "TeacherSectionCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResult_ExamId_StudentId",
                table: "ExamResult",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResult_StudentId",
                table: "ExamResult",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamResult");

            migrationBuilder.DropTable(
                name: "Exam");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11365bab-9d6e-4c55-80fc-e421eeb19026");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "880519df-a364-42f2-b0bb-9d7099b844c2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d51d456c-979e-45bb-bbf6-45c296c47254");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f172cc4f-9941-4886-963c-b0bfe7b71a18");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fea55e56-c74b-4dfa-8d6b-fa4dda551fdd");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1235745d-12ab-42d9-bef9-3a9b4ef5c9e0", null, "HOD", "HOD" },
                    { "792cd491-bfc1-4cb6-b7bb-2c2492b7ddeb", null, "Teacher", "TEACHER" },
                    { "bd1f72f9-bd7e-4bc9-8534-5106e139c05e", null, "Student", "STUDENT" },
                    { "d2c817f3-ce65-4919-ade5-7316f488cdd7", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "e0797b13-092f-486a-9295-29c0045bb7fa", null, "Admin", "ADMIN" }
                });
        }
    }
}
