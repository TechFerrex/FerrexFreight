namespace FerrexWeb.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int QuantityAvailable { get; set; }
        public List<string> Types { get; set; }
    }
}
