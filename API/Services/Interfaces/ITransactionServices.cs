using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;

namespace MasterBilling.API.Services.Interfaces
{
    // ==================== INVENTORY ====================
    public interface IInventoryService
    {
        Task<List<InventoryResponse>> GetAllInventoryAsync(int tenantId);
        Task<InventoryResponse> AdjustStockAsync(int productId, AdjustStockRequest request, int tenantId);
        Task<List<ProductResponse>> GetLowStockProductsAsync(int tenantId);
        Task<List<StockHistoryResponse>> GetStockHistoryAsync(int productId, int tenantId);
    }

    // ==================== STOCK ====================
    public interface IStockService
    {
        Task<List<StockTransaction>> GetStockTransactionsAsync(int tenantId, PaginationRequest pagination);
        Task<StockAdjustment> CreateAdjustmentAsync(CreateStockAdjustmentRequest request, int tenantId);
        Task<List<StockAdjustment>> GetAdjustmentsAsync(int tenantId);
        Task<StockTransfer> CreateTransferAsync(CreateStockTransferRequest request, int tenantId);
        Task<List<StockTransfer>> GetTransfersAsync(int tenantId);
        Task<StockTransfer> UpdateTransferStatusAsync(int id, string status);
    }

    // IWarehouseService moved to ITenantManagementServices.cs (fully upgraded)

    // ==================== PURCHASE ====================
    public interface IPurchaseService
    {
        Task<(List<PurchaseResponse> Items, int TotalCount)> GetAllPurchasesAsync(int tenantId, PaginationRequest pagination);
        Task<PurchaseResponse?> GetPurchaseByIdAsync(int id, int tenantId);
        Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request, int tenantId);
        Task<PurchaseResponse> UpdatePurchaseAsync(int id, UpdatePurchaseRequest request, int tenantId);
        Task<bool> DeletePurchaseAsync(int id, int tenantId);
    }

    // ==================== PURCHASE ORDER ====================
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrder>> GetAllAsync(int tenantId);
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<PurchaseOrder> CreateAsync(CreatePurchaseOrderRequest request, int tenantId);
        Task<PurchaseOrder> UpdateAsync(int id, UpdatePurchaseOrderRequest request);
        Task<bool> DeleteAsync(int id);
    }

    // ==================== PURCHASE PAYMENT ====================
    public interface IPurchasePaymentService
    {
        Task<List<PurchasePayment>> GetPaymentsByPurchaseAsync(int purchaseId);
        Task<PurchasePayment> CreatePaymentAsync(CreatePurchasePaymentRequest request, int tenantId);
    }

    // ==================== PURCHASE RETURN ====================
    public interface IPurchaseReturnService
    {
        Task<List<PurchaseReturn>> GetAllAsync(int tenantId);
        Task<PurchaseReturn?> GetByIdAsync(int id);
        Task<PurchaseReturn> CreateAsync(CreatePurchaseReturnRequest request, int tenantId);
        Task<bool> DeleteAsync(int id);
    }

    // ==================== GOODS RECEIPT ====================
    public interface IGoodsReceiptService
    {
        Task<List<GoodsReceipt>> GetAllAsync(int tenantId);
        Task<GoodsReceipt?> GetByIdAsync(int id);
        Task<GoodsReceipt> CreateAsync(CreateGoodsReceiptRequest request, int tenantId);
    }

    // ==================== INVOICE ====================
    public interface IInvoiceService
    {
        Task<(List<InvoiceResponse> Items, int TotalCount)> GetAllInvoicesAsync(int tenantId, PaginationRequest pagination);
        Task<InvoiceResponse?> GetInvoiceByIdAsync(int id, int tenantId);
        Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, int tenantId);
        Task<InvoiceResponse> UpdateInvoiceAsync(int id, UpdateInvoiceRequest request, int tenantId);
        Task<bool> DeleteInvoiceAsync(int id, int tenantId);
        Task<string> GetNextInvoiceNumberAsync(int tenantId);
        Task<byte[]> GenerateInvoicePdfAsync(int id, int tenantId);
    }

    // ==================== POS ====================
    public interface IPOSService
    {
        Task<InvoiceResponse> CreatePOSBillAsync(POSBillRequest request, int tenantId);
        Task<InvoiceResponse> GetLastBillAsync(int tenantId);
    }

    // ==================== SALES RETURN ====================
    public interface ISalesReturnService
    {
        Task<List<SalesReturn>> GetAllAsync(int tenantId);
        Task<SalesReturn?> GetByIdAsync(int id);
        Task<SalesReturn> CreateAsync(CreateSalesReturnRequest request, int tenantId);
        Task<bool> DeleteAsync(int id);
    }

    // ==================== QUOTATION ====================
    public interface IQuotationService
    {
        Task<List<Quotation>> GetAllAsync(int tenantId);
        Task<Quotation?> GetByIdAsync(int id);
        Task<Quotation> CreateAsync(CreateQuotationRequest request, int tenantId);
        Task<Quotation> UpdateAsync(int id, UpdateQuotationRequest request);
        Task<bool> DeleteAsync(int id);
        Task<InvoiceResponse> ConvertToInvoiceAsync(int quotationId, int tenantId);
    }

    // ==================== SALES ORDER ====================
    public interface ISalesOrderService
    {
        Task<List<SalesOrder>> GetAllAsync(int tenantId);
        Task<SalesOrder?> GetByIdAsync(int id);
        Task<SalesOrder> CreateAsync(CreateSalesOrderRequest request, int tenantId);
        Task<SalesOrder> UpdateAsync(int id, UpdateSalesOrderRequest request);
        Task<bool> DeleteAsync(int id);
        Task<InvoiceResponse> ConvertToInvoiceAsync(int salesOrderId, int tenantId);
    }
}
