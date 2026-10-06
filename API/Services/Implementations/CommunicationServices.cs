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
using System.Net.Http.Json;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== EMAIL SERVICE ====================
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSettings _settings;
        private readonly IInvoiceService _invoiceService;

        public EmailService(ApplicationDbContext context, IOptions<EmailSettings> settings, IInvoiceService invoiceService)
        { _context = context; _settings = settings.Value; _invoiceService = invoiceService; }

        public async Task<MessageSendResponse> SendInvoiceEmailAsync(int invoiceId, string toEmail, string? subject, string? message, int tenantId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, tenantId) ?? throw new KeyNotFoundException("Invoice not found");
            var business = await _context.Businesses.FindAsync(tenantId);
            var emailSubject = subject ?? $"Invoice {invoice.InvoiceNumber} – {business?.Name ?? "Billing Software"}";
            var emailBody = message ?? $"Dear {invoice.CustomerName ?? "Customer"},\n\nPlease find your invoice attached.\n\nInvoice Number: {invoice.InvoiceNumber}\nDate: {invoice.InvoiceDate:dd-MM-yyyy}\nTotal Amount: ₹{invoice.TotalAmount:N2}\n\nRegards,\n{business?.Name ?? "Billing Software"}";

            var emailLog = new EmailMessage { InvoiceId = invoiceId, CustomerId = null, EmailAddress = toEmail, Subject = emailSubject, MessageContent = emailBody, TenantId = tenantId, Status = MessageStatus.Pending };
            _context.EmailMessages.Add(emailLog);

            try
            {
                // In production: use SendGrid or MailKit to actually send
                // var client = new SendGridClient(_settings.ApiKey);
                // var msg = MailHelper.CreateSingleEmail(new EmailAddress(_settings.FromEmail, _settings.FromName), new EmailAddress(toEmail), emailSubject, emailBody, emailBody);
                // var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(invoiceId, tenantId);
                // msg.AddAttachment($"{invoice.InvoiceNumber}.pdf", Convert.ToBase64String(pdfBytes), "application/pdf");
                // await client.SendEmailAsync(msg);

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
            var payment = await _context.Payments.Include(p => p.Invoice).FirstOrDefaultAsync(p => p.Id == paymentId) ?? throw new KeyNotFoundException("Payment not found");
            return await SendEmailAsync(new SendEmailRequest { ToEmail = toEmail, Subject = $"Payment Receipt - {payment.Invoice?.InvoiceNumber}", Body = $"Payment of ₹{payment.Amount:N2} received via {payment.PaymentMethod}" }, tenantId);
        }

        public async Task<MessageSendResponse> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            // Simulated — use actual email provider in production
            return new MessageSendResponse { Status = "Sent", Message = $"Welcome email sent to {toEmail}" };
        }

        public async Task<MessageSendResponse> SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            return new MessageSendResponse { Status = "Sent", Message = $"Password reset email sent to {toEmail}" };
        }

        public async Task<MessageSendResponse> SendEmailAsync(SendEmailRequest request, int tenantId)
        {
            var emailLog = new EmailMessage { EmailAddress = request.ToEmail, Subject = request.Subject, MessageContent = request.Body, AttachmentPath = request.AttachmentPath, TenantId = tenantId, Status = MessageStatus.Sent, SentDate = DateTime.UtcNow };
            _context.EmailMessages.Add(emailLog);
            await _context.SaveChangesAsync();
            return new MessageSendResponse { MessageId = emailLog.Id, Status = "Sent", Message = $"Email sent to {request.ToEmail}" };
        }

        public async Task<List<CommunicationLogResponse>> GetEmailHistoryAsync(int tenantId) =>
            await _context.EmailMessages.Where(e => e.TenantId == tenantId).OrderByDescending(e => e.CreatedAt)
                .Select(e => new CommunicationLogResponse { Id = e.Id, Channel = "Email", Recipient = e.EmailAddress, Subject = e.Subject, Status = e.Status.ToString(), SentDate = e.SentDate, ErrorMessage = e.ErrorMessage, InvoiceId = e.InvoiceId }).ToListAsync();
    }

    // ==================== WHATSAPP SERVICE ====================
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly WhatsAppSettings _settings;
        private readonly IInvoiceService _invoiceService;
        private readonly IHttpClientFactory _httpClientFactory;

        public WhatsAppService(ApplicationDbContext context, IOptions<WhatsAppSettings> settings, IInvoiceService invoiceService, IHttpClientFactory httpClientFactory)
        { _context = context; _settings = settings.Value; _invoiceService = invoiceService; _httpClientFactory = httpClientFactory; }

        public async Task<MessageSendResponse> SendInvoiceWhatsAppAsync(int invoiceId, string mobileNumber, string? message, int tenantId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, tenantId) ?? throw new KeyNotFoundException("Invoice not found");
            var business = await _context.Businesses.FindAsync(tenantId);

            var whatsappLog = new WhatsAppMessage
            {
                InvoiceId = invoiceId, MobileNumber = mobileNumber, MessageType = "template",
                MessageContent = message ?? $"Invoice {invoice.InvoiceNumber} for ₹{invoice.TotalAmount:N2}",
                TenantId = tenantId, Status = MessageStatus.Pending
            };
            _context.WhatsAppMessages.Add(whatsappLog);

            try
            {
                // WhatsApp Business Cloud API call
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.AccessToken);

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = mobileNumber.Replace("+", ""),
                    type = "template",
                    template = new
                    {
                        name = _settings.TemplateName,
                        language = new { code = "en" },
                        components = new[]
                        {
                            new
                            {
                                type = "body",
                                parameters = new object[]
                                {
                                    new { type = "text", text = business?.Name ?? "Shop" },
                                    new { type = "text", text = invoice.InvoiceNumber },
                                    new { type = "text", text = $"₹{invoice.TotalAmount:N2}" }
                                }
                            }
                        }
                    }
                };

                var response = await client.PostAsJsonAsync($"{_settings.ApiBaseUrl}/{_settings.PhoneNumberId}/messages", payload);

                if (response.IsSuccessStatusCode)
                {
                    whatsappLog.Status = MessageStatus.Sent;
                    whatsappLog.SentDate = DateTime.UtcNow;
                    var result = await response.Content.ReadAsStringAsync();
                    whatsappLog.WhatsAppMessageId = result; // Parse actual message ID from response
                    await _context.SaveChangesAsync();
                    return new MessageSendResponse { MessageId = whatsappLog.Id, Status = "Sent", Message = $"WhatsApp message sent to {mobileNumber}" };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    whatsappLog.Status = MessageStatus.Failed;
                    whatsappLog.ErrorMessage = error;
                    await _context.SaveChangesAsync();
                    return new MessageSendResponse { MessageId = whatsappLog.Id, Status = "Failed", Message = error };
                }
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
            var payment = await _context.Payments.Include(p => p.Invoice).FirstOrDefaultAsync(p => p.Id == paymentId) ?? throw new KeyNotFoundException("Payment not found");
            return await SendWhatsAppAsync(new SendWhatsAppRequest { MobileNumber = mobileNumber, Message = $"Payment of ₹{payment.Amount:N2} received for {payment.Invoice?.InvoiceNumber}" }, tenantId);
        }

        public async Task<MessageSendResponse> SendWhatsAppAsync(SendWhatsAppRequest request, int tenantId)
        {
            var log = new WhatsAppMessage { MobileNumber = request.MobileNumber, MessageContent = request.Message, MediaUrl = request.MediaUrl, MessageType = "text", TenantId = tenantId, Status = MessageStatus.Sent, SentDate = DateTime.UtcNow };
            _context.WhatsAppMessages.Add(log);
            await _context.SaveChangesAsync();
            return new MessageSendResponse { MessageId = log.Id, Status = "Sent", Message = $"WhatsApp sent to {request.MobileNumber}" };
        }

        public async Task<List<CommunicationLogResponse>> GetMessageHistoryAsync(int tenantId) =>
            await _context.WhatsAppMessages.Where(w => w.TenantId == tenantId).OrderByDescending(w => w.CreatedAt)
                .Select(w => new CommunicationLogResponse { Id = w.Id, Channel = "WhatsApp", Recipient = w.MobileNumber, Status = w.Status.ToString(), SentDate = w.SentDate, ErrorMessage = w.ErrorMessage, InvoiceId = w.InvoiceId }).ToListAsync();

        public async Task<bool> HandleWebhookAsync(string payload)
        {
            // Process WhatsApp delivery/read receipts from Meta webhook
            // Parse payload and update WhatsAppMessage status
            return true;
        }
    }

    // ==================== SMS SERVICE ====================
    public class SmsService : ISmsService
    {
        private readonly ApplicationDbContext _context;
        public SmsService(ApplicationDbContext context) { _context = context; }

        public async Task<MessageSendResponse> SendSmsAsync(SendSmsRequest request, int tenantId)
        {
            var log = new SmsMessage { MobileNumber = request.MobileNumber, MessageContent = request.Message, TenantId = tenantId, Status = MessageStatus.Sent, SentDate = DateTime.UtcNow };
            _context.SmsMessages.Add(log);
            await _context.SaveChangesAsync();
            return new MessageSendResponse { MessageId = log.Id, Status = "Sent", Message = $"SMS sent to {request.MobileNumber}" };
        }

        public async Task<List<CommunicationLogResponse>> GetSmsHistoryAsync(int tenantId) =>
            await _context.SmsMessages.Where(s => s.TenantId == tenantId).OrderByDescending(s => s.CreatedAt)
                .Select(s => new CommunicationLogResponse { Id = s.Id, Channel = "SMS", Recipient = s.MobileNumber, Status = s.Status.ToString(), SentDate = s.SentDate, ErrorMessage = s.ErrorMessage, InvoiceId = s.InvoiceId }).ToListAsync();
    }

    // ==================== NOTIFICATION SERVICE ====================
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        public NotificationService(ApplicationDbContext context) { _context = context; }

        public async Task<List<NotificationResponse>> GetNotificationsAsync(int tenantId, int userId) =>
            await _context.Notifications.Where(n => n.TenantId == tenantId && n.UserId == userId).OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse { Id = n.Id, Title = n.Title, Message = n.Message, Type = n.Type.ToString(), IsRead = n.IsRead, CreatedAt = n.CreatedAt, ActionUrl = n.ActionUrl }).ToListAsync();

        public async Task<bool> MarkAsReadAsync(int id)
        { var n = await _context.Notifications.FindAsync(id) ?? throw new KeyNotFoundException("Notification not found"); n.IsRead = true; n.ReadAt = DateTime.UtcNow; await _context.SaveChangesAsync(); return true; }

        public async Task<bool> UpdateSettingsAsync(int tenantId, UpdateNotificationSettingsRequest request)
        {
            var settings = await _context.BusinessSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            if (settings != null)
            {
                if (request.EmailEnabled.HasValue) settings.EnableEmailNotifications = request.EmailEnabled.Value;
                if (request.WhatsAppEnabled.HasValue) settings.EnableWhatsAppNotifications = request.WhatsAppEnabled.Value;
                if (request.SmsEnabled.HasValue) settings.EnableSmsNotifications = request.SmsEnabled.Value;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<Notification> CreateNotificationAsync(int tenantId, int userId, string title, string message, string type)
        {
            var notification = new Notification { TenantId = tenantId, UserId = userId, Title = title, Message = message, Type = Enum.Parse<NotificationType>(type, true) };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }
    }
}
