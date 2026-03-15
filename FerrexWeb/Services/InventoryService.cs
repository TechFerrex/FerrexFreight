using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FerrexWeb.Services
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Cities ──

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Include(c => c.InventoryPoints)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<City?> GetCityByIdAsync(int id)
        {
            return await _context.Cities
                .Include(c => c.InventoryPoints)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCityAsync(City city)
        {
            city.CreatedDate = DateTime.Now;
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCityAsync(City city)
        {
            var existing = await _context.Cities.FindAsync(city.Id);
            if (existing != null)
            {
                existing.Name = city.Name;
                existing.Department = city.Department;
                existing.IsActive = city.IsActive;
                _context.Cities.Update(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteCityAsync(int id)
        {
            var city = await _context.Cities
                .Include(c => c.InventoryPoints)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null) return false;

            if (city.InventoryPoints.Any())
                return false; // Cannot delete city with points

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Inventory Points ──

        public async Task<List<InventoryPoint>> GetAllPointsAsync()
        {
            return await _context.InventoryPoints
                .Include(ip => ip.City)
                .Include(ip => ip.Items)
                .OrderBy(ip => ip.City.Name)
                .ThenBy(ip => ip.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<InventoryPoint>> GetPointsByCityIdAsync(int cityId)
        {
            return await _context.InventoryPoints
                .Include(ip => ip.City)
                .Include(ip => ip.Items)
                .Where(ip => ip.CityId == cityId)
                .OrderBy(ip => ip.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InventoryPoint?> GetPointByIdAsync(int id)
        {
            return await _context.InventoryPoints
                .Include(ip => ip.City)
                .Include(ip => ip.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(ip => ip.Id == id);
        }

        public async Task AddPointAsync(InventoryPoint point)
        {
            point.CreatedDate = DateTime.Now;
            _context.InventoryPoints.Add(point);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePointAsync(InventoryPoint point)
        {
            var existing = await _context.InventoryPoints.FindAsync(point.Id);
            if (existing != null)
            {
                existing.Name = point.Name;
                existing.Address = point.Address;
                existing.Phone = point.Phone;
                existing.CityId = point.CityId;
                existing.IsActive = point.IsActive;
                _context.InventoryPoints.Update(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeletePointAsync(int id)
        {
            var point = await _context.InventoryPoints
                .Include(ip => ip.Items)
                .FirstOrDefaultAsync(ip => ip.Id == id);

            if (point == null) return false;

            if (point.Items.Any())
                return false; // Cannot delete point with items

            _context.InventoryPoints.Remove(point);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Inventory Items ──

        public async Task<List<InventoryItem>> GetItemsByPointIdAsync(int pointId)
        {
            return await _context.InventoryItems
                .Include(ii => ii.Product)
                .Include(ii => ii.InventoryPoint)
                    .ThenInclude(ip => ip.City)
                .Where(ii => ii.InventoryPointId == pointId)
                .OrderBy(ii => ii.Product.DescProducto)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InventoryItem?> GetItemByIdAsync(int id)
        {
            return await _context.InventoryItems
                .Include(ii => ii.Product)
                .Include(ii => ii.InventoryPoint)
                    .ThenInclude(ip => ip.City)
                .Include(ii => ii.Movements.OrderByDescending(m => m.MovementDate))
                .FirstOrDefaultAsync(ii => ii.Id == id);
        }

        public async Task<bool> ProductExistsInPointAsync(int pointId, int productId)
        {
            return await _context.InventoryItems
                .AnyAsync(ii => ii.InventoryPointId == pointId && ii.ProductId == productId);
        }

        public async Task AddItemAsync(InventoryItem item, int? userId = null)
        {
            item.LastUpdated = DateTime.Now;
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Record initial entry movement
            if (item.Quantity > 0)
            {
                var movement = new InventoryMovement
                {
                    InventoryItemId = item.Id,
                    MovementType = (int)Models.MovementType.Entry,
                    Quantity = item.Quantity,
                    PreviousQuantity = 0,
                    NewQuantity = item.Quantity,
                    MovementDate = DateTime.Now,
                    Description = "Stock inicial",
                    UserId = userId
                };
                _context.InventoryMovements.Add(movement);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockAsync(int itemId, int movementType, int quantity, string? description, int? userId = null)
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null) return;

            int previousQuantity = item.Quantity;
            int newQuantity = previousQuantity;

            switch ((MovementType)movementType)
            {
                case Models.MovementType.Entry:
                    newQuantity = previousQuantity + quantity;
                    break;
                case Models.MovementType.Exit:
                    newQuantity = Math.Max(0, previousQuantity - quantity);
                    break;
                case Models.MovementType.Adjustment:
                    newQuantity = quantity; // Direct set
                    break;
            }

            item.Quantity = newQuantity;
            item.LastUpdated = DateTime.Now;
            _context.InventoryItems.Update(item);

            var movement = new InventoryMovement
            {
                InventoryItemId = itemId,
                MovementType = movementType,
                Quantity = quantity,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                MovementDate = DateTime.Now,
                Description = description,
                UserId = userId
            };
            _context.InventoryMovements.Add(movement);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _context.InventoryItems
                .Include(ii => ii.Movements)
                .FirstOrDefaultAsync(ii => ii.Id == id);

            if (item == null) return false;

            // Remove movements first
            _context.InventoryMovements.RemoveRange(item.Movements);
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Movements ──

        public async Task<List<InventoryMovement>> GetMovementsByItemIdAsync(int itemId)
        {
            return await _context.InventoryMovements
                .Include(im => im.User)
                .Where(im => im.InventoryItemId == itemId)
                .OrderByDescending(im => im.MovementDate)
                .AsNoTracking()
                .ToListAsync();
        }

        // ── Transfers ──

        public async Task TransferStockAsync(int sourceItemId, int destinationPointId, int quantity, string? description, int? userId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sourceItem = await _context.InventoryItems.FindAsync(sourceItemId);
                if (sourceItem == null || sourceItem.Quantity < quantity)
                    throw new InvalidOperationException("Stock insuficiente para transferir.");

                // Update source
                int sourcePrev = sourceItem.Quantity;
                sourceItem.Quantity -= quantity;
                sourceItem.LastUpdated = DateTime.Now;
                _context.InventoryItems.Update(sourceItem);

                // Source movement (TransferOut)
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    InventoryItemId = sourceItemId,
                    MovementType = (int)Models.MovementType.TransferOut,
                    Quantity = quantity,
                    PreviousQuantity = sourcePrev,
                    NewQuantity = sourceItem.Quantity,
                    MovementDate = DateTime.Now,
                    Description = description,
                    UserId = userId,
                    TransferFromPointId = sourceItem.InventoryPointId,
                    TransferToPointId = destinationPointId
                });

                // Find or create destination item
                var destItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(ii => ii.InventoryPointId == destinationPointId && ii.ProductId == sourceItem.ProductId);

                if (destItem == null)
                {
                    destItem = new InventoryItem
                    {
                        InventoryPointId = destinationPointId,
                        ProductId = sourceItem.ProductId,
                        Quantity = quantity,
                        MinStock = sourceItem.MinStock,
                        LastUpdated = DateTime.Now
                    };
                    _context.InventoryItems.Add(destItem);
                    await _context.SaveChangesAsync(); // To get destItem.Id

                    _context.InventoryMovements.Add(new InventoryMovement
                    {
                        InventoryItemId = destItem.Id,
                        MovementType = (int)Models.MovementType.TransferIn,
                        Quantity = quantity,
                        PreviousQuantity = 0,
                        NewQuantity = quantity,
                        MovementDate = DateTime.Now,
                        Description = description,
                        UserId = userId,
                        TransferFromPointId = sourceItem.InventoryPointId,
                        TransferToPointId = destinationPointId
                    });
                }
                else
                {
                    int destPrev = destItem.Quantity;
                    destItem.Quantity += quantity;
                    destItem.LastUpdated = DateTime.Now;
                    _context.InventoryItems.Update(destItem);

                    _context.InventoryMovements.Add(new InventoryMovement
                    {
                        InventoryItemId = destItem.Id,
                        MovementType = (int)Models.MovementType.TransferIn,
                        Quantity = quantity,
                        PreviousQuantity = destPrev,
                        NewQuantity = destItem.Quantity,
                        MovementDate = DateTime.Now,
                        Description = description,
                        UserId = userId,
                        TransferFromPointId = sourceItem.InventoryPointId,
                        TransferToPointId = destinationPointId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ── Dashboard ──

        public async Task<List<InventoryItem>> GetLowStockItemsAsync()
        {
            return await _context.InventoryItems
                .Include(ii => ii.Product)
                .Include(ii => ii.InventoryPoint)
                    .ThenInclude(ip => ip.City)
                .Where(ii => ii.Quantity <= ii.MinStock)
                .OrderBy(ii => ii.Quantity)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalInventoryQuantityAsync()
        {
            return await _context.InventoryItems.SumAsync(ii => ii.Quantity);
        }

        public async Task<int> GetActivePointsCountAsync()
        {
            return await _context.InventoryPoints.CountAsync(ip => ip.IsActive);
        }

        public async Task<int> GetActiveCitiesCountAsync()
        {
            return await _context.Cities.CountAsync(c => c.IsActive);
        }

        public async Task<int> GetLowStockAlertCountAsync()
        {
            return await _context.InventoryItems.CountAsync(ii => ii.Quantity <= ii.MinStock);
        }

        // ── Helpers ──

        public async Task<List<Products>> SearchProductsAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return new List<Products>();

            return await _context.Products
                .Where(p => p.DescProducto.Contains(search) || p.Codigo.Contains(search))
                .OrderBy(p => p.DescProducto)
                .Take(20)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
