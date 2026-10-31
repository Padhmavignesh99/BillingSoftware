using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class ReorderService : IReorderService
    {
        private readonly ApplicationDbContext _db;
        public ReorderService(ApplicationDbContext db) { _db = db; }

        public async Task<List<ReorderSuggestion>> GetReorderSuggestionsAsync(int tenantId)
        {
            var products = await _db.Products
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && !p.IsService)
                .ToListAsync();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var salesData = await _db.InvoiceItems
                .Include(i => i.Invoice)
                .Where(i => i.Invoice != null && i.Invoice.TenantId == tenantId
                    && i.Invoice.InvoiceDate >= thirtyDaysAgo && !i.Invoice.IsDeleted)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // Last purchase prices
            var lastPurchases = await _db.PurchaseItems
                .Include(pi => pi.Purchase)
                .Where(pi => pi.Purchase != null && pi.Purchase.TenantId == tenantId && !pi.Purchase.IsDeleted)
                .GroupBy(pi => pi.ProductId)
                .Select(g => new { ProductId = g.Key, LastPrice = g.OrderByDescending(x => x.Id).First().UnitPrice, SupplierId = g.OrderByDescending(x => x.Id).First().Purchase!.SupplierId })
                .ToListAsync();

            var supplierNames = await _db.Suppliers
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            var suggestions = new List<ReorderSuggestion>();

            foreach (var product in products)
            {
                var sales = salesData.FirstOrDefault(s => s.ProductId == product.Id);
                var lastPurch = lastPurchases.FirstOrDefault(lp => lp.ProductId == product.Id);

                var avgDailySales = sales != null ? (double)sales.TotalSold / 30.0 : 0;
                var daysOfStockLeft = avgDailySales > 0 ? (double)product.CurrentStock / avgDailySales : 999;
                var reorderPoint = product.ReorderLevel > 0 ? product.ReorderLevel : product.MinStockLevel ?? 0;

                // Only suggest if stock is at or below reorder level, or < 7 days stock
                if (product.CurrentStock > reorderPoint && daysOfStockLeft >= 14) continue;

                var suggestedQty = avgDailySales > 0
                    ? Math.Max((decimal)(avgDailySales * 30) - product.CurrentStock, reorderPoint * 2)
                    : reorderPoint * 2;

                if (suggestedQty <= 0) suggestedQty = 10;

                var urgency = daysOfStockLeft <= 0 ? "Critical"
                    : daysOfStockLeft <= 3 ? "Urgent"
                    : daysOfStockLeft <= 7 ? "Watch"
                    : "Normal";

                var lastPrice = lastPurch?.LastPrice ?? product.PurchasePrice;
                supplierNames.TryGetValue(lastPurch?.SupplierId ?? 0, out var supplierName);

                suggestions.Add(new ReorderSuggestion
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    CurrentStock = product.CurrentStock,
                    ReorderLevel = reorderPoint,
                    AvgDailySales = (decimal)Math.Round(avgDailySales, 2),
                    DaysOfStockLeft = (decimal)Math.Round(daysOfStockLeft, 1),
                    SuggestedOrderQty = Math.Ceiling(suggestedQty),
                    PreferredSupplierId = lastPurch?.SupplierId,
                    SupplierName = supplierName,
                    LastPurchasePrice = lastPrice,
                    EstimatedCost = Math.Ceiling(suggestedQty) * lastPrice,
                    Urgency = urgency
                });
            }

            return suggestions.OrderBy(s => s.DaysOfStockLeft).ToList();
        }

        public async Task<object> CreatePurchaseOrderFromSuggestionsAsync(List<int> productIds, int tenantId)
        {
            var suggestions = await GetReorderSuggestionsAsync(tenantId);
            var filtered = suggestions.Where(s => productIds.Contains(s.ProductId)).ToList();

            var grouped = filtered
                .Where(s => s.PreferredSupplierId.HasValue)
                .GroupBy(s => s.PreferredSupplierId!.Value);

            var poNumbers = new List<string>();

            foreach (var group in grouped)
            {
                var poNum = $"PO-{DateTime.Now:yyyyMMdd}-{group.Key}";
                var po = new Models.Entities.PurchaseOrder
                {
                    TenantId = tenantId,
                    OrderNumber = poNum,
                    SupplierId = group.Key,
                    OrderDate = DateTime.UtcNow,
                    Status = Models.Enums.PurchaseOrderStatus.Draft,
                    TotalAmount = group.Sum(s => s.EstimatedCost),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Items = group.Select(s => new Models.Entities.PurchaseOrderItem
                    {
                        TenantId = tenantId,
                        ProductId = s.ProductId,
                        Quantity = s.SuggestedOrderQty,
                        UnitPrice = s.LastPurchasePrice ?? 0,
                        TotalPrice = s.EstimatedCost,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }).ToList()
                };
                _db.PurchaseOrders.Add(po);
                poNumbers.Add(poNum);
            }

            await _db.SaveChangesAsync();
            return new { CreatedPOs = poNumbers, Count = poNumbers.Count };
        }
    }
}
