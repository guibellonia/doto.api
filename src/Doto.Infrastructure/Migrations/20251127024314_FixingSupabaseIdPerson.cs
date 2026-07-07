using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixingSupabaseIdPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                           name: "SupabaseUserId",
                           table: "Persons",
                           type: "text",
                           nullable: true,
                           oldClrType: typeof(string),
                           oldType: "character varying(50)",
                           oldMaxLength: 50,
                           oldNullable: true
                       );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SupabaseUserId",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );
        }
    }
}
