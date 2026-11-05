namespace MasterBilling.API.Models.Entities
{
    // ── Product Variant System ─────────────────────────────────────────────────

    /// <summary>Attribute type (e.g. "Size", "Color", "Material")</summary>
    public class VariantAttribute : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; } = 0;
        public ICollection<VariantAttributeValue> Values { get; set; } = new List<VariantAttributeValue>();
    }

    /// <summary>Specific attribute value (e.g. "Red", "XL", "Cotton")</summary>
    public class VariantAttributeValue : TenantEntity
    {
        public int AttributeId { get; set; }
        public VariantAttribute? Attribute { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? ColorHex { get; set; }  // For color-type attributes
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>A specific product variant (e.g. T-Shirt | Red | XL)</summary>
    public class ProductVariant : TenantEntity
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? MRP { get; set; }
        public int CurrentStock { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public ICollection<ProductVariantAttribute> VariantAttributes { get; set; } = new List<ProductVariantAttribute>();
    }

    /// <summary>Pivot: which attribute values make up a given variant</summary>
    public class ProductVariantAttribute
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }
        public int AttributeValueId { get; set; }
        public VariantAttributeValue? AttributeValue { get; set; }
    }
}

