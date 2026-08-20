using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class ExamAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_TeacherSectionCourses_TeacherSectionCourseId",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResult_Exam_ExamId",
                table: "ExamResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exam",
                table: "Exam");

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

            migrationBuilder.RenameTable(
                name: "Exam",
                newName: "Exams");

            migrationBuilder.RenameIndex(
                name: "IX_Exam_TeacherSectionCourseId",
                table: "Exams",
                newName: "IX_Exams_TeacherSectionCourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exams",
                table: "Exams",
                column: "ExamID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResult_Exams_ExamId",
                table: "ExamResult",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_TeacherSectionCourses_TeacherSectionCourseId",
                table: "Exams",
                column: "TeacherSectionCourseId",
                principalTable: "TeacherSectionCourses",
                principalColumn: "TeacherSectionCourseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResult_Exams_ExamId",
                table: "ExamResult");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_TeacherSectionCourses_TeacherSectionCourseId",
                table: "Exams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exams",
                table: "Exams");

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

            migrationBuilder.RenameTable(
                name: "Exams",
                newName: "Exam");

            migrationBuilder.RenameIndex(
                name: "IX_Exams_TeacherSectionCourseId",
                table: "Exam",
                newName: "IX_Exam_TeacherSectionCourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exam",
                table: "Exam",
                column: "ExamID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_TeacherSectionCourses_TeacherSectionCourseId",
                table: "Exam",
                column: "TeacherSectionCourseId",
                principalTable: "TeacherSectionCourses",
                principalColumn: "TeacherSectionCourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResult_Exam_ExamId",
                table: "ExamResult",
                column: "ExamId",
                principalTable: "Exam",
                principalColumn: "ExamID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
