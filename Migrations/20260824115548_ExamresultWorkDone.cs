using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class ExamresultWorkDone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResult_Exams_ExamId",
                table: "ExamResult");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResult_Students_StudentId",
                table: "ExamResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamResult",
                table: "ExamResult");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11a3ab30-256e-4e3d-856b-8280571c4bb4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "909a15e5-9f49-4c65-a8d3-1ae6c8bdfc38");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd33ab72-3de7-4a7d-9b74-b11c830643eb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd7e5740-36fc-4837-933a-5a3742938a32");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c4f39b1c-b2b6-4ca5-876d-1ca3bb4c69e3");

            migrationBuilder.RenameTable(
                name: "ExamResult",
                newName: "ExamResults");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResult_StudentId",
                table: "ExamResults",
                newName: "IX_ExamResults_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResult_ExamId_StudentId",
                table: "ExamResults",
                newName: "IX_ExamResults_ExamId_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamResults",
                table: "ExamResults",
                column: "ExamResultId");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4cba2d3d-ca1e-44e0-b579-a4749a005efe", null, "Teacher", "TEACHER" },
                    { "6340f5fb-898f-4812-a3cf-6657490677bf", null, "HOD", "HOD" },
                    { "7f16d18a-21ad-4f38-b821-c4e317f06869", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "c655921b-a722-45eb-87d7-4cf16dcf16db", null, "Admin", "ADMIN" },
                    { "fc11c108-cde5-4d2b-b770-dbfaada21416", null, "Student", "STUDENT" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Exams_ExamId",
                table: "ExamResults",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Students_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Exams_ExamId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Students_StudentId",
                table: "ExamResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamResults",
                table: "ExamResults");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4cba2d3d-ca1e-44e0-b579-a4749a005efe");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6340f5fb-898f-4812-a3cf-6657490677bf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7f16d18a-21ad-4f38-b821-c4e317f06869");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c655921b-a722-45eb-87d7-4cf16dcf16db");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fc11c108-cde5-4d2b-b770-dbfaada21416");

            migrationBuilder.RenameTable(
                name: "ExamResults",
                newName: "ExamResult");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResult",
                newName: "IX_ExamResult_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResults_ExamId_StudentId",
                table: "ExamResult",
                newName: "IX_ExamResult_ExamId_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamResult",
                table: "ExamResult",
                column: "ExamResultId");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "11a3ab30-256e-4e3d-856b-8280571c4bb4", null, "HOD", "HOD" },
                    { "909a15e5-9f49-4c65-a8d3-1ae6c8bdfc38", null, "Student", "STUDENT" },
                    { "bd33ab72-3de7-4a7d-9b74-b11c830643eb", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "bd7e5740-36fc-4837-933a-5a3742938a32", null, "Admin", "ADMIN" },
                    { "c4f39b1c-b2b6-4ca5-876d-1ca3bb4c69e3", null, "Teacher", "TEACHER" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResult_Exams_ExamId",
                table: "ExamResult",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResult_Students_StudentId",
                table: "ExamResult",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
