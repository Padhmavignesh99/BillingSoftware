using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Sales
{
    // ── Payments — create, list, due, collect, daily summary ─────────────────────
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    [Tags("Payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentsController(IFullPaymentService svc) { _svc = svc; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>List all payments (paginated).</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> GetAll([FromQuery] PaginationRequest p)
        {
            var (items, total) = await _svc.GetPaymentsAsync(TenantId, p);
            return Ok(ApiResponse<List<PaymentResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize));
        }

        /// <summary>Get a single payment by ID.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(int id)
        {
            var r = await _svc.GetPaymentAsync(id, TenantId);
            return r == null
                ? NotFound(ApiResponse<PaymentResponse>.FailResponse("Not found"))
                : Ok(ApiResponse<PaymentResponse>.SuccessResponse(r));
        }

        /// <summary>
        /// Create a payment for an invoice. Supports split payment (multiple methods).
        /// For cash: set CashReceived; change is calculated automatically.
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Create([FromBody] CreateFullPaymentRequest req)
            => Ok(ApiResponse<PaymentResponse>.SuccessResponse(
                await _svc.CreatePaymentAsync(req, TenantId), "Payment recorded"));

        /// <summary>List all invoices with an outstanding due amount.</summary>
        [HttpGet("due")]
        public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> GetDue()
            => Ok(ApiResponse<List<PaymentResponse>>.SuccessResponse(await _svc.GetDueListAsync(TenantId)));

        /// <summary>Collect payment on a due invoice (single method shorthand).</summary>
        [HttpPost("collect-due")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> CollectDue([FromBody] CollectDueRequest req)
            => Ok(ApiResponse<PaymentResponse>.SuccessResponse(
                await _svc.CollectDueAsync(req, TenantId), "Payment collected"));

        /// <summary>Daily collection summary split by payment method.</summary>
        [HttpGet("daily-summary")]
        public async Task<ActionResult<ApiResponse<PaymentSummaryResponse>>> DailySummary(
            [FromQuery] DateTime? date, [FromQuery] int? branchId)
            => Ok(ApiResponse<PaymentSummaryResponse>.SuccessResponse(
                await _svc.GetDailySummaryAsync(TenantId, date, branchId)));
    }

    // ── POS Terminals ─────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/payment-terminals")]
    [Authorize]
    [Tags("Payment Terminals")]
    public class PaymentTerminalsController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentTerminalsController(IFullPaymentService svc) { _svc = svc; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>List all registered POS terminals.</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PaymentTerminalResponse>>>> GetAll()
            => Ok(ApiResponse<List<PaymentTerminalResponse>>.SuccessResponse(await _svc.GetTerminalsAsync(TenantId)));

        /// <summary>Register a new POS terminal (Pine Labs, Paytm EDC, etc.).</summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<PaymentTerminalResponse>>> Register([FromBody] RegisterTerminalRequest req)
            => Ok(ApiResponse<PaymentTerminalResponse>.SuccessResponse(
                await _svc.RegisterTerminalAsync(req, TenantId), "Terminal registered"));

        /// <summary>Update terminal settings or status.</summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentTerminalResponse>>> Update(int id, [FromBody] UpdateTerminalRequest req)
            => Ok(ApiResponse<PaymentTerminalResponse>.SuccessResponse(
                await _svc.UpdateTerminalAsync(id, req, TenantId), "Updated"));

        /// <summary>Test connectivity to a registered POS terminal.</summary>
        [HttpPost("{id}/test-connection")]
        public async Task<ActionResult<ApiResponse<TerminalConnectionTestResponse>>> TestConnection(int id)
            => Ok(ApiResponse<TerminalConnectionTestResponse>.SuccessResponse(
                await _svc.TestTerminalConnectionAsync(id, TenantId)));
    }

    // ── Terminal Transactions ─────────────────────────────────────────────────────
    [ApiController]
    [Route("api/payment-transactions")]
    [Authorize]
    [Tags("Payment Transactions")]
    public class PaymentTransactionsController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentTransactionsController(IFullPaymentService svc) { _svc = svc; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Send a payment amount to a connected POS terminal to initiate card/UPI transaction.</summary>
        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionResponse>>> Initiate([FromBody] InitiateTerminalPaymentRequest req)
            => Ok(ApiResponse<PaymentTransactionResponse>.SuccessResponse(
                await _svc.InitiateTerminalTransactionAsync(req, TenantId), "Transaction initiated — waiting for card"));

        /// <summary>Poll the status of an in-progress terminal transaction.</summary>
        [HttpGet("status/{id}")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionResponse>>> Status(int id)
        {
            var r = await _svc.GetTransactionStatusAsync(id, TenantId);
            return r == null
                ? NotFound(ApiResponse<PaymentTransactionResponse>.FailResponse("Transaction not found"))
                : Ok(ApiResponse<PaymentTransactionResponse>.SuccessResponse(r));
        }

        /// <summary>Cancel an in-progress terminal transaction.</summary>
        [HttpPost("cancel/{id}")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionResponse>>> Cancel(int id)
            => Ok(ApiResponse<PaymentTransactionResponse>.SuccessResponse(
                await _svc.CancelTransactionAsync(id, TenantId), "Cancelled"));
    }

    // ── Refunds ───────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/payment-refunds")]
    [Authorize]
    [Tags("Refunds")]
    public class PaymentRefundsController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentRefundsController(IFullPaymentService svc) { _svc = svc; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Create a full or partial refund.</summary>
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<RefundResponse>>> Create([FromBody] CreateRefundRequest req)
            => Ok(ApiResponse<RefundResponse>.SuccessResponse(
                await _svc.CreateRefundAsync(req, TenantId), "Refund created"));

        /// <summary>Get refunds for a specific payment.</summary>
        [HttpGet("{paymentId}")]
        public async Task<ActionResult<ApiResponse<List<RefundResponse>>>> GetByPayment(int paymentId)
            => Ok(ApiResponse<List<RefundResponse>>.SuccessResponse(
                await _svc.GetRefundsAsync(paymentId, TenantId)));

        /// <summary>Get all refunds for this tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RefundResponse>>>> GetAll()
            => Ok(ApiResponse<List<RefundResponse>>.SuccessResponse(
                await _svc.GetRefundsAsync(null, TenantId)));
    }

    // ── Payment Webhooks — provider callbacks ─────────────────────────────────────
    [ApiController]
    [Route("api/payment-webhooks")]
    [Tags("Payment Webhooks")]
    public class PaymentWebhooksController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentWebhooksController(IFullPaymentService svc) { _svc = svc; }

        /// <summary>
        /// Receive payment result callbacks from Pine Labs, Paytm, Razorpay, etc.
        /// No JWT required — provider calls this URL directly.
        /// </summary>
        [HttpPost("{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook(string provider, [FromBody] PaymentWebhookRequest req)
        {
            req.Provider = provider;
            await _svc.ProcessWebhookAsync(provider, req);
            return Ok(new { received = true, provider });
        }
    }

    // ── Payment Configuration — per-shop method enable/disable ────────────────────
    [ApiController]
    [Route("api/payment-config")]
    [Authorize]
    [Tags("Payment Configuration")]
    public class PaymentConfigController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentConfigController(IFullPaymentService svc) { _svc = svc; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Get enabled payment methods for this shop (optional branch filter).</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PaymentMethodConfigResponse>>>> Get([FromQuery] int? branchId)
            => Ok(ApiResponse<List<PaymentMethodConfigResponse>>.SuccessResponse(
                await _svc.GetPaymentConfigAsync(TenantId, branchId)));

        /// <summary>Update which payment methods are enabled and their display order.</summary>
        [HttpPut]
        public async Task<ActionResult<ApiResponse<bool>>> Update(
            [FromBody] UpdatePaymentConfigRequest req, [FromQuery] int? branchId)
        {
            await _svc.UpdatePaymentConfigAsync(req, TenantId, branchId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Payment configuration updated"));
        }
    }

    // ── Payment Providers ─────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/payment-providers")]
    [Authorize]
    [Tags("Payment Providers")]
    public class PaymentProvidersController : ControllerBase
    {
        private readonly IFullPaymentService _svc;
        public PaymentProvidersController(IFullPaymentService svc) { _svc = svc; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>List registered payment providers (Pine Labs, Paytm, etc.).</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PaymentProviderResponse>>>> Get()
            => Ok(ApiResponse<List<PaymentProviderResponse>>.SuccessResponse(
                await _svc.GetProvidersAsync(TenantId)));
    }
}
