using MasterBilling.API.Data;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Models.Enums;
using MasterBilling.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBilling.API.Services.Implementations
{
    // ==================== FULL PAYMENT SERVICE ====================
    public class FullPaymentService : IFullPaymentService
    {
        private readonly ApplicationDbContext _db;
        public FullPaymentService(ApplicationDbContext db) { _db = db; }

        // ── Create payment with split-payment support ─────────────────────────────
        public async Task<PaymentResponse> CreatePaymentAsync(CreateFullPaymentRequest req, int tenantId)
        {
            var paidTotal   = req.Details.Sum(d => d.Amount);
            var cashLine    = req.Details.FirstOrDefault(d => d.Method == PaymentMethod.Cash);
            var changeTotal = cashLine != null ? Math.Max(0, cashLine.CashReceived - cashLine.Amount) : 0;
            var balance     = req.TotalAmount - paidTotal;

            var payment = new Payment
            {
                TenantId        = tenantId,
                InvoiceId       = req.InvoiceId,
                CustomerId      = req.CustomerId,
                BranchId        = req.BranchId,
                TotalAmount     = req.TotalAmount,
                PaidAmount      = paidTotal,
                ChangeAmount    = Math.Max(0, changeTotal),
                BalanceAmount   = Math.Max(0, balance),
                Status          = balance <= 0 ? PaymentStatus.Completed : PaymentStatus.Pending,
                Notes           = req.Notes,
                CashierName     = req.CashierName,
            };

            foreach (var d in req.Details)
            {
                var change = d.Method == PaymentMethod.Cash ? Math.Max(0, d.CashReceived - d.Amount) : 0;
                payment.Details.Add(new PaymentDetail
                {
                    TenantId        = tenantId,
                    Method          = d.Method,
                    Amount          = d.Amount,
                    CashReceived    = d.CashReceived,
                    ChangeGiven     = change,
                    ReferenceNumber = d.ReferenceNumber,
                    UPIApp          = d.UPIApp,
                    CardLastFour    = d.CardLastFour,
                    CardNetwork     = d.CardNetwork,
                    BankName        = d.BankName,
                    WalletName      = d.WalletName,
                    GiftCardCode    = d.GiftCardCode,
                    ChequeNumber    = d.ChequeNumber,
                    ChequeDate      = d.ChequeDate,
                    EMIBankName     = d.EMIBankName,
                    EMIMonths       = d.EMIMonths,
                    EMIInterestRate = d.EMIInterestRate,
                    Status          = PaymentStatus.Completed,
                });
            }

            _db.Payments.Add(payment);

            // Update invoice paid/due amounts
            if (req.InvoiceId.HasValue)
            {
                var inv = await _db.Invoices.FindAsync(req.InvoiceId.Value);
                if (inv != null && inv.TenantId == tenantId)
                {
                    inv.PaidAmount += paidTotal;
                    inv.DueAmount   = Math.Max(0, inv.TotalAmount - inv.PaidAmount);
                    inv.Status      = inv.DueAmount <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
                    if (req.Details.Count == 1)
                        inv.PaymentMethod = req.Details[0].Method;
                }
            }

            await _db.SaveChangesAsync();
            return (await MapPaymentAsync(payment.Id, tenantId))!;
        }

        public async Task<PaymentResponse?> GetPaymentAsync(int id, int tenantId)
            => await MapPaymentAsync(id, tenantId);

        public async Task<(List<PaymentResponse> Items, int Total)> GetPaymentsAsync(int tenantId, PaginationRequest p)
        {
            var q     = _db.Payments.Where(x => x.TenantId == tenantId && !x.IsDeleted);
            var total = await q.CountAsync();
            var ids   = await q.OrderByDescending(x => x.PaymentDate)
                               .Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
                               .Select(x => x.Id).ToListAsync();
            var items = new List<PaymentResponse>();
            foreach (var id in ids)
            {
                var r = await MapPaymentAsync(id, tenantId);
                if (r != null) items.Add(r);
            }
            return (items, total);
        }

        public async Task<List<PaymentResponse>> GetDueListAsync(int tenantId)
            => await _db.Invoices.Include(i => i.Customer)
                .Where(i => i.TenantId == tenantId && i.DueAmount > 0
                            && i.Status != InvoiceStatus.Cancelled && !i.IsDeleted)
                .Select(i => new PaymentResponse
                {
                    InvoiceId     = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    CustomerName  = i.Customer != null ? i.Customer.Name : "Walk-in",
                    TotalAmount   = i.TotalAmount,
                    PaidAmount    = i.PaidAmount,
                    BalanceAmount = i.DueAmount,
                    Status        = i.Status.ToString(),
                    PaymentDate   = i.DueDate ?? i.InvoiceDate,
                    Amount        = i.DueAmount,
                }).ToListAsync();

        public async Task<PaymentResponse> CollectDueAsync(CollectDueRequest req, int tenantId)
            => await CreatePaymentAsync(new CreateFullPaymentRequest
            {
                InvoiceId   = req.InvoiceId,
                CustomerId  = req.CustomerId,
                TotalAmount = req.Amount,
                Notes       = req.Notes,
                Details     =
                [
                    new() { Method = req.PaymentMethod, Amount = req.Amount, ReferenceNumber = req.ReferenceNumber }
                ]
            }, tenantId);

        // ── POS Terminals ─────────────────────────────────────────────────────────
        public async Task<List<PaymentTerminalResponse>> GetTerminalsAsync(int tenantId)
            => await _db.PaymentTerminals.Include(t => t.Provider)
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .Select(t => new PaymentTerminalResponse
                {
                    Id              = t.Id,
                    TerminalName    = t.TerminalName,
                    TerminalId      = t.TerminalId,
                    MerchantId      = t.MerchantId,
                    ProviderId      = t.ProviderId,
                    ProviderName    = t.Provider != null ? t.Provider.ProviderName : null,
                    BranchId        = t.BranchId,
                    ConnectionType  = t.ConnectionType,
                    IpAddress       = t.IpAddress,
                    Port            = t.Port,
                    Status          = t.Status.ToString(),
                    LastConnectedAt = t.LastConnectedAt,
                }).ToListAsync();

        public async Task<PaymentTerminalResponse> RegisterTerminalAsync(RegisterTerminalRequest req, int tenantId)
        {
            var t = new PaymentTerminal
            {
                TenantId       = tenantId,
                TerminalName   = req.TerminalName,
                TerminalId     = req.TerminalId,
                MerchantId     = req.MerchantId,
                ProviderId     = req.ProviderId,
                BranchId       = req.BranchId,
                ConnectionType = req.ConnectionType,
                IpAddress      = req.IpAddress,
                Port           = req.Port,
                Status         = TerminalStatus.Active,
            };
            _db.PaymentTerminals.Add(t);
            await _db.SaveChangesAsync();
            return (await GetTerminalsAsync(tenantId)).First(x => x.Id == t.Id);
        }

        public async Task<PaymentTerminalResponse> UpdateTerminalAsync(int id, UpdateTerminalRequest req, int tenantId)
        {
            var t = await _db.PaymentTerminals.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Terminal not found");
            if (req.TerminalName   != null) t.TerminalName   = req.TerminalName;
            if (req.ConnectionType != null) t.ConnectionType = req.ConnectionType;
            if (req.IpAddress      != null) t.IpAddress      = req.IpAddress;
            if (req.Port.HasValue)          t.Port           = req.Port;
            if (req.Status.HasValue)        t.Status         = req.Status.Value;
            await _db.SaveChangesAsync();
            return (await GetTerminalsAsync(tenantId)).First(x => x.Id == id);
        }

        public async Task<TerminalConnectionTestResponse> TestTerminalConnectionAsync(int id, int tenantId)
        {
            var t = await _db.PaymentTerminals.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (t == null)
                return new TerminalConnectionTestResponse { IsConnected = false, Message = "Terminal not found", TestedAt = DateTime.UtcNow };

            // In production: call provider SDK/API here (Pine Labs, Paytm, etc.)
            t.LastConnectedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return new TerminalConnectionTestResponse { IsConnected = true, Message = "Connection successful (simulated — integrate provider SDK for live check)", TestedAt = DateTime.UtcNow };
        }

        // ── Terminal transactions ─────────────────────────────────────────────────
        public async Task<PaymentTransactionResponse> InitiateTerminalTransactionAsync(InitiateTerminalPaymentRequest req, int tenantId)
        {
            var txn = new PaymentTransaction
            {
                TenantId       = tenantId,
                PaymentId      = req.PaymentId,
                TerminalId     = req.TerminalId,
                Amount         = req.Amount,
                Method         = req.Method,
                TransactionRef = $"TXN-{tenantId}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Status         = TransactionStatus.Initiated,
                InitiatedAt    = DateTime.UtcNow,
            };
            _db.PaymentTransactions.Add(txn);
            await _db.SaveChangesAsync();
            return MapTransaction(txn, null);
        }

        public async Task<PaymentTransactionResponse?> GetTransactionStatusAsync(int id, int tenantId)
        {
            var txn = await _db.PaymentTransactions.Include(t => t.Terminal)
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            return txn == null ? null : MapTransaction(txn, txn.Terminal?.TerminalName);
        }

        public async Task<PaymentTransactionResponse> CancelTransactionAsync(int id, int tenantId)
        {
            var txn = await _db.PaymentTransactions.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Transaction not found");
            txn.Status      = TransactionStatus.Cancelled;
            txn.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapTransaction(txn, null);
        }

        // ── Webhook processing ────────────────────────────────────────────────────
        public async Task ProcessWebhookAsync(string provider, PaymentWebhookRequest req)
        {
            // Find by TransactionRef across all tenants (webhook has no JWT)
            var txn = await _db.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TransactionRef == req.TransactionRef);
            if (txn == null) return;

            txn.ProviderTransactionId = req.ProviderTransactionId;
            txn.StatusMessage         = req.Status;
            txn.RawResponse           = req.RawPayload;
            txn.Status                = req.Status?.ToLower() switch
            {
                "success" or "paid" or "captured" or "approved" => TransactionStatus.Success,
                "failed"  or "failure" or "declined"             => TransactionStatus.Failed,
                "cancelled" or "canceled"                        => TransactionStatus.Cancelled,
                "timeout" or "timedout"                          => TransactionStatus.TimedOut,
                _ => txn.Status
            };
            txn.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ── Refunds ───────────────────────────────────────────────────────────────
        public async Task<RefundResponse> CreateRefundAsync(CreateRefundRequest req, int tenantId)
        {
            var refund = new Refund
            {
                TenantId     = tenantId,
                InvoiceId    = req.InvoiceId,
                PaymentId    = req.PaymentId,
                CustomerId   = req.CustomerId,
                Amount       = req.Amount,
                Reason       = req.Reason,
                RefundMethod = req.RefundMethod,
                Notes        = req.Notes,
                Status       = RefundStatus.Pending,
            };
            _db.Refunds.Add(refund);
            await _db.SaveChangesAsync();
            return await MapRefundAsync(refund.Id, tenantId);
        }

        public async Task<List<RefundResponse>> GetRefundsAsync(int? paymentId, int tenantId)
        {
            var q = _db.Refunds.Include(r => r.Invoice).Include(r => r.Customer)
                .Where(r => r.TenantId == tenantId && !r.IsDeleted);
            if (paymentId.HasValue) q = q.Where(r => r.PaymentId == paymentId);
            return await q.Select(r => new RefundResponse
            {
                Id              = r.Id,
                InvoiceId       = r.InvoiceId,
                InvoiceNumber   = r.Invoice != null ? r.Invoice.InvoiceNumber : null,
                PaymentId       = r.PaymentId,
                CustomerId      = r.CustomerId,
                CustomerName    = r.Customer != null ? r.Customer.Name : null,
                Amount          = r.Amount,
                Reason          = r.Reason,
                Status          = r.Status.ToString(),
                RefundMethod    = r.RefundMethod.ToString(),
                RefundDate      = r.RefundDate,
                ReferenceNumber = r.ReferenceNumber,
            }).ToListAsync();
        }

        // ── Payment configuration ─────────────────────────────────────────────────
        public async Task<List<PaymentMethodConfigResponse>> GetPaymentConfigAsync(int tenantId, int? branchId)
        {
            var configs = await _db.PaymentConfigurations
                .Where(c => c.TenantId == tenantId
                            && (!branchId.HasValue || c.BranchId == branchId)
                            && !c.IsDeleted)
                .ToListAsync();

            if (!configs.Any())
            {
                // Return all methods as enabled by default when not configured
                return Enum.GetValues<PaymentMethod>()
                    .Select((m, i) => new PaymentMethodConfigResponse
                    {
                        Method      = m.ToString(),
                        MethodCode  = ((int)m).ToString(),
                        IsEnabled   = true,
                        SortOrder   = i,
                        DisplayName = m switch
                        {
                            PaymentMethod.Cash             => "Cash",
                            PaymentMethod.UPI              => "UPI (GPay / PhonePe / Paytm)",
                            PaymentMethod.DebitCard        => "Debit Card",
                            PaymentMethod.CreditCard       => "Credit Card",
                            PaymentMethod.NetBanking       => "Net Banking",
                            PaymentMethod.Wallet           => "Wallet",
                            PaymentMethod.GiftCard         => "Gift Card",
                            PaymentMethod.StoreCredit      => "Store Credit",
                            PaymentMethod.CustomerCredit   => "Pay Later / Khata",
                            PaymentMethod.BankTransfer     => "Bank Transfer (NEFT/IMPS/RTGS)",
                            PaymentMethod.Cheque           => "Cheque",
                            PaymentMethod.EMI              => "EMI",
                            PaymentMethod.PaymentOnDelivery => "Payment on Delivery",
                            PaymentMethod.Online           => "Online / QR",
                            PaymentMethod.Card             => "Card",
                            _ => m.ToString(),
                        }
                    }).ToList();
            }

            return configs.OrderBy(c => c.SortOrder)
                .Select(c => new PaymentMethodConfigResponse
                {
                    Method      = c.Method.ToString(),
                    MethodCode  = ((int)c.Method).ToString(),
                    IsEnabled   = c.IsEnabled,
                    SortOrder   = c.SortOrder,
                    DisplayName = c.DisplayName ?? c.Method.ToString(),
                }).ToList();
        }

        public async Task UpdatePaymentConfigAsync(UpdatePaymentConfigRequest req, int tenantId, int? branchId)
        {
            var existing = await _db.PaymentConfigurations
                .Where(c => c.TenantId == tenantId && (!branchId.HasValue || c.BranchId == branchId))
                .ToListAsync();
            _db.PaymentConfigurations.RemoveRange(existing);
            foreach (var m in req.Methods)
            {
                _db.PaymentConfigurations.Add(new PaymentConfiguration
                {
                    TenantId    = tenantId,
                    BranchId    = branchId,
                    Method      = m.Method,
                    IsEnabled   = m.IsEnabled,
                    SortOrder   = m.SortOrder,
                    DisplayName = m.DisplayName,
                });
            }
            await _db.SaveChangesAsync();
        }

        // ── Daily summary ─────────────────────────────────────────────────────────
        public async Task<PaymentSummaryResponse> GetDailySummaryAsync(int tenantId, DateTime? date, int? branchId)
        {
            var d = (date ?? DateTime.UtcNow).Date;
            var q = _db.Payments.Include(p => p.Details)
                .Where(p => p.TenantId == tenantId && p.PaymentDate.Date == d && !p.IsDeleted);
            if (branchId.HasValue) q = q.Where(p => p.BranchId == branchId);
            var payments = await q.ToListAsync();
            var details  = payments.SelectMany(p => p.Details).ToList();

            return new PaymentSummaryResponse
            {
                TotalAmount       = payments.Sum(p => p.PaidAmount),
                CashTotal         = details.Where(x => x.Method == PaymentMethod.Cash).Sum(x => x.Amount),
                UPITotal          = details.Where(x => x.Method == PaymentMethod.UPI).Sum(x => x.Amount),
                CardTotal         = details.Where(x => x.Method is PaymentMethod.DebitCard
                                                             or PaymentMethod.CreditCard
                                                             or PaymentMethod.Card
                                                             or PaymentMethod.EMI).Sum(x => x.Amount),
                OtherTotal        = details.Where(x => x.Method is not PaymentMethod.Cash
                                                             and not PaymentMethod.UPI
                                                             and not PaymentMethod.DebitCard
                                                             and not PaymentMethod.CreditCard
                                                             and not PaymentMethod.Card
                                                             and not PaymentMethod.EMI).Sum(x => x.Amount),
                TotalTransactions = payments.Count,
                Date              = d,
            };
        }

        // ── Providers ─────────────────────────────────────────────────────────────
        public async Task<List<PaymentProviderResponse>> GetProvidersAsync(int tenantId)
            => await _db.PaymentProviders
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .Select(p => new PaymentProviderResponse
                {
                    Id               = p.Id,
                    ProviderName     = p.ProviderName,
                    ProviderCode     = p.ProviderCode,
                    ApiEndpoint      = p.ApiEndpoint,
                    IsActive         = p.IsActive,
                    SupportedMethods = p.SupportedMethods,
                }).ToListAsync();

        // ── Helpers ───────────────────────────────────────────────────────────────
        private async Task<PaymentResponse?> MapPaymentAsync(int id, int tenantId)
        {
            var p = await _db.Payments
                .Include(x => x.Details)
                .Include(x => x.Invoice)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (p == null) return null;

            var r = new PaymentResponse
            {
                Id            = p.Id,
                InvoiceId     = p.InvoiceId,
                InvoiceNumber = p.Invoice?.InvoiceNumber,
                CustomerId    = p.CustomerId,
                CustomerName  = p.Customer?.Name,
                TotalAmount   = p.TotalAmount,
                PaidAmount    = p.PaidAmount,
                ChangeAmount  = p.ChangeAmount,
                BalanceAmount = p.BalanceAmount,
                Status        = p.Status.ToString(),
                PaymentDate   = p.PaymentDate,
                Notes         = p.Notes,
                CashierName   = p.CashierName,
                Amount        = p.PaidAmount,
                Details       = p.Details.Select(d => new PaymentDetailResponse
                {
                    Id              = d.Id,
                    Method          = d.Method.ToString(),
                    Amount          = d.Amount,
                    CashReceived    = d.CashReceived,
                    ChangeGiven     = d.ChangeGiven,
                    ReferenceNumber = d.ReferenceNumber,
                    TransactionId   = d.TransactionId,
                    UPIApp          = d.UPIApp,
                    CardLastFour    = d.CardLastFour,
                    CardNetwork     = d.CardNetwork,
                    Status          = d.Status.ToString(),
                }).ToList(),
            };

            // Single-method convenience fields (legacy)
            if (p.Details.Count == 1)
            {
                var first         = p.Details.First();
                r.PaymentMethod   = first.Method.ToString();
                r.ReferenceNumber = first.ReferenceNumber;
                r.TransactionId   = first.TransactionId;
            }
            else if (p.Details.Count > 1)
            {
                r.PaymentMethod = "Split";
            }
            return r;
        }

        private static PaymentTransactionResponse MapTransaction(PaymentTransaction t, string? terminalName)
            => new()
            {
                Id                    = t.Id,
                PaymentId             = t.PaymentId,
                TerminalId            = t.TerminalId,
                TerminalName          = terminalName,
                TransactionRef        = t.TransactionRef,
                ProviderTransactionId = t.ProviderTransactionId,
                Amount                = t.Amount,
                Method                = t.Method.ToString(),
                Status                = t.Status.ToString(),
                StatusMessage         = t.StatusMessage,
                InitiatedAt           = t.InitiatedAt,
                CompletedAt           = t.CompletedAt,
            };

        private async Task<RefundResponse> MapRefundAsync(int id, int tenantId)
        {
            var r = await _db.Refunds
                .Include(x => x.Invoice)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Refund not found");
            return new RefundResponse
            {
                Id              = r.Id,
                InvoiceId       = r.InvoiceId,
                InvoiceNumber   = r.Invoice?.InvoiceNumber,
                PaymentId       = r.PaymentId,
                CustomerId      = r.CustomerId,
                CustomerName    = r.Customer?.Name,
                Amount          = r.Amount,
                Reason          = r.Reason,
                Status          = r.Status.ToString(),
                RefundMethod    = r.RefundMethod.ToString(),
                RefundDate      = r.RefundDate,
                ReferenceNumber = r.ReferenceNumber,
            };
        }
    }
}
