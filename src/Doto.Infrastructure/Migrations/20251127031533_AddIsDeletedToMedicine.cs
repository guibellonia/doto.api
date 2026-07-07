using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToMedicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Medicines",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Medicines");
        }
    }
}
