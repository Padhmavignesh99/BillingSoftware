using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;

namespace MasterBilling.API.Services.Interfaces
{
    // =====================================================================
    // WAREHOUSE
    // =====================================================================
    public interface IWarehouseService
    {
        Task<List<WarehouseResponse>> GetAllAsync(int tenantId, int? branchId = null);
        Task<WarehouseResponse?>      GetByIdAsync(int id, int tenantId);
        Task<WarehouseResponse>       CreateAsync(CreateWarehouseRequest request, int tenantId);
        Task<WarehouseResponse>       UpdateAsync(int id, UpdateWarehouseRequest request, int tenantId);
        Task<bool>                    DeleteAsync(int id, int tenantId);
        Task<WarehouseResponse>       SetDefaultAsync(int id, int tenantId);
    }

    // =====================================================================
    // COUNTER
    // =====================================================================
    public interface ICounterService
    {
        Task<List<CounterResponse>> GetAllAsync(int tenantId, int? branchId = null);
        Task<CounterResponse?>      GetByIdAsync(int id, int tenantId);
        Task<CounterResponse>       CreateAsync(CreateCounterRequest request, int tenantId);
        Task<CounterResponse>       UpdateAsync(int id, UpdateCounterRequest request, int tenantId);
        Task<bool>                  DeleteAsync(int id, int tenantId);
        Task<CounterResponse>       AssignUserAsync(int counterId, int userId, int tenantId);
        Task<CounterResponse>       SetOnlineStatusAsync(int counterId, bool isOnline, int tenantId);
    }

    // =====================================================================
    // EMPLOYEE ROLE
    // =====================================================================
    public interface IEmployeeRoleService
    {
        Task<List<EmployeeRoleResponse>>   GetAllAsync(int tenantId);
        Task<EmployeeRoleResponse?>        GetByIdAsync(int id, int tenantId);
        Task<EmployeeRoleResponse>         CreateAsync(CreateEmployeeRoleRequest request, int tenantId);
        Task<EmployeeRoleResponse>         UpdateAsync(int id, UpdateEmployeeRoleRequest request, int tenantId);
        Task<bool>                         DeleteAsync(int id, int tenantId);
        Task<EmployeeRoleResponse>         SetPermissionsAsync(int roleId, SetRolePermissionsRequest request, int tenantId);
        Task<List<PermissionGroupResponse>> GetAllPermissionsAsync();
        Task<List<UserRoleResponse>>       GetUsersInRoleAsync(int roleId, int tenantId);
    }

    // =====================================================================
    // USER ROLE ASSIGNMENT
    // =====================================================================
    public interface IUserRoleService
    {
        Task<List<UserRoleResponse>> GetUserRolesAsync(int userId, int tenantId);
        Task<List<UserRoleResponse>> GetTenantUsersAsync(int tenantId);
        Task<UserRoleResponse>       AssignRoleAsync(AssignUserRoleRequest request, int tenantId);
        Task<bool>                   RevokeRoleAsync(RevokeUserRoleRequest request, int tenantId);
        Task<List<string>>           GetUserPermissionsAsync(int userId, int tenantId);
        Task<bool>                   HasPermissionAsync(int userId, int tenantId, string permissionKey);
    }

    // =====================================================================
    // SHOP INVITE
    // =====================================================================
    public interface IShopInviteService
    {
        Task<List<ShopInviteResponse>> GetAllAsync(int tenantId);
        Task<ShopInviteResponse>       SendInviteAsync(SendShopInviteRequest request, int tenantId, int invitedByUserId);
        Task<ShopInviteResponse>       AcceptInviteAsync(AcceptShopInviteRequest request);
        Task<bool>                     CancelInviteAsync(int inviteId, int tenantId);
        Task<bool>                     ResendInviteAsync(int inviteId, int tenantId);
    }

    // =====================================================================
    // SUBSCRIPTION USAGE
    // =====================================================================
    public interface ISubscriptionUsageService
    {
        Task<SubscriptionUsageResponse> GetCurrentUsageAsync(int tenantId);
        Task<List<SubscriptionUsageResponse>> GetUsageHistoryAsync(int tenantId, int months = 6);
        Task IncrementInvoiceCountAsync(int tenantId);
        Task RecalculateAsync(int tenantId);
        Task<bool> IsWithinLimitAsync(int tenantId, string limitType);
        Task<List<UsageAlertResponse>> GetActiveAlertsAsync(int tenantId);
        Task AcknowledgeAlertAsync(int alertId, int tenantId, int userId);
    }

    // =====================================================================
    // AI SERVICE
    // =====================================================================
    public interface IAIInsightService
    {
        Task<List<AIInsightResponse>>   GetInsightsAsync(int tenantId, string? type = null, bool unreadOnly = false, int page = 1, int pageSize = 20);
        Task<AIInsightResponse?>        GetByIdAsync(int id, int tenantId);
        Task<AIInsightResponse>         GenerateInsightAsync(GenerateInsightRequest request, int tenantId, int userId);
        Task<AIInsightResponse>         MarkReadAsync(int id, int tenantId, MarkInsightReadRequest request);
        Task<bool>                      DeleteInsightAsync(int id, int tenantId);
        Task<int>                       GetUnreadCountAsync(int tenantId);

        // Forecasts
        Task<List<AIForecastResponse>>  GetForecastsAsync(int tenantId, int? productId = null, string granularity = "Day", int days = 30);
        Task<AIForecastResponse>        GenerateForecastAsync(int tenantId, int productId, string granularity, int userId);
        Task                            UpdateActualQtyAsync(int tenantId, int productId, DateTime date, decimal actualQty);

        // High-level summaries
        Task<object> GetSalesSummaryAsync(int tenantId, DateTime from, DateTime to, int? branchId = null);
        Task<List<object>> GetReorderSuggestionsAsync(int tenantId, int? branchId = null);
    }
}
