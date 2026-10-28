using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Announcements_SenderId",
                table: "Announcements");





            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Announcements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "Announcements",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockedBy",
                table: "Announcements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "Announcements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAt",
                table: "Announcements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipientCount",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "Announcements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedAt",
                table: "AnnouncementRecipients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAttempts",
                table: "AnnouncementRecipients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryStatus",
                table: "AnnouncementRecipients",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptAt",
                table: "AnnouncementRecipients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "AnnouncementRecipients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_SectionId",
                table: "Announcements",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_SenderId_CreatedAt",
                table: "Announcements",
                columns: new[] { "SenderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Status_NextAttemptAt",
                table: "Announcements",
                columns: new[] { "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipients_AnnouncementId_DeliveryStatus",
                table: "AnnouncementRecipients",
                columns: new[] { "AnnouncementId", "DeliveryStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Sections_SectionId",
                table: "Announcements",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Sections_SectionId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_SectionId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_SenderId_CreatedAt",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Status_NextAttemptAt",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_AnnouncementRecipients_AnnouncementId_DeliveryStatus",
                table: "AnnouncementRecipients");





            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "NextAttemptAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "RecipientCount",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "AcknowledgedAt",
                table: "AnnouncementRecipients");

            migrationBuilder.DropColumn(
                name: "DeliveryAttempts",
                table: "AnnouncementRecipients");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "AnnouncementRecipients");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "AnnouncementRecipients");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "AnnouncementRecipients");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_SenderId",
                table: "Announcements",
                column: "SenderId");
        }
    }
}
