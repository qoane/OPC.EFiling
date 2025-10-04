using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OPC.EFiling.Infrastructure.Migrations
{
    public partial class AddCirculationLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create CirculationLogs table to record every time a draft is sent
            migrationBuilder.CreateTable(
                name: "CirculationLogs",
                columns: table => new
                {
                    CirculationLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(nullable: false),
                    VersionLabel = table.Column<string>(maxLength: 32, nullable: true),
                    SentToEmail = table.Column<string>(maxLength: 256, nullable: false),
                    CcEmail = table.Column<string>(maxLength: 256, nullable: true),
                    Subject = table.Column<string>(maxLength: 512, nullable: false),
                    SentAt = table.Column<DateTime>(nullable: false),
                    SentByUserId = table.Column<int>(nullable: true),
                    Notes = table.Column<string>(maxLength: 2000, nullable: true),
                    DocumentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirculationLogs", x => x.CirculationLogId);
                    table.ForeignKey(
                        name: "FK_CirculationLogs_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "DraftID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CirculationLogs_AspNetUsers_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CirculationLogs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_DraftId",
                table: "CirculationLogs",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_SentByUserId",
                table: "CirculationLogs",
                column: "SentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_DocumentId",
                table: "CirculationLogs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_SentAt",
                table: "CirculationLogs",
                column: "SentAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CirculationLogs");
        }
    }
}