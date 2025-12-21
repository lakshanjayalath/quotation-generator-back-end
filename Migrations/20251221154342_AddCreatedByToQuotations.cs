using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quotation_generator_back_end.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToQuotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CreatedById",
                table: "Quotations",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Users_CreatedById",
                table: "Quotations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Users_CreatedById",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_CreatedById",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Quotations");
        }
    }
}
