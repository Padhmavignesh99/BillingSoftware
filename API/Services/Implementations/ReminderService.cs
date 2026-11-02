using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class ReminderService : IReminderService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWhatsAppService _whatsapp;
        private readonly IEmailService _email;
        private readonly ISmsService _sms;

        public ReminderService(ApplicationDbContext db, IWhatsAppService whatsapp, IEmailService email, ISmsService sms)
        {
            _db = db; _whatsapp = whatsapp; _email = email; _sms = sms;
        }

        public async Task<List<ReminderRuleResponse>> GetRulesAsync(int tenantId)
        {
            return await _db.ReminderRules
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .OrderBy(r => r.SortOrder)
                .Select(r => MapRule(r)).ToListAsync();
        }

        public async Task<ReminderRuleResponse> CreateRuleAsync(CreateReminderRuleRequest r, int tenantId)
        {
            var rule = new ReminderRule
            {
                TenantId = tenantId, Name = r.Name, DaysOverdue = r.DaysOverdue,
                SendWhatsApp = r.SendWhatsApp, SendEmail = r.SendEmail, SendSms = r.SendSms,
                MessageTemplate = r.MessageTemplate, IsEnabled = r.IsEnabled, SortOrder = r.SortOrder,
                CreatedAt = DateTime.UtcNow, IsActive = true
            };
            _db.ReminderRules.Add(rule);
            await _db.SaveChangesAsync();
            return MapRule(rule);
        }

        public async Task<ReminderRuleResponse> UpdateRuleAsync(int id, CreateReminderRuleRequest r, int tenantId)
        {
            var rule = await _db.ReminderRules.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Rule not found");
            rule.Name = r.Name; rule.DaysOverdue = r.DaysOverdue;
            rule.SendWhatsApp = r.SendWhatsApp; rule.SendEmail = r.SendEmail; rule.SendSms = r.SendSms;
            rule.MessageTemplate = r.MessageTemplate; rule.IsEnabled = r.IsEnabled;
            rule.SortOrder = r.SortOrder; rule.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapRule(rule);
        }

        public async Task<bool> DeleteRuleAsync(int id, int tenantId)
        {
            var rule = await _db.ReminderRules.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (rule == null) return false;
            rule.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<OverdueCustomerResponse>> GetOverdueCustomersAsync(int tenantId)
        {
            var today = DateTime.UtcNow.Date;
            var overdueInvoices = await _db.Invoices
                .Include(i => i.Customer)
                .Where(i => i.TenantId == tenantId && !i.IsDeleted
                    && i.DueDate.HasValue && i.DueDate < today
                    && i.DueAmount > 0)
                .ToListAsync();

            return overdueInvoices
                .GroupBy(i => i.CustomerId)
                .Select(g => new OverdueCustomerResponse
                {
                    CustomerId = g.Key ?? 0,
                    CustomerName = g.First().Customer?.Name ?? "Unknown",
                    Phone = g.First().Customer?.Phone,
                    Email = g.First().Customer?.Email,
                    TotalOverdue = g.Sum(i => i.DueAmount),
                    OverdueInvoiceCount = g.Count(),
                    MaxDaysOverdue = g.Max(i => (today - i.DueDate!.Value.Date).Days),
                    Invoices = g.Select(i => new OverdueInvoiceItem
                    {
                        InvoiceId = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        DueAmount = i.DueAmount,
                        DueDate = i.DueDate!.Value,
                        DaysOverdue = (today - i.DueDate.Value.Date).Days
                    }).ToList()
                })
                .OrderByDescending(c => c.TotalOverdue)
                .ToList();
        }

        public async Task<List<ReminderLogResponse>> GetLogsAsync(int tenantId, PaginationRequest p)
        {
            var page = p.Page < 1 ? 1 : p.Page;
            var pageSize = p.PageSize < 1 ? 20 : p.PageSize;
            return await _db.ReminderLogs
                .Include(l => l.Customer)
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.SentAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(l => new ReminderLogResponse
                {
                    Id = l.Id, CustomerId = l.CustomerId,
                    CustomerName = l.Customer != null ? l.Customer.Name : null,
                    InvoiceId = l.InvoiceId, Channel = l.Channel,
                    Status = l.Status, Message = l.Message,
                    ErrorMessage = l.ErrorMessage, SentAt = l.SentAt
                }).ToListAsync();
        }

        public async Task SendManualReminderAsync(SendManualReminderRequest r, int tenantId)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == r.CustomerId && c.TenantId == tenantId);
            if (customer == null) return;

            var msg = r.CustomMessage ?? $"Dear {customer.Name}, please clear your outstanding dues at the earliest.";

            if (r.SendWhatsApp && !string.IsNullOrEmpty(customer.Phone))
            {
                var status = "Sent";
                try
                {
                    await _whatsapp.SendWhatsAppAsync(new SendWhatsAppRequest { MobileNumber = customer.Phone, Message = msg }, tenantId);
                }
                catch { status = "Failed"; }
                _db.ReminderLogs.Add(new ReminderLog
                {
                    TenantId = tenantId, CustomerId = r.CustomerId, InvoiceId = r.InvoiceId,
                    Channel = "WhatsApp", Status = status, Message = msg,
                    SentAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, IsActive = true
                });
            }
            if (r.SendEmail && !string.IsNullOrEmpty(customer.Email))
            {
                var status = "Sent";
                try
                {
                    await _email.SendEmailAsync(new SendEmailRequest { ToEmail = customer.Email, Subject = "Payment Reminder", Body = msg }, tenantId);
                }
                catch { status = "Failed"; }
                _db.ReminderLogs.Add(new ReminderLog
                {
                    TenantId = tenantId, CustomerId = r.CustomerId, InvoiceId = r.InvoiceId,
                    Channel = "Email", Status = status, Message = msg,
                    SentAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, IsActive = true
                });
            }
            await _db.SaveChangesAsync();
        }

        private static ReminderRuleResponse MapRule(ReminderRule r) => new()
        {
            Id = r.Id, Name = r.Name, DaysOverdue = r.DaysOverdue,
            SendWhatsApp = r.SendWhatsApp, SendEmail = r.SendEmail, SendSms = r.SendSms,
            MessageTemplate = r.MessageTemplate, IsEnabled = r.IsEnabled, SortOrder = r.SortOrder
        };
    }
}

