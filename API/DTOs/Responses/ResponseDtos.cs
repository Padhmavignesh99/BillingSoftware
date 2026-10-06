using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.DTOs.Responses
{
    // ==================== AUTH ====================
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserResponse User { get; set; } = new();
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<PermissionResponse> Permissions { get; set; } = new();
    }

    public class PermissionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? Description { get; set; }
    }

    public class StaffResponse
    {
        public int Id { get; set; }
        public UserResponse User { get; set; } = new();
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
    }

    // ==================== BUSINESS ====================
    public class BusinessResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? BusinessTypeName { get; set; }
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
        public string? LogoUrl { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BranchResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsMainBranch { get; set; }
    }

    // ==================== PRODUCT ====================
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? HSNCode { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? UnitName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? MRP { get; set; }
        public string? TaxRateName { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal? MinStockLevel { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsService { get; set; }
        public bool IsActive { get; set; }
        /// <summary>Optional product expiry date.</summary>
        public DateTime? ExpiryDate { get; set; }
    }

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryResponse> SubCategories { get; set; } = new();
    }

    public class TaxRateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string? Description { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public bool IsDefault { get; set; }
    }

    // ==================== INVOICE ====================
    public class InvoiceResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethodName { get; set; }
        public string? Notes { get; set; }
        public string? PdfUrl { get; set; }
        public List<InvoiceItemResponse> Items { get; set; } = new();
        public List<PaymentResponse> Payments { get; set; } = new();
    }

    public class InvoiceItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? HSNCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ==================== PAYMENT ====================
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? TransactionId { get; set; }
    }

    // ==================== CUSTOMER ====================
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? GSTNumber { get; set; }
        public string? CustomerGroupName { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public int TotalPurchaseCount { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public bool IsActive { get; set; }
    }

    // ==================== SUPPLIER ====================
    public class SupplierResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? GSTNumber { get; set; }
        public string? SupplierGroupName { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int CreditDays { get; set; }
        public bool IsActive { get; set; }
    }

    // ==================== PURCHASE ====================
    public class PurchaseResponse
    {
        public int Id { get; set; }
        public string PurchaseNumber { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ==================== DASHBOARD ====================
    public class DashboardSummaryResponse
    {
        public decimal TodaySales { get; set; }
        public decimal TodayExpenses { get; set; }
        public int TodayInvoiceCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDueAmount { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProductCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyExpenses { get; set; }
    }

    public class DashboardChartResponse
    {
        public List<ChartDataPoint> SalesChart { get; set; } = new();
        public List<ChartDataPoint> ExpenseChart { get; set; } = new();
        public List<TopProductResponse> TopProducts { get; set; } = new();
        public List<PaymentMethodBreakdown> PaymentBreakdown { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class TopProductResponse
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class PaymentMethodBreakdown
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    // ==================== COMMUNICATION ====================
    public class MessageSendResponse
    {
        public int MessageId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ProviderMessageId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CommunicationLogResponse
    {
        public int Id { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? CustomerName { get; set; }
    }

    // ==================== SUBSCRIPTION ====================
    public class SubscriptionPlanResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public int MaxProducts { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBranches { get; set; }
        public int MaxInvoicesPerMonth { get; set; }
        public bool HasWhatsAppIntegration { get; set; }
        public bool HasEmailIntegration { get; set; }
        public bool HasReports { get; set; }
        public bool IsPopular { get; set; }
        public List<string> Features { get; set; } = new();
    }

    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BillingCycle { get; set; }
        public decimal AmountPaid { get; set; }
        public bool AutoRenew { get; set; }
        public int DaysRemaining { get; set; }
    }

    // ==================== REPORT ====================
    public class SalesSummaryResponse
    {
        public decimal TotalSales { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public int TotalInvoices { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<SalesDayWise> DayWiseSales { get; set; } = new();
    }

    public class SalesDayWise
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class ProfitLossResponse
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCostOfGoods { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class TaxReportResponse
    {
        public decimal TotalTaxCollected { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalInputTax { get; set; }
        public decimal NetTaxPayable { get; set; }
        public List<TaxBreakdown> Breakdown { get; set; } = new();
    }

    public class TaxBreakdown
    {
        public string TaxName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class StockReportResponse
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<StockItemResponse> Items { get; set; } = new();
    }

    public class StockItemResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal StockValue { get; set; }
        public string StockStatus { get; set; } = string.Empty;
    }

    // ==================== INVENTORY ====================
    public class InventoryResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReorderLevel { get; set; }
        public string? WarehouseName { get; set; }
        public string? BranchName { get; set; }
    }

    public class StockHistoryResponse
    {
        public int Id { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? CreatedBy { get; set; }
    }

    // ==================== EXPENSE ====================
    public class ExpenseResponse
    {
        public int Id { get; set; }
        public string? ExpenseNumber { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? PaidTo { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ==================== NOTIFICATION ====================
    public class NotificationResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ActionUrl { get; set; }
    }

    // ==================== FILE ====================
    public class FileResponse
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==================== ADMIN ====================
    public class AdminShopResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? OwnerName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PlanName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
        public int ProductCount { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class RevenueReportResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalShops { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public List<ChartDataPoint> RevenueChart { get; set; } = new();
    }

    // ==================== TENANT (SUPERADMIN) ====================
    public class TenantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? GSTNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BusinessTypeName { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Owner details
        public string? OwnerName { get; set; }
        public string? OwnerEmail { get; set; }

        // Subscription
        public string? SubscriptionPlan { get; set; }
        public string? SubscriptionStatus { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
    }

    public class TenantStatsResponse
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalInvoicesThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int TotalBranches { get; set; }
        public string? ActiveSubscriptionPlan { get; set; }
        public string? SubscriptionStatus { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==================== PRINT SETTINGS ====================
    public class PrintSettingsResponse
    {
        public int Id { get; set; }

        // Paper
        public string PaperSize              { get; set; } = "A4";
        public decimal? CustomPaperWidthMm   { get; set; }
        public decimal? CustomPaperHeightMm  { get; set; }
        public string Orientation            { get; set; } = "Portrait";

        // Margins
        public decimal MarginTopMm    { get; set; }
        public decimal MarginBottomMm { get; set; }
        public decimal MarginLeftMm   { get; set; }
        public decimal MarginRightMm  { get; set; }

        // Typography
        public string  FontFamily       { get; set; } = "Arial";
        public decimal FontSizePt       { get; set; }
        public decimal HeaderFontSizePt { get; set; }
        public decimal FooterFontSizePt { get; set; }

        // Invoice display options
        public bool ShowBusinessLogo       { get; set; }
        public bool ShowBusinessAddress    { get; set; }
        public bool ShowGSTDetails         { get; set; }
        public bool ShowCustomerAddress    { get; set; }
        public bool ShowItemHSNCode        { get; set; }
        public bool ShowItemDiscount       { get; set; }
        public bool ShowTaxBreakdown       { get; set; }
        public bool ShowRoundOff           { get; set; }
        public bool ShowAmountInWords      { get; set; }
        public bool ShowTermsAndConditions { get; set; }
        public bool ShowFooterNote         { get; set; }
        public bool ShowSignatureLine      { get; set; }
        public bool ShowQRCode             { get; set; }
        public bool ShowBarcode            { get; set; }
        public bool ShowPaymentDetails     { get; set; }
        public bool ShowDueAmount          { get; set; }
        public bool ShowBankDetails        { get; set; }
        public string? BankDetails         { get; set; }
        public string? UpiId               { get; set; }

        // Copies
        public int InvoiceCopies   { get; set; }
        public int QuotationCopies { get; set; }
        public int PurchaseCopies  { get; set; }
        public int ReceiptCopies   { get; set; }

        // Copy labels
        public string? InvoiceCopyLabel1 { get; set; }
        public string? InvoiceCopyLabel2 { get; set; }
        public string? InvoiceCopyLabel3 { get; set; }

        // Thermal / POS
        public bool   ThermalPrintEnabled     { get; set; }
        public string ThermalPaperWidth       { get; set; } = "80mm";
        public bool   ThermalShowLogo         { get; set; }
        public bool   ThermalShowQRCode       { get; set; }
        public bool   ThermalShowItemCode     { get; set; }
        public bool   ThermalShowCustomerName { get; set; }
        public bool   ThermalAutocut          { get; set; }
        public int    ThermalLineFeedCount    { get; set; }

        // Label
        public bool   LabelPrintEnabled   { get; set; }
        public string LabelPaperSize      { get; set; } = "50x25mm";
        public bool   LabelShowBarcode    { get; set; }
        public bool   LabelShowMRP        { get; set; }
        public bool   LabelShowProductName{ get; set; }
        public bool   LabelShowBusiness   { get; set; }
        public int    LabelsPerRow        { get; set; }

        // Report
        public bool ShowReportHeader  { get; set; }
        public bool ShowReportFooter  { get; set; }
        public bool ShowPageNumbers   { get; set; }
        public bool ShowPrintDateTime { get; set; }
        public bool ShowReportLogo    { get; set; }

        // Color / branding
        public string? PrintPrimaryColor { get; set; }
        public bool    PrintInColor      { get; set; }

        // Default printers
        public string? DefaultPrinterName    { get; set; }
        public string? DefaultThermalPrinter { get; set; }
        public string? DefaultLabelPrinter   { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    // ==================== THEME SETTINGS ====================
    /// <summary>
    /// Full theme settings for a single user.
    /// Also includes a <see cref="CssVariables"/> dictionary that the front-end
    /// can apply directly as CSS custom properties.
    /// </summary>
    public class ThemeSettingsResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }

        // Color mode
        public string ColorMode { get; set; } = "Light";

        // Palette
        public string PrimaryColor   { get; set; } = "#1a73e8";
        public string SecondaryColor { get; set; } = "#fbbc04";
        public string SuccessColor   { get; set; } = "#34a853";
        public string WarningColor   { get; set; } = "#ea8600";
        public string DangerColor    { get; set; } = "#ea4335";

        // Background & surfaces
        public string? BackgroundColor { get; set; }
        public string? SurfaceColor    { get; set; }
        public string? SidebarColor    { get; set; }
        public string? NavbarColor     { get; set; }

        // Typography
        public string FontFamily { get; set; } = "Inter";
        public string FontSize   { get; set; } = "md";

        // Layout
        public string SidebarLayout    { get; set; } = "Vertical";
        public bool   SidebarCollapsed { get; set; } = false;
        public string ContentWidth     { get; set; } = "Fluid";

        // Density
        public string Density { get; set; } = "Comfortable";

        // Visual effects
        public bool EnableAnimations { get; set; } = true;
        public bool EnableShadows    { get; set; } = true;
        public bool EnableGradients  { get; set; } = false;
        public string BorderRadius   { get; set; } = "Md";

        // Preset
        public string? PresetTheme { get; set; } = "Default";

        // Timestamps
        public DateTime  LastChangedAt { get; set; }
        public DateTime? UpdatedAt     { get; set; }

        /// <summary>
        /// Ready-to-use CSS custom-property map.
        /// Keys are variable names (e.g. "--primary-color"), values are the CSS values.
        /// The front-end can inject these into :root { } with a single loop.
        /// </summary>
        public Dictionary<string, string> CssVariables { get; set; } = new();
    }

    /// <summary>Metadata about one available built-in theme preset.</summary>
    /// Returned by GET /api/themesettings/presets
    /// </summary>
    public class ThemePresetInfo
    {
        public string Name          { get; set; } = string.Empty;
        public string DisplayName   { get; set; } = string.Empty;
        public string Description   { get; set; } = string.Empty;
        public string PrimaryColor  { get; set; } = string.Empty;
        public string SecondaryColor{ get; set; } = string.Empty;
        public string ColorMode     { get; set; } = "Light";
        /// <summary>A palette preview — first 4 colours from the preset.</summary>
        public List<string> PreviewColors { get; set; } = new();
    }

    // ==================== REPORT — SALES RETURN ====================
    public class SalesReturnReportResponse
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public List<SalesReturnReportItem> Items { get; set; } = new();
    }

    public class SalesReturnReportItem
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public string? CustomerName { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // ==================== REPORT — PURCHASE ====================
    public class PurchaseReportResponse
    {
        public int TotalPurchases { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
        public List<PurchaseReportItem> Items { get; set; } = new();
    }

    public class PurchaseReportItem
    {
        public int Id { get; set; }
        public string PurchaseNumber { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ==================== REPORT — PURCHASE RETURN ====================
    public class PurchaseReturnReportResponse
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public List<PurchaseReturnReportItem> Items { get; set; } = new();
    }

    public class PurchaseReturnReportItem
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // ==================== REPORT — LOW STOCK ====================
    public class LowStockReportResponse
    {
        public int TotalLowStockProducts { get; set; }
        public List<LowStockReportItem> Items { get; set; } = new();
    }

    public class LowStockReportItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? CategoryName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal ShortageQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
    }

    // ==================== REPORT — CREDIT BILL ====================
    public class CreditBillReportResponse
    {
        public int TotalCreditBills { get; set; }
        public decimal TotalCreditAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalBalanceAmount { get; set; }
        public List<CreditBillReportItem> Items { get; set; } = new();
    }

    public class CreditBillReportItem
    {
        public int Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
    }

    // ==================== REPORT — PRODUCT MINI STATEMENT ====================
    public class ProductMiniStatementResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal CurrentStock { get; set; }
        public List<ProductTransactionItem> Transactions { get; set; } = new();
    }

    public class ProductTransactionItem
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Sales, SalesReturn, Purchase, PurchaseReturn
        public string? ReferenceNumber { get; set; }
        public string? PartyName { get; set; }  // Customer or Supplier name
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // ==================== REPORT — PRODUCT STOCK ====================
    public class ProductStockReportResponse
    {
        public int TotalProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<ProductStockReportItem> Items { get; set; } = new();
    }

    public class ProductStockReportItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? CategoryName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal StockValue { get; set; }
        public string StockStatus { get; set; } = string.Empty; // InStock, LowStock, OutOfStock
        public DateTime? ExpiryDate { get; set; }
    }

    // ==================== REPORT — EXPIRED STOCK ====================
    public class ExpiredStockReportResponse
    {
        public int TotalExpiredProducts { get; set; }
        public int TotalExpiringSoonProducts { get; set; }
        public decimal TotalExpiredStockValue { get; set; }
        public List<ExpiredStockReportItem> Items { get; set; } = new();
    }

    public class ExpiredStockReportItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? CategoryName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal StockValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryStatus { get; set; } = string.Empty; // Expired, ExpiringSoon, Valid
        public int DaysToExpiry { get; set; } // Negative = already expired
    }
}
