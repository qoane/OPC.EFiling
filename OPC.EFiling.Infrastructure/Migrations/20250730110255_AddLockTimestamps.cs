using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPC.EFiling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLockTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftVersions",
                columns: table => new
                {
                    DraftVersionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftingInstructionID = table.Column<int>(type: "int", nullable: false),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VersionNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftVersions", x => x.DraftVersionID);
                    table.ForeignKey(
                        name: "FK_DraftVersions_DraftingInstructions_DraftingInstructionID",
                        column: x => x.DraftingInstructionID,
                        principalTable: "DraftingInstructions",
                        principalColumn: "DraftingInstructionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstructionLocks",
                columns: table => new
                {
                    InstructionLockID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftingInstructionID = table.Column<int>(type: "int", nullable: false),
                    LockedByUserID = table.Column<int>(type: "int", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructionLocks", x => x.InstructionLockID);
                    table.ForeignKey(
                        name: "FK_InstructionLocks_DraftingInstructions_DraftingInstructionID",
                        column: x => x.DraftingInstructionID,
                        principalTable: "DraftingInstructions",
                        principalColumn: "DraftingInstructionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftVersions_DraftingInstructionID",
                table: "DraftVersions",
                column: "DraftingInstructionID");

            migrationBuilder.CreateIndex(
                name: "IX_InstructionLocks_DraftingInstructionID",
                table: "InstructionLocks",
                column: "DraftingInstructionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftVersions");

            migrationBuilder.DropTable(
                name: "InstructionLocks");
        }
    }
}
