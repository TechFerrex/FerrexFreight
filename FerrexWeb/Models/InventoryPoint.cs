using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class InventoryPoint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [Required]
        public int CityId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("CityId")]
        public City City { get; set; }

        public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();

        [NotMapped]
        public int TotalProducts => Items?.Count ?? 0;

        [NotMapped]
        public int LowStockCount => Items?.Count(i => i.Quantity <= i.MinStock) ?? 0;
    }
}
