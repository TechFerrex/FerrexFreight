using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    [Table("Subcategory2")]
    public class Subcategory2
    {
        [Key]
        [Column("id_subcategory2")]
        public int id_subcategory2 { get; set; }

        [Column("id_subcategory")] 
        public int id_subcategory { get; set; }

        [Column("name")]
        public string name { get; set; }

        public int? ImageCat { get; set; }

        // NAV: Referencia a la SubCategory padre
        [ForeignKey(nameof(id_subcategory))]
        public SubCategory SubCategory { get; set; }
    }
}
