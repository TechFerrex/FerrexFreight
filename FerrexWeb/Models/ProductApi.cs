namespace FerrexWeb.Models
{
    public class ProductApi
    {
        public int id_producto { get; set; }
        public string codigo { get; set; }
        public string desc_producto { get; set; }
        public decimal precio { get; set; }
    }

    public class ApiResponse
    {
        public ProductosResponse Productos { get; set; }
    }

    public class ProductosResponse
    {
        public List<ProductApi> Data { get; set; }
    }
    
}
