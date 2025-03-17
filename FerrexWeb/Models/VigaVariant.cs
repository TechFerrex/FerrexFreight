using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FerrexWeb.Models
{
    public class VigaVariant
    {
        [Key]
        public int Id { get; set; }

        // Por ejemplo, "6", "8", etc.
        public string Size { get; set; }

        // Peso en libras por pie (ej. 2.5, 3.0, etc.)
        public decimal WeightPerFoot { get; set; }

        // Opcional: si quieres relacionar la variante a un producto específico:
        public string ProductId { get; set; }
    }
}
