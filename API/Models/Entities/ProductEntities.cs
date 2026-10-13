using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    public class Product : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? HSNCode { get; set; }
        public int? CategoryId { get; set; }
        public ProductCategory? Category { get; set; }
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? MRP { get; set; }
        public int? TaxRateId { get; set; }
        public TaxRate? TaxRate { get; set; }
        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public decimal CurrentStock { get; set; } = 0;
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        /// <summary>Reorder point — when stock falls at or below this, a repurchase is triggered. Alias of MinStockLevel for AI service compatibility.</summary>
        public decimal ReorderLevel   { get; set; } = 0;
        public string? ImageUrl       { get; set; }
        public decimal? Weight        { get; set; }
        public string?  WeightUnit    { get; set; }
        public bool IsService         { get; set; } = false;
        /// <summary>Optional product expiry date. Used for FMCG, pharmacy, grocery etc.</summary>
        public DateTime? ExpiryDate   { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }

    public class ProductCategory : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public ProductCategory? ParentCategory { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; } = 0;
        public ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Brand : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
    }

    public class Unit : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TaxRate : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string? Description { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class Discount : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public DiscountType Type { get; set; } = DiscountType.Percentage;
        public decimal Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string? CouponCode { get; set; }
    }

    public class ProductImage : TenantEntity
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }
}
