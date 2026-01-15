using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fahrplanauskunft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Footpaths",
                columns: table => new
                {
                    FromStopId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToStopId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    WheelchairAccessible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DistanceMeters = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footpaths", x => new { x.FromStopId, x.ToStopId });
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LongName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    TextColor = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    PlatformCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WheelchairAccessible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StopTimes",
                columns: table => new
                {
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    TripId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ArrivalMinutes = table.Column<int>(type: "integer", nullable: false),
                    DepartureMinutes = table.Column<int>(type: "integer", nullable: false),
                    PickupAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DropOffAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StopId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopTimes", x => new { x.TripId, x.Sequence });
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Headsign = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RouteId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Footpaths_FromStopId",
                table: "Footpaths",
                column: "FromStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Footpaths_ToStopId",
                table: "Footpaths",
                column: "ToStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Footpaths_ToStopId_FromStopId",
                table: "Footpaths",
                columns: new[] { "ToStopId", "FromStopId" });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Mode",
                table: "Routes",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ShortName",
                table: "Routes",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_Name",
                table: "Stops",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_Arrival",
                table: "StopTimes",
                column: "ArrivalMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_Departure",
                table: "StopTimes",
                column: "DepartureMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_StopId",
                table: "StopTimes",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_StopId_Arrival",
                table: "StopTimes",
                columns: new[] { "StopId", "ArrivalMinutes" });

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_StopId_Departure",
                table: "StopTimes",
                columns: new[] { "StopId", "DepartureMinutes" });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_Headsign",
                table: "Trips",
                column: "Headsign");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_RouteId",
                table: "Trips",
                column: "RouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Footpaths");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Stops");

            migrationBuilder.DropTable(
                name: "StopTimes");

            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
