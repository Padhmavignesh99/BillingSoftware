using BillingSoftware.API.Configuration;
using BillingSoftware.API.Data;
using BillingSoftware.API.Services.Implementations;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Database — Multi-provider support ────────────────────────────────────────
// Read DataSettings from appsettings.json to determine which DB to use.
// To switch providers: change "DataSettings:ActiveProvider" in appsettings.json.
// Accepted values: "MySql" | "MSSql" | "PostgreSQL"
// Cloud variants:   "MySql_Cloud" | "MSSql_Cloud" | "PostgreSQL_Cloud"
var dataSettings = builder.Configuration
    .GetSection("DataSettings")
    .Get<DataSettings>()
    ?? throw new InvalidOperationException("Missing 'DataSettings' section in appsettings.json.");

var activeProvider = dataSettings.ActiveProvider;

var connectionString = builder.Configuration.GetConnectionString(activeProvider)
    ?? throw new InvalidOperationException(
        $"No connection string found for provider '{activeProvider}'. " +
        $"Add a matching key under 'ConnectionStrings' in appsettings.json.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    DbContextConfigurator.Configure(options, activeProvider, connectionString));

// Register DataSettings so ApplicationDbContext can read it via IConfiguration
builder.Services.Configure<DataSettings>(
    builder.Configuration.GetSection("DataSettings"));

// ─── Core Services ────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Required for TenantContext to resolve the current user from the HTTP request
builder.Services.AddHttpContextAccessor();
// Required by WhatsAppService (and any typed HTTP clients)
builder.Services.AddHttpClient();

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? new JwtSettings { SecretKey = "BillingSoftware_SuperSecret_JWT_Key_2024!", Issuer = "BillingSoftwareAPI", Audience = "BillingSoftwareClient" };
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidAudience            = jwtSettings.Audience,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew                = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

// ─── Multi-Tenant Services ────────────────────────────────────────────────────
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantService, TenantService>();

// ─── Print & Theme Settings ───────────────────────────────────────────────────
builder.Services.AddScoped<IPrintSettingsService, PrintSettingsService>();
builder.Services.AddScoped<IThemeSettingsService, ThemeSettingsService>();

// ─── Auth / User Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,       AuthService>();
builder.Services.AddScoped<IUserService,       UserService>();
builder.Services.AddScoped<IRoleService,       RoleService>();
builder.Services.AddScoped<IStaffService,      StaffService>();

// ─── Business / Branch ────────────────────────────────────────────────────────
builder.Services.AddScoped<IBusinessService,         BusinessService>();
builder.Services.AddScoped<IBranchService,           BranchService>();
builder.Services.AddScoped<IBusinessSettingsService, BusinessSettingsService>();

// ─── Product Services ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductService,   ProductService>();
builder.Services.AddScoped<ICategoryService,  CategoryService>();
builder.Services.AddScoped<IBrandService,     BrandService>();
builder.Services.AddScoped<IUnitService,      UnitService>();
builder.Services.AddScoped<ITaxService,       TaxService>();
builder.Services.AddScoped<IDiscountService,  DiscountService>();
builder.Services.AddScoped<IBarcodeService,   BarcodeService>();

// ─── Inventory / Stock ────────────────────────────────────────────────────────
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IStockService,     StockService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

// ─── Sales Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IInvoiceService,     InvoiceService>();
builder.Services.AddScoped<IPOSService,         POSService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<IQuotationService,   QuotationService>();
builder.Services.AddScoped<ISalesOrderService,  SalesOrderService>();

// ─── Purchase Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<IPurchaseService,        PurchaseService>();
builder.Services.AddScoped<IPurchaseOrderService,   PurchaseOrderService>();
builder.Services.AddScoped<IPurchasePaymentService, PurchasePaymentService>();
builder.Services.AddScoped<IPurchaseReturnService,  PurchaseReturnService>();
builder.Services.AddScoped<IGoodsReceiptService,    GoodsReceiptService>();

// ─── Customer Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICustomerService,       CustomerService>();
builder.Services.AddScoped<ICustomerGroupService,  CustomerGroupService>();
builder.Services.AddScoped<ICustomerLedgerService, CustomerLedgerService>();
builder.Services.AddScoped<ICustomerPaymentService,CustomerPaymentService>();
builder.Services.AddScoped<ICustomerCreditService, CustomerCreditService>();

// ─── Supplier Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<ISupplierService,       SupplierService>();
builder.Services.AddScoped<ISupplierGroupService,  SupplierGroupService>();
builder.Services.AddScoped<ISupplierLedgerService, SupplierLedgerService>();
builder.Services.AddScoped<ISupplierPaymentService,SupplierPaymentService>();
builder.Services.AddScoped<ISupplierCreditService, SupplierCreditService>();

// ─── Payment Services ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IPaymentService,        PaymentService>();
builder.Services.AddScoped<IPaymentMethodService,  PaymentMethodService>();
builder.Services.AddScoped<IRefundService,         RefundService>();
builder.Services.AddScoped<ICashRegisterService,   CashRegisterService>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

// ─── Expense Services ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IExpenseService,         ExpenseService>();
builder.Services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
builder.Services.AddScoped<IExpensePaymentService,  ExpensePaymentService>();

// ─── Communication Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService,        EmailService>();
builder.Services.AddScoped<IWhatsAppService,     WhatsAppService>();
builder.Services.AddScoped<ISmsService,          SmsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ─── Report & Dashboard ───────────────────────────────────────────────────────
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService,    ReportService>();

// ─── Subscription Services ────────────────────────────────────────────────────
builder.Services.AddScoped<ISubscriptionService,        SubscriptionService>();
builder.Services.AddScoped<ISubscriptionPlanService,    SubscriptionPlanService>();
builder.Services.AddScoped<ISubscriptionPaymentService, SubscriptionPaymentService>();
builder.Services.AddScoped<ITrialService,               TrialService>();
builder.Services.AddScoped<IPlanFeatureService,         PlanFeatureService>();

// ─── System Services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IFileService,      FileService>();
builder.Services.AddScoped<IAuditLogService,  AuditLogService>();
builder.Services.AddScoped<IBackupService,    BackupService>();
builder.Services.AddScoped<ILookupService,    LookupService>();
builder.Services.AddScoped<IAdminService,     AdminService>();

// ─── Device Integration Layer ──────────────────────────────────────────────────
// Provides a hardware-agnostic abstraction over all physical billing devices.
// Each service decouples the billing modules from specific device implementations,
// allowing the same software to work for any shop type.
builder.Services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
builder.Services.AddScoped<IDeviceSettingsService,      DeviceSettingsService>();
builder.Services.AddScoped<IBarcodeScannerService,      BarcodeScannerService>();
builder.Services.AddScoped<IQRScannerService,           QRScannerService>();
builder.Services.AddScoped<IWeighingScaleService,       WeighingScaleService>();
builder.Services.AddScoped<IOcrScannerService,          OcrScannerService>();
builder.Services.AddScoped<ICardPaymentService,         CardPaymentService>();
builder.Services.AddScoped<ICashDrawerService,          CashDrawerService>();


// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Billing Software API",
        Version     = "v1",
        Description = "REST API for the Billing Software application.\n\n" +
                      "**Authentication**: Most endpoints require a JWT Bearer token.\n" +
                      "Use the **Authorize** button (🔒) after logging in via `/api/auth/login`.",
        Contact = new OpenApiContact
        {
            Name  = "Padhmavignesh",
            Email = "padhmavignesh99@example.com",
            Url   = new Uri("https://github.com/Padhmavignesh99/BillingSoftware")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url  = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // ── JWT Bearer security definition ────────────────────────────────────────
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  =
            "Enter your JWT token in the field below.\n\n" +
            "Example: **eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...**"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Swagger UI — available in all environments ────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing Software API v1");
    options.RoutePrefix       = "swagger";
    options.DocumentTitle     = "Billing Software API";
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.DefaultModelsExpandDepth(-1); // hides the schema section by default
});

// ─── HTTP Pipeline ────────────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Redirect root "/" to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
