using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrganisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "UserAccounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Persons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "PeopleImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "IdentifiedAssignDevices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "IdentifiedAssignCards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "FingerPrints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Cards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "AccessLevels",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PeopleImages");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "FingerPrints");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AccessLevels");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "UserAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
