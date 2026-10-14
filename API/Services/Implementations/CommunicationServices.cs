using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Configuration;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== EMAIL SERVICE ====================
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSettings _settings;
        private readonly IInvoiceService _invoiceService;

        public EmailService(ApplicationDbContext context, IOptions<EmailSettings> settings, IInvoiceService invoiceService)
        {
            _context = context;
            _settings = settings.Value;
            _invoiceService = invoiceService;
        }

        public async Task<MessageSendResponse> SendInvoiceEmailAsync(int invoiceId, string toEmail, string? subject, string? message, int tenantId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, tenantId)
                ?? throw new KeyNotFoundException("Invoice not found");
            var business = await _context.Businesses.FindAsync(tenantId);
            var emailSubject = subject ?? $"Invoice {invoice.InvoiceNumber} - {business?.Name ?? "Billing Software"}";
            var emailBody = message ?? BuildInvoiceEmailBody(invoice, business);
            var emailLog = new EmailMessage
            {
                InvoiceId = invoiceId, EmailAddress = toEmail, Subject = emailSubject,
                MessageContent = emailBody, TenantId = tenantId, Status = MessageStatus.Pending
            };
            _context.EmailMessages.Add(emailLog);
            try
            {
                await SendSmtpEmailAsync(toEmail, emailSubject, emailBody);
                emailLog.Status = MessageStatus.Sent;
                emailLog.SentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = emailLog.Id, Status = "Sent", Message = $"Invoice email sent to {toEmail}" };
            }
            catch (Exception ex)
            {
                emailLog.Status = MessageStatus.Failed;
                emailLog.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = emailLog.Id, Status = "Failed", Message = ex.Message };
            }
        }

        public async Task<MessageSendResponse> SendPaymentReceiptEmailAsync(int paymentId, string toEmail, int tenantId)
        {
            var payment = await _context.Payments.Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == paymentId) ?? throw new KeyNotFoundException("Payment not found");
            return await SendEmailAsync(new SendEmailRequest
            {
                ToEmail = toEmail,
                Subject = $"Payment Receipt - {payment.Invoice?.InvoiceNumber}",
                Body = $"Dear Customer,\n\nPayment of Rs.{payment.Amount:N2} received via {payment.PaymentMethod} for invoice {payment.Invoice?.InvoiceNumber}.\n\nThank you!"
            }, tenantId);
        }

        public async Task<MessageSendResponse> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                await SendSmtpEmailAsync(toEmail, "Welcome to Billing Software",
                    $"Dear {userName},\n\nWelcome! Your account is ready.\n\nRegards,\nBilling Software Team");
                return new MessageSendResponse { Status = "Sent", Message = $"Welcome email sent to {toEmail}" };
            }
            catch (Exception ex) { return new MessageSendResponse { Status = "Failed", Message = ex.Message }; }
        }

        public async Task<MessageSendResponse> SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            try
            {
                await SendSmtpEmailAsync(toEmail, "Password Reset Request",
                    $"Use the following token to reset your password: {resetToken}\n\nThis token expires in 30 minutes.");
                return new MessageSendResponse { Status = "Sent", Message = $"Password reset email sent to {toEmail}" };
            }
            catch (Exception ex) { return new MessageSendResponse { Status = "Failed", Message = ex.Message }; }
        }

        public async Task<MessageSendResponse> SendEmailAsync(SendEmailRequest request, int tenantId)
        {
            var emailLog = new EmailMessage
            {
                EmailAddress = request.ToEmail, Subject = request.Subject,
                MessageContent = request.Body, AttachmentPath = request.AttachmentPath,
                TenantId = tenantId, Status = MessageStatus.Pending
            };
            _context.EmailMessages.Add(emailLog);
            try
            {
                await SendSmtpEmailAsync(request.ToEmail, request.Subject, request.Body, request.AttachmentPath);
                emailLog.Status = MessageStatus.Sent;
                emailLog.SentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = emailLog.Id, Status = "Sent", Message = $"Email sent to {request.ToEmail}" };
            }
            catch (Exception ex)
            {
                emailLog.Status = MessageStatus.Failed;
                emailLog.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = emailLog.Id, Status = "Failed", Message = ex.Message };
            }
        }

        public async Task<List<CommunicationLogResponse>> GetEmailHistoryAsync(int tenantId) =>
            await _context.EmailMessages.Where(e => e.TenantId == tenantId)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new CommunicationLogResponse
                {
                    Id = e.Id, Channel = "Email", Recipient = e.EmailAddress, Subject = e.Subject,
                    Status = e.Status.ToString(), SentDate = e.SentDate, ErrorMessage = e.ErrorMessage, InvoiceId = e.InvoiceId
                }).ToListAsync();

        private async Task SendSmtpEmailAsync(string toEmail, string subject, string body, string? attachmentPath = null)
        {
            // Dev/demo mode: if SmtpHost is empty, skip actual send (record is already logged in DB)
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost)) return;

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };
            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject, Body = body, IsBodyHtml = false
            };
            mail.To.Add(toEmail);
            if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
                mail.Attachments.Add(new Attachment(attachmentPath));
            await client.SendMailAsync(mail);
        }

        private static string BuildInvoiceEmailBody(InvoiceResponse invoice, Business? business)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Dear {invoice.CustomerName ?? "Customer"},");
            sb.AppendLine();
            sb.AppendLine("Please find below your invoice details:");
            sb.AppendLine();
            sb.AppendLine($"Invoice Number : {invoice.InvoiceNumber}");
            sb.AppendLine($"Invoice Date   : {invoice.InvoiceDate:dd-MM-yyyy}");
            sb.AppendLine($"Total Amount   : Rs.{invoice.TotalAmount:N2}");
            sb.AppendLine($"Amount Paid    : Rs.{invoice.PaidAmount:N2}");
            if (invoice.DueAmount > 0)
                sb.AppendLine($"Amount Due     : Rs.{invoice.DueAmount:N2}");
            sb.AppendLine();
            sb.AppendLine("Thank you for your business!");
            sb.AppendLine();
            sb.AppendLine("Regards,");
            sb.AppendLine(business?.Name ?? "Billing Software");
            if (!string.IsNullOrWhiteSpace(business?.Phone)) sb.AppendLine($"Phone: {business.Phone}");
            if (!string.IsNullOrWhiteSpace(business?.Email)) sb.AppendLine($"Email: {business.Email}");
            return sb.ToString();
        }
    }

    // ==================== WHATSAPP SERVICE ====================
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly WhatsAppSettings _settings;
        private readonly IInvoiceService _invoiceService;
        private readonly IHttpClientFactory _httpClientFactory;

        public WhatsAppService(ApplicationDbContext context, IOptions<WhatsAppSettings> settings,
            IInvoiceService invoiceService, IHttpClientFactory httpClientFactory)
        {
            _context = context; _settings = settings.Value;
            _invoiceService = invoiceService; _httpClientFactory = httpClientFactory;
        }

        public async Task<MessageSendResponse> SendInvoiceWhatsAppAsync(int invoiceId, string mobileNumber, string? message, int tenantId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, tenantId)
                ?? throw new KeyNotFoundException("Invoice not found");
            var business = await _context.Businesses.FindAsync(tenantId);

            var whatsappLog = new WhatsAppMessage
            {
                InvoiceId = invoiceId, MobileNumber = mobileNumber,
                MessageType = string.IsNullOrWhiteSpace(_settings.TemplateName) ? "text" : "template",
                MessageContent = message ?? $"Dear Customer, your invoice {invoice.InvoiceNumber} for Rs.{invoice.TotalAmount:N2} from {business?.Name ?? "our store"} is ready. Thank you!",
                TenantId = tenantId, Status = MessageStatus.Pending
            };
            _context.WhatsAppMessages.Add(whatsappLog);

            try
            {
                if (string.IsNullOrWhiteSpace(_settings.AccessToken))
                {
                    whatsappLog.Status = MessageStatus.Sent;
                    whatsappLog.SentDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return new MessageSendResponse { MessageId = whatsappLog.Id, Status = "Sent", Message = $"[Demo] WhatsApp logged to {mobileNumber}" };
                }

                string messageId = !string.IsNullOrWhiteSpace(_settings.TemplateName)
                    ? await SendTemplateAsync(mobileNumber, invoice, business)
                    : await SendTextAsync(mobileNumber, whatsappLog.MessageContent!);

                whatsappLog.Status = MessageStatus.Sent;
                whatsappLog.SentDate = DateTime.UtcNow;
                whatsappLog.WhatsAppMessageId = messageId;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = whatsappLog.Id, Status = "Sent", ProviderMessageId = messageId, Message = $"WhatsApp sent to {mobileNumber}" };
            }
            catch (Exception ex)
            {
                whatsappLog.Status = MessageStatus.Failed;
                whatsappLog.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = whatsappLog.Id, Status = "Failed", Message = ex.Message };
            }
        }

        public async Task<MessageSendResponse> SendPaymentReceiptWhatsAppAsync(int paymentId, string mobileNumber, int tenantId)
        {
            var payment = await _context.Payments.Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == paymentId) ?? throw new KeyNotFoundException("Payment not found");
            return await SendWhatsAppAsync(new SendWhatsAppRequest
            {
                MobileNumber = mobileNumber,
                Message = $"Payment of Rs.{payment.Amount:N2} received for invoice {payment.Invoice?.InvoiceNumber}. Thank you!"
            }, tenantId);
        }

        public async Task<MessageSendResponse> SendWhatsAppAsync(SendWhatsAppRequest request, int tenantId)
        {
            var log = new WhatsAppMessage
            {
                MobileNumber = request.MobileNumber, MessageContent = request.Message,
                MediaUrl = request.MediaUrl, MessageType = "text", TenantId = tenantId, Status = MessageStatus.Pending
            };
            _context.WhatsAppMessages.Add(log);
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.AccessToken))
                {
                    log.Status = MessageStatus.Sent;
                    log.SentDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return new MessageSendResponse { MessageId = log.Id, Status = "Sent", Message = $"[Demo] WhatsApp logged to {request.MobileNumber}" };
                }
                var messageId = await SendTextAsync(request.MobileNumber, request.Message ?? "");
                log.Status = MessageStatus.Sent;
                log.SentDate = DateTime.UtcNow;
                log.WhatsAppMessageId = messageId;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = log.Id, Status = "Sent", ProviderMessageId = messageId, Message = $"WhatsApp sent to {request.MobileNumber}" };
            }
            catch (Exception ex)
            {
                log.Status = MessageStatus.Failed;
                log.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = log.Id, Status = "Failed", Message = ex.Message };
            }
        }

        public async Task<List<CommunicationLogResponse>> GetMessageHistoryAsync(int tenantId) =>
            await _context.WhatsAppMessages.Where(w => w.TenantId == tenantId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new CommunicationLogResponse
                {
                    Id = w.Id, Channel = "WhatsApp", Recipient = w.MobileNumber,
                    Status = w.Status.ToString(), SentDate = w.SentDate, ErrorMessage = w.ErrorMessage, InvoiceId = w.InvoiceId
                }).ToListAsync();

        public async Task<bool> HandleWebhookAsync(string payload) => await Task.FromResult(true);

        private async Task<string> SendTemplateAsync(string mobileNumber, InvoiceResponse invoice, Business? business)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            var payload = new
            {
                messaging_product = "whatsapp",
                to = Normalize(mobileNumber),
                type = "template",
                template = new
                {
                    name = _settings.TemplateName,
                    language = new { code = "en" },
                    components = new[] { new { type = "body", parameters = new object[]
                    {
                        new { type = "text", text = business?.Name ?? "Shop" },
                        new { type = "text", text = invoice.InvoiceNumber },
                        new { type = "text", text = $"Rs.{invoice.TotalAmount:N2}" }
                    }}}
                }
            };
            var response = await client.PostAsJsonAsync($"{_settings.ApiBaseUrl}/{_settings.PhoneNumberId}/messages", payload);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"WhatsApp API {response.StatusCode}: {body}");
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString() ?? "";
        }

        private async Task<string> SendTextAsync(string mobileNumber, string text)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            var payload = new { messaging_product = "whatsapp", to = Normalize(mobileNumber), type = "text", text = new { body = text } };
            var response = await client.PostAsJsonAsync($"{_settings.ApiBaseUrl}/{_settings.PhoneNumberId}/messages", payload);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"WhatsApp API {response.StatusCode}: {body}");
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString() ?? "";
        }

        private static string Normalize(string number) => number.Replace("+", "").Replace(" ", "").Replace("-", "");
    }

    // ==================== SMS SERVICE ====================
    public class SmsService : ISmsService
    {
        private readonly ApplicationDbContext _context;
        private readonly SmsSettings _settings;
        private readonly IInvoiceService _invoiceService;
        private readonly IHttpClientFactory _httpClientFactory;

        public SmsService(ApplicationDbContext context, IOptions<SmsSettings> settings,
            IInvoiceService invoiceService, IHttpClientFactory httpClientFactory)
        {
            _context = context; _settings = settings.Value;
            _invoiceService = invoiceService; _httpClientFactory = httpClientFactory;
        }

        public async Task<MessageSendResponse> SendInvoiceSmsAsync(int invoiceId, string mobileNumber, string? message, int tenantId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, tenantId)
                ?? throw new KeyNotFoundException("Invoice not found");
            var business = await _context.Businesses.FindAsync(tenantId);
            var smsText = message
                ?? $"Dear Customer, invoice {invoice.InvoiceNumber} for Rs.{invoice.TotalAmount:N2} from {business?.Name ?? "our store"} has been generated. Thank you!";
            return await SendSmsAsync(new SendSmsRequest { MobileNumber = mobileNumber, Message = smsText }, tenantId);
        }

        public async Task<MessageSendResponse> SendSmsAsync(SendSmsRequest request, int tenantId)
        {
            var log = new SmsMessage
            {
                MobileNumber = request.MobileNumber, MessageContent = request.Message,
                TenantId = tenantId, Status = MessageStatus.Pending
            };
            _context.SmsMessages.Add(log);
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.AccountSid) || string.IsNullOrWhiteSpace(_settings.AuthToken))
                {
                    log.Status = MessageStatus.Sent;
                    log.SentDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return new MessageSendResponse { MessageId = log.Id, Status = "Sent", Message = $"[Demo] SMS logged to {request.MobileNumber}" };
                }
                var sid = await SendTwilioSmsAsync(request.MobileNumber, request.Message);
                log.ProviderMessageId = sid;
                log.Status = MessageStatus.Sent;
                log.SentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = log.Id, Status = "Sent", ProviderMessageId = sid, Message = $"SMS sent to {request.MobileNumber}" };
            }
            catch (Exception ex)
            {
                log.Status = MessageStatus.Failed;
                log.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return new MessageSendResponse { MessageId = log.Id, Status = "Failed", Message = ex.Message };
            }
        }

        public async Task<List<CommunicationLogResponse>> GetSmsHistoryAsync(int tenantId) =>
            await _context.SmsMessages.Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new CommunicationLogResponse
                {
                    Id = s.Id, Channel = "SMS", Recipient = s.MobileNumber,
                    Status = s.Status.ToString(), SentDate = s.SentDate, ErrorMessage = s.ErrorMessage, InvoiceId = s.InvoiceId
                }).ToListAsync();

        private async Task<string> SendTwilioSmsAsync(string toNumber, string body)
        {
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json";
            var client = _httpClientFactory.CreateClient();
            var creds = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To",   toNumber),
                new KeyValuePair<string, string>("From", _settings.FromNumber),
                new KeyValuePair<string, string>("Body", body)
            });
            var response = await client.PostAsync(url, form);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"Twilio SMS {response.StatusCode}: {responseBody}");
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("sid").GetString() ?? "";
        }
    }

    // ==================== NOTIFICATION SERVICE ====================
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        public NotificationService(ApplicationDbContext context) { _context = context; }

        public async Task<List<NotificationResponse>> GetNotificationsAsync(int tenantId, int userId) =>
            await _context.Notifications.Where(n => n.TenantId == tenantId && n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id, Title = n.Title, Message = n.Message,
                    Type = n.Type.ToString(), IsRead = n.IsRead, CreatedAt = n.CreatedAt, ActionUrl = n.ActionUrl
                }).ToListAsync();

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var n = await _context.Notifications.FindAsync(id) ?? throw new KeyNotFoundException("Notification not found");
            n.IsRead = true; n.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSettingsAsync(int tenantId, UpdateNotificationSettingsRequest request)
        {
            var settings = await _context.BusinessSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            if (settings != null)
            {
                if (request.EmailEnabled.HasValue)    settings.EnableEmailNotifications    = request.EmailEnabled.Value;
                if (request.WhatsAppEnabled.HasValue) settings.EnableWhatsAppNotifications = request.WhatsAppEnabled.Value;
                if (request.SmsEnabled.HasValue)      settings.EnableSmsNotifications      = request.SmsEnabled.Value;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<Notification> CreateNotificationAsync(int tenantId, int userId, string title, string message, string type)
        {
            var notification = new Notification
            {
                TenantId = tenantId, UserId = userId, Title = title,
                Message = message, Type = Enum.Parse<NotificationType>(type, true)
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }
    }

    // ==================== POST-BILLING NOTIFICATION SERVICE ====================
    /// <summary>
    /// Orchestrates Email / WhatsApp / SMS after a billing event.
    /// Feature gates:  Email = always allowed.
    ///                 WhatsApp = plan.HasWhatsAppIntegration.
    ///                 SMS      = plan.HasSmsIntegration.
    /// </summary>
    public class PostBillingNotificationService : IPostBillingNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ISmsService _smsService;

        public PostBillingNotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            ISmsService smsService)
        {
            _context = context;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _smsService = smsService;
        }

        public async Task<PostBillingNotificationResult> SendAsync(PostBillingSendRequest request, int tenantId)
        {
            var result = new PostBillingNotificationResult();

            // ── Email (always allowed) ─────────────────────────────────────────────
            if (request.SendEmail)
            {
                result.Email = new ChannelSendResult { Attempted = true, PlanAllowed = true };
                if (string.IsNullOrWhiteSpace(request.EmailAddress))
                {
                    result.Email.Status = "Skipped";
                    result.Email.Message = "No email address provided.";
                }
                else
                {
                    try
                    {
                        var r = await _emailService.SendInvoiceEmailAsync(
                            request.InvoiceId, request.EmailAddress,
                            request.EmailSubject, request.EmailMessage, tenantId);
                        result.Email.Status = r.Status;
                        result.Email.Message = r.Message;
                        result.Email.MessageId = r.MessageId;
                    }
                    catch (Exception ex) { result.Email.Status = "Failed"; result.Email.Message = ex.Message; }
                }
            }
            else
            {
                result.Email = new ChannelSendResult { Attempted = false, PlanAllowed = true, Status = "Skipped" };
            }

            // ── WhatsApp (plan gated) ─────────────────────────────────────────────
            var waAllowed = await IsWhatsAppEnabledAsync(tenantId);
            if (request.SendWhatsApp)
            {
                result.WhatsApp = new ChannelSendResult { Attempted = true, PlanAllowed = waAllowed };
                if (!waAllowed)
                {
                    result.WhatsApp.Status = "NotAllowed";
                    result.WhatsApp.Message = "WhatsApp messaging is not included in your current plan. Please upgrade.";
                }
                else if (string.IsNullOrWhiteSpace(request.WhatsAppNumber))
                {
                    result.WhatsApp.Status = "Skipped";
                    result.WhatsApp.Message = "No WhatsApp number provided.";
                }
                else
                {
                    try
                    {
                        var r = await _whatsAppService.SendInvoiceWhatsAppAsync(
                            request.InvoiceId, request.WhatsAppNumber, request.WhatsAppMessage, tenantId);
                        result.WhatsApp.Status = r.Status;
                        result.WhatsApp.Message = r.Message;
                        result.WhatsApp.MessageId = r.MessageId;
                    }
                    catch (Exception ex) { result.WhatsApp.Status = "Failed"; result.WhatsApp.Message = ex.Message; }
                }
            }
            else
            {
                result.WhatsApp = new ChannelSendResult { Attempted = false, PlanAllowed = waAllowed, Status = "Skipped" };
            }

            // ── SMS (plan gated) ───────────────────────────────────────────────────
            var smsAllowed = await IsSmsEnabledAsync(tenantId);
            if (request.SendSms)
            {
                result.Sms = new ChannelSendResult { Attempted = true, PlanAllowed = smsAllowed };
                if (!smsAllowed)
                {
                    result.Sms.Status = "NotAllowed";
                    result.Sms.Message = "SMS messaging is not included in your current plan. Please upgrade.";
                }
                else if (string.IsNullOrWhiteSpace(request.SmsNumber))
                {
                    result.Sms.Status = "Skipped";
                    result.Sms.Message = "No SMS number provided.";
                }
                else
                {
                    try
                    {
                        var r = await _smsService.SendInvoiceSmsAsync(
                            request.InvoiceId, request.SmsNumber, request.SmsMessage, tenantId);
                        result.Sms.Status = r.Status;
                        result.Sms.Message = r.Message;
                        result.Sms.MessageId = r.MessageId;
                    }
                    catch (Exception ex) { result.Sms.Status = "Failed"; result.Sms.Message = ex.Message; }
                }
            }
            else
            {
                result.Sms = new ChannelSendResult { Attempted = false, PlanAllowed = smsAllowed, Status = "Skipped" };
            }

            return result;
        }

        public async Task<bool> IsWhatsAppEnabledAsync(int tenantId)
        {
            var plan = await GetActivePlanAsync(tenantId);
            return plan?.HasWhatsAppIntegration ?? false;
        }

        public async Task<bool> IsSmsEnabledAsync(int tenantId)
        {
            var plan = await GetActivePlanAsync(tenantId);
            return plan?.HasSmsIntegration ?? false;
        }

        public Task<bool> IsEmailEnabledAsync(int tenantId) => Task.FromResult(true);

        private async Task<SubscriptionPlan?> GetActivePlanAsync(int tenantId) =>
            (await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == tenantId
                    && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                    && s.EndDate >= DateTime.UtcNow)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync())?.Plan;
    }
}
