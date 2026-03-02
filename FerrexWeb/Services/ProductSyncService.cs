using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FerrexWeb.Services
{
    public class ProductSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProductSyncService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }


        private const int BatchSize = 100;

        public async Task UpdatePricesByCodigoAsync()
        {
            var finzConfig = _configuration.GetSection("FinzCloud");

            // 1. Preparar el cuerpo de la petición
            var requestBody = new
            {
                token = finzConfig["Token"],
                licencia = finzConfig["Licencia"],
                pais = "Honduras",
                user = finzConfig["User"],
                ubicacion = 1,
                lista = 1,
                almacen = 1,
                page = 1,
                itemsPerPage = 1000,
                buscar = ""
            };

            var baseUrl = finzConfig["BaseUrl"] ?? "https://app.finzcloud.com/domain/getProductosAS";
            var response = await _httpClient.PostAsJsonAsync(baseUrl, requestBody);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error al obtener productos de la API: " + response.StatusCode);
                return;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            var productosApi = apiResponse?.Productos?.Data ?? new List<ProductApi>();

            // Optimización: Crear lista de códigos para buscar en lotes
            var codigosToUpdate = new Dictionary<string, decimal>();
            foreach (var apiProd in productosApi)
            {
                int idProdApi = apiProd.id_producto;
                if (idProdApi <= 0) continue;

                int sumado = idProdApi + 100000;
                string codigoBd = sumado < 100765 ? sumado.ToString() : $"F{sumado}";
                codigosToUpdate[codigoBd] = apiProd.precio;
            }

            // Procesar en lotes para evitar cargar todo en memoria
            var codigosList = codigosToUpdate.Keys.ToList();
            int totalUpdated = 0;

            for (int i = 0; i < codigosList.Count; i += BatchSize)
            {
                var batchCodigos = codigosList.Skip(i).Take(BatchSize).ToList();

                // Cargar solo los productos del lote actual
                var productosBatch = await _context.Products
                    .Where(p => batchCodigos.Contains(p.Codigo))
                    .ToListAsync();

                foreach (var producto in productosBatch)
                {
                    if (codigosToUpdate.TryGetValue(producto.Codigo, out var nuevoPrecio))
                    {
                        if (producto.Precio != nuevoPrecio)
                        {
                            producto.Precio = nuevoPrecio;
                            totalUpdated++;
                        }
                    }
                }

                // Guardar cambios del lote actual
                await _context.SaveChangesAsync();

                // Liberar memoria desvinculando entidades procesadas
                foreach (var producto in productosBatch)
                {
                    _context.Entry(producto).State = EntityState.Detached;
                }
            }

            Console.WriteLine($"Actualización de precios finalizada. {totalUpdated} productos actualizados.");
        }
    }
}
