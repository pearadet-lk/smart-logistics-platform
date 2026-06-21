using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLogistics.Tracking.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerTrackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerNo = table.Column<string>(type: "text", nullable: false),
                    SealNo = table.Column<string>(type: "text", nullable: false),
                    CurrentPort = table.Column<string>(type: "text", nullable: false),
                    Etd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTrackings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStatusHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTrackings_ShipmentId",
                table: "ContainerTrackings",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistories_ShipmentId",
                table: "ShipmentStatusHistories",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerTrackings");

            migrationBuilder.DropTable(
                name: "ShipmentStatusHistories");
        }
    }
}
