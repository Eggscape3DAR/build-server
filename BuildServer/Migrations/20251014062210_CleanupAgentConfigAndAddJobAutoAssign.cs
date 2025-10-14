using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildServer.Migrations
{
    public partial class CleanupAgentConfigAndAddJobAutoAssign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArtifactsPath",
                table: "AgentConfigurations");

            migrationBuilder.DropColumn(
                name: "GitToken",
                table: "AgentConfigurations");

            migrationBuilder.DropColumn(
                name: "GitUsername",
                table: "AgentConfigurations");

            migrationBuilder.DropColumn(
                name: "RepositoryUrl",
                table: "AgentConfigurations");

            migrationBuilder.DropColumn(
                name: "UnityProjectPath",
                table: "AgentConfigurations");

            migrationBuilder.RenameColumn(
                name: "WorkspacePath",
                table: "AgentConfigurations",
                newName: "BuildOutputFolder");

            migrationBuilder.AddColumn<bool>(
                name: "AutoAssign",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "AgentConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoAssign",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "AgentConfigurations");

            migrationBuilder.RenameColumn(
                name: "BuildOutputFolder",
                table: "AgentConfigurations",
                newName: "WorkspacePath");

            migrationBuilder.AddColumn<string>(
                name: "ArtifactsPath",
                table: "AgentConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitToken",
                table: "AgentConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitUsername",
                table: "AgentConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RepositoryUrl",
                table: "AgentConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnityProjectPath",
                table: "AgentConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
