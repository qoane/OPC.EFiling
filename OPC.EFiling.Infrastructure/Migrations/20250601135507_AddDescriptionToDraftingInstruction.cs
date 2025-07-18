using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPC.EFiling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToDraftingInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DraftingInstructions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Draft",
                columns: table => new
                {
                    DraftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    DraftingInstructionID = table.Column<int>(type: "int", nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Draft", x => x.DraftID);
                    table.ForeignKey(
                        name: "FK_Draft_DraftingInstructions_DraftingInstructionID",
                        column: x => x.DraftingInstructionID,
                        principalTable: "DraftingInstructions",
                        principalColumn: "DraftingInstructionID");
                });

            migrationBuilder.CreateTable(
                name: "InstructionAttachment",
                columns: table => new
                {
                    InstructionAttachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftingInstructionID = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructionAttachment", x => x.InstructionAttachmentID);
                    table.ForeignKey(
                        name: "FK_InstructionAttachment_DraftingInstructions_DraftingInstructionID",
                        column: x => x.DraftingInstructionID,
                        principalTable: "DraftingInstructions",
                        principalColumn: "DraftingInstructionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Draft_DraftingInstructionID",
                table: "Draft",
                column: "DraftingInstructionID");

            migrationBuilder.CreateIndex(
                name: "IX_InstructionAttachment_DraftingInstructionID",
                table: "InstructionAttachment",
                column: "DraftingInstructionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Draft");

            migrationBuilder.DropTable(
                name: "InstructionAttachment");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DraftingInstructions");
        }
    }
}
