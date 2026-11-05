namespace MasterBilling.API.Models.Entities
{
    public class Expense : TenantEntity
    {
        public string? ExpenseNumber { get; set; }
        public int? ExpenseCategoryId { get; set; }
        public ExpenseCategory? ExpenseCategory { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? AttachmentUrl { get; set; }
        public Enums.PaymentMethod? PaymentMethod { get; set; }
        public string? PaidTo { get; set; }
        public string Status { get; set; } = "Approved";
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string? Notes { get; set; }
        public ICollection<ExpensePayment> Payments { get; set; } = new List<ExpensePayment>();
    }

    public class ExpenseCategory : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int? ParentCategoryId { get; set; }
        public ExpenseCategory? ParentCategory { get; set; }
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

    public class ExpensePayment : TenantEntity
    {
        public int ExpenseId { get; set; }
        public Expense? Expense { get; set; }
        public decimal Amount { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
