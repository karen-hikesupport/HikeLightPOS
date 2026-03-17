using System;
namespace HikePOS.Enums
{
    //Start #76208 IOS:FR:Terms of payments by Pratik
    public enum InvoicesDueType
    {
        // [LocalizedDescriptionAttribute("OfTheFollowingMonth")]
        OfTheFollowingMonth = 0,

        //  [LocalizedDescriptionAttribute("DaysAfterTheInvoiceDate")]
        DaysAfterTheInvoiceDate = 1,

        // [LocalizedDescriptionAttribute("DaysAfterTheEndOfTheInvoiceMonth")]
        DaysAfterTheEndOfTheInvoiceMonth = 2,

        // [LocalizedDescriptionAttribute("OfTheCurrentMonth")]
        OfTheCurrentMonth = 3
    }
    //END #76208 by Pratik
}

