using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Correcting_Days_Schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleWeekDays",
                table: "ScheduleWeekDays");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleWeekDays_ScheduleId",
                table: "ScheduleWeekDays");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ScheduleWeekDays");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleWeekDays",
                table: "ScheduleWeekDays",
                columns: new[] { "ScheduleId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleWeekDays",
                table: "ScheduleWeekDays");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ScheduleWeekDays",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleWeekDays",
                table: "ScheduleWeekDays",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleWeekDays_ScheduleId",
                table: "ScheduleWeekDays",
                column: "ScheduleId");
        }
    }
}
