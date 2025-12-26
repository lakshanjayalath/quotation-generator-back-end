using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quotation_generator_back_end.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdFormatted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientIdFormatted",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientIdFormatted",
                table: "Clients");
        }
    }
}
