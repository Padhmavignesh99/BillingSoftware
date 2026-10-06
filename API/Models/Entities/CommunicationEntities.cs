using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    public class WhatsAppMessage : TenantEntity
    {
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? MessageType { get; set; }
        public string? MessageContent { get; set; }
        public string? MediaUrl { get; set; }
        public string? WhatsAppMessageId { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Pending;
        public DateTime? SentDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EmailMessage : TenantEntity
    {
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? MessageContent { get; set; }
        public string? AttachmentPath { get; set; }
        public string? ProviderMessageId { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Pending;
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SmsMessage : TenantEntity
    {
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public string? ProviderMessageId { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Pending;
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class Notification : TenantEntity
    {
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public string? ActionUrl { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
    }
}
