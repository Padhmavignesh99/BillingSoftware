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
        Task<MessageSendResponse> SendInvoiceSmsAsync(int invoiceId, string mobileNumber, string? message, int tenantId);
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

    // ==================== POST-BILLING NOTIFICATION ====================
    /// <summary>
    /// Orchestrates sending Email / WhatsApp / SMS after a billing event.
    /// Enforces subscription plan feature gates before dispatching.
    /// </summary>
    public interface IPostBillingNotificationService
    {
        /// <summary>
        /// Sends to whichever channels are ticked in <paramref name="request"/> and
        /// allowed by the tenant's active subscription plan.
        /// Returns a per-channel result summary.
        /// </summary>
        Task<PostBillingNotificationResult> SendAsync(PostBillingSendRequest request, int tenantId);

        /// <summary>Check whether the tenant plan allows WhatsApp messaging.</summary>
        Task<bool> IsWhatsAppEnabledAsync(int tenantId);

        /// <summary>Check whether the tenant plan allows SMS messaging.</summary>
        Task<bool> IsSmsEnabledAsync(int tenantId);

        /// <summary>Email is enabled on all plans; always returns true.</summary>
        Task<bool> IsEmailEnabledAsync(int tenantId);
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
        // ── Core Settings CRUD ────────────────────────────────────────────────────
        /// <summary>
        /// Returns the most specific print settings for the authenticated tenant,
        /// using the three-level hierarchy: counter → branch → tenant.
        /// If no settings exist yet, system defaults are returned (not persisted).
        /// </summary>
        Task<PrintSettingsResponse> GetPrintSettingsAsync(int tenantId, int? branchId = null, int? counterId = null);

        /// <summary>Creates or fully replaces the print settings for the given scope.</summary>
        Task<PrintSettingsResponse> UpdatePrintSettingsAsync(int tenantId, UpdatePrintSettingsRequest request, int? branchId = null, int? counterId = null);

        /// <summary>Partially updates only the supplied non-null fields (PATCH semantics).</summary>
        Task<PrintSettingsResponse> PatchPrintSettingsAsync(int tenantId, PatchPrintSettingsRequest request, int? branchId = null, int? counterId = null);

        /// <summary>Resets the settings for the given scope back to system defaults.</summary>
        Task<PrintSettingsResponse> ResetToDefaultsAsync(int tenantId, int? branchId = null, int? counterId = null);

        /// <summary>
        /// Returns all settings configured for the tenant (tenant-level + all branch + all counter overrides).
        /// Useful for the admin settings page showing the full hierarchy.
        /// </summary>
        Task<List<PrintSettingsResponse>> GetAllScopedSettingsAsync(int tenantId);

        // ── Preview ───────────────────────────────────────────────────────────────
        /// <summary>Returns a preview-ready HTML page rendered with the current print settings.</summary>
        Task<string> GetPrintPreviewHtmlAsync(int tenantId, string documentType, int? branchId = null, int? counterId = null);

        // ── Lookup Data ───────────────────────────────────────────────────────────
        /// <summary>Returns the supported paper sizes with their dimensions.</summary>
        List<PaperSizeInfo> GetPaperSizes();

        /// <summary>Returns the supported printer types with descriptions.</summary>
        List<PrinterTypeInfo> GetPrinterTypes();

        /// <summary>Returns the supported printer connection types.</summary>
        List<string> GetConnectionTypes();

        // ── Printer Test ──────────────────────────────────────────────────────────
        /// <summary>
        /// Validates the printer configuration and returns a connectivity status.
        /// For network printers it attempts a TCP socket check; for others it confirms configuration only.
        /// </summary>
        Task<PrinterTestResult> TestPrinterAsync(int tenantId, PrinterTestRequest request);

        // ── Invoice Print Data ────────────────────────────────────────────────────
        /// <summary>
        /// Returns the invoice data formatted for printing, including the tenant's active print settings.
        /// The Angular/browser layer uses this to render the invoice and call window.print().
        /// </summary>
        Task<InvoicePrintDataResponse> GetInvoicePrintDataAsync(int invoiceId, int tenantId);

        // ── Print Job Management ──────────────────────────────────────────────────
        /// <summary>Queues a new print job and returns the job record.</summary>
        Task<PrintJobResponse> QueuePrintJobAsync(int tenantId, QueuePrintJobRequest request);

        /// <summary>Updates the status of an existing print job (e.g. Printing → Printed | Failed).</summary>
        Task<PrintJobResponse> UpdatePrintJobStatusAsync(int jobId, int tenantId, string status, string? errorMessage = null);

        /// <summary>Retries a failed print job if it has not exceeded MaxRetries.</summary>
        Task<PrintJobResponse> RetryPrintJobAsync(int jobId, int tenantId);

        /// <summary>
        /// Creates a reprint job for an already-printed document.
        /// Marks the new job with IsReprint = true and links it to the original job.
        /// </summary>
        Task<PrintJobResponse> ReprintAsync(int originalJobId, int tenantId, int? userId);

        /// <summary>Returns print job history for the tenant with optional filters.</summary>
        Task<List<PrintJobResponse>> GetPrintJobsAsync(int tenantId, string? status = null, string? documentType = null, int page = 1, int pageSize = 20);

        /// <summary>Gets a single print job by ID.</summary>
        Task<PrintJobResponse?> GetPrintJobByIdAsync(int jobId, int tenantId);

        /// <summary>Cancels a queued print job.</summary>
        Task<bool> CancelPrintJobAsync(int jobId, int tenantId);
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
