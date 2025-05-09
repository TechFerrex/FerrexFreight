using System.ComponentModel.DataAnnotations;

namespace FerrexWeb.Models
{
    public class FreightQuotation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // ID del usuario que genera la cotización

        [Required]
        [StringLength(50)]
        public string QuotationNumber { get; set; } // Número o código de la cotización

        [Required]
        [StringLength(255)]
        public string Origin { get; set; } // Dirección o coordenadas de origen

        [Required]
        [StringLength(255)]
        public string Destination { get; set; } // Dirección o coordenadas de destino

        [Required]
        public DateTime FreightDate { get; set; } // Fecha de programación

        [Required]
        [StringLength(50)]
        public string TruckType { get; set; } // Tipo de camión (ej. "small", "medium")

        [Required]
        [StringLength(50)]
        public string InsuranceOption { get; set; } // Opción de seguro (ej. "none", "basic", "premium")

        [Required]
        public decimal DistanceKm { get; set; } // Distancia en km

        [Required]
        public decimal CostPerKm { get; set; } // Tarifa por km según el tipo de camión

        [Required]
        public decimal BaseCost { get; set; } // Costo base (DistanceKm * CostPerKm)

        [Required]
        public decimal InsuranceCost { get; set; } // Costo adicional del seguro

        [Required]
        public decimal TotalCost { get; set; } // Costo total (BaseCost + InsuranceCost)

        public decimal? FreightLatitude { get; set; } // Latitud del punto de flete (opcional)

        public decimal? FreightLongitude { get; set; } // Longitud del punto de flete (opcional)

        [Required]
        public int Status { get; set; } // 0 = Cotización, 1 = Ordenada, 2 = Expirada/eliminada

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Fecha de creación

        public DateTime? UpdatedDate { get; set; } // Fecha de última actualización
        public User User { get; set; }               // ← navegación

    }
}
