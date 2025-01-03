using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerrexWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationNumbersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuotationNumber",
                table: "Quotations");

            migrationBuilder.AddColumn<int>(
                name: "QuotationNumberId",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuotationNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationNumberValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationNumbers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_QuotationNumberId",
                table: "Quotations",
                column: "QuotationNumberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_QuotationNumbers_QuotationNumberId",
                table: "Quotations",
                column: "QuotationNumberId",
                principalTable: "QuotationNumbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_QuotationNumbers_QuotationNumberId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "QuotationNumbers");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_QuotationNumberId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "QuotationNumberId",
                table: "Quotations");

            migrationBuilder.AddColumn<string>(
                name: "QuotationNumber",
                table: "Quotations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
