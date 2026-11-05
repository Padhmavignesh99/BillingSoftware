using MasterBilling.API.DTOs.Common;
using MasterBilling.API.Services.Implementations;
using MasterBilling.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterBilling.API.Controllers.Business
{
    [ApiController]
    [Authorize]
    [Route("api/backup")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        public BackupController(IBackupService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetBackups() =>
            Ok(ApiResponse<object>.SuccessResponse(await _svc.GetBackupsAsync(TenantId)));

        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            var record = await _svc.CreateBackupAsync(TenantId);
            return Ok(ApiResponse<object>.SuccessResponse(record, "Backup created successfully"));
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadBackup(int id)
        {
            var data = await _svc.DownloadBackupAsync(id);
            if (data == null || data.Length == 0)
                return NotFound(ApiResponse<object>.FailResponse("Backup file not found"));
            return File(data, "application/json", $"backup_{id}.json");
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/tally")]
    public class TallyController : ControllerBase
    {
        private readonly TallyExportService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        public TallyController(TallyExportService svc) => _svc = svc;

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var xml = await _svc.ExportToTallyXmlAsync(TenantId, from, to);
            var fileName = $"tally_export_{from:yyyyMMdd}_{to:yyyyMMdd}.xml";
            return File(xml, "application/xml", fileName);
        }
    }
}
