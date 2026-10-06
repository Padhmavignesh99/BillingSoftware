using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== INVENTORY SERVICE ====================
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        public InventoryService(ApplicationDbContext context) { _context = context; }

        public async Task<List<InventoryResponse>> GetAllInventoryAsync(int tenantId) =>
            await _context.Products.Where(p => p.TenantId == tenantId).Select(p => new InventoryResponse { Id = p.Id, ProductId = p.Id, ProductName = p.Name, SKU = p.SKU, Quantity = p.CurrentStock, ReorderLevel = p.MinStockLevel }).ToListAsync();

        public async Task<InventoryResponse> AdjustStockAsync(int productId, AdjustStockRequest request, int tenantId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Product not found");
            var previousStock = product.CurrentStock;
            product.CurrentStock += request.Quantity;
            _context.StockTransactions.Add(new StockTransaction { ProductId = productId, TransactionType = StockTransactionType.Adjustment, Quantity = request.Quantity, PreviousStock = previousStock, NewStock = product.CurrentStock, Notes = request.Reason, TenantId = tenantId });
            await _context.SaveChangesAsync();
            return new InventoryResponse { Id = product.Id, ProductId = product.Id, ProductName = product.Name, SKU = product.SKU, Quantity = product.CurrentStock, ReorderLevel = product.MinStockLevel };
        }

        public async Task<List<ProductResponse>> GetLowStockProductsAsync(int tenantId) =>
            await _context.Products.Include(p => p.Category).Where(p => p.TenantId == tenantId && p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel.Value)
                .Select(p => new ProductResponse { Id = p.Id, Name = p.Name, SKU = p.SKU, CurrentStock = p.CurrentStock, MinStockLevel = p.MinStockLevel, CategoryName = p.Category != null ? p.Category.Name : null }).ToListAsync();

        public async Task<List<StockHistoryResponse>> GetStockHistoryAsync(int productId, int tenantId) =>
            await _context.StockTransactions.Where(s => s.ProductId == productId && s.TenantId == tenantId).OrderByDescending(s => s.TransactionDate)
                .Select(s => new StockHistoryResponse { Id = s.Id, TransactionType = s.TransactionType.ToString(), Quantity = s.Quantity, PreviousStock = s.PreviousStock, NewStock = s.NewStock, ReferenceNumber = s.ReferenceNumber, Notes = s.Notes, TransactionDate = s.TransactionDate, CreatedBy = s.CreatedBy }).ToListAsync();
    }

    // ==================== STOCK SERVICE ====================
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;
        public StockService(ApplicationDbContext context) { _context = context; }

        public async Task<List<StockTransaction>> GetStockTransactionsAsync(int tenantId, PaginationRequest pagination) =>
            await _context.StockTransactions.Include(s => s.Product).Where(s => s.TenantId == tenantId).OrderByDescending(s => s.TransactionDate).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();

        public async Task<StockAdjustment> CreateAdjustmentAsync(CreateStockAdjustmentRequest request, int tenantId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Product not found");
            var adj = new StockAdjustment { AdjustmentNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}", ProductId = request.ProductId, QuantityAdjusted = request.QuantityAdjusted, Reason = request.Reason, Notes = request.Notes, BranchId = request.BranchId, TenantId = tenantId };
            _context.StockAdjustments.Add(adj);
            var prev = product.CurrentStock; product.CurrentStock += request.QuantityAdjusted;
            _context.StockTransactions.Add(new StockTransaction { ProductId = request.ProductId, TransactionType = StockTransactionType.Adjustment, Quantity = request.QuantityAdjusted, PreviousStock = prev, NewStock = product.CurrentStock, Notes = request.Reason, TenantId = tenantId });
            await _context.SaveChangesAsync();
            return adj;
        }

        public async Task<List<StockAdjustment>> GetAdjustmentsAsync(int tenantId) => await _context.StockAdjustments.Include(a => a.Product).Where(a => a.TenantId == tenantId).OrderByDescending(a => a.AdjustmentDate).ToListAsync();

        public async Task<StockTransfer> CreateTransferAsync(CreateStockTransferRequest request, int tenantId)
        {
            var transfer = new StockTransfer { TransferNumber = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}", ProductId = request.ProductId, FromBranchId = request.FromBranchId, ToBranchId = request.ToBranchId, FromWarehouseId = request.FromWarehouseId, ToWarehouseId = request.ToWarehouseId, Quantity = request.Quantity, Notes = request.Notes, TenantId = tenantId };
            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<List<StockTransfer>> GetTransfersAsync(int tenantId) => await _context.StockTransfers.Include(t => t.Product).Where(t => t.TenantId == tenantId).OrderByDescending(t => t.TransferDate).ToListAsync();

        public async Task<StockTransfer> UpdateTransferStatusAsync(int id, string status)
        { var t = await _context.StockTransfers.FindAsync(id) ?? throw new KeyNotFoundException("Transfer not found"); t.Status = status; await _context.SaveChangesAsync(); return t; }
    }

    // ==================== WAREHOUSE SERVICE ====================
    public class WarehouseService : IWarehouseService
    {
        private readonly ApplicationDbContext _context;
        public WarehouseService(ApplicationDbContext context) { _context = context; }
        public async Task<List<Warehouse>> GetAllWarehousesAsync(int tenantId) => await _context.Warehouses.Where(w => w.TenantId == tenantId && !w.IsDeleted).ToListAsync();
        public async Task<Warehouse?> GetWarehouseByIdAsync(int id) => await _context.Warehouses.FindAsync(id);
        public async Task<Warehouse> CreateWarehouseAsync(CreateWarehouseRequest request, int tenantId) { var w = new Warehouse { Name = request.Name, Code = request.Code, Address = request.Address, City = request.City, State = request.State, Phone = request.Phone, ManagerName = request.ManagerName, BranchId = request.BranchId, TenantId = tenantId }; _context.Warehouses.Add(w); await _context.SaveChangesAsync(); return w; }
        public async Task<Warehouse> UpdateWarehouseAsync(int id, UpdateWarehouseRequest request) { var w = await _context.Warehouses.FindAsync(id) ?? throw new KeyNotFoundException("Warehouse not found"); if (request.Name != null) w.Name = request.Name; if (request.Code != null) w.Code = request.Code; if (request.Address != null) w.Address = request.Address; if (request.City != null) w.City = request.City; if (request.Phone != null) w.Phone = request.Phone; if (request.ManagerName != null) w.ManagerName = request.ManagerName; await _context.SaveChangesAsync(); return w; }
        public async Task<bool> DeleteWarehouseAsync(int id) { var w = await _context.Warehouses.FindAsync(id) ?? throw new KeyNotFoundException("Warehouse not found"); w.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== CUSTOMER SERVICE ====================
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        public CustomerService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<CustomerResponse> Items, int TotalCount)> GetAllCustomersAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Customers.Include(c => c.CustomerGroup).Where(c => c.TenantId == tenantId);
            if (!string.IsNullOrEmpty(pagination.SearchQuery))
                query = query.Where(c => c.Name.Contains(pagination.SearchQuery) || (c.Phone != null && c.Phone.Contains(pagination.SearchQuery)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.CreatedAt).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(c => MapToResponse(c)).ToListAsync();
            return (items, total);
        }

        public async Task<CustomerResponse?> GetCustomerByIdAsync(int id, int tenantId)
        { var c = await _context.Customers.Include(c => c.CustomerGroup).FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId); return c == null ? null : MapToResponse(c); }

        public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, int tenantId)
        {
            var c = new Customer { Name = request.Name, Phone = request.Phone, Email = request.Email, Address = request.Address, City = request.City, State = request.State, PinCode = request.PinCode, GSTNumber = request.GSTNumber, PANNumber = request.PANNumber, CustomerGroupId = request.CustomerGroupId, CreditLimit = request.CreditLimit, Notes = request.Notes, TenantId = tenantId };
            _context.Customers.Add(c); await _context.SaveChangesAsync();
            return (await GetCustomerByIdAsync(c.Id, tenantId))!;
        }

        public async Task<CustomerResponse> UpdateCustomerAsync(int id, UpdateCustomerRequest request, int tenantId)
        {
            var c = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId) ?? throw new KeyNotFoundException("Customer not found");
            if (request.Name != null) c.Name = request.Name; if (request.Phone != null) c.Phone = request.Phone; if (request.Email != null) c.Email = request.Email;
            if (request.Address != null) c.Address = request.Address; if (request.City != null) c.City = request.City; if (request.State != null) c.State = request.State;
            if (request.PinCode != null) c.PinCode = request.PinCode; if (request.GSTNumber != null) c.GSTNumber = request.GSTNumber; if (request.PANNumber != null) c.PANNumber = request.PANNumber;
            if (request.CustomerGroupId.HasValue) c.CustomerGroupId = request.CustomerGroupId; if (request.CreditLimit.HasValue) c.CreditLimit = request.CreditLimit.Value; if (request.Notes != null) c.Notes = request.Notes;
            await _context.SaveChangesAsync();
            return (await GetCustomerByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteCustomerAsync(int id, int tenantId)
        { var c = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId) ?? throw new KeyNotFoundException("Customer not found"); c.IsDeleted = true; await _context.SaveChangesAsync(); return true; }

        private static CustomerResponse MapToResponse(Customer c) => new()
        { Id = c.Id, Name = c.Name, Phone = c.Phone, Email = c.Email, Address = c.Address, City = c.City, State = c.State, GSTNumber = c.GSTNumber, CustomerGroupName = c.CustomerGroup?.Name, CreditLimit = c.CreditLimit, OutstandingBalance = c.OutstandingBalance, TotalPurchaseAmount = c.TotalPurchaseAmount, TotalPurchaseCount = c.TotalPurchaseCount, LastPurchaseDate = c.LastPurchaseDate, IsActive = c.IsActive };
    }

    // ==================== SUPPLIER SERVICE ====================
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;
        public SupplierService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<SupplierResponse> Items, int TotalCount)> GetAllSuppliersAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Suppliers.Include(s => s.SupplierGroup).Where(s => s.TenantId == tenantId);
            if (!string.IsNullOrEmpty(pagination.SearchQuery))
                query = query.Where(s => s.Name.Contains(pagination.SearchQuery) || (s.Phone != null && s.Phone.Contains(pagination.SearchQuery)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(s => s.CreatedAt).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(s => new SupplierResponse { Id = s.Id, Name = s.Name, CompanyName = s.CompanyName, Phone = s.Phone, Email = s.Email, Address = s.Address, City = s.City, GSTNumber = s.GSTNumber, SupplierGroupName = s.SupplierGroup != null ? s.SupplierGroup.Name : null, CreditLimit = s.CreditLimit, OutstandingBalance = s.OutstandingBalance, CreditDays = s.CreditDays, IsActive = s.IsActive }).ToListAsync();
            return (items, total);
        }

        public async Task<SupplierResponse?> GetSupplierByIdAsync(int id, int tenantId)
        {
            var s = await _context.Suppliers.Include(s => s.SupplierGroup).FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
            return s == null ? null : new SupplierResponse { Id = s.Id, Name = s.Name, CompanyName = s.CompanyName, Phone = s.Phone, Email = s.Email, Address = s.Address, City = s.City, GSTNumber = s.GSTNumber, SupplierGroupName = s.SupplierGroup?.Name, CreditLimit = s.CreditLimit, OutstandingBalance = s.OutstandingBalance, CreditDays = s.CreditDays, IsActive = s.IsActive };
        }

        public async Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, int tenantId)
        {
            var s = new Supplier { Name = request.Name, CompanyName = request.CompanyName, Phone = request.Phone, Email = request.Email, Address = request.Address, City = request.City, State = request.State, PinCode = request.PinCode, GSTNumber = request.GSTNumber, PANNumber = request.PANNumber, BankName = request.BankName, BankAccountNumber = request.BankAccountNumber, BankIFSC = request.BankIFSC, SupplierGroupId = request.SupplierGroupId, CreditLimit = request.CreditLimit, CreditDays = request.CreditDays, Notes = request.Notes, TenantId = tenantId };
            _context.Suppliers.Add(s); await _context.SaveChangesAsync();
            return (await GetSupplierByIdAsync(s.Id, tenantId))!;
        }

        public async Task<SupplierResponse> UpdateSupplierAsync(int id, UpdateSupplierRequest request, int tenantId)
        {
            var s = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId) ?? throw new KeyNotFoundException("Supplier not found");
            if (request.Name != null) s.Name = request.Name; if (request.CompanyName != null) s.CompanyName = request.CompanyName; if (request.Phone != null) s.Phone = request.Phone;
            if (request.Email != null) s.Email = request.Email; if (request.Address != null) s.Address = request.Address; if (request.City != null) s.City = request.City;
            if (request.GSTNumber != null) s.GSTNumber = request.GSTNumber; if (request.BankName != null) s.BankName = request.BankName; if (request.BankAccountNumber != null) s.BankAccountNumber = request.BankAccountNumber;
            if (request.BankIFSC != null) s.BankIFSC = request.BankIFSC; if (request.CreditLimit.HasValue) s.CreditLimit = request.CreditLimit.Value; if (request.CreditDays.HasValue) s.CreditDays = request.CreditDays.Value;
            await _context.SaveChangesAsync();
            return (await GetSupplierByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteSupplierAsync(int id, int tenantId)
        { var s = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId) ?? throw new KeyNotFoundException("Supplier not found"); s.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }
}
