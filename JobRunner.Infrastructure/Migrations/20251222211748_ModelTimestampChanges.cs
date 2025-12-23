using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobRunner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModelTimestampChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "JobRuns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "JobAttempts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "JobRuns");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "JobAttempts");
        }
    }
}
