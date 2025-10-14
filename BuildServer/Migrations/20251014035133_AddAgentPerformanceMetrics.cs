using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildServer.Migrations
{
    public partial class AddAgentPerformanceMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AverageBuildDurationSeconds",
                table: "Agents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastBuildDurationSeconds",
                table: "Agents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalBuildsCompleted",
                table: "Agents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageBuildDurationSeconds",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastBuildDurationSeconds",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "TotalBuildsCompleted",
                table: "Agents");
        }
    }
}
