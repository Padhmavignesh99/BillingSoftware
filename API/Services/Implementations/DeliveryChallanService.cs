using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class DeliveryChallanService : IDeliveryChallanService
    {
        private readonly ApplicationDbContext _db;
        public DeliveryChallanService(ApplicationDbContext db) { _db = db; }

        public async Task<(List<DeliveryChallanResponse> Items, int Total)> GetAllAsync(int tenantId, PaginationRequest p)
        {
            var query = _db.DeliveryChallans
                .Include(c => c.Customer)
                .Include(c => c.Invoice)
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .OrderByDescending(c => c.ChallanDate);

            var total = await query.CountAsync();
            var page = p.Page < 1 ? 1 : p.Page;
            var pageSize = p.PageSize < 1 ? 20 : p.PageSize;
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(c => MapToResponse(c)).ToListAsync();
            return (items, total);
        }

        public async Task<DeliveryChallanResponse?> GetByIdAsync(int id, int tenantId)
        {
            var c = await _db.DeliveryChallans
                .Include(x => x.Customer)
                .Include(x => x.Invoice)
                .Include(x => x.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return c == null ? null : MapToResponse(c);
        }

        public async Task<DeliveryChallanResponse> CreateAsync(CreateDeliveryChallanRequest r, int tenantId)
        {
            var num = await GetNextChallanNumberAsync(tenantId);
            var challan = new DeliveryChallan
            {
                TenantId = tenantId,
                ChallanNumber = num,
                CustomerId = r.CustomerId,
                InvoiceId = r.InvoiceId,
                BranchId = r.BranchId,
                DeliveryAddress = r.DeliveryAddress,
                TransportMode = r.TransportMode,
                VehicleNumber = r.VehicleNumber,
                DriverName = r.DriverName,
                LRNumber = r.LRNumber,
                Notes = r.Notes,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Items = r.Items.Select(i => new DeliveryChallanItem
                {
                    TenantId = tenantId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Notes = i.Notes,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }).ToList()
            };
            _db.DeliveryChallans.Add(challan);
            await _db.SaveChangesAsync();
            return (await GetByIdAsync(challan.Id, tenantId))!;
        }

        public async Task<DeliveryChallanResponse> UpdateStatusAsync(int id, string status, int tenantId)
        {
            var c = await _db.DeliveryChallans.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Challan not found");
            c.Status = status;
            c.UpdatedAt = DateTime.UtcNow;
            if (status == "Dispatched") c.DispatchedAt = DateTime.UtcNow;
            if (status == "Delivered") c.DeliveredAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (await GetByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var c = await _db.DeliveryChallans.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (c == null) return false;
            c.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<InvoiceResponse> ConvertToInvoiceAsync(int id, int tenantId)
        {
            var challan = await _db.DeliveryChallans
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Challan not found");

            var invoiceService = _db; // We'll create invoice directly via DB
            var invNumber = $"INV-{DateTime.Now:yyyyMMdd-HHmmss}";
            var invoice = new Invoice
            {
                TenantId = tenantId,
                InvoiceNumber = invNumber,
                CustomerId = challan.CustomerId,
                InvoiceDate = DateTime.UtcNow,
                Status = Models.Enums.InvoiceStatus.Pending,
                BranchId = challan.BranchId,
                Notes = $"Converted from Challan {challan.ChallanNumber}",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            decimal subtotal = 0;
            foreach (var item in challan.Items)
            {
                var price = item.Product?.SellingPrice ?? 0;
                var total = price * item.Quantity;
                subtotal += total;
                invoice.Items.Add(new InvoiceItem
                {
                    TenantId = tenantId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = price,
                    TotalPrice = total,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            invoice.SubTotal = subtotal;
            invoice.TotalAmount = subtotal;
            invoice.DueAmount = subtotal;
            _db.Invoices.Add(invoice);

            challan.InvoiceId = invoice.Id;
            challan.Status = "Delivered";
            await _db.SaveChangesAsync();

            return new InvoiceResponse { Id = invoice.Id, InvoiceNumber = invoice.InvoiceNumber, TotalAmount = invoice.TotalAmount };
        }

        public async Task<string> GetNextChallanNumberAsync(int tenantId)
        {
            var count = await _db.DeliveryChallans.CountAsync(c => c.TenantId == tenantId) + 1;
            return $"DC-{DateTime.Now:yyyy}-{count:D4}";
        }

        private static DeliveryChallanResponse MapToResponse(DeliveryChallan c) => new()
        {
            Id = c.Id,
            ChallanNumber = c.ChallanNumber,
            ChallanDate = c.ChallanDate,
            CustomerId = c.CustomerId,
            CustomerName = c.Customer?.Name,
            InvoiceId = c.InvoiceId,
            InvoiceNumber = c.Invoice?.InvoiceNumber,
            Status = c.Status,
            DeliveryAddress = c.DeliveryAddress,
            TransportMode = c.TransportMode,
            VehicleNumber = c.VehicleNumber,
            DriverName = c.DriverName,
            LRNumber = c.LRNumber,
            Notes = c.Notes,
            DispatchedAt = c.DispatchedAt,
            DeliveredAt = c.DeliveredAt,
            CreatedAt = c.CreatedAt,
            Items = c.Items?.Select(i => new ChallanItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Notes = i.Notes
            }).ToList() ?? new()
        };
    }
}
