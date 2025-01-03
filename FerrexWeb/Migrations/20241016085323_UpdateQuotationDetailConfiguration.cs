using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerrexWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuotationDetailConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuotationDetails",
                table: "QuotationDetails");

            migrationBuilder.DropIndex(
                name: "IX_QuotationDetails_QuotationId",
                table: "QuotationDetails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "QuotationDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuotationDetails",
                table: "QuotationDetails",
                columns: new[] { "QuotationId", "Line" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuotationDetails",
                table: "QuotationDetails");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "QuotationDetails",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuotationDetails",
                table: "QuotationDetails",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationDetails_QuotationId",
                table: "QuotationDetails",
                column: "QuotationId");
        }
    }
}
