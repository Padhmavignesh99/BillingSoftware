using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class ProductVariantService
    {
        private readonly ApplicationDbContext _db;
        public ProductVariantService(ApplicationDbContext db) => _db = db;

        // ── Attributes ─────────────────────────────────────────────────────────
        public async Task<List<VariantAttribute>> GetAttributesAsync(int tenantId) =>
            await _db.VariantAttributes
                .Where(a => a.TenantId == tenantId && a.IsActive)
                .Include(a => a.Values)
                .OrderBy(a => a.SortOrder).ThenBy(a => a.Name)
                .ToListAsync();

        public async Task<VariantAttribute?> GetAttributeByIdAsync(int id, int tenantId) =>
            await _db.VariantAttributes
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        public async Task<VariantAttribute> CreateAttributeAsync(int tenantId, string name, string? description)
        {
            var attr = new VariantAttribute
            {
                TenantId = tenantId, Name = name, Description = description,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true
            };
            _db.VariantAttributes.Add(attr);
            await _db.SaveChangesAsync();
            return attr;
        }

        public async Task<VariantAttributeValue> AddAttributeValueAsync(int tenantId, int attributeId, string value, string? colorHex)
        {
            var val = new VariantAttributeValue
            {
                TenantId = tenantId, AttributeId = attributeId,
                Value = value, ColorHex = colorHex,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true
            };
            _db.VariantAttributeValues.Add(val);
            await _db.SaveChangesAsync();
            return val;
        }

        // ── Product Variants ───────────────────────────────────────────────────
        public async Task<List<ProductVariant>> GetVariantsAsync(int productId, int tenantId) =>
            await _db.ProductVariants
                .Where(v => v.ProductId == productId && v.TenantId == tenantId)
                .Include(v => v.VariantAttributes).ThenInclude(va => va.AttributeValue).ThenInclude(av => av!.Attribute)
                .ToListAsync();

        public async Task<ProductVariant> CreateVariantAsync(int tenantId, int productId,
            string sku, decimal? sellingPrice, decimal? purchasePrice, decimal? mrp,
            List<int> attributeValueIds)
        {
            var variant = new ProductVariant
            {
                TenantId = tenantId, ProductId = productId,
                SKU = sku, SellingPrice = sellingPrice,
                PurchasePrice = purchasePrice, MRP = mrp,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true
            };
            _db.ProductVariants.Add(variant);
            await _db.SaveChangesAsync();

            foreach (var valId in attributeValueIds)
            {
                _db.ProductVariantAttributes.Add(new ProductVariantAttribute
                {
                    ProductVariantId = variant.Id,
                    AttributeValueId = valId
                });
            }
            await _db.SaveChangesAsync();
            return variant;
        }

        public async Task<bool> UpdateVariantStockAsync(int variantId, int tenantId, int newStock)
        {
            var v = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId && x.TenantId == tenantId);
            if (v == null) return false;
            v.CurrentStock = newStock;
            v.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVariantAsync(int variantId, int tenantId)
        {
            var v = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId && x.TenantId == tenantId);
            if (v == null) return false;
            v.IsActive = false;
            v.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

