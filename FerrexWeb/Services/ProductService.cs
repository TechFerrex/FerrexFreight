using FerrexWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FerrexWeb.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        
        public ProductService(ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }


        public async Task<List<Products>> GetRandomProductsAsync(int count)
        {
            var products = await _dbContext.Products
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            foreach (var producto in products)
            {
                producto.ImageUrl = GetProductImageUrl(producto.ImageUrl);
            }

            return products;
        }


        public async Task<Products> GetProductByIdAsync(int productId)
        {
            var producto = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.IdProducto == productId);

            if (producto != null)
            {
                producto.ImageUrl = GetProductImageUrl(producto.ImageUrl);
            }

            return producto;
        }
        public string GetProductImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return "images/product/default.png"; 

            var extensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };

            // Si imageUrl ya tiene extensión, retornamos tal cual
            if (extensions.Any(ext => imageUrl.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                return imageUrl;
            }

            // Ruta física de la carpeta de imágenes
            var imagesFolder = Path.Combine(_hostingEnvironment.WebRootPath, Path.GetDirectoryName(imageUrl) ?? string.Empty);
            var imageName = Path.GetFileName(imageUrl);

            foreach (var ext in extensions)
            {
                var fullImagePath = Path.Combine(imagesFolder, imageName + ext);
                if (System.IO.File.Exists(fullImagePath))
                {
                    // Retornamos la URL con la extensión encontrada
                    return Path.Combine(Path.GetDirectoryName(imageUrl) ?? string.Empty, imageName + ext).Replace("\\", "/");
                }
            }

            // Si no se encuentra ningún archivo, retornamos una imagen por defecto
            return "images/product/default.png";
        }

        public async Task<List<Products>> GetProductsByNewProductoTypeAsync(string newProductType)
        {
            return await _dbContext.Products
                .Where(p => p.NewProductoType == newProductType)
                .ToListAsync();

        }

        // Nuevo método para obtener una variante específica
        public async Task<Products> GetVariantAsync(string newProductType, string tipo, string size)
        {
            return await _dbContext.Products
                .FirstOrDefaultAsync(p => p.NewProductoType == newProductType
                                       && p.Types == tipo
                                       && p.Size == size);
        }

        public async Task<List<Products>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _dbContext.Products
                .Where(p => p.CategoriaID == categoryId)
                .ToListAsync();

            foreach (var producto in products)
            {
                producto.ImageUrl = GetProductImageUrl(producto.ImageUrl);
            }

            return products;
        }
    }
}