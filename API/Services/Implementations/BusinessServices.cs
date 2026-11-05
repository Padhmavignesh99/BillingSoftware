using MasterBilling.API.Data;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Models.Enums;
using MasterBilling.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBilling.API.Services.Implementations
{
    // ==================== BUSINESS SERVICE ====================
    public class BusinessService : IBusinessService
    {
        private readonly ApplicationDbContext _context;
        public BusinessService(ApplicationDbContext context) { _context = context; }

        public async Task<BusinessResponse?> GetBusinessByIdAsync(int id)
        {
            var b = await _context.Businesses.Include(b => b.BusinessType).FirstOrDefaultAsync(b => b.Id == id);
            return b == null ? null : MapToResponse(b);
        }

        public async Task<BusinessResponse> CreateBusinessAsync(CreateBusinessRequest request)
        {
            var business = new Business { Name = request.Name, LegalName = request.LegalName, BusinessTypeId = request.BusinessTypeId, Email = request.Email, Phone = request.Phone, Website = request.Website, Address = request.Address, City = request.City, State = request.State, Country = request.Country ?? "India", PinCode = request.PinCode, GSTNumber = request.GSTNumber, PANNumber = request.PANNumber, CurrencyCode = request.CurrencyCode ?? "INR", CurrencySymbol = request.CurrencySymbol ?? "₹" };
            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();
            return MapToResponse(business);
        }

        public async Task<BusinessResponse> UpdateBusinessAsync(int id, UpdateBusinessRequest request)
        {
            var b = await _context.Businesses.FindAsync(id) ?? throw new KeyNotFoundException("Business not found");
            if (request.Name != null) b.Name = request.Name;
            if (request.LegalName != null) b.LegalName = request.LegalName;
            if (request.BusinessTypeId.HasValue) b.BusinessTypeId = request.BusinessTypeId;
            if (request.Email != null) b.Email = request.Email;
            if (request.Phone != null) b.Phone = request.Phone;
            if (request.Website != null) b.Website = request.Website;
            if (request.Address != null) b.Address = request.Address;
            if (request.City != null) b.City = request.City;
            if (request.State != null) b.State = request.State;
            if (request.Country != null) b.Country = request.Country;
            if (request.PinCode != null) b.PinCode = request.PinCode;
            if (request.GSTNumber != null) b.GSTNumber = request.GSTNumber;
            if (request.PANNumber != null) b.PANNumber = request.PANNumber;
            if (request.CurrencyCode != null) b.CurrencyCode = request.CurrencyCode;
            if (request.CurrencySymbol != null) b.CurrencySymbol = request.CurrencySymbol;
            await _context.SaveChangesAsync();
            return MapToResponse(b);
        }

        public async Task<bool> UploadLogoAsync(int businessId, string logoUrl)
        {
            var b = await _context.Businesses.FindAsync(businessId) ?? throw new KeyNotFoundException("Business not found");
            b.LogoUrl = logoUrl;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BusinessResponse>> GetAllBusinessesAsync() =>
            await _context.Businesses.Include(b => b.BusinessType).Select(b => MapToResponse(b)).ToListAsync();

        private static BusinessResponse MapToResponse(Business b) => new()
        { Id = b.Id, Name = b.Name, LegalName = b.LegalName, BusinessTypeName = b.BusinessType?.Name, Email = b.Email, Phone = b.Phone, Website = b.Website, Address = b.Address, City = b.City, State = b.State, Country = b.Country, PinCode = b.PinCode, GSTNumber = b.GSTNumber, PANNumber = b.PANNumber, LogoUrl = b.LogoUrl, CurrencyCode = b.CurrencyCode, CurrencySymbol = b.CurrencySymbol, Status = b.Status.ToString() };
    }

    // ==================== BUSINESS TYPE SERVICE ====================
    public class BusinessTypeService : IBusinessTypeService
    {
        private readonly ApplicationDbContext _context;
        public BusinessTypeService(ApplicationDbContext context) { _context = context; }

        public async Task<List<BusinessType>> GetAllAsync() => await _context.BusinessTypes.Where(t => t.IsActive).ToListAsync();
        public async Task<BusinessType> CreateAsync(CreateBusinessTypeRequest request) { var t = new BusinessType { Name = request.Name, Description = request.Description, Icon = request.Icon }; _context.BusinessTypes.Add(t); await _context.SaveChangesAsync(); return t; }
        public async Task<BusinessType> UpdateAsync(int id, CreateBusinessTypeRequest request) { var t = await _context.BusinessTypes.FindAsync(id) ?? throw new KeyNotFoundException("Not found"); t.Name = request.Name; t.Description = request.Description; t.Icon = request.Icon; await _context.SaveChangesAsync(); return t; }
        public async Task<bool> DeleteAsync(int id) { var t = await _context.BusinessTypes.FindAsync(id) ?? throw new KeyNotFoundException("Not found"); t.IsActive = false; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== BRANCH SERVICE ====================
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;
        public BranchService(ApplicationDbContext context) { _context = context; }

        public async Task<List<BranchResponse>> GetAllBranchesAsync(int tenantId) =>
            await _context.Branches.Where(b => b.TenantId == tenantId && !b.IsDeleted).Select(b => new BranchResponse { Id = b.Id, Name = b.Name, Code = b.Code, Address = b.Address, City = b.City, State = b.State, Phone = b.Phone, Email = b.Email, IsMainBranch = b.IsMainBranch }).ToListAsync();

        public async Task<BranchResponse?> GetBranchByIdAsync(int id)
        {
            var b = await _context.Branches.FindAsync(id);
            return b == null ? null : new BranchResponse { Id = b.Id, Name = b.Name, Code = b.Code, Address = b.Address, City = b.City, State = b.State, Phone = b.Phone, Email = b.Email, IsMainBranch = b.IsMainBranch };
        }

        public async Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, int tenantId)
        {
            var branch = new Branch { Name = request.Name, Code = request.Code, Address = request.Address, City = request.City, State = request.State, Phone = request.Phone, Email = request.Email, IsMainBranch = request.IsMainBranch, TenantId = tenantId };
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return new BranchResponse { Id = branch.Id, Name = branch.Name, Code = branch.Code, Address = branch.Address, City = branch.City, State = branch.State, Phone = branch.Phone, Email = branch.Email, IsMainBranch = branch.IsMainBranch };
        }

        public async Task<BranchResponse> UpdateBranchAsync(int id, UpdateBranchRequest request)
        {
            var b = await _context.Branches.FindAsync(id) ?? throw new KeyNotFoundException("Branch not found");
            if (request.Name != null) b.Name = request.Name;
            if (request.Code != null) b.Code = request.Code;
            if (request.Address != null) b.Address = request.Address;
            if (request.City != null) b.City = request.City;
            if (request.State != null) b.State = request.State;
            if (request.Phone != null) b.Phone = request.Phone;
            if (request.Email != null) b.Email = request.Email;
            await _context.SaveChangesAsync();
            return (await GetBranchByIdAsync(id))!;
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var b = await _context.Branches.FindAsync(id) ?? throw new KeyNotFoundException("Branch not found");
            b.IsDeleted = true; await _context.SaveChangesAsync(); return true;
        }
    }

    // ==================== BUSINESS SETTINGS SERVICE ====================
    public class BusinessSettingsService : IBusinessSettingsService
    {
        private readonly ApplicationDbContext _context;
        public BusinessSettingsService(ApplicationDbContext context) { _context = context; }

        public async Task<BusinessSettings?> GetSettingsAsync(int tenantId) => await _context.BusinessSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);

        public async Task<BusinessSettings> UpdateSettingsAsync(int tenantId, UpdateBusinessSettingsRequest request)
        {
            var s = await _context.BusinessSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId) ?? new BusinessSettings { TenantId = tenantId };
            if (s.Id == 0) _context.BusinessSettings.Add(s);
            if (request.DateFormat != null) s.DateFormat = request.DateFormat;
            if (request.TimeZone != null) s.TimeZone = request.TimeZone;
            if (request.Language != null) s.Language = request.Language;
            if (request.EnableGST.HasValue) s.EnableGST = request.EnableGST.Value;
            if (request.EnableMultiBranch.HasValue) s.EnableMultiBranch = request.EnableMultiBranch.Value;
            if (request.EnableBarcode.HasValue) s.EnableBarcode = request.EnableBarcode.Value;
            if (request.EnableLowStockAlert.HasValue) s.EnableLowStockAlert = request.EnableLowStockAlert.Value;
            if (request.LowStockThreshold.HasValue) s.LowStockThreshold = request.LowStockThreshold.Value;
            if (request.EnableEmailNotifications.HasValue) s.EnableEmailNotifications = request.EnableEmailNotifications.Value;
            if (request.EnableWhatsAppNotifications.HasValue) s.EnableWhatsAppNotifications = request.EnableWhatsAppNotifications.Value;
            if (request.EnableSmsNotifications.HasValue) s.EnableSmsNotifications = request.EnableSmsNotifications.Value;
            await _context.SaveChangesAsync();
            return s;
        }

        public async Task<InvoiceSettings?> GetInvoiceSettingsAsync(int tenantId) => await _context.InvoiceSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);

        public async Task<InvoiceSettings> UpdateInvoiceSettingsAsync(int tenantId, UpdateInvoiceSettingsRequest request)
        {
            var s = await _context.InvoiceSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId) ?? new InvoiceSettings { TenantId = tenantId };
            if (s.Id == 0) _context.InvoiceSettings.Add(s);
            if (request.InvoicePrefix != null) s.InvoicePrefix = request.InvoicePrefix;
            if (request.InvoiceNumberFormat != null) s.InvoiceNumberFormat = request.InvoiceNumberFormat;
            if (request.TermsAndConditions != null) s.TermsAndConditions = request.TermsAndConditions;
            if (request.FooterNote != null) s.FooterNote = request.FooterNote;
            if (request.ShowLogo.HasValue) s.ShowLogo = request.ShowLogo.Value;
            if (request.ShowGST.HasValue) s.ShowGST = request.ShowGST.Value;
            if (request.ShowDiscount.HasValue) s.ShowDiscount = request.ShowDiscount.Value;
            if (request.ShowSignature.HasValue) s.ShowSignature = request.ShowSignature.Value;
            if (request.SignatureImageUrl != null) s.SignatureImageUrl = request.SignatureImageUrl;
            if (request.TemplateName != null) s.TemplateName = request.TemplateName;
            if (request.PrimaryColor != null) s.PrimaryColor = request.PrimaryColor;
            await _context.SaveChangesAsync();
            return s;
        }
    }
}
