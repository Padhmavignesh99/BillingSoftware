namespace MasterBilling.API.DTOs.Responses
{
    // ── Full payment response (wraps split lines) ─────────────────────────────────
    public class PaymentResponse
    {
        public int    Id              { get; set; }
        public int?   InvoiceId       { get; set; }
        public string? InvoiceNumber  { get; set; }
        public int?   CustomerId      { get; set; }
        public string? CustomerName   { get; set; }
        public decimal TotalAmount    { get; set; }
        public decimal PaidAmount     { get; set; }
        public decimal ChangeAmount   { get; set; }
        public decimal BalanceAmount  { get; set; }
        public string  Status         { get; set; } = string.Empty;
        public DateTime PaymentDate   { get; set; }
        public string? Notes          { get; set; }
        public string? CashierName    { get; set; }

        public List<PaymentDetailResponse> Details { get; set; } = new();

        // ── Legacy / convenience fields ────────────────────────────────────────────
        public string?  PaymentMethod   { get; set; }
        public string?  ReferenceNumber { get; set; }
        public string?  TransactionId   { get; set; }
        public decimal  Amount          { get; set; }
    }

    public class PaymentDetailResponse
    {
        public int    Id              { get; set; }
        public string Method          { get; set; } = string.Empty;
        public decimal Amount         { get; set; }
        public decimal CashReceived   { get; set; }
        public decimal ChangeGiven    { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? TransactionId  { get; set; }
        public string? UPIApp         { get; set; }
        public string? CardLastFour   { get; set; }
        public string? CardNetwork    { get; set; }
        public string  Status         { get; set; } = string.Empty;
    }

    // ── POS Terminal response ─────────────────────────────────────────────────────
    public class PaymentTerminalResponse
    {
        public int    Id              { get; set; }
        public string TerminalName    { get; set; } = string.Empty;
        public string TerminalId      { get; set; } = string.Empty;
        public string? MerchantId     { get; set; }
        public int    ProviderId      { get; set; }
        public string? ProviderName   { get; set; }
        public int?   BranchId        { get; set; }
        public string ConnectionType  { get; set; } = string.Empty;
        public string? IpAddress      { get; set; }
        public int?   Port            { get; set; }
        public string Status          { get; set; } = string.Empty;
        public DateTime? LastConnectedAt { get; set; }
    }

    // ── Payment provider response ─────────────────────────────────────────────────
    public class PaymentProviderResponse
    {
        public int    Id              { get; set; }
        public string ProviderName    { get; set; } = string.Empty;
        public string ProviderCode    { get; set; } = string.Empty;
        public string? ApiEndpoint    { get; set; }
        public bool   IsActive        { get; set; }
        public string? SupportedMethods { get; set; }
    }

    // ── Terminal transaction response ─────────────────────────────────────────────
    public class PaymentTransactionResponse
    {
        public int    Id                     { get; set; }
        public int    PaymentId              { get; set; }
        public int?   TerminalId             { get; set; }
        public string? TerminalName          { get; set; }
        public string TransactionRef         { get; set; } = string.Empty;
        public string? ProviderTransactionId { get; set; }
        public decimal Amount                { get; set; }
        public string Method                 { get; set; } = string.Empty;
        public string Status                 { get; set; } = string.Empty;
        public string? StatusMessage         { get; set; }
        public DateTime? InitiatedAt         { get; set; }
        public DateTime? CompletedAt         { get; set; }
    }

    // ── Refund response ───────────────────────────────────────────────────────────
    public class RefundResponse
    {
        public int    Id              { get; set; }
        public int?   InvoiceId       { get; set; }
        public string? InvoiceNumber  { get; set; }
        public int?   PaymentId       { get; set; }
        public int?   CustomerId      { get; set; }
        public string? CustomerName   { get; set; }
        public decimal Amount         { get; set; }
        public string Reason          { get; set; } = string.Empty;
        public string Status          { get; set; } = string.Empty;
        public string RefundMethod    { get; set; } = string.Empty;
        public DateTime RefundDate    { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    // ── Payment method config response ────────────────────────────────────────────
    public class PaymentMethodConfigResponse
    {
        public string Method      { get; set; } = string.Empty;
        public string MethodCode  { get; set; } = string.Empty;
        public bool   IsEnabled   { get; set; }
        public int    SortOrder   { get; set; }
        public string? DisplayName { get; set; }
        public string? Icon        { get; set; }
    }

    // ── Daily payment summary ─────────────────────────────────────────────────────
    public class PaymentSummaryResponse
    {
        public decimal TotalAmount      { get; set; }
        public decimal CashTotal        { get; set; }
        public decimal UPITotal         { get; set; }
        public decimal CardTotal        { get; set; }
        public decimal OtherTotal       { get; set; }
        public int     TotalTransactions { get; set; }
        public DateTime Date            { get; set; }
    }

    // ── Terminal connection test ───────────────────────────────────────────────────
    public class TerminalConnectionTestResponse
    {
        public bool   IsConnected { get; set; }
        public string Message     { get; set; } = string.Empty;
        public DateTime TestedAt  { get; set; }
    }
}
