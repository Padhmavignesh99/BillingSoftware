using MasterBilling.API.Models.Enums;

namespace MasterBilling.API.Models.Entities
{
    // ── Core payment record for a bill (supports split payment) ──────────────────
    public class Payment : TenantEntity
    {
        public int?      InvoiceId          { get; set; }
        public Invoice?  Invoice            { get; set; }
        public int?      CustomerId         { get; set; }
        public Customer? Customer           { get; set; }
        public int?      BranchId           { get; set; }
        public Branch?   Branch             { get; set; }

        public decimal TotalAmount          { get; set; }
        public decimal PaidAmount           { get; set; }
        public decimal ChangeAmount         { get; set; }   // cash change returned
        public decimal BalanceAmount        { get; set; }   // remaining due after partial pay

        public PaymentStatus Status         { get; set; } = PaymentStatus.Pending;
        public DateTime PaymentDate         { get; set; } = DateTime.UtcNow;
        public string?  Notes              { get; set; }
        public string?  CashierName        { get; set; }
        public int?     ProcessedByUserId  { get; set; }

        // Split payment lines — one row per method used
        public ICollection<PaymentDetail>      Details      { get; set; } = new List<PaymentDetail>();
        // Terminal transactions (card / UPI machine)
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }

    // ── One line per payment method in a split payment ────────────────────────────
    public class PaymentDetail : TenantEntity
    {
        public int      PaymentId       { get; set; }
        public Payment? Payment         { get; set; }

        public PaymentMethod Method     { get; set; }
        public decimal Amount           { get; set; }
        public decimal CashReceived     { get; set; }   // for cash payments
        public decimal ChangeGiven      { get; set; }   // for cash payments

        public string?   ReferenceNumber { get; set; }  // UPI ref, cheque no, etc.
        public string?   TransactionId   { get; set; }  // gateway/terminal txn id
        public string?   UPIApp          { get; set; }  // "GPay", "PhonePe", etc.
        public string?   CardLastFour    { get; set; }  // last 4 digits (never full card)
        public string?   CardNetwork     { get; set; }  // Visa, Mastercard, RuPay
        public string?   BankName        { get; set; }
        public string?   WalletName      { get; set; }
        public string?   GiftCardCode    { get; set; }
        public string?   ChequeNumber    { get; set; }
        public DateTime? ChequeDate      { get; set; }
        public string?   EMIBankName     { get; set; }
        public int?      EMIMonths       { get; set; }
        public decimal?  EMIInterestRate { get; set; }

        public PaymentStatus Status     { get; set; } = PaymentStatus.Completed;
        public string?   FailureReason  { get; set; }
    }

    // ── POS terminal record (Pine Labs, Paytm EDC, etc.) ─────────────────────────
    public class PaymentTerminal : TenantEntity
    {
        public string  TerminalName    { get; set; } = string.Empty;
        public string  TerminalId      { get; set; } = string.Empty;  // provider TID
        public string? MerchantId      { get; set; }
        public int     ProviderId      { get; set; }
        public PaymentProvider? Provider { get; set; }
        public int?    BranchId        { get; set; }
        public Branch? Branch          { get; set; }
        public string  ConnectionType  { get; set; } = "Cloud";       // USB|LAN|Cloud|Bluetooth
        public string? IpAddress       { get; set; }
        public int?    Port            { get; set; }
        public TerminalStatus Status   { get; set; } = TerminalStatus.Active;
        public DateTime? LastConnectedAt { get; set; }
        public string? Notes           { get; set; }
    }

    // ── Payment provider master (Pine Labs, Paytm, Razorpay…) ───────────────────
    public class PaymentProvider : TenantEntity
    {
        public string  ProviderName      { get; set; } = string.Empty;  // "Pine Labs"
        public string  ProviderCode      { get; set; } = string.Empty;  // "PINELABS"
        public string? WebhookSecret     { get; set; }                   // stored securely
        public string? ApiEndpoint       { get; set; }
        public new bool IsActive         { get; set; } = true;
        public string? SupportedMethods  { get; set; }                   // JSON array
        public ICollection<PaymentTerminal> Terminals { get; set; } = new List<PaymentTerminal>();
    }

    // ── Individual terminal transaction ───────────────────────────────────────────
    public class PaymentTransaction : TenantEntity
    {
        public int              PaymentId              { get; set; }
        public Payment?         Payment                { get; set; }
        public int?             TerminalId             { get; set; }
        public PaymentTerminal? Terminal               { get; set; }
        public int?             ProviderId             { get; set; }
        public PaymentProvider? Provider               { get; set; }

        public string   TransactionRef         { get; set; } = string.Empty;  // our unique ref
        public string?  ProviderTransactionId  { get; set; }                  // RRN / approval code
        public string?  ProviderOrderId        { get; set; }
        public decimal  Amount                 { get; set; }
        public PaymentMethod    Method         { get; set; }
        public TransactionStatus Status        { get; set; } = TransactionStatus.Initiated;
        public string?  StatusMessage          { get; set; }
        public string?  RawResponse            { get; set; }   // provider JSON (no card data)
        public DateTime? InitiatedAt           { get; set; }
        public DateTime? CompletedAt           { get; set; }
        public int      RetryCount             { get; set; } = 0;
    }

    // ── Shop-wise payment method configuration ────────────────────────────────────
    public class PaymentConfiguration : TenantEntity
    {
        public PaymentMethod Method    { get; set; }
        public bool   IsEnabled        { get; set; } = true;
        public int    SortOrder        { get; set; } = 0;
        public string? DisplayName     { get; set; }
        public int?   BranchId        { get; set; }
        public Branch? Branch         { get; set; }
    }

    // ── Refund record ─────────────────────────────────────────────────────────────
    public class Refund : TenantEntity
    {
        public int?      InvoiceId     { get; set; }
        public Invoice?  Invoice       { get; set; }
        public int?      PaymentId     { get; set; }
        public Payment?  Payment       { get; set; }
        public int?      CustomerId    { get; set; }
        public Customer? Customer      { get; set; }
        public decimal   Amount        { get; set; }
        public string    Reason        { get; set; } = string.Empty;
        public RefundStatus  Status    { get; set; } = RefundStatus.Pending;
        public PaymentMethod RefundMethod { get; set; }
        public DateTime  RefundDate    { get; set; } = DateTime.UtcNow;
        public string?   ReferenceNumber { get; set; }
        public string?   Notes         { get; set; }
        public int?      ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt    { get; set; }
    }

    // ── Cash register session ─────────────────────────────────────────────────────
    public class CashRegister : TenantEntity
    {
        public string  RegisterName       { get; set; } = string.Empty;
        public int?    BranchId           { get; set; }
        public Branch? Branch             { get; set; }
        public int     OpenedByUserId     { get; set; }
        public User?   OpenedByUser       { get; set; }
        public int?    ClosedByUserId     { get; set; }
        public User?   ClosedByUser       { get; set; }
        public decimal OpeningBalance     { get; set; }
        public decimal ClosingBalance     { get; set; }
        public decimal TotalSales         { get; set; }
        public decimal TotalCash          { get; set; }
        public decimal TotalUPI           { get; set; }
        public decimal TotalCard          { get; set; }
        public decimal TotalOther         { get; set; }
        public decimal TotalRefunds       { get; set; }
        public decimal TotalCashIn        { get; set; }
        public decimal TotalCashOut       { get; set; }
        public DateTime  OpenedAt         { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt         { get; set; }
        public CashRegisterStatus Status  { get; set; } = CashRegisterStatus.Open;
        public string? Notes              { get; set; }
    }

    // ── Payment gateway transaction (online / QR) ─────────────────────────────────
    public class PaymentGatewayTransaction : TenantEntity
    {
        public string  GatewayName           { get; set; } = string.Empty;
        public string? GatewayTransactionId  { get; set; }
        public string? OrderId               { get; set; }
        public int?    InvoiceId             { get; set; }
        public Invoice? Invoice              { get; set; }
        public decimal Amount                { get; set; }
        public string? Currency              { get; set; } = "INR";
        public string  Status               { get; set; } = "Pending";
        public string? PaymentLink           { get; set; }
        public string? QRCodeData            { get; set; }
        public string? CallbackData          { get; set; }
        public DateTime? CompletedAt         { get; set; }
    }

    // ── Daily settlement report ───────────────────────────────────────────────────
    public class PaymentSettlement : TenantEntity
    {
        public int?             TerminalId          { get; set; }
        public PaymentTerminal? Terminal            { get; set; }
        public int?             BranchId            { get; set; }
        public Branch?          Branch              { get; set; }
        public DateTime         SettlementDate      { get; set; } = DateTime.UtcNow;
        public decimal          TotalAmount         { get; set; }
        public int              TransactionCount    { get; set; }
        public string           Status             { get; set; } = "Pending";
        public string?          SettlementReference { get; set; }
        public string?          Notes              { get; set; }
    }

    // ── Legacy alias kept so existing code compiles ───────────────────────────────
    public class PaymentMethodEntity : TenantEntity
    {
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool    IsDefault   { get; set; } = false;
        public string? Icon        { get; set; }
        public int     SortOrder   { get; set; } = 0;
    }
}
