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
                .Include(p => p.Image)
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return products;
        }

        public async Task<Products> GetProductByIdAsync(int productId)
        {
            var producto = await _dbContext.Products
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
                .Include(p => p.Image)
                .Where(p => p.Types == type)
                .ToListAsync();
        }

        public async Task<Products> GetVariantAsync(string type, string tipo, string size)
        {
            return await _dbContext.Products
                .Include(p => p.Image)
                .FirstOrDefaultAsync(p => p.Types == type
                                       && p.Types == tipo
                                       && p.Size == size);
        }

        public async Task<List<Products>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _dbContext.Products
                .Include(p => p.Image)
                .Where(p => p.CategoriaID == categoryId)
                .ToListAsync();

            return products;
        }


        public async Task<List<AluzincVariant>> GetAluzincVariantsByProductIdAsync(string productId)
        {
            return await _dbContext.AluzincVariants
                .Where(a => a.ProductId == productId)
                .ToListAsync();
        }


        public async Task<AluzincVariant> GetAluzincVariantByIdAsync(int variantId)
        {
             return await _dbContext.AluzincVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }

        public async Task<List<AluzincVariant>> GetAllAluzincVariantsAsync()
        {
            return await _dbContext.AluzincVariants.ToListAsync();
        }

        public async Task<List<AluzincVariant>> GetAluzincVariantsByCodeAsync(string productCode)
        {
            return await _dbContext.AluzincVariants
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
                .Include(p => p.Image)
                .Where(p => p.id_subcategory2 == subCategory2Id)
                .OrderBy(p => p.IdProducto)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Products>> GetProductsBySubCategory2Async(int subCategory2Id)
        {
            return await _dbContext.Products
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
                .Include(p => p.Image)
                .Where(p => p.id_subcategory == subCategoryId)
                .OrderBy(p => p.IdProducto)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Products>> GetProductsBySubCategoryAsync(int subCategoryId)
        {
            return await _dbContext.Products
                .Include(p => p.Image)
                .Where(p => p.id_subcategory == subCategoryId)
                .ToListAsync();
        }
    }
}