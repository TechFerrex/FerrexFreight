using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FerrexWeb.Services
{
    public class SubCategoryService
    {
        private readonly ApplicationDbContext _dbContext;

        public SubCategoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SubCategory>> GetSubCategoriesByCategoryIdAsync(int categoryId)
        {
            return await _dbContext.SubCategories
               .Include(s => s.SubCategories2) 
               .Where(s => s.id_categories == categoryId)
               .ToListAsync();
        }

        public async Task<SubCategory> GetSubCategoryByIdAsync(int subCategoryId)
        {
            return await _dbContext.SubCategories
                .Include(s => s.SubCategories2) // opcional es pendejo
                .FirstOrDefaultAsync(s => s.id_subcategory == subCategoryId);
        }
    }
}
