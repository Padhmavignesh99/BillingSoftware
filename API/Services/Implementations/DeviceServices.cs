using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== DEVICE CONFIGURATION SERVICE ====================
    public class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly ApplicationDbContext _db;
        public DeviceConfigurationService(ApplicationDbContext db) => _db = db;

        public async Task<List<DeviceConfigurationResponse>> GetAllDevicesAsync(int tenantId)
            => await _db.DeviceConfigurations
                .Where(d => d.TenantId == tenantId && !d.IsDeleted)
                .Include(d => d.Branch)
                .Select(d => MapToResponse(d))
                .ToListAsync();

        public async Task<DeviceConfigurationResponse?> GetDeviceByIdAsync(int id, int tenantId)
        {
            var d = await _db.DeviceConfigurations
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return d == null ? null : MapToResponse(d);
        }

        public async Task<List<DeviceConfigurationResponse>> GetDevicesByTypeAsync(DeviceType deviceType, int tenantId)
            => await _db.DeviceConfigurations
                .Where(d => d.TenantId == tenantId && d.DeviceType == deviceType && !d.IsDeleted)
                .Include(d => d.Branch)
                .Select(d => MapToResponse(d))
                .ToListAsync();

        public async Task<List<DeviceConfigurationResponse>> GetDevicesByCategoryAsync(DeviceCategory category, int tenantId)
            => await _db.DeviceConfigurations
                .Where(d => d.TenantId == tenantId && d.DeviceCategory == category && !d.IsDeleted)
                .Include(d => d.Branch)
                .Select(d => MapToResponse(d))
                .ToListAsync();

        public async Task<DeviceConfigurationResponse> CreateDeviceAsync(CreateDeviceConfigurationRequest request, int tenantId)
        {
            // Auto-set DeviceCategory based on DeviceType
            var category = GetCategory(request.DeviceType);

            // If this is the first device of this type and IsDefault not set, make it default
            bool isDefault = request.IsDefault ??
                !await _db.DeviceConfigurations.AnyAsync(d => d.TenantId == tenantId && d.DeviceType == request.DeviceType && !d.IsDeleted);

            // Clear IsDefault from other devices of same type if this one becomes default
            if (isDefault)
                await ClearDefaultAsync(request.DeviceType, tenantId);

            var device = new DeviceConfiguration
            {
                TenantId          = tenantId,
                Name              = request.Name,
                DeviceCategory    = category,
                DeviceType        = request.DeviceType,
                ConnectionType    = request.ConnectionType,
                Status            = DeviceStatus.Active,
                PortOrAddress     = request.PortOrAddress,
                IpAddress         = request.IpAddress,
                NetworkPort       = request.NetworkPort,
                BaudRate          = request.BaudRate,
                PaperSize         = request.PaperSize,
                IsDefault         = isDefault,
                Model             = request.Model,
                SerialNumber      = request.SerialNumber,
                BranchId          = request.BranchId,
                ConfigurationJson = request.ConfigurationJson,
                Notes             = request.Notes
            };
            _db.DeviceConfigurations.Add(device);
            await _db.SaveChangesAsync();
            return MapToResponse(device);
        }

        public async Task<DeviceConfigurationResponse> UpdateDeviceAsync(int id, UpdateDeviceConfigurationRequest request, int tenantId)
        {
            var device = await _db.DeviceConfigurations
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId && !d.IsDeleted)
                ?? throw new KeyNotFoundException($"Device {id} not found.");

            if (request.Name             != null) device.Name             = request.Name;
            if (request.ConnectionType   != null) device.ConnectionType   = request.ConnectionType.Value;
            if (request.Status           != null) device.Status           = request.Status.Value;
            if (request.PortOrAddress    != null) device.PortOrAddress    = request.PortOrAddress;
            if (request.IpAddress        != null) device.IpAddress        = request.IpAddress;
            if (request.NetworkPort      != null) device.NetworkPort      = request.NetworkPort;
            if (request.BaudRate         != null) device.BaudRate         = request.BaudRate;
            if (request.PaperSize        != null) device.PaperSize        = request.PaperSize;
            if (request.Model            != null) device.Model            = request.Model;
            if (request.SerialNumber     != null) device.SerialNumber     = request.SerialNumber;
            if (request.BranchId         != null) device.BranchId         = request.BranchId;
            if (request.ConfigurationJson != null) device.ConfigurationJson = request.ConfigurationJson;
            if (request.Notes            != null) device.Notes            = request.Notes;

            // Handle IsDefault change
            if (request.IsDefault == true)
            {
                await ClearDefaultAsync(device.DeviceType, tenantId, excludeId: id);
                device.IsDefault = true;
            }
            else if (request.IsDefault == false)
            {
                device.IsDefault = false;
            }

            await _db.SaveChangesAsync();
            return MapToResponse(device);
        }

        public async Task<bool> DeleteDeviceAsync(int id, int tenantId)
        {
            var device = await _db.DeviceConfigurations
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Device {id} not found.");
            device.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDeviceStatusAsync(int id, DeviceStatus status, int tenantId)
        {
            var device = await _db.DeviceConfigurations
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Device {id} not found.");
            device.Status = status;
            if (status == DeviceStatus.Connected)
                device.LastConnectedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<DeviceStatusResponse> GetDeviceStatusSummaryAsync(int tenantId)
        {
            var devices  = await _db.DeviceConfigurations.Where(d => d.TenantId == tenantId && !d.IsDeleted).ToListAsync();
            var settings = await GetOrCreateDeviceSettingsAsync(tenantId);

            return new DeviceStatusResponse
            {
                Devices = devices.Select(d => new DeviceStatusItem
                {
                    Id              = d.Id,
                    Name            = d.Name,
                    DeviceType      = d.DeviceType.ToString(),
                    Status          = d.Status.ToString(),
                    LastConnectedAt = d.LastConnectedAt,
                    IsOnline        = d.Status is DeviceStatus.Connected or DeviceStatus.Active
                }).ToList(),
                Settings  = MapSettingsToResponse(settings),
                CheckedAt = DateTime.UtcNow
            };
        }

        // ── Private Helpers ──────────────────────────────────────────────────────
        private async Task ClearDefaultAsync(DeviceType type, int tenantId, int? excludeId = null)
        {
            var defaults = await _db.DeviceConfigurations
                .Where(d => d.TenantId == tenantId && d.DeviceType == type && d.IsDefault && !d.IsDeleted
                         && (excludeId == null || d.Id != excludeId))
                .ToListAsync();
            foreach (var d in defaults) d.IsDefault = false;
        }

        private static DeviceCategory GetCategory(DeviceType type) => type switch
        {
            DeviceType.ThermalPrinter  or DeviceType.A4Printer    or DeviceType.LabelPrinter or
            DeviceType.CustomerDisplay or DeviceType.Speaker       or DeviceType.CashDrawer   or
            DeviceType.Monitor         => DeviceCategory.Output,

            DeviceType.CardPOSTerminal or DeviceType.UPITerminal   or DeviceType.PaymentGateway => DeviceCategory.Payment,

            DeviceType.EmailDelivery   or DeviceType.WhatsAppDelivery or DeviceType.SMSDelivery or
            DeviceType.PDFGenerator    => DeviceCategory.Digital,

            _ => DeviceCategory.Input
        };

        private static DeviceConfigurationResponse MapToResponse(DeviceConfiguration d) => new()
        {
            Id                = d.Id,
            Name              = d.Name,
            DeviceCategory    = d.DeviceCategory.ToString(),
            DeviceType        = d.DeviceType.ToString(),
            ConnectionType    = d.ConnectionType.ToString(),
            Status            = d.Status.ToString(),
            PortOrAddress     = d.PortOrAddress,
            IpAddress         = d.IpAddress,
            NetworkPort       = d.NetworkPort,
            BaudRate          = d.BaudRate,
            PaperSize         = d.PaperSize?.ToString(),
            IsDefault         = d.IsDefault,
            Model             = d.Model,
            SerialNumber      = d.SerialNumber,
            BranchId          = d.BranchId,
            BranchName        = d.Branch?.Name,
            ConfigurationJson = d.ConfigurationJson,
            LastConnectedAt   = d.LastConnectedAt,
            Notes             = d.Notes,
            CreatedAt         = d.CreatedAt
        };

        private async Task<DeviceSettings> GetOrCreateDeviceSettingsAsync(int tenantId)
        {
            var s = await _db.DeviceSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId);
            if (s == null) { s = new DeviceSettings { TenantId = tenantId }; _db.DeviceSettings.Add(s); await _db.SaveChangesAsync(); }
            return s;
        }

        private static DeviceSettingsResponse MapSettingsToResponse(DeviceSettings s) => DeviceSettingsMapper.Map(s);
    }

    // ==================== DEVICE SETTINGS SERVICE ====================
    public class DeviceSettingsService : IDeviceSettingsService
    {
        private readonly ApplicationDbContext _db;
        public DeviceSettingsService(ApplicationDbContext db) => _db = db;

        public async Task<DeviceSettingsResponse> GetSettingsAsync(int tenantId)
            => DeviceSettingsMapper.Map(await GetOrCreateAsync(tenantId));

        public async Task<DeviceSettingsResponse> UpdateSettingsAsync(int tenantId, UpdateDeviceSettingsRequest r)
        {
            var s = await GetOrCreateAsync(tenantId);

            // ── INPUT: Barcode ───────────────────────────────────────────────────
            if (r.BarcodeScannerEnabled     != null) s.BarcodeScannerEnabled     = r.BarcodeScannerEnabled.Value;
            if (r.BarcodeScannerSuffix      != null) s.BarcodeScannerSuffix      = r.BarcodeScannerSuffix;
            if (r.BarcodeScannerMinLength   != null) s.BarcodeScannerMinLength   = r.BarcodeScannerMinLength.Value;
            if (r.BarcodeScannerThresholdMs != null) s.BarcodeScannerThresholdMs = r.BarcodeScannerThresholdMs.Value;

            // ── INPUT: QR ────────────────────────────────────────────────────────
            if (r.QRScannerEnabled != null) s.QRScannerEnabled = r.QRScannerEnabled.Value;
            if (r.CameraQREnabled  != null) s.CameraQREnabled  = r.CameraQREnabled.Value;

            // ── INPUT: Weighing Scale ────────────────────────────────────────────
            if (r.WeighingScaleEnabled       != null) s.WeighingScaleEnabled       = r.WeighingScaleEnabled.Value;
            if (r.WeighingScaleMode          != null) s.WeighingScaleMode          = r.WeighingScaleMode.Value;
            if (r.WeighingScaleUnit          != null) s.WeighingScaleUnit          = r.WeighingScaleUnit.Value;
            if (r.WeighingScaleDecimalPlaces != null) s.WeighingScaleDecimalPlaces = r.WeighingScaleDecimalPlaces.Value;
            if (r.WeighingScalePort          != null) s.WeighingScalePort          = r.WeighingScalePort;
            if (r.WeighingScaleBaudRate      != null) s.WeighingScaleBaudRate      = r.WeighingScaleBaudRate.Value;

            // ── INPUT: Other ─────────────────────────────────────────────────────
            if (r.TouchscreenModeEnabled != null) s.TouchscreenModeEnabled = r.TouchscreenModeEnabled.Value;
            if (r.RFIDEnabled            != null) s.RFIDEnabled            = r.RFIDEnabled.Value;
            if (r.BiometricEnabled       != null) s.BiometricEnabled       = r.BiometricEnabled.Value;
            if (r.OcrScannerEnabled      != null) s.OcrScannerEnabled      = r.OcrScannerEnabled.Value;
            if (r.OcrProvider            != null) s.OcrProvider            = r.OcrProvider;
            if (r.OcrApiKey              != null) s.OcrApiKey              = r.OcrApiKey;

            // ── OUTPUT: Thermal Printer ──────────────────────────────────────────
            if (r.ThermalPrinterEnabled  != null) s.ThermalPrinterEnabled  = r.ThermalPrinterEnabled.Value;
            if (r.ThermalPrinterName     != null) s.ThermalPrinterName     = r.ThermalPrinterName;
            if (r.ThermalPaperSize       != null) s.ThermalPaperSize       = r.ThermalPaperSize.Value;
            if (r.ThermalPrinterConnection != null) s.ThermalPrinterConnection = r.ThermalPrinterConnection.Value;
            if (r.ThermalPrinterPort     != null) s.ThermalPrinterPort     = r.ThermalPrinterPort;
            if (r.ThermalPrinterCopies   != null) s.ThermalPrinterCopies   = r.ThermalPrinterCopies.Value;
            if (r.ThermalAutoPrint       != null) s.ThermalAutoPrint       = r.ThermalAutoPrint.Value;
            if (r.ThermalOpenCashDrawer  != null) s.ThermalOpenCashDrawer  = r.ThermalOpenCashDrawer.Value;

            // ── OUTPUT: A4 Printer ───────────────────────────────────────────────
            if (r.A4PrinterEnabled   != null) s.A4PrinterEnabled   = r.A4PrinterEnabled.Value;
            if (r.A4PrinterName      != null) s.A4PrinterName      = r.A4PrinterName;
            if (r.A4PaperSize        != null) s.A4PaperSize        = r.A4PaperSize.Value;
            if (r.A4ColorMode        != null) s.A4ColorMode        = r.A4ColorMode.Value;
            if (r.A4PrinterCopies    != null) s.A4PrinterCopies    = r.A4PrinterCopies.Value;
            if (r.A4PrinterIsDefault != null) s.A4PrinterIsDefault = r.A4PrinterIsDefault.Value;

            // ── OUTPUT: Label Printer ────────────────────────────────────────────
            if (r.LabelPrinterEnabled != null) s.LabelPrinterEnabled = r.LabelPrinterEnabled.Value;
            if (r.LabelPrinterName    != null) s.LabelPrinterName    = r.LabelPrinterName;
            if (r.LabelWidthMm        != null) s.LabelWidthMm        = r.LabelWidthMm;
            if (r.LabelHeightMm       != null) s.LabelHeightMm       = r.LabelHeightMm;
            if (r.LabelPerRow         != null) s.LabelPerRow         = r.LabelPerRow.Value;
            if (r.LabelShowBarcode    != null) s.LabelShowBarcode    = r.LabelShowBarcode.Value;
            if (r.LabelShowMRP        != null) s.LabelShowMRP        = r.LabelShowMRP.Value;
            if (r.LabelShowBusiness   != null) s.LabelShowBusiness   = r.LabelShowBusiness.Value;

            // ── OUTPUT: Customer Display ─────────────────────────────────────────
            if (r.CustomerDisplayEnabled        != null) s.CustomerDisplayEnabled        = r.CustomerDisplayEnabled.Value;
            if (r.CustomerDisplayProtocol       != null) s.CustomerDisplayProtocol       = r.CustomerDisplayProtocol.Value;
            if (r.CustomerDisplayPort           != null) s.CustomerDisplayPort           = r.CustomerDisplayPort;
            if (r.CustomerDisplayWelcomeMessage != null) s.CustomerDisplayWelcomeMessage = r.CustomerDisplayWelcomeMessage;
            if (r.CustomerDisplayShowShopName   != null) s.CustomerDisplayShowShopName   = r.CustomerDisplayShowShopName.Value;

            // ── OUTPUT: Speaker ──────────────────────────────────────────────────
            if (r.SpeakerEnabled          != null) s.SpeakerEnabled          = r.SpeakerEnabled.Value;
            if (r.SpeakerBeepOnScan       != null) s.SpeakerBeepOnScan       = r.SpeakerBeepOnScan.Value;
            if (r.SpeakerAnnounceTotal    != null) s.SpeakerAnnounceTotal    = r.SpeakerAnnounceTotal.Value;
            if (r.SpeakerAnnouncePayment  != null) s.SpeakerAnnouncePayment  = r.SpeakerAnnouncePayment.Value;

            // ── OUTPUT: Cash Drawer ──────────────────────────────────────────────
            if (r.CashDrawerEnabled        != null) s.CashDrawerEnabled        = r.CashDrawerEnabled.Value;
            if (r.CashDrawerConnectionType != null) s.CashDrawerConnectionType = r.CashDrawerConnectionType.Value;
            if (r.CashDrawerPort           != null) s.CashDrawerPort           = r.CashDrawerPort;
            if (r.CashDrawerTrigger        != null) s.CashDrawerTrigger        = r.CashDrawerTrigger.Value;

            // ── PAYMENT: Card Terminal ───────────────────────────────────────────
            if (r.CardTerminalEnabled  != null) s.CardTerminalEnabled  = r.CardTerminalEnabled.Value;
            if (r.CardTerminalProvider != null) s.CardTerminalProvider = r.CardTerminalProvider;
            if (r.CardTerminalApiKey   != null) s.CardTerminalApiKey   = r.CardTerminalApiKey;
            if (r.CardTerminalId       != null) s.CardTerminalId       = r.CardTerminalId;

            // ── PAYMENT: UPI ─────────────────────────────────────────────────────
            if (r.UPIEnabled              != null) s.UPIEnabled              = r.UPIEnabled.Value;
            if (r.UPIId                   != null) s.UPIId                   = r.UPIId;
            if (r.UPIMerchantName         != null) s.UPIMerchantName         = r.UPIMerchantName;
            if (r.UPIShowOnReceipt        != null) s.UPIShowOnReceipt        = r.UPIShowOnReceipt.Value;
            if (r.UPIShowOnCustomerDisplay != null) s.UPIShowOnCustomerDisplay = r.UPIShowOnCustomerDisplay.Value;

            // ── PAYMENT: Gateway ─────────────────────────────────────────────────
            if (r.PaymentGatewayEnabled    != null) s.PaymentGatewayEnabled    = r.PaymentGatewayEnabled.Value;
            if (r.PaymentGatewayProvider   != null) s.PaymentGatewayProvider   = r.PaymentGatewayProvider;
            if (r.PaymentGatewayApiKey     != null) s.PaymentGatewayApiKey     = r.PaymentGatewayApiKey;
            if (r.PaymentGatewayApiSecret  != null) s.PaymentGatewayApiSecret  = r.PaymentGatewayApiSecret;
            if (r.PaymentGatewayWebhookUrl != null) s.PaymentGatewayWebhookUrl = r.PaymentGatewayWebhookUrl;

            // ── DIGITAL: Email ───────────────────────────────────────────────────
            if (r.EmailDeliveryEnabled != null) s.EmailDeliveryEnabled = r.EmailDeliveryEnabled.Value;
            if (r.EmailAutoSendOnSale  != null) s.EmailAutoSendOnSale  = r.EmailAutoSendOnSale.Value;

            // ── DIGITAL: WhatsApp ────────────────────────────────────────────────
            if (r.WhatsAppDeliveryEnabled != null) s.WhatsAppDeliveryEnabled = r.WhatsAppDeliveryEnabled.Value;
            if (r.WhatsAppAutoSendOnSale  != null) s.WhatsAppAutoSendOnSale  = r.WhatsAppAutoSendOnSale.Value;

            // ── DIGITAL: SMS ─────────────────────────────────────────────────────
            if (r.SMSDeliveryEnabled != null) s.SMSDeliveryEnabled = r.SMSDeliveryEnabled.Value;
            if (r.SMSAutoSendOnSale  != null) s.SMSAutoSendOnSale  = r.SMSAutoSendOnSale.Value;

            // ── DIGITAL: PDF ─────────────────────────────────────────────────────
            if (r.PDFEnabled        != null) s.PDFEnabled        = r.PDFEnabled.Value;
            if (r.PDFAutoGenerate   != null) s.PDFAutoGenerate   = r.PDFAutoGenerate.Value;
            if (r.PDFSaveToStorage  != null) s.PDFSaveToStorage  = r.PDFSaveToStorage.Value;

            await _db.SaveChangesAsync();
            return DeviceSettingsMapper.Map(s);
        }

        public async Task<DeviceSettingsResponse> ResetToDefaultsAsync(int tenantId)
        {
            var s = await GetOrCreateAsync(tenantId);

            // ── INPUT ────────────────────────────────────────────────────────────
            s.BarcodeScannerEnabled     = true;   s.BarcodeScannerSuffix      = "\r\n";
            s.BarcodeScannerMinLength   = 3;      s.BarcodeScannerThresholdMs = 50;
            s.QRScannerEnabled          = true;   s.CameraQREnabled           = false;
            s.WeighingScaleEnabled      = false;  s.WeighingScaleMode         = ScaleCommunicationMode.ManualEntry;
            s.WeighingScaleUnit         = WeightUnit.Kilogram;
            s.WeighingScaleDecimalPlaces = 3;     s.WeighingScaleBaudRate     = 9600;
            s.WeighingScalePort         = null;
            s.TouchscreenModeEnabled    = false;  s.RFIDEnabled               = false;
            s.BiometricEnabled          = false;  s.OcrScannerEnabled         = false;
            s.OcrProvider               = null;   s.OcrApiKey                 = null;

            // ── OUTPUT: Thermal Printer ──────────────────────────────────────────
            s.ThermalPrinterEnabled     = false;  s.ThermalPrinterName        = null;
            s.ThermalPaperSize          = PrinterPaperSize.Thermal80mm;
            s.ThermalPrinterConnection  = ThermalPrinterConnection.USB;
            s.ThermalPrinterPort        = null;   s.ThermalPrinterCopies      = 1;
            s.ThermalAutoPrint          = true;   s.ThermalOpenCashDrawer     = false;

            // ── OUTPUT: A4 Printer ───────────────────────────────────────────────
            s.A4PrinterEnabled          = false;  s.A4PrinterName             = null;
            s.A4PaperSize               = PrinterPaperSize.A4;
            s.A4ColorMode               = PrintColorMode.BlackAndWhite;
            s.A4PrinterCopies           = 1;      s.A4PrinterIsDefault        = false;

            // ── OUTPUT: Label Printer ────────────────────────────────────────────
            s.LabelPrinterEnabled       = false;  s.LabelPrinterName          = null;
            s.LabelWidthMm              = null;   s.LabelHeightMm             = null;
            s.LabelPerRow               = 1;      s.LabelShowBarcode          = true;
            s.LabelShowMRP              = true;   s.LabelShowBusiness         = true;

            // ── OUTPUT: Customer Display ─────────────────────────────────────────
            s.CustomerDisplayEnabled        = false;
            s.CustomerDisplayProtocol       = CustomerDisplayProtocol.Serial;
            s.CustomerDisplayPort           = null;
            s.CustomerDisplayWelcomeMessage = "Welcome!";
            s.CustomerDisplayShowShopName   = true;

            // ── OUTPUT: Speaker ──────────────────────────────────────────────────
            s.SpeakerEnabled            = false;  s.SpeakerBeepOnScan         = true;
            s.SpeakerAnnounceTotal      = false;  s.SpeakerAnnouncePayment    = false;

            // ── OUTPUT: Cash Drawer ──────────────────────────────────────────────
            s.CashDrawerEnabled         = false;
            s.CashDrawerConnectionType  = DeviceConnectionType.PrinterPort;
            s.CashDrawerPort            = null;
            s.CashDrawerTrigger         = CashDrawerTrigger.OnCashPayment;

            // ── PAYMENT ──────────────────────────────────────────────────────────
            s.CardTerminalEnabled       = false;  s.CardTerminalProvider      = null;
            s.CardTerminalApiKey        = null;   s.CardTerminalId            = null;
            s.UPIEnabled                = false;  s.UPIId                     = null;
            s.UPIMerchantName           = null;   s.UPIShowOnReceipt          = true;
            s.UPIShowOnCustomerDisplay  = false;
            s.PaymentGatewayEnabled     = false;  s.PaymentGatewayProvider    = null;
            s.PaymentGatewayApiKey      = null;   s.PaymentGatewayApiSecret   = null;
            s.PaymentGatewayWebhookUrl  = null;

            // ── DIGITAL ──────────────────────────────────────────────────────────
            s.EmailDeliveryEnabled      = false;  s.EmailAutoSendOnSale       = false;
            s.WhatsAppDeliveryEnabled   = false;  s.WhatsAppAutoSendOnSale    = false;
            s.SMSDeliveryEnabled        = false;  s.SMSAutoSendOnSale         = false;
            s.PDFEnabled                = true;   s.PDFAutoGenerate           = false;
            s.PDFSaveToStorage          = true;

            await _db.SaveChangesAsync();
            return DeviceSettingsMapper.Map(s);
        }

        private async Task<DeviceSettings> GetOrCreateAsync(int tenantId)
        {
            var s = await _db.DeviceSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId);
            if (s == null) { s = new DeviceSettings { TenantId = tenantId }; _db.DeviceSettings.Add(s); await _db.SaveChangesAsync(); }
            return s;
        }
    }

    // ==================== DEVICE SETTINGS MAPPER (shared static) ====================
    /// <summary>
    /// Shared mapping helper — used by both DeviceConfigurationService (status dashboard)
    /// and DeviceSettingsService to avoid duplication.
    /// </summary>
    public static class DeviceSettingsMapper
    {
        public static DeviceSettingsResponse Map(DeviceSettings s) => new()
        {
            Id = s.Id,

            // INPUT
            BarcodeScannerEnabled     = s.BarcodeScannerEnabled,
            BarcodeScannerSuffix      = s.BarcodeScannerSuffix,
            BarcodeScannerMinLength   = s.BarcodeScannerMinLength,
            BarcodeScannerThresholdMs = s.BarcodeScannerThresholdMs,
            QRScannerEnabled          = s.QRScannerEnabled,
            CameraQREnabled           = s.CameraQREnabled,
            WeighingScaleEnabled      = s.WeighingScaleEnabled,
            WeighingScaleMode         = s.WeighingScaleMode.ToString(),
            WeighingScaleUnit         = s.WeighingScaleUnit.ToString(),
            WeighingScaleDecimalPlaces = s.WeighingScaleDecimalPlaces,
            WeighingScalePort         = s.WeighingScalePort,
            WeighingScaleBaudRate     = s.WeighingScaleBaudRate,
            TouchscreenModeEnabled    = s.TouchscreenModeEnabled,
            RFIDEnabled               = s.RFIDEnabled,
            BiometricEnabled          = s.BiometricEnabled,
            OcrScannerEnabled         = s.OcrScannerEnabled,
            OcrProvider               = s.OcrProvider,

            // OUTPUT: Thermal Printer
            ThermalPrinterEnabled     = s.ThermalPrinterEnabled,
            ThermalPrinterName        = s.ThermalPrinterName,
            ThermalPaperSize          = s.ThermalPaperSize.ToString(),
            ThermalPrinterConnection  = s.ThermalPrinterConnection.ToString(),
            ThermalPrinterPort        = s.ThermalPrinterPort,
            ThermalPrinterCopies      = s.ThermalPrinterCopies,
            ThermalAutoPrint          = s.ThermalAutoPrint,
            ThermalOpenCashDrawer     = s.ThermalOpenCashDrawer,

            // OUTPUT: A4 Printer
            A4PrinterEnabled          = s.A4PrinterEnabled,
            A4PrinterName             = s.A4PrinterName,
            A4PaperSize               = s.A4PaperSize.ToString(),
            A4ColorMode               = s.A4ColorMode.ToString(),
            A4PrinterCopies           = s.A4PrinterCopies,
            A4PrinterIsDefault        = s.A4PrinterIsDefault,

            // OUTPUT: Label Printer
            LabelPrinterEnabled       = s.LabelPrinterEnabled,
            LabelPrinterName          = s.LabelPrinterName,
            LabelWidthMm              = s.LabelWidthMm,
            LabelHeightMm             = s.LabelHeightMm,
            LabelPerRow               = s.LabelPerRow,
            LabelShowBarcode          = s.LabelShowBarcode,
            LabelShowMRP              = s.LabelShowMRP,
            LabelShowBusiness         = s.LabelShowBusiness,

            // OUTPUT: Customer Display
            CustomerDisplayEnabled        = s.CustomerDisplayEnabled,
            CustomerDisplayProtocol       = s.CustomerDisplayProtocol.ToString(),
            CustomerDisplayPort           = s.CustomerDisplayPort,
            CustomerDisplayWelcomeMessage = s.CustomerDisplayWelcomeMessage,
            CustomerDisplayShowShopName   = s.CustomerDisplayShowShopName,

            // OUTPUT: Speaker
            SpeakerEnabled            = s.SpeakerEnabled,
            SpeakerBeepOnScan         = s.SpeakerBeepOnScan,
            SpeakerAnnounceTotal      = s.SpeakerAnnounceTotal,
            SpeakerAnnouncePayment    = s.SpeakerAnnouncePayment,

            // OUTPUT: Cash Drawer
            CashDrawerEnabled         = s.CashDrawerEnabled,
            CashDrawerConnectionType  = s.CashDrawerConnectionType.ToString(),
            CashDrawerPort            = s.CashDrawerPort,
            CashDrawerTrigger         = s.CashDrawerTrigger.ToString(),

            // PAYMENT
            CardTerminalEnabled       = s.CardTerminalEnabled,
            CardTerminalProvider      = s.CardTerminalProvider,
            CardTerminalId            = s.CardTerminalId,
            UPIEnabled                = s.UPIEnabled,
            UPIId                     = s.UPIId,
            UPIMerchantName           = s.UPIMerchantName,
            UPIShowOnReceipt          = s.UPIShowOnReceipt,
            UPIShowOnCustomerDisplay  = s.UPIShowOnCustomerDisplay,
            PaymentGatewayEnabled     = s.PaymentGatewayEnabled,
            PaymentGatewayProvider    = s.PaymentGatewayProvider,
            PaymentGatewayWebhookUrl  = s.PaymentGatewayWebhookUrl,

            // DIGITAL
            EmailDeliveryEnabled      = s.EmailDeliveryEnabled,
            EmailAutoSendOnSale       = s.EmailAutoSendOnSale,
            WhatsAppDeliveryEnabled   = s.WhatsAppDeliveryEnabled,
            WhatsAppAutoSendOnSale    = s.WhatsAppAutoSendOnSale,
            SMSDeliveryEnabled        = s.SMSDeliveryEnabled,
            SMSAutoSendOnSale         = s.SMSAutoSendOnSale,
            PDFEnabled                = s.PDFEnabled,
            PDFAutoGenerate           = s.PDFAutoGenerate,
            PDFSaveToStorage          = s.PDFSaveToStorage
        };
    }

    // ==================== BARCODE SCANNER SERVICE ====================
    public class BarcodeScannerService : IBarcodeScannerService
    {
        private readonly ApplicationDbContext _db;
        public BarcodeScannerService(ApplicationDbContext db) => _db = db;

        public async Task<BarcodeScanResponse> ProcessScanAsync(BarcodeScanRequest request, int tenantId, int userId)
        {
            var barcodeValue = request.BarcodeValue.Trim('\r', '\n', '\t').Trim();

            var product = await _db.Products
                .Include(p => p.Category).Include(p => p.Unit).Include(p => p.TaxRate)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == barcodeValue && !p.IsDeleted);

            bool isResolved  = product != null;
            string? unresolved = product == null ? "Product not found for barcode: " + barcodeValue : null;

            var log = new ScanEventLog
            {
                TenantId              = tenantId,
                ScannedValue          = barcodeValue,
                Module                = request.Module,
                ResolvedProductId     = product?.Id,
                IsResolved            = isResolved,
                UnresolvedReason      = unresolved,
                DeviceConfigurationId = request.DeviceConfigurationId,
                UserId                = userId,
                ScannedAt             = DateTime.UtcNow
            };
            _db.ScanEventLogs.Add(log);
            await _db.SaveChangesAsync();

            return new BarcodeScanResponse
            {
                ScannedValue     = barcodeValue,
                IsResolved       = isResolved,
                UnresolvedReason = unresolved,
                Module           = request.Module.ToString(),
                Product          = product == null ? null : MapProduct(product),
                ScanEventLogId   = log.Id
            };
        }

        public async Task<ProductResponse?> LookupProductByBarcodeAsync(string barcodeValue, int tenantId)
        {
            var p = await _db.Products
                .Include(p => p.Category).Include(p => p.Unit).Include(p => p.TaxRate)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == barcodeValue.Trim() && !p.IsDeleted);
            return p == null ? null : MapProduct(p);
        }

        public async Task<(List<ScanEventLogResponse> Items, int TotalCount)> GetScanHistoryAsync(
            int tenantId, ScanModule? module, PaginationRequest pagination)
        {
            var query   = _db.ScanEventLogs.Where(s => s.TenantId == tenantId)
                .Include(s => s.ResolvedProduct).Include(s => s.User).Include(s => s.DeviceConfiguration);
            var filtered = module.HasValue ? query.Where(s => s.Module == module.Value) : query;
            var total   = await filtered.CountAsync();
            var items   = await filtered.OrderByDescending(s => s.ScannedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(s => MapScanLog(s)).ToListAsync();
            return (items, total);
        }

        public async Task<List<ScanEventLogResponse>> GetRecentScansAsync(int tenantId, int count = 10)
            => await _db.ScanEventLogs.Where(s => s.TenantId == tenantId)
                .Include(s => s.ResolvedProduct).Include(s => s.User)
                .OrderByDescending(s => s.ScannedAt).Take(count)
                .Select(s => MapScanLog(s)).ToListAsync();

        private static ProductResponse MapProduct(Product p) => new()
        {
            Id = p.Id, Name = p.Name, SKU = p.SKU, Barcode = p.Barcode, Description = p.Description,
            HSNCode = p.HSNCode, CategoryName = p.Category?.Name, UnitName = p.Unit?.ShortName,
            PurchasePrice = p.PurchasePrice, SellingPrice = p.SellingPrice, MRP = p.MRP,
            CurrentStock = p.CurrentStock, TaxPercentage = p.TaxRate?.Percentage, IsService = p.IsService
        };

        private static ScanEventLogResponse MapScanLog(ScanEventLog s) => new()
        {
            Id = s.Id, ScannedValue = s.ScannedValue, Module = s.Module.ToString(),
            IsResolved = s.IsResolved, UnresolvedReason = s.UnresolvedReason,
            ResolvedProductId = s.ResolvedProductId, ResolvedProductName = s.ResolvedProduct?.Name,
            DeviceConfigurationId = s.DeviceConfigurationId, DeviceName = s.DeviceConfiguration?.Name,
            UserId = s.UserId, UserName = s.User?.FullName, ScannedAt = s.ScannedAt
        };
    }

    // ==================== QR SCANNER SERVICE ====================
    public class QRScannerService : IQRScannerService
    {
        private readonly ApplicationDbContext _db;
        public QRScannerService(ApplicationDbContext db) => _db = db;

        public async Task<QRScanResponse> ProcessScanAsync(QRScanRequest request, int tenantId, int userId)
        {
            var content = request.QRContent.Trim();
            var purpose = request.Purpose;
            if (purpose == QRScanPurpose.ProductIdentification && IsUPIQR(content))
                purpose = QRScanPurpose.UPIPayment;

            Product? product        = null;
            UPIPaymentInfo? upiInfo = null;
            bool isResolved         = false;
            string? unresolvedReason = null;

            switch (purpose)
            {
                case QRScanPurpose.ProductIdentification:
                    product = await _db.Products
                        .Include(p => p.Category).Include(p => p.Unit).Include(p => p.TaxRate)
                        .FirstOrDefaultAsync(p => p.TenantId == tenantId && !p.IsDeleted &&
                            (p.Barcode == content || p.SKU == content));
                    isResolved = product != null;
                    if (!isResolved) unresolvedReason = "Product not found for QR content.";
                    break;

                case QRScanPurpose.UPIPayment:
                    upiInfo    = ParseUPIString(content);
                    isResolved = upiInfo != null;
                    if (!isResolved) unresolvedReason = "Could not parse UPI payment string.";
                    break;

                default:
                    isResolved = true;
                    break;
            }

            var evt = new QRScanEvent
            {
                TenantId = tenantId, QRContent = content, Purpose = purpose,
                Module = request.Module, ResolvedProductId = product?.Id,
                UPIString = purpose == QRScanPurpose.UPIPayment ? content : null,
                UPIAmount = upiInfo?.Amount, IsFromCamera = request.IsFromCamera,
                IsResolved = isResolved, UnresolvedReason = unresolvedReason,
                UserId = userId, ScannedAt = DateTime.UtcNow
            };
            _db.QRScanEvents.Add(evt);
            await _db.SaveChangesAsync();

            return new QRScanResponse
            {
                QRContent = content, Purpose = purpose.ToString(), Module = request.Module.ToString(),
                IsResolved = isResolved, UnresolvedReason = unresolvedReason,
                Product = product == null ? null : MapProduct(product),
                UPIInfo = upiInfo, QRScanEventId = evt.Id
            };
        }

        public Task<UPIPaymentInfo> ParseUPIQRAsync(string qrContent)
            => Task.FromResult(ParseUPIString(qrContent) ?? new UPIPaymentInfo { UPIString = qrContent });

        public async Task<List<QRScanEvent>> GetScanHistoryAsync(int tenantId, PaginationRequest pagination)
            => await _db.QRScanEvents.Where(e => e.TenantId == tenantId)
                .OrderByDescending(e => e.ScannedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .ToListAsync();

        private static bool IsUPIQR(string c)
            => c.StartsWith("upi://", StringComparison.OrdinalIgnoreCase) || c.Contains("pa=", StringComparison.OrdinalIgnoreCase);

        private static UPIPaymentInfo? ParseUPIString(string content)
        {
            try
            {
                var uri   = content.StartsWith("upi://", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(content) : new Uri("upi://pay?" + content.Split('?').Last());
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return new UPIPaymentInfo
                {
                    UPIString = content, UPIId = query["pa"], PayeeName = query["pn"],
                    Amount = decimal.TryParse(query["am"], out var a) ? a : null,
                    TransactionNote = query["tn"]
                };
            }
            catch { return null; }
        }

        private static ProductResponse MapProduct(Product p) => new()
        {
            Id = p.Id, Name = p.Name, SKU = p.SKU, Barcode = p.Barcode,
            PurchasePrice = p.PurchasePrice, SellingPrice = p.SellingPrice,
            CurrentStock = p.CurrentStock, CategoryName = p.Category?.Name,
            UnitName = p.Unit?.ShortName, TaxPercentage = p.TaxRate?.Percentage, IsService = p.IsService
        };
    }

    // ==================== WEIGHING SCALE SERVICE ====================
    public class WeighingScaleService : IWeighingScaleService
    {
        private readonly ApplicationDbContext _db;
        public WeighingScaleService(ApplicationDbContext db) => _db = db;

        public async Task<WeightCaptureResponse> CaptureWeightAsync(WeightCaptureRequest request, int tenantId, int userId)
        {
            decimal computedAmount = request.RatePerUnit.HasValue
                ? Math.Round(request.Weight * request.RatePerUnit.Value, 2) : 0;

            var log = new WeightCaptureLog
            {
                TenantId    = tenantId, Weight = request.Weight, WeightUnit = request.WeightUnit,
                ProductId   = request.ProductId, Module = request.Module, CaptureMode = request.CaptureMode,
                RatePerUnit = request.RatePerUnit,
                ComputedAmount = request.RatePerUnit.HasValue ? computedAmount : null,
                UserId = userId, CapturedAt = DateTime.UtcNow
            };
            _db.WeightCaptureLogs.Add(log);
            await _db.SaveChangesAsync();

            string? productName = null;
            if (request.ProductId.HasValue)
            {
                var prod = await _db.Products.FindAsync(request.ProductId.Value);
                productName = prod?.Name;
            }

            string unitAbbr     = GetUnitAbbr(request.WeightUnit);
            string displayText  = request.RatePerUnit.HasValue
                ? $"{request.Weight:F3} {unitAbbr} × ₹{request.RatePerUnit.Value:F2}/{unitAbbr} = ₹{computedAmount:F2}"
                : $"{request.Weight:F3} {unitAbbr}";

            return new WeightCaptureResponse
            {
                Id = log.Id, Weight = request.Weight, WeightUnit = request.WeightUnit.ToString(),
                ProductId = request.ProductId, ProductName = productName,
                Module = request.Module.ToString(), CaptureMode = request.CaptureMode.ToString(),
                RatePerUnit = request.RatePerUnit,
                ComputedAmount = request.RatePerUnit.HasValue ? computedAmount : null,
                DisplayText = displayText, CapturedAt = log.CapturedAt
            };
        }

        public Task<decimal> ConvertWeightAsync(decimal weight, WeightUnit fromUnit, WeightUnit toUnit)
        {
            decimal inGrams = fromUnit switch
            {
                WeightUnit.Gram => weight, WeightUnit.Kilogram => weight * 1000,
                WeightUnit.Pound => weight * 453.592m, WeightUnit.Ounce => weight * 28.3495m,
                WeightUnit.Milligram => weight / 1000, _ => weight
            };
            decimal result = toUnit switch
            {
                WeightUnit.Gram => inGrams, WeightUnit.Kilogram => inGrams / 1000,
                WeightUnit.Pound => inGrams / 453.592m, WeightUnit.Ounce => inGrams / 28.3495m,
                WeightUnit.Milligram => inGrams * 1000, _ => inGrams
            };
            return Task.FromResult(Math.Round(result, 6));
        }

        public async Task<List<WeightCaptureResponse>> GetRecentCapturesAsync(int tenantId, int count = 20)
        {
            var logs = await _db.WeightCaptureLogs.Where(w => w.TenantId == tenantId)
                .Include(w => w.Product).OrderByDescending(w => w.CapturedAt).Take(count).ToListAsync();
            return logs.Select(MapCaptureLog).ToList();
        }

        public async Task<(List<WeightCaptureResponse> Items, int TotalCount)> GetCaptureHistoryAsync(
            int tenantId, ScanModule? module, PaginationRequest pagination)
        {
            var query   = _db.WeightCaptureLogs.Where(w => w.TenantId == tenantId).Include(w => w.Product);
            var filtered = module.HasValue ? query.Where(w => w.Module == module.Value) : query;
            var total   = await filtered.CountAsync();
            var items   = await filtered.OrderByDescending(w => w.CapturedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return (items.Select(MapCaptureLog).ToList(), total);
        }

        private static string GetUnitAbbr(WeightUnit u) => u switch
        {
            WeightUnit.Gram => "g", WeightUnit.Kilogram => "kg", WeightUnit.Pound => "lb",
            WeightUnit.Ounce => "oz", WeightUnit.Milligram => "mg", _ => "kg"
        };

        private static WeightCaptureResponse MapCaptureLog(WeightCaptureLog w)
        {
            string abbr = GetUnitAbbr(w.WeightUnit);
            string txt  = w.RatePerUnit.HasValue
                ? $"{w.Weight:F3} {abbr} × ₹{w.RatePerUnit.Value:F2}/{abbr} = ₹{w.ComputedAmount:F2}"
                : $"{w.Weight:F3} {abbr}";
            return new WeightCaptureResponse
            {
                Id = w.Id, Weight = w.Weight, WeightUnit = w.WeightUnit.ToString(),
                ProductId = w.ProductId, ProductName = w.Product?.Name,
                Module = w.Module.ToString(), CaptureMode = w.CaptureMode.ToString(),
                RatePerUnit = w.RatePerUnit, ComputedAmount = w.ComputedAmount,
                DisplayText = txt, CapturedAt = w.CapturedAt
            };
        }
    }

    // ==================== OCR SCANNER SERVICE ====================
    public class OcrScannerService : IOcrScannerService
    {
        private readonly ApplicationDbContext _db;
        public OcrScannerService(ApplicationDbContext db) => _db = db;

        public async Task<OcrScanResponse> ProcessDocumentAsync(OcrScanRequest request, int tenantId, int userId)
        {
            var result = new OcrScanResult
            {
                TenantId        = tenantId, DocumentType = request.DocumentType,
                DocumentFileUrl = request.DocumentFileUrl, IsUsed = false,
                OcrProvider     = "Tesseract",   // ← swap for real provider call
                UserId          = userId, ScannedAt = DateTime.UtcNow
            };
            _db.OcrScanResults.Add(result);
            await _db.SaveChangesAsync();

            return new OcrScanResponse
            {
                Id = result.Id, DocumentType = result.DocumentType.ToString(),
                DocumentFileUrl = result.DocumentFileUrl, ParsedItems = new(),
                OcrProvider = result.OcrProvider, IsUsed = false, ScannedAt = result.ScannedAt
            };
        }

        public async Task<OcrScanResponse?> GetOcrResultAsync(int id, int tenantId)
        {
            var r = await _db.OcrScanResults.Include(o => o.LinkedPurchase)
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
            if (r == null) return null;
            var items = string.IsNullOrEmpty(r.ParsedDataJson)
                ? new List<OcrLineItem>()
                : JsonSerializer.Deserialize<List<OcrLineItem>>(r.ParsedDataJson) ?? new();
            return new OcrScanResponse
            {
                Id = r.Id, DocumentType = r.DocumentType.ToString(), DocumentFileUrl = r.DocumentFileUrl,
                ExtractedText = r.ExtractedText, ParsedItems = items,
                ConfidenceScore = r.ConfidenceScore, OcrProvider = r.OcrProvider,
                IsUsed = r.IsUsed, CreatedPurchaseId = r.LinkedPurchaseId, ScannedAt = r.ScannedAt
            };
        }

        public async Task<bool> MarkAsUsedAsync(int id, int purchaseId, int tenantId)
        {
            var r = await _db.OcrScanResults.FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"OCR result {id} not found.");
            r.IsUsed = true; r.LinkedPurchaseId = purchaseId;
            await _db.SaveChangesAsync(); return true;
        }

        public async Task<(List<OcrScanResult> Items, int TotalCount)> GetScanHistoryAsync(
            int tenantId, OcrDocumentType? docType, PaginationRequest pagination)
        {
            var query   = _db.OcrScanResults.Where(o => o.TenantId == tenantId);
            if (docType.HasValue) query = query.Where(o => o.DocumentType == docType.Value);
            var total   = await query.CountAsync();
            var items   = await query.OrderByDescending(o => o.ScannedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return (items, total);
        }

        public async Task<List<OcrLineItem>> MatchProductsAsync(List<OcrLineItem> items, int tenantId)
        {
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Barcode) && string.IsNullOrWhiteSpace(item.ProductName)) continue;
                Product? match = null;
                if (!string.IsNullOrWhiteSpace(item.Barcode))
                    match = await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == item.Barcode && !p.IsDeleted);
                if (match == null && !string.IsNullOrWhiteSpace(item.ProductName))
                    match = await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Name.ToLower().Contains(item.ProductName!.ToLower()) && !p.IsDeleted);
                if (match != null) { item.MatchedProductId = match.Id; item.MatchedProductName = match.Name; }
            }
            return items;
        }
    }

    // ==================== CARD PAYMENT SERVICE ====================
    public class CardPaymentService : ICardPaymentService
    {
        private readonly ApplicationDbContext _db;
        public CardPaymentService(ApplicationDbContext db) => _db = db;

        public async Task<CardPaymentTransactionResponse> InitiatePaymentAsync(InitiateCardPaymentRequest request, int tenantId, int userId)
        {
            var settings = await _db.DeviceSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            var txn = new CardPaymentTransaction
            {
                TenantId = tenantId, InvoiceId = request.InvoiceId, Amount = request.Amount,
                Status = CardPaymentStatus.Initiated,
                TerminalProvider = settings?.CardTerminalProvider, TerminalId = settings?.CardTerminalId,
                UserId = userId, InitiatedAt = DateTime.UtcNow
            };
            _db.CardPaymentTransactions.Add(txn);
            await _db.SaveChangesAsync();
            return MapTransaction(txn);
        }

        public async Task<CardPaymentTransactionResponse> UpdatePaymentStatusAsync(int transactionId, UpdateCardPaymentStatusRequest request, int tenantId)
        {
            var txn = await _db.CardPaymentTransactions.FirstOrDefaultAsync(t => t.Id == transactionId && t.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Card payment transaction {transactionId} not found.");
            txn.Status = request.Status; txn.ApprovalCode = request.ApprovalCode;
            txn.ProviderTransactionId = request.ProviderTransactionId; txn.CardType = request.CardType;
            txn.CardLast4 = request.CardLast4; txn.RawResponseJson = request.RawResponseJson;
            txn.ErrorMessage = request.ErrorMessage;
            if (request.Status is CardPaymentStatus.Approved or CardPaymentStatus.Declined or CardPaymentStatus.Cancelled or CardPaymentStatus.Error)
                txn.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapTransaction(txn);
        }

        public async Task<CardPaymentTransactionResponse?> GetTransactionAsync(int id, int tenantId)
        {
            var txn = await _db.CardPaymentTransactions.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            return txn == null ? null : MapTransaction(txn);
        }

        public async Task<List<CardPaymentTransactionResponse>> GetTransactionsByInvoiceAsync(int invoiceId, int tenantId)
            => await _db.CardPaymentTransactions.Where(t => t.InvoiceId == invoiceId && t.TenantId == tenantId)
                .OrderByDescending(t => t.InitiatedAt).Select(t => MapTransaction(t)).ToListAsync();

        public async Task<bool> CancelTransactionAsync(int id, int tenantId)
        {
            var txn = await _db.CardPaymentTransactions.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Transaction {id} not found.");
            if (txn.Status != CardPaymentStatus.Initiated)
                throw new InvalidOperationException("Only initiated transactions can be cancelled.");
            txn.Status = CardPaymentStatus.Cancelled; txn.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return true;
        }

        private static CardPaymentTransactionResponse MapTransaction(CardPaymentTransaction t) => new()
        {
            Id = t.Id, InvoiceId = t.InvoiceId, Amount = t.Amount, Status = t.Status.ToString(),
            ApprovalCode = t.ApprovalCode, ProviderTransactionId = t.ProviderTransactionId,
            CardType = t.CardType, CardLast4 = t.CardLast4, TerminalProvider = t.TerminalProvider,
            TerminalId = t.TerminalId, ErrorMessage = t.ErrorMessage,
            InitiatedAt = t.InitiatedAt, CompletedAt = t.CompletedAt
        };
    }

    // ==================== CASH DRAWER SERVICE ====================
    public class CashDrawerService : ICashDrawerService
    {
        private readonly ApplicationDbContext _db;
        public CashDrawerService(ApplicationDbContext db) => _db = db;

        public async Task<bool> OpenDrawerAsync(OpenCashDrawerRequest request, int tenantId, int userId)
        {
            var settings = await _db.DeviceSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            if (settings == null || !settings.CashDrawerEnabled) return false;
            // ESC/POS command: byte[] cmd = { 0x1B, 0x70, 0x00, 0x19, 0xFA };
            await Task.CompletedTask;
            return true;
        }

        public async Task<string> GetDrawerStatusAsync(int tenantId)
        {
            await Task.CompletedTask;
            return "Unknown";
        }
    }
}
