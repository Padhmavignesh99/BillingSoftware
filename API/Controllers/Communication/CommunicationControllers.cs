using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Communication
{
    // ==================== EMAIL CONTROLLER ====================
    [ApiController]
    [Route("api/email")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _email;
        public EmailController(IEmailService email) { _email = email; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Send a free-form email.</summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> Send(
            [FromBody] SendEmailRequest r)
            => Ok(ApiResponse<MessageSendResponse>.SuccessResponse(
                await _email.SendEmailAsync(r, TenantId), "Email sent"));

        /// <summary>Send invoice email by invoice id.</summary>
        [HttpPost("send-invoice")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> SendInvoice(
            [FromBody] SendInvoiceEmailRequest r)
            => Ok(ApiResponse<MessageSendResponse>.SuccessResponse(
                await _email.SendInvoiceEmailAsync(r.InvoiceId, r.EmailAddress, r.Subject, r.Message, TenantId),
                "Invoice email sent"));

        /// <summary>Email history for the tenant.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<CommunicationLogResponse>>>> History()
            => Ok(ApiResponse<List<CommunicationLogResponse>>.SuccessResponse(
                await _email.GetEmailHistoryAsync(TenantId)));
    }

    // ==================== WHATSAPP CONTROLLER ====================
    [ApiController]
    [Route("api/whatsapp")]
    [Authorize]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _wa;
        private readonly IPostBillingNotificationService _notify;
        public WhatsAppController(IWhatsAppService wa, IPostBillingNotificationService notify)
        {
            _wa = wa;
            _notify = notify;
        }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Send a free-form WhatsApp message.</summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> Send(
            [FromBody] SendWhatsAppRequest r)
        {
            var allowed = await _notify.IsWhatsAppEnabledAsync(TenantId);
            if (!allowed)
                return BadRequest(ApiResponse<MessageSendResponse>.FailResponse(
                    "WhatsApp messaging is not included in your current subscription plan. Please upgrade."));

            return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(
                await _wa.SendWhatsAppAsync(r, TenantId), "WhatsApp message sent"));
        }

        /// <summary>Send invoice notification via WhatsApp.</summary>
        [HttpPost("send-invoice")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> SendInvoice(
            [FromBody] SendInvoiceWhatsAppRequest r)
        {
            var allowed = await _notify.IsWhatsAppEnabledAsync(TenantId);
            if (!allowed)
                return BadRequest(ApiResponse<MessageSendResponse>.FailResponse(
                    "WhatsApp messaging is not included in your current subscription plan. Please upgrade."));

            return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(
                await _wa.SendInvoiceWhatsAppAsync(r.InvoiceId, r.MobileNumber, r.Message, TenantId),
                "WhatsApp invoice sent"));
        }

        /// <summary>WhatsApp message history for the tenant.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<CommunicationLogResponse>>>> History()
            => Ok(ApiResponse<List<CommunicationLogResponse>>.SuccessResponse(
                await _wa.GetMessageHistoryAsync(TenantId)));

        /// <summary>Check whether WhatsApp is enabled on this tenant's plan.</summary>
        [HttpGet("is-enabled")]
        public async Task<ActionResult<ApiResponse<bool>>> IsEnabled()
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _notify.IsWhatsAppEnabledAsync(TenantId)));

        /// <summary>Meta webhook — delivery / read receipts.</summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] string payload)
        {
            await _wa.HandleWebhookAsync(payload);
            return Ok();
        }
    }

    // ==================== SMS CONTROLLER ====================
    [ApiController]
    [Route("api/sms")]
    [Authorize]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _sms;
        private readonly IPostBillingNotificationService _notify;
        public SmsController(ISmsService sms, IPostBillingNotificationService notify)
        {
            _sms = sms;
            _notify = notify;
        }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>Send a free-form SMS.</summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> Send(
            [FromBody] SendSmsRequest r)
        {
            var allowed = await _notify.IsSmsEnabledAsync(TenantId);
            if (!allowed)
                return BadRequest(ApiResponse<MessageSendResponse>.FailResponse(
                    "SMS messaging is not included in your current subscription plan. Please upgrade."));

            return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(
                await _sms.SendSmsAsync(r, TenantId), "SMS sent"));
        }

        /// <summary>Send invoice notification via SMS.</summary>
        [HttpPost("send-invoice")]
        public async Task<ActionResult<ApiResponse<MessageSendResponse>>> SendInvoice(
            [FromBody] SendInvoiceSmsRequest r)
        {
            var allowed = await _notify.IsSmsEnabledAsync(TenantId);
            if (!allowed)
                return BadRequest(ApiResponse<MessageSendResponse>.FailResponse(
                    "SMS messaging is not included in your current subscription plan. Please upgrade."));

            return Ok(ApiResponse<MessageSendResponse>.SuccessResponse(
                await _sms.SendInvoiceSmsAsync(r.InvoiceId, r.MobileNumber, r.Message, TenantId),
                "Invoice SMS sent"));
        }

        /// <summary>SMS history for the tenant.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<CommunicationLogResponse>>>> History()
            => Ok(ApiResponse<List<CommunicationLogResponse>>.SuccessResponse(
                await _sms.GetSmsHistoryAsync(TenantId)));

        /// <summary>Check whether SMS is enabled on this tenant's plan.</summary>
        [HttpGet("is-enabled")]
        public async Task<ActionResult<ApiResponse<bool>>> IsEnabled()
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _notify.IsSmsEnabledAsync(TenantId)));
    }

    // ==================== NOTIFICATION SETTINGS CONTROLLER ====================
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notify;
        private readonly IPostBillingNotificationService _postBilling;
        public NotificationController(INotificationService notify, IPostBillingNotificationService postBilling)
        {
            _notify = notify;
            _postBilling = postBilling;
        }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        /// <summary>Get in-app notifications for the current user.</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationResponse>>>> GetAll()
            => Ok(ApiResponse<List<NotificationResponse>>.SuccessResponse(
                await _notify.GetNotificationsAsync(TenantId, UserId)));

        /// <summary>Mark a single notification as read.</summary>
        [HttpPatch("{id}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkRead(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _notify.MarkAsReadAsync(id, TenantId, UserId), "Marked as read"));

        [HttpPatch("read-all")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAllRead()
            => Ok(ApiResponse<bool>.SuccessResponse(await _notify.MarkAllAsReadAsync(TenantId, UserId), "Marked all as read"));

        /// <summary>Update which channels are enabled for in-app notifications.</summary>
        [HttpPut("settings")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateSettings(
            [FromBody] UpdateNotificationSettingsRequest r)
            => Ok(ApiResponse<bool>.SuccessResponse(
                await _notify.UpdateSettingsAsync(TenantId, r), "Settings updated"));

        /// <summary>
        /// Returns whether each communication channel is enabled on the
        /// tenant's active subscription plan. Used by the UI to show/hide
        /// the WhatsApp and SMS checkboxes in the post-billing dialog.
        /// </summary>
        [HttpGet("channel-status")]
        public async Task<ActionResult<ApiResponse<object>>> ChannelStatus()
        {
            var emailAllowed    = await _postBilling.IsEmailEnabledAsync(TenantId);
            var whatsappAllowed = await _postBilling.IsWhatsAppEnabledAsync(TenantId);
            var smsAllowed      = await _postBilling.IsSmsEnabledAsync(TenantId);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Email    = new { Allowed = emailAllowed },
                WhatsApp = new { Allowed = whatsappAllowed },
                Sms      = new { Allowed = smsAllowed }
            }));
        }
    }

    // ==================== POST-BILLING SEND-ALL CONTROLLER ====================
    [ApiController]
    [Route("api/post-billing")]
    [Authorize]
    public class PostBillingController : ControllerBase
    {
        private readonly IPostBillingNotificationService _postBilling;
        public PostBillingController(IPostBillingNotificationService postBilling)
        {
            _postBilling = postBilling;
        }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        /// <summary>
        /// Called immediately after creating a bill. Dispatches Email / WhatsApp / SMS
        /// based on which checkboxes the user ticked (SendEmail, SendWhatsApp, SendSms).
        /// Channels not enabled by the subscription plan return Status="NotAllowed".
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<PostBillingNotificationResult>>> Send(
            [FromBody] PostBillingSendRequest r)
            => Ok(ApiResponse<PostBillingNotificationResult>.SuccessResponse(
                await _postBilling.SendAsync(r, TenantId),
                "Notification dispatch complete"));
    }
}
