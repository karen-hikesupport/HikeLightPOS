using System;
using HikePOS.Enums;

namespace HikePOS.Models.Enum
{
    public enum AccountTransactionType : int
    {
        [LocalizedDescriptionAttribute("Sale")]
        Sale = 0,

        [LocalizedDescriptionAttribute("Payment")]
        Payment = 1,

        [LocalizedDescriptionAttribute("CreditNote")]
        CreditNote = 2,

        [LocalizedDescriptionAttribute("Refund")]
        Refund = 3,

        [LocalizedDescriptionAttribute("Issued")]
        Issued = 4
    }
}
