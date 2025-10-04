using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPC.EFiling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCirculationLogAndResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CirculationLogs",
                columns: table => new
                {
                    CirculationLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    VersionLabel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SentToEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CcEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DocumentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirculationLogs", x => x.CirculationLogId);
                    table.ForeignKey(
                        name: "FK_CirculationLogs_AspNetUsers_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CirculationLogs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CirculationLogs_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "DraftID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CirculationResponses",
                columns: table => new
                {
                    CirculationResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CirculationLogId = table.Column<int>(type: "int", nullable: false),
                    ResponseText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirculationResponses", x => x.CirculationResponseId);
                    table.ForeignKey(
                        name: "FK_CirculationResponses_AspNetUsers_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CirculationResponses_CirculationLogs_CirculationLogId",
                        column: x => x.CirculationLogId,
                        principalTable: "CirculationLogs",
                        principalColumn: "CirculationLogId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CirculationResponses_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_DocumentId",
                table: "CirculationLogs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_DraftId",
                table: "CirculationLogs",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_SentAt",
                table: "CirculationLogs",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationLogs_SentByUserId",
                table: "CirculationLogs",
                column: "SentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_CirculationLogId",
                table: "CirculationResponses",
                column: "CirculationLogId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_DocumentId",
                table: "CirculationResponses",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_ReceivedAt",
                table: "CirculationResponses",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_ReceivedByUserId",
                table: "CirculationResponses",
                column: "ReceivedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CirculationResponses");

            migrationBuilder.DropTable(
                name: "CirculationLogs");
        }
    }
}
