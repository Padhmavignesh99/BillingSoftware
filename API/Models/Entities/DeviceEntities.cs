using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    // ==================== DEVICE CONFIGURATION ====================
    /// <summary>
    /// Stores per-tenant device registration and configuration.
    /// A tenant can have multiple devices (e.g., 2 barcode scanners, 1 scale, 1 POS terminal).
    /// </summary>
    public class DeviceConfiguration : TenantEntity
    {
        /// <summary>Friendly name for the device (e.g., "Counter 1 Scanner").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>High-level category: Input | Output | Payment | Digital.</summary>
        public DeviceCategory DeviceCategory { get; set; } = DeviceCategory.Input;

        /// <summary>Type of device (barcode scanner, weighing scale, thermal printer, etc.).</summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>How the device connects to the system.</summary>
        public DeviceConnectionType ConnectionType { get; set; }

        /// <summary>Current operational status.</summary>
        public DeviceStatus Status { get; set; } = DeviceStatus.Active;

        /// <summary>Port/address (e.g., "COM3", "192.168.1.10:9100", "USB001").</summary>
        public string? PortOrAddress { get; set; }

        /// <summary>IP address for network devices (printers, customer displays on LAN).</summary>
        public string? IpAddress { get; set; }

        /// <summary>Network port number (e.g., 9100 for RAW printing, 80 for HTTP).</summary>
        public int? NetworkPort { get; set; }

        /// <summary>Serial port baud rate (for serial devices like weighing scales, VFD displays).</summary>
        public int? BaudRate { get; set; }

        /// <summary>Paper size for printers (Thermal58mm, Thermal80mm, A4, etc.).</summary>
        public PrinterPaperSize? PaperSize { get; set; }

        /// <summary>Whether this is the default device of its type for automatic selection.</summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>Brand/model of the device (e.g., "Honeywell Voyager 1200g").</summary>
        public string? Model { get; set; }

        /// <summary>Serial number or unique identifier of the physical device.</summary>
        public string? SerialNumber { get; set; }

        /// <summary>Which branch/counter this device is assigned to.</summary>
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        /// <summary>JSON blob for device-specific settings (baud rate, data bits, paper width, etc.).</summary>
        public string? ConfigurationJson { get; set; }

        /// <summary>When the device was last seen/connected.</summary>
        public DateTime? LastConnectedAt { get; set; }

        /// <summary>Notes about the device (installation, maintenance, etc.).</summary>
        public string? Notes { get; set; }
    }

    // ==================== DEVICE SETTINGS (PER TENANT) ====================
    /// <summary>
    /// Global device integration settings for a tenant — enables/disables device support at the shop level.
    /// One record per tenant.
    /// </summary>
    public class DeviceSettings : TenantEntity
    {
        // ── Barcode Scanner ──────────────────────────────────────────────────────
        /// <summary>Whether barcode scanner input is enabled across all modules.</summary>
        public bool BarcodeScannerEnabled { get; set; } = true;

        /// <summary>
        /// Suffix character(s) sent by the scanner after the barcode (Enter = "\r\n", Tab = "\t").
        /// The frontend uses this to detect when a full barcode has been received.
        /// </summary>
        public string BarcodeScannerSuffix { get; set; } = "\r\n";

        /// <summary>Minimum barcode length to treat as a valid scan (filters accidental keystrokes).</summary>
        public int BarcodeScannerMinLength { get; set; } = 3;

        /// <summary>
        /// Maximum time (ms) between keystrokes to consider input as a scanner (vs. manual typing).
        /// Default 50ms is safe for most scanners.
        /// </summary>
        public int BarcodeScannerThresholdMs { get; set; } = 50;

        // ── QR Scanner ───────────────────────────────────────────────────────────
        /// <summary>Whether QR scanner integration is active.</summary>
        public bool QRScannerEnabled { get; set; } = true;

        /// <summary>Whether to use device camera for QR scanning (WebRTC / HTML5 camera).</summary>
        public bool CameraQREnabled { get; set; } = false;

        // ── Weighing Scale ───────────────────────────────────────────────────────
        /// <summary>Whether weighing scale integration is enabled.</summary>
        public bool WeighingScaleEnabled { get; set; } = false;

        /// <summary>Communication mode for weighing scale.</summary>
        public ScaleCommunicationMode WeighingScaleMode { get; set; } = ScaleCommunicationMode.ManualEntry;

        /// <summary>Default weight unit used by the scale.</summary>
        public WeightUnit WeighingScaleUnit { get; set; } = WeightUnit.Kilogram;

        /// <summary>Number of decimal places in weight reading.</summary>
        public int WeighingScaleDecimalPlaces { get; set; } = 3;

        /// <summary>Serial port for the weighing scale (e.g., "COM4").</summary>
        public string? WeighingScalePort { get; set; }

        /// <summary>Baud rate of the weighing scale serial connection.</summary>
        public int WeighingScaleBaudRate { get; set; } = 9600;

        // ── OCR / Document Scanner ───────────────────────────────────────────────
        /// <summary>Whether OCR invoice scanning is enabled for Purchase module.</summary>
        public bool OcrScannerEnabled { get; set; } = false;

        /// <summary>OCR provider/engine to use (e.g., "Tesseract", "Azure OCR", "Google Vision").</summary>
        public string? OcrProvider { get; set; }

        /// <summary>API key for cloud OCR service.</summary>
        public string? OcrApiKey { get; set; }

        // ── Touchscreen ──────────────────────────────────────────────────────────
        /// <summary>Whether touchscreen-optimized UI is enabled (larger buttons, etc.).</summary>
        public bool TouchscreenModeEnabled { get; set; } = false;

        // ── RFID ─────────────────────────────────────────────────────────────────
        /// <summary>Whether RFID reader integration is enabled.</summary>
        public bool RFIDEnabled { get; set; } = false;

        // ── Biometric ────────────────────────────────────────────────────────────
        /// <summary>Whether biometric/fingerprint device integration is enabled.</summary>
        public bool BiometricEnabled { get; set; } = false;


        // ══ OUTPUT DEVICES ═══════════════════════════════════════════════════════

        // ── Thermal Receipt Printer ──────────────────────────────────────────────
        /// <summary>Whether thermal receipt printer integration is enabled.</summary>
        public bool ThermalPrinterEnabled { get; set; } = false;

        /// <summary>Friendly name (e.g., "POS Printer 1").</summary>
        public string? ThermalPrinterName { get; set; }

        /// <summary>Paper width: Thermal58mm or Thermal80mm.</summary>
        public PrinterPaperSize ThermalPaperSize { get; set; } = PrinterPaperSize.Thermal80mm;

        /// <summary>Connection type for the thermal printer.</summary>
        public ThermalPrinterConnection ThermalPrinterConnection { get; set; } = ThermalPrinterConnection.USB;

        /// <summary>Port or IP address (e.g., "COM1", "192.168.1.100:9100").</summary>
        public string? ThermalPrinterPort { get; set; }

        /// <summary>Print number of copies of each receipt.</summary>
        public int ThermalPrinterCopies { get; set; } = 1;

        /// <summary>Automatically print the bill when a sale is completed.</summary>
        public bool ThermalAutoPrint { get; set; } = true;

        /// <summary>Open cash drawer after printing via ESC/POS command.</summary>
        public bool ThermalOpenCashDrawer { get; set; } = false;

        // ── A4 Printer ───────────────────────────────────────────────────────────
        /// <summary>Whether A4 printer integration is enabled for invoice/document printing.</summary>
        public bool A4PrinterEnabled { get; set; } = false;

        /// <summary>Windows printer name as registered in the OS (e.g., "HP LaserJet").</summary>
        public string? A4PrinterName { get; set; }

        /// <summary>Paper size for the A4 printer.</summary>
        public PrinterPaperSize A4PaperSize { get; set; } = PrinterPaperSize.A4;

        /// <summary>Color or black-and-white output.</summary>
        public PrintColorMode A4ColorMode { get; set; } = PrintColorMode.BlackAndWhite;

        /// <summary>Number of copies to print.</summary>
        public int A4PrinterCopies { get; set; } = 1;

        /// <summary>Whether this is the default A4 printer.</summary>
        public bool A4PrinterIsDefault { get; set; } = false;

        // ── Label Printer ────────────────────────────────────────────────────────
        /// <summary>Whether label/barcode printer integration is enabled.</summary>
        public bool LabelPrinterEnabled { get; set; } = false;

        /// <summary>Windows/driver name of the label printer.</summary>
        public string? LabelPrinterName { get; set; }

        /// <summary>Label width in mm (e.g., 50).</summary>
        public decimal? LabelWidthMm { get; set; }

        /// <summary>Label height in mm (e.g., 25).</summary>
        public decimal? LabelHeightMm { get; set; }

        /// <summary>Number of labels per row on the sheet/roll.</summary>
        public int LabelPerRow { get; set; } = 1;

        /// <summary>Print product barcode on label.</summary>
        public bool LabelShowBarcode { get; set; } = true;

        /// <summary>Print MRP on label.</summary>
        public bool LabelShowMRP { get; set; } = true;

        /// <summary>Print business name on label.</summary>
        public bool LabelShowBusiness { get; set; } = true;

        // ── Customer Display ─────────────────────────────────────────────────────
        /// <summary>Whether a customer-facing display is connected.</summary>
        public bool CustomerDisplayEnabled { get; set; } = false;

        /// <summary>Communication protocol for the customer display.</summary>
        public CustomerDisplayProtocol CustomerDisplayProtocol { get; set; } = CustomerDisplayProtocol.Serial;

        /// <summary>Port or network address (e.g., "COM2", "192.168.1.200").</summary>
        public string? CustomerDisplayPort { get; set; }

        /// <summary>Welcome message shown when display is idle.</summary>
        public string? CustomerDisplayWelcomeMessage { get; set; } = "Welcome!";

        /// <summary>Show shop name on idle screen.</summary>
        public bool CustomerDisplayShowShopName { get; set; } = true;

        // ── Speaker / Audio ──────────────────────────────────────────────────────
        /// <summary>Whether audio/speaker feedback is enabled.</summary>
        public bool SpeakerEnabled { get; set; } = false;

        /// <summary>Play a beep sound when a barcode is successfully scanned.</summary>
        public bool SpeakerBeepOnScan { get; set; } = true;

        /// <summary>Announce the item total verbally (text-to-speech).</summary>
        public bool SpeakerAnnounceTotal { get; set; } = false;

        /// <summary>Announce payment confirmation verbally.</summary>
        public bool SpeakerAnnouncePayment { get; set; } = false;

        // ── Cash Drawer ──────────────────────────────────────────────────────────
        /// <summary>Whether automatic cash drawer opening is enabled.</summary>
        public bool CashDrawerEnabled { get; set; } = false;

        /// <summary>Connection method for the cash drawer.</summary>
        public DeviceConnectionType CashDrawerConnectionType { get; set; } = DeviceConnectionType.PrinterPort;

        /// <summary>Port the cash drawer is on (or the printer port it is wired through).</summary>
        public string? CashDrawerPort { get; set; }

        /// <summary>When the cash drawer should open automatically.</summary>
        public CashDrawerTrigger CashDrawerTrigger { get; set; } = CashDrawerTrigger.OnCashPayment;

        // ══ PAYMENT DEVICES ═══════════════════════════════════════════════════════

        // ── Card / POS Terminal ──────────────────────────────────────────────────
        /// <summary>Whether card/POS terminal integration is enabled.</summary>
        public bool CardTerminalEnabled { get; set; } = false;

        /// <summary>Card terminal provider (e.g., "Pine Labs", "Razorpay POS", "Paytm EDC").</summary>
        public string? CardTerminalProvider { get; set; }

        /// <summary>API key or credentials for card terminal (store encrypted).</summary>
        public string? CardTerminalApiKey { get; set; }

        /// <summary>Terminal ID assigned by the payment provider.</summary>
        public string? CardTerminalId { get; set; }

        // ── UPI QR ───────────────────────────────────────────────────────────────
        /// <summary>Whether UPI QR payment is enabled.</summary>
        public bool UPIEnabled { get; set; } = false;

        /// <summary>UPI ID of the shop (e.g., "shopname@upi").</summary>
        public string? UPIId { get; set; }

        /// <summary>Merchant name displayed in the UPI payment app.</summary>
        public string? UPIMerchantName { get; set; }

        /// <summary>Show UPI QR on receipt after payment.</summary>
        public bool UPIShowOnReceipt { get; set; } = true;

        /// <summary>Display UPI QR on customer display while billing.</summary>
        public bool UPIShowOnCustomerDisplay { get; set; } = false;

        // ── Payment Gateway ──────────────────────────────────────────────────────
        /// <summary>Whether an online payment gateway is enabled.</summary>
        public bool PaymentGatewayEnabled { get; set; } = false;

        /// <summary>Gateway provider name (e.g., "Razorpay", "Paytm", "Stripe", "PayU").</summary>
        public string? PaymentGatewayProvider { get; set; }

        /// <summary>API key (public) for the payment gateway.</summary>
        public string? PaymentGatewayApiKey { get; set; }

        /// <summary>API secret for the payment gateway (store encrypted).</summary>
        public string? PaymentGatewayApiSecret { get; set; }

        /// <summary>Webhook URL for receiving payment notifications.</summary>
        public string? PaymentGatewayWebhookUrl { get; set; }

        // ══ DIGITAL DELIVERY ══════════════════════════════════════════════════════

        // ── Email ────────────────────────────────────────────────────────────────
        /// <summary>Whether email invoice delivery is enabled.</summary>
        public bool EmailDeliveryEnabled { get; set; } = false;

        /// <summary>Auto-send invoice email on every completed sale.</summary>
        public bool EmailAutoSendOnSale { get; set; } = false;

        // ── WhatsApp ─────────────────────────────────────────────────────────────
        /// <summary>Whether WhatsApp invoice delivery is enabled.</summary>
        public bool WhatsAppDeliveryEnabled { get; set; } = false;

        /// <summary>Auto-send invoice via WhatsApp on every completed sale.</summary>
        public bool WhatsAppAutoSendOnSale { get; set; } = false;

        // ── SMS ──────────────────────────────────────────────────────────────────
        /// <summary>Whether SMS invoice/receipt delivery is enabled.</summary>
        public bool SMSDeliveryEnabled { get; set; } = false;

        /// <summary>Auto-send SMS receipt on every completed sale.</summary>
        public bool SMSAutoSendOnSale { get; set; } = false;

        // ── PDF ──────────────────────────────────────────────────────────────────
        /// <summary>Whether PDF invoice generation is enabled.</summary>
        public bool PDFEnabled { get; set; } = true;

        /// <summary>Auto-generate PDF when a sale is completed.</summary>
        public bool PDFAutoGenerate { get; set; } = false;

        /// <summary>Save generated PDFs to file storage.</summary>
        public bool PDFSaveToStorage { get; set; } = true;
    }

    // ==================== SCAN EVENT LOG ====================
    /// <summary>
    /// Audit log of every barcode/QR scan event processed by the system.
    /// Cross-module — records which module used the scan and the outcome.
    /// </summary>
    public class ScanEventLog : TenantEntity
    {
        /// <summary>Raw scanned value (barcode string or QR content).</summary>
        public string ScannedValue { get; set; } = string.Empty;

        /// <summary>Which module triggered this scan.</summary>
        public ScanModule Module { get; set; }

        /// <summary>Product resolved from the scan (if any).</summary>
        public int? ResolvedProductId { get; set; }
        public Product? ResolvedProduct { get; set; }

        /// <summary>Whether the scan resolved to a known product.</summary>
        public bool IsResolved { get; set; } = false;

        /// <summary>Reason if not resolved (e.g., "Product not found", "Invalid barcode").</summary>
        public string? UnresolvedReason { get; set; }

        /// <summary>ID of the device that generated this scan (if registered).</summary>
        public int? DeviceConfigurationId { get; set; }
        public DeviceConfiguration? DeviceConfiguration { get; set; }

        /// <summary>User who was logged in when the scan happened.</summary>
        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }

    // ==================== WEIGHT CAPTURE LOG ====================
    /// <summary>
    /// Records each weight reading captured from the weighing scale.
    /// Linked to the transaction that consumed it (invoice item, purchase item, etc.).
    /// </summary>
    public class WeightCaptureLog : TenantEntity
    {
        /// <summary>Weight value read from the scale (in the configured unit).</summary>
        public decimal Weight { get; set; }

        /// <summary>Unit of the captured weight.</summary>
        public WeightUnit WeightUnit { get; set; } = WeightUnit.Kilogram;

        /// <summary>Product being weighed.</summary>
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Module where the weight was captured.</summary>
        public ScanModule Module { get; set; }

        /// <summary>How the weight was obtained.</summary>
        public ScaleCommunicationMode CaptureMode { get; set; }

        /// <summary>Purchase rate per unit weight at time of capture (for purchase module).</summary>
        public decimal? RatePerUnit { get; set; }

        /// <summary>Computed amount = Weight * RatePerUnit.</summary>
        public decimal? ComputedAmount { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }

    // ==================== OCR SCAN RESULT ====================
    /// <summary>
    /// Stores the result of an OCR scan (supplier invoice, delivery note, etc.)
    /// for Purchase and stock-entry workflows.
    /// </summary>
    public class OcrScanResult : TenantEntity
    {
        /// <summary>Type of document that was scanned.</summary>
        public OcrDocumentType DocumentType { get; set; } = OcrDocumentType.SupplierInvoice;

        /// <summary>File URL of the uploaded document/image.</summary>
        public string? DocumentFileUrl { get; set; }

        /// <summary>Raw OCR text extracted from the document.</summary>
        public string? ExtractedText { get; set; }

        /// <summary>Structured JSON parsed from the OCR text (line items, totals, etc.).</summary>
        public string? ParsedDataJson { get; set; }

        /// <summary>Whether the OCR result was accepted and used to create a purchase draft.</summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>Linked purchase created from this OCR result.</summary>
        public int? LinkedPurchaseId { get; set; }
        public Purchase? LinkedPurchase { get; set; }

        /// <summary>OCR confidence score (0.0 to 1.0).</summary>
        public decimal? ConfidenceScore { get; set; }

        /// <summary>Provider used for OCR (e.g., "Tesseract", "Azure", "Google Vision").</summary>
        public string? OcrProvider { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }

    // ==================== CARD PAYMENT TRANSACTION ====================
    /// <summary>
    /// Tracks card/POS terminal transactions initiated from the billing software.
    /// Linked to an invoice payment.
    /// </summary>
    public class CardPaymentTransaction : TenantEntity
    {
        /// <summary>Invoice this card payment is for.</summary>
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        /// <summary>Amount sent to the card terminal.</summary>
        public decimal Amount { get; set; }

        /// <summary>Current status of the card payment.</summary>
        public CardPaymentStatus Status { get; set; } = CardPaymentStatus.Initiated;

        /// <summary>Approval code returned by the payment provider on success.</summary>
        public string? ApprovalCode { get; set; }

        /// <summary>Transaction reference number from provider.</summary>
        public string? ProviderTransactionId { get; set; }

        /// <summary>Card type (Visa, Mastercard, RuPay, UPI, etc.).</summary>
        public string? CardType { get; set; }

        /// <summary>Last 4 digits of the card (for receipt display).</summary>
        public string? CardLast4 { get; set; }

        /// <summary>Name of the card terminal provider.</summary>
        public string? TerminalProvider { get; set; }

        /// <summary>Terminal ID used for this transaction.</summary>
        public string? TerminalId { get; set; }

        /// <summary>Raw response JSON from the terminal.</summary>
        public string? RawResponseJson { get; set; }

        /// <summary>Error message if the transaction failed.</summary>
        public string? ErrorMessage { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    // ==================== QR SCAN EVENT ====================
    /// <summary>
    /// Records QR code scans separately from barcode scans,
    /// since QR codes can serve multiple purposes (product, UPI payment, customer ID, etc.).
    /// </summary>
    public class QRScanEvent : TenantEntity
    {
        /// <summary>Full decoded content of the QR code.</summary>
        public string QRContent { get; set; } = string.Empty;

        /// <summary>Purpose/intent of this QR scan.</summary>
        public QRScanPurpose Purpose { get; set; }

        /// <summary>Module where the scan was triggered.</summary>
        public ScanModule Module { get; set; }

        /// <summary>Resolved product (if QR maps to a product).</summary>
        public int? ResolvedProductId { get; set; }
        public Product? ResolvedProduct { get; set; }

        /// <summary>UPI payment string if Purpose = UPIPayment.</summary>
        public string? UPIString { get; set; }

        /// <summary>Amount from UPI QR (if applicable).</summary>
        public decimal? UPIAmount { get; set; }

        /// <summary>Whether scan was from camera (vs. dedicated QR scanner device).</summary>
        public bool IsFromCamera { get; set; } = false;

        public bool IsResolved { get; set; } = false;
        public string? UnresolvedReason { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }
}
