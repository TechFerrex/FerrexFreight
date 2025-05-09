﻿using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FerrexWeb.Services
{
    public class QuotationService
    {
        private readonly ApplicationDbContext _dbContext;

        public QuotationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Quotation>> GetQuotationsAsync()
        {
            return _dbContext.Quotations
                .Include(q => q.QuotedItems)
                .Include(q => q.QuotationNumber)
                .ToListAsync();
        }

        public async Task<Quotation> GetQuotationByIdAsync(int id)
        {
            return await _dbContext.Quotations
                .Include(q => q.QuotedItems)
                    .ThenInclude(qi => qi.Product)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task SaveQuotationAsync(Quotation quotation)
        {
            _dbContext.Quotations.Add(quotation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveQuotationNumberAsync(QuotationNumber quotationNumber)
        {
            _dbContext.QuotationNumbers.Add(quotationNumber);
            await _dbContext.SaveChangesAsync();
        }
        public async Task SaveQuotationDetailsAsync(List<QuotationDetail> quotationDetails)
        {
            // Disconnect all Product navigation properties before saving
            foreach (var detail in quotationDetails)
            {
                // Store the custom description if it exists
                if (string.IsNullOrEmpty(detail.CustomDescription) && detail.Product != null)
                {
                    detail.CustomDescription = detail.Product.DescProducto;
                }

                // Explicitly set the navigation property to null
                detail.Product = null;
            }

            _dbContext.QuotationDetails.AddRange(quotationDetails);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Quotation>> GetQuotationsByUserIdAsync(int userId)
        {
            return await _dbContext.Quotations
                .Where(q => q.UserID == userId)
                .Include(q => q.QuotedItems)
                .ToListAsync();
        }
        public async Task UpdateQuotationAsync(Quotation quotation)
        {
            _dbContext.Quotations.Update(quotation);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteQuotationAsync(int quotationId)
        {
            // Primero, obtenemos la cotización junto con sus detalles
            var quotation = await _dbContext.Quotations
                .Include(q => q.QuotedItems)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation != null)
            {
                // Eliminamos los detalles asociados a la cotización
                _dbContext.QuotationDetails.RemoveRange(quotation.QuotedItems);

                // Eliminamos la cotización
                _dbContext.Quotations.Remove(quotation);

                // Guardamos los cambios en la base de datos
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Quotation>> GetAllQuotationsAsync()
        {
            return await _dbContext.Quotations
                .Include(q => q.QuotedItems)
                .ToListAsync();
        }

        public async Task DeleteOldestQuotationForUserAsync(int userId)
        {
            // Traemos todas las cotizaciones del usuario ordenadas por fecha ascendente.
            var oldest = await _dbContext.Quotations
                .Where(q => q.UserID == userId && !q.IsOrdered) // Si quieres solo contar las que aún no se han convertido en orden
                .OrderBy(q => q.Date)
                .FirstOrDefaultAsync();

            if (oldest != null)
            {
                // Primero eliminamos los detalles
                _dbContext.QuotationDetails.RemoveRange(oldest.QuotedItems);

                // Luego eliminamos la cotización
                _dbContext.Quotations.Remove(oldest);


                await _dbContext.SaveChangesAsync();
                //// Eliminamos la cotización
                //await DeleteQuotationAsync(oldestQuotation.Id);
            }
        }



    }

}
