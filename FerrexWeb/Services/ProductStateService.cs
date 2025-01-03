using FerrexWeb.Models;

namespace FerrexWeb.Services
{
    public class ProductStateService
    {
        public Products SelectedProduct { get; private set; }

        public void SetSelectedProduct(Products product)
        {
            SelectedProduct = product;
        }
    }
}
