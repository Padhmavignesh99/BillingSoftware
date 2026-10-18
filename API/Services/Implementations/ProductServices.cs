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
    // ==================== PRODUCT SERVICE ====================
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<ProductResponse> Items, int TotalCount)> GetAllProductsAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Unit).Include(p => p.TaxRate)
                .Where(p => p.TenantId == tenantId);
            if (!string.IsNullOrEmpty(pagination.SearchQuery))
                query = query.Where(p => p.Name.Contains(pagination.SearchQuery) || (p.SKU != null && p.SKU.Contains(pagination.SearchQuery)) || (p.Barcode != null && p.Barcode.Contains(pagination.SearchQuery)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.CreatedAt).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(p => MapToResponse(p)).ToListAsync();
            return (items, total);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id, int tenantId)
        {
            var p = await _context.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Unit).Include(p => p.TaxRate)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
            return p == null ? null : MapToResponse(p);
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, int tenantId)
        {
            var product = new Product
            {
                Name = request.Name, SKU = request.SKU, Barcode = request.Barcode, Description = request.Description, HSNCode = request.HSNCode,
                CategoryId = request.CategoryId, BrandId = request.BrandId, UnitId = request.UnitId, PurchasePrice = request.PurchasePrice,
                SellingPrice = request.SellingPrice, MRP = request.MRP, TaxRateId = request.TaxRateId, DiscountId = request.DiscountId,
                CurrentStock = request.CurrentStock ?? 0, MinStockLevel = request.MinStockLevel, MaxStockLevel = request.MaxStockLevel,
                ImageUrl = request.ImageUrl, IsService = request.IsService, ExpiryDate = request.ExpiryDate, TenantId = tenantId
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (request.CurrentStock > 0)
            {
                _context.StockTransactions.Add(new StockTransaction { ProductId = product.Id, TransactionType = StockTransactionType.Opening, Quantity = request.CurrentStock ?? 0, PreviousStock = 0, NewStock = request.CurrentStock ?? 0, Notes = "Opening stock", TenantId = tenantId });
                await _context.SaveChangesAsync();
            }
            return (await GetProductByIdAsync(product.Id, tenantId))!;
        }

        public async Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request, int tenantId)
        {
            var p = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Product not found");
            if (request.Name != null) p.Name = request.Name;
            if (request.SKU != null) p.SKU = request.SKU;
            if (request.Barcode != null) p.Barcode = request.Barcode;
            if (request.Description != null) p.Description = request.Description;
            if (request.HSNCode != null) p.HSNCode = request.HSNCode;
            if (request.CategoryId.HasValue) p.CategoryId = request.CategoryId;
            if (request.BrandId.HasValue) p.BrandId = request.BrandId;
            if (request.UnitId.HasValue) p.UnitId = request.UnitId;
            if (request.PurchasePrice.HasValue) p.PurchasePrice = request.PurchasePrice.Value;
            if (request.SellingPrice.HasValue) p.SellingPrice = request.SellingPrice.Value;
            if (request.MRP.HasValue) p.MRP = request.MRP;
            if (request.TaxRateId.HasValue) p.TaxRateId = request.TaxRateId;
            if (request.DiscountId.HasValue) p.DiscountId = request.DiscountId;
            if (request.MinStockLevel.HasValue) p.MinStockLevel = request.MinStockLevel;
            if (request.MaxStockLevel.HasValue) p.MaxStockLevel = request.MaxStockLevel;
            if (request.ImageUrl != null) p.ImageUrl = request.ImageUrl;
            if (request.IsService.HasValue) p.IsService = request.IsService.Value;
            if (request.ExpiryDate.HasValue) p.ExpiryDate = request.ExpiryDate.Value;
            await _context.SaveChangesAsync();
            return (await GetProductByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteProductAsync(int id, int tenantId)
        {
            var p = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Product not found");
            p.IsDeleted = true; await _context.SaveChangesAsync(); return true;
        }

        public async Task<List<ProductResponse>> SearchProductsAsync(int tenantId, string query) =>
            await _context.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Unit).Include(p => p.TaxRate)
                .Where(p => p.TenantId == tenantId && (p.Name.Contains(query) || (p.SKU != null && p.SKU.Contains(query)) || (p.Barcode != null && p.Barcode.Contains(query))))
                .Take(20).Select(p => MapToResponse(p)).ToListAsync();

        public async Task<ProductResponse?> GetProductByBarcodeAsync(int tenantId, string barcode)
        {
            var p = await _context.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Unit).Include(p => p.TaxRate)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == barcode);
            return p == null ? null : MapToResponse(p);
        }

        public async Task<List<ProductResponse>> BulkUploadAsync(BulkProductUploadRequest request, int tenantId)
        {
            var results = new List<ProductResponse>();
            foreach (var item in request.Products)
                results.Add(await CreateProductAsync(item, tenantId));
            return results;
        }

        private static ProductResponse MapToResponse(Product p) => new()
        {
            Id = p.Id, Name = p.Name, SKU = p.SKU, Barcode = p.Barcode, Description = p.Description, HSNCode = p.HSNCode,
            CategoryName = p.Category?.Name, BrandName = p.Brand?.Name, UnitName = p.Unit?.Name, PurchasePrice = p.PurchasePrice,
            SellingPrice = p.SellingPrice, MRP = p.MRP, TaxRateName = p.TaxRate?.Name, TaxPercentage = p.TaxRate?.Percentage,
            CurrentStock = p.CurrentStock, MinStockLevel = p.MinStockLevel, ImageUrl = p.ImageUrl, IsService = p.IsService, IsActive = p.IsActive, ExpiryDate = p.ExpiryDate
        };
    }

    // ==================== CATEGORY SERVICE ====================
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        public CategoryService(ApplicationDbContext context) { _context = context; }

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync(int tenantId) =>
            await _context.ProductCategories.Include(c => c.Products).Where(c => c.TenantId == tenantId && !c.IsDeleted && c.ParentCategoryId == null)
                .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name, Description = c.Description, ImageUrl = c.ImageUrl, SortOrder = c.SortOrder, ProductCount = c.Products.Count,
                    SubCategories = c.SubCategories.Select(sc => new CategoryResponse { Id = sc.Id, Name = sc.Name, Description = sc.Description, ParentCategoryId = sc.ParentCategoryId, ParentCategoryName = c.Name }).ToList() }).ToListAsync();

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, int tenantId)
        {
            var cat = new ProductCategory { Name = request.Name, Description = request.Description, ParentCategoryId = request.ParentCategoryId, ImageUrl = request.ImageUrl, SortOrder = request.SortOrder, TenantId = tenantId };
            _context.ProductCategories.Add(cat);
            await _context.SaveChangesAsync();
            return new CategoryResponse { Id = cat.Id, Name = cat.Name, Description = cat.Description, ParentCategoryId = cat.ParentCategoryId, ImageUrl = cat.ImageUrl, SortOrder = cat.SortOrder };
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var cat = await _context.ProductCategories.FindAsync(id) ?? throw new KeyNotFoundException("Category not found");
            if (request.Name != null) cat.Name = request.Name;
            if (request.Description != null) cat.Description = request.Description;
            if (request.ParentCategoryId.HasValue) cat.ParentCategoryId = request.ParentCategoryId;
            if (request.ImageUrl != null) cat.ImageUrl = request.ImageUrl;
            if (request.SortOrder.HasValue) cat.SortOrder = request.SortOrder.Value;
            await _context.SaveChangesAsync();
            return new CategoryResponse { Id = cat.Id, Name = cat.Name, Description = cat.Description, ParentCategoryId = cat.ParentCategoryId, ImageUrl = cat.ImageUrl, SortOrder = cat.SortOrder };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        { var cat = await _context.ProductCategories.FindAsync(id) ?? throw new KeyNotFoundException("Category not found"); cat.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== BRAND SERVICE ====================
    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;
        public BrandService(ApplicationDbContext context) { _context = context; }
        public async Task<List<Brand>> GetAllBrandsAsync(int tenantId) => await _context.Brands.Where(b => b.TenantId == tenantId && !b.IsDeleted).ToListAsync();
        public async Task<Brand> CreateBrandAsync(CreateBrandRequest request, int tenantId) { var b = new Brand { Name = request.Name, Description = request.Description, LogoUrl = request.LogoUrl, TenantId = tenantId }; _context.Brands.Add(b); await _context.SaveChangesAsync(); return b; }
        public async Task<Brand> UpdateBrandAsync(int id, CreateBrandRequest request) { var b = await _context.Brands.FindAsync(id) ?? throw new KeyNotFoundException("Brand not found"); b.Name = request.Name; b.Description = request.Description; b.LogoUrl = request.LogoUrl; await _context.SaveChangesAsync(); return b; }
        public async Task<bool> DeleteBrandAsync(int id) { var b = await _context.Brands.FindAsync(id) ?? throw new KeyNotFoundException("Brand not found"); b.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== UNIT SERVICE ====================
    public class UnitService : IUnitService
    {
        private readonly ApplicationDbContext _context;
        public UnitService(ApplicationDbContext context) { _context = context; }
        public async Task<List<Unit>> GetAllUnitsAsync(int tenantId) => await _context.Units.Where(u => u.TenantId == tenantId && !u.IsDeleted).ToListAsync();
        public async Task<Unit> CreateUnitAsync(CreateUnitRequest request, int tenantId) { var u = new Unit { Name = request.Name, ShortName = request.ShortName, Description = request.Description, TenantId = tenantId }; _context.Units.Add(u); await _context.SaveChangesAsync(); return u; }
        public async Task<Unit> UpdateUnitAsync(int id, CreateUnitRequest request) { var u = await _context.Units.FindAsync(id) ?? throw new KeyNotFoundException("Unit not found"); u.Name = request.Name; u.ShortName = request.ShortName; u.Description = request.Description; await _context.SaveChangesAsync(); return u; }
        public async Task<bool> DeleteUnitAsync(int id) { var u = await _context.Units.FindAsync(id) ?? throw new KeyNotFoundException("Unit not found"); u.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== TAX SERVICE ====================
    public class TaxService : ITaxService
    {
        private readonly ApplicationDbContext _context;
        public TaxService(ApplicationDbContext context) { _context = context; }
        public async Task<List<TaxRateResponse>> GetAllTaxRatesAsync(int tenantId) => await _context.TaxRates.Where(t => t.TenantId == tenantId && !t.IsDeleted)
            .Select(t => new TaxRateResponse { Id = t.Id, Name = t.Name, Percentage = t.Percentage, Description = t.Description, CGSTPercentage = t.CGSTPercentage, SGSTPercentage = t.SGSTPercentage, IGSTPercentage = t.IGSTPercentage, IsDefault = t.IsDefault }).ToListAsync();

        public async Task<TaxRateResponse> CreateTaxRateAsync(CreateTaxRateRequest request, int tenantId)
        {
            var t = new TaxRate { Name = request.Name, Percentage = request.Percentage, Description = request.Description, CGSTPercentage = request.CGSTPercentage, SGSTPercentage = request.SGSTPercentage, IGSTPercentage = request.IGSTPercentage, IsDefault = request.IsDefault, TenantId = tenantId };
            _context.TaxRates.Add(t);
            await _context.SaveChangesAsync();
            return new TaxRateResponse { Id = t.Id, Name = t.Name, Percentage = t.Percentage, Description = t.Description, CGSTPercentage = t.CGSTPercentage, SGSTPercentage = t.SGSTPercentage, IGSTPercentage = t.IGSTPercentage, IsDefault = t.IsDefault };
        }

        public async Task<TaxRateResponse> UpdateTaxRateAsync(int id, UpdateTaxRateRequest request)
        {
            var t = await _context.TaxRates.FindAsync(id) ?? throw new KeyNotFoundException("TaxRate not found");
            if (request.Name != null) t.Name = request.Name;
            if (request.Percentage.HasValue) t.Percentage = request.Percentage.Value;
            if (request.Description != null) t.Description = request.Description;
            if (request.CGSTPercentage.HasValue) t.CGSTPercentage = request.CGSTPercentage;
            if (request.SGSTPercentage.HasValue) t.SGSTPercentage = request.SGSTPercentage;
            if (request.IGSTPercentage.HasValue) t.IGSTPercentage = request.IGSTPercentage;
            if (request.IsDefault.HasValue) t.IsDefault = request.IsDefault.Value;
            await _context.SaveChangesAsync();
            return new TaxRateResponse { Id = t.Id, Name = t.Name, Percentage = t.Percentage, Description = t.Description, CGSTPercentage = t.CGSTPercentage, SGSTPercentage = t.SGSTPercentage, IGSTPercentage = t.IGSTPercentage, IsDefault = t.IsDefault };
        }

        public async Task<bool> DeleteTaxRateAsync(int id) { var t = await _context.TaxRates.FindAsync(id) ?? throw new KeyNotFoundException("TaxRate not found"); t.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== DISCOUNT SERVICE ====================
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;
        public DiscountService(ApplicationDbContext context) { _context = context; }
        public async Task<List<Discount>> GetAllDiscountsAsync(int tenantId) => await _context.Discounts.Where(d => d.TenantId == tenantId && !d.IsDeleted).ToListAsync();
        public async Task<Discount> CreateDiscountAsync(CreateDiscountRequest request, int tenantId) { var d = new Discount { Name = request.Name, Type = request.Type, Value = request.Value, StartDate = request.StartDate, EndDate = request.EndDate, MinPurchaseAmount = request.MinPurchaseAmount, MaxDiscountAmount = request.MaxDiscountAmount, CouponCode = request.CouponCode, TenantId = tenantId }; _context.Discounts.Add(d); await _context.SaveChangesAsync(); return d; }
        public async Task<Discount> UpdateDiscountAsync(int id, UpdateDiscountRequest request) { var d = await _context.Discounts.FindAsync(id) ?? throw new KeyNotFoundException("Discount not found"); if (request.Name != null) d.Name = request.Name; if (request.Type.HasValue) d.Type = request.Type.Value; if (request.Value.HasValue) d.Value = request.Value.Value; if (request.StartDate.HasValue) d.StartDate = request.StartDate; if (request.EndDate.HasValue) d.EndDate = request.EndDate; await _context.SaveChangesAsync(); return d; }
        public async Task<bool> DeleteDiscountAsync(int id) { var d = await _context.Discounts.FindAsync(id) ?? throw new KeyNotFoundException("Discount not found"); d.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== BARCODE SERVICE ====================
    public class BarcodeService : IBarcodeService
    {
        private readonly ApplicationDbContext _context;
        public BarcodeService(ApplicationDbContext context) { _context = context; }

        public async Task<string> GenerateBarcodeAsync(int productId, int tenantId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Product not found");
            if (string.IsNullOrEmpty(product.Barcode))
            {
                product.Barcode = $"PRD{tenantId:D4}{productId:D8}";
                await _context.SaveChangesAsync();
            }
            return product.Barcode;
        }

        public async Task<byte[]> GetBarcodeImageAsync(int productId, int tenantId)
        {
            var barcode = await GenerateBarcodeAsync(productId, tenantId);
            // Placeholder — in production, use a barcode generation library
            return System.Text.Encoding.UTF8.GetBytes(barcode);
        }

        private static System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
}
