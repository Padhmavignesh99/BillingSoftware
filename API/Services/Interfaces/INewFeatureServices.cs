using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;

namespace BillingSoftware.API.Services.Interfaces
{
    // ─── Delivery Challan ────────────────────────────────────────────────────────
    public interface IDeliveryChallanService
    {
        Task<(List<DeliveryChallanResponse> Items, int Total)> GetAllAsync(int tenantId, PaginationRequest p);
        Task<DeliveryChallanResponse?> GetByIdAsync(int id, int tenantId);
        Task<DeliveryChallanResponse> CreateAsync(CreateDeliveryChallanRequest r, int tenantId);
        Task<DeliveryChallanResponse> UpdateStatusAsync(int id, string status, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<InvoiceResponse> ConvertToInvoiceAsync(int id, int tenantId);
        Task<string> GetNextChallanNumberAsync(int tenantId);
    }

    // ─── Recurring Invoice ────────────────────────────────────────────────────────
    public interface IRecurringInvoiceService
    {
        Task<(List<RecurringInvoiceResponse> Items, int Total)> GetAllAsync(int tenantId, PaginationRequest p);
        Task<RecurringInvoiceResponse?> GetByIdAsync(int id, int tenantId);
        Task<RecurringInvoiceResponse> CreateAsync(CreateRecurringInvoiceRequest r, int tenantId);
        Task<RecurringInvoiceResponse> UpdateAsync(int id, CreateRecurringInvoiceRequest r, int tenantId);
        Task<RecurringInvoiceResponse> ToggleStatusAsync(int id, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<InvoiceResponse> GenerateNowAsync(int id, int tenantId);
    }

    // ─── Stock Take ─────────────────────────────────────────────────────────────
    public interface IStockTakeService
    {
        Task<(List<StockTakeSessionResponse> Items, int Total)> GetSessionsAsync(int tenantId, PaginationRequest p);
        Task<StockTakeSessionResponse?> GetSessionByIdAsync(int id, int tenantId);
        Task<StockTakeSessionResponse> CreateSessionAsync(CreateStockTakeRequest r, int tenantId);
        Task BulkUpdateItemsAsync(int sessionId, List<UpdateStockTakeItemRequest> items, int tenantId);
        Task<StockTakeSessionResponse> CompleteSessionAsync(int id, int tenantId);
        Task<int> ApplyAdjustmentsAsync(int id, int tenantId);
        Task<bool> DeleteSessionAsync(int id, int tenantId);
    }

    // ─── Loyalty ─────────────────────────────────────────────────────────────────
    public interface ILoyaltyService
    {
        Task<LoyaltyProgramResponse> GetProgramAsync(int tenantId);
        Task<LoyaltyProgramResponse> UpdateProgramAsync(UpdateLoyaltyProgramRequest r, int tenantId);
        Task<List<LoyaltyTierResponse>> GetTiersAsync(int tenantId);
        Task<LoyaltyTierResponse> CreateTierAsync(CreateLoyaltyTierRequest r, int tenantId);
        Task<bool> DeleteTierAsync(int id, int tenantId);
        Task<LoyaltyAccountResponse> GetCustomerAccountAsync(int customerId, int tenantId);
        Task<LoyaltyAccountResponse> EarnPointsAsync(EarnPointsRequest r, int tenantId);
        Task<LoyaltyAccountResponse> RedeemPointsAsync(RedeemPointsRequest r, int tenantId);
        Task<LoyaltyAccountResponse> AdjustPointsAsync(AdjustPointsRequest r, int tenantId);
        Task<RedeemCheckResponse> CheckRedemptionAsync(int customerId, decimal points, int tenantId);
        Task<List<LoyaltyAccountResponse>> GetTopCustomersAsync(int tenantId, int take = 10);
    }

    // ─── Reminders ────────────────────────────────────────────────────────────────
    public interface IReminderService
    {
        Task<List<ReminderRuleResponse>> GetRulesAsync(int tenantId);
        Task<ReminderRuleResponse> CreateRuleAsync(CreateReminderRuleRequest r, int tenantId);
        Task<ReminderRuleResponse> UpdateRuleAsync(int id, CreateReminderRuleRequest r, int tenantId);
        Task<bool> DeleteRuleAsync(int id, int tenantId);
        Task<List<OverdueCustomerResponse>> GetOverdueCustomersAsync(int tenantId);
        Task<List<ReminderLogResponse>> GetLogsAsync(int tenantId, PaginationRequest p);
        Task SendManualReminderAsync(SendManualReminderRequest r, int tenantId);
    }

    // ─── Reorder ─────────────────────────────────────────────────────────────────
    public interface IReorderService
    {
        Task<List<ReorderSuggestion>> GetReorderSuggestionsAsync(int tenantId);
        Task<object> CreatePurchaseOrderFromSuggestionsAsync(List<int> productIds, int tenantId);
    }
}
