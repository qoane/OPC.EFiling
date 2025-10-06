using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OPC.EFiling.Infrastructure.Migrations
{
    /// <summary>
    /// Migration adding the Signatures table to store captured approval
    /// signatures for drafting instructions. Each signature record stores a
    /// reference to the instruction, the signer's name, a base64 encoded image
    /// and the timestamp of the signature.
    /// </summary>
    public partial class AddSignature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Signatures",
                columns: table => new
                {
                    SignatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftingInstructionId = table.Column<int>(type: "int", nullable: false),
                    SignerName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ImageData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.SignatureId);
                    table.ForeignKey(
                        name: "FK_Signatures_DraftingInstructions_DraftingInstructionId",
                        column: x => x.DraftingInstructionId,
                        principalTable: "DraftingInstructions",
                        principalColumn: "DraftingInstructionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_DraftingInstructionId",
                table: "Signatures",
                column: "DraftingInstructionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Signatures");
        }
    }
}