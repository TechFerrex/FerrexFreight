using System.Collections.Generic;
using System.Linq;
using FerrexWeb.Models;
namespace FerrexWeb.Services
{
    public class ProductoService
    {
        private List<Products> productos = new List<Products>();
        public IReadOnlyList<Products> Productos => productos.AsReadOnly();

        public void AgregarProducto(Products producto)
        {
            var item = productos.FirstOrDefault(p => p.IdProducto == producto.IdProducto);
            if (item == null)
            {
                productos.Add(producto);
            }
            else
            {
                 item.DescProducto = producto.DescProducto;
                item.Precio = producto.Precio;
            }
        }
        public Products ObtenerProductoPorId(int productId)
        {
            return productos.FirstOrDefault(p => p.IdProducto == productId);
        }
        public void LimpiarProductos()
        {
            productos.Clear();
        }
    }
}
