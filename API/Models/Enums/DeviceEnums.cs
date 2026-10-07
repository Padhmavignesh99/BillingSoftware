namespace BillingSoftware.API.Models.Enums
{
    // ==================== DEVICE CATEGORY ====================
    /// <summary>
    /// High-level category grouping for the Device Settings page.
    /// Mirrors the UI hierarchy: Input → Output → Payment → Digital.
    /// </summary>
    public enum DeviceCategory
    {
        /// <summary>Devices that send data INTO the billing software (scanners, scales, keyboard, etc.).</summary>
        Input = 1,

        /// <summary>Devices that receive data FROM the billing software (printers, displays, drawers, speakers).</summary>
        Output = 2,

        /// <summary>Payment hardware (card/POS terminals, UPI QR readers, payment gateway integrations).</summary>
        Payment = 3,

        /// <summary>Digital delivery channels (email, WhatsApp, SMS, PDF).</summary>
        Digital = 4
    }

    // ==================== DEVICE TYPE ====================
    /// <summary>
    /// Identifies the specific type of device within its category.
    /// </summary>
    public enum DeviceType
    {
        // ── INPUT DEVICES ────────────────────────────────────────────────────────
        /// <summary>Standard keyboard — present on all setups.</summary>
        Keyboard = 1,

        /// <summary>Standard mouse — present on all desktop setups.</summary>
        Mouse = 2,

        /// <summary>USB/Serial/Bluetooth barcode scanner (HID keyboard emulation).</summary>
        BarcodeScanner = 3,

        /// <summary>QR code reader/scanner device.</summary>
        QRScanner = 4,

        /// <summary>POS touchscreen or capacitive touch display.</summary>
        Touchscreen = 5,

        /// <summary>Weighing scale (serial/USB/Bluetooth weight capture).</summary>
        WeighingScale = 6,

        /// <summary>Biometric fingerprint reader for employee auth/attendance.</summary>
        BiometricDevice = 7,

        /// <summary>Mobile phone or tablet used for billing.</summary>
        MobileTablet = 9,

        /// <summary>RFID reader for tagged inventory.</summary>
        RFIDReader = 12,

        /// <summary>Camera used for barcode/QR scanning or document capture.</summary>
        Camera = 13,

        /// <summary>Digital signature capture pad.</summary>
        SignaturePad = 14,

        /// <summary>Document/invoice OCR scanner or flatbed scanner.</summary>
        DocumentScanner = 15,

        // ── OUTPUT DEVICES ───────────────────────────────────────────────────────
        /// <summary>Thermal receipt printer (58mm/80mm paper roll). Prints customer bills at POS.</summary>
        ThermalPrinter = 20,

        /// <summary>A4 inkjet/laser printer. Prints invoices, reports, and purchase orders.</summary>
        A4Printer = 21,

        /// <summary>Label/barcode printer. Prints product labels with barcode/QR/MRP.</summary>
        LabelPrinter = 22,

        /// <summary>Secondary customer-facing display. Shows scanned items and total.</summary>
        CustomerDisplay = 23,

        /// <summary>Audio speaker or buzzer. Plays scan beeps or voice totals.</summary>
        Speaker = 24,

        /// <summary>Automated cash drawer connected to POS or thermal printer.</summary>
        CashDrawer = 25,

        /// <summary>Secondary POS monitor (billboard/customer TV display).</summary>
        Monitor = 26,

        // ── PAYMENT DEVICES ──────────────────────────────────────────────────────
        /// <summary>Card swipe / EMV chip / contactless POS terminal.</summary>
        CardPOSTerminal = 30,

        /// <summary>UPI QR code display/reader for UPI payments.</summary>
        UPITerminal = 31,

        /// <summary>Online payment gateway integration (Razorpay, Paytm, Stripe, etc.).</summary>
        PaymentGateway = 32,

        // ── DIGITAL DELIVERY ─────────────────────────────────────────────────────
        /// <summary>Email delivery for invoices, receipts, and reports.</summary>
        EmailDelivery = 40,

        /// <summary>WhatsApp invoice/receipt delivery.</summary>
        WhatsAppDelivery = 41,

        /// <summary>SMS invoice/receipt delivery.</summary>
        SMSDelivery = 42,

        /// <summary>PDF generation and download for invoices and reports.</summary>
        PDFGenerator = 43
    }

    // ==================== DEVICE CONNECTION TYPE ====================
    /// <summary>How the device is physically/logically connected.</summary>
    public enum DeviceConnectionType
    {
        USB = 1,
        Serial = 2,
        Bluetooth = 3,
        Network = 4,       // TCP/IP over LAN
        HIDKeyboard = 5,   // Barcode scanners that emulate keyboard
        WebAPI = 6,        // Cloud-connected payment terminals / digital services
        Camera = 7,        // WebRTC / device camera
        Virtual = 8,       // Software-only / simulated
        PrinterPort = 9    // Cash drawer wired through receipt printer port
    }

    // ==================== DEVICE STATUS ====================
    public enum DeviceStatus
    {
        Active = 1,
        Inactive = 2,
        Connected = 3,
        Disconnected = 4,
        Error = 5
    }

    // ==================== SCAN MODULE ====================
    /// <summary>Which billing module triggered a barcode/QR scan event.</summary>
    public enum ScanModule
    {
        Sales = 1,
        Purchase = 2,
        PurchaseReturn = 3,
        SalesReturn = 4,
        StockAdjustment = 5,
        StockTransfer = 6,
        Inventory = 7,
        ProductLookup = 8
    }

    // ==================== WEIGHT UNIT ====================
    public enum WeightUnit
    {
        Gram = 1,
        Kilogram = 2,
        Pound = 3,
        Ounce = 4,
        Milligram = 5
    }

    // ==================== SCALE COMMUNICATION ====================
    public enum ScaleCommunicationMode
    {
        /// <summary>Scale sends data automatically when stable weight detected.</summary>
        AutoSend = 1,
        /// <summary>Operator presses a button to capture weight.</summary>
        ManualCapture = 2,
        /// <summary>Software sends a poll command to request current weight.</summary>
        Poll = 3,
        /// <summary>Weight entered manually by operator (no physical scale).</summary>
        ManualEntry = 4
    }

    // ==================== OCR DOCUMENT TYPE ====================
    public enum OcrDocumentType
    {
        SupplierInvoice = 1,
        PurchaseReceipt = 2,
        DeliveryNote = 3,
        ProductLabel = 4,
        Other = 5
    }

    // ==================== QR SCAN PURPOSE ====================
    public enum QRScanPurpose
    {
        ProductIdentification = 1,
        UPIPayment = 2,
        CustomerIdentification = 3,
        InvoiceSharing = 4,
        Attendance = 5
    }

    // ==================== CARD PAYMENT STATUS ====================
    public enum CardPaymentStatus
    {
        Initiated = 1,
        Processing = 2,
        Approved = 3,
        Declined = 4,
        Cancelled = 5,
        Timeout = 6,
        Error = 7
    }

    // ==================== PRINTER PAPER SIZE ====================
    public enum PrinterPaperSize
    {
        /// <summary>58mm thermal roll — compact POS printers.</summary>
        Thermal58mm = 1,

        /// <summary>80mm thermal roll — standard POS printers.</summary>
        Thermal80mm = 2,

        /// <summary>Standard A4 (210×297mm).</summary>
        A4 = 3,

        /// <summary>A5 (148×210mm).</summary>
        A5 = 4,

        /// <summary>US Letter (216×279mm).</summary>
        Letter = 5,

        /// <summary>Label printer — size specified by width×height fields.</summary>
        CustomLabel = 6
    }

    // ==================== THERMAL PRINTER CONNECTION ====================
    public enum ThermalPrinterConnection
    {
        USB = 1,
        Serial = 2,
        Network = 3,
        Bluetooth = 4,
        Windows = 5   // Installed as Windows printer driver
    }

    // ==================== PRINT COLOR MODE ====================
    public enum PrintColorMode
    {
        BlackAndWhite = 1,
        Color = 2
    }

    // ==================== CASH DRAWER TRIGGER ====================
    /// <summary>When the cash drawer should open automatically.</summary>
    public enum CashDrawerTrigger
    {
        /// <summary>Open automatically on any cash payment.</summary>
        OnCashPayment = 1,

        /// <summary>Open after every successful bill (regardless of payment method).</summary>
        OnEveryBill = 2,

        /// <summary>Manual open only — no automatic trigger.</summary>
        ManualOnly = 3
    }

    // ==================== CUSTOMER DISPLAY PROTOCOL ====================
    public enum CustomerDisplayProtocol
    {
        /// <summary>RS-232 serial (most common for 2×20 VFD displays).</summary>
        Serial = 1,

        /// <summary>USB HID.</summary>
        USB = 2,

        /// <summary>Network display (web-based customer screen on tablet/TV).</summary>
        Network = 3,

        /// <summary>Secondary browser window on same machine (URL-based).</summary>
        BrowserWindow = 4
    }

    // ==================== UPI QR STATUS ====================
    public enum UPIQRStatus
    {
        Pending = 1,
        Paid = 2,
        Expired = 3,
        Failed = 4
    }

    // ==================== PAYMENT GATEWAY STATUS ====================
    public enum PaymentGatewayStatus
    {
        Initiated = 1,
        Processing = 2,
        Success = 3,
        Failed = 4,
        Refunded = 5,
        Cancelled = 6
    }

    // ==================== DIGITAL DELIVERY STATUS ====================
    public enum DigitalDeliveryStatus
    {
        Pending = 1,
        Sent = 2,
        Delivered = 3,
        Failed = 4,
        Bounced = 5
    }

    // ==================== BARCODE SCANNER MODE ====================
    public enum BarcodeScannerMode
    {
        /// <summary>Scanner emulates a USB keyboard (most common).</summary>
        HID = 1,
        /// <summary>Serial/COM port communication.</summary>
        Serial = 2,
        /// <summary>Scanner communicates over Bluetooth.</summary>
        Bluetooth = 3
    }
}
