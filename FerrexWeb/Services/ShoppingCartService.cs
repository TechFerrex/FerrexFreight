using Blazored.LocalStorage;
using FerrexWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FerrexWeb.Services
{
    public class ShoppingCartService
    {
        private readonly ILocalStorageService _localStorage;
        private const string CartKey = "shoppingCart";
        private bool isInitialized = false;
        public event Action OnChange;

        public List<CartItem> Items { get; private set; } = new List<CartItem>();
        public int? CurrentQuotationId { get; set; }


        public ShoppingCartService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task EnsureInitializedAsync()
        {
            if (!isInitialized)
            {
                Items = await _localStorage.GetItemAsync<List<CartItem>>(CartKey) ?? new List<CartItem>();
                isInitialized = true;
                NotifyStateChanged();
            }
        }
        public async Task AddToCartAsync(
     Products product,
     int quantity,
     decimal? aluzincLargo = null,
     string? aluzincCalibre = null,
     string? aluzincMilimetro = null,
     string? aluzincColor = null,
     string? customDescription = null,
     decimal? customPricePerPie = null
 )

        {
            // Crea un clon completo del producto para que no se sobrescriba en otros ítems.
            var productClone = new Products
            {
                IdProducto = product.IdProducto,           // Asegúrate de copiar el Id
                Codigo = product.Codigo,
                Unit = product.Unit,
                Types = product.Types,
                // Si se pasó un precio base personalizado, lo usamos; de lo contrario, se usa el precio normal
                Precio = customPricePerPie ?? product.Precio,
                // Si se pasó una descripción personalizada (que incluya detalles), la usamos
                DescProducto = !string.IsNullOrEmpty(customDescription) ? customDescription : product.DescProducto,
                // Copia otras propiedades necesarias (por ejemplo, ImageUrl, etc.)
            };

            // Busca si ya existe un ítem con la misma combinación (por ejemplo, misma variante Aluzinc)
            var existingItem = Items.FirstOrDefault(item =>
                item.Product.IdProducto == productClone.IdProducto &&
                item.AluzincCalibre == aluzincCalibre &&
                item.AluzincMilimetro == aluzincMilimetro &&
                item.AluzincColor == aluzincColor
            );

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (existingItem.AluzincLargo.HasValue)
                    existingItem.TotalPiesAluzinc = existingItem.AluzincLargo.Value * existingItem.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    Product = productClone,
                    Quantity = quantity,
                    AluzincLargo = aluzincLargo,
                    AluzincCalibre = aluzincCalibre,
                    AluzincMilimetro = aluzincMilimetro,
                    AluzincColor = aluzincColor,
                    BasePrice = customPricePerPie 
                };

                if (aluzincLargo.HasValue)newItem.TotalPiesAluzinc = aluzincLargo.Value * quantity;

                Items.Add(newItem);
            }

            await SaveCartAsync();
            NotifyStateChanged();
        }



        public async Task RemoveFromCartAsync(Products product)
        {
            var cartItem = Items.Find(item => item.Product.IdProducto == product.IdProducto);
            if (cartItem != null)
            {
                Items.Remove(cartItem);
                await SaveCartAsync();
                NotifyStateChanged();
            }
        }

        public decimal GetTotalPrice()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                if (item.Product.Types?.Contains("Aluzinc", StringComparison.OrdinalIgnoreCase) == true
                    && item.AluzincLargo.HasValue)
                {
                    total += item.Product.Precio * (item.AluzincLargo.Value * item.Quantity);
                }
                else
                {
                    total += item.Product.Precio * item.Quantity;
                }
            }
            return total;
        }


        public async Task SaveCartAsync()
        {
            await _localStorage.SetItemAsync(CartKey, Items);
        }


        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task ClearCartAsync()
        {
            Items.Clear();
            await _localStorage.RemoveItemAsync(CartKey);
            NotifyStateChanged();
        }
    }
}
