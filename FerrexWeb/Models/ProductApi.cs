namespace FerrexWeb.Models
{
    public class ProductApi
    {
        public int id_producto { get; set; }
        public string codigo { get; set; }
        public string desc_producto { get; set; }
        public decimal precio { get; set; }
        // Agregar más propiedades si la API retorna más campos
    }

    public class ApiResponse
    {
        // La estructura depende de tu JSON de respuesta
        // Ajusta nombres o propiedades según tu respuesta real
        public ProductosResponse Productos { get; set; }
    }

    public class ProductosResponse
    {
        public List<ProductApi> Data { get; set; }
    }
    
}
