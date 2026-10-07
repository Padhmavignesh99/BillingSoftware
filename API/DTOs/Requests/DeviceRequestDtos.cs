using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.DTOs.Requests
{
    // ==================== DEVICE CONFIGURATION ====================
    public class CreateDeviceConfigurationRequest
    {
        public string Name { get; set; } = string.Empty;
        public DeviceType DeviceType { get; set; }
        public DeviceConnectionType ConnectionType { get; set; }
        public string? PortOrAddress { get; set; }
        public string? IpAddress { get; set; }
        public int? NetworkPort { get; set; }
        public int? BaudRate { get; set; }
        public PrinterPaperSize? PaperSize { get; set; }
        public bool? IsDefault { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int? BranchId { get; set; }
        public string? ConfigurationJson { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDeviceConfigurationRequest
    {
        public string? Name { get; set; }
        public DeviceConnectionType? ConnectionType { get; set; }
        public DeviceStatus? Status { get; set; }
        public string? PortOrAddress { get; set; }
        public string? IpAddress { get; set; }
        public int? NetworkPort { get; set; }
        public int? BaudRate { get; set; }
        public PrinterPaperSize? PaperSize { get; set; }
        public bool? IsDefault { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int? BranchId { get; set; }
        public string? ConfigurationJson { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== DEVICE SETTINGS ====================
    public class UpdateDeviceSettingsRequest
    {
        // Barcode Scanner
        public bool? BarcodeScannerEnabled { get; set; }
        public string? BarcodeScannerSuffix { get; set; }
        public int? BarcodeScannerMinLength { get; set; }
        public int? BarcodeScannerThresholdMs { get; set; }

        // QR Scanner
        public bool? QRScannerEnabled { get; set; }
        public bool? CameraQREnabled { get; set; }

        // Weighing Scale
        public bool? WeighingScaleEnabled { get; set; }
        public ScaleCommunicationMode? WeighingScaleMode { get; set; }
        public WeightUnit? WeighingScaleUnit { get; set; }
        public int? WeighingScaleDecimalPlaces { get; set; }
        public string? WeighingScalePort { get; set; }
        public int? WeighingScaleBaudRate { get; set; }

        // Other Input
        public bool? TouchscreenModeEnabled { get; set; }
        public bool? RFIDEnabled { get; set; }
        public bool? BiometricEnabled { get; set; }
        public bool? OcrScannerEnabled { get; set; }
        public string? OcrProvider { get; set; }
        public string? OcrApiKey { get; set; }

        // ── OUTPUT: Thermal Printer ───────────────────────────────────────────────
        public bool? ThermalPrinterEnabled { get; set; }
        public string? ThermalPrinterName { get; set; }
        public PrinterPaperSize? ThermalPaperSize { get; set; }
        public ThermalPrinterConnection? ThermalPrinterConnection { get; set; }
        public string? ThermalPrinterPort { get; set; }
        public int? ThermalPrinterCopies { get; set; }
        public bool? ThermalAutoPrint { get; set; }
        public bool? ThermalOpenCashDrawer { get; set; }

        // ── OUTPUT: A4 Printer ────────────────────────────────────────────────────
        public bool? A4PrinterEnabled { get; set; }
        public string? A4PrinterName { get; set; }
        public PrinterPaperSize? A4PaperSize { get; set; }
        public PrintColorMode? A4ColorMode { get; set; }
        public int? A4PrinterCopies { get; set; }
        public bool? A4PrinterIsDefault { get; set; }

        // ── OUTPUT: Label Printer ─────────────────────────────────────────────────
        public bool? LabelPrinterEnabled { get; set; }
        public string? LabelPrinterName { get; set; }
        public decimal? LabelWidthMm { get; set; }
        public decimal? LabelHeightMm { get; set; }
        public int? LabelPerRow { get; set; }
        public bool? LabelShowBarcode { get; set; }
        public bool? LabelShowMRP { get; set; }
        public bool? LabelShowBusiness { get; set; }

        // ── OUTPUT: Customer Display ──────────────────────────────────────────────
        public bool? CustomerDisplayEnabled { get; set; }
        public CustomerDisplayProtocol? CustomerDisplayProtocol { get; set; }
        public string? CustomerDisplayPort { get; set; }
        public string? CustomerDisplayWelcomeMessage { get; set; }
        public bool? CustomerDisplayShowShopName { get; set; }

        // ── OUTPUT: Speaker ───────────────────────────────────────────────────────
        public bool? SpeakerEnabled { get; set; }
        public bool? SpeakerBeepOnScan { get; set; }
        public bool? SpeakerAnnounceTotal { get; set; }
        public bool? SpeakerAnnouncePayment { get; set; }

        // ── OUTPUT: Cash Drawer ───────────────────────────────────────────────────
        public bool? CashDrawerEnabled { get; set; }
        public DeviceConnectionType? CashDrawerConnectionType { get; set; }
        public string? CashDrawerPort { get; set; }
        public CashDrawerTrigger? CashDrawerTrigger { get; set; }

        // ── PAYMENT: Card Terminal ────────────────────────────────────────────────
        public bool? CardTerminalEnabled { get; set; }
        public string? CardTerminalProvider { get; set; }
        public string? CardTerminalApiKey { get; set; }
        public string? CardTerminalId { get; set; }

        // ── PAYMENT: UPI ──────────────────────────────────────────────────────────
        public bool? UPIEnabled { get; set; }
        public string? UPIId { get; set; }
        public string? UPIMerchantName { get; set; }
        public bool? UPIShowOnReceipt { get; set; }
        public bool? UPIShowOnCustomerDisplay { get; set; }

        // ── PAYMENT: Gateway ──────────────────────────────────────────────────────
        public bool? PaymentGatewayEnabled { get; set; }
        public string? PaymentGatewayProvider { get; set; }
        public string? PaymentGatewayApiKey { get; set; }
        public string? PaymentGatewayApiSecret { get; set; }
        public string? PaymentGatewayWebhookUrl { get; set; }

        // ── DIGITAL: Email ────────────────────────────────────────────────────────
        public bool? EmailDeliveryEnabled { get; set; }
        public bool? EmailAutoSendOnSale { get; set; }

        // ── DIGITAL: WhatsApp ─────────────────────────────────────────────────────
        public bool? WhatsAppDeliveryEnabled { get; set; }
        public bool? WhatsAppAutoSendOnSale { get; set; }

        // ── DIGITAL: SMS ──────────────────────────────────────────────────────────
        public bool? SMSDeliveryEnabled { get; set; }
        public bool? SMSAutoSendOnSale { get; set; }

        // ── DIGITAL: PDF ──────────────────────────────────────────────────────────
        public bool? PDFEnabled { get; set; }
        public bool? PDFAutoGenerate { get; set; }
        public bool? PDFSaveToStorage { get; set; }
    }


    // ==================== BARCODE SCAN ====================
    /// <summary>Request sent when a barcode is scanned from any module.</summary>
    public class BarcodeScanRequest
    {
        /// <summary>Raw barcode value as received from the scanner.</summary>
        public string BarcodeValue { get; set; } = string.Empty;

        /// <summary>Which module triggered the scan.</summary>
        public ScanModule Module { get; set; }

        /// <summary>Registered device that produced this scan (optional).</summary>
        public int? DeviceConfigurationId { get; set; }
    }

    // ==================== QR SCAN ====================
    public class QRScanRequest
    {
        /// <summary>Decoded content of the QR code.</summary>
        public string QRContent { get; set; } = string.Empty;

        /// <summary>Intended purpose of the scan.</summary>
        public QRScanPurpose Purpose { get; set; }

        /// <summary>Which module triggered the scan.</summary>
        public ScanModule Module { get; set; }

        /// <summary>True if the QR was scanned via camera (not a physical QR scanner).</summary>
        public bool IsFromCamera { get; set; } = false;
    }

    // ==================== WEIGHT CAPTURE ====================
    public class WeightCaptureRequest
    {
        /// <summary>Weight value (in the configured unit).</summary>
        public decimal Weight { get; set; }

        /// <summary>Unit of the captured weight.</summary>
        public WeightUnit WeightUnit { get; set; } = WeightUnit.Kilogram;

        /// <summary>Product being weighed.</summary>
        public int? ProductId { get; set; }

        /// <summary>Module consuming this weight reading.</summary>
        public ScanModule Module { get; set; }

        /// <summary>How the weight was captured.</summary>
        public ScaleCommunicationMode CaptureMode { get; set; } = ScaleCommunicationMode.ManualEntry;

        /// <summary>Purchase/selling rate per weight unit.</summary>
        public decimal? RatePerUnit { get; set; }
    }

    // ==================== OCR SCAN ====================
    public class OcrScanRequest
    {
        /// <summary>Type of document being scanned.</summary>
        public OcrDocumentType DocumentType { get; set; } = OcrDocumentType.SupplierInvoice;

        /// <summary>Base64-encoded image or URL of the uploaded document.</summary>
        public string? DocumentBase64 { get; set; }

        /// <summary>URL to an already-uploaded file (alternative to Base64).</summary>
        public string? DocumentFileUrl { get; set; }

        /// <summary>If not null, auto-create a purchase draft from the OCR result.</summary>
        public bool AutoCreatePurchaseDraft { get; set; } = false;

        /// <summary>Supplier to assign if auto-creating a purchase draft.</summary>
        public int? SupplierId { get; set; }
    }

    // ==================== CARD PAYMENT ====================
    public class InitiateCardPaymentRequest
    {
        /// <summary>Invoice to pay.</summary>
        public int InvoiceId { get; set; }

        /// <summary>Amount to charge on the card terminal.</summary>
        public decimal Amount { get; set; }

        /// <summary>Terminal device configuration ID (if multiple terminals registered).</summary>
        public int? DeviceConfigurationId { get; set; }
    }

    public class UpdateCardPaymentStatusRequest
    {
        public CardPaymentStatus Status { get; set; }
        public string? ApprovalCode { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string? CardType { get; set; }
        public string? CardLast4 { get; set; }
        public string? RawResponseJson { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // ==================== CASH DRAWER ====================
    public class OpenCashDrawerRequest
    {
        /// <summary>Invoice that triggered the cash drawer open (for audit).</summary>
        public int? InvoiceId { get; set; }

        /// <summary>Reason for opening (if not from a payment).</summary>
        public string? Reason { get; set; }
    }
}
