using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Sales
{
    [ApiController] [Route("api/sales")] [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly IInvoiceService _s;
        public SalesController(IInvoiceService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<InvoiceResponse>>>> GetAll([FromQuery] PaginationRequest p) { var (items, total) = await _s.GetAllInvoicesAsync(TenantId, p); return Ok(ApiResponse<List<InvoiceResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize)); }
    }

    [ApiController] [Route("api/pos")] [Authorize]
    public class POSController : ControllerBase
    {
        private readonly IPOSService _s;
        public POSController(IPOSService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpPost("bill")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> CreateBill([FromBody] POSBillRequest r) => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.CreatePOSBillAsync(r, TenantId), "Bill created"));
        [HttpGet("last-bill")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetLastBill() => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.GetLastBillAsync(TenantId)));
    }

    [ApiController] [Route("api/invoices")] [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _s;
        private readonly IEmailService _email;
        private readonly IWhatsAppService _whatsapp;
        private readonly ISmsService _sms;
        private readonly IPostBillingNotificationService _postBilling;

        public InvoiceController(
            IInvoiceService s,
            IEmailService email,
            IWhatsAppService whatsapp,
            ISmsService sms,
            IPostBillingNotificationService postBilling)
        {
            _s = s; _email = email; _whatsapp = whatsapp;
            _sms = sms; _postBilling = postBilling;
        }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet] public async Task<ActionResult<ApiResponse<List<InvoiceResponse>>>> GetAll([FromQuery] PaginationRequest p) { var (items, total) = await _s.GetAllInvoicesAsync(TenantId, p); return Ok(ApiResponse<List<InvoiceResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize)); }
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetById(int id) { var r = await _s.GetInvoiceByIdAsync(id, TenantId); return r == null ? NotFound(ApiResponse<InvoiceResponse>.FailResponse("Not found")) : Ok(ApiResponse<InvoiceResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Create([FromBody] CreateInvoiceRequest r) => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.CreateInvoiceAsync(r, TenantId), "Invoice created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Update(int id, [FromBody] UpdateInvoiceRequest r) => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.UpdateInvoiceAsync(id, r, TenantId), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteInvoiceAsync(id, TenantId), "Deleted"));
        [HttpGet("{id}/pdf")] public async Task<IActionResult> GetPdf(int id) { var pdf = await _s.GenerateInvoicePdfAsync(id, TenantId); return File(pdf, "application/pdf", $"Invoice-{id}.pdf"); }
        [HttpGet("next-number")] public async Task<ActionResult<ApiResponse<string>>> GetNextNumber() => Ok(ApiResponse<string>.SuccessResponse(await _s.GetNextInvoiceNumberAsync(TenantId)));

        // ── Per-channel invoice sending ────────────────────────────────────────────
        [HttpPost("{id}/send-email")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> SendEmail(int id, [FromBody] SendInvoiceEmailRequest r)
        { r.InvoiceId = id; return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(await _email.SendInvoiceEmailAsync(id, r.EmailAddress, r.Subject, r.Message, TenantId), "Email sent")); }

        [HttpPost("{id}/send-whatsapp")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> SendWhatsApp(int id, [FromBody] SendInvoiceWhatsAppRequest r)
        {
            var allowed = await _postBilling.IsWhatsAppEnabledAsync(TenantId);
            if (!allowed) return BadRequest(ApiResponse<MessageSendResponse>.FailResponse("WhatsApp not included in your plan. Please upgrade."));
            r.InvoiceId = id;
            return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(await _whatsapp.SendInvoiceWhatsAppAsync(id, r.MobileNumber, r.Message, TenantId), "WhatsApp sent"));
        }

        [HttpPost("{id}/send-sms")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> SendSms(int id, [FromBody] SendInvoiceSmsRequest r)
        {
            var allowed = await _postBilling.IsSmsEnabledAsync(TenantId);
            if (!allowed) return BadRequest(ApiResponse<MessageSendResponse>.FailResponse("SMS not included in your plan. Please upgrade."));
            r.InvoiceId = id;
            return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(await _sms.SendInvoiceSmsAsync(id, r.MobileNumber, r.Message, TenantId), "SMS sent"));
        }

        /// <summary>
        /// Post-billing multi-channel dispatch. Called by the UI after generating a bill;
        /// the user selects which channels to use via checkboxes (Email / WhatsApp / SMS).
        /// Returns a per-channel result; channels blocked by the plan return Status="NotAllowed".
        /// </summary>
        [HttpPost("{id}/send-all")]
        public async Task<ActionResult<ApiResponse<PostBillingNotificationResult>>> SendAll(int id, [FromBody] PostBillingSendRequest r)
        {
            r.InvoiceId = id;
            return Ok(ApiResponse<PostBillingNotificationResult>.SuccessResponse(
                await _postBilling.SendAsync(r, TenantId), "Notification dispatch complete"));
        }

        [HttpGet("{id}/delivery-status")] public async Task<ActionResult<ApiResponse<object>>> GetDeliveryStatus(int id) => Ok(ApiResponse<object>.SuccessResponse(new { InvoiceId = id, Status = "Delivered" }));
    }

    [ApiController] [Route("api/invoice-items")] [Authorize]
    public class InvoiceItemController : ControllerBase
    {
        private readonly IInvoiceService _s;
        public InvoiceItemController(IInvoiceService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet("{invoiceId}")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetByInvoice(int invoiceId) { var r = await _s.GetInvoiceByIdAsync(invoiceId, TenantId); return r == null ? NotFound(ApiResponse<InvoiceResponse>.FailResponse("Not found")) : Ok(ApiResponse<InvoiceResponse>.SuccessResponse(r)); }
    }

    [ApiController] [Route("api/sales-returns")] [Authorize]
    public class SalesReturnController : ControllerBase
    {
        private readonly ISalesReturnService _s;
        public SalesReturnController(ISalesReturnService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private static SalesReturnResponse Map(SalesReturn ret) => new()
        {
            Id = ret.Id, ReturnNumber = ret.ReturnNumber, InvoiceId = ret.InvoiceId, CustomerId = ret.CustomerId,
            Reason = ret.Reason, ReturnDate = ret.ReturnDate, TotalAmount = ret.TotalAmount, Status = ret.Status,
            Items = ret.Items.Select(i => new ReturnItemResponse { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice, TotalPrice = i.TotalPrice }).ToList()
        };
        [HttpGet] public async Task<ActionResult<ApiResponse<List<SalesReturnResponse>>>> GetAll() => Ok(ApiResponse<List<SalesReturnResponse>>.SuccessResponse((await _s.GetAllAsync(TenantId)).Select(Map).ToList()));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<SalesReturnResponse>>> GetById(int id) { var r = await _s.GetByIdAsync(id); return r == null || r.TenantId != TenantId ? NotFound(ApiResponse<SalesReturnResponse>.FailResponse("Not found")) : Ok(ApiResponse<SalesReturnResponse>.SuccessResponse(Map(r))); }
        [HttpPost] public async Task<ActionResult<ApiResponse<SalesReturnResponse>>> Create([FromBody] CreateSalesReturnRequest r) => Ok(ApiResponse<SalesReturnResponse>.SuccessResponse(Map(await _s.CreateAsync(r, TenantId)), "Created"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) { var ret = await _s.GetByIdAsync(id); if (ret == null || ret.TenantId != TenantId) return NotFound(ApiResponse<bool>.FailResponse("Not found")); return Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id), "Deleted")); }
    }

    [ApiController] [Route("api/quotations")] [Authorize]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _s;
        public QuotationController(IQuotationService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<Quotation>>>> GetAll() => Ok(ApiResponse<List<Quotation>>.SuccessResponse(await _s.GetAllAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<Quotation>>> GetById(int id) { var r = await _s.GetByIdAsync(id); return r == null ? NotFound(ApiResponse<Quotation>.FailResponse("Not found")) : Ok(ApiResponse<Quotation>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<Quotation>>> Create([FromBody] CreateQuotationRequest r) => Ok(ApiResponse<Quotation>.SuccessResponse(await _s.CreateAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<Quotation>>> Update(int id, [FromBody] UpdateQuotationRequest r) => Ok(ApiResponse<Quotation>.SuccessResponse(await _s.UpdateAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id), "Deleted"));
        [HttpPost("{id}/convert-to-invoice")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Convert(int id) => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.ConvertToInvoiceAsync(id, TenantId), "Converted"));
    }

    [ApiController] [Route("api/sales-orders")] [Authorize]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _s;
        public SalesOrderController(ISalesOrderService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<SalesOrder>>>> GetAll() => Ok(ApiResponse<List<SalesOrder>>.SuccessResponse(await _s.GetAllAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<SalesOrder>>> GetById(int id) { var r = await _s.GetByIdAsync(id); return r == null ? NotFound(ApiResponse<SalesOrder>.FailResponse("Not found")) : Ok(ApiResponse<SalesOrder>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<SalesOrder>>> Create([FromBody] CreateSalesOrderRequest r) => Ok(ApiResponse<SalesOrder>.SuccessResponse(await _s.CreateAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<SalesOrder>>> Update(int id, [FromBody] UpdateSalesOrderRequest r) => Ok(ApiResponse<SalesOrder>.SuccessResponse(await _s.UpdateAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id), "Deleted"));
        [HttpPost("{id}/convert-to-invoice")] public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Convert(int id) => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.ConvertToInvoiceAsync(id, TenantId), "Converted"));
    }
}
