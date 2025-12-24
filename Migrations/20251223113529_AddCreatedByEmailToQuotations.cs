using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quotation_generator_back_end.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByEmailToQuotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByEmail",
                table: "Quotations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CreatedByEmail",
                table: "Quotations",
                column: "CreatedByEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quotations_CreatedByEmail",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "CreatedByEmail",
                table: "Quotations");
        }
    }
}
