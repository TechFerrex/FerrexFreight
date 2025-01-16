using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    [Table("Products")]
    public class Products
    {
        [Key]
        [Column("ID")]
        public int IdProducto { get; set; }

        [Column("Codigo")]
        public string Codigo { get; set; }

        [Column("Product")]
        public string DescProducto { get; set; } 

        [Column("NewProductoType")]
        public string? NewProductoType { get; set; }

        [Column("Types")]
        public string? Types { get; set; }

        [Column("Size")]
        public string? Size { get; set; }

        [Column("Precio")]
        public decimal Precio { get; set; }

        [Column("CategoriaID")]
        public int CategoriaID { get; set; } // Mapeado a la columna 'CategoriaIDzzzzzzzzzzzzzzzzzzzzzzzzzzz'

        [Column("Unit")]
        public string Unit { get; set; }

        [Column("ImageUrl")]
        public string? ImageUrl { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    }
}