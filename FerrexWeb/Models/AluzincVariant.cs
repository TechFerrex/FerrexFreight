using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    [Table("AluzincVariants")]
    public class AluzincVariant
    {
        [Key]
        public int Id { get; set; }

        public string ProductId { get; set; }

        // Campos que tenías en tu Excel
        public string Color { get; set; }
        public string Calibre { get; set; }
        public string Milimetros { get; set; }
        public string PerfilCrestas { get; set; }   // p. ej. "8 Crestas", "Romana", etc.

        // Precio por pie para esta combinación
        public decimal PricePerPie { get; set; }
        public int id_image { get; set; }

        // Relación con Products (opcional, si quieres navegar EF)
        //[ForeignKey(nameof(ProductId))]
        //public Products Product { get; set; }
    }
}
