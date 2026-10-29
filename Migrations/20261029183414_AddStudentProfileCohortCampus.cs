using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    /// <summary>
    /// Student code, status, cohort and campus. Existing students are back-filled before the new constraints
    /// are added: unique codes from 2001001, cohort by class (XII -> Cohort-2025, XI -> Cohort-2026, the intake
    /// years for the 2026-27 session), Qayyum-Abad Campus, Active status and sample phone numbers where missing.
    /// Seeded teachers move to the Science department. Batch and the free-text campus column are dropped last.
    /// </summary>
    public partial class AddStudentProfileCohortCampus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cohorts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cohorts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_Name",
                table: "Campuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cohorts_Name",
                table: "Cohorts",
                column: "Name",
                unique: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CampusId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CohortId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Nullable while existing rows are given codes; made required below
            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql(@"
INSERT INTO Cohorts (Id, Name, IsActive, CreatedAt) VALUES
    (NEWID(), 'Cohort-2025', 1, SYSUTCDATETIME()),
    (NEWID(), 'Cohort-2026', 1, SYSUTCDATETIME());
INSERT INTO Campuses (Id, Name, IsActive, CreatedAt) VALUES
    (NEWID(), 'Qayyum-Abad Campus', 1, SYSUTCDATETIME());

;WITH numbered AS (
    SELECT StudentId, ROW_NUMBER() OVER (ORDER BY EnrollmentDate, StudentId) AS rn FROM Students
)
UPDATE s SET StudentCode = CAST(2001000 + n.rn AS nvarchar(10))
FROM Students s JOIN numbered n ON n.StudentId = s.StudentId;

UPDATE s SET CohortId = (SELECT Id FROM Cohorts WHERE Name =
    CASE WHEN EXISTS (
            SELECT 1 FROM StudentEnrollments e JOIN Sections sec ON sec.SectionId = e.SectionId
            WHERE e.StudentId = s.StudentId AND e.IsActive = 1 AND sec.IntermediateClass = 'XII')
         OR (s.Batch LIKE '2024%' AND NOT EXISTS (SELECT 1 FROM StudentEnrollments e WHERE e.StudentId = s.StudentId AND e.IsActive = 1))
        THEN 'Cohort-2025' ELSE 'Cohort-2026' END)
FROM Students s;

UPDATE Students SET CampusId = (SELECT Id FROM Campuses WHERE Name = 'Qayyum-Abad Campus');

;WITH missing AS (
    SELECT u.Id, ROW_NUMBER() OVER (ORDER BY u.Email) AS rn
    FROM AspNetUsers u JOIN Students s ON s.UserId = u.Id
    WHERE u.PhoneNumber IS NULL OR LTRIM(RTRIM(u.PhoneNumber)) = ''
)
UPDATE u SET PhoneNumber = '0300-' + CAST(1000000 + m.rn AS nvarchar(7))
FROM AspNetUsers u JOIN missing m ON m.Id = u.Id;

UPDATE t SET DepartmentId = (SELECT DepartmentId FROM Departments WHERE Name = 'Science')
FROM Teachers t JOIN AspNetUsers u ON u.Id = t.UserId
WHERE u.Email IN (
    'ahmed.khan@lms.edu.pk', 'fatima.siddiqui@lms.edu.pk', 'ali.raza@lms.edu.pk', 'ayesha.noor@lms.edu.pk',
    'usman.malik@lms.edu.pk', 'sana.malik@lms.edu.pk', 'bilal.sheikh@lms.edu.pk', 'imran.qureshi@lms.edu.pk',
    'zainab.hassan@lms.edu.pk', 'hamza.tariq@lms.edu.pk')
  AND EXISTS (SELECT 1 FROM Departments WHERE Name = 'Science');
");

            migrationBuilder.AlterColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentCode",
                table: "Students",
                column: "StudentCode",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_StudentCode_Digits",
                table: "Students",
                sql: "LEN([StudentCode]) BETWEEN 1 AND 10 AND [StudentCode] NOT LIKE '%[^0-9]%'");

            migrationBuilder.CreateIndex(
                name: "IX_Students_CampusId",
                table: "Students",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_CohortId",
                table: "Students",
                column: "CohortId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Campuses_CampusId",
                table: "Students",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Cohorts_CohortId",
                table: "Students",
                column: "CohortId",
                principalTable: "Cohorts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Batch",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CampusName",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Batch",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CampusName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE s SET Batch = ISNULL(c.Name, ''), CampusName = ISNULL(p.Name, '')
FROM Students s
LEFT JOIN Cohorts c ON c.Id = s.CohortId
LEFT JOIN Campuses p ON p.Id = s.CampusId;
");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Campuses_CampusId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Cohorts_CohortId",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_StudentCode_Digits",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentCode",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CampusId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CohortId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CohortId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Campuses");

            migrationBuilder.DropTable(
                name: "Cohorts");
        }
    }
}
