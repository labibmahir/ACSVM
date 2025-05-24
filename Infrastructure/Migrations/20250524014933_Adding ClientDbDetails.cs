using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingClientDbDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientDBDetails",
                columns: table => new
                {
                    Oid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseType = table.Column<int>(type: "int", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConnectionMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsConnectionActive = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateModified = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDBDetails", x => x.Oid);
                });

            migrationBuilder.CreateTable(
                name: "ClientFieldMappings",
                columns: table => new
                {
                    Oid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StandardField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormatType = table.Column<int>(type: "int", nullable: true),
                    ClientDBDetailId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateModified = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientFieldMappings", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_ClientFieldMappings_ClientDBDetails_ClientDBDetailId",
                        column: x => x.ClientDBDetailId,
                        principalTable: "ClientDBDetails",
                        principalColumn: "Oid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientFieldMappings_ClientDBDetailId",
                table: "ClientFieldMappings",
                column: "ClientDBDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientFieldMappings");

            migrationBuilder.DropTable(
                name: "ClientDBDetails");
        }
    }
}
