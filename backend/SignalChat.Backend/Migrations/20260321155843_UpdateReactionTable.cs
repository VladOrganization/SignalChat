using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignalChat.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reactions",
                table: "Reactions",
                newName: "Emoji");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Emoji",
                table: "Reactions",
                newName: "Reactions");
        }
    }
}
