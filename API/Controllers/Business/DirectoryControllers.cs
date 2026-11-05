using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Services.Interfaces;
using MasterBilling.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterBilling.API.Controllers.Business;

[ApiController]
[Authorize]
[Route("api/customers")]
public class CustomersController(ICustomerService service) : ControllerBase
{
    private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerResponse>>>> GetAll([FromQuery] PaginationRequest page)
    {
        var (items, total) = await service.GetAllCustomersAsync(TenantId, page);
        return Ok(ApiResponse<List<CustomerResponse>>.SuccessResponse(items, "Success", total, page.Page, page.PageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetById(int id)
    {
        var item = await service.GetCustomerByIdAsync(id, TenantId);
        return item is null ? NotFound(ApiResponse<CustomerResponse>.FailResponse("Customer not found"))
            : Ok(ApiResponse<CustomerResponse>.SuccessResponse(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Create([FromBody] CreateCustomerRequest request) =>
        Ok(ApiResponse<CustomerResponse>.SuccessResponse(await service.CreateCustomerAsync(request, TenantId), "Created"));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Update(int id, [FromBody] UpdateCustomerRequest request) =>
        Ok(ApiResponse<CustomerResponse>.SuccessResponse(await service.UpdateCustomerAsync(id, request, TenantId), "Updated"));

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) =>
        Ok(ApiResponse<bool>.SuccessResponse(await service.DeleteCustomerAsync(id, TenantId), "Deleted"));
}

[ApiController]
[Authorize]
[Route("api/customer-payments")]
public class CustomerPaymentsController(ICustomerPaymentService service) : ControllerBase
{
    private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<ApiResponse<List<CustomerPayment>>>> GetByCustomer(int customerId) =>
        Ok(ApiResponse<List<CustomerPayment>>.SuccessResponse(await service.GetPaymentsAsync(customerId, TenantId)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerPayment>>> Create([FromBody] CreateCustomerPaymentRequest request) =>
        Ok(ApiResponse<CustomerPayment>.SuccessResponse(await service.CreatePaymentAsync(request, TenantId), "Payment recorded"));
}

[ApiController]
[Authorize]
[Route("api/suppliers")]
public class SuppliersController(ISupplierService service) : ControllerBase
{
    private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SupplierResponse>>>> GetAll([FromQuery] PaginationRequest page)
    {
        var (items, total) = await service.GetAllSuppliersAsync(TenantId, page);
        return Ok(ApiResponse<List<SupplierResponse>>.SuccessResponse(items, "Success", total, page.Page, page.PageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> GetById(int id)
    {
        var item = await service.GetSupplierByIdAsync(id, TenantId);
        return item is null ? NotFound(ApiResponse<SupplierResponse>.FailResponse("Supplier not found"))
            : Ok(ApiResponse<SupplierResponse>.SuccessResponse(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> Create([FromBody] CreateSupplierRequest request) =>
        Ok(ApiResponse<SupplierResponse>.SuccessResponse(await service.CreateSupplierAsync(request, TenantId), "Created"));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> Update(int id, [FromBody] UpdateSupplierRequest request) =>
        Ok(ApiResponse<SupplierResponse>.SuccessResponse(await service.UpdateSupplierAsync(id, request, TenantId), "Updated"));

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) =>
        Ok(ApiResponse<bool>.SuccessResponse(await service.DeleteSupplierAsync(id, TenantId), "Deleted"));
}

[ApiController]
[Authorize]
[Route("api/expenses")]
public class ExpensesController(IExpenseService service) : ControllerBase
{
    private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExpenseResponse>>>> GetAll([FromQuery] PaginationRequest page)
    {
        var (items, total) = await service.GetAllExpensesAsync(TenantId, page);
        return Ok(ApiResponse<List<ExpenseResponse>>.SuccessResponse(items, "Success", total, page.Page, page.PageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExpenseResponse>>> GetById(int id)
    {
        var item = await service.GetExpenseByIdAsync(id, TenantId);
        return item is null ? NotFound(ApiResponse<ExpenseResponse>.FailResponse("Expense not found"))
            : Ok(ApiResponse<ExpenseResponse>.SuccessResponse(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseResponse>>> Create([FromBody] CreateExpenseRequest request) =>
        Ok(ApiResponse<ExpenseResponse>.SuccessResponse(await service.CreateExpenseAsync(request, TenantId), "Created"));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExpenseResponse>>> Update(int id, [FromBody] UpdateExpenseRequest request) =>
        Ok(ApiResponse<ExpenseResponse>.SuccessResponse(await service.UpdateExpenseAsync(id, request, TenantId), "Updated"));

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) =>
        Ok(ApiResponse<bool>.SuccessResponse(await service.DeleteExpenseAsync(id, TenantId), "Deleted"));
}
