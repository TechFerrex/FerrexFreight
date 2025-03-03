using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FerrexWeb.Services
{
    public class SubCategory2Service
    {
        private readonly ApplicationDbContext _dbContext;

        public SubCategory2Service(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Subcategory2>> GetSubCategory2BySubCategoryIdAsync(int subCategoryId)
        {
            return await _dbContext.SubCategories2
                .Where(s => s.id_subcategory == subCategoryId)
                .ToListAsync();
        }

        public async Task<Subcategory2> GetSubCategory2ByIdAsync(int subCategory2Id)
        {
            return await _dbContext.SubCategories2
                .FirstOrDefaultAsync(s => s.id_subcategory2 == subCategory2Id);
        }
    }
}
