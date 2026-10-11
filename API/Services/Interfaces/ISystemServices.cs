using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;

namespace BillingSoftware.API.Services.Interfaces
{
    // ==================== DASHBOARD ====================
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetSummaryAsync(int tenantId);
        Task<DashboardChartResponse> GetChartsAsync(int tenantId, DateTime? fromDate, DateTime? toDate);
    }

    // ==================== REPORTS ====================
    public interface IReportService
    {
        // Existing reports
        Task<SalesSummaryResponse> GetSalesSummaryAsync(int tenantId, ReportFilterRequest filter);
        Task<ProfitLossResponse> GetProfitLossAsync(int tenantId, ReportFilterRequest filter);
        Task<TaxReportResponse> GetTaxReportAsync(int tenantId, ReportFilterRequest filter);
        Task<StockReportResponse> GetStockReportAsync(int tenantId, ReportFilterRequest filter);
        Task<byte[]> ExportReportAsync(int tenantId, ExportReportRequest request);
        Task<object> GetInventoryReportAsync(int tenantId, ReportFilterRequest filter);
        Task<object> GetCustomerReportAsync(int tenantId, ReportFilterRequest filter);
        Task<object> GetSupplierReportAsync(int tenantId, ReportFilterRequest filter);
        Task<object> GetExpenseReportAsync(int tenantId, ReportFilterRequest filter);

        // New typed reports
        Task<PurchaseReportResponse> GetPurchaseReportAsync(int tenantId, ReportFilterRequest filter);
        Task<SalesReturnReportResponse> GetSalesReturnReportAsync(int tenantId, ReportFilterRequest filter);
        Task<PurchaseReturnReportResponse> GetPurchaseReturnReportAsync(int tenantId, ReportFilterRequest filter);
        Task<LowStockReportResponse> GetLowStockReportAsync(int tenantId);
        Task<CreditBillReportResponse> GetCreditBillReportAsync(int tenantId, int? userId = null);
        Task<ProductMiniStatementResponse> GetProductMiniStatementAsync(int tenantId, int productId, ReportFilterRequest filter);
        Task<ProductStockReportResponse> GetProductStockReportAsync(int tenantId, ReportFilterRequest filter);
        Task<ExpiredStockReportResponse> GetExpiredStockReportAsync(int tenantId, int daysAhead = 30);
    }

    // ==================== EMAIL ====================
    public interface IEmailService
    {
        Task<MessageSendResponse> SendInvoiceEmailAsync(int invoiceId, string toEmail, string? subject, string? message, int tenantId);
        Task<MessageSendResponse> SendPaymentReceiptEmailAsync(int paymentId, string toEmail, int tenantId);
        Task<MessageSendResponse> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<MessageSendResponse> SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task<MessageSendResponse> SendEmailAsync(SendEmailRequest request, int tenantId);
        Task<List<CommunicationLogResponse>> GetEmailHistoryAsync(int tenantId);
    }

    // ==================== WHATSAPP ====================
    public interface IWhatsAppService
    {
        Task<MessageSendResponse> SendInvoiceWhatsAppAsync(int invoiceId, string mobileNumber, string? message, int tenantId);
        Task<MessageSendResponse> SendPaymentReceiptWhatsAppAsync(int paymentId, string mobileNumber, int tenantId);
        Task<MessageSendResponse> SendWhatsAppAsync(SendWhatsAppRequest request, int tenantId);
        Task<List<CommunicationLogResponse>> GetMessageHistoryAsync(int tenantId);
        Task<bool> HandleWebhookAsync(string payload);
    }

    // ==================== SMS ====================
    public interface ISmsService
    {
        Task<MessageSendResponse> SendSmsAsync(SendSmsRequest request, int tenantId);
        Task<List<CommunicationLogResponse>> GetSmsHistoryAsync(int tenantId);
    }

    // ==================== NOTIFICATION ====================
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetNotificationsAsync(int tenantId, int userId);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> UpdateSettingsAsync(int tenantId, UpdateNotificationSettingsRequest request);
        Task<Notification> CreateNotificationAsync(int tenantId, int userId, string title, string message, string type);
    }

    // ==================== INVOICE DOCUMENT ====================
    public interface IInvoiceDocumentService
    {
        Task<byte[]> GeneratePdfAsync(int invoiceId, int tenantId);
        Task<string> GenerateAndSavePdfAsync(int invoiceId, int tenantId);
        Task<byte[]> GetPdfAsync(int invoiceId, int tenantId);
        Task<string> GetPreviewHtmlAsync(int invoiceId, int tenantId);
    }

    // ==================== SUBSCRIPTION ====================
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlanResponse>> GetPlansAsync();
        Task<SubscriptionResponse?> GetCurrentSubscriptionAsync(int businessId);
        Task<SubscriptionResponse> SubscribeAsync(SubscribeRequest request, int businessId);
        Task<bool> CancelSubscriptionAsync(int businessId, CancelSubscriptionRequest request);
        Task<bool> HandleWebhookAsync(string payload);
    }

    public interface ISubscriptionPlanService
    {
        Task<List<SubscriptionPlanResponse>> GetAllPlansAsync();
        Task<SubscriptionPlanResponse?> GetPlanByIdAsync(int id);
        Task<SubscriptionPlanResponse> CreatePlanAsync(CreateSubscriptionPlanRequest request);
        Task<SubscriptionPlanResponse> UpdatePlanAsync(int id, CreateSubscriptionPlanRequest request);
        Task<bool> DeletePlanAsync(int id);
    }

    public interface ISubscriptionPaymentService
    {
        Task<List<SubscriptionPayment>> GetPaymentsAsync(int businessId);
    }

    public interface ITrialService
    {
        Task<Trial?> GetActiveTrialAsync(int businessId);
        Task<Trial> StartTrialAsync(int businessId);
        Task<bool> ConvertTrialAsync(int businessId, int planId);
    }

    public interface IPlanFeatureService
    {
        Task<List<PlanFeature>> GetFeaturesAsync(int planId);
        Task<PlanFeature> AddFeatureAsync(int planId, string featureName, string? description);
        Task<bool> RemoveFeatureAsync(int featureId);
    }

    // ==================== FILE ====================
    public interface IFileService
    {
        Task<FileResponse> UploadFileAsync(Stream fileStream, string fileName, string contentType, int tenantId, string? referenceType = null, int? referenceId = null);
        Task<FileUpload?> GetFileByIdAsync(int id);
        Task<bool> DeleteFileAsync(int id);
    }

    // ==================== SYSTEM ====================
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetLogsAsync(int tenantId, PaginationRequest pagination);
        Task LogAsync(int? tenantId, int? userId, string action, string entityType, int? entityId, string? oldValues, string? newValues);
    }

    public interface IBackupService
    {
        Task<BackupRecord> CreateBackupAsync(int tenantId);
        Task<List<BackupRecord>> GetBackupsAsync(int tenantId);
        Task<byte[]> DownloadBackupAsync(int backupId);
    }

    public interface ILookupService
    {
        Task<List<Lookup>> GetLookupsByCategoryAsync(string category);
        Task<Lookup> CreateLookupAsync(string category, string key, string value, string? description);
    }

    // ==================== ADMIN ====================
    public interface IAdminService
    {
        // ── Dashboard ────────────────────────────────────────────────────────────
        Task<AdminDashboardResponse> GetDashboardAsync();
        Task<RevenueReportResponse> GetRevenueReportAsync();

        // ── Shops / Tenants ──────────────────────────────────────────────────────
        Task<List<AdminShopResponse>> GetAllShopsAsync();
        Task<AdminShopResponse?> GetShopByIdAsync(int shopId);
        Task<bool> SuspendShopAsync(int shopId, string? reason);
        Task<bool> ActivateShopAsync(int shopId);
        Task<bool> DeleteShopAsync(int shopId);
        Task<bool> UpdateShopAsync(int shopId, AdminUpdateShopRequest request);

        // ── Users ────────────────────────────────────────────────────────────────
        Task<List<AdminUserResponse>> GetAllUsersAsync();
        Task<AdminUserResponse?> GetUserByIdAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ResetUserPasswordAsync(int userId, string newPassword);
        Task<bool> DeleteUserAsync(int userId);

        // ── Subscription Plans (Admin CRUD) ──────────────────────────────────────
        Task<List<AdminPlanResponse>> GetAllPlansAsync();
        Task<AdminPlanResponse?> GetPlanByIdAsync(int planId);
        Task<AdminPlanResponse> CreatePlanAsync(AdminCreatePlanRequest request);
        Task<AdminPlanResponse> UpdatePlanAsync(int planId, AdminCreatePlanRequest request);
        Task<bool> DeletePlanAsync(int planId);

        // ── Plan Features ────────────────────────────────────────────────────────
        Task<List<AdminPlanFeatureResponse>> GetPlanFeaturesAsync(int planId);
        Task<AdminPlanFeatureResponse> AddPlanFeatureAsync(int planId, AdminPlanFeatureRequest request);
        Task<AdminPlanFeatureResponse> UpdatePlanFeatureAsync(int featureId, AdminPlanFeatureRequest request);
        Task<bool> DeletePlanFeatureAsync(int featureId);

        // ── Subscriptions ────────────────────────────────────────────────────────
        Task<List<AdminSubscriptionResponse>> GetAllSubscriptionsAsync();
        Task<AdminSubscriptionResponse?> GetSubscriptionByIdAsync(int id);
        Task<AdminSubscriptionResponse> AssignSubscriptionAsync(AdminAssignSubscriptionRequest request);
        Task<bool> CancelSubscriptionAsync(int subscriptionId, string? reason);
        Task<bool> ExtendSubscriptionAsync(int subscriptionId, int days);
        Task<bool> UpdateSubscriptionStatusAsync(int subscriptionId, string status);

        // ── Trials ──────────────────────────────────────────────────────────────
        Task<List<AdminTrialResponse>> GetAllTrialsAsync();
        Task<AdminTrialResponse?> GetTrialByShopAsync(int shopId);
        Task<AdminTrialResponse> StartTrialForShopAsync(int shopId, int trialDays);
        Task<bool> EndTrialAsync(int shopId);
        Task<bool> ExtendTrialAsync(int shopId, int days);

        // ── Subscription Payments ────────────────────────────────────────────────
        Task<List<AdminSubscriptionPaymentResponse>> GetAllSubscriptionPaymentsAsync();
        Task<AdminSubscriptionPaymentResponse> RecordSubscriptionPaymentAsync(AdminRecordPaymentRequest request);

        // ── Audit Logs ───────────────────────────────────────────────────────────
        Task<List<AdminAuditLogResponse>> GetAuditLogsAsync(int page, int pageSize, int? tenantId);
    }

    // ==================== PRINT SETTINGS ====================
    public interface IPrintSettingsService
    {
        /// <summary>Returns the current print settings for the tenant, or system defaults if not yet configured.</summary>
        Task<PrintSettingsResponse> GetPrintSettingsAsync(int tenantId);

        /// <summary>Creates or fully replaces the print settings for the tenant.</summary>
        Task<PrintSettingsResponse> UpdatePrintSettingsAsync(int tenantId, UpdatePrintSettingsRequest request);

        /// <summary>Partially updates only the supplied fields (PATCH semantics).</summary>
        Task<PrintSettingsResponse> PatchPrintSettingsAsync(int tenantId, PatchPrintSettingsRequest request);

        /// <summary>Resets all print settings back to system defaults for the tenant.</summary>
        Task<PrintSettingsResponse> ResetToDefaultsAsync(int tenantId);

        /// <summary>Returns a preview-ready HTML snippet rendered with the current print settings applied to a sample invoice.</summary>
        Task<string> GetPrintPreviewHtmlAsync(int tenantId, string documentType);
    }

    // ==================== THEME SETTINGS ====================
    public interface IThemeSettingsService
    {
        /// <summary>
        /// Returns the theme settings for the given user.
        /// Creates and returns system defaults if the user has no saved settings yet.
        /// </summary>
        Task<ThemeSettingsResponse> GetThemeSettingsAsync(int userId, int tenantId);

        /// <summary>
        /// Fully replaces (PUT) the user's theme settings.
        /// Creates a new record if one does not yet exist.
        /// </summary>
        Task<ThemeSettingsResponse> UpdateThemeSettingsAsync(int userId, int tenantId, UpdateThemeSettingsRequest request);

        /// <summary>
        /// Partially updates (PATCH) only the non-null fields in the request.
        /// Creates a new record with defaults first if none exists.
        /// </summary>
        Task<ThemeSettingsResponse> PatchThemeSettingsAsync(int userId, int tenantId, PatchThemeSettingsRequest request);

        /// <summary>
        /// Resets the user's theme to system defaults and returns the fresh settings.
        /// </summary>
        Task<ThemeSettingsResponse> ResetToDefaultsAsync(int userId, int tenantId);

        /// <summary>
        /// Applies a named preset (e.g. "Ocean", "Sunset") to the user's settings.
        /// Individual color overrides from the preset are applied; custom fields remain untouched.
        /// Returns the updated settings.
        /// </summary>
        Task<ThemeSettingsResponse> ApplyPresetAsync(int userId, int tenantId, string presetName);

        /// <summary>
        /// Returns the catalogue of all available built-in theme presets.
        /// </summary>
        Task<List<ThemePresetInfo>> GetPresetsAsync();
    }
}
