using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gemelo.Components.Cts.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CtsUsers",
                columns: table => new
                {
                    CtsUserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Rfid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DetailsAsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CtsUsers", x => x.CtsUserID);
                });

            migrationBuilder.CreateTable(
                name: "StationConfigurations",
                columns: table => new
                {
                    StationConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailsAsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationConfigurations", x => x.StationConfigurationID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CtsUsers");

            migrationBuilder.DropTable(
                name: "StationConfigurations");
        }
    }
}
