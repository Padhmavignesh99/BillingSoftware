using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Models.Enums;

namespace MasterBilling.API.Services.Interfaces
{
    // ==================== DEVICE CONFIGURATION ====================
    /// <summary>
    /// Manages the registry of physical devices connected to the billing terminal.
    /// Supports CRUD for barcode scanners, weighing scales, card terminals, etc.
    /// </summary>
    public interface IDeviceConfigurationService
    {
        Task<List<DeviceConfigurationResponse>> GetAllDevicesAsync(int tenantId);
        Task<DeviceConfigurationResponse?> GetDeviceByIdAsync(int id, int tenantId);
        Task<List<DeviceConfigurationResponse>> GetDevicesByTypeAsync(DeviceType deviceType, int tenantId);
        Task<List<DeviceConfigurationResponse>> GetDevicesByCategoryAsync(DeviceCategory category, int tenantId);
        Task<DeviceConfigurationResponse> CreateDeviceAsync(CreateDeviceConfigurationRequest request, int tenantId);
        Task<DeviceConfigurationResponse> UpdateDeviceAsync(int id, UpdateDeviceConfigurationRequest request, int tenantId);
        Task<bool> DeleteDeviceAsync(int id, int tenantId);
        Task<bool> UpdateDeviceStatusAsync(int id, DeviceStatus status, int tenantId);
        Task<DeviceStatusResponse> GetDeviceStatusSummaryAsync(int tenantId);
    }

    // ==================== DEVICE SETTINGS ====================
    /// <summary>
    /// Manages tenant-level feature flags for device integration.
    /// Enables/disables barcode scanners, scales, card terminals, etc.
    /// </summary>
    public interface IDeviceSettingsService
    {
        /// <summary>Returns current device settings for the tenant (creates defaults if not configured).</summary>
        Task<DeviceSettingsResponse> GetSettingsAsync(int tenantId);

        /// <summary>Partially updates device settings (PATCH semantics).</summary>
        Task<DeviceSettingsResponse> UpdateSettingsAsync(int tenantId, UpdateDeviceSettingsRequest request);

        /// <summary>Resets all device settings to system defaults.</summary>
        Task<DeviceSettingsResponse> ResetToDefaultsAsync(int tenantId);
    }

    // ==================== BARCODE SCANNER SERVICE ====================
    /// <summary>
    /// Cross-module barcode scanner service.
    /// Used by Sales, Purchase, Purchase Return, Sales Return, Stock Adjustment, Inventory modules.
    /// A barcode scanner typically emulates a keyboard — the frontend sends the captured
    /// barcode string to this service, which looks up the product and returns the result.
    /// </summary>
    public interface IBarcodeScannerService
    {
        /// <summary>
        /// Processes a barcode scan event from any module.
        /// Looks up the product by barcode and logs the scan event.
        /// </summary>
        Task<BarcodeScanResponse> ProcessScanAsync(BarcodeScanRequest request, int tenantId, int userId);

        /// <summary>Looks up a product by barcode without logging a scan event.</summary>
        Task<ProductResponse?> LookupProductByBarcodeAsync(string barcodeValue, int tenantId);

        /// <summary>Returns scan history for a tenant with pagination.</summary>
        Task<(List<ScanEventLogResponse> Items, int TotalCount)> GetScanHistoryAsync(
            int tenantId, ScanModule? module, PaginationRequest pagination);

        /// <summary>Returns the last N scan events (for POS-style real-time display).</summary>
        Task<List<ScanEventLogResponse>> GetRecentScansAsync(int tenantId, int count = 10);
    }

    // ==================== QR SCANNER SERVICE ====================
    /// <summary>
    /// Handles QR code scan events across all modules.
    /// QR codes can encode product info, UPI payment strings, customer IDs, invoice links, etc.
    /// Also supports camera-based QR scanning (HTML5/WebRTC frontend).
    /// </summary>
    public interface IQRScannerService
    {
        /// <summary>
        /// Processes a QR scan.
        /// Automatically detects the QR content type (product, UPI, customer, etc.)
        /// and returns the appropriate resolved response.
        /// </summary>
        Task<QRScanResponse> ProcessScanAsync(QRScanRequest request, int tenantId, int userId);

        /// <summary>Parses a UPI QR string into its components (payee name, UPI ID, amount, etc.).</summary>
        Task<UPIPaymentInfo> ParseUPIQRAsync(string qrContent);

        /// <summary>Returns QR scan history for a tenant.</summary>
        Task<List<QRScanEvent>> GetScanHistoryAsync(int tenantId, PaginationRequest pagination);
    }

    // ==================== WEIGHING SCALE SERVICE ====================
    /// <summary>
    /// Manages weight capture from connected weighing scales.
    /// Used in grocery, vegetable, fruit, meat, bakery, hardware shops.
    /// Supports serial/USB scales as well as manual weight entry.
    /// Works across Sales, Purchase, Purchase Return, Sales Return, Stock Entry modules.
    /// </summary>
    public interface IWeighingScaleService
    {
        /// <summary>
        /// Captures and records a weight reading.
        /// Computes the line-item amount = Weight × RatePerUnit if rate provided.
        /// </summary>
        Task<WeightCaptureResponse> CaptureWeightAsync(WeightCaptureRequest request, int tenantId, int userId);

        /// <summary>Converts a weight between different units.</summary>
        Task<decimal> ConvertWeightAsync(decimal weight, WeightUnit fromUnit, WeightUnit toUnit);

        /// <summary>Returns recent weight capture logs for a tenant.</summary>
        Task<List<WeightCaptureResponse>> GetRecentCapturesAsync(int tenantId, int count = 20);

        /// <summary>Returns weight capture history with pagination.</summary>
        Task<(List<WeightCaptureResponse> Items, int TotalCount)> GetCaptureHistoryAsync(
            int tenantId, ScanModule? module, PaginationRequest pagination);
    }

    // ==================== OCR DOCUMENT SCANNER SERVICE ====================
    /// <summary>
    /// Handles OCR scanning of supplier invoices, delivery notes, and product labels.
    /// Used primarily in the Purchase module to auto-populate purchase drafts from scanned documents.
    /// Supports Tesseract (local), Azure Computer Vision, and Google Vision as OCR providers.
    /// </summary>
    public interface IOcrScannerService
    {
        /// <summary>
        /// Processes an uploaded document image through OCR.
        /// Returns extracted text and parsed line items.
        /// Optionally creates a purchase draft from the results.
        /// </summary>
        Task<OcrScanResponse> ProcessDocumentAsync(OcrScanRequest request, int tenantId, int userId);

        /// <summary>Returns a previously processed OCR result by ID.</summary>
        Task<OcrScanResponse?> GetOcrResultAsync(int id, int tenantId);

        /// <summary>Marks an OCR result as used (linked to a purchase).</summary>
        Task<bool> MarkAsUsedAsync(int id, int purchaseId, int tenantId);

        /// <summary>Returns OCR scan history for a tenant.</summary>
        Task<(List<OcrScanResult> Items, int TotalCount)> GetScanHistoryAsync(
            int tenantId, OcrDocumentType? docType, PaginationRequest pagination);

        /// <summary>
        /// Attempts to match OCR-extracted product names/barcodes to existing products in the database.
        /// Returns enriched line items with MatchedProductId populated.
        /// </summary>
        Task<List<OcrLineItem>> MatchProductsAsync(List<OcrLineItem> items, int tenantId);
    }

    // ==================== CARD PAYMENT SERVICE ====================
    /// <summary>
    /// Manages card/POS terminal payment transactions.
    /// Supports Pine Labs, Razorpay POS, Paytm EDC, and generic payment terminal APIs.
    /// Decoupled from the payment provider — uses the configured CardTerminalProvider setting.
    /// </summary>
    public interface ICardPaymentService
    {
        /// <summary>Initiates a card payment on the connected POS terminal.</summary>
        Task<CardPaymentTransactionResponse> InitiatePaymentAsync(
            InitiateCardPaymentRequest request, int tenantId, int userId);

        /// <summary>Updates the status of a card payment (called when terminal responds).</summary>
        Task<CardPaymentTransactionResponse> UpdatePaymentStatusAsync(
            int transactionId, UpdateCardPaymentStatusRequest request, int tenantId);

        /// <summary>Returns the current status of a card payment transaction.</summary>
        Task<CardPaymentTransactionResponse?> GetTransactionAsync(int transactionId, int tenantId);

        /// <summary>Returns card payment transaction history for an invoice.</summary>
        Task<List<CardPaymentTransactionResponse>> GetTransactionsByInvoiceAsync(int invoiceId, int tenantId);

        /// <summary>Cancels a pending card payment transaction.</summary>
        Task<bool> CancelTransactionAsync(int transactionId, int tenantId);
    }

    // ==================== CASH DRAWER SERVICE ====================
    /// <summary>
    /// Controls the automated cash drawer connected to the POS terminal.
    /// The drawer is typically wired to a receipt printer and opened via an ESC/POS command.
    /// </summary>
    public interface ICashDrawerService
    {
        /// <summary>Sends an open command to the cash drawer.</summary>
        Task<bool> OpenDrawerAsync(OpenCashDrawerRequest request, int tenantId, int userId);

        /// <summary>Returns the current state of the cash drawer (open/closed) if supported by hardware.</summary>
        Task<string> GetDrawerStatusAsync(int tenantId);
    }
}
