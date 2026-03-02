using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerrexWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_UserID",
                table: "Quotations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoriaID",
                table: "Products",
                column: "CategoriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Codigo",
                table: "Products",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_Products_id_subcategory",
                table: "Products",
                column: "id_subcategory");

            migrationBuilder.CreateIndex(
                name: "IX_FreightQuotations_Status",
                table: "FreightQuotations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quotations_UserID",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoriaID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Codigo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_id_subcategory",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FreightQuotations_Status",
                table: "FreightQuotations");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
