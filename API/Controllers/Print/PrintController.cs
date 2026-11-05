using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Print
{
    /// <summary>
    /// Manage per-tenant print settings for invoices, receipts, thermal POS receipts,
    /// product labels, reports, printer lookup data, print jobs and invoice print data.
    ///
    /// All endpoints require a valid JWT Bearer token.
    /// TenantId is resolved automatically from the authenticated user's JWT claims.
    /// Settings follow a 3-level hierarchy: Tenant → Branch → Counter.
    /// </summary>
    [ApiController]
    [Route("api/print-settings")]
    [Authorize]
    [Produces("application/json")]
    public class PrintController : ControllerBase
    {
        private readonly IPrintSettingsService _svc;

        public PrintController(IPrintSettingsService svc) => _svc = svc;

        /// <summary>Resolves TenantId from the authenticated JWT claim.</summary>
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Resolves optional UserId from JWT (used for audit on print jobs).</summary>
        private int? UserId
        {
            get
            {
                var v = User.FindFirst("UserId")?.Value;
                return v != null ? int.Parse(v) : null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // SETTINGS CRUD (tenant / branch / counter scoped)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the effective print settings for the authenticated tenant.
        /// Pass optional branchId and/or counterId to get the most specific settings
        /// for that scope (falls back through branch → tenant if not found).
        /// If no settings are stored, system defaults are returned.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> Get(
            [FromQuery] int? branchId = null,
            [FromQuery] int? counterId = null)
        {
            var result = await _svc.GetPrintSettingsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns all scoped print settings stored for this tenant
        /// (tenant-level + all branch-level + all counter-level).
        /// Useful for an admin overview page.
        /// </summary>
        [HttpGet("all-scopes")]
        [ProducesResponseType(typeof(ApiResponse<List<PrintSettingsResponse>>), 200)]
        public async Task<ActionResult<ApiResponse<List<PrintSettingsResponse>>>> GetAllScopes()
        {
            var result = await _svc.GetAllScopedSettingsAsync(TenantId);
            return Ok(ApiResponse<List<PrintSettingsResponse>>.SuccessResponse(result));
        }

        /// <summary>
        /// Creates or fully replaces the print settings for the specified scope.
        /// Use branchId / counterId query params (or include them in the body) to target
        /// a branch or counter override. Omit both for the tenant-level default.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> Update(
            [FromBody] UpdatePrintSettingsRequest request,
            [FromQuery] int? branchId = null,
            [FromQuery] int? counterId = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.UpdatePrintSettingsAsync(TenantId, request, branchId, counterId);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result, "Print settings updated successfully"));
        }

        /// <summary>
        /// Partially updates print settings — only the fields included in the request body
        /// are changed; all other settings remain unchanged.
        /// Supports branch/counter scope via query params or request body.
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> Patch(
            [FromBody] PatchPrintSettingsRequest request,
            [FromQuery] int? branchId = null,
            [FromQuery] int? counterId = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.PatchPrintSettingsAsync(TenantId, request, branchId, counterId);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result, "Print settings patched successfully"));
        }

        /// <summary>
        /// Resets print settings back to system defaults for the specified scope.
        /// ⚠️ This permanently overwrites any existing customisations for that scope.
        /// </summary>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> ResetToDefaults(
            [FromQuery] int? branchId = null,
            [FromQuery] int? counterId = null)
        {
            var result = await _svc.ResetToDefaultsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result, "Print settings reset to system defaults"));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // LOOKUP ENDPOINTS
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all supported paper sizes with dimensions and type metadata.
        /// Use the <c>Value</c> field as the <c>PaperSize</c> in settings requests.
        /// </summary>
        [HttpGet("paper-sizes")]
        [ProducesResponseType(typeof(ApiResponse<List<PaperSizeInfo>>), 200)]
        public ActionResult<ApiResponse<List<PaperSizeInfo>>> GetPaperSizes()
        {
            var result = _svc.GetPaperSizes();
            return Ok(ApiResponse<List<PaperSizeInfo>>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns all supported printer types with capability metadata.
        /// Use the <c>Value</c> field as the printer type in settings requests.
        /// </summary>
        [HttpGet("printer-types")]
        [ProducesResponseType(typeof(ApiResponse<List<PrinterTypeInfo>>), 200)]
        public ActionResult<ApiResponse<List<PrinterTypeInfo>>> GetPrinterTypes()
        {
            var result = _svc.GetPrinterTypes();
            return Ok(ApiResponse<List<PrinterTypeInfo>>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns all supported printer connection types (USB, Network, Bluetooth, Cloud).
        /// </summary>
        [HttpGet("connection-types")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public ActionResult<ApiResponse<List<string>>> GetConnectionTypes()
        {
            var result = _svc.GetConnectionTypes();
            return Ok(ApiResponse<List<string>>.SuccessResponse(result));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // PRINTER TEST
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tests connectivity to a printer.
        /// For Network printers: attempts a TCP connection to the supplied IP and port.
        /// For USB / Bluetooth / Cloud: validates configuration only.
        /// Returns latency in milliseconds for network printers.
        /// </summary>
        [HttpPost("test")]
        [ProducesResponseType(typeof(ApiResponse<PrinterTestResult>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrinterTestResult>>> TestPrinter(
            [FromBody] PrinterTestRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.TestPrinterAsync(TenantId, request);
            var msg    = result.Success ? "Printer is reachable" : "Printer connectivity test failed";
            return Ok(ApiResponse<PrinterTestResult>.SuccessResponse(result, msg));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // PRINT PREVIEW
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a rendered HTML preview of a sample document using the effective
        /// print settings for the given scope. Open the response directly in a browser.
        /// Supports: invoice | quotation | receipt | report.
        /// </summary>
        [HttpGet("preview/{documentType}")]
        [Produces("text/html")]
        [ProducesResponseType(typeof(string), 200, "text/html")]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Preview(
            [FromRoute] string documentType,
            [FromQuery] int? branchId  = null,
            [FromQuery] int? counterId = null)
        {
            var allowed = new[] { "invoice", "quotation", "receipt", "report" };
            if (!allowed.Contains(documentType.ToLowerInvariant()))
                return BadRequest(ApiResponse<object>.FailResponse(
                    $"Invalid documentType. Allowed values: {string.Join(", ", allowed)}"));

            var html = await _svc.GetPrintPreviewHtmlAsync(TenantId, documentType, branchId, counterId);
            return Content(html, "text/html");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // SECTION READS (lightweight partial reads)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Returns only the invoice-specific print settings section.</summary>
        [HttpGet("section/invoice")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetInvoiceSection(
            [FromQuery] int? branchId = null, [FromQuery] int? counterId = null)
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                s.PaperSize, s.Orientation, s.MarginTopMm, s.MarginBottomMm, s.MarginLeftMm, s.MarginRightMm,
                s.FontFamily, s.FontSizePt, s.HeaderFontSizePt,
                s.ShowBusinessLogo, s.ShowBusinessAddress, s.ShowGSTDetails,
                s.ShowCustomerAddress, s.ShowItemHSNCode, s.ShowItemDiscount,
                s.ShowTaxBreakdown, s.ShowRoundOff, s.ShowAmountInWords,
                s.ShowTermsAndConditions, s.ShowFooterNote, s.ShowSignatureLine,
                s.ShowQRCode, s.ShowBarcode, s.ShowPaymentDetails, s.ShowDueAmount,
                s.ShowBankDetails, s.BankDetails, s.UpiId,
                s.InvoiceCopies, s.InvoiceCopyLabel1, s.InvoiceCopyLabel2, s.InvoiceCopyLabel3,
                s.PrintPrimaryColor, s.PrintInColor, s.DefaultPrinterName,
                s.FooterText, s.ScopeLabel
            }));
        }

        /// <summary>Returns only the thermal / POS receipt section of the print settings.</summary>
        [HttpGet("section/thermal")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetThermalSection(
            [FromQuery] int? branchId = null, [FromQuery] int? counterId = null)
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                s.ThermalPrintEnabled, s.ThermalPaperWidth,
                s.ThermalShowLogo, s.ThermalShowQRCode, s.ThermalShowItemCode,
                s.ThermalShowCustomerName, s.ThermalAutocut, s.ThermalLineFeedCount,
                s.ReceiptCopies, s.DefaultThermalPrinter,
                s.PrinterIpAddress, s.PrinterPort, s.PrinterConnection,
                s.OpenCashDrawer, s.FooterText, s.ScopeLabel
            }));
        }

        /// <summary>Returns only the product-label print section of the print settings.</summary>
        [HttpGet("section/label")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetLabelSection(
            [FromQuery] int? branchId = null, [FromQuery] int? counterId = null)
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                s.LabelPrintEnabled, s.LabelPaperSize,
                s.LabelShowBarcode, s.LabelShowMRP, s.LabelShowProductName, s.LabelShowBusiness,
                s.LabelsPerRow, s.DefaultLabelPrinter, s.ScopeLabel
            }));
        }

        /// <summary>Returns only the report print section of the print settings.</summary>
        [HttpGet("section/report")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetReportSection(
            [FromQuery] int? branchId = null, [FromQuery] int? counterId = null)
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                s.PaperSize, s.Orientation, s.MarginTopMm, s.MarginBottomMm, s.MarginLeftMm, s.MarginRightMm,
                s.ShowReportHeader, s.ShowReportFooter, s.ShowPageNumbers,
                s.ShowPrintDateTime, s.ShowReportLogo,
                s.FontFamily, s.FontSizePt, s.PrintPrimaryColor, s.DefaultPrinterName, s.ScopeLabel
            }));
        }

        /// <summary>Returns the printer / connection settings section.</summary>
        [HttpGet("section/printer")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetPrinterSection(
            [FromQuery] int? branchId = null, [FromQuery] int? counterId = null)
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId, branchId, counterId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                s.DefaultPrinterName, s.DefaultThermalPrinter, s.DefaultLabelPrinter,
                s.PrinterIpAddress, s.PrinterPort, s.PrinterConnection,
                s.OpenCashDrawer, s.ScopeLabel
            }));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // PRINT JOB MANAGEMENT
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Queues a new print job for a document.
        /// The job picks up the effective print settings (counter → branch → tenant).
        /// Returns the queued job record with its ID and initial "Queued" status.
        /// </summary>
        [HttpPost("jobs")]
        [ProducesResponseType(typeof(ApiResponse<PrintJobResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintJobResponse>>> QueuePrintJob(
            [FromBody] QueuePrintJobRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.QueuePrintJobAsync(TenantId, request);
            return Ok(ApiResponse<PrintJobResponse>.SuccessResponse(result, "Print job queued"));
        }

        /// <summary>
        /// Lists print jobs for the authenticated tenant.
        /// Filter by status (Queued | Printing | Printed | Failed | Cancelled) and/or documentType.
        /// Supports pagination via page and pageSize query params.
        /// </summary>
        [HttpGet("jobs")]
        [ProducesResponseType(typeof(ApiResponse<List<PrintJobResponse>>), 200)]
        public async Task<ActionResult<ApiResponse<List<PrintJobResponse>>>> GetPrintJobs(
            [FromQuery] string? status       = null,
            [FromQuery] string? documentType = null,
            [FromQuery] int     page         = 1,
            [FromQuery] int     pageSize     = 20)
        {
            var result = await _svc.GetPrintJobsAsync(TenantId, status, documentType, page, pageSize);
            return Ok(ApiResponse<List<PrintJobResponse>>.SuccessResponse(result));
        }

        /// <summary>Returns a single print job by its ID.</summary>
        [HttpGet("jobs/{jobId:int}")]
        [ProducesResponseType(typeof(ApiResponse<PrintJobResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrintJobResponse>>> GetPrintJob([FromRoute] int jobId)
        {
            var result = await _svc.GetPrintJobByIdAsync(jobId, TenantId);
            if (result == null)
                return NotFound(ApiResponse<object>.FailResponse($"Print job #{jobId} not found"));
            return Ok(ApiResponse<PrintJobResponse>.SuccessResponse(result));
        }

        /// <summary>
        /// Updates the status of a print job.
        /// Accepted statuses: Queued | Printing | Printed | Failed | Cancelled.
        /// </summary>
        [HttpPatch("jobs/{jobId:int}/status")]
        [ProducesResponseType(typeof(ApiResponse<PrintJobResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintJobResponse>>> UpdateJobStatus(
            [FromRoute] int    jobId,
            [FromQuery] string status,
            [FromQuery] string? errorMessage = null)
        {
            var allowed = new[] { "Queued", "Printing", "Printed", "Failed", "Cancelled" };
            if (!allowed.Contains(status))
                return BadRequest(ApiResponse<object>.FailResponse(
                    $"Invalid status. Allowed: {string.Join(", ", allowed)}"));

            var result = await _svc.UpdatePrintJobStatusAsync(jobId, TenantId, status, errorMessage);
            return Ok(ApiResponse<PrintJobResponse>.SuccessResponse(result, $"Job status updated to '{status}'"));
        }

        /// <summary>
        /// Retries a Failed or Cancelled print job.
        /// The retry counter is incremented; throws if the retry limit has been reached.
        /// </summary>
        [HttpPost("jobs/{jobId:int}/retry")]
        [ProducesResponseType(typeof(ApiResponse<PrintJobResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintJobResponse>>> RetryJob([FromRoute] int jobId)
        {
            var result = await _svc.RetryPrintJobAsync(jobId, TenantId);
            return Ok(ApiResponse<PrintJobResponse>.SuccessResponse(result, "Print job re-queued for retry"));
        }

        /// <summary>
        /// Creates a reprint of a previously printed job.
        /// Marks the new job with <c>IsReprint = true</c> and links it to the original job.
        /// </summary>
        [HttpPost("jobs/{jobId:int}/reprint")]
        [ProducesResponseType(typeof(ApiResponse<PrintJobResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrintJobResponse>>> Reprint([FromRoute] int jobId)
        {
            var result = await _svc.ReprintAsync(jobId, TenantId, UserId);
            return Ok(ApiResponse<PrintJobResponse>.SuccessResponse(result, "Reprint job queued"));
        }

        /// <summary>
        /// Cancels a Queued or Printing job.
        /// Already-Printed jobs cannot be cancelled — use Reprint instead.
        /// </summary>
        [HttpDelete("jobs/{jobId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<object>>> CancelJob([FromRoute] int jobId)
        {
            await _svc.CancelPrintJobAsync(jobId, TenantId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Print job cancelled"));
        }
    }
}
