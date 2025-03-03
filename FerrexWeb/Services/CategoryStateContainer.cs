using FerrexWeb.Models;
using FerrexWeb.Services;

namespace FerrexWeb.Services
{
    public class CategoryStateContainer
    {
        public List<Categories> Categories { get; set; }

        public async Task LoadCategoriesAsync(CategoryService categoryService)
        {
            // Solo cargamos las categorías si aún no existen en memoria
            if (Categories == null)
            {
                Categories = await categoryService.GetAllCategoriesAsync();
            }
        }
    }
}
