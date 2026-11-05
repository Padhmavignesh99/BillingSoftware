using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Business
{
    [ApiController]
    [Route("api/loyalty")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _s;
        public LoyaltyController(ILoyaltyService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet("program")]
        public async Task<ActionResult<ApiResponse<LoyaltyProgramResponse>>> GetProgram()
            => Ok(ApiResponse<LoyaltyProgramResponse>.SuccessResponse(await _s.GetProgramAsync(TenantId)));

        [HttpPut("program")]
        public async Task<ActionResult<ApiResponse<LoyaltyProgramResponse>>> UpdateProgram([FromBody] UpdateLoyaltyProgramRequest r)
            => Ok(ApiResponse<LoyaltyProgramResponse>.SuccessResponse(await _s.UpdateProgramAsync(r, TenantId), "Updated"));

        [HttpGet("tiers")]
        public async Task<ActionResult<ApiResponse<List<LoyaltyTierResponse>>>> GetTiers()
            => Ok(ApiResponse<List<LoyaltyTierResponse>>.SuccessResponse(await _s.GetTiersAsync(TenantId)));

        [HttpPost("tiers")]
        public async Task<ActionResult<ApiResponse<LoyaltyTierResponse>>> CreateTier([FromBody] CreateLoyaltyTierRequest r)
            => Ok(ApiResponse<LoyaltyTierResponse>.SuccessResponse(await _s.CreateTierAsync(r, TenantId), "Tier created"));

        [HttpDelete("tiers/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTier(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteTierAsync(id, TenantId), "Deleted"));

        [HttpGet("customers/{customerId}/account")]
        public async Task<ActionResult<ApiResponse<LoyaltyAccountResponse>>> GetCustomerAccount(int customerId)
            => Ok(ApiResponse<LoyaltyAccountResponse>.SuccessResponse(await _s.GetCustomerAccountAsync(customerId, TenantId)));

        [HttpPost("earn")]
        public async Task<ActionResult<ApiResponse<LoyaltyAccountResponse>>> Earn([FromBody] EarnPointsRequest r)
            => Ok(ApiResponse<LoyaltyAccountResponse>.SuccessResponse(await _s.EarnPointsAsync(r, TenantId), "Points earned"));

        [HttpPost("redeem")]
        public async Task<ActionResult<ApiResponse<LoyaltyAccountResponse>>> Redeem([FromBody] RedeemPointsRequest r)
            => Ok(ApiResponse<LoyaltyAccountResponse>.SuccessResponse(await _s.RedeemPointsAsync(r, TenantId), "Points redeemed"));

        [HttpPost("adjust")]
        public async Task<ActionResult<ApiResponse<LoyaltyAccountResponse>>> Adjust([FromBody] AdjustPointsRequest r)
            => Ok(ApiResponse<LoyaltyAccountResponse>.SuccessResponse(await _s.AdjustPointsAsync(r, TenantId), "Points adjusted"));

        [HttpGet("check")]
        public async Task<ActionResult<ApiResponse<RedeemCheckResponse>>> CheckRedemption([FromQuery] int customerId, [FromQuery] decimal points)
            => Ok(ApiResponse<RedeemCheckResponse>.SuccessResponse(await _s.CheckRedemptionAsync(customerId, points, TenantId)));

        [HttpGet("top-customers")]
        public async Task<ActionResult<ApiResponse<List<LoyaltyAccountResponse>>>> TopCustomers([FromQuery] int take = 10)
            => Ok(ApiResponse<List<LoyaltyAccountResponse>>.SuccessResponse(await _s.GetTopCustomersAsync(TenantId, take)));
    }

    [ApiController]
    [Route("api/reminders")]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _s;
        public ReminderController(IReminderService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet("rules")]
        public async Task<ActionResult<ApiResponse<List<ReminderRuleResponse>>>> GetRules()
            => Ok(ApiResponse<List<ReminderRuleResponse>>.SuccessResponse(await _s.GetRulesAsync(TenantId)));

        [HttpPost("rules")]
        public async Task<ActionResult<ApiResponse<ReminderRuleResponse>>> CreateRule([FromBody] CreateReminderRuleRequest r)
            => Ok(ApiResponse<ReminderRuleResponse>.SuccessResponse(await _s.CreateRuleAsync(r, TenantId), "Rule created"));

        [HttpPut("rules/{id}")]
        public async Task<ActionResult<ApiResponse<ReminderRuleResponse>>> UpdateRule(int id, [FromBody] CreateReminderRuleRequest r)
            => Ok(ApiResponse<ReminderRuleResponse>.SuccessResponse(await _s.UpdateRuleAsync(id, r, TenantId), "Updated"));

        [HttpDelete("rules/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRule(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteRuleAsync(id, TenantId), "Deleted"));

        [HttpGet("overdue-customers")]
        public async Task<ActionResult<ApiResponse<List<OverdueCustomerResponse>>>> GetOverdueCustomers()
            => Ok(ApiResponse<List<OverdueCustomerResponse>>.SuccessResponse(await _s.GetOverdueCustomersAsync(TenantId)));

        [HttpPost("send-manual")]
        public async Task<ActionResult<ApiResponse<bool>>> SendManual([FromBody] SendManualReminderRequest r)
        {
            await _s.SendManualReminderAsync(r, TenantId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Reminder sent"));
        }

        [HttpGet("logs")]
        public async Task<ActionResult<ApiResponse<List<ReminderLogResponse>>>> GetLogs([FromQuery] PaginationRequest p)
            => Ok(ApiResponse<List<ReminderLogResponse>>.SuccessResponse(await _s.GetLogsAsync(TenantId, p)));
    }
}

