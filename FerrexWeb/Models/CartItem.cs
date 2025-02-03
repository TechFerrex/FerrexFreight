namespace FerrexWeb.Models
{
    public class CartItem
    {
        public Products Product { get; set; }
        public int Quantity { get; set; }

        public decimal? AluzincLargo { get; set; }
        public decimal? TotalPiesAluzinc { get; set; }

        // Para identificar la combinación elegida
        public string? AluzincCalibre { get; set; }
        public string? AluzincMilimetro { get; set; }
        public string? AluzincColor { get; set; }
    }

}
