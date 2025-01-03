using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public int Line { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Propiedades de navegación
        public Order Order { get; set; }
        public Products Product { get; set; }
    }
}
