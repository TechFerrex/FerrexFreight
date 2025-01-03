using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FerrexWeb.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveOrderAsync(Order order)
        {
            _dbContext.Order.Add(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _dbContext.Order
                .Include(o => o.OrderedItems)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _dbContext.Order
                .Include(o => o.OrderedItems)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _dbContext.Order
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderedItems)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _dbContext.Order.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
