using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    [Table("Subcategory")] 
    public class SubCategory
    {
        [Key]
        [Column("id_subcategory")]
        public int id_subcategory { get; set; }

        [Column("id_categories")]  
        public int id_categories { get; set; }

        [Column("name")]
        public string subcategory { get; set; }

        public string? imageCat { get; set; }

        // NAV: Referencia a la categoría padre
        [ForeignKey(nameof(id_categories))]
        public Categories Category { get; set; }

        // Navegación a Subcategory2
        public ICollection<Subcategory2> SubCategories2 { get; set; }
            = new List<Subcategory2>();
    }
}
