namespace BillingSoftware.API.DTOs.Requests
{
    // ═══════════════════════════════════════════════════════════════
    // PRINTER REGISTRY — Request DTOs
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Creates or fully replaces a printer registration.</summary>
    public class CreatePrinterRequest
    {
        public required string PrinterName       { get; set; }
        /// <summary>Thermal | Laser | Inkjet | DotMatrix | Label | Portable | Multifunction</summary>
        public required string PrinterType       { get; set; }
        /// <summary>USB | Ethernet | WiFi | Bluetooth | Shared | Cloud</summary>
        public required string ConnectionType    { get; set; }
        public string? ConnectionAddress         { get; set; }
        public int?    Port                      { get; set; }
        public string? OsDriverName              { get; set; }

        // Paper / format
        public string  PaperSize                 { get; set; } = "A4";
        public string  Orientation               { get; set; } = "Portrait";
        public int?    CharsPerLine              { get; set; }
        public int?    DpiResolution             { get; set; }

        // Capabilities
        public bool    SupportsColor             { get; set; } = false;
        public bool    SupportsAutoCut           { get; set; } = false;
        public bool    SupportsCashDrawer        { get; set; } = false;
        public bool    SupportsBarcodes          { get; set; } = false;
        public bool    SupportsQRCode            { get; set; } = false;
        public bool    SupportsLogo              { get; set; } = false;
        public bool    SupportsEscPos            { get; set; } = false;
        public bool    SupportsPdf               { get; set; } = true;

        // Operational
        public bool    IsDefault                 { get; set; } = false;
        public bool    IsActive                  { get; set; } = true;
        public int?    BranchId                  { get; set; }
        public int?    CounterId                 { get; set; }

        // ESC/POS
        public int?    CodePage                  { get; set; }
        public int     LineFeedCount             { get; set; } = 4;
        public string? Notes                     { get; set; }
    }

    /// <summary>Partially updates a printer — only non-null fields are changed.</summary>
    public class PatchPrinterRequest
    {
        public string? PrinterName       { get; set; }
        public string? PrinterType       { get; set; }
        public string? ConnectionType    { get; set; }
        public string? ConnectionAddress { get; set; }
        public int?    Port              { get; set; }
        public string? OsDriverName      { get; set; }
        public string? PaperSize         { get; set; }
        public string? Orientation       { get; set; }
        public int?    CharsPerLine      { get; set; }
        public int?    DpiResolution     { get; set; }
        public bool?   SupportsColor     { get; set; }
        public bool?   SupportsAutoCut   { get; set; }
        public bool?   SupportsCashDrawer{ get; set; }
        public bool?   SupportsBarcodes  { get; set; }
        public bool?   SupportsQRCode    { get; set; }
        public bool?   SupportsLogo      { get; set; }
        public bool?   SupportsEscPos    { get; set; }
        public bool?   SupportsPdf       { get; set; }
        public bool?   IsDefault         { get; set; }
        public bool?   IsActive          { get; set; }
        public int?    BranchId          { get; set; }
        public int?    CounterId         { get; set; }
        public int?    CodePage          { get; set; }
        public int?    LineFeedCount     { get; set; }
        public string? Notes             { get; set; }
    }

    /// <summary>Request body for printing a bill/invoice.</summary>
    public class BillPrintRequest
    {
        /// <summary>ID of the printer registered in the Printer Registry</summary>
        public int?   PrinterId       { get; set; }
        /// <summary>Copies to print (overrides the printer default if supplied)</summary>
        public int    Copies          { get; set; } = 1;
        /// <summary>Invoice | Receipt | Quotation | Purchase | Label</summary>
        public string DocumentType    { get; set; } = "Invoice";
        /// <summary>Paper size override — leave null to use printer's default</summary>
        public string? PaperSize      { get; set; }
        public int?   BranchId        { get; set; }
        public int?   CounterId       { get; set; }
    }
}

namespace BillingSoftware.API.DTOs.Responses
{
    // ═══════════════════════════════════════════════════════════════
    // PRINTER REGISTRY — Response DTOs
    // ═══════════════════════════════════════════════════════════════

    public class PrinterResponse
    {
        public int      Id               { get; set; }
        public string   PrinterName      { get; set; } = string.Empty;
        public string   PrinterType      { get; set; } = string.Empty;
        public string   PrinterTypeLabel { get; set; } = string.Empty;
        public string   ConnectionType   { get; set; } = string.Empty;
        public string?  ConnectionAddress{ get; set; }
        public int?     Port             { get; set; }
        public string?  OsDriverName     { get; set; }
        public string   PaperSize        { get; set; } = string.Empty;
        public string   Orientation      { get; set; } = string.Empty;
        public int?     CharsPerLine     { get; set; }
        public int?     DpiResolution    { get; set; }
        public bool     SupportsColor    { get; set; }
        public bool     SupportsAutoCut  { get; set; }
        public bool     SupportsCashDrawer { get; set; }
        public bool     SupportsBarcodes { get; set; }
        public bool     SupportsQRCode   { get; set; }
        public bool     SupportsLogo     { get; set; }
        public bool     SupportsEscPos   { get; set; }
        public bool     SupportsPdf      { get; set; }
        public bool     IsDefault        { get; set; }
        public bool     IsActive         { get; set; }
        public int?     BranchId         { get; set; }
        public int?     CounterId        { get; set; }
        public int?     CodePage         { get; set; }
        public int      LineFeedCount    { get; set; }
        public string?  Notes            { get; set; }
        public DateTime? LastTestedAt    { get; set; }
        public string?   LastTestStatus  { get; set; }
        public DateTime  CreatedAt       { get; set; }
        public string    StatusLabel     { get; set; } = string.Empty;
    }

    public class PrinterTestResultResponse
    {
        public bool    Success           { get; set; }
        public string  Message           { get; set; } = string.Empty;
        public int?    LatencyMs         { get; set; }
        public string  ConnectionType    { get; set; } = string.Empty;
        public string? ConnectionAddress { get; set; }
        public int?    Port              { get; set; }
        public DateTime TestedAt        { get; set; } = DateTime.UtcNow;
    }

    public class BillPrintResponse
    {
        public int    PrintJobId      { get; set; }
        public string Status          { get; set; } = string.Empty;
        public string DocumentType    { get; set; } = string.Empty;
        public int?   DocumentId      { get; set; }
        public string? DocumentRef    { get; set; }
        public string? PrinterName    { get; set; }
        public string PrintMethod     { get; set; } = string.Empty;  // Browser | ESC/POS | PDF | Network
        public int    Copies          { get; set; }
        public string? HtmlContent    { get; set; }  // For browser printing
        public string? Message        { get; set; }
    }
}
