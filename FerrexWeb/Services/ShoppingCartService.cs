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
    string? aluzincColor = null
)
        {
            // 1) Verificar si YA existe un ítem con la MISMA variante Aluzinc
            //    (Coincide ID del producto, calibre, milímetros y color).
            var existingItem = Items.FirstOrDefault(item =>
                item.Product.IdProducto == product.IdProducto
                && item.AluzincCalibre == aluzincCalibre
                && item.AluzincMilimetro == aluzincMilimetro
                && item.AluzincColor == aluzincColor
            );

            if (existingItem != null)
            {
                // Si ya existe EXACTAMENTE esa variante en el carrito, sumamos la cantidad
                existingItem.Quantity += quantity;

                // Recalcular total de pies (si se definió el largo)
                if (existingItem.AluzincLargo.HasValue)
                {
                    existingItem.TotalPiesAluzinc = existingItem.AluzincLargo.Value * existingItem.Quantity;
                }
            }
            else
            {
                // 2) Crear un nuevo CartItem
                var newItem = new CartItem
                {
                    Product = product,
                    Quantity = quantity,

                    // Asignar los datos de la variante
                    AluzincLargo = aluzincLargo,
                    AluzincCalibre = aluzincCalibre,
                    AluzincMilimetro = aluzincMilimetro,
                    AluzincColor = aluzincColor
                };

                // 3) Calcular total de pies si hay largo
                if (aluzincLargo.HasValue)
                    newItem.TotalPiesAluzinc = aluzincLargo.Value * quantity;

                // 4) Agregar al carrito
                Items.Add(newItem);
            }

            // 5) Guardar en Local Storage y notificar
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
