using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class RecurringInvoiceService : IRecurringInvoiceService
    {
        private readonly ApplicationDbContext _db;
        public RecurringInvoiceService(ApplicationDbContext db) { _db = db; }

        public async Task<(List<RecurringInvoiceResponse> Items, int Total)> GetAllAsync(int tenantId, PaginationRequest p)
        {
            var query = _db.RecurringInvoices
                .Include(r => r.Customer)
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var page = p.Page < 1 ? 1 : p.Page;
            var pageSize = p.PageSize < 1 ? 20 : p.PageSize;
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => MapToResponse(r)).ToListAsync();
            return (items, total);
        }

        public async Task<RecurringInvoiceResponse?> GetByIdAsync(int id, int tenantId)
        {
            var r = await _db.RecurringInvoices
                .Include(x => x.Customer)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return r == null ? null : MapToResponse(r);
        }

        public async Task<RecurringInvoiceResponse> CreateAsync(CreateRecurringInvoiceRequest r, int tenantId)
        {
            var rec = new RecurringInvoice
            {
                TenantId = tenantId,
                Name = r.Name,
                CustomerId = r.CustomerId,
                Frequency = r.Frequency,
                FrequencyValue = r.FrequencyValue,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                MaxRuns = r.MaxRuns,
                Notes = r.Notes,
                BranchId = r.BranchId,
                Status = "Active",
                NextRunDate = r.StartDate,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Items = r.Items.Select(i => new RecurringInvoiceItem
                {
                    TenantId = tenantId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount,
                    TaxPercentage = i.TaxPercentage,
                    TotalPrice = i.TotalPrice,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }).ToList()
            };

            rec.TotalAmount = rec.Items.Sum(i => i.TotalPrice);
            _db.RecurringInvoices.Add(rec);
            await _db.SaveChangesAsync();
            return (await GetByIdAsync(rec.Id, tenantId))!;
        }

        public async Task<RecurringInvoiceResponse> UpdateAsync(int id, CreateRecurringInvoiceRequest r, int tenantId)
        {
            var rec = await _db.RecurringInvoices
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Recurring invoice not found");

            rec.Name = r.Name;
            rec.CustomerId = r.CustomerId;
            rec.Frequency = r.Frequency;
            rec.FrequencyValue = r.FrequencyValue;
            rec.EndDate = r.EndDate;
            rec.MaxRuns = r.MaxRuns;
            rec.Notes = r.Notes;
            rec.UpdatedAt = DateTime.UtcNow;

            _db.RecurringInvoiceItems.RemoveRange(rec.Items);
            rec.Items = r.Items.Select(i => new RecurringInvoiceItem
            {
                TenantId = tenantId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                TaxPercentage = i.TaxPercentage,
                TotalPrice = i.TotalPrice,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList();
            rec.TotalAmount = rec.Items.Sum(i => i.TotalPrice);

            await _db.SaveChangesAsync();
            return (await GetByIdAsync(id, tenantId))!;
        }

        public async Task<RecurringInvoiceResponse> ToggleStatusAsync(int id, int tenantId)
        {
            var rec = await _db.RecurringInvoices.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Not found");
            rec.Status = rec.Status == "Active" ? "Paused" : "Active";
            rec.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (await GetByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var rec = await _db.RecurringInvoices.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (rec == null) return false;
            rec.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<InvoiceResponse> GenerateNowAsync(int id, int tenantId)
        {
            var rec = await _db.RecurringInvoices
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Recurring invoice not found");

            var invNum = $"INV-{DateTime.Now:yyyyMMdd-HHmmss}";
            var invoice = new Invoice
            {
                TenantId = tenantId,
                InvoiceNumber = invNum,
                CustomerId = rec.CustomerId,
                InvoiceDate = DateTime.UtcNow,
                Status = Models.Enums.InvoiceStatus.Pending,
                BranchId = rec.BranchId,
                Notes = $"Generated from recurring template: {rec.Name}",
                SubTotal = rec.Items.Sum(i => i.TotalPrice - i.TaxAmount),
                TaxAmount = rec.Items.Sum(i => i.TaxAmount),
                DiscountAmount = rec.Items.Sum(i => i.DiscountAmount),
                TotalAmount = rec.TotalAmount,
                DueAmount = rec.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Items = rec.Items.Select(i => new InvoiceItem
                {
                    TenantId = tenantId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount,
                    TaxPercentage = i.TaxPercentage,
                    TotalPrice = i.TotalPrice,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }).ToList()
            };

            _db.Invoices.Add(invoice);

            rec.LastRunDate = DateTime.UtcNow;
            rec.RunCount++;
            rec.NextRunDate = CalculateNextRun(rec);
            if (rec.MaxRuns.HasValue && rec.RunCount >= rec.MaxRuns)
                rec.Status = "Completed";

            await _db.SaveChangesAsync();
            return new InvoiceResponse { Id = invoice.Id, InvoiceNumber = invoice.InvoiceNumber, TotalAmount = invoice.TotalAmount };
        }

        private static DateTime? CalculateNextRun(RecurringInvoice rec)
        {
            var from = rec.LastRunDate ?? rec.StartDate;
            return rec.Frequency switch
            {
                "Daily"     => from.AddDays(rec.FrequencyValue),
                "Weekly"    => from.AddDays(rec.FrequencyValue * 7),
                "Monthly"   => from.AddMonths(rec.FrequencyValue),
                "Quarterly" => from.AddMonths(rec.FrequencyValue * 3),
                "Yearly"    => from.AddYears(rec.FrequencyValue),
                _           => from.AddMonths(1)
            };
        }

        private static RecurringInvoiceResponse MapToResponse(RecurringInvoice r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name,
            Frequency = r.Frequency,
            FrequencyValue = r.FrequencyValue,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            NextRunDate = r.NextRunDate,
            LastRunDate = r.LastRunDate,
            RunCount = r.RunCount,
            MaxRuns = r.MaxRuns,
            Status = r.Status,
            Notes = r.Notes,
            TotalAmount = r.TotalAmount,
            CreatedAt = r.CreatedAt,
            Items = r.Items?.Select(i => new RecurringItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList() ?? new()
        };
    }
}

