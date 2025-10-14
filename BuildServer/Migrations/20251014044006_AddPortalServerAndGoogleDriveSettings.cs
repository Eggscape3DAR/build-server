using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildServer.Migrations
{
    public partial class AddPortalServerAndGoogleDriveSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleDriveCredentialsJson",
                table: "GlobalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GoogleDriveFolderId",
                table: "GlobalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortalServerSecret",
                table: "GlobalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortalServerUrl",
                table: "GlobalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleDriveCredentialsJson",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "GoogleDriveFolderId",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "PortalServerSecret",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "PortalServerUrl",
                table: "GlobalSettings");
        }
    }
}
