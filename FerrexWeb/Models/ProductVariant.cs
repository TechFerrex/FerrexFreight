using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public String Size { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } 
        public Products Product { get; set; }

    }
}
