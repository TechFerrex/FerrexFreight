namespace FerrexWeb.Models
{
    public class FreightConfirmation
    {
        public int Id { get; set; }

        // FK a la cotización / orden
        public int FreightQuotationId { get; set; }
        public FreightQuotation FreightQuotation { get; set; }

        // Token único que se envía al cliente
        public string Token { get; set; } = Guid.NewGuid().ToString("N");

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        // Se llena cuando el cliente hace clic
        public DateTime? ConfirmedAt { get; set; }

        // Estado rápido
        public bool IsConfirmed => ConfirmedAt != null;
    }
}
