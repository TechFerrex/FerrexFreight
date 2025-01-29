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
            Products product, int quantity, decimal? aluzincLargo = null, string? aluzincCalibre = null, string? aluzincMilimetro = null, string? aluzincColor = null)
        {
            // 1) Verificar si ya existe el ítem en el carrito
            var cartItem = Items.FirstOrDefault(item =>
                item.Product.IdProducto == product.IdProducto
                // Si quieres además discriminar por calibre, color, etc., agréga más condiciones:
                && item.AluzincCalibre == aluzincCalibre
                && item.AluzincMilimetro == aluzincMilimetro
                && item.AluzincColor == aluzincColor
                && item.AluzincLargo == aluzincLargo
            );

            // 2) Si existe, solo sumamos la cantidad
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                if (cartItem.AluzincLargo.HasValue)
                {
                    // Recalcular total de pies
                    cartItem.TotalPiesAluzinc = cartItem.AluzincLargo.Value * cartItem.Quantity;
                }
            }
            else
            {
                // 3) Crear un nuevo CartItem con campos extra
                var newItem = new CartItem
                {
                    Product = product,
                    Quantity = quantity,
                    AluzincLargo = aluzincLargo,
                    AluzincCalibre = aluzincCalibre,
                    AluzincMilimetro = aluzincMilimetro,
                    AluzincColor = aluzincColor
                };

                // Calcular total de pies si aplica
                if (aluzincLargo.HasValue)
                    newItem.TotalPiesAluzinc = aluzincLargo.Value * quantity;

                Items.Add(newItem);
            }

            // Guardar en Local Storage
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
            return Items.Sum(item => item.Product.Precio * item.Quantity);
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
