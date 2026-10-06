namespace BillingSoftware.API.Configuration
{
    /// <summary>
    /// Controls which database provider is used at runtime.
    /// Set <see cref="ActiveProvider"/> in appsettings.json → DataSettings section.
    /// </summary>
    public class DataSettings
    {
        /// <summary>
        /// The active database provider key.
        /// Must match a key in the ConnectionStrings section of appsettings.json.
        /// Accepted values: "MySql" | "MSSql" | "PostgreSQL"
        /// Cloud variants:   "MySql_Cloud" | "MSSql_Cloud" | "PostgreSQL_Cloud"
        /// </summary>
        public string ActiveProvider { get; set; } = "MSSql";
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    public class EmailSettings
    {
        public string Provider { get; set; } = "SMTP"; // SMTP or SendGrid
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    public class WhatsAppSettings
    {
        public string Provider { get; set; } = "Meta"; // Meta or Twilio
        public string PhoneNumberId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string TemplateName { get; set; } = "invoice_notification";
        public string ApiBaseUrl { get; set; } = "https://graph.facebook.com/v19.0";
        public string? TwilioAccountSid { get; set; }
        public string? TwilioAuthToken { get; set; }
        public string? TwilioFromNumber { get; set; }
    }

    public class SmsSettings
    {
        public string Provider { get; set; } = "Twilio";
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }

    public class StorageSettings
    {
        public string Provider { get; set; } = "Local"; // Local, S3, Azure
        public string BasePath { get; set; } = "uploads";
        public string BaseUrl { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; } = 10485760; // 10MB
        public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
    }
}
