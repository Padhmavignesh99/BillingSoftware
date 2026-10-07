using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Device
{
    // ==================== DEVICE CONFIGURATION CONTROLLER ====================
    /// <summary>
    /// Manages the registry of physical devices connected to the billing terminal.
    /// Each device (barcode scanner, scale, card terminal, etc.) can be registered
    /// with its connection settings and assigned to a branch/counter.
    /// </summary>
    [ApiController]
    [Route("api/devices")]
    [Authorize]
    public class DeviceConfigurationController : ControllerBase
    {
        private readonly IDeviceConfigurationService _service;
        public DeviceConfigurationController(IDeviceConfigurationService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>Get all registered devices for this tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DeviceConfigurationResponse>>>> GetAll()
            => Ok(ApiResponse<List<DeviceConfigurationResponse>>.SuccessResponse(
                await _service.GetAllDevicesAsync(TenantId)));

        /// <summary>Get a specific device by ID.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DeviceConfigurationResponse>>> GetById(int id)
        {
            var result = await _service.GetDeviceByIdAsync(id, TenantId);
            return result == null
                ? NotFound(ApiResponse<DeviceConfigurationResponse>.FailResponse("Device not found"))
                : Ok(ApiResponse<DeviceConfigurationResponse>.SuccessResponse(result));
        }

        /// <summary>Get all devices of a specific type (e.g., all barcode scanners).</summary>
        [HttpGet("by-type/{deviceType}")]
        public async Task<ActionResult<ApiResponse<List<DeviceConfigurationResponse>>>> GetByType(DeviceType deviceType)
            => Ok(ApiResponse<List<DeviceConfigurationResponse>>.SuccessResponse(
                await _service.GetDevicesByTypeAsync(deviceType, TenantId)));

        /// <summary>
        /// Get all devices by category.
        /// Used by the Device Settings page to load each section (Input / Output / Payment / Digital).
        /// </summary>
        [HttpGet("by-category/{category}")]
        public async Task<ActionResult<ApiResponse<List<DeviceConfigurationResponse>>>> GetByCategory(DeviceCategory category)
            => Ok(ApiResponse<List<DeviceConfigurationResponse>>.SuccessResponse(
                await _service.GetDevicesByCategoryAsync(category, TenantId)));

        /// <summary>Get real-time status summary of all registered devices.</summary>
        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<DeviceStatusResponse>>> GetStatus()
            => Ok(ApiResponse<DeviceStatusResponse>.SuccessResponse(
                await _service.GetDeviceStatusSummaryAsync(TenantId)));

        /// <summary>Register a new device.</summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<DeviceConfigurationResponse>>> Create(
            [FromBody] CreateDeviceConfigurationRequest request)
            => Ok(ApiResponse<DeviceConfigurationResponse>.SuccessResponse(
                await _service.CreateDeviceAsync(request, TenantId), "Device registered"));

        /// <summary>Update a device's settings.</summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<DeviceConfigurationResponse>>> Update(
            int id, [FromBody] UpdateDeviceConfigurationRequest request)
            => Ok(ApiResponse<DeviceConfigurationResponse>.SuccessResponse(
                await _service.UpdateDeviceAsync(id, request, TenantId), "Updated"));

        /// <summary>Update a device's online status (Connected / Disconnected / Error).</summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromQuery] DeviceStatus status)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _service.UpdateDeviceStatusAsync(id, status, TenantId)));

        /// <summary>Deregister a device.</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _service.DeleteDeviceAsync(id, TenantId), "Device removed"));
    }

    // ==================== DEVICE SETTINGS CONTROLLER ====================
    /// <summary>
    /// Manages tenant-level device feature flags.
    /// Controls which device types are enabled for this shop
    /// (barcode scanner, weighing scale, card terminal, OCR, etc.).
    /// </summary>
    [ApiController]
    [Route("api/device-settings")]
    [Authorize]
    public class DeviceSettingsController : ControllerBase
    {
        private readonly IDeviceSettingsService _service;
        public DeviceSettingsController(IDeviceSettingsService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Get current device settings for this tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<DeviceSettingsResponse>>> Get()
            => Ok(ApiResponse<DeviceSettingsResponse>.SuccessResponse(
                await _service.GetSettingsAsync(TenantId)));

        /// <summary>Update device settings (PATCH — only supplied fields are updated).</summary>
        [HttpPatch]
        public async Task<ActionResult<ApiResponse<DeviceSettingsResponse>>> Update(
            [FromBody] UpdateDeviceSettingsRequest request)
            => Ok(ApiResponse<DeviceSettingsResponse>.SuccessResponse(
                await _service.UpdateSettingsAsync(TenantId, request), "Settings updated"));

        /// <summary>Reset all device settings to system defaults.</summary>
        [HttpPost("reset")]
        public async Task<ActionResult<ApiResponse<DeviceSettingsResponse>>> Reset()
            => Ok(ApiResponse<DeviceSettingsResponse>.SuccessResponse(
                await _service.ResetToDefaultsAsync(TenantId), "Settings reset to defaults"));
    }

    // ==================== BARCODE SCANNER CONTROLLER ====================
    /// <summary>
    /// Handles barcode scan events from all billing modules.
    ///
    /// USAGE — How barcode scanning works in this software:
    ///   1. The physical barcode scanner emulates a keyboard (HID mode).
    ///   2. The Angular frontend captures the scan in a dedicated barcode input field
    ///      (using keystroke timing to distinguish scanner from keyboard).
    ///   3. The frontend sends the captured barcode string to POST /api/barcode/scan.
    ///   4. The API looks up the product and returns the result immediately.
    ///   5. The frontend adds the product to the current bill/purchase/return form.
    ///
    /// This endpoint works identically for:
    ///   - Sales / POS billing
    ///   - Purchase entry
    ///   - Purchase Return
    ///   - Sales Return
    ///   - Stock Adjustment
    ///   - Stock Transfer
    ///   - Inventory lookup
    /// </summary>
    [ApiController]
    [Route("api/barcode")]
    [Authorize]
    public class BarcodeScannerController : ControllerBase
    {
        private readonly IBarcodeScannerService _service;
        public BarcodeScannerController(IBarcodeScannerService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>
        /// Process a barcode scan event from any module.
        /// Returns the resolved product (if found) along with a scan audit log entry.
        /// </summary>
        [HttpPost("scan")]
        public async Task<ActionResult<ApiResponse<BarcodeScanResponse>>> Scan(
            [FromBody] BarcodeScanRequest request)
            => Ok(ApiResponse<BarcodeScanResponse>.SuccessResponse(
                await _service.ProcessScanAsync(request, TenantId, UserId)));

        /// <summary>
        /// Quickly look up a product by barcode without creating a scan log entry.
        /// Useful for product search/verification.
        /// </summary>
        [HttpGet("lookup/{barcodeValue}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> Lookup(string barcodeValue)
        {
            var result = await _service.LookupProductByBarcodeAsync(barcodeValue, TenantId);
            return result == null
                ? NotFound(ApiResponse<ProductResponse>.FailResponse("Product not found"))
                : Ok(ApiResponse<ProductResponse>.SuccessResponse(result));
        }

        /// <summary>Get barcode scan history with optional module filter and pagination.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<ScanEventLogResponse>>>> GetHistory(
            [FromQuery] ScanModule? module,
            [FromQuery] PaginationRequest pagination)
        {
            var (items, total) = await _service.GetScanHistoryAsync(TenantId, module, pagination);
            return Ok(ApiResponse<List<ScanEventLogResponse>>.SuccessResponse(
                items, "Success", total, pagination.Page, pagination.PageSize));
        }

        /// <summary>Get the most recent N scan events (for POS real-time display).</summary>
        [HttpGet("recent")]
        public async Task<ActionResult<ApiResponse<List<ScanEventLogResponse>>>> GetRecent(
            [FromQuery] int count = 10)
            => Ok(ApiResponse<List<ScanEventLogResponse>>.SuccessResponse(
                await _service.GetRecentScansAsync(TenantId, count)));
    }

    // ==================== QR SCANNER CONTROLLER ====================
    /// <summary>
    /// Handles QR code scan events for products, UPI payments, customer identification, etc.
    ///
    /// QR codes in billing software can encode:
    ///   - Product barcodes/SKU → same as barcode scanner
    ///   - UPI payment strings → upi://pay?pa=merchant@upi&am=500
    ///   - Customer loyalty QR → for customer identification
    ///   - Invoice sharing QR  → for WhatsApp/email sharing
    ///
    /// Both dedicated QR scanners and HTML5 camera-based scanning are supported.
    /// </summary>
    [ApiController]
    [Route("api/qr")]
    [Authorize]
    public class QRScannerController : ControllerBase
    {
        private readonly IQRScannerService _service;
        public QRScannerController(IQRScannerService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>
        /// Process a QR code scan from any module.
        /// Auto-detects the QR type (product, UPI, customer, etc.) and returns the resolved result.
        /// </summary>
        [HttpPost("scan")]
        public async Task<ActionResult<ApiResponse<QRScanResponse>>> Scan(
            [FromBody] QRScanRequest request)
            => Ok(ApiResponse<QRScanResponse>.SuccessResponse(
                await _service.ProcessScanAsync(request, TenantId, UserId)));

        /// <summary>Parse a raw UPI QR string into its components (payee, amount, etc.).</summary>
        [HttpPost("parse-upi")]
        public async Task<ActionResult<ApiResponse<UPIPaymentInfo>>> ParseUPI(
            [FromBody] string qrContent)
            => Ok(ApiResponse<UPIPaymentInfo>.SuccessResponse(
                await _service.ParseUPIQRAsync(qrContent)));

        /// <summary>Get QR scan history for this tenant.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetHistory(
            [FromQuery] PaginationRequest pagination)
        {
            var items = await _service.GetScanHistoryAsync(TenantId, pagination);
            return Ok(ApiResponse<List<object>>.SuccessResponse(
                items.Cast<object>().ToList(), "Success", items.Count, pagination.Page, pagination.PageSize));
        }
    }

    // ==================== WEIGHING SCALE CONTROLLER ====================
    /// <summary>
    /// Manages weight capture from connected weighing scales.
    ///
    /// Used in shops that sell by weight:
    ///   - Grocery / Kirana stores
    ///   - Vegetable and fruit shops
    ///   - Meat and fish shops
    ///   - Bakery and sweet shops
    ///   - Rice, dal, and grain stores
    ///   - Hardware and material stores
    ///
    /// Supported capture modes:
    ///   - AutoSend: Scale automatically sends weight when stable
    ///   - ManualCapture: Operator presses button on scale
    ///   - Poll: Software polls scale for current weight
    ///   - ManualEntry: Operator types weight manually (no physical scale)
    ///
    /// Works across all modules: Sales, Purchase, Purchase Return, Sales Return, Stock.
    /// </summary>
    [ApiController]
    [Route("api/weighing-scale")]
    [Authorize]
    public class WeighingScaleController : ControllerBase
    {
        private readonly IWeighingScaleService _service;
        public WeighingScaleController(IWeighingScaleService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>
        /// Capture and record a weight reading.
        /// If RatePerUnit is provided, the computed amount (Weight × Rate) is returned.
        ///
        /// Example: Rice → Weight=5.250 kg, Rate=₹52/kg → Amount=₹273.00
        /// </summary>
        [HttpPost("capture")]
        public async Task<ActionResult<ApiResponse<WeightCaptureResponse>>> Capture(
            [FromBody] WeightCaptureRequest request)
            => Ok(ApiResponse<WeightCaptureResponse>.SuccessResponse(
                await _service.CaptureWeightAsync(request, TenantId, UserId)));

        /// <summary>Convert a weight value between units (e.g., kg to grams).</summary>
        [HttpGet("convert")]
        public async Task<ActionResult<ApiResponse<decimal>>> Convert(
            [FromQuery] decimal weight,
            [FromQuery] WeightUnit fromUnit,
            [FromQuery] WeightUnit toUnit)
            => Ok(ApiResponse<decimal>.SuccessResponse(
                await _service.ConvertWeightAsync(weight, fromUnit, toUnit)));

        /// <summary>Get the most recent weight captures (for real-time display).</summary>
        [HttpGet("recent")]
        public async Task<ActionResult<ApiResponse<List<WeightCaptureResponse>>>> GetRecent(
            [FromQuery] int count = 20)
            => Ok(ApiResponse<List<WeightCaptureResponse>>.SuccessResponse(
                await _service.GetRecentCapturesAsync(TenantId, count)));

        /// <summary>Get weight capture history with optional module filter and pagination.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<WeightCaptureResponse>>>> GetHistory(
            [FromQuery] ScanModule? module,
            [FromQuery] PaginationRequest pagination)
        {
            var (items, total) = await _service.GetCaptureHistoryAsync(TenantId, module, pagination);
            return Ok(ApiResponse<List<WeightCaptureResponse>>.SuccessResponse(
                items, "Success", total, pagination.Page, pagination.PageSize));
        }
    }

    // ==================== OCR SCANNER CONTROLLER ====================
    /// <summary>
    /// OCR (Optical Character Recognition) scanning for supplier invoices and delivery notes.
    ///
    /// WORKFLOW — Supplier Invoice OCR:
    ///   1. Supplier hands over a physical invoice.
    ///   2. Operator uploads a photo/scan of the invoice.
    ///   3. OCR extracts line items (Product, Qty, Rate, Total).
    ///   4. System attempts to auto-match product names/barcodes to the database.
    ///   5. Operator reviews and corrects the parsed data.
    ///   6. A Purchase draft is created — operator saves to finalize.
    ///
    /// Supported OCR providers:
    ///   - Tesseract (open-source, runs locally)
    ///   - Azure Computer Vision
    ///   - Google Cloud Vision
    ///
    /// Configure the provider in POST /api/device-settings.
    /// </summary>
    [ApiController]
    [Route("api/ocr")]
    [Authorize]
    public class OcrScannerController : ControllerBase
    {
        private readonly IOcrScannerService _service;
        public OcrScannerController(IOcrScannerService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>
        /// Upload and process a supplier invoice or delivery note image through OCR.
        /// Returns extracted text and parsed line items.
        /// </summary>
        [HttpPost("scan")]
        public async Task<ActionResult<ApiResponse<OcrScanResponse>>> Scan(
            [FromBody] OcrScanRequest request)
            => Ok(ApiResponse<OcrScanResponse>.SuccessResponse(
                await _service.ProcessDocumentAsync(request, TenantId, UserId)));

        /// <summary>Get a previously processed OCR result by ID.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OcrScanResponse>>> GetById(int id)
        {
            var result = await _service.GetOcrResultAsync(id, TenantId);
            return result == null
                ? NotFound(ApiResponse<OcrScanResponse>.FailResponse("OCR result not found"))
                : Ok(ApiResponse<OcrScanResponse>.SuccessResponse(result));
        }

        /// <summary>
        /// Attempt to auto-match OCR-extracted product names/barcodes to existing products.
        /// Returns enriched items with MatchedProductId populated where possible.
        /// </summary>
        [HttpPost("match-products")]
        public async Task<ActionResult<ApiResponse<List<OcrLineItem>>>> MatchProducts(
            [FromBody] List<OcrLineItem> items)
            => Ok(ApiResponse<List<OcrLineItem>>.SuccessResponse(
                await _service.MatchProductsAsync(items, TenantId)));

        /// <summary>Mark an OCR result as used (linked to a purchase record).</summary>
        [HttpPatch("{id}/mark-used")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkUsed(int id, [FromQuery] int purchaseId)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _service.MarkAsUsedAsync(id, purchaseId, TenantId)));

        /// <summary>Get OCR scan history with optional document type filter.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetHistory(
            [FromQuery] OcrDocumentType? docType,
            [FromQuery] PaginationRequest pagination)
        {
            var (items, total) = await _service.GetScanHistoryAsync(TenantId, docType, pagination);
            return Ok(ApiResponse<List<object>>.SuccessResponse(
                items.Cast<object>().ToList(), "Success", total, pagination.Page, pagination.PageSize));
        }
    }

    // ==================== CARD PAYMENT CONTROLLER ====================
    /// <summary>
    /// Manages card/POS terminal payment transactions.
    ///
    /// FLOW — Card Payment:
    ///   1. Cashier clicks "Pay by Card" on the invoice.
    ///   2. POST /api/card-payment/initiate → sends amount to physical terminal.
    ///   3. Customer swipes/taps card on the terminal.
    ///   4. Terminal sends response to the software (webhook or polling).
    ///   5. PATCH /api/card-payment/{id}/status → updates payment record.
    ///   6. Invoice is marked as paid on approval.
    ///
    /// Supported providers: Pine Labs, Razorpay POS, Paytm EDC, and generic terminal APIs.
    /// Configure the provider in POST /api/device-settings.
    /// </summary>
    [ApiController]
    [Route("api/card-payment")]
    [Authorize]
    public class CardPaymentController : ControllerBase
    {
        private readonly ICardPaymentService _service;
        public CardPaymentController(ICardPaymentService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>Initiate a card payment on the connected POS terminal.</summary>
        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponse<CardPaymentTransactionResponse>>> Initiate(
            [FromBody] InitiateCardPaymentRequest request)
            => Ok(ApiResponse<CardPaymentTransactionResponse>.SuccessResponse(
                await _service.InitiatePaymentAsync(request, TenantId, UserId),
                "Card payment initiated — waiting for terminal response"));

        /// <summary>Update the status of a card payment (called when terminal sends its response).</summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<CardPaymentTransactionResponse>>> UpdateStatus(
            int id, [FromBody] UpdateCardPaymentStatusRequest request)
            => Ok(ApiResponse<CardPaymentTransactionResponse>.SuccessResponse(
                await _service.UpdatePaymentStatusAsync(id, request, TenantId)));

        /// <summary>Get the current status of a card payment transaction.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CardPaymentTransactionResponse>>> GetById(int id)
        {
            var result = await _service.GetTransactionAsync(id, TenantId);
            return result == null
                ? NotFound(ApiResponse<CardPaymentTransactionResponse>.FailResponse("Transaction not found"))
                : Ok(ApiResponse<CardPaymentTransactionResponse>.SuccessResponse(result));
        }

        /// <summary>Get all card payment transactions for a specific invoice.</summary>
        [HttpGet("by-invoice/{invoiceId}")]
        public async Task<ActionResult<ApiResponse<List<CardPaymentTransactionResponse>>>> GetByInvoice(int invoiceId)
            => Ok(ApiResponse<List<CardPaymentTransactionResponse>>.SuccessResponse(
                await _service.GetTransactionsByInvoiceAsync(invoiceId, TenantId)));

        /// <summary>Cancel a pending card payment transaction.</summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> Cancel(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _service.CancelTransactionAsync(id, TenantId), "Transaction cancelled"));
    }

    // ==================== CASH DRAWER CONTROLLER ====================
    /// <summary>
    /// Controls the automated cash drawer connected to the POS system.
    /// The cash drawer is typically wired to the receipt printer
    /// and opened via an ESC/POS command sent to the printer port.
    ///
    /// Configure the cash drawer port in POST /api/device-settings.
    /// </summary>
    [ApiController]
    [Route("api/cash-drawer")]
    [Authorize]
    public class CashDrawerController : ControllerBase
    {
        private readonly ICashDrawerService _service;
        public CashDrawerController(ICashDrawerService service) => _service = service;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        /// <summary>
        /// Send an open command to the cash drawer.
        /// The drawer opens automatically on cash payment completion.
        /// Can also be triggered manually for end-of-day cash counting.
        /// </summary>
        [HttpPost("open")]
        public async Task<ActionResult<ApiResponse<bool>>> Open([FromBody] OpenCashDrawerRequest request)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _service.OpenDrawerAsync(request, TenantId, UserId),
                "Cash drawer opened"));

        /// <summary>
        /// Query the current state of the cash drawer.
        /// Note: Not all drawer models support status reporting.
        /// Returns "Open", "Closed", or "Unknown".
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<string>>> GetStatus()
            => Ok(ApiResponse<string>.SuccessResponse(
                await _service.GetDrawerStatusAsync(TenantId)));
    }
}
