namespace FerrexWeb.Models
{
    public class CartItem
    {
        public Products Product { get; set; }  // Cambia de `Product` a `ProductApi`
        public int Quantity { get; set; }
    }
}
