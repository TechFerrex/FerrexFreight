using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using FerrexWeb.Models;
using Microsoft.Extensions.Configuration;

namespace FerrexWeb.Services
{
    public class SeekerService
    {
        private readonly string _connectionString;

        public SeekerService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Products>> GetProductosAsync(string searchTerm = "")
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT TOP 5
                    p.ID AS IdProducto,
                    p.Codigo,
                    p.Product AS DescProducto,
                    p.Precio,
                    c.Nombre AS Categoria
                FROM Products p
                LEFT JOIN Categories c ON p.CategoriaID = c.ID
                WHERE p.Product LIKE @SearchTerm OR p.Codigo LIKE @SearchTerm
                ORDER BY p.Product";
                var products = await connection.QueryAsync<Products>(
                    query,
                    new { SearchTerm = $"%{searchTerm}%" });

                return products.AsList();
            }
        }

        /// <summary>
        /// Búsqueda completa: divide el término en palabras y busca coincidencias
        /// en nombre, código, tipo y tamaño. Ordena por relevancia (más palabras coincidentes primero).
        /// </summary>
        public async Task<List<Products>> SearchProductsFullAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Products>();

            var words = searchTerm
                .Split(new[] { ' ', ',', ';', '.', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();

            if (!words.Any())
                words.Add(searchTerm.Trim());

            // Construimos las condiciones LIKE dinámicamente con parámetros seguros
            var whereParts = new List<string>();
            var parameters = new DynamicParameters();
            int i = 0;
            foreach (var w in words)
            {
                var pname = $"@w{i}";
                whereParts.Add($"(p.Product LIKE {pname} OR p.Codigo LIKE {pname} OR p.Types LIKE {pname} OR p.Size LIKE {pname})");
                parameters.Add($"w{i}", $"%{w}%");
                i++;
            }
            var whereClause = string.Join(" OR ", whereParts);

            // Score: cuenta cuántas palabras coinciden en el nombre del producto
            var scoreParts = new List<string>();
            int j = 0;
            foreach (var w in words)
            {
                scoreParts.Add($"CASE WHEN p.Product LIKE @w{j} THEN 2 ELSE 0 END + CASE WHEN p.Codigo LIKE @w{j} OR p.Types LIKE @w{j} OR p.Size LIKE @w{j} THEN 1 ELSE 0 END");
                j++;
            }
            var scoreExpr = string.Join(" + ", scoreParts);

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string query = $@"
                        SELECT TOP 60
                            p.ID AS IdProducto,
                            p.Codigo,
                            p.Product AS DescProducto,
                            p.Types,
                            p.Size,
                            p.Precio,
                            p.CategoriaID,
                            p.Unit,
                            p.id_subcategory,
                            p.id_subcategory2,
                            p.id_image
                        FROM Products p
                        WHERE {whereClause}
                        ORDER BY ({scoreExpr}) DESC, p.Product ASC";

                    var products = (await connection.QueryAsync<Products>(query, parameters)).ToList();

                    if (products.Any())
                    {
                        var imageIds = products
                            .Where(p => p.id_image.HasValue && p.id_image.Value > 0)
                            .Select(p => p.id_image.Value)
                            .Distinct()
                            .ToList();

                        if (imageIds.Any())
                        {
                            var imageQuery = "SELECT id_image, url, title FROM Image WHERE id_image IN @ids";
                            var images = (await connection.QueryAsync<Image>(imageQuery, new { ids = imageIds }))
                                .ToDictionary(img => img.id_image);

                            foreach (var p in products)
                            {
                                if (p.id_image.HasValue && images.TryGetValue(p.id_image.Value, out var img))
                                {
                                    p.Image = img;
                                }
                            }
                        }
                    }

                    return products;
                }
            }
            catch
            {
                return new List<Products>();
            }
        }

        /// <summary>
        /// Devuelve sugerencias de palabras relacionadas a partir de los nombres de productos coincidentes.
        /// </summary>
        public async Task<List<string>> GetRelatedKeywordsAsync(string searchTerm, int maxResults = 8)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<string>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT TOP 40 p.Product
                        FROM Products p
                        WHERE p.Product LIKE @SearchTerm OR p.Types LIKE @SearchTerm";

                    var names = (await connection.QueryAsync<string>(
                        query,
                        new { SearchTerm = $"%{searchTerm}%" })).ToList();

                    var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "de", "del", "la", "el", "los", "las", "un", "una", "y", "o", "en",
                        "para", "con", "por", "a", "al", "x", "mm", "cm", "m", "kg", "lb"
                    };
                    var inputWords = new HashSet<string>(
                        searchTerm.Split(new[] { ' ', ',', ';', '.', '-' }, StringSplitOptions.RemoveEmptyEntries),
                        StringComparer.OrdinalIgnoreCase);

                    return names
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .SelectMany(n => n.Split(new[] { ' ', ',', ';', '.', '-', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(w => w.Trim())
                        .Where(w => w.Length >= 3 && !stopWords.Contains(w) && !inputWords.Contains(w))
                        .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .Take(maxResults)
                        .ToList();
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<Products> GetProductoByIdAsync(int productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        p.ID AS IdProducto,
                        p.Codigo,
                        p.Product AS DescProducto,
                        p.Precio,
                        c.Nombre AS Categoria
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoriaID = c.ID
                    WHERE p.ID = @ProductId";
                var product = await connection.QuerySingleOrDefaultAsync<Products>(
                    query,
                    new { ProductId = productId });

                return product;
            }
        }
    }
}
