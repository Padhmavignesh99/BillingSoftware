using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Business
{
    /// <summary>
    /// Report API — provides all business reports for the Master Billing.
    /// All endpoints are scoped to the authenticated tenant.
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _report;
        public ReportController(IReportService report) { _report = report; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int? UserId { get { var c = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; return int.TryParse(c, out var id) ? id : null; } }

        [HttpGet("sales")] public async Task<ActionResult<ApiResponse<SalesSummaryResponse>>> GetSalesReport([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<SalesSummaryResponse>.SuccessResponse(await _report.GetSalesSummaryAsync(TenantId, filter)));
        [HttpGet("sales-return")] public async Task<ActionResult<ApiResponse<SalesReturnReportResponse>>> GetSalesReturnReport([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<SalesReturnReportResponse>.SuccessResponse(await _report.GetSalesReturnReportAsync(TenantId, filter)));
        [HttpGet("purchase")] public async Task<ActionResult<ApiResponse<PurchaseReportResponse>>> GetPurchaseReport([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<PurchaseReportResponse>.SuccessResponse(await _report.GetPurchaseReportAsync(TenantId, filter)));
        [HttpGet("purchase-return")] public async Task<ActionResult<ApiResponse<PurchaseReturnReportResponse>>> GetPurchaseReturnReport([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<PurchaseReturnReportResponse>.SuccessResponse(await _report.GetPurchaseReturnReportAsync(TenantId, filter)));
        [HttpGet("low-stock")] public async Task<ActionResult<ApiResponse<LowStockReportResponse>>> GetLowStockReport() => Ok(ApiResponse<LowStockReportResponse>.SuccessResponse(await _report.GetLowStockReportAsync(TenantId)));
        [HttpGet("credit-bill")] public async Task<ActionResult<ApiResponse<CreditBillReportResponse>>> GetCreditBillReport([FromQuery] int? userId = null) => Ok(ApiResponse<CreditBillReportResponse>.SuccessResponse(await _report.GetCreditBillReportAsync(TenantId, userId)));
        [HttpGet("credit-bill/my")] public async Task<ActionResult<ApiResponse<CreditBillReportResponse>>> GetMyCreditBillReport() => Ok(ApiResponse<CreditBillReportResponse>.SuccessResponse(await _report.GetCreditBillReportAsync(TenantId, UserId)));
        [HttpGet("product-mini-statement/{productId:int}")] public async Task<ActionResult<ApiResponse<ProductMiniStatementResponse>>> GetProductMiniStatement(int productId, [FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<ProductMiniStatementResponse>.SuccessResponse(await _report.GetProductMiniStatementAsync(TenantId, productId, filter)));
        [HttpGet("product-stock")] public async Task<ActionResult<ApiResponse<ProductStockReportResponse>>> GetProductStockReport([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<ProductStockReportResponse>.SuccessResponse(await _report.GetProductStockReportAsync(TenantId, filter)));
        [HttpGet("expired-stock")] public async Task<ActionResult<ApiResponse<ExpiredStockReportResponse>>> GetExpiredStockReport([FromQuery] int daysAhead = 30) => Ok(ApiResponse<ExpiredStockReportResponse>.SuccessResponse(await _report.GetExpiredStockReportAsync(TenantId, daysAhead)));
        [HttpGet("profit-loss")] public async Task<ActionResult<ApiResponse<ProfitLossResponse>>> GetProfitLoss([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<ProfitLossResponse>.SuccessResponse(await _report.GetProfitLossAsync(TenantId, filter)));
        [HttpGet("tax")] public async Task<ActionResult<ApiResponse<TaxReportResponse>>> GetTaxReport([FromQuery] ReportFilterRequest filter) => Ok(ApiResponse<TaxReportResponse>.SuccessResponse(await _report.GetTaxReportAsync(TenantId, filter)));

        [HttpPost("export")]
        public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request)
        {
            var bytes = await _report.ExportReportAsync(TenantId, request);
            var contentType = request.Format?.ToUpper() switch { "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CSV" => "text/csv", _ => "application/pdf" };
            var ext = request.Format?.ToUpper() switch { "EXCEL" => "xlsx", "CSV" => "csv", _ => "pdf" };
            return File(bytes, contentType, $"report-{request.ReportType?.ToLower()}.{ext}");
        }
    }

    [ApiController]
    [Route("api/credit-bills")]
    [Authorize]
    public class CreditBillController : ControllerBase
    {
        private readonly ICustomerCreditService _credit;
        public CreditBillController(ICustomerCreditService credit) { _credit = credit; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int? CurrentUserId { get { var c = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; return int.TryParse(c, out var id) ? id : null; } }

        private static CreditBillResponse MapCredit(Models.Entities.CustomerCredit credit) => new()
        {
            Id = credit.Id,
            CreditBillNumber = $"CR-{credit.Id}",
            CustomerId = credit.CustomerId,
            CustomerName = credit.Customer?.Name ?? string.Empty,
            CustomerPhone = credit.Customer?.Phone,
            InvoiceId = credit.InvoiceId,
            InvoiceNumber = credit.Invoice?.InvoiceNumber,
            UserId = credit.UserId,
            Amount = credit.Amount,
            SettledAmount = credit.PaidAmount,
            BalanceAmount = credit.BalanceAmount,
            DueDate = credit.DueDate,
            Status = credit.Status == "Paid" ? "settled" : credit.Status == "PartiallyPaid" ? "partially_settled" : "pending",
            Notes = credit.Notes,
            CreatedAt = credit.CreatedAt
        };

        [HttpGet("customer/{customerId:int}")] public async Task<ActionResult<ApiResponse<List<CreditBillResponse>>>> GetByCustomer(int customerId) => Ok(ApiResponse<List<CreditBillResponse>>.SuccessResponse((await _credit.GetCreditsAsync(customerId, TenantId)).Select(MapCredit).ToList()));
        [HttpGet("user/{userId:int}")] public async Task<ActionResult<ApiResponse<List<CreditBillResponse>>>> GetByUser(int userId) => Ok(ApiResponse<List<CreditBillResponse>>.SuccessResponse((await _credit.GetCreditsByUserAsync(userId, TenantId)).Select(MapCredit).ToList()));
        [HttpGet("my")] public async Task<ActionResult<ApiResponse<List<CreditBillResponse>>>> GetMyCredits() { if (CurrentUserId == null) return Unauthorized(ApiResponse<List<CreditBillResponse>>.FailResponse("User not found")); return Ok(ApiResponse<List<CreditBillResponse>>.SuccessResponse((await _credit.GetCreditsByUserAsync(CurrentUserId.Value, TenantId)).Select(MapCredit).ToList())); }
        [HttpGet("user/{userId:int}/balance")] public async Task<ActionResult<ApiResponse<decimal>>> GetUserBalance(int userId) => Ok(ApiResponse<decimal>.SuccessResponse(await _credit.GetUserCreditBalanceAsync(userId, TenantId)));
        [HttpGet("my/balance")] public async Task<ActionResult<ApiResponse<decimal>>> GetMyBalance() { if (CurrentUserId == null) return Unauthorized(ApiResponse<decimal>.FailResponse("User not found")); return Ok(ApiResponse<decimal>.SuccessResponse(await _credit.GetUserCreditBalanceAsync(CurrentUserId.Value, TenantId))); }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreditBillResponse>>> CreateCredit([FromBody] CreateCreditBillRequest r)
        {
            var effectiveUserId = r.UserId ?? CurrentUserId;
            var result = await _credit.CreateCreditAsync(r.CustomerId, r.InvoiceId, r.Amount, TenantId, effectiveUserId, r.DueDate, r.Notes);
            return Ok(ApiResponse<CreditBillResponse>.SuccessResponse(MapCredit(result), "Credit bill created"));
        }

        [HttpPost("settle")]
        public async Task<ActionResult<ApiResponse<CreditBillResponse>>> SettleCredit([FromBody] SettleCreditRequest r)
        {
            var result = await _credit.SettleCreditAsync(r.CreditId, r.PaymentAmount, TenantId);
            return Ok(ApiResponse<CreditBillResponse>.SuccessResponse(MapCredit(result), "Credit settled"));
        }
    }
}
