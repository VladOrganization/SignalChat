using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignalChat.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addadialogconfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DialogId",
                table: "Messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Dialogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    DirectKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogParticipants",
                columns: table => new
                {
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogParticipants", x => new { x.DialogId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DialogParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogParticipants_Dialogs_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DialogId_Time",
                table: "Messages",
                columns: new[] { "DialogId", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_DialogParticipants_UserId",
                table: "DialogParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogs_DirectKey",
                table: "Dialogs",
                column: "DirectKey",
                unique: true,
                filter: "\"DirectKey\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Dialogs_DialogId",
                table: "Messages",
                column: "DialogId",
                principalTable: "Dialogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Dialogs_DialogId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "DialogParticipants");

            migrationBuilder.DropTable(
                name: "Dialogs");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DialogId_Time",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DialogId",
                table: "Messages");
        }
    }
}
