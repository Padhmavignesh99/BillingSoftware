using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;

namespace BillingSoftware.API.Services.Interfaces
{
    /// <summary>
    /// Manages physical/logical printer registrations for a tenant.
    /// Supports all printer types (Thermal, Laser, Inkjet, DotMatrix, Label, Portable)
    /// and all connection methods (USB, Ethernet, WiFi, Bluetooth, Shared, Cloud).
    /// </summary>
    public interface IPrinterService
    {
        // ── CRUD ──────────────────────────────────────────────────────────────────
        Task<List<PrinterResponse>> GetAllAsync(int tenantId, int? branchId = null, int? counterId = null, bool? activeOnly = null, string? printerType = null);
        Task<PrinterResponse?> GetByIdAsync(int id, int tenantId);
        Task<PrinterResponse> CreateAsync(int tenantId, CreatePrinterRequest request);
        Task<PrinterResponse> UpdateAsync(int id, int tenantId, CreatePrinterRequest request);
        Task<PrinterResponse> PatchAsync(int id, int tenantId, PatchPrinterRequest request);
        Task<bool> DeleteAsync(int id, int tenantId);

        // ── Default Management ─────────────────────────────────────────────────────
        Task<PrinterResponse> SetDefaultAsync(int id, int tenantId);

        // ── Connectivity Test ──────────────────────────────────────────────────────
        Task<PrinterTestResultResponse> TestConnectionAsync(int id, int tenantId);
        Task<PrinterTestResultResponse> TestConnectionByAddressAsync(int tenantId, string connectionType, string? address, int? port);

        // ── Bill Printing ──────────────────────────────────────────────────────────
        Task<BillPrintResponse> PrintBillAsync(int invoiceId, int tenantId, BillPrintRequest request, int? userId = null);
        Task<BillPrintResponse> ReprintAsync(int printJobId, int tenantId, int? userId = null);

        // ── Lookup Data ────────────────────────────────────────────────────────────
        List<object> GetPrinterTypes();
        List<object> GetConnectionTypes();
        List<object> GetSupportedPaperSizes();
    }
}
