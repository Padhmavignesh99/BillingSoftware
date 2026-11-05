using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;

namespace MasterBilling.API.Services.Interfaces
{
    // ==================== CUSTOMER ====================
    public interface ICustomerService
    {
        Task<(List<CustomerResponse> Items, int TotalCount)> GetAllCustomersAsync(int tenantId, PaginationRequest pagination);
        Task<CustomerResponse?> GetCustomerByIdAsync(int id, int tenantId);
        Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, int tenantId);
        Task<CustomerResponse> UpdateCustomerAsync(int id, UpdateCustomerRequest request, int tenantId);
        Task<bool> DeleteCustomerAsync(int id, int tenantId);
    }

    public interface ICustomerGroupService
    {
        Task<List<CustomerGroup>> GetAllAsync(int tenantId);
        Task<CustomerGroup> CreateAsync(CreateCustomerGroupRequest request, int tenantId);
        Task<CustomerGroup> UpdateAsync(int id, CreateCustomerGroupRequest request);
        Task<bool> DeleteAsync(int id);
    }

    public interface ICustomerLedgerService
    {
        Task<List<CustomerLedger>> GetLedgerAsync(int customerId, int tenantId);
        Task<decimal> GetDueBalanceAsync(int customerId, int tenantId);
        Task<List<InvoiceResponse>> GetPurchaseHistoryAsync(int customerId, int tenantId);
    }

    public interface ICustomerPaymentService
    {
        Task<List<CustomerPayment>> GetPaymentsAsync(int customerId, int tenantId);
        Task<CustomerPayment> CreatePaymentAsync(CreateCustomerPaymentRequest request, int tenantId);
    }

    public interface ICustomerCreditService
    {
        Task<List<CustomerCredit>> GetCreditsAsync(int customerId, int tenantId);
        /// <summary>Create a credit entry for a billing user. userId tracks which staff/user created the credit.</summary>
        Task<CustomerCredit> CreateCreditAsync(int customerId, int? invoiceId, decimal amount, int tenantId, int? userId = null, DateTime? dueDate = null, string? notes = null);
        /// <summary>Get all credit bills created by a specific user (userwise).</summary>
        Task<List<CustomerCredit>> GetCreditsByUserAsync(int userId, int tenantId);
        /// <summary>Settle (partially or fully) a credit bill.</summary>
        Task<CustomerCredit> SettleCreditAsync(int creditId, decimal paymentAmount, int tenantId);
        /// <summary>Get total outstanding credit balance for a specific user.</summary>
        Task<decimal> GetUserCreditBalanceAsync(int userId, int tenantId);
    }

    // ==================== SUPPLIER ====================
    public interface ISupplierService
    {
        Task<(List<SupplierResponse> Items, int TotalCount)> GetAllSuppliersAsync(int tenantId, PaginationRequest pagination);
        Task<SupplierResponse?> GetSupplierByIdAsync(int id, int tenantId);
        Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, int tenantId);
        Task<SupplierResponse> UpdateSupplierAsync(int id, UpdateSupplierRequest request, int tenantId);
        Task<bool> DeleteSupplierAsync(int id, int tenantId);
    }

    public interface ISupplierGroupService
    {
        Task<List<SupplierGroup>> GetAllAsync(int tenantId);
        Task<SupplierGroup> CreateAsync(CreateSupplierGroupRequest request, int tenantId);
        Task<SupplierGroup> UpdateAsync(int id, CreateSupplierGroupRequest request);
        Task<bool> DeleteAsync(int id);
    }

    public interface ISupplierLedgerService
    {
        Task<List<SupplierLedger>> GetLedgerAsync(int supplierId, int tenantId);
        Task<decimal> GetBalanceAsync(int supplierId, int tenantId);
    }

    public interface ISupplierPaymentService
    {
        Task<List<SupplierPayment>> GetPaymentsAsync(int supplierId, int tenantId);
        Task<SupplierPayment> CreatePaymentAsync(CreateSupplierPaymentRequest request, int tenantId);
    }

    public interface ISupplierCreditService
    {
        Task<List<SupplierCredit>> GetCreditsAsync(int supplierId, int tenantId);
    }

    // ==================== PAYMENT ====================
    public interface IPaymentService
    {
        Task<(List<PaymentResponse> Items, int TotalCount)> GetAllPaymentsAsync(int tenantId, PaginationRequest pagination);
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, int tenantId);
        Task<List<PaymentResponse>> GetDueListAsync(int tenantId);
        Task<PaymentResponse> CollectDueAsync(CollectDueRequest request, int tenantId);
    }

    public interface IPaymentMethodService
    {
        Task<List<PaymentMethodEntity>> GetAllAsync(int tenantId);
        Task<PaymentMethodEntity> CreateAsync(CreatePaymentMethodRequest request, int tenantId);
        Task<PaymentMethodEntity> UpdateAsync(int id, CreatePaymentMethodRequest request);
        Task<bool> DeleteAsync(int id);
    }

    public interface IRefundService
    {
        Task<List<Refund>> GetAllRefundsAsync(int tenantId);
        Task<Refund?> GetRefundByIdAsync(int id);
        Task<Refund> CreateRefundAsync(CreateRefundRequest request, int tenantId);
        Task<Refund> UpdateRefundStatusAsync(int id, string status);
    }

    public interface ICashRegisterService
    {
        Task<CashRegister> OpenRegisterAsync(OpenCashRegisterRequest request, int tenantId, int userId);
        Task<CashRegister> CloseRegisterAsync(CloseCashRegisterRequest request, int userId);
        Task<CashRegister?> GetCurrentRegisterAsync(int tenantId, int userId);
        Task<List<CashRegister>> GetRegisterHistoryAsync(int tenantId);
    }

    public interface IPaymentGatewayService
    {
        Task<PaymentGatewayTransaction> CreatePaymentLinkAsync(int invoiceId, int tenantId);
        Task<bool> HandleWebhookAsync(string payload);
        Task<PaymentGatewayTransaction?> GetTransactionStatusAsync(string transactionId);
    }

    // ==================== EXPENSE ====================
    public interface IExpenseService
    {
        Task<(List<ExpenseResponse> Items, int TotalCount)> GetAllExpensesAsync(int tenantId, PaginationRequest pagination);
        Task<ExpenseResponse?> GetExpenseByIdAsync(int id, int tenantId);
        Task<ExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, int tenantId);
        Task<ExpenseResponse> UpdateExpenseAsync(int id, UpdateExpenseRequest request, int tenantId);
        Task<bool> DeleteExpenseAsync(int id, int tenantId);
    }

    public interface IExpenseCategoryService
    {
        Task<List<ExpenseCategory>> GetAllAsync(int tenantId);
        Task<ExpenseCategory> CreateAsync(CreateExpenseCategoryRequest request, int tenantId);
        Task<ExpenseCategory> UpdateAsync(int id, CreateExpenseCategoryRequest request);
        Task<bool> DeleteAsync(int id);
    }

    public interface IExpensePaymentService
    {
        Task<List<ExpensePayment>> GetPaymentsByExpenseAsync(int expenseId);
        Task<ExpensePayment> CreatePaymentAsync(CreateExpensePaymentRequest request, int tenantId);
    }
}
