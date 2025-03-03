using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;

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
            // Obtiene categorías aleatorias
            return await _dbContext.Categories
                .OrderBy(c => Guid.NewGuid())
                .Take(count)
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
    }
}
