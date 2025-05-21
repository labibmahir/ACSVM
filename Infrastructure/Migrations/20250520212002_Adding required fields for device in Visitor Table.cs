using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingrequiredfieldsfordeviceinVisitorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Visitors",
                newName: "Surname");

            migrationBuilder.AddColumn<byte>(
                name: "Gender",
                table: "Visitors",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "UserVerifyMode",
                table: "Visitors",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidateEndPeriod",
                table: "Visitors",
                type: "smalldatetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidateStartPeriod",
                table: "Visitors",
                type: "smalldatetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "UserVerifyMode",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "ValidateEndPeriod",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "ValidateStartPeriod",
                table: "Visitors");

            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "Visitors",
                newName: "LastName");
        }
    }
}
