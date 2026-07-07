using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDateTimeOffsetConversions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DateTimeOffset conversions are handled in memory by HasConversion
            // No schema changes needed for TakenAt and SnoozedUntil columns
            // They already use "timestamp with time zone" which supports timezone-aware values
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No changes to revert
        }
    }
}
