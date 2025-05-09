﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    [Table("Categories")]
    public class Categories
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Nombre")]
        public string Name { get; set; }

        // Propiedades adicionales
        [Column("Description")]
        public string? Description { get; set; }
        
        [Column("ImageUrl")]
        public string? ImageUrl { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        public ICollection<SubCategory> SubCategories { get; set; }
            = new List<SubCategory>();
    }
}
