using System;
namespace HikePOS.Models.Enum
{
    public enum PaypalRetailInvoicePaymentMethod : long
    {
        None = 0,
        BankTransfer = 1,
        Cash = 2,
        Check = 3,
        CreditCard = 4,
        DebitCard = 5,
        Paypal = 6,
        WireTransfer = 7,
        Other = 8
    }
}
