namespace BillingSoftware.API.Models.Entities
{
    public class ReminderRule : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public int DaysOverdue { get; set; } = 0; // 0=on due date, 7=7 days overdue
        public bool SendWhatsApp { get; set; } = true;
        public bool SendEmail { get; set; } = false;
        public bool SendSms { get; set; } = false;
        public string MessageTemplate { get; set; } = "Dear {CustomerName}, your invoice {InvoiceNumber} of ₹{Amount} is overdue. Please pay at the earliest.";
        public bool IsEnabled { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class ReminderLog : TenantEntity
    {
        public int? ReminderRuleId { get; set; }
        public ReminderRule? ReminderRule { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public string Channel { get; set; } = "WhatsApp"; // WhatsApp, Email, SMS
        public string Status { get; set; } = "Sent"; // Sent, Failed, Pending
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

