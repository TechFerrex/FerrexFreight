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
            // Optimizado: obtener IDs primero, luego seleccionar aleatoriamente
            var totalCount = await _dbContext.Products.CountAsync();
            if (totalCount == 0) return new List<Products>();

            var random = new Random();
            var skipCount = Math.Max(0, random.Next(totalCount - count));

            var products = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Skip(skipCount)
                .Take(count)
                .ToListAsync();

            return products;
        }

        public async Task<Products> GetProductByIdAsync(int productId)
        {
            var producto = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .FirstOrDefaultAsync(p => p.IdProducto == productId);

            return producto;
        }

        public string GetProductImageUrl(int imageId)
        {
            if (imageId <= 0)
                return "images/product/default.png";

            int imageName = imageId;

            var extensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            var imagesFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/product");

            foreach (var ext in extensions)
            {
                var fullImagePath = Path.Combine(imagesFolder, imageName + ext);
                if (System.IO.File.Exists(fullImagePath))
                {
                    return $"images/product/{imageName}{ext}";
                }
            }

            return "images/product/default.png";
        }


        public async Task<List<Products>> GetProductsByTypeAsync(string type)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.Types == type)
                .ToListAsync();
        }

        public async Task<Products> GetVariantAsync(string type, string size)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .FirstOrDefaultAsync(p => p.Types == type
                                       && p.Size == size);
        }

        public async Task<List<Products>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.CategoriaID == categoryId)
                .ToListAsync();
        }


        public async Task<List<AluzincVariant>> GetAluzincVariantsByProductIdAsync(string productId)
        {
            return await _dbContext.AluzincVariants
                .AsNoTracking()
                .Where(a => a.ProductId == productId)
                .ToListAsync();
        }

        public async Task<AluzincVariant> GetAluzincVariantByIdAsync(int variantId)
        {
            return await _dbContext.AluzincVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }

        public async Task<List<AluzincVariant>> GetAllAluzincVariantsAsync()
        {
            return await _dbContext.AluzincVariants
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<AluzincVariant>> GetAluzincVariantsByCodeAsync(string productCode)
        {
            return await _dbContext.AluzincVariants
                .AsNoTracking()
                .Where(a => a.ProductId == productCode)
                .ToListAsync();
        }


        public async Task<int> GetProductCountBySubCategoryIdAsync(int subCategoryId)
        {
            return await _dbContext.Products
                .Where(p => p.id_subcategory == subCategoryId)
                .CountAsync();
        }

        public async Task<int> GetProductCountBySubCategory2Async(int subCategory2Id)
        {
            return await _dbContext.Products
                .CountAsync(p => p.id_subcategory2 == subCategory2Id);
        }

        public async Task<Products> GetFirstProductBySubCategory2Async(int subCategory2Id)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.id_subcategory2 == subCategory2Id)
                .OrderBy(p => p.IdProducto)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Products>> GetProductsBySubCategory2Async(int subCategory2Id)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.id_subcategory2 == subCategory2Id)
                .ToListAsync();
        }

        public async Task<int> GetProductCountBySubCategoryAsync(int subCategoryId)
        {
            return await _dbContext.Products
                .CountAsync(p => p.id_subcategory == subCategoryId);
        }

        public async Task<Products> GetFirstProductBySubCategoryAsync(int subCategoryId)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.id_subcategory == subCategoryId)
                .OrderBy(p => p.IdProducto)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Products>> GetProductsBySubCategoryAsync(int subCategoryId)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.id_subcategory == subCategoryId)
                .ToListAsync();
        }

        public async Task<List<VigaVariant>> GetVigaVariantsAsync()
        {
            return await _dbContext.VigaVariants
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el primer producto de cada subcategoría en una sola query (evita N+1).
        /// </summary>
        public async Task<Dictionary<int, Products>> GetFirstProductBySubCategoryIdsAsync(IEnumerable<int> subCategoryIds)
        {
            var ids = subCategoryIds.ToList();
            if (!ids.Any()) return new Dictionary<int, Products>();

            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => ids.Contains(p.id_subcategory))
                .GroupBy(p => p.id_subcategory)
                .Select(g => g.OrderBy(p => p.IdProducto).First())
                .ToDictionaryAsync(p => p.id_subcategory);
        }

        /// <summary>
        /// Obtiene el primer producto de cada subcategory2 en una sola query (evita N+1).
        /// </summary>
        public async Task<Dictionary<int, Products>> GetFirstProductBySubCategory2IdsAsync(IEnumerable<int> subCategory2Ids)
        {
            var ids = subCategory2Ids.ToList();
            if (!ids.Any()) return new Dictionary<int, Products>();

            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.id_subcategory2.HasValue && ids.Contains(p.id_subcategory2.Value))
                .GroupBy(p => p.id_subcategory2.Value)
                .Select(g => g.OrderBy(p => p.IdProducto).First())
                .ToDictionaryAsync(p => p.id_subcategory2.Value);
        }

        // ── CRUD ──

        public async Task<List<Products>> SearchProductsAdminAsync(string search, int page, int pageSize)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.DescProducto.ToLower().Contains(term) ||
                    p.Codigo.ToLower().Contains(term));
            }

            return await query
                .OrderByDescending(p => p.IdProducto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountProductsAdminAsync(string search)
        {
            var query = _dbContext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.DescProducto.ToLower().Contains(term) ||
                    p.Codigo.ToLower().Contains(term));
            }

            return await query.CountAsync();
        }

        public async Task<Products> CreateProductAsync(Products product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task UpdateProductAsync(Products product)
        {
            var existing = await _dbContext.Products.FindAsync(product.IdProducto);
            if (existing == null) return;

            existing.Codigo = product.Codigo;
            existing.DescProducto = product.DescProducto;
            existing.Precio = product.Precio;
            existing.Unit = product.Unit;
            existing.Types = product.Types;
            existing.Size = product.Size;
            existing.CategoriaID = product.CategoriaID;
            existing.id_subcategory = product.id_subcategory;
            existing.id_subcategory2 = product.id_subcategory2;
            existing.id_image = product.id_image;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null) return false;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Image> CreateImageAsync(string url, string title)
        {
            var image = new Image { url = url, title = title };
            _dbContext.Set<Image>().Add(image);
            await _dbContext.SaveChangesAsync();
            return image;
        }

        public async Task UpdateImageUrlAsync(int imageId, string url)
        {
            var image = await _dbContext.Set<Image>().FindAsync(imageId);
            if (image != null)
            {
                image.url = url;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}