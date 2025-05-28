using FerrexWeb.Pages;
using FerrexWeb.Models;

using Microsoft.EntityFrameworkCore;


namespace FerrexWeb.Services
{
    public class FreightQuotationService
    {
        private readonly ApplicationDbContext _context;

        public FreightQuotationService(ApplicationDbContext context)
        {
            _context = context;
        }
        // Método existente para agregar una cotización
        public async Task AddFreightQuotationAsync(FreightQuotation freightQuotation)
        {
            _context.FreightQuotations.Add(freightQuotation);
            await _context.SaveChangesAsync();
        }

        // Método para obtener cotizaciones del usuario (ejemplo anterior)
        public async Task<List<FreightQuotation>> GetQuotationsByUserIdAsync(int userId)
        {
            return await _context.FreightQuotations
                                 .Where(fq => fq.UserId == userId)
                                 .OrderByDescending(fq => fq.CreatedDate)
                                 .ToListAsync();
        }

        // Nuevo método para obtener TODAS las cotizaciones (para SuperAdmin)
        public async Task<List<FreightQuotation>> GetAllFreightQuotationsAsync()
        {
            return await _context.FreightQuotations
                         .Include(fq => fq.User)         // 👈  eager‑loading del usuario
                         .OrderByDescending(fq => fq.CreatedDate)
                         .AsNoTracking()                 
                         .ToListAsync();
        }

        // Método para actualizar el estado de una cotización
        public async Task UpdateStatusAsync(int id, int newStatus)
        {
            var fq = await _context.FreightQuotations.FirstOrDefaultAsync(f => f.Id == id);
            if (fq != null)
            {
                fq.Status = newStatus;
                fq.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteFreightQuotationAsync(int id)
        {
            // Busca la entidad por su clave
            var fq = await _context.FreightQuotations.FindAsync(id);
            if (fq != null)
            {
                // Márkalo para borrado y guarda
                _context.FreightQuotations.Remove(fq);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<FreightQuotation>> GetCalendarItemsAsync(int userId) =>
    _context.FreightQuotations
        .Where(q => q.UserId == userId
                    && q.Status != (int)FreightStatus.Quotation)
        .AsNoTracking()
        .OrderBy(q => q.FreightDate)
        .ToListAsync();

        public Task<List<FreightQuotation>> GetCalendarItemsForAllUsersAsync() =>
    _context.FreightQuotations
        .Where(q => q.Status == (int)FreightStatus.Ordered
                 || q.Status == (int)FreightStatus.Expired)
        .AsNoTracking()
        .ToListAsync();


    }
}
