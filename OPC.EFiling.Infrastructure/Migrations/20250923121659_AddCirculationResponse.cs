using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OPC.EFiling.Infrastructure.Migrations
{
    public partial class AddCirculationResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CirculationResponses",
                columns: table => new
                {
                    CirculationResponseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CirculationLogId = table.Column<int>(nullable: false),
                    ResponseText = table.Column<string>(maxLength: 2000, nullable: true),
                    DocumentId = table.Column<int>(nullable: true),
                    ReceivedAt = table.Column<DateTime>(nullable: false),
                    ReceivedByUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirculationResponses", x => x.CirculationResponseId);
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
                    table.ForeignKey(
                        name: "FK_CirculationResponses_AspNetUsers_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_CirculationLogId",
                table: "CirculationResponses",
                column: "CirculationLogId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_DocumentId",
                table: "CirculationResponses",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_ReceivedByUserId",
                table: "CirculationResponses",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CirculationResponses_ReceivedAt",
                table: "CirculationResponses",
                column: "ReceivedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CirculationResponses");
        }
    }
}