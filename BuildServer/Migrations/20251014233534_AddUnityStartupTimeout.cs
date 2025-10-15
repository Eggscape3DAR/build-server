using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildServer.Migrations
{
    public partial class AddUnityStartupTimeout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnityStartupTimeoutMinutes",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 45);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnityStartupTimeoutMinutes",
                table: "Jobs");
        }
    }
}
