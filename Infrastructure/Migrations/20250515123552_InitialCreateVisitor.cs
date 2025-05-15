using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateVisitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FingerPrints_Persons_PersonId",
                table: "FingerPrints");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentifiedAssignCards_Persons_PersonId",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentifiedAssignDevices_Persons_PersonId",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleImages_Persons_Oid",
                table: "PeopleImages");

            migrationBuilder.DropColumn(
                name: "PersonType",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IdentifiedAssignCards");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "PeopleImages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VisitorId",
                table: "PeopleImages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "IdentifiedAssignDevices",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "VisitorId",
                table: "IdentifiedAssignDevices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "IdentifiedAssignCards",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "VisitorId",
                table: "IdentifiedAssignCards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "FingerPrints",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "VisitorId",
                table: "FingerPrints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "Cards",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    Oid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<TimeSpan>(type: "time(7)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(7)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateModified = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.Oid);
                });

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateModified = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.Oid);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    VisitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateModified = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_Appointments_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentifiedAssignedAppointments",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateModified = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifiedAssignedAppointments", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_IdentifiedAssignedAppointments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdentifiedAssignedAppointments_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeopleImages_PersonId",
                table: "PeopleImages",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleImages_VisitorId",
                table: "PeopleImages",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifiedAssignDevices_VisitorId",
                table: "IdentifiedAssignDevices",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifiedAssignCards_VisitorId",
                table: "IdentifiedAssignCards",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_FingerPrints_VisitorId",
                table: "FingerPrints",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SlotId",
                table: "Appointments",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VisitorId",
                table: "Appointments",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifiedAssignedAppointments_AppointmentId",
                table: "IdentifiedAssignedAppointments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifiedAssignedAppointments_PersonId",
                table: "IdentifiedAssignedAppointments",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_FingerPrints_Persons_PersonId",
                table: "FingerPrints",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_FingerPrints_Visitors_VisitorId",
                table: "FingerPrints",
                column: "VisitorId",
                principalTable: "Visitors",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifiedAssignCards_Persons_PersonId",
                table: "IdentifiedAssignCards",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifiedAssignCards_Visitors_VisitorId",
                table: "IdentifiedAssignCards",
                column: "VisitorId",
                principalTable: "Visitors",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifiedAssignDevices_Persons_PersonId",
                table: "IdentifiedAssignDevices",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifiedAssignDevices_Visitors_VisitorId",
                table: "IdentifiedAssignDevices",
                column: "VisitorId",
                principalTable: "Visitors",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleImages_Persons_PersonId",
                table: "PeopleImages",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleImages_Visitors_VisitorId",
                table: "PeopleImages",
                column: "VisitorId",
                principalTable: "Visitors",
                principalColumn: "Oid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FingerPrints_Persons_PersonId",
                table: "FingerPrints");

            migrationBuilder.DropForeignKey(
                name: "FK_FingerPrints_Visitors_VisitorId",
                table: "FingerPrints");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentifiedAssignCards_Persons_PersonId",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentifiedAssignCards_Visitors_VisitorId",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentifiedAssignDevices_Persons_PersonId",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentifiedAssignDevices_Visitors_VisitorId",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleImages_Persons_PersonId",
                table: "PeopleImages");

            migrationBuilder.DropForeignKey(
                name: "FK_PeopleImages_Visitors_VisitorId",
                table: "PeopleImages");

            migrationBuilder.DropTable(
                name: "IdentifiedAssignedAppointments");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropIndex(
                name: "IX_PeopleImages_PersonId",
                table: "PeopleImages");

            migrationBuilder.DropIndex(
                name: "IX_PeopleImages_VisitorId",
                table: "PeopleImages");

            migrationBuilder.DropIndex(
                name: "IX_IdentifiedAssignDevices_VisitorId",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropIndex(
                name: "IX_IdentifiedAssignCards_VisitorId",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropIndex(
                name: "IX_FingerPrints_VisitorId",
                table: "FingerPrints");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "PeopleImages");

            migrationBuilder.DropColumn(
                name: "VisitorId",
                table: "PeopleImages");

            migrationBuilder.DropColumn(
                name: "VisitorId",
                table: "IdentifiedAssignDevices");

            migrationBuilder.DropColumn(
                name: "VisitorId",
                table: "IdentifiedAssignCards");

            migrationBuilder.DropColumn(
                name: "VisitorId",
                table: "FingerPrints");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cards");

            migrationBuilder.AddColumn<byte>(
                name: "PersonType",
                table: "Persons",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "IdentifiedAssignDevices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "IdentifiedAssignCards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "IdentifiedAssignCards",
                type: "smalldatetime",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "IdentifiedAssignCards",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "FingerPrints",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FingerPrints_Persons_PersonId",
                table: "FingerPrints",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifiedAssignCards_Persons_PersonId",
                table: "IdentifiedAssignCards",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifiedAssignDevices_Persons_PersonId",
                table: "IdentifiedAssignDevices",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Oid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeopleImages_Persons_Oid",
                table: "PeopleImages",
                column: "Oid",
                principalTable: "Persons",
                principalColumn: "Oid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
