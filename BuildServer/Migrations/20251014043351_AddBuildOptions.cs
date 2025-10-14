using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildServer.Migrations
{
    public partial class AddBuildOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuildType",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BundleCode",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UploadToChannel",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UploadToGoogleDrive",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BundleCode",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UploadToChannel",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UploadToGoogleDrive",
                table: "Jobs");
        }
    }
}
