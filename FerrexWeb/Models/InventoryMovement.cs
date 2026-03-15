using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public enum MovementType
    {
        Entry = 0,
        Exit = 1,
        Adjustment = 2,
        TransferIn = 3,
        TransferOut = 4
    }

    public class InventoryMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryItemId { get; set; }

        public int MovementType { get; set; }

        public int Quantity { get; set; }

        public int PreviousQuantity { get; set; }

        public int NewQuantity { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Description { get; set; }

        public int? UserId { get; set; }

        public int? TransferFromPointId { get; set; }

        public int? TransferToPointId { get; set; }

        [ForeignKey("InventoryItemId")]
        public InventoryItem InventoryItem { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
