using MasterBilling.API.Models.Enums;

namespace MasterBilling.API.Models.Entities
{
    public class Business : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public int? BusinessTypeId { get; set; }
        public BusinessType? BusinessType { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? CurrencyCode { get; set; } = "INR";
        public string? CurrencySymbol { get; set; } = "₹";
        public BusinessStatus Status { get; set; } = BusinessStatus.Active;
        public string? SuspendedReason { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public BusinessSettings? Settings { get; set; }
        public InvoiceSettings? InvoiceSettings { get; set; }
    }

    public class BusinessType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    public class Branch : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsMainBranch { get; set; } = false;
    }

    public class BusinessSettings : TenantEntity
    {
        public string? DateFormat { get; set; } = "dd-MM-yyyy";
        public string? TimeZone { get; set; } = "Asia/Kolkata";
        public string? Language { get; set; } = "en";
        public bool EnableGST { get; set; } = true;
        public bool EnableMultiBranch { get; set; } = false;
        public bool EnableBarcode { get; set; } = true;
        public bool EnableLowStockAlert { get; set; } = true;
        public int LowStockThreshold { get; set; } = 10;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableWhatsAppNotifications { get; set; } = false;
        public bool EnableSmsNotifications { get; set; } = false;
    }

    public class InvoiceSettings : TenantEntity
    {
        public string? InvoicePrefix { get; set; } = "INV";
        public int NextInvoiceNumber { get; set; } = 1;
        public string? InvoiceNumberFormat { get; set; } = "{PREFIX}-{YEAR}-{NUMBER:5}";
        public string? TermsAndConditions { get; set; }
        public string? FooterNote { get; set; }
        public bool ShowLogo { get; set; } = true;
        public bool ShowGST { get; set; } = true;
        public bool ShowDiscount { get; set; } = true;
        public bool ShowSignature { get; set; } = false;
        public string? SignatureImageUrl { get; set; }
        public string? TemplateName { get; set; } = "Default";
        public string? PrimaryColor { get; set; } = "#1a73e8";
    }
}
