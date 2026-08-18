using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class EnrollmenUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_TeacherSectionCourses_TeacherSectionCourseId",
                table: "StudentEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_StudentEnrollments_StudentId_TeacherSectionCourseId",
                table: "StudentEnrollments");

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

            migrationBuilder.RenameColumn(
                name: "TeacherSectionCourseId",
                table: "StudentEnrollments",
                newName: "SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentEnrollments_TeacherSectionCourseId",
                table: "StudentEnrollments",
                newName: "IX_StudentEnrollments_SectionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_StudentId_StudentEnrollmentId",
                table: "StudentEnrollments",
                columns: new[] { "StudentId", "StudentEnrollmentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_Sections_SectionId",
                table: "StudentEnrollments",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_Sections_SectionId",
                table: "StudentEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_StudentEnrollments_StudentId_StudentEnrollmentId",
                table: "StudentEnrollments");

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

            migrationBuilder.RenameColumn(
                name: "SectionId",
                table: "StudentEnrollments",
                newName: "TeacherSectionCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentEnrollments_SectionId",
                table: "StudentEnrollments",
                newName: "IX_StudentEnrollments_TeacherSectionCourseId");

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

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_StudentId_TeacherSectionCourseId",
                table: "StudentEnrollments",
                columns: new[] { "StudentId", "TeacherSectionCourseId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_TeacherSectionCourses_TeacherSectionCourseId",
                table: "StudentEnrollments",
                column: "TeacherSectionCourseId",
                principalTable: "TeacherSectionCourses",
                principalColumn: "TeacherSectionCourseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
