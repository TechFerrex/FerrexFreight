using System.ComponentModel.DataAnnotations;

namespace FerrexWeb.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<InventoryPoint> InventoryPoints { get; set; } = new List<InventoryPoint>();
    }
}
