using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BillingSoftware.API.Services.Implementations
{
    public class ExportImportService : IExportImportService
    {
        private readonly ApplicationDbContext _db;

        public ExportImportService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> ExportProductsCsvAsync(int tenantId)
        {
            var products = await _db.Products
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Include(p => p.TaxRate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Name,SKU,Barcode,Description,HSNCode,Category,Brand,Unit,PurchasePrice,SellingPrice,MRP,TaxRate,CurrentStock,MinStockLevel,MaxStockLevel,ReorderLevel,IsService,ExpiryDate");

            foreach (var p in products)
            {
                sb.AppendLine($"\"{EscapeCsv(p.Name)}\",\"{EscapeCsv(p.SKU)}\",\"{EscapeCsv(p.Barcode)}\",\"{EscapeCsv(p.Description)}\",\"{EscapeCsv(p.HSNCode)}\",\"{EscapeCsv(p.Category?.Name)}\",\"{EscapeCsv(p.Brand?.Name)}\",\"{EscapeCsv(p.Unit?.Name)}\",{p.PurchasePrice},{p.SellingPrice},{p.MRP},{p.TaxRate?.Rate},{p.CurrentStock},{p.MinStockLevel},{p.MaxStockLevel},{p.ReorderLevel},{p.IsService},{p.ExpiryDate?.ToString("yyyy-MM-dd")}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportCustomersCsvAsync(int tenantId)
        {
            var customers = await _db.Customers
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .Include(c => c.CustomerGroup)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Name,Phone,Email,Address,City,State,PinCode,GSTNumber,PANNumber,CustomerGroup,CreditLimit,OutstandingBalance,Notes");

            foreach (var c in customers)
            {
                sb.AppendLine($"\"{EscapeCsv(c.Name)}\",\"{EscapeCsv(c.Phone)}\",\"{EscapeCsv(c.Email)}\",\"{EscapeCsv(c.Address)}\",\"{EscapeCsv(c.City)}\",\"{EscapeCsv(c.State)}\",\"{EscapeCsv(c.PinCode)}\",\"{EscapeCsv(c.GSTNumber)}\",\"{EscapeCsv(c.PANNumber)}\",\"{EscapeCsv(c.CustomerGroup?.Name)}\",{c.CreditLimit},{c.OutstandingBalance},\"{EscapeCsv(c.Notes)}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportSuppliersCsvAsync(int tenantId)
        {
            var suppliers = await _db.Suppliers
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .Include(s => s.SupplierGroup)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Name,CompanyName,Phone,Email,Address,City,State,PinCode,GSTNumber,PANNumber,SupplierGroup,CreditLimit,Notes");

            foreach (var s in suppliers)
            {
                sb.AppendLine($"\"{EscapeCsv(s.Name)}\",\"{EscapeCsv(s.CompanyName)}\",\"{EscapeCsv(s.Phone)}\",\"{EscapeCsv(s.Email)}\",\"{EscapeCsv(s.Address)}\",\"{EscapeCsv(s.City)}\",\"{EscapeCsv(s.State)}\",\"{EscapeCsv(s.PinCode)}\",\"{EscapeCsv(s.GSTNumber)}\",\"{EscapeCsv(s.PANNumber)}\",\"{EscapeCsv(s.SupplierGroup?.Name)}\",{s.CreditLimit},\"{EscapeCsv(s.Notes)}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportInvoicesCsvAsync(int tenantId, DateTime? from, DateTime? to)
        {
            var query = _db.Invoices
                .Where(i => i.TenantId == tenantId && !i.IsDeleted)
                .Include(i => i.Customer)
                .AsQueryable();

            if (from.HasValue) query = query.Where(i => i.InvoiceDate >= from.Value);
            if (to.HasValue) query = query.Where(i => i.InvoiceDate <= to.Value);

            var invoices = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("InvoiceNumber,InvoiceDate,DueDate,CustomerName,SubTotal,TaxAmount,DiscountAmount,TotalAmount,PaidAmount,DueAmount,Status,PaymentMethod,Notes");

            foreach (var inv in invoices)
            {
                sb.AppendLine($"\"{EscapeCsv(inv.InvoiceNumber)}\",{inv.InvoiceDate:yyyy-MM-dd},{inv.DueDate?.ToString("yyyy-MM-dd")},\"{EscapeCsv(inv.Customer?.Name)}\",{inv.SubTotal},{inv.TaxAmount},{inv.DiscountAmount},{inv.TotalAmount},{inv.PaidAmount},{inv.DueAmount},{inv.Status},{inv.PaymentMethod},\"{EscapeCsv(inv.Notes)}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<ImportResult> ImportProductsCsvAsync(Stream csvStream, int tenantId)
        {
            var result = new ImportResult();
            using var reader = new StreamReader(csvStream);
            var header = await reader.ReadLineAsync(); // skip header

            int lineNum = 1;
            while (!reader.EndOfStream)
            {
                lineNum++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var cols = ParseCsvLine(line);
                    if (cols.Length < 10)
                    {
                        result.Failed++;
                        result.Errors.Add($"Line {lineNum}: Not enough columns");
                        continue;
                    }

                    var product = new Product
                    {
                        TenantId = tenantId,
                        Name = cols[0].Trim(),
                        SKU = string.IsNullOrEmpty(cols[1]) ? null : cols[1].Trim(),
                        Barcode = string.IsNullOrEmpty(cols[2]) ? null : cols[2].Trim(),
                        Description = string.IsNullOrEmpty(cols[3]) ? null : cols[3].Trim(),
                        HSNCode = string.IsNullOrEmpty(cols[4]) ? null : cols[4].Trim(),
                        PurchasePrice = decimal.TryParse(cols[8], out var pp) ? pp : 0,
                        SellingPrice = decimal.TryParse(cols[9], out var sp) ? sp : 0,
                        MRP = decimal.TryParse(cols[10], out var mrp) ? mrp : null,
                        CurrentStock = decimal.TryParse(cols[12], out var cs) ? cs : 0,
                        MinStockLevel = decimal.TryParse(cols[13], out var min) ? min : null,
                        MaxStockLevel = decimal.TryParse(cols[14], out var max) ? max : null,
                        ReorderLevel = decimal.TryParse(cols[15], out var rl) ? rl : 0,
                        IsService = cols.Length > 16 && cols[16].Trim().ToLower() == "true",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        result.Failed++;
                        result.Errors.Add($"Line {lineNum}: Name is required");
                        continue;
                    }

                    _db.Products.Add(product);
                    result.Success++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Line {lineNum}: {ex.Message}");
                }
            }

            if (result.Success > 0)
                await _db.SaveChangesAsync();

            return result;
        }

        public async Task<ImportResult> ImportCustomersCsvAsync(Stream csvStream, int tenantId)
        {
            var result = new ImportResult();
            using var reader = new StreamReader(csvStream);
            var header = await reader.ReadLineAsync(); // skip header

            int lineNum = 1;
            while (!reader.EndOfStream)
            {
                lineNum++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var cols = ParseCsvLine(line);
                    if (cols.Length < 1)
                    {
                        result.Failed++;
                        result.Errors.Add($"Line {lineNum}: Not enough columns");
                        continue;
                    }

                    var customer = new Customer
                    {
                        TenantId = tenantId,
                        Name = cols[0].Trim(),
                        Phone = cols.Length > 1 ? cols[1].Trim() : null,
                        Email = cols.Length > 2 ? cols[2].Trim() : null,
                        Address = cols.Length > 3 ? cols[3].Trim() : null,
                        City = cols.Length > 4 ? cols[4].Trim() : null,
                        State = cols.Length > 5 ? cols[5].Trim() : null,
                        PinCode = cols.Length > 6 ? cols[6].Trim() : null,
                        GSTNumber = cols.Length > 7 ? cols[7].Trim() : null,
                        PANNumber = cols.Length > 8 ? cols[8].Trim() : null,
                        CreditLimit = cols.Length > 10 && decimal.TryParse(cols[10], out var cl) ? cl : 0,
                        Notes = cols.Length > 12 ? cols[12].Trim() : null,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    if (string.IsNullOrWhiteSpace(customer.Name))
                    {
                        result.Failed++;
                        result.Errors.Add($"Line {lineNum}: Name is required");
                        continue;
                    }

                    _db.Customers.Add(customer);
                    result.Success++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Line {lineNum}: {ex.Message}");
                }
            }

            if (result.Success > 0)
                await _db.SaveChangesAsync();

            return result;
        }

        public Task<byte[]> GetProductsTemplateCsvAsync()
        {
            var csv = "Name,SKU,Barcode,Description,HSNCode,Category,Brand,Unit,PurchasePrice,SellingPrice,MRP,TaxRate,CurrentStock,MinStockLevel,MaxStockLevel,ReorderLevel,IsService,ExpiryDate\n\"Sample Product\",\"SKU001\",\"1234567890\",\"Product description\",\"1001\",\"Electronics\",\"BrandName\",\"Pcs\",100.00,150.00,200.00,18,50,5,200,10,false,";
            return Task.FromResult(Encoding.UTF8.GetBytes(csv));
        }

        public Task<byte[]> GetCustomersTemplateCsvAsync()
        {
            var csv = "Name,Phone,Email,Address,City,State,PinCode,GSTNumber,PANNumber,CustomerGroup,CreditLimit,OutstandingBalance,Notes\n\"John Doe\",\"9876543210\",\"john@example.com\",\"123 Main St\",\"Mumbai\",\"Maharashtra\",\"400001\",\"27ABCDE1234F1Z5\",\"ABCDE1234F\",\"Regular\",10000,0,\"VIP Customer\"";
            return Task.FromResult(Encoding.UTF8.GetBytes(csv));
        }

        private static string EscapeCsv(string? value)
        {
            if (value == null) return string.Empty;
            return value.Replace("\"", "\"\"");
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
