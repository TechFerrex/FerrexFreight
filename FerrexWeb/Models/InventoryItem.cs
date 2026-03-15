using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryPointId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int Quantity { get; set; } = 0;

        public int MinStock { get; set; } = 5;

        public int? MaxStock { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Notes { get; set; }

        [ForeignKey("InventoryPointId")]
        public InventoryPoint InventoryPoint { get; set; }

        [ForeignKey("ProductId")]
        public Products Product { get; set; }

        public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();

        [NotMapped]
        public bool IsLowStock => Quantity <= MinStock;

        [NotMapped]
        public string StockStatus
        {
            get
            {
                if (Quantity == 0) return "Sin Stock";
                if (Quantity <= MinStock) return "Bajo Stock";
                return "Normal";
            }
        }
    }
}
