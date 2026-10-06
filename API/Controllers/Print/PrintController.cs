using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Print
{
    /// <summary>
    /// Manage per-tenant print settings for invoices, receipts, thermal POS receipts,
    /// product labels, and reports.
    ///
    /// All endpoints require a valid JWT Bearer token.
    /// TenantId is resolved automatically from the authenticated user's claims.
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

        // ── GET api/print-settings ─────────────────────────────────────────────
        /// <summary>
        /// Returns the current print settings for the authenticated tenant.
        /// If the tenant has not yet saved any custom settings, system defaults are returned
        /// (the response is not persisted until the first PUT/PATCH call).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> Get()
        {
            var result = await _svc.GetPrintSettingsAsync(TenantId);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result));
        }

        // ── PUT api/print-settings ─────────────────────────────────────────────
        /// <summary>
        /// Creates or fully replaces the print settings for the tenant.
        /// Any field not supplied in the body will be set to its system default.
        /// Use PATCH if you only want to update specific fields.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> Update(
            [FromBody] UpdatePrintSettingsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.UpdatePrintSettingsAsync(TenantId, request);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result, "Print settings updated successfully"));
        }

        // ── PATCH api/print-settings ───────────────────────────────────────────
        /// <summary>
        /// Partially updates print settings — only the fields included in the request body
        /// are changed; all other settings remain unchanged.
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> Patch(
            [FromBody] PatchPrintSettingsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.PatchPrintSettingsAsync(TenantId, request);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result, "Print settings patched successfully"));
        }

        // ── POST api/print-settings/reset ──────────────────────────────────────
        /// <summary>
        /// Resets all print settings back to system defaults for the authenticated tenant.
        /// ⚠️ This permanently overwrites any existing customizations.
        /// </summary>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(ApiResponse<PrintSettingsResponse>), 200)]
        public async Task<ActionResult<ApiResponse<PrintSettingsResponse>>> ResetToDefaults()
        {
            var result = await _svc.ResetToDefaultsAsync(TenantId);
            return Ok(ApiResponse<PrintSettingsResponse>.SuccessResponse(result, "Print settings reset to system defaults"));
        }

        // ── GET api/print-settings/preview/{documentType} ─────────────────────
        /// <summary>
        /// Returns a rendered HTML preview page that shows how a sample document will look
        /// with the current print settings applied.
        ///
        /// <b>documentType</b>: invoice | quotation | receipt | report
        ///
        /// Open the returned URL directly in a browser to see a pixel-accurate print preview.
        /// The response Content-Type is text/html.
        /// </summary>
        [HttpGet("preview/{documentType}")]
        [Produces("text/html")]
        [ProducesResponseType(typeof(string), 200, "text/html")]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Preview(
            [FromRoute] string documentType)
        {
            var allowed = new[] { "invoice", "quotation", "receipt", "report" };
            if (!allowed.Contains(documentType.ToLowerInvariant()))
                return BadRequest(ApiResponse<object>.FailResponse(
                    $"Invalid documentType. Allowed values: {string.Join(", ", allowed)}"));

            var html = await _svc.GetPrintPreviewHtmlAsync(TenantId, documentType);
            return Content(html, "text/html");
        }

        // ── GET api/print-settings/section/invoice ─────────────────────────────
        /// <summary>
        /// Returns only the Invoice-specific section of the print settings
        /// (paper, margins, font, invoice display options, copies, copy labels).
        /// Useful for lightweight reads by front-end invoice editors.
        /// </summary>
        [HttpGet("section/invoice")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetInvoiceSection()
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId);
            var section = new
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
                s.PrintPrimaryColor, s.PrintInColor, s.DefaultPrinterName
            };
            return Ok(ApiResponse<object>.SuccessResponse(section));
        }

        // ── GET api/print-settings/section/thermal ─────────────────────────────
        /// <summary>
        /// Returns only the Thermal / POS receipt section of the print settings.
        /// </summary>
        [HttpGet("section/thermal")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetThermalSection()
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId);
            var section = new
            {
                s.ThermalPrintEnabled, s.ThermalPaperWidth,
                s.ThermalShowLogo, s.ThermalShowQRCode, s.ThermalShowItemCode,
                s.ThermalShowCustomerName, s.ThermalAutocut, s.ThermalLineFeedCount,
                s.ReceiptCopies, s.DefaultThermalPrinter
            };
            return Ok(ApiResponse<object>.SuccessResponse(section));
        }

        // ── GET api/print-settings/section/label ───────────────────────────────
        /// <summary>
        /// Returns only the product-label print section of the print settings.
        /// </summary>
        [HttpGet("section/label")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetLabelSection()
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId);
            var section = new
            {
                s.LabelPrintEnabled, s.LabelPaperSize,
                s.LabelShowBarcode, s.LabelShowMRP, s.LabelShowProductName, s.LabelShowBusiness,
                s.LabelsPerRow, s.DefaultLabelPrinter
            };
            return Ok(ApiResponse<object>.SuccessResponse(section));
        }

        // ── GET api/print-settings/section/report ──────────────────────────────
        /// <summary>
        /// Returns only the report print section of the print settings.
        /// </summary>
        [HttpGet("section/report")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetReportSection()
        {
            var s = await _svc.GetPrintSettingsAsync(TenantId);
            var section = new
            {
                s.PaperSize, s.Orientation, s.MarginTopMm, s.MarginBottomMm, s.MarginLeftMm, s.MarginRightMm,
                s.ShowReportHeader, s.ShowReportFooter, s.ShowPageNumbers,
                s.ShowPrintDateTime, s.ShowReportLogo,
                s.FontFamily, s.FontSizePt, s.PrintPrimaryColor, s.DefaultPrinterName
            };
            return Ok(ApiResponse<object>.SuccessResponse(section));
        }
    }
}
