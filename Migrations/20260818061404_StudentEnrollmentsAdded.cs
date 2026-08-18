using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class StudentEnrollmentsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "172ba36b-d0e7-4fa8-936f-ef87e23bf359");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1ed38c40-e93e-41b1-a069-6c63c4673965");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3f5141c1-8476-4089-9ef4-73d7e2b8bfd4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b59be87d-aef8-41f6-9771-1a3fd80214b2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b86e7b45-2bcd-4813-808e-ea61e974459e");

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentEnrollments",
                columns: table => new
                {
                    StudentEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherSectionCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WithdrawnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentEnrollments", x => x.StudentEnrollmentId);
                    table.ForeignKey(
                        name: "FK_StudentEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentEnrollments_TeacherSectionCourses_TeacherSectionCourseId",
                        column: x => x.TeacherSectionCourseId,
                        principalTable: "TeacherSectionCourses",
                        principalColumn: "TeacherSectionCourseId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_StudentId_TeacherSectionCourseId",
                table: "StudentEnrollments",
                columns: new[] { "StudentId", "TeacherSectionCourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_TeacherSectionCourseId",
                table: "StudentEnrollments",
                column: "TeacherSectionCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentEnrollments");

            migrationBuilder.DropTable(
                name: "Students");

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

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "172ba36b-d0e7-4fa8-936f-ef87e23bf359", null, "Teacher", "TEACHER" },
                    { "1ed38c40-e93e-41b1-a069-6c63c4673965", null, "Student", "STUDENT" },
                    { "3f5141c1-8476-4089-9ef4-73d7e2b8bfd4", null, "HOD", "HOD" },
                    { "b59be87d-aef8-41f6-9771-1a3fd80214b2", null, "Admin", "ADMIN" },
                    { "b86e7b45-2bcd-4813-808e-ea61e974459e", null, "CourseCoordinator", "COURSECOORDINATOR" }
                });
        }
    }
}
