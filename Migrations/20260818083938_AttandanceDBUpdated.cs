using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class AttandanceDBUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "26abdede-d320-4038-8f5a-94ee446d76ae");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2a755d75-241b-497c-8589-c42fabf5ac03");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "36d87289-20fe-4c4a-8e9d-17361cb885a3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "568d55e3-72a9-4d98-a3cd-e8855d862a81");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c1eb90a2-e0ec-42f3-a569-0940782ab07c");

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    AttendanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherSectionCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarkedByTeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_Attendances_StudentEnrollments_StudentEnrollmentId",
                        column: x => x.StudentEnrollmentId,
                        principalTable: "StudentEnrollments",
                        principalColumn: "StudentEnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_TeacherSectionCourses_TeacherSectionCourseId",
                        column: x => x.TeacherSectionCourseId,
                        principalTable: "TeacherSectionCourses",
                        principalColumn: "TeacherSectionCourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Teachers_MarkedByTeacherId",
                        column: x => x.MarkedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Teacher_Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MarkedByTeacherId",
                table: "Attendances",
                column: "MarkedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentEnrollmentId_TeacherSectionCourseId_AttendanceDate",
                table: "Attendances",
                columns: new[] { "StudentEnrollmentId", "TeacherSectionCourseId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_TeacherSectionCourseId",
                table: "Attendances",
                column: "TeacherSectionCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

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

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "26abdede-d320-4038-8f5a-94ee446d76ae", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "2a755d75-241b-497c-8589-c42fabf5ac03", null, "Admin", "ADMIN" },
                    { "36d87289-20fe-4c4a-8e9d-17361cb885a3", null, "HOD", "HOD" },
                    { "568d55e3-72a9-4d98-a3cd-e8855d862a81", null, "Student", "STUDENT" },
                    { "c1eb90a2-e0ec-42f3-a569-0940782ab07c", null, "Teacher", "TEACHER" }
                });
        }
    }
}
