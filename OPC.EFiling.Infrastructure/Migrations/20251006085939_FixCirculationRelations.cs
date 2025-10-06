using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPC.EFiling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCirculationRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Signatures_DraftingInstructionId",
                table: "Signatures",
                column: "DraftingInstructionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_DraftingInstructions_DraftingInstructionId",
                table: "Signatures",
                column: "DraftingInstructionId",
                principalTable: "DraftingInstructions",
                principalColumn: "DraftingInstructionID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_DraftingInstructions_DraftingInstructionId",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Signatures_DraftingInstructionId",
                table: "Signatures");
        }
    }
}
