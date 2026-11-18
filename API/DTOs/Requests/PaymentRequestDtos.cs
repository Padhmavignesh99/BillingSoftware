using MasterBilling.API.Models.Enums;

namespace MasterBilling.API.DTOs.Requests
{
    // ── Create a full payment for an invoice (supports split payment) ─────────────
    public class CreateFullPaymentRequest
    {
        public int?    InvoiceId    { get; set; }
        public int?    CustomerId   { get; set; }
        public int?    BranchId     { get; set; }
        public decimal TotalAmount  { get; set; }
        public string? Notes        { get; set; }
        public string? CashierName  { get; set; }

        /// <summary>One or more payment lines — split payment = multiple lines.</summary>
        public List<PaymentDetailRequest> Details { get; set; } = new();
    }

    public class PaymentDetailRequest
    {
        public PaymentMethod Method          { get; set; }
        public decimal       Amount          { get; set; }
        public decimal       CashReceived    { get; set; }   // cash only
        public string?       ReferenceNumber { get; set; }   // UPI ref, approval code…
        public string?       UPIApp          { get; set; }   // "GPay", "PhonePe"…
        public string?       CardLastFour    { get; set; }
        public string?       CardNetwork     { get; set; }   // Visa, Mastercard, RuPay
        public string?       BankName        { get; set; }
        public string?       WalletName      { get; set; }
        public string?       GiftCardCode    { get; set; }
        public string?       ChequeNumber    { get; set; }
        public DateTime?     ChequeDate      { get; set; }
        public string?       EMIBankName     { get; set; }
        public int?          EMIMonths       { get; set; }
        public decimal?      EMIInterestRate { get; set; }
    }

    // ── Initiate a physical POS terminal transaction ──────────────────────────────
    public class InitiateTerminalPaymentRequest
    {
        public int          PaymentId      { get; set; }
        public int          TerminalId     { get; set; }
        public decimal      Amount         { get; set; }
        public PaymentMethod Method        { get; set; }
        public string?      OrderReference { get; set; }
    }

    // ── Register a POS terminal ───────────────────────────────────────────────────
    public class RegisterTerminalRequest
    {
        public string  TerminalName    { get; set; } = string.Empty;
        public string  TerminalId      { get; set; } = string.Empty;
        public string? MerchantId      { get; set; }
        public int     ProviderId      { get; set; }
        public int?    BranchId        { get; set; }
        public string  ConnectionType  { get; set; } = "Cloud";
        public string? IpAddress       { get; set; }
        public int?    Port            { get; set; }
    }

    public class UpdateTerminalRequest
    {
        public string?         TerminalName   { get; set; }
        public string?         ConnectionType { get; set; }
        public string?         IpAddress      { get; set; }
        public int?            Port           { get; set; }
        public TerminalStatus? Status         { get; set; }
    }

    // ── Create a refund ───────────────────────────────────────────────────────────
    public class CreateRefundRequest
    {
        public int?          InvoiceId    { get; set; }
        public int?          PaymentId    { get; set; }
        public int?          CustomerId   { get; set; }
        public decimal       Amount       { get; set; }
        public string        Reason       { get; set; } = string.Empty;
        public PaymentMethod RefundMethod { get; set; }
        public string?       Notes        { get; set; }
    }

    // ── Collect due on old credit invoice ─────────────────────────────────────────
    public class CollectDueRequest
    {
        public int           InvoiceId       { get; set; }
        public int?          CustomerId      { get; set; }
        public decimal       Amount          { get; set; }
        public PaymentMethod PaymentMethod   { get; set; }
        public string?       ReferenceNumber { get; set; }
        public string?       Notes           { get; set; }
    }

    // ── Payment configuration per shop / branch ───────────────────────────────────
    public class UpdatePaymentConfigRequest
    {
        public List<PaymentMethodConfigItem> Methods { get; set; } = new();
    }

    public class PaymentMethodConfigItem
    {
        public PaymentMethod Method      { get; set; }
        public bool          IsEnabled   { get; set; }
        public int           SortOrder   { get; set; }
        public string?       DisplayName { get; set; }
    }

    // ── Webhook callback from payment provider (Pine Labs, Paytm, etc.) ───────────
    public class PaymentWebhookRequest
    {
        public string  Provider               { get; set; } = string.Empty;
        public string  Event                  { get; set; } = string.Empty;
        public string  TransactionRef         { get; set; } = string.Empty;
        public string? ProviderTransactionId  { get; set; }
        public string  Status                { get; set; } = string.Empty;
        public decimal? Amount               { get; set; }
        public string? RawPayload            { get; set; }
    }
}
