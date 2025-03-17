namespace FerrexWeb.Models
{
    public class QuotationDetail
    {
        public int QuotationId { get; set; }
        public int Line { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int? VariantId { get; set; } 
        public Quotation? Quotation { get; set; }
        public Products? Product { get; set; }
        public string CustomDescription { get; set; } // Nueva propiedad para descripción personalizada
    

    }
}
