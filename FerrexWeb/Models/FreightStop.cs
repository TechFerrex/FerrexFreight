using System.ComponentModel.DataAnnotations;

namespace FerrexWeb.Models
{
    public class FreightStop
    {
        [Key] public int Id { get; set; }

        [Required] public int FreightQuotationId { get; set; }
        public FreightQuotation FreightQuotation { get; set; }

        [Required, StringLength(255)] public string Address { get; set; }
        [Required] public decimal Latitude { get; set; }
        [Required] public decimal Longitude { get; set; }

        [Required] public int Sequence { get; set; }          // 1,2,3…
        [Required] public decimal ExtraCost { get; set; } = 150;
    }
}
