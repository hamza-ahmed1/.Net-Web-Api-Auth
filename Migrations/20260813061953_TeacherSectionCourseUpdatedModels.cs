using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class TeacherSectionCourseUpdatedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseDuration = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntermediateClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionId);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSectionCourses",
                columns: table => new
                {
                    TeacherSectionCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSectionCourses", x => x.TeacherSectionCourseId);
                    table.ForeignKey(
                        name: "FK_TeacherSectionCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherSectionCourses_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherSectionCourses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Teacher_Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSectionCourses_CourseId",
                table: "TeacherSectionCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSectionCourses_SectionId",
                table: "TeacherSectionCourses",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSectionCourses_TeacherId_SectionId_CourseId",
                table: "TeacherSectionCourses",
                columns: new[] { "TeacherId", "SectionId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherSectionCourses");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Sections");

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
    }
}
