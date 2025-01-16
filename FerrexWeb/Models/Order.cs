using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int UserId { get; set; } // Clave foránea hacia la tabla User
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Freight { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }
        public string DeliveryInstructions { get; set; }
        public double FreightLatitude { get; set; }
        public double FreightLongitude { get; set; }
        public string Status { get; set; } = "Pending";
        public User User { get; set; }
        public List<OrderDetail> OrderedItems { get; set; } = new List<OrderDetail>();
    }
}
