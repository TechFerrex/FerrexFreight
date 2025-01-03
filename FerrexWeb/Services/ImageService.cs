using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace FerrexWeb.Services
{
    public class ImageService
    {
        private readonly IJSRuntime jsRuntime;

        public ImageService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task<string> GetValidImageUrl(string baseUrl, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                var fullUrl = baseUrl + ext;
                bool exists = await jsRuntime.InvokeAsync<bool>("imageExists", fullUrl);
                if (exists)
                {
                    return fullUrl;
                }
            }
            // Si ninguna imagen existe, devuelve una imagen por defecto o una cadena vacía
            return "images/product/default.png";
        }
    }
}
