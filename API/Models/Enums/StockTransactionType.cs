namespace BillingSoftware.API.Models.Enums
{
    public enum StockTransactionType
    {
        Purchase = 1,
        Sale = 2,
        Return = 3,
        Adjustment = 4,
        Transfer = 5,
        Damaged = 6,
        Opening = 7
    }

    public enum PurchaseStatus
    {
        Draft = 1,
        Ordered = 2,
        Received = 3,
        PartiallyReceived = 4,
        Cancelled = 5
    }

    public enum PurchaseOrderStatus
    {
        Draft = 1,
        Sent = 2,
        Confirmed = 3,
        PartiallyReceived = 4,
        Completed = 5,
        Cancelled = 6
    }

    public enum QuotationStatus
    {
        Draft = 1,
        Sent = 2,
        Accepted = 3,
        Rejected = 4,
        Expired = 5,
        ConvertedToInvoice = 6
    }

    public enum SalesOrderStatus
    {
        Draft = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6
    }

    public enum RefundStatus
    {
        Pending = 1,
        Approved = 2,
        Processed = 3,
        Rejected = 4
    }

    public enum NotificationType
    {
        Info = 1,
        Warning = 2,
        Success = 3,
        Error = 4,
        LowStock = 5,
        PaymentDue = 6,
        InvoiceSent = 7
    }

    public enum BusinessStatus
    {
        Active = 1,
        Suspended = 2,
        Deactivated = 3
    }

    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }

    public enum CashRegisterStatus
    {
        Open = 1,
        Closed = 2
    }

    public enum FileType
    {
        Image = 1,
        Document = 2,
        PDF = 3,
        Other = 4
    }
}
