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
            return await _dbContext.Categories
        .Where(c => c.IsActive)            // Solo categorías activas
        .OrderBy(c => Guid.NewGuid())      // Orden aleatorio
        .Take(count)                       // Toma n (ej. 3)
        .ToListAsync();
        }

        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories.ToListAsync();
        }

        public async Task<Categories> GetCategoryByIdAsync(int categoryId)
        {
            return await _dbContext.Categories
                .Include(c => c.SubCategories) // opcional: eager load
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<List<Categories>> GetActiveCategoriesAsync()
        {
            return await _dbContext.Categories.Where(c => c.IsActive).ToListAsync();
        }

    }
}
