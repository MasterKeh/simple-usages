using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SUs.KeepLatest.Cli.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeepLatestItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    ItemUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ItemVer = table.Column<string>(type: "TEXT", nullable: true),
                    ItemLatestVer = table.Column<string>(type: "TEXT", nullable: true),
                    ItemLatestReleasedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeepLatestItems", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeepLatestItems");
        }
    }
}
