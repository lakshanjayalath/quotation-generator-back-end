using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quotation_generator_back_end.Migrations
{
    /// <inheritdoc />
    public partial class AddClientsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientIdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoutingId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidVat = table.Column<bool>(type: "bit", nullable: false),
                    TaxExempt = table.Column<bool>(type: "bit", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingStreet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingSuite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingPostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingStreet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingSuite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddToInvoices = table.Column<bool>(type: "bit", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientContacts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_ClientId",
                table: "ClientContacts",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientContacts");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
