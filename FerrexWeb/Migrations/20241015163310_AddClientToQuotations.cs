using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerrexWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddClientToQuotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
           name: "Client",
           table: "Quotations",
           type: "nvarchar(max)",
           nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "Client",
            table: "Quotations");

        }
    }
}
