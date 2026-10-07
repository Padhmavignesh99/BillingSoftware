using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.DTOs.Responses
{
    // ==================== DEVICE CONFIGURATION ====================
    public class DeviceConfigurationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DeviceCategory { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ConnectionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PortOrAddress { get; set; }
        public string? IpAddress { get; set; }
        public int? NetworkPort { get; set; }
        public int? BaudRate { get; set; }
        public string? PaperSize { get; set; }
        public bool IsDefault { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? ConfigurationJson { get; set; }
        public DateTime? LastConnectedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==================== DEVICE SETTINGS ====================
    public class DeviceSettingsResponse
    {
        public int Id { get; set; }

        // ── INPUT: Barcode Scanner
        public bool BarcodeScannerEnabled { get; set; }
        public string BarcodeScannerSuffix { get; set; } = string.Empty;
        public int BarcodeScannerMinLength { get; set; }
        public int BarcodeScannerThresholdMs { get; set; }

        // ── INPUT: QR Scanner
        public bool QRScannerEnabled { get; set; }
        public bool CameraQREnabled { get; set; }

        // ── INPUT: Weighing Scale
        public bool WeighingScaleEnabled { get; set; }
        public string WeighingScaleMode { get; set; } = string.Empty;
        public string WeighingScaleUnit { get; set; } = string.Empty;
        public int WeighingScaleDecimalPlaces { get; set; }
        public string? WeighingScalePort { get; set; }
        public int WeighingScaleBaudRate { get; set; }

        // ── INPUT: Other
        public bool TouchscreenModeEnabled { get; set; }
        public bool RFIDEnabled { get; set; }
        public bool BiometricEnabled { get; set; }
        public bool OcrScannerEnabled { get; set; }
        public string? OcrProvider { get; set; }

        // ── OUTPUT: Thermal Printer
        public bool ThermalPrinterEnabled { get; set; }
        public string? ThermalPrinterName { get; set; }
        public string ThermalPaperSize { get; set; } = string.Empty;
        public string ThermalPrinterConnection { get; set; } = string.Empty;
        public string? ThermalPrinterPort { get; set; }
        public int ThermalPrinterCopies { get; set; }
        public bool ThermalAutoPrint { get; set; }
        public bool ThermalOpenCashDrawer { get; set; }

        // ── OUTPUT: A4 Printer
        public bool A4PrinterEnabled { get; set; }
        public string? A4PrinterName { get; set; }
        public string A4PaperSize { get; set; } = string.Empty;
        public string A4ColorMode { get; set; } = string.Empty;
        public int A4PrinterCopies { get; set; }
        public bool A4PrinterIsDefault { get; set; }

        // ── OUTPUT: Label Printer
        public bool LabelPrinterEnabled { get; set; }
        public string? LabelPrinterName { get; set; }
        public decimal? LabelWidthMm { get; set; }
        public decimal? LabelHeightMm { get; set; }
        public int LabelPerRow { get; set; }
        public bool LabelShowBarcode { get; set; }
        public bool LabelShowMRP { get; set; }
        public bool LabelShowBusiness { get; set; }

        // ── OUTPUT: Customer Display
        public bool CustomerDisplayEnabled { get; set; }
        public string CustomerDisplayProtocol { get; set; } = string.Empty;
        public string? CustomerDisplayPort { get; set; }
        public string? CustomerDisplayWelcomeMessage { get; set; }
        public bool CustomerDisplayShowShopName { get; set; }

        // ── OUTPUT: Speaker
        public bool SpeakerEnabled { get; set; }
        public bool SpeakerBeepOnScan { get; set; }
        public bool SpeakerAnnounceTotal { get; set; }
        public bool SpeakerAnnouncePayment { get; set; }

        // ── OUTPUT: Cash Drawer
        public bool CashDrawerEnabled { get; set; }
        public string CashDrawerConnectionType { get; set; } = string.Empty;
        public string? CashDrawerPort { get; set; }
        public string CashDrawerTrigger { get; set; } = string.Empty;

        // ── PAYMENT: Card Terminal
        public bool CardTerminalEnabled { get; set; }
        public string? CardTerminalProvider { get; set; }
        public string? CardTerminalId { get; set; }

        // ── PAYMENT: UPI
        public bool UPIEnabled { get; set; }
        public string? UPIId { get; set; }
        public string? UPIMerchantName { get; set; }
        public bool UPIShowOnReceipt { get; set; }
        public bool UPIShowOnCustomerDisplay { get; set; }

        // ── PAYMENT: Gateway
        public bool PaymentGatewayEnabled { get; set; }
        public string? PaymentGatewayProvider { get; set; }
        public string? PaymentGatewayWebhookUrl { get; set; }

        // ── DIGITAL: Email
        public bool EmailDeliveryEnabled { get; set; }
        public bool EmailAutoSendOnSale { get; set; }

        // ── DIGITAL: WhatsApp
        public bool WhatsAppDeliveryEnabled { get; set; }
        public bool WhatsAppAutoSendOnSale { get; set; }

        // ── DIGITAL: SMS
        public bool SMSDeliveryEnabled { get; set; }
        public bool SMSAutoSendOnSale { get; set; }

        // ── DIGITAL: PDF
        public bool PDFEnabled { get; set; }
        public bool PDFAutoGenerate { get; set; }
        public bool PDFSaveToStorage { get; set; }
    }

    // ==================== BARCODE SCAN ====================
    /// <summary>
    /// Result returned when a barcode is scanned from any module.
    /// Contains the resolved product (if found) plus context for the UI.
    /// </summary>
    public class BarcodeScanResponse
    {
        public string ScannedValue { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public string? UnresolvedReason { get; set; }
        public string Module { get; set; } = string.Empty;

        /// <summary>Resolved product details (null if not found).</summary>
        public ProductResponse? Product { get; set; }

        /// <summary>Scan event log ID for auditing.</summary>
        public int ScanEventLogId { get; set; }
    }

    // ==================== QR SCAN ====================
    public class QRScanResponse
    {
        public string QRContent { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public string? UnresolvedReason { get; set; }

        /// <summary>Resolved product (if QR maps to a product).</summary>
        public ProductResponse? Product { get; set; }

        /// <summary>UPI payment info (if Purpose = UPIPayment).</summary>
        public UPIPaymentInfo? UPIInfo { get; set; }

        public int QRScanEventId { get; set; }
    }

    public class UPIPaymentInfo
    {
        public string UPIString { get; set; } = string.Empty;
        public string? PayeeName { get; set; }
        public string? UPIId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionNote { get; set; }
    }

    // ==================== WEIGHT CAPTURE ====================
    public class WeightCaptureResponse
    {
        public int Id { get; set; }
        public decimal Weight { get; set; }
        public string WeightUnit { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Module { get; set; } = string.Empty;
        public string CaptureMode { get; set; } = string.Empty;
        public decimal? RatePerUnit { get; set; }
        public decimal? ComputedAmount { get; set; }

        /// <summary>Formatted display string, e.g., "5.250 kg × ₹52.00 = ₹273.00".</summary>
        public string DisplayText { get; set; } = string.Empty;

        public DateTime CapturedAt { get; set; }
    }

    // ==================== OCR SCAN ====================
    public class OcrScanResponse
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string? DocumentFileUrl { get; set; }
        public string? ExtractedText { get; set; }

        /// <summary>Parsed line items extracted from the invoice.</summary>
        public List<OcrLineItem> ParsedItems { get; set; } = new();

        /// <summary>Supplier name detected in the document.</summary>
        public string? DetectedSupplierName { get; set; }

        /// <summary>Invoice number detected.</summary>
        public string? DetectedInvoiceNumber { get; set; }

        /// <summary>Invoice date detected.</summary>
        public DateTime? DetectedInvoiceDate { get; set; }

        /// <summary>Total amount detected.</summary>
        public decimal? DetectedTotal { get; set; }

        public decimal? ConfidenceScore { get; set; }
        public string? OcrProvider { get; set; }

        /// <summary>Purchase draft ID if auto-creation was triggered.</summary>
        public int? CreatedPurchaseId { get; set; }

        public bool IsUsed { get; set; }
        public DateTime ScannedAt { get; set; }
    }

    public class OcrLineItem
    {
        public string? ProductName { get; set; }
        public string? Barcode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? HSNCode { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }

        /// <summary>Matched product ID from the database (null if not auto-matched).</summary>
        public int? MatchedProductId { get; set; }
        public string? MatchedProductName { get; set; }
    }

    // ==================== CARD PAYMENT ====================
    public class CardPaymentTransactionResponse
    {
        public int Id { get; set; }
        public int? InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ApprovalCode { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string? CardType { get; set; }
        public string? CardLast4 { get; set; }
        public string? TerminalProvider { get; set; }
        public string? TerminalId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime InitiatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    // ==================== SCAN EVENT LOG ====================
    public class ScanEventLogResponse
    {
        public int Id { get; set; }
        public string ScannedValue { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public string? UnresolvedReason { get; set; }
        public int? ResolvedProductId { get; set; }
        public string? ResolvedProductName { get; set; }
        public int? DeviceConfigurationId { get; set; }
        public string? DeviceName { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime ScannedAt { get; set; }
    }

    // ==================== DEVICE STATUS CHECK ====================
    /// <summary>Real-time status of all registered devices for a tenant.</summary>
    public class DeviceStatusResponse
    {
        public List<DeviceStatusItem> Devices { get; set; } = new();
        public DeviceSettingsResponse Settings { get; set; } = new();
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class DeviceStatusItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastConnectedAt { get; set; }
        public bool IsOnline { get; set; }
    }
}
