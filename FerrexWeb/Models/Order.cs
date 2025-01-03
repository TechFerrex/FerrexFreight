using System;
using System.Collections.Generic;

namespace FerrexWeb.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int UserId { get; set; } // Clave foránea hacia la tabla User
        public DateTime OrderDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Freight { get; set; }
        public decimal Total { get; set; }
        public string DeliveryInstructions { get; set; }
        public double FreightLatitude { get; set; }
        public double FreightLongitude { get; set; }
        public string Status { get; set; } = "Pending"; // Estado de la orden cuando se ordena

        // Propiedades de navegación
        public User User { get; set; }
        public List<OrderDetail> OrderedItems { get; set; } = new List<OrderDetail>();
    }
}
