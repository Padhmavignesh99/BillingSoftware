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
        public bool ShowBusinessLogo      { get; set; } = true;
        public bool ShowBusinessAddress   { get; set; } = true;
        public bool ShowGSTDetails        { get; set; } = true;
        public bool ShowCustomerAddress   { get; set; } = true;
        public bool ShowItemHSNCode       { get; set; } = true;
        public bool ShowItemDiscount      { get; set; } = true;
        public bool ShowTaxBreakdown      { get; set; } = true;
        public bool ShowRoundOff          { get; set; } = true;
        public bool ShowAmountInWords     { get; set; } = true;
        public bool ShowTermsAndConditions{ get; set; } = true;
        public bool ShowFooterNote        { get; set; } = true;
        public bool ShowSignatureLine     { get; set; } = false;
        public bool ShowQRCode            { get; set; } = false;
        public bool ShowBarcode           { get; set; } = false;
        public bool ShowPaymentDetails    { get; set; } = true;
        public bool ShowDueAmount         { get; set; } = true;
        public bool ShowBankDetails       { get; set; } = false;

        /// <summary>Bank account details for printing (NEFT/UPI etc.)</summary>
        public string? BankDetails { get; set; }

        /// <summary>UPI ID for QR code generation on invoice</summary>
        public string? UpiId { get; set; }

        // ── Number of Copies ─────────────────────────────────────────────────────
        public int InvoiceCopies  { get; set; } = 1;
        public int QuotationCopies{ get; set; } = 1;
        public int PurchaseCopies { get; set; } = 1;
        public int ReceiptCopies  { get; set; } = 1;

        // ── Copy Labels (e.g. "Original", "Customer Copy", "Office Copy") ────────
        public string? InvoiceCopyLabel1  { get; set; } = "Original";
        public string? InvoiceCopyLabel2  { get; set; } = "Customer Copy";
        public string? InvoiceCopyLabel3  { get; set; } = "Office Copy";

        // ── Thermal / POS Receipt ────────────────────────────────────────────────
        /// <summary>Enable thermal / POS receipt mode (compact, no page breaks)</summary>
        public bool   ThermalPrintEnabled    { get; set; } = false;
        public string ThermalPaperWidth      { get; set; } = "80mm"; // 80mm | 58mm
        public bool   ThermalShowLogo        { get; set; } = true;
        public bool   ThermalShowQRCode      { get; set; } = true;
        public bool   ThermalShowItemCode    { get; set; } = false;
        public bool   ThermalShowCustomerName{ get; set; } = true;
        public bool   ThermalAutocut         { get; set; } = true;
        /// <summary>Number of blank lines after receipt before cut</summary>
        public int    ThermalLineFeedCount   { get; set; } = 3;

        // ── Label Printing ───────────────────────────────────────────────────────
        public bool   LabelPrintEnabled  { get; set; } = false;
        public string LabelPaperSize     { get; set; } = "50x25mm"; // e.g. 50x25mm, 60x40mm
        public bool   LabelShowBarcode   { get; set; } = true;
        public bool   LabelShowMRP       { get; set; } = true;
        public bool   LabelShowProductName{ get; set; } = true;
        public bool   LabelShowBusiness  { get; set; } = true;
        public int    LabelsPerRow       { get; set; } = 2;

        // ── Report Print Options ─────────────────────────────────────────────────
        public bool ShowReportHeader    { get; set; } = true;
        public bool ShowReportFooter    { get; set; } = true;
        public bool ShowPageNumbers     { get; set; } = true;
        public bool ShowPrintDateTime   { get; set; } = true;
        public bool ShowReportLogo      { get; set; } = true;

        // ── Color / Branding ─────────────────────────────────────────────────────
        /// <summary>Primary accent color for table headers (hex, e.g. "#1a73e8")</summary>
        public string? PrintPrimaryColor { get; set; } = "#1a73e8";
        public bool    PrintInColor      { get; set; } = true;

        // ── Default Printer ──────────────────────────────────────────────────────
        public string? DefaultPrinterName      { get; set; }
        public string? DefaultThermalPrinter   { get; set; }
        public string? DefaultLabelPrinter     { get; set; }
    }
}
