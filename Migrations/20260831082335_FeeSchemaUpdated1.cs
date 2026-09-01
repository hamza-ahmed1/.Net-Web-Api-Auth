using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class FeeSchemaUpdated1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicableFees_Students_StudentId1",
                table: "ApplicableFees");

            migrationBuilder.DropIndex(
                name: "IX_ApplicableFees_StudentId1",
                table: "ApplicableFees");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06f70fab-c89e-422e-87b7-7d9f9f4c3c58");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1aab365e-2ba3-4dd3-9544-8f92431eacc5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a5ec2ba0-4487-4e92-a58e-b783db21a093");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a7383e51-3e60-41e2-8975-0b39c98daa03");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bac307ec-a046-4643-8d86-66ed35141ba7");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "ApplicableFees");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "ApplicableFees",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1dbaf08c-a636-4b50-ad45-be0102d9b203", null, "Student", "STUDENT" },
                    { "5afd286e-6386-4660-9496-88c8f093df06", null, "HOD", "HOD" },
                    { "84e0a274-5d70-4261-ba81-52758e3241e0", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "a6641622-2cbd-444c-b2db-e89a863cb170", null, "Admin", "ADMIN" },
                    { "cb83e2e3-d65a-4143-9adc-a7a491f80cca", null, "Teacher", "TEACHER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicableFees_StudentId",
                table: "ApplicableFees",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicableFees_Students_StudentId",
                table: "ApplicableFees",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicableFees_Students_StudentId",
                table: "ApplicableFees");

            migrationBuilder.DropIndex(
                name: "IX_ApplicableFees_StudentId",
                table: "ApplicableFees");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1dbaf08c-a636-4b50-ad45-be0102d9b203");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5afd286e-6386-4660-9496-88c8f093df06");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "84e0a274-5d70-4261-ba81-52758e3241e0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a6641622-2cbd-444c-b2db-e89a863cb170");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cb83e2e3-d65a-4143-9adc-a7a491f80cca");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "ApplicableFees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId1",
                table: "ApplicableFees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "06f70fab-c89e-422e-87b7-7d9f9f4c3c58", null, "Teacher", "TEACHER" },
                    { "1aab365e-2ba3-4dd3-9544-8f92431eacc5", null, "Admin", "ADMIN" },
                    { "a5ec2ba0-4487-4e92-a58e-b783db21a093", null, "HOD", "HOD" },
                    { "a7383e51-3e60-41e2-8975-0b39c98daa03", null, "CourseCoordinator", "COURSECOORDINATOR" },
                    { "bac307ec-a046-4643-8d86-66ed35141ba7", null, "Student", "STUDENT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicableFees_StudentId1",
                table: "ApplicableFees",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicableFees_Students_StudentId1",
                table: "ApplicableFees",
                column: "StudentId1",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
