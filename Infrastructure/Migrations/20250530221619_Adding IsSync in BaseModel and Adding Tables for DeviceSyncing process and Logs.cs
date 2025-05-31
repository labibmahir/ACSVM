using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingIsSyncinBaseModelandAddingTablesforDeviceSyncingprocessandLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "Visitors",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "UserAccounts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "Persons",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "PeopleImages",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "IdentifiedAssignedAppointments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "IdentifiedAssignDevices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "IdentifiedAssignCards",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "FingerPrints",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "Devices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "ClientFieldMappings",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "ClientDBDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "Cards",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "Attendances",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "Appointments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSync",
                table: "AccessLevels",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "PeopleImages");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "IdentifiedAssignedAppointments");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "FingerPrints");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "ClientFieldMappings");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "ClientDBDetails");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsSync",
                table: "AccessLevels");
        }
    }
}
