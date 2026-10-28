using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    /// <summary>
    /// Replaces the free-text Teachers.Department with a Departments table. Existing names become departments
    /// and teachers are linked by name before the old column is dropped; Pre-Engineering and Pre-Medical
    /// students are placed in "Science".
    /// </summary>
    public partial class AddDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Teachers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            // Move existing data before the text column is dropped
            migrationBuilder.Sql(@"
INSERT INTO Departments (DepartmentId, Name, Description, IsActive, CreatedAt)
SELECT NEWID(), names.Name, NULL, 1, SYSUTCDATETIME()
FROM (SELECT DISTINCT LTRIM(RTRIM(Department)) AS Name FROM Teachers WHERE LTRIM(RTRIM(Department)) <> '') AS names;

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Science')
    INSERT INTO Departments (DepartmentId, Name, Description, IsActive, CreatedAt)
    VALUES (NEWID(), 'Science', NULL, 1, SYSUTCDATETIME());

UPDATE t SET t.DepartmentId = d.DepartmentId
FROM Teachers t
JOIN Departments d ON d.Name = LTRIM(RTRIM(t.Department));

UPDATE s SET s.DepartmentId = (SELECT DepartmentId FROM Departments WHERE Name = 'Science')
FROM Students s
WHERE EXISTS (
    SELECT 1 FROM StudentEnrollments e
    JOIN Sections sec ON sec.SectionId = e.SectionId
    WHERE e.StudentId = s.StudentId
      AND (sec.SectionName LIKE '%Pre-Engineering%' OR sec.SectionName LIKE '%Pre-Medical%'));
");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_DepartmentId",
                table: "Teachers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_DepartmentId",
                table: "Students",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Departments_DepartmentId",
                table: "Teachers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Teachers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Teachers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE t SET t.Department = d.Name
FROM Teachers t
JOIN Departments d ON d.DepartmentId = t.DepartmentId;
");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Departments_DepartmentId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_DepartmentId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_DepartmentId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
