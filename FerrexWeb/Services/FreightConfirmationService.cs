using FerrexWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FerrexWeb.Services
{
    public class FreightConfirmationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _ctxFactory;
        private readonly NavigationManager _nav;
        private readonly ILogger<FreightConfirmationService> _logger;

        public FreightConfirmationService(IDbContextFactory<ApplicationDbContext> ctxFactory,
                                          NavigationManager nav,
                                          ILogger<FreightConfirmationService> logger)
        {
            _ctxFactory = ctxFactory;
            _nav = nav;
            _logger = logger;
        }

        /// Devuelve el link si ya existe un token activo, o null.
        public async Task<string?> GetExistingLinkAsync(int quotationId)
        {
            await using var ctx = await _ctxFactory.CreateDbContextAsync();  // ← aquí

            var fc = await ctx.FreightConfirmations
                                       .Where(f => f.FreightQuotationId == quotationId &&
                                           f.ConfirmedAt == null &&
                                           f.ExpiresAt > DateTime.UtcNow)
                               .OrderByDescending(f => f.CreatedAt)
                               .FirstOrDefaultAsync();

            return fc == null
                ? null
                : $"{_nav.BaseUri.TrimEnd('/')}/confirmfreight?token={fc.Token}";
        }
        /// Crea (o reutiliza) un token y devuelve el link
        /// Crea un link; si NO pides uno nuevo reutiliza el existente activo.
        public async Task<string> CreateLinkAsync(int quotationId, bool forceNew = false)
        {
            await using var ctx = await _ctxFactory.CreateDbContextAsync();  // ← aquí

            FreightConfirmation? fc = null;

            if (!forceNew)
            {
                 fc = await ctx.FreightConfirmations
                                   .Where(f => f.FreightQuotationId == quotationId &&
                                           f.ConfirmedAt == null &&
                                           f.ExpiresAt > DateTime.UtcNow)
                               .FirstOrDefaultAsync();
            }
            else
            {
                // Invalidar tokens anteriores activos al forzar uno nuevo
                var oldTokens = await ctx.FreightConfirmations
                    .Where(f => f.FreightQuotationId == quotationId &&
                                f.ConfirmedAt == null &&
                                f.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();
                foreach (var old in oldTokens)
                    old.ExpiresAt = DateTime.UtcNow;
            }

            if (fc == null)                          // no había, o forceNew = true
            {
                fc = new FreightConfirmation
                {
                    FreightQuotationId = quotationId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                ctx.FreightConfirmations.Add(fc);
                await ctx.SaveChangesAsync();

            }

            return $"{_nav.BaseUri.TrimEnd('/')}/confirmfreight?token={fc.Token}";
        }

        ///  ←  **EL MÉTODO QUE FALTABA**
        public async Task<bool> ConfirmAsync(string token)
        {
            await using var ctx = await _ctxFactory.CreateDbContextAsync();

            // 1) Trae la confirmación *con* tracking y la cotización enlazada
            var fc = await ctx.FreightConfirmations
                              .AsTracking()
                              .Include(f => f.FreightQuotation)
                              .SingleOrDefaultAsync(f => f.Token == token);

            if (fc == null || fc.IsConfirmed || fc.ExpiresAt < DateTime.UtcNow)
                return false;

            // 2) Marca confirmación + expira el enlace
            fc.ConfirmedAt = DateTime.UtcNow;
            fc.ExpiresAt = DateTime.UtcNow;
            fc.FreightQuotation.Status = (int)FreightStatus.Delivered;
            fc.FreightQuotation.UpdatedDate = DateTime.UtcNow;

            // 3) Guarda TODO en un solo SaveChanges
            var rows = await ctx.SaveChangesAsync();
            _logger.LogInformation("FreightConfirmation token={Token} confirmado, rows={Rows}", token, rows);

            return rows > 0;
        }

        // FreightConfirmationService.cs
        public async Task<bool> IsTokenValidAsync(string token)
        {
            await using var ctx = await _ctxFactory.CreateDbContextAsync();
            return await ctx.FreightConfirmations
                            .AnyAsync(f => f.Token == token &&
                                           f.ConfirmedAt == null &&
                                           f.ExpiresAt > DateTime.UtcNow);
        }

    }
}
