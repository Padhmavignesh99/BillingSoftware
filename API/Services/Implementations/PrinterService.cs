using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Services.Implementations
{
    /// <summary>
    /// Manages the physical/logical printer registry per tenant.
    /// Handles all printer types and connection modes.
    /// </summary>
    public class PrinterService : IPrinterService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPrintSettingsService _printSettings;

        public PrinterService(ApplicationDbContext db, IPrintSettingsService printSettings)
        {
            _db            = db;
            _printSettings = printSettings;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static PrinterResponse Map(Printer p) => new()
        {
            Id                = p.Id,
            PrinterName       = p.PrinterName,
            PrinterType       = p.PrinterType,
            PrinterTypeLabel  = GetTypeLabel(p.PrinterType),
            ConnectionType    = p.ConnectionType,
            ConnectionAddress = p.ConnectionAddress,
            Port              = p.Port,
            OsDriverName      = p.OsDriverName,
            PaperSize         = p.PaperSize,
            Orientation       = p.Orientation,
            CharsPerLine      = p.CharsPerLine,
            DpiResolution     = p.DpiResolution,
            SupportsColor     = p.SupportsColor,
            SupportsAutoCut   = p.SupportsAutoCut,
            SupportsCashDrawer= p.SupportsCashDrawer,
            SupportsBarcodes  = p.SupportsBarcodes,
            SupportsQRCode    = p.SupportsQRCode,
            SupportsLogo      = p.SupportsLogo,
            SupportsEscPos    = p.SupportsEscPos,
            SupportsPdf       = p.SupportsPdf,
            IsDefault         = p.IsDefault,
            IsActive          = p.IsActive,
            BranchId          = p.BranchId,
            CounterId         = p.CounterId,
            CodePage          = p.CodePage,
            LineFeedCount     = p.LineFeedCount,
            Notes             = p.Notes,
            LastTestedAt      = p.LastTestedAt,
            LastTestStatus    = p.LastTestStatus,
            CreatedAt         = p.CreatedAt,
            StatusLabel       = p.IsActive ? "Active" : "Inactive",
        };

        private static string GetTypeLabel(string type) => type switch
        {
            "Thermal"       => "Thermal Receipt Printer",
            "Laser"         => "Laser Printer (A4/A5)",
            "Inkjet"        => "Inkjet / Color Printer",
            "DotMatrix"     => "Dot Matrix Printer",
            "Label"         => "Barcode / Label Printer",
            "Portable"      => "Portable / Bluetooth Printer",
            "Multifunction" => "Multifunction Printer",
            _               => type
        };

        private static void Apply(Printer p, CreatePrinterRequest r)
        {
            p.PrinterName       = r.PrinterName;
            p.PrinterType       = r.PrinterType;
            p.ConnectionType    = r.ConnectionType;
            p.ConnectionAddress = r.ConnectionAddress;
            p.Port              = r.Port;
            p.OsDriverName      = r.OsDriverName;
            p.PaperSize         = r.PaperSize;
            p.Orientation       = r.Orientation;
            p.CharsPerLine      = r.CharsPerLine;
            p.DpiResolution     = r.DpiResolution;
            p.SupportsColor     = r.SupportsColor;
            p.SupportsAutoCut   = r.SupportsAutoCut;
            p.SupportsCashDrawer= r.SupportsCashDrawer;
            p.SupportsBarcodes  = r.SupportsBarcodes;
            p.SupportsQRCode    = r.SupportsQRCode;
            p.SupportsLogo      = r.SupportsLogo;
            p.SupportsEscPos    = r.SupportsEscPos;
            p.SupportsPdf       = r.SupportsPdf;
            p.IsDefault         = r.IsDefault;
            p.IsActive          = r.IsActive;
            p.BranchId          = r.BranchId;
            p.CounterId         = r.CounterId;
            p.CodePage          = r.CodePage;
            p.LineFeedCount     = r.LineFeedCount;
            p.Notes             = r.Notes;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────────

        public async Task<List<PrinterResponse>> GetAllAsync(
            int tenantId, int? branchId = null, int? counterId = null,
            bool? activeOnly = null, string? printerType = null)
        {
            var q = _db.Printers.Where(p => p.TenantId == tenantId);
            if (branchId != null)    q = q.Where(p => p.BranchId == branchId || p.BranchId == null);
            if (counterId != null)   q = q.Where(p => p.CounterId == counterId || p.CounterId == null);
            if (activeOnly == true)  q = q.Where(p => p.IsActive);
            if (printerType != null) q = q.Where(p => p.PrinterType == printerType);
            var list = await q.OrderByDescending(p => p.IsDefault).ThenBy(p => p.PrinterName).ToListAsync();
            return list.Select(Map).ToList();
        }

        public async Task<PrinterResponse?> GetByIdAsync(int id, int tenantId)
        {
            var p = await _db.Printers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            return p == null ? null : Map(p);
        }

        public async Task<PrinterResponse> CreateAsync(int tenantId, CreatePrinterRequest request)
        {
            // If this is marked default, clear existing defaults of same type+scope
            if (request.IsDefault)
                await ClearDefaultsAsync(tenantId, request.PrinterType, request.BranchId, request.CounterId);

            var printer = new Printer { TenantId = tenantId };
            Apply(printer, request);
            _db.Printers.Add(printer);
            await _db.SaveChangesAsync();
            return Map(printer);
        }

        public async Task<PrinterResponse> UpdateAsync(int id, int tenantId, CreatePrinterRequest request)
        {
            var printer = await _db.Printers.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Printer #{id} not found");

            if (request.IsDefault)
                await ClearDefaultsAsync(tenantId, request.PrinterType, request.BranchId, request.CounterId, excludeId: id);

            Apply(printer, request);
            printer.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(printer);
        }

        public async Task<PrinterResponse> PatchAsync(int id, int tenantId, PatchPrinterRequest r)
        {
            var p = await _db.Printers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Printer #{id} not found");

            if (r.PrinterName       != null) p.PrinterName       = r.PrinterName;
            if (r.PrinterType       != null) p.PrinterType       = r.PrinterType;
            if (r.ConnectionType    != null) p.ConnectionType    = r.ConnectionType;
            if (r.ConnectionAddress != null) p.ConnectionAddress = r.ConnectionAddress;
            if (r.Port              != null) p.Port              = r.Port;
            if (r.OsDriverName      != null) p.OsDriverName      = r.OsDriverName;
            if (r.PaperSize         != null) p.PaperSize         = r.PaperSize;
            if (r.Orientation       != null) p.Orientation       = r.Orientation;
            if (r.CharsPerLine      != null) p.CharsPerLine      = r.CharsPerLine;
            if (r.DpiResolution     != null) p.DpiResolution     = r.DpiResolution;
            if (r.SupportsColor     != null) p.SupportsColor     = r.SupportsColor.Value;
            if (r.SupportsAutoCut   != null) p.SupportsAutoCut   = r.SupportsAutoCut.Value;
            if (r.SupportsCashDrawer!= null) p.SupportsCashDrawer= r.SupportsCashDrawer.Value;
            if (r.SupportsBarcodes  != null) p.SupportsBarcodes  = r.SupportsBarcodes.Value;
            if (r.SupportsQRCode    != null) p.SupportsQRCode    = r.SupportsQRCode.Value;
            if (r.SupportsLogo      != null) p.SupportsLogo      = r.SupportsLogo.Value;
            if (r.SupportsEscPos    != null) p.SupportsEscPos    = r.SupportsEscPos.Value;
            if (r.SupportsPdf       != null) p.SupportsPdf       = r.SupportsPdf.Value;
            if (r.IsActive          != null) p.IsActive          = r.IsActive.Value;
            if (r.CodePage          != null) p.CodePage          = r.CodePage;
            if (r.LineFeedCount     != null) p.LineFeedCount     = r.LineFeedCount.Value;
            if (r.Notes             != null) p.Notes             = r.Notes;
            if (r.IsDefault == true)
            {
                await ClearDefaultsAsync(tenantId, p.PrinterType, p.BranchId, p.CounterId, excludeId: id);
                p.IsDefault = true;
            }
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(p);
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var p = await _db.Printers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (p == null) return false;
            _db.Printers.Remove(p);
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Default Management ─────────────────────────────────────────────────────

        public async Task<PrinterResponse> SetDefaultAsync(int id, int tenantId)
        {
            var printer = await _db.Printers.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Printer #{id} not found");

            await ClearDefaultsAsync(tenantId, printer.PrinterType, printer.BranchId, printer.CounterId, excludeId: id);
            printer.IsDefault = true;
            printer.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(printer);
        }

        private async Task ClearDefaultsAsync(int tenantId, string printerType, int? branchId, int? counterId, int? excludeId = null)
        {
            var existing = await _db.Printers
                .Where(p => p.TenantId == tenantId && p.PrinterType == printerType
                         && p.BranchId == branchId && p.CounterId == counterId
                         && p.IsDefault && (excludeId == null || p.Id != excludeId))
                .ToListAsync();
            foreach (var e in existing)
            {
                e.IsDefault  = false;
                e.UpdatedAt  = DateTime.UtcNow;
            }
        }

        // ── Connectivity Test ──────────────────────────────────────────────────────

        public async Task<PrinterTestResultResponse> TestConnectionAsync(int id, int tenantId)
        {
            var printer = await _db.Printers.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Printer #{id} not found");

            var result = await TestAsync(printer.ConnectionType, printer.ConnectionAddress, printer.Port);

            // Persist test result
            printer.LastTestedAt   = DateTime.UtcNow;
            printer.LastTestStatus = result.Success ? "OK" : "Failed";
            printer.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return result;
        }

        public async Task<PrinterTestResultResponse> TestConnectionByAddressAsync(
            int tenantId, string connectionType, string? address, int? port)
        {
            return await TestAsync(connectionType, address, port);
        }

        private static async Task<PrinterTestResultResponse> TestAsync(string connectionType, string? address, int? port)
        {
            var result = new PrinterTestResultResponse
            {
                ConnectionType    = connectionType,
                ConnectionAddress = address,
                Port              = port,
                TestedAt          = DateTime.UtcNow
            };

            // Network / Ethernet / WiFi: try raw TCP socket to ESC/POS port
            if (connectionType is "Ethernet" or "WiFi" or "Network")
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    result.Success = false;
                    result.Message = "No IP address configured for network printer.";
                    return result;
                }
                int tcpPort = port ?? 9100;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    using var client = new TcpClient();
                    var connect = client.ConnectAsync(address, tcpPort);
                    if (await Task.WhenAny(connect, Task.Delay(3000)) == connect && client.Connected)
                    {
                        sw.Stop();
                        result.Success   = true;
                        result.LatencyMs = (int)sw.ElapsedMilliseconds;
                        result.Message   = $"Network printer reachable at {address}:{tcpPort} ({result.LatencyMs}ms latency)";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = $"TCP connection to {address}:{tcpPort} timed out (3s). Verify IP, port, and firewall.";
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = $"TCP error: {ex.Message}";
                }
                return result;
            }

            // USB / Shared / Bluetooth / Cloud — validate config only (no raw TCP)
            if (connectionType == "USB")
            {
                result.Success = true;
                result.Message = "USB printer configuration is valid. Connect the printer and test from the billing counter.";
                return result;
            }
            if (connectionType == "Shared")
            {
                result.Success = !string.IsNullOrWhiteSpace(address);
                result.Message = result.Success
                    ? $"Shared printer path configured: {address}. Ensure the shared printer is accessible from this machine."
                    : "Shared printer path is empty. Provide the UNC path (e.g. \\\\SERVER\\PRINTER).";
                return result;
            }
            if (connectionType == "Bluetooth")
            {
                result.Success = true;
                result.Message = "Bluetooth configuration saved. Pair the printer with this device and test from the POS counter.";
                return result;
            }
            if (connectionType == "Cloud")
            {
                result.Success = !string.IsNullOrWhiteSpace(address);
                result.Message = result.Success
                    ? $"Cloud print endpoint configured: {address}"
                    : "Cloud endpoint URL is required.";
                return result;
            }

            result.Success = true;
            result.Message = "Printer configuration validated.";
            return result;
        }

        // ── Bill Printing ──────────────────────────────────────────────────────────

        public async Task<BillPrintResponse> PrintBillAsync(int invoiceId, int tenantId, BillPrintRequest request, int? userId = null)
        {
            // Resolve the printer (use supplied id, or find the default thermal/laser)
            Printer? printer = null;
            if (request.PrinterId.HasValue)
            {
                printer = await _db.Printers.FirstOrDefaultAsync(p => p.Id == request.PrinterId && p.TenantId == tenantId && p.IsActive);
            }
            else
            {
                // Auto-select default printer for the requested document type
                var preferredType = request.DocumentType == "Receipt" ? "Thermal" : "Laser";
                printer = await _db.Printers
                    .Where(p => p.TenantId == tenantId && p.IsActive && p.IsDefault
                             && (request.BranchId == null || p.BranchId == null || p.BranchId == request.BranchId))
                    .OrderBy(p => p.PrinterType == preferredType ? 0 : 1)
                    .FirstOrDefaultAsync();
            }

            // Get the print settings to build the HTML
            var settings = await _printSettings.GetPrintSettingsAsync(tenantId, request.BranchId, request.CounterId);
            var htmlPreview = await _printSettings.GetPrintPreviewHtmlAsync(tenantId, request.DocumentType.ToLower(), request.BranchId, request.CounterId);

            // Determine print method
            var method = printer?.SupportsEscPos == true && printer?.ConnectionType is "Ethernet" or "WiFi" or "USB"
                ? "ESC/POS"
                : printer?.SupportsPdf == true
                    ? "PDF"
                    : "Browser";

            // Queue a print job for tracking
            var job = await _printSettings.QueuePrintJobAsync(tenantId, new QueuePrintJobRequest
            {
                DocumentType      = request.DocumentType,
                DocumentId        = invoiceId,
                PrinterTarget     = printer?.PrinterName ?? method,
                PaperSize         = request.PaperSize ?? printer?.PaperSize ?? settings.PaperSize,
                Copies            = request.Copies,
                BranchId          = request.BranchId,
                CounterId         = request.CounterId,
            });

            return new BillPrintResponse
            {
                PrintJobId    = job.Id,
                Status        = job.Status,
                DocumentType  = request.DocumentType,
                DocumentId    = invoiceId,
                PrinterName   = printer?.PrinterName,
                PrintMethod   = method,
                Copies        = request.Copies,
                HtmlContent   = method == "Browser" ? htmlPreview : null,
                Message       = $"Print job #{job.Id} queued via {method}. {printer?.PrinterName ?? "No printer auto-selected."}"
            };
        }

        public async Task<BillPrintResponse> ReprintAsync(int printJobId, int tenantId, int? userId = null)
        {
            var job = await _printSettings.ReprintAsync(printJobId, tenantId, userId);
            return new BillPrintResponse
            {
                PrintJobId   = job.Id,
                Status       = job.Status,
                DocumentType = job.DocumentType,
                DocumentId   = job.DocumentId,
                DocumentRef  = job.DocumentReference,
                PrinterName  = job.PrinterTarget,
                PrintMethod  = "Reprint",
                Copies       = job.Copies,
                Message      = $"Reprint job #{job.Id} queued."
            };
        }

        // ── Lookup Data ────────────────────────────────────────────────────────────

        public List<object> GetPrinterTypes() => new()
        {
            new { Value = "Thermal",       Label = "Thermal Receipt Printer",    Description = "58mm/80mm ESC/POS receipts for POS billing",           Icon = "receipt" },
            new { Value = "Laser",         Label = "Laser Printer (A4/A5)",      Description = "A4 invoices, reports, GST bills",                       Icon = "file-text" },
            new { Value = "Inkjet",        Label = "Inkjet / Colour Printer",    Description = "Colour invoices, quotations, certificates",             Icon = "printer" },
            new { Value = "DotMatrix",     Label = "Dot Matrix Printer",         Description = "Multi-copy duplicate invoices, continuous paper",        Icon = "layers" },
            new { Value = "Label",         Label = "Barcode / Label Printer",    Description = "Product labels (TSPL/ZPL), barcodes, price stickers",   Icon = "tag" },
            new { Value = "Portable",      Label = "Portable / Bluetooth",       Description = "Field billing, mobile POS, compact receipts",            Icon = "smartphone" },
            new { Value = "Multifunction", Label = "Multifunction Printer",      Description = "Print, scan, copy — typically LAN-connected",           Icon = "copy" },
        };

        public List<object> GetConnectionTypes() => new()
        {
            new { Value = "USB",      Label = "USB (Locally Installed)",   Description = "Direct USB connection; uses OS driver for printing" },
            new { Value = "Ethernet", Label = "Ethernet / LAN (TCP:9100)", Description = "Raw TCP socket; best for thermal POS printers" },
            new { Value = "WiFi",     Label = "Wi-Fi (Network)",           Description = "Same as Ethernet but over wireless network" },
            new { Value = "Bluetooth",Label = "Bluetooth",                 Description = "Mobile/portable printers; requires pairing" },
            new { Value = "Shared",   Label = "Shared (OS / Network)",     Description = "Windows/CUPS shared printer via UNC path" },
            new { Value = "Cloud",    Label = "Cloud Print",               Description = "Cloud-based print relay (e.g. Google Cloud Print, PrintNode)" },
        };

        public List<object> GetSupportedPaperSizes() => new()
        {
            new { Value = "A4",        Label = "A4 (210×297mm)",       Category = "Standard",  WidthMm = 210, HeightMm = 297 },
            new { Value = "A5",        Label = "A5 (148×210mm)",       Category = "Standard",  WidthMm = 148, HeightMm = 210 },
            new { Value = "Letter",    Label = "Letter (216×279mm)",   Category = "Standard",  WidthMm = 216, HeightMm = 279 },
            new { Value = "Legal",     Label = "Legal (216×356mm)",    Category = "Standard",  WidthMm = 216, HeightMm = 356 },
            new { Value = "80mm",      Label = "80mm Thermal",         Category = "Thermal",   WidthMm = 80,  HeightMm = 0   },
            new { Value = "58mm",      Label = "58mm Thermal",         Category = "Thermal",   WidthMm = 58,  HeightMm = 0   },
            new { Value = "50x25mm",   Label = "50×25mm Label",        Category = "Label",     WidthMm = 50,  HeightMm = 25  },
            new { Value = "100x50mm",  Label = "100×50mm Label",       Category = "Label",     WidthMm = 100, HeightMm = 50  },
            new { Value = "Custom",    Label = "Custom Size",          Category = "Custom",    WidthMm = 0,   HeightMm = 0   },
        };
    }
}
