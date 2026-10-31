using BillingSoftware.API.DTOs.Requests;

namespace BillingSoftware.API.Services.Interfaces
{
    public interface IExportImportService
    {
        Task<byte[]> ExportProductsCsvAsync(int tenantId);
        Task<byte[]> ExportCustomersCsvAsync(int tenantId);
        Task<byte[]> ExportSuppliersCsvAsync(int tenantId);
        Task<byte[]> ExportInvoicesCsvAsync(int tenantId, DateTime? from, DateTime? to);
        Task<ImportResult> ImportProductsCsvAsync(Stream csvStream, int tenantId);
        Task<ImportResult> ImportCustomersCsvAsync(Stream csvStream, int tenantId);
        Task<byte[]> GetProductsTemplateCsvAsync();
        Task<byte[]> GetCustomersTemplateCsvAsync();
    }
}

