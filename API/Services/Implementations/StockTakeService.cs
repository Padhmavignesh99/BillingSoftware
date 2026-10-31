using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class StockTakeService : IStockTakeService
    {
        private readonly ApplicationDbContext _db;
        public StockTakeService(ApplicationDbContext db) { _db = db; }

        public async Task<(List<StockTakeSessionResponse> Items, int Total)> GetSessionsAsync(int tenantId, PaginationRequest p)
        {
            var query = _db.StockTakeSessions
                .Include(s => s.Branch)
                .Include(s => s.Warehouse)
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt);

            var total = await query.CountAsync();
            var page = p.Page < 1 ? 1 : p.Page;
            var pageSize = p.PageSize < 1 ? 20 : p.PageSize;
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => MapSessionToResponse(s, new List<StockTakeItem>())).ToListAsync();
            return (items, total);
        }

        public async Task<StockTakeSessionResponse?> GetSessionByIdAsync(int id, int tenantId)
        {
            var s = await _db.StockTakeSessions
                .Include(x => x.Branch)
                .Include(x => x.Warehouse)
                .Include(x => x.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return s == null ? null : MapSessionToResponse(s, s.Items.ToList());
        }

        public async Task<StockTakeSessionResponse> CreateSessionAsync(CreateStockTakeRequest r, int tenantId)
        {
            var count = await _db.StockTakeSessions.CountAsync(s => s.TenantId == tenantId) + 1;
            var session = new StockTakeSession
            {
                TenantId = tenantId,
                SessionNumber = $"ST-{DateTime.Now:yyyy}-{count:D4}",
                Name = r.Name,
                BranchId = r.BranchId,
                WarehouseId = r.WarehouseId,
                Notes = r.Notes,
                Status = "InProgress",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var productsQuery = _db.Products.Where(p => p.TenantId == tenantId && !p.IsDeleted && p.IsActive);
            if (r.ProductIds != null && r.ProductIds.Any())
                productsQuery = productsQuery.Where(p => r.ProductIds.Contains(p.Id));

            var products = await productsQuery.ToListAsync();
            session.TotalProducts = products.Count;

            session.Items = products.Select(p => new StockTakeItem
            {
                TenantId = tenantId,
                ProductId = p.Id,
                ProductName = p.Name,
                SKU = p.SKU,
                Barcode = p.Barcode,
                SystemStock = p.CurrentStock,
                CountedStock = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            _db.StockTakeSessions.Add(session);
            await _db.SaveChangesAsync();
            return (await GetSessionByIdAsync(session.Id, tenantId))!;
        }

        public async Task BulkUpdateItemsAsync(int sessionId, List<UpdateStockTakeItemRequest> items, int tenantId)
        {
            var session = await _db.StockTakeSessions
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Session not found");

            foreach (var update in items)
            {
                var item = session.Items.FirstOrDefault(i => i.Id == update.ItemId);
                if (item == null) continue;
                item.CountedStock = update.CountedStock;
                item.Discrepancy = update.CountedStock - item.SystemStock;
                item.Notes = update.Notes;
                item.UpdatedAt = DateTime.UtcNow;
            }

            session.CountedProducts = session.Items.Count(i => i.CountedStock.HasValue);
            session.DiscrepancyCount = session.Items.Count(i => i.CountedStock.HasValue && i.CountedStock != i.SystemStock);
            await _db.SaveChangesAsync();
        }

        public async Task<StockTakeSessionResponse> CompleteSessionAsync(int id, int tenantId)
        {
            var session = await _db.StockTakeSessions
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Session not found");

            session.Status = "Completed";
            session.CompletedAt = DateTime.UtcNow;
            session.DiscrepancyCount = session.Items.Count(i => i.CountedStock.HasValue && i.CountedStock != i.SystemStock);
            await _db.SaveChangesAsync();
            return (await GetSessionByIdAsync(id, tenantId))!;
        }

        public async Task<int> ApplyAdjustmentsAsync(int id, int tenantId)
        {
            var session = await _db.StockTakeSessions
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Session not found");

            var adjusted = 0;
            foreach (var item in session.Items.Where(i => i.CountedStock.HasValue && !i.IsAdjusted && i.CountedStock != i.SystemStock))
            {
                if (item.Product == null) continue;
                var diff = item.CountedStock!.Value - item.SystemStock;
                item.Product.CurrentStock = item.CountedStock.Value;

                _db.StockAdjustments.Add(new StockAdjustment
                {
                    TenantId = tenantId,
                    ProductId = item.ProductId,
                    AdjustmentNumber = $"SA-{DateTime.Now:yyyyMMddHHmmss}-{item.ProductId}",
                    QuantityAdjusted = diff,
                    Reason = $"Stock take adjustment: {session.SessionNumber}",
                    AdjustmentDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });

                item.IsAdjusted = true;
                adjusted++;
            }

            await _db.SaveChangesAsync();
            return adjusted;
        }

        public async Task<bool> DeleteSessionAsync(int id, int tenantId)
        {
            var s = await _db.StockTakeSessions.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (s == null) return false;
            s.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        private static StockTakeSessionResponse MapSessionToResponse(StockTakeSession s, List<StockTakeItem> items) => new()
        {
            Id = s.Id,
            SessionNumber = s.SessionNumber,
            Name = s.Name,
            BranchId = s.BranchId,
            BranchName = s.Branch?.Name,
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse?.Name,
            Status = s.Status,
            CompletedAt = s.CompletedAt,
            Notes = s.Notes,
            TotalProducts = s.TotalProducts,
            CountedProducts = s.CountedProducts,
            DiscrepancyCount = s.DiscrepancyCount,
            CreatedAt = s.CreatedAt,
            Items = items.Select(i => new StockTakeItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                SKU = i.SKU,
                Barcode = i.Barcode,
                SystemStock = i.SystemStock,
                CountedStock = i.CountedStock,
                Discrepancy = i.Discrepancy,
                IsAdjusted = i.IsAdjusted,
                Notes = i.Notes
            }).ToList()
        };
    }
}
