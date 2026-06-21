using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLogistics.Shipment.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrackingFromShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerTrackings");

            migrationBuilder.DropTable(
                name: "ShipmentStatusHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerTrackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerNo = table.Column<string>(type: "text", nullable: false),
                    CurrentPort = table.Column<string>(type: "text", nullable: false),
                    Eta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Etd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SealNo = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStatusHistories", x => x.Id);
                });
        }
    }
}
