namespace BillingSoftware.API.Models.Enums
{
    public enum SubscriptionStatus
    {
        Trial        = 1,
        Active       = 2,
        Expired      = 3,
        Cancelled    = 4,
        Suspended    = 5,
        LifetimeFree = 6,   // Granted lifetime free by SuperAdmin
        MonthlyFree  = 7    // Granted 1-month free by SuperAdmin
    }

    /// <summary>SuperAdmin-assigned plan permit type.</summary>
    public enum PlanPermitType
    {
        Paid = 1,  // Normal paying customer
        Free = 2   // SuperAdmin-granted free access
    }

    /// <summary>Duration of a SuperAdmin-granted free permit.</summary>
    public enum PlanPermitDuration
    {
        OneMonth     = 1,   // Free for 1 month
        LifetimeFree = 2    // Free forever
    }
}
