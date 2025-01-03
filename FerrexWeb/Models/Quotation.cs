namespace FerrexWeb.Models
{
    public class Quotation
    {
        public int Id { get; set; }
        public string? Client { get; set; }
        public DateTime Date { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ISV { get; set; }
        public decimal Total { get; set; }
        public decimal Freight { get; set; }
        public int UserID { get; set; }
        public string? QuotationNumber { get; set; }
        public List<QuotationDetail> QuotedItems { get; set; } = new List<QuotationDetail>();
        public double FreightLatitude { get; set; }
        public double FreightLongitude { get; set; }
        public string DeliveryInstructions { get; set; }


        public bool IsOrdered { get; set; } = false;

    }
}
