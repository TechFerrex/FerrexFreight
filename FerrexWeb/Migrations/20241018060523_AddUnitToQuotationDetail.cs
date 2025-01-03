using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerrexWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToQuotationDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "DescProducto",
                table: "Products",
                newName: "Product");

            migrationBuilder.RenameColumn(
                name: "IdProducto",
                table: "Products",
                newName: "ID");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "QuotationDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaID",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "QuotationDetails");

            migrationBuilder.DropColumn(
                name: "CategoriaID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Product",
                table: "Products",
                newName: "DescProducto");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Products",
                newName: "IdProducto");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
