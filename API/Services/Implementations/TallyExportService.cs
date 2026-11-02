using BillingSoftware.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BillingSoftware.API.Services.Implementations
{
    public class TallyExportService
    {
        private readonly ApplicationDbContext _db;
        public TallyExportService(ApplicationDbContext db) => _db = db;

        public async Task<byte[]> ExportToTallyXmlAsync(int tenantId, DateTime from, DateTime to)
        {
            var invoices = await _db.Invoices
                .Where(i => i.TenantId == tenantId && i.InvoiceDate >= from && i.InvoiceDate <= to && !i.IsDeleted)
                .Include(i => i.Customer)
                .Include(i => i.Items).ThenInclude(item => item.Product)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<ENVELOPE>");
            sb.AppendLine("  <HEADER>");
            sb.AppendLine("    <TALLYREQUEST>Import Data</TALLYREQUEST>");
            sb.AppendLine("  </HEADER>");
            sb.AppendLine("  <BODY>");
            sb.AppendLine("    <IMPORTDATA>");
            sb.AppendLine("      <REQUESTDESC>");
            sb.AppendLine("        <REPORTNAME>Vouchers</REPORTNAME>");
            sb.AppendLine("      </REQUESTDESC>");
            sb.AppendLine("      <REQUESTDATA>");

            foreach (var inv in invoices)
            {
                sb.AppendLine("        <TALLYMESSAGE xmlns:UDF=\"TallyUDF\">");
                sb.AppendLine("          <VOUCHER VCHTYPE=\"Sales\" ACTION=\"Create\">");
                sb.AppendLine($"            <DATE>{inv.InvoiceDate:yyyyMMdd}</DATE>");
                sb.AppendLine($"            <VOUCHERNUMBER>{inv.InvoiceNumber}</VOUCHERNUMBER>");
                sb.AppendLine($"            <PARTYLEDGERNAME>{EscapeXml(inv.Customer?.Name ?? "Cash")}</PARTYLEDGERNAME>");
                sb.AppendLine($"            <NARRATION>Invoice {inv.InvoiceNumber}</NARRATION>");
                sb.AppendLine("            <ALLLEDGERENTRIES.LIST>");
                sb.AppendLine($"              <LEDGERNAME>{EscapeXml(inv.Customer?.Name ?? "Cash")}</LEDGERNAME>");
                sb.AppendLine($"              <ISDEEMEDPOSITIVE>Yes</ISDEEMEDPOSITIVE>");
                sb.AppendLine($"              <AMOUNT>-{inv.TotalAmount}</AMOUNT>");
                sb.AppendLine("            </ALLLEDGERENTRIES.LIST>");
                foreach (var item in inv.Items)
                {
                    sb.AppendLine("            <ALLINVENTORYENTRIES.LIST>");
                    sb.AppendLine($"              <STOCKITEMNAME>{EscapeXml(item.ProductName)}</STOCKITEMNAME>");
                    sb.AppendLine($"              <ISDEEMEDPOSITIVE>No</ISDEEMEDPOSITIVE>");
                    sb.AppendLine($"              <RATE>{item.UnitPrice}</RATE>");
                    sb.AppendLine($"              <AMOUNT>{item.TotalPrice}</AMOUNT>");
                    sb.AppendLine($"              <ACTUALQTY>{item.Quantity} Nos</ACTUALQTY>");
                    sb.AppendLine($"              <BILLEDQTY>{item.Quantity} Nos</BILLEDQTY>");
                    sb.AppendLine("            </ALLINVENTORYENTRIES.LIST>");
                }
                sb.AppendLine("          </VOUCHER>");
                sb.AppendLine("        </TALLYMESSAGE>");
            }

            sb.AppendLine("      </REQUESTDATA>");
            sb.AppendLine("    </IMPORTDATA>");
            sb.AppendLine("  </BODY>");
            sb.AppendLine("</ENVELOPE>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeXml(string? s) =>
            (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
