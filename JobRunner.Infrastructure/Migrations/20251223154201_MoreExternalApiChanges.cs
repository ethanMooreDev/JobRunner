using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobRunner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoreExternalApiChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "ExternalApiSyncResults",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HttpStatusCode",
                table: "ExternalApiSyncResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "ExternalApiSyncResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "ExternalApiSyncResults",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "ExternalApiSyncResults");

            migrationBuilder.DropColumn(
                name: "HttpStatusCode",
                table: "ExternalApiSyncResults");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "ExternalApiSyncResults");

            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "ExternalApiSyncResults");
        }
    }
}
