namespace FerrexWeb.Models
{
    public class CartItem
    {
        public Products Product { get; set; }
        public int Quantity { get; set; }

        // Campos extra SOLO para lamina Aluzinc (opcionalmente nulos)
        public string? AluzincCalibre { get; set; }
        public string? AluzincMilimetro { get; set; }
        public string? AluzincColor { get; set; }

        /// <summary>
        /// Largo en pies (si el usuario elige 20 pies, acá se guarda 20).
        /// </summary>
        public decimal? AluzincLargo { get; set; }

        /// <summary>
        /// Cantidad total de pies, calculado como (AluzincLargo * Quantity) cuando se agrega al carrito.
        /// </summary>
        public decimal? TotalPiesAluzinc { get; set; }
    }

}
