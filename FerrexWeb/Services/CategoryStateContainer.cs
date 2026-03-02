using FerrexWeb.Models;
using FerrexWeb.Services;

namespace FerrexWeb.Services
{
    public class CategoryStateContainer
    {
        private List<Categories> _categories;
        private DateTime _lastLoaded = DateTime.MinValue;
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public List<Categories> Categories => _categories;

        public async Task LoadCategoriesAsync(CategoryService categoryService)
        {
            // Verificar si el caché es válido
            if (_categories != null && DateTime.UtcNow - _lastLoaded < CacheExpiration)
            {
                return;
            }

            // Usar semáforo para evitar cargas concurrentes
            await _semaphore.WaitAsync();
            try
            {
                // Doble verificación después de obtener el lock
                if (_categories != null && DateTime.UtcNow - _lastLoaded < CacheExpiration)
                {
                    return;
                }

                _categories = await categoryService.GetActiveCategoriesWithProductsAsync();
                _lastLoaded = DateTime.UtcNow;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Método para forzar recarga si es necesario
        public void InvalidateCache()
        {
            _lastLoaded = DateTime.MinValue;
        }
    }
}
