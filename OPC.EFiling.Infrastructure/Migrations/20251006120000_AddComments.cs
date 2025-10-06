using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OPC.EFiling.Infrastructure.Migrations
{
    /// <summary>
    /// Migration that adds a Comments table for threaded comments on drafting instructions. Each comment
    /// stores the author name, text, creation timestamp and supports replies via a self‑referential
    /// foreign key. Comments are linked to drafting instructions by DraftingInstructionID and
    /// persist independently of individual draft versions.
    /// </summary>
    public partial class AddComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftingInstructionID = table.Column<int>(nullable: false),
                    AuthorName = table.Column<string>(maxLength: 200, nullable: false),
                    Text = table.Column<string>(nullable: false),
                    IsResolved = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ParentCommentId = table.Column<int>(nullable: true),
                    Selection = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DraftingInstructionID",
                table: "Comments",
                column: "DraftingInstructionID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Comments");
        }
    }
}