using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPC.EFiling.Infrastructure.Migrations
{
    public partial class RemoveRoleIDFromAspNetRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Draft_DraftingInstructions_DraftingInstructionID",
                table: "Draft");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructionAttachment_DraftingInstructions_DraftingInstructionID",
                table: "InstructionAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstructionAttachment",
                table: "InstructionAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Draft",
                table: "Draft");

            migrationBuilder.DropColumn(
                name: "RoleID",
                table: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "InstructionAttachment",
                newName: "InstructionAttachments");

            migrationBuilder.RenameTable(
                name: "Draft",
                newName: "Drafts");

            migrationBuilder.RenameIndex(
                name: "IX_InstructionAttachment_DraftingInstructionID",
                table: "InstructionAttachments",
                newName: "IX_InstructionAttachments_DraftingInstructionID");

            migrationBuilder.RenameIndex(
                name: "IX_Draft_DraftingInstructionID",
                table: "Drafts",
                newName: "IX_Drafts_DraftingInstructionID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstructionAttachments",
                table: "InstructionAttachments",
                column: "InstructionAttachmentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drafts",
                table: "Drafts",
                column: "DraftID");

            migrationBuilder.AddForeignKey(
                name: "FK_Drafts_DraftingInstructions_DraftingInstructionID",
                table: "Drafts",
                column: "DraftingInstructionID",
                principalTable: "DraftingInstructions",
                principalColumn: "DraftingInstructionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructionAttachments_DraftingInstructions_DraftingInstructionID",
                table: "InstructionAttachments",
                column: "DraftingInstructionID",
                principalTable: "DraftingInstructions",
                principalColumn: "DraftingInstructionID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drafts_DraftingInstructions_DraftingInstructionID",
                table: "Drafts");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructionAttachments_DraftingInstructions_DraftingInstructionID",
                table: "InstructionAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstructionAttachments",
                table: "InstructionAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drafts",
                table: "Drafts");

            migrationBuilder.RenameTable(
                name: "InstructionAttachments",
                newName: "InstructionAttachment");

            migrationBuilder.RenameTable(
                name: "Drafts",
                newName: "Draft");

            migrationBuilder.RenameIndex(
                name: "IX_InstructionAttachments_DraftingInstructionID",
                table: "InstructionAttachment",
                newName: "IX_InstructionAttachment_DraftingInstructionID");

            migrationBuilder.RenameIndex(
                name: "IX_Drafts_DraftingInstructionID",
                table: "Draft",
                newName: "IX_Draft_DraftingInstructionID");

            migrationBuilder.AddColumn<int>(
                name: "RoleID",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstructionAttachment",
                table: "InstructionAttachment",
                column: "InstructionAttachmentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Draft",
                table: "Draft",
                column: "DraftID");

            migrationBuilder.AddForeignKey(
                name: "FK_Draft_DraftingInstructions_DraftingInstructionID",
                table: "Draft",
                column: "DraftingInstructionID",
                principalTable: "DraftingInstructions",
                principalColumn: "DraftingInstructionID");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructionAttachment_DraftingInstructions_DraftingInstructionID",
                table: "InstructionAttachment",
                column: "DraftingInstructionID",
                principalTable: "DraftingInstructions",
                principalColumn: "DraftingInstructionID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
