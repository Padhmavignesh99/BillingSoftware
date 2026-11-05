using MasterBilling.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MasterBilling.API.DTOs.Requests
{
    // ==================== AUTH ====================
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BusinessName { get; set; }
        public int? BusinessTypeId { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    // ==================== USER ====================
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public UserRole Role { get; set; } = UserRole.Staff;
    }

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public UserRole Role { get; set; }
    }

    // ==================== ROLE / PERMISSION ====================
    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int>? PermissionIds { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<int>? PermissionIds { get; set; }
    }

    public class CreatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? Description { get; set; }
    }

    // ==================== STAFF ====================
    public class CreateStaffRequest
    {
        public int UserId { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? BranchId { get; set; }
    }

    public class UpdateStaffRequest
    {
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public int? BranchId { get; set; }
    }

    // ==================== BUSINESS ====================
    public class CreateBusinessRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public int? BusinessTypeId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
    }

    public class UpdateBusinessRequest
    {
        public string? Name { get; set; }
        public string? LegalName { get; set; }
        public int? BusinessTypeId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
    }

    public class CreateBusinessTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    // ==================== BRANCH ====================
    public class CreateBranchRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsMainBranch { get; set; } = false;
    }

    public class UpdateBranchRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    // ==================== BUSINESS SETTINGS ====================
    public class UpdateBusinessSettingsRequest
    {
        public string? DateFormat { get; set; }
        public string? TimeZone { get; set; }
        public string? Language { get; set; }
        public bool? EnableGST { get; set; }
        public bool? EnableMultiBranch { get; set; }
        public bool? EnableBarcode { get; set; }
        public bool? EnableLowStockAlert { get; set; }
        public int? LowStockThreshold { get; set; }
        public bool? EnableEmailNotifications { get; set; }
        public bool? EnableWhatsAppNotifications { get; set; }
        public bool? EnableSmsNotifications { get; set; }
    }

    public class UpdateInvoiceSettingsRequest
    {
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNumberFormat { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? FooterNote { get; set; }
        public bool? ShowLogo { get; set; }
        public bool? ShowGST { get; set; }
        public bool? ShowDiscount { get; set; }
        public bool? ShowSignature { get; set; }
        public string? SignatureImageUrl { get; set; }
        public string? TemplateName { get; set; }
        public string? PrimaryColor { get; set; }
    }

    // ==================== PRODUCT ====================
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? HSNCode { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? UnitId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? MRP { get; set; }
        public int? TaxRateId { get; set; }
        public int? DiscountId { get; set; }
        public decimal? CurrentStock { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsService { get; set; } = false;
        /// <summary>Optional expiry date for the product (FMCG, pharmacy, grocery).</summary>
        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? HSNCode { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? UnitId { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? MRP { get; set; }
        public int? TaxRateId { get; set; }
        public int? DiscountId { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsService { get; set; }
        /// <summary>Optional expiry date for the product (FMCG, pharmacy, grocery).</summary>
        public DateTime? ExpiryDate { get; set; }
    }

    public class BulkProductUploadRequest
    {
        public List<CreateProductRequest> Products { get; set; } = new();
    }

    // ==================== CATEGORY ====================
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public int? SortOrder { get; set; }
    }

    // ==================== BRAND ====================
    public class CreateBrandRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
    }

    // ==================== UNIT ====================
    public class CreateUnitRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // ==================== TAX RATE ====================
    public class CreateTaxRateRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string? Description { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateTaxRateRequest
    {
        public string? Name { get; set; }
        public decimal? Percentage { get; set; }
        public string? Description { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public bool? IsDefault { get; set; }
    }

    // ==================== DISCOUNT ====================
    public class CreateDiscountRequest
    {
        public string Name { get; set; } = string.Empty;
        public DiscountType Type { get; set; } = DiscountType.Percentage;
        public decimal Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string? CouponCode { get; set; }
    }

    public class UpdateDiscountRequest
    {
        public string? Name { get; set; }
        public DiscountType? Type { get; set; }
        public decimal? Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string? CouponCode { get; set; }
    }

    // ==================== BARCODE ====================
    public class GenerateBarcodeRequest
    {
        public int ProductId { get; set; }
        public string? BarcodeFormat { get; set; } = "Code128";
    }

    // ==================== PRINT SETTINGS ====================
    /// <summary>Full update / upsert of all print settings.</summary>
    public class UpdatePrintSettingsRequest
    {
        // Paper
        public string? PaperSize             { get; set; }
        public decimal? CustomPaperWidthMm   { get; set; }
        public decimal? CustomPaperHeightMm  { get; set; }
        public string? Orientation           { get; set; }

        // Margins
        public decimal? MarginTopMm    { get; set; }
        public decimal? MarginBottomMm { get; set; }
        public decimal? MarginLeftMm   { get; set; }
        public decimal? MarginRightMm  { get; set; }

        // Typography
        public string?  FontFamily        { get; set; }
        public decimal? FontSizePt        { get; set; }
        public decimal? HeaderFontSizePt  { get; set; }
        public decimal? FooterFontSizePt  { get; set; }

        // Invoice display options
        public bool? ShowBusinessLogo       { get; set; }
        public bool? ShowBusinessAddress    { get; set; }
        public bool? ShowGSTDetails         { get; set; }
        public bool? ShowCustomerAddress    { get; set; }
        public bool? ShowItemHSNCode        { get; set; }
        public bool? ShowItemDiscount       { get; set; }
        public bool? ShowTaxBreakdown       { get; set; }
        public bool? ShowRoundOff           { get; set; }
        public bool? ShowAmountInWords      { get; set; }
        public bool? ShowTermsAndConditions { get; set; }
        public bool? ShowFooterNote         { get; set; }
        public bool? ShowSignatureLine      { get; set; }
        public bool? ShowQRCode             { get; set; }
        public bool? ShowBarcode            { get; set; }
        public bool? ShowPaymentDetails     { get; set; }
        public bool? ShowDueAmount          { get; set; }
        public bool? ShowBankDetails        { get; set; }
        public string? BankDetails          { get; set; }
        public string? UpiId                { get; set; }

        // Copies
        public int? InvoiceCopies   { get; set; }
        public int? QuotationCopies { get; set; }
        public int? PurchaseCopies  { get; set; }
        public int? ReceiptCopies   { get; set; }

        // Copy labels
        public string? InvoiceCopyLabel1 { get; set; }
        public string? InvoiceCopyLabel2 { get; set; }
        public string? InvoiceCopyLabel3 { get; set; }

        // Thermal / POS
        public bool?   ThermalPrintEnabled     { get; set; }
        public string? ThermalPaperWidth       { get; set; }
        public bool?   ThermalShowLogo         { get; set; }
        public bool?   ThermalShowQRCode       { get; set; }
        public bool?   ThermalShowItemCode     { get; set; }
        public bool?   ThermalShowCustomerName { get; set; }
        public bool?   ThermalAutocut          { get; set; }
        public int?    ThermalLineFeedCount    { get; set; }

        // Label
        public bool?   LabelPrintEnabled   { get; set; }
        public string? LabelPaperSize      { get; set; }
        public bool?   LabelShowBarcode    { get; set; }
        public bool?   LabelShowMRP        { get; set; }
        public bool?   LabelShowProductName{ get; set; }
        public bool?   LabelShowBusiness   { get; set; }
        public int?    LabelsPerRow        { get; set; }

        // Report
        public bool? ShowReportHeader   { get; set; }
        public bool? ShowReportFooter   { get; set; }
        public bool? ShowPageNumbers    { get; set; }
        public bool? ShowPrintDateTime  { get; set; }
        public bool? ShowReportLogo     { get; set; }

        // Color / branding
        public string? PrintPrimaryColor { get; set; }
        public bool?   PrintInColor      { get; set; }

        // Default printers
        public string? DefaultPrinterName    { get; set; }
        public string? DefaultThermalPrinter { get; set; }
        public string? DefaultLabelPrinter   { get; set; }

        // ── Network / Direct Printer Configuration ──────────────────────────────
        /// <summary>Network printer IP address (e.g. "192.168.1.50")</summary>
        public string? PrinterIpAddress    { get; set; }
        /// <summary>Network printer port (default 9100 for ESC/POS raw printing)</summary>
        public int?    PrinterPort         { get; set; }
        /// <summary>USB | Network | Bluetooth | Cloud</summary>
        public string? PrinterConnection   { get; set; }
        /// <summary>Open cash drawer after printing</summary>
        public bool?   OpenCashDrawer      { get; set; }
        /// <summary>Footer text shown at the bottom of every thermal receipt</summary>
        public string? FooterText          { get; set; }

        // ── Branch / Counter Scope ───────────────────────────────────────────────
        /// <summary>null = save as tenant-level default; set to target a specific branch</summary>
        public int?    BranchId            { get; set; }
        /// <summary>null = branch or tenant level; set to target a specific billing counter</summary>
        public int?    CounterId           { get; set; }
    }

    /// <summary>
    /// Patch (partial update) request — same fields as UpdatePrintSettingsRequest; only non-null values are applied.
    /// Keeping as a distinct class allows different Swagger docs and validation rules in the future.
    /// </summary>
    public class PatchPrintSettingsRequest : UpdatePrintSettingsRequest { }

    // ==================== PRINT JOB REQUESTS ====================

    /// <summary>Queue a new print job for a document.</summary>
    public class QueuePrintJobRequest
    {
        /// <summary>Invoice | Receipt | Quotation | Purchase | SalesReturn | Report | Label</summary>
        [Required]
        public string DocumentType       { get; set; } = "Invoice";

        /// <summary>Primary key of the source document</summary>
        public int?   DocumentId         { get; set; }

        /// <summary>Human-readable document number (e.g. "INV-2024-00001")</summary>
        public string? DocumentReference { get; set; }

        /// <summary>Printer name, "IP:Port", or "Browser" / "PDF"</summary>
        public string? PrinterTarget     { get; set; }

        /// <summary>Paper size override for this job (null = use tenant default)</summary>
        public string? PaperSize         { get; set; }

        /// <summary>Number of copies (default 1)</summary>
        [Range(1, 10)]
        public int    Copies             { get; set; } = 1;

        /// <summary>Maximum retry attempts on failure</summary>
        [Range(0, 10)]
        public int    MaxRetries         { get; set; } = 3;

        public int?   BranchId          { get; set; }
        public int?   CounterId         { get; set; }
    }

    /// <summary>Request body for the printer connectivity test endpoint.</summary>
    public class PrinterTestRequest
    {
        /// <summary>Printer name or IP:Port to test</summary>
        [Required]
        public string PrinterTarget   { get; set; } = string.Empty;

        /// <summary>USB | Network | Bluetooth | Cloud</summary>
        public string ConnectionType  { get; set; } = "USB";

        /// <summary>Network printer IP (required when ConnectionType = Network)</summary>
        public string? IpAddress      { get; set; }

        /// <summary>Network printer port (default 9100)</summary>
        public int     Port           { get; set; } = 9100;

        /// <summary>TCP connect timeout in milliseconds (default 3000)</summary>
        [Range(500, 30000)]
        public int     TimeoutMs      { get; set; } = 3000;
    }

    // ==================== THEME SETTINGS ====================

    /// <summary>
    /// Full-replacement request for a user's theme settings.
    /// All required fields must be supplied; optional fields may be null (removes override).
    /// </summary>
    public class UpdateThemeSettingsRequest
    {
        // Color mode
        /// <summary>Light | Dark | System</summary>
        public string ColorMode { get; set; } = "Light";

        // Palette
        public string PrimaryColor   { get; set; } = "#1a73e8";
        public string SecondaryColor { get; set; } = "#fbbc04";
        public string SuccessColor   { get; set; } = "#34a853";
        public string WarningColor   { get; set; } = "#ea8600";
        public string DangerColor    { get; set; } = "#ea4335";

        // Background & surfaces (null = use theme default)
        public string? BackgroundColor { get; set; }
        public string? SurfaceColor    { get; set; }
        public string? SidebarColor    { get; set; }
        public string? NavbarColor     { get; set; }

        // Typography
        public string FontFamily { get; set; } = "Inter";
        /// <summary>sm | md | lg</summary>
        public string FontSize   { get; set; } = "md";

        // Layout
        /// <summary>Vertical | Horizontal | Mini | Compact</summary>
        public string SidebarLayout   { get; set; } = "Vertical";
        public bool   SidebarCollapsed { get; set; } = false;
        /// <summary>Fluid | Boxed</summary>
        public string ContentWidth    { get; set; } = "Fluid";

        // Density
        /// <summary>Comfortable | Compact | Cozy</summary>
        public string Density { get; set; } = "Comfortable";

        // Visual effects
        public bool EnableAnimations { get; set; } = true;
        public bool EnableShadows    { get; set; } = true;
        public bool EnableGradients  { get; set; } = false;

        /// <summary>None | Sm | Md | Lg | Full</summary>
        public string BorderRadius { get; set; } = "Md";

        // Preset
        /// <summary>Default | Ocean | Sunset | Forest | Midnight | null for full custom</summary>
        public string? PresetTheme { get; set; } = "Default";
    }

    /// <summary>
    /// Partial-update request for theme settings.
    /// Only non-null fields are applied (PATCH semantics).
    /// </summary>
    public class PatchThemeSettingsRequest
    {
        public string? ColorMode      { get; set; }
        public string? PrimaryColor   { get; set; }
        public string? SecondaryColor { get; set; }
        public string? SuccessColor   { get; set; }
        public string? WarningColor   { get; set; }
        public string? DangerColor    { get; set; }
        public string? BackgroundColor { get; set; }
        public string? SurfaceColor    { get; set; }
        public string? SidebarColor    { get; set; }
        public string? NavbarColor     { get; set; }
        public string? FontFamily      { get; set; }
        public string? FontSize        { get; set; }
        public string? SidebarLayout   { get; set; }
        public bool?   SidebarCollapsed { get; set; }
        public string? ContentWidth    { get; set; }
        public string? Density         { get; set; }
        public bool?   EnableAnimations { get; set; }
        public bool?   EnableShadows    { get; set; }
        public bool?   EnableGradients  { get; set; }
        public string? BorderRadius    { get; set; }
        public string? PresetTheme     { get; set; }
    }

    /// <summary>
    /// Apply a named preset without changing individual color overrides.
    /// </summary>
    public class ApplyThemePresetRequest
    {
        /// <summary>One of: Default | Ocean | Sunset | Forest | Midnight | Custom</summary>
        public string PresetName { get; set; } = "Default";
    }
}
