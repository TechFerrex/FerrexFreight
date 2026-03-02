using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;
using static iTextSharp.tool.xml.html.HTML;

namespace FerrexWeb.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Categories>> GetRandomCategoriesAsync(int count)
        {
            // Optimizado: las categorías son pocas, traer activas y mezclar en memoria
            var activeCategories = await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync();

            var random = new Random();
            return activeCategories
                .OrderBy(_ => random.Next())
                .Take(count)
                .ToList();
        }

        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Categories> GetCategoryByIdAsync(int categoryId)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<List<Categories>> GetActiveCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve solo las categorías activas que tienen al menos un producto.
        /// </summary>
        public async Task<List<Categories>> GetActiveCategoriesWithProductsAsync()
        {
            var categoryIdsWithProducts = await _dbContext.Products
                .Select(p => p.CategoriaID)
                .Distinct()
                .ToListAsync();

            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && categoryIdsWithProducts.Contains(c.Id))
                .ToListAsync();
        }

    }
}
