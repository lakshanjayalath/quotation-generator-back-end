using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quotation_generator_back_end.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing Users table if it exists and recreate with all columns
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [Users];
                END
            ");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TwoFactorAuth = table.Column<bool>(type: "bit", nullable: false),
                    LoginNotification = table.Column<bool>(type: "bit", nullable: false),
                    TaskAssignNotification = table.Column<bool>(type: "bit", nullable: false),
                    DisableRecurringPaymentNotification = table.Column<bool>(type: "bit", nullable: false),
                    AllEvents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceCreated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceSent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuoteCreated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuoteSent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuoteView = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
