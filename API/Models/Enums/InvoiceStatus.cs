namespace BillingSoftware.API.Models.Enums
{
    public enum InvoiceStatus
    {
        Draft = 1,
        Pending = 2,
        Paid = 3,
        PartiallyPaid = 4,
        Overdue = 5,
        Cancelled = 6,
        Refunded = 7
    }
}
