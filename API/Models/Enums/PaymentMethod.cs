namespace MasterBilling.API.Models.Enums
{
    public enum PaymentMethod
    {
        Cash            = 1,
        UPI             = 2,
        DebitCard       = 3,
        CreditCard      = 4,
        NetBanking      = 5,
        Wallet          = 6,
        GiftCard        = 7,
        StoreCredit     = 8,
        CustomerCredit  = 9,   // Pay Later / Khata
        BankTransfer    = 10,  // NEFT / IMPS / RTGS
        Cheque          = 11,
        EMI             = 12,
        PaymentOnDelivery = 13,
        Online          = 14,
        Card            = 15   // generic card (legacy compat)
    }
}
