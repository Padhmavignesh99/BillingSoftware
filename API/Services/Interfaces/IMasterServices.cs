using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;

namespace MasterBilling.API.Services.Interfaces
{
    // ==================== AUTH ====================
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
    }

    // ==================== USER ====================
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllUsersAsync(int tenantId);
        Task<UserResponse?> GetUserByIdAsync(int id);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request, int tenantId);
        Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int id);
        Task<UserResponse> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request);
    }

    // ==================== ROLE ====================
    public interface IRoleService
    {
        Task<List<RoleResponse>> GetAllRolesAsync();
        Task<RoleResponse?> GetRoleByIdAsync(int id);
        Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request);
        Task<RoleResponse> UpdateRoleAsync(int id, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(int id);
    }

    // ==================== PERMISSION ====================
    public interface IPermissionService
    {
        Task<List<PermissionResponse>> GetAllPermissionsAsync();
        Task<PermissionResponse> CreatePermissionAsync(CreatePermissionRequest request);
        Task<bool> DeletePermissionAsync(int id);
    }

    // ==================== STAFF ====================
    public interface IStaffService
    {
        Task<List<StaffResponse>> GetAllStaffAsync(int tenantId);
        Task<StaffResponse?> GetStaffByIdAsync(int id);
        Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request, int tenantId);
        Task<StaffResponse> UpdateStaffAsync(int id, UpdateStaffRequest request);
        Task<bool> DeleteStaffAsync(int id);
    }

    // ==================== BUSINESS ====================
    public interface IBusinessService
    {
        Task<BusinessResponse?> GetBusinessByIdAsync(int id);
        Task<BusinessResponse> CreateBusinessAsync(CreateBusinessRequest request);
        Task<BusinessResponse> UpdateBusinessAsync(int id, UpdateBusinessRequest request);
        Task<bool> UploadLogoAsync(int businessId, string logoUrl);
        Task<List<BusinessResponse>> GetAllBusinessesAsync(); // Admin
    }

    // ==================== BUSINESS TYPE ====================
    public interface IBusinessTypeService
    {
        Task<List<BusinessType>> GetAllAsync();
        Task<BusinessType> CreateAsync(CreateBusinessTypeRequest request);
        Task<BusinessType> UpdateAsync(int id, CreateBusinessTypeRequest request);
        Task<bool> DeleteAsync(int id);
    }

    // ==================== BRANCH ====================
    public interface IBranchService
    {
        Task<List<BranchResponse>> GetAllBranchesAsync(int tenantId);
        Task<BranchResponse?> GetBranchByIdAsync(int id);
        Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, int tenantId);
        Task<BranchResponse> UpdateBranchAsync(int id, UpdateBranchRequest request);
        Task<bool> DeleteBranchAsync(int id);
    }

    // ==================== BUSINESS SETTINGS ====================
    public interface IBusinessSettingsService
    {
        Task<BusinessSettings?> GetSettingsAsync(int tenantId);
        Task<BusinessSettings> UpdateSettingsAsync(int tenantId, UpdateBusinessSettingsRequest request);
        Task<InvoiceSettings?> GetInvoiceSettingsAsync(int tenantId);
        Task<InvoiceSettings> UpdateInvoiceSettingsAsync(int tenantId, UpdateInvoiceSettingsRequest request);
    }

    // ==================== PRODUCT ====================
    public interface IProductService
    {
        Task<(List<ProductResponse> Items, int TotalCount)> GetAllProductsAsync(int tenantId, PaginationRequest pagination);
        Task<ProductResponse?> GetProductByIdAsync(int id, int tenantId);
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request, int tenantId);
        Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request, int tenantId);
        Task<bool> DeleteProductAsync(int id, int tenantId);
        Task<List<ProductResponse>> SearchProductsAsync(int tenantId, string query);
        Task<ProductResponse?> GetProductByBarcodeAsync(int tenantId, string barcode);
        Task<List<ProductResponse>> BulkUploadAsync(BulkProductUploadRequest request, int tenantId);
    }

    // ==================== CATEGORY ====================
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAllCategoriesAsync(int tenantId);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, int tenantId);
        Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }

    // ==================== BRAND ====================
    public interface IBrandService
    {
        Task<List<Brand>> GetAllBrandsAsync(int tenantId);
        Task<Brand> CreateBrandAsync(CreateBrandRequest request, int tenantId);
        Task<Brand> UpdateBrandAsync(int id, CreateBrandRequest request);
        Task<bool> DeleteBrandAsync(int id);
    }

    // ==================== UNIT ====================
    public interface IUnitService
    {
        Task<List<Unit>> GetAllUnitsAsync(int tenantId);
        Task<Unit> CreateUnitAsync(CreateUnitRequest request, int tenantId);
        Task<Unit> UpdateUnitAsync(int id, CreateUnitRequest request);
        Task<bool> DeleteUnitAsync(int id);
    }

    // ==================== TAX ====================
    public interface ITaxService
    {
        Task<List<TaxRateResponse>> GetAllTaxRatesAsync(int tenantId);
        Task<TaxRateResponse> CreateTaxRateAsync(CreateTaxRateRequest request, int tenantId);
        Task<TaxRateResponse> UpdateTaxRateAsync(int id, UpdateTaxRateRequest request);
        Task<bool> DeleteTaxRateAsync(int id);
    }

    // ==================== DISCOUNT ====================
    public interface IDiscountService
    {
        Task<List<Discount>> GetAllDiscountsAsync(int tenantId);
        Task<Discount> CreateDiscountAsync(CreateDiscountRequest request, int tenantId);
        Task<Discount> UpdateDiscountAsync(int id, UpdateDiscountRequest request);
        Task<bool> DeleteDiscountAsync(int id);
    }

    // ==================== BARCODE ====================
    public interface IBarcodeService
    {
        Task<string> GenerateBarcodeAsync(int productId, int tenantId);
        Task<byte[]> GetBarcodeImageAsync(int productId, int tenantId);
    }
}
