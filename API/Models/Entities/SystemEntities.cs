using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    public class FileUpload : TenantEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public FileType FileType { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
    }

    public class AuditLog : BaseEntity
    {
        public int? TenantId { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class Lookup : BaseEntity
    {
        public string Category { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; } = 0;
        public int? ParentId { get; set; }
    }

    public class BackupRecord : BaseEntity
    {
        public int? TenantId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Status { get; set; } = "Completed";
        public string? Notes { get; set; }
        public DateTime BackupDate { get; set; } = DateTime.UtcNow;
    }

    // ==================== PRINT SETTINGS ====================
    /// <summary>
    /// Stores per-tenant print configuration for invoices, receipts, reports, and labels.
    /// Supports a three-level hierarchy: tenant → branch → counter.
    /// When BranchId and CounterId are both null → tenant-level defaults (applies to all branches/counters).
    /// When BranchId is set but CounterId is null → branch-level override.
    /// When both BranchId and CounterId are set → counter-level override.
    /// </summary>
    public class PrintSettings : TenantEntity
    {
        // ── General / Paper ──────────────────────────────────────────────────────
        /// <summary>Paper size: A4 | A5 | Letter | Legal | Thermal80mm | Thermal58mm | Custom</summary>
        public string PaperSize { get; set; } = "A4";

        /// <summary>Custom width in mm (used only when PaperSize = "Custom")</summary>
        public decimal? CustomPaperWidthMm { get; set; }

        /// <summary>Custom height in mm (used only when PaperSize = "Custom")</summary>
        public decimal? CustomPaperHeightMm { get; set; }

        /// <summary>Portrait | Landscape</summary>
        public string Orientation { get; set; } = "Portrait";

        // ── Margins (mm) ─────────────────────────────────────────────────────────
        public decimal MarginTopMm    { get; set; } = 10;
        public decimal MarginBottomMm { get; set; } = 10;
        public decimal MarginLeftMm   { get; set; } = 10;
        public decimal MarginRightMm  { get; set; } = 10;

        // ── Typography ───────────────────────────────────────────────────────────
        /// <summary>Font family name, e.g. "Arial", "Roboto", "NotoSans"</summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>Base body font size in pt</summary>
        public decimal FontSizePt { get; set; } = 10;

        /// <summary>Header / title font size in pt</summary>
        public decimal HeaderFontSizePt { get; set; } = 14;

        /// <summary>Footer font size in pt</summary>
        public decimal FooterFontSizePt { get; set; } = 8;

        // ── Invoice Print Options ────────────────────────────────────────────────
        public bool ShowBusinessLogo       { get; set; } = true;
        public bool ShowBusinessAddress    { get; set; } = true;
        public bool ShowGSTDetails         { get; set; } = true;
        public bool ShowCustomerAddress    { get; set; } = true;
        public bool ShowItemHSNCode        { get; set; } = true;
        public bool ShowItemDiscount       { get; set; } = true;
        public bool ShowTaxBreakdown       { get; set; } = true;
        public bool ShowRoundOff           { get; set; } = true;
        public bool ShowAmountInWords      { get; set; } = true;
        public bool ShowTermsAndConditions { get; set; } = true;
        public bool ShowFooterNote         { get; set; } = true;
        public bool ShowSignatureLine      { get; set; } = false;
        public bool ShowQRCode             { get; set; } = false;
        public bool ShowBarcode            { get; set; } = false;
        public bool ShowPaymentDetails     { get; set; } = true;
        public bool ShowDueAmount          { get; set; } = true;
        public bool ShowBankDetails        { get; set; } = false;

        /// <summary>Bank account details for printing (NEFT/UPI etc.)</summary>
        public string? BankDetails { get; set; }

        /// <summary>UPI ID for QR code generation on invoice</summary>
        public string? UpiId { get; set; }

        // ── Number of Copies ─────────────────────────────────────────────────────
        public int InvoiceCopies   { get; set; } = 1;
        public int QuotationCopies { get; set; } = 1;
        public int PurchaseCopies  { get; set; } = 1;
        public int ReceiptCopies   { get; set; } = 1;

        // ── Copy Labels (e.g. "Original", "Customer Copy", "Office Copy") ────────
        public string? InvoiceCopyLabel1 { get; set; } = "Original";
        public string? InvoiceCopyLabel2 { get; set; } = "Customer Copy";
        public string? InvoiceCopyLabel3 { get; set; } = "Office Copy";

        // ── Thermal / POS Receipt ────────────────────────────────────────────────
        /// <summary>Enable thermal / POS receipt mode (compact, no page breaks)</summary>
        public bool   ThermalPrintEnabled     { get; set; } = false;
        public string ThermalPaperWidth       { get; set; } = "80mm"; // 80mm | 58mm
        public bool   ThermalShowLogo         { get; set; } = true;
        public bool   ThermalShowQRCode       { get; set; } = true;
        public bool   ThermalShowItemCode     { get; set; } = false;
        public bool   ThermalShowCustomerName { get; set; } = true;
        public bool   ThermalAutocut          { get; set; } = true;
        /// <summary>Number of blank lines after receipt before cut</summary>
        public int    ThermalLineFeedCount    { get; set; } = 3;

        // ── Label Printing ───────────────────────────────────────────────────────
        public bool   LabelPrintEnabled   { get; set; } = false;
        public string LabelPaperSize      { get; set; } = "50x25mm"; // e.g. 50x25mm, 60x40mm
        public bool   LabelShowBarcode    { get; set; } = true;
        public bool   LabelShowMRP        { get; set; } = true;
        public bool   LabelShowProductName { get; set; } = true;
        public bool   LabelShowBusiness   { get; set; } = true;
        public int    LabelsPerRow        { get; set; } = 2;

        // ── Report Print Options ─────────────────────────────────────────────────
        public bool ShowReportHeader  { get; set; } = true;
        public bool ShowReportFooter  { get; set; } = true;
        public bool ShowPageNumbers   { get; set; } = true;
        public bool ShowPrintDateTime { get; set; } = true;
        public bool ShowReportLogo    { get; set; } = true;

        // ── Color / Branding ─────────────────────────────────────────────────────
        /// <summary>Primary accent color for table headers (hex, e.g. "#1a73e8")</summary>
        public string? PrintPrimaryColor { get; set; } = "#1a73e8";
        public bool    PrintInColor      { get; set; } = true;

        // ── Default Printer Names ────────────────────────────────────────────────
        public string? DefaultPrinterName    { get; set; }
        public string? DefaultThermalPrinter { get; set; }
        public string? DefaultLabelPrinter   { get; set; }

        // ── Network / Direct Printer Configuration ───────────────────────────────
        /// <summary>IP address of the network printer (e.g. "192.168.1.50")</summary>
        public string? PrinterIpAddress    { get; set; }

        /// <summary>Network printer port — 9100 for RAW/ESC-POS, 631 for IPP</summary>
        public int?    PrinterPort         { get; set; } = 9100;

        /// <summary>Printer connection type: USB | Network | Bluetooth | Cloud</summary>
        public string  PrinterConnection   { get; set; } = "USB";

        /// <summary>Open cash drawer after printing a receipt</summary>
        public bool    OpenCashDrawer      { get; set; } = false;

        /// <summary>Footer text shown at the bottom of every thermal receipt</summary>
        public string? FooterText          { get; set; } = "Thank you! Visit again";

        // ── Branch / Counter Level Configuration Hierarchy ───────────────────────
        /// <summary>
        /// Optional BranchId — if set, this record is a branch-level override.
        /// Null = tenant-level default (applies to all branches unless overridden at branch or counter level).
        /// </summary>
        public int?    BranchId  { get; set; }
        public Branch? Branch    { get; set; }

        /// <summary>
        /// Optional counter number within a branch (e.g. 1, 2, 3).
        /// Null = applies to the entire branch or tenant level.
        /// Useful in large shops with multiple billing counters sharing different printers.
        /// </summary>
        public int?    CounterId { get; set; }
    }

    // ==================== PRINT JOB ====================
    /// <summary>
    /// Tracks every print job submitted to the system.
    /// Supports full lifecycle: Queued → Printing → Printed | Failed,
    /// automatic retry with back-off, reprint with duplicate watermark,
    /// and a complete audit trail per branch / counter.
    /// </summary>
    public class PrintJob : TenantEntity
    {
        /// <summary>Document type: Invoice | Receipt | Quotation | Purchase | SalesReturn | Report | Label</summary>
        public string  DocumentType        { get; set; } = "Invoice";

        /// <summary>Primary key of the source document (InvoiceId, QuotationId, etc.)</summary>
        public int?    DocumentId          { get; set; }

        /// <summary>Human-readable document number shown in logs (e.g. "INV-2024-00001")</summary>
        public string? DocumentReference   { get; set; }

        /// <summary>Job lifecycle status: Queued | Printing | Printed | Failed | Cancelled</summary>
        public string  Status              { get; set; } = "Queued";

        /// <summary>Printer name, IP:Port, or "Browser" / "PDF"</summary>
        public string? PrinterTarget       { get; set; }

        /// <summary>Paper size used for this job (e.g. "A4", "Thermal80mm")</summary>
        public string? PaperSize           { get; set; }

        /// <summary>Number of copies requested (each copy is a separate ESC/POS or PDF page)</summary>
        public int     Copies              { get; set; } = 1;

        // ── Retry Logic ──────────────────────────────────────────────────────────
        /// <summary>Number of retry attempts made so far for this job</summary>
        public int     RetryCount          { get; set; } = 0;

        /// <summary>Maximum allowed retries before job is permanently marked Failed</summary>
        public int     MaxRetries          { get; set; } = 3;

        /// <summary>Error message captured from the last failed attempt</summary>
        public string? ErrorMessage        { get; set; }

        /// <summary>UTC timestamp when the job was successfully printed</summary>
        public DateTime? PrintedAt         { get; set; }

        /// <summary>Earliest UTC time at which the next retry should be attempted</summary>
        public DateTime? RetryAfter        { get; set; }

        // ── Reprint / Duplicate Copy ──────────────────────────────────────────────
        /// <summary>True when this job is a reprint of a previously printed document</summary>
        public bool    IsReprint           { get; set; } = false;

        /// <summary>
        /// Id of the original PrintJob (when IsReprint = true).
        /// Enables full reprint audit trail: who reprinted, when, and from which counter.
        /// </summary>
        public int?    OriginalPrintJobId  { get; set; }

        // ── Initiator Context ─────────────────────────────────────────────────────
        /// <summary>UserId who triggered this print job</summary>
        public int?    InitiatedByUserId   { get; set; }
        public User?   InitiatedByUser     { get; set; }

        /// <summary>BranchId from which the print was initiated</summary>
        public int?    BranchId            { get; set; }

        /// <summary>Counter number from which the print was initiated</summary>
        public int?    CounterId           { get; set; }
    }
}
