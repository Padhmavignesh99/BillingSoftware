using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;

namespace MasterBilling.API.Services.Interfaces
{
    // ==================== FULL PAYMENT SERVICE ====================
    public interface IFullPaymentService
    {
        // Core payments
        Task<PaymentResponse> CreatePaymentAsync(CreateFullPaymentRequest request, int tenantId);
        Task<PaymentResponse?> GetPaymentAsync(int id, int tenantId);
        Task<(List<PaymentResponse> Items, int Total)> GetPaymentsAsync(int tenantId, PaginationRequest p);
        Task<List<PaymentResponse>> GetDueListAsync(int tenantId);
        Task<PaymentResponse> CollectDueAsync(CollectDueRequest request, int tenantId);

        // POS Terminals
        Task<List<PaymentTerminalResponse>> GetTerminalsAsync(int tenantId);
        Task<PaymentTerminalResponse> RegisterTerminalAsync(RegisterTerminalRequest req, int tenantId);
        Task<PaymentTerminalResponse> UpdateTerminalAsync(int id, UpdateTerminalRequest req, int tenantId);
        Task<TerminalConnectionTestResponse> TestTerminalConnectionAsync(int id, int tenantId);

        // Terminal transactions
        Task<PaymentTransactionResponse> InitiateTerminalTransactionAsync(InitiateTerminalPaymentRequest req, int tenantId);
        Task<PaymentTransactionResponse?> GetTransactionStatusAsync(int id, int tenantId);
        Task<PaymentTransactionResponse> CancelTransactionAsync(int id, int tenantId);

        // Webhooks
        Task ProcessWebhookAsync(string provider, PaymentWebhookRequest req);

        // Refunds
        Task<RefundResponse> CreateRefundAsync(CreateRefundRequest req, int tenantId);
        Task<List<RefundResponse>> GetRefundsAsync(int? paymentId, int tenantId);

        // Configuration
        Task<List<PaymentMethodConfigResponse>> GetPaymentConfigAsync(int tenantId, int? branchId);
        Task UpdatePaymentConfigAsync(UpdatePaymentConfigRequest req, int tenantId, int? branchId);

        // Reporting
        Task<PaymentSummaryResponse> GetDailySummaryAsync(int tenantId, DateTime? date, int? branchId);
        Task<List<PaymentProviderResponse>> GetProvidersAsync(int tenantId);
    }
}
