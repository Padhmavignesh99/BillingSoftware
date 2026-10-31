using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Print
{
    /// <summary>
    /// Printer Registry — register, configure, and manage physical/logical printers per tenant.
    ///
    /// Supports all printer types: Thermal | Laser | Inkjet | DotMatrix | Label | Portable | Multifunction
    /// Supports all connection modes: USB | Ethernet | WiFi | Bluetooth | Shared | Cloud
    ///
    /// Architecture:
    ///   • This API manages printer configuration and print job tracking.
    ///   • Browser/PDF printing → frontend calls window.print() with the HTML from the preview endpoint.
    ///   • Thermal/ESC/POS printing → a local print agent (QZ Tray / custom service) polls /api/printer/jobs
    ///     and sends raw ESC/POS bytes to the connected printer.
    ///   • Network printers → the API can test TCP connectivity; the local agent sends the job directly.
    ///
    /// TenantId is resolved from the authenticated JWT. Requires Bearer token.
    /// </summary>
    [ApiController]
    [Route("api/printer")]
    [Authorize]
    [Produces("application/json")]
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _svc;

        public PrinterController(IPrinterService svc) => _svc = svc;

        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int? UserId
        {
            get
            {
                var v = User.FindFirst("UserId")?.Value;
                return v != null ? int.Parse(v) : null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // LOOKUP ENDPOINTS — call these first to populate printer type/connection dropdowns
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all supported printer types with descriptions and icons.
        /// Use the Value field in CreatePrinterRequest.PrinterType.
        /// </summary>
        [HttpGet("types")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        public ActionResult<ApiResponse<List<object>>> GetPrinterTypes()
            => Ok(ApiResponse<List<object>>.SuccessResponse(_svc.GetPrinterTypes()));

        /// <summary>
        /// Returns all supported connection types (USB, Ethernet, WiFi, Bluetooth, Shared, Cloud).
        /// Use the Value field in CreatePrinterRequest.ConnectionType.
        /// </summary>
        [HttpGet("connection-types")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        public ActionResult<ApiResponse<List<object>>> GetConnectionTypes()
            => Ok(ApiResponse<List<object>>.SuccessResponse(_svc.GetConnectionTypes()));

        /// <summary>
        /// Returns all supported paper sizes with dimensions.
        /// Covers standard (A4, A5, Letter), thermal (58mm, 80mm), and label sizes.
        /// </summary>
        [HttpGet("paper-sizes")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        public ActionResult<ApiResponse<List<object>>> GetPaperSizes()
            => Ok(ApiResponse<List<object>>.SuccessResponse(_svc.GetSupportedPaperSizes()));

        // ─────────────────────────────────────────────────────────────────────────
        // PRINTER CRUD
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lists all printers registered for this tenant.
        /// Optional filters: branchId, counterId, printerType, activeOnly.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PrinterResponse>>), 200)]
        public async Task<ActionResult<ApiResponse<List<PrinterResponse>>>> GetAll(
            [FromQuery] int?    branchId    = null,
            [FromQuery] int?    counterId   = null,
            [FromQuery] bool?   activeOnly  = null,
            [FromQuery] string? printerType = null)
        {
            var result = await _svc.GetAllAsync(TenantId, branchId, counterId, activeOnly, printerType);
            return Ok(ApiResponse<List<PrinterResponse>>.SuccessResponse(result));
        }

        /// <summary>Returns a single printer by its ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PrinterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrinterResponse>>> GetById([FromRoute] int id)
        {
            var result = await _svc.GetByIdAsync(id, TenantId);
            if (result == null)
                return NotFound(ApiResponse<object>.FailResponse($"Printer #{id} not found"));
            return Ok(ApiResponse<PrinterResponse>.SuccessResponse(result));
        }

        /// <summary>
        /// Registers a new printer for this tenant.
        /// Set IsDefault = true to make it the default printer for its type and scope.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PrinterResponse>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<PrinterResponse>>> Create(
            [FromBody] CreatePrinterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));

            var result = await _svc.CreateAsync(TenantId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<PrinterResponse>.SuccessResponse(result, "Printer registered successfully"));
        }

        /// <summary>
        /// Fully replaces a printer registration (PUT semantics).
        /// All fields are overwritten with the values in the request body.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PrinterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrinterResponse>>> Update(
            [FromRoute] int id,
            [FromBody] CreatePrinterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload"));
            try
            {
                var result = await _svc.UpdateAsync(id, TenantId, request);
                return Ok(ApiResponse<PrinterResponse>.SuccessResponse(result, "Printer updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Partially updates a printer — only the fields included in the body are changed.
        /// All other fields remain untouched.
        /// </summary>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PrinterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrinterResponse>>> Patch(
            [FromRoute] int id,
            [FromBody] PatchPrinterRequest request)
        {
            try
            {
                var result = await _svc.PatchAsync(id, TenantId, request);
                return Ok(ApiResponse<PrinterResponse>.SuccessResponse(result, "Printer updated"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Removes a printer registration.
        /// ⚠️ Does NOT delete completed print job history — those remain in the audit log.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int id)
        {
            var ok = await _svc.DeleteAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Printer #{id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Printer removed successfully"));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // DEFAULT MANAGEMENT
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets this printer as the default for its type and scope.
        /// Any existing default of the same type+scope is automatically cleared.
        /// </summary>
        [HttpPost("{id:int}/set-default")]
        [ProducesResponseType(typeof(ApiResponse<PrinterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrinterResponse>>> SetDefault([FromRoute] int id)
        {
            try
            {
                var result = await _svc.SetDefaultAsync(id, TenantId);
                return Ok(ApiResponse<PrinterResponse>.SuccessResponse(result, "Printer set as default"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // CONNECTIVITY TEST
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tests the connection to a registered printer by its ID.
        /// Network (Ethernet/WiFi) printers: attempts a TCP socket on the configured IP:Port.
        /// USB/Bluetooth/Shared: validates configuration only.
        /// Returns latency in milliseconds for network printers.
        /// </summary>
        [HttpPost("{id:int}/test")]
        [ProducesResponseType(typeof(ApiResponse<PrinterTestResultResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<PrinterTestResultResponse>>> Test([FromRoute] int id)
        {
            try
            {
                var result = await _svc.TestConnectionAsync(id, TenantId);
                var msg = result.Success ? "Printer is reachable" : "Printer connectivity test failed";
                return Ok(ApiResponse<PrinterTestResultResponse>.SuccessResponse(result, msg));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailResponse(ex.Message));
            }
        }

        /// <summary>
        /// Tests a printer address without a registered printer.
        /// Useful for testing before saving a printer configuration.
        /// Body: { "connectionType": "Ethernet", "address": "192.168.1.100", "port": 9100 }
        /// </summary>
        [HttpPost("test-address")]
        [ProducesResponseType(typeof(ApiResponse<PrinterTestResultResponse>), 200)]
        public async Task<ActionResult<ApiResponse<PrinterTestResultResponse>>> TestAddress(
            [FromBody] PrinterAddressTestRequest request)
        {
            var result = await _svc.TestConnectionByAddressAsync(TenantId, request.ConnectionType, request.Address, request.Port);
            return Ok(ApiResponse<PrinterTestResultResponse>.SuccessResponse(result,
                result.Success ? "Connection test passed" : "Connection test failed"));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // BILL PRINTING
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Queues a print job for an invoice/bill.
        /// Automatically selects the default printer if PrinterId is not specified.
        /// Returns:
        ///   - PrintMethod = "Browser" → use HtmlContent with window.print() on the client
        ///   - PrintMethod = "ESC/POS" → local print agent should pick up the job via GET /api/printer/jobs?status=Queued
        ///   - PrintMethod = "PDF" → client generates PDF from the HTML using the browser's print dialog
        /// </summary>
        [HttpPost("bill/{invoiceId:int}")]
        [ProducesResponseType(typeof(ApiResponse<BillPrintResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<BillPrintResponse>>> PrintBill(
            [FromRoute] int invoiceId,
            [FromBody] BillPrintRequest? request)
        {
            request ??= new BillPrintRequest();
            var result = await _svc.PrintBillAsync(invoiceId, TenantId, request, UserId);
            return Ok(ApiResponse<BillPrintResponse>.SuccessResponse(result, result.Message));
        }

        /// <summary>
        /// Creates a reprint of a previously printed job.
        /// The new job is marked IsReprint=true and linked to the original for full audit trail.
        /// </summary>
        [HttpPost("reprint/{printJobId:int}")]
        [ProducesResponseType(typeof(ApiResponse<BillPrintResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<BillPrintResponse>>> Reprint([FromRoute] int printJobId)
        {
            try
            {
                var result = await _svc.ReprintAsync(printJobId, TenantId, UserId);
                return Ok(ApiResponse<BillPrintResponse>.SuccessResponse(result, result.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailResponse(ex.Message));
            }
        }
    }

    /// <summary>Request body for testing a printer address without saving.</summary>
    public class PrinterAddressTestRequest
    {
        public required string ConnectionType { get; set; }
        public string? Address                { get; set; }
        public int?    Port                   { get; set; }
    }
}
