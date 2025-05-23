using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removingvisitorfromAttendanceandMakingphonenomanditorfromVisitorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Persons_PersonId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Visitors_VisitorId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_VisitorId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "VisitorId",
                table: "Attendances");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Visitors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Attendances",
                type: "uniqueidentifier",
                maxLength: 50,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Persons_PersonId",
                table: "Attendances",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Persons_PersonId",
                table: "Attendances");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Visitors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Attendances",
                type: "uniqueidentifier",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "VisitorId",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_VisitorId",
                table: "Attendances",
                column: "VisitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Persons_PersonId",
                table: "Attendances",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Visitors_VisitorId",
                table: "Attendances",
                column: "VisitorId",
                principalTable: "Visitors",
                principalColumn: "Oid");
        }
    }
}
