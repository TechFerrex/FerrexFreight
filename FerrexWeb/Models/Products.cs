using System.Collections.Generic;
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

        [Column("Types")]
        public string? Types { get; set; }

        [Column("Size")]
        public string? Size { get; set; }

        [Column("Precio")]
        public decimal Precio { get; set; }

        [Column("CategoriaID")]
        public int CategoriaID { get; set; } 

        [Column("Unit")]
        public string Unit { get; set; }

        [Column("id_subcategory")]
        public int id_subcategory { get; set; }

        [Column("id_subcategory2")]
        public int? id_subcategory2 { get; set; }

        [Column("id_image")]
        public int? id_image { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        [ForeignKey("id_image")]
        public Image Image { get; set; }

        private List<Products> productsInSameSubcat = new();
    
        [NotMapped]
        public string ImageUrl
        {
            get
            {
                return Image != null && !string.IsNullOrEmpty(Image.url)
                    ? Image.url
                    : "images/product/default.png";
            }
        }

    }
}