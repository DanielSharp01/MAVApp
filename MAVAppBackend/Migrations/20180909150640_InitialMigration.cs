using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVAppBackend.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    NormName = table.Column<string>(maxLength: 256, nullable: false),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Number = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Type = table.Column<string>(maxLength: 256, nullable: true),
                    EncodedPolyline = table.Column<string>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainInstances",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ElviraId = table.Column<string>(maxLength: 16, nullable: true),
                    TrainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainInstances_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainStations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TrainId = table.Column<int>(nullable: false),
                    Ordinal = table.Column<int>(nullable: false),
                    StationId = table.Column<int>(nullable: false),
                    Arrival = table.Column<TimeSpan>(nullable: true),
                    Departure = table.Column<TimeSpan>(nullable: true),
                    RelativeDistance = table.Column<double>(nullable: true),
                    Platform = table.Column<string>(maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainStations_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainStations_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trace",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TrainInstanceId = table.Column<int>(nullable: false),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    DelayMinutes = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trace_TrainInstances_TrainInstanceId",
                        column: x => x.TrainInstanceId,
                        principalTable: "TrainInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainInstanceStations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TrainInstanceId = table.Column<int>(nullable: false),
                    TrainStationId = table.Column<int>(nullable: false),
                    ActualArrival = table.Column<TimeSpan>(nullable: true),
                    ActualDeparture = table.Column<TimeSpan>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainInstanceStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainInstanceStations_TrainInstances_TrainInstanceId",
                        column: x => x.TrainInstanceId,
                        principalTable: "TrainInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainInstanceStations_TrainStations_TrainStationId",
                        column: x => x.TrainStationId,
                        principalTable: "TrainStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trace_TrainInstanceId",
                table: "Trace",
                column: "TrainInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainInstances_TrainId",
                table: "TrainInstances",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainInstanceStations_TrainInstanceId",
                table: "TrainInstanceStations",
                column: "TrainInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainInstanceStations_TrainStationId",
                table: "TrainInstanceStations",
                column: "TrainStationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainStations_StationId",
                table: "TrainStations",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainStations_TrainId",
                table: "TrainStations",
                column: "TrainId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trace");

            migrationBuilder.DropTable(
                name: "TrainInstanceStations");

            migrationBuilder.DropTable(
                name: "TrainInstances");

            migrationBuilder.DropTable(
                name: "TrainStations");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Trains");
        }
    }
}
