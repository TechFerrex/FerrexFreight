using AngleSharp.Io;
using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FerrexWeb.Services
{
    public class ProductSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public ProductSyncService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Llama a la API, obtiene los productos (usando id_producto),
        /// y actualiza precios en la BD donde la clave es 'Codigo'.
        /// </summary>
        public async Task UpdatePricesByCodigoAsync()
        {
            // 1. Preparar el cuerpo de la petición
            var requestBody = new
            {
                token = "Vda9dGGywdwLxAhc0ieVbFf2Q8viT6VJOTrILYLM9w1xDWYhlPzz5X2HgKjGtzAI",
                licencia = "ferrexpresshn",
                pais = "Honduras",
                user = "api",
                ubicacion = 1,
                lista = 1,
                almacen = 1,
                page = 1,
                itemsPerPage = 1000,
                buscar = ""
            };
            var response = await _httpClient.PostAsJsonAsync("https://app.finzcloud.com/domain/getProductosAS", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error al obtener productos de la API: " + response.StatusCode);
                return;
            }
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            var productosApi = apiResponse?.Productos?.Data ?? new List<ProductApi>();
            var productosBd = await _context.Products.ToListAsync();
            var productosBdPorCodigo = productosBd.ToDictionary(p => p.Codigo, p => p);
            foreach (var apiProd in productosApi)
            {

                int idProdApi = apiProd.id_producto;
                if (idProdApi <= 0)
                {
                    Console.WriteLine($"[ERROR] El 'id_producto' de la API no es válido: {idProdApi}");
                    continue;
                }

                int sumado = idProdApi + 100000;
                string codigoBd;
                if (sumado < 100765)
                {
                    codigoBd = sumado.ToString();
                }
                else
                {
                    codigoBd = "F" + (sumado);
                }
                if (productosBdPorCodigo.TryGetValue(codigoBd, out var productoExistente))
                {
                    var precioAnterior = productoExistente.Precio;
                    productoExistente.Precio = apiProd.precio;
                    Console.WriteLine($"[ACTUALIZADO] Codigo={productoExistente.Codigo} | " +
                                      $"Precio: {precioAnterior} => {apiProd.precio}");
                }
                else
                {
                    Console.WriteLine($"[NO ENCONTRADO] Producto con codigo={codigoBd} no existe en la BD");
                }
            }
            await _context.SaveChangesAsync();
            Console.WriteLine("Actualización de precios finalizada.");
        }
    }
}
