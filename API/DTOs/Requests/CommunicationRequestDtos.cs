using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.DTOs.Requests
{
    // ==================== COMMUNICATION ====================
    public class SendInvoiceEmailRequest
    {
        public int InvoiceId { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Message { get; set; }
    }

    public class SendInvoiceWhatsAppRequest
    {
        public int InvoiceId { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class SendEmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }
    }

    public class SendWhatsAppRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? MediaUrl { get; set; }
        public string? TemplateName { get; set; }
        public List<string>? TemplateParameters { get; set; }
    }

    public class SendSmsRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class SendInvoiceSmsRequest
    {
        public int InvoiceId { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    /// <summary>
    /// Used by the post-billing dialog – the user ticks which channels to send on.
    /// </summary>
    public class PostBillingSendRequest
    {
        public int InvoiceId { get; set; }

        // ── Email ──────────────────────────────────────────────────────────────
        public bool SendEmail { get; set; } = false;
        public string? EmailAddress { get; set; }
        public string? EmailSubject { get; set; }
        public string? EmailMessage { get; set; }

        // ── WhatsApp ───────────────────────────────────────────────────────────
        public bool SendWhatsApp { get; set; } = false;
        public string? WhatsAppNumber { get; set; }
        public string? WhatsAppMessage { get; set; }

        // ── SMS ────────────────────────────────────────────────────────────────
        public bool SendSms { get; set; } = false;
        public string? SmsNumber { get; set; }
        public string? SmsMessage { get; set; }
    }

    public class UpdateNotificationSettingsRequest
    {
        public bool? EmailEnabled { get; set; }
        public bool? WhatsAppEnabled { get; set; }
        public bool? SmsEnabled { get; set; }
        public bool? LowStockAlert { get; set; }
        public bool? PaymentDueAlert { get; set; }
        public bool? NewOrderAlert { get; set; }
    }

    // ==================== SUBSCRIPTION ====================
    public class SubscribeRequest
    {
        public int PlanId { get; set; }
        public string? BillingCycle { get; set; } = "Monthly";
        public string? PaymentGateway { get; set; }
        public string? CouponCode { get; set; }
    }

    public class CancelSubscriptionRequest
    {
        public string? Reason { get; set; }
    }

    public class CreateSubscriptionPlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public int MaxProducts { get; set; } = 100;
        public int MaxUsers { get; set; } = 3;
        public int MaxBranches { get; set; } = 1;
        public int MaxInvoicesPerMonth { get; set; } = 500;
        public bool HasWhatsAppIntegration { get; set; }
        public bool HasSmsIntegration { get; set; }
        public bool HasEmailIntegration { get; set; }
        public bool HasReports { get; set; }
        public bool HasMultiCurrency { get; set; }
        public bool HasInventoryManagement { get; set; }
    }

    // ==================== FILE ====================
    public class FileUploadRequest
    {
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
    }

    // ==================== REPORT FILTERS ====================
    public class ReportFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? BranchId { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public string? Status { get; set; }
        public string? GroupBy { get; set; } // day, week, month, year
    }

    public class ExportReportRequest
    {
        public string ReportType { get; set; } = string.Empty; // Sales, Purchase, SalesReturn, PurchaseReturn, LowStock, CreditBill, ProductStock, ExpiredStock
        public string Format { get; set; } = "PDF"; // PDF, Excel, CSV
        public ReportFilterRequest? Filter { get; set; }
    }

    // ==================== ADMIN ====================
    public class SuspendShopRequest
    {
        public string? Reason { get; set; }
    }
}
