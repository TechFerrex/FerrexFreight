using System.Collections.Generic;
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
