namespace HikePOS.Enums
{
    public enum InvoicePaymentKey
    {
        //[LocalizedDescriptionAttribute("GiftCardNumber")]
        GiftCardNumber = 0,

        //[LocalizedDescriptionAttribute("ExpiryDate")]
        ExpiryDate = 1,

        //[LocalizedDescriptionAttribute("CardType")]
        CardType = 2,

        //[LocalizedDescriptionAttribute("CardNumber")]
        CardNumber = 3,

        //[LocalizedDescriptionAttribute("CVV")]
        CVV = 4,

        //[LocalizedDescriptionAttribute("TransactionNumber")]
        TransactionNumber = 5,

        //[LocalizedDescriptionAttribute("AuthorizationNo")]
        AuthorizationNo = 6,

        //[LocalizedDescriptionAttribute("RRN")]
        RRN = 7,

        //[LocalizedDescriptionAttribute("PrintTyroSeparately")]
        PrintTyroSeparately = 8,

        //[LocalizedDescriptionAttribute("CustomerCopy")]
        CustomerCopy = 9,

        //[LocalizedDescriptionAttribute("MerchantCopy")]
        MerchantCopy = 10,

        //[LocalizedDescriptionAttribute("MerchantSignatureRequired")]
        MerchantSignatureRequired = 11,

        //[LocalizedDescriptionAttribute("vantivResponse")]
        vantivResponse = 12,

        //[LocalizedDescriptionAttribute("TyroTerminalId")]
        TyroTerminalId = 13,

        //[LocalizedDescriptionAttribute("TyroMerchantId")]
        TyroMerchantId = 14,

        //[LocalizedDescriptionAttribute("RequestId")]
        // RequestId = 15,

        //[LocalizedDescriptionAttribute("RequestId")]
        Zip = 26,

        //[LocalizedDescriptionAttribute("iZettle")]
        iZettle = 16,

        //[LocalizedDescriptionAttribute("Vantiv")]
        Vantiv = 17,

        //[LocalizedDescriptionAttribute("MintResponse")]
        MintResponse = 18,

        //[LocalizedDescriptionAttribute("PaypalResponse")]
        PaypalResponse = 19,

        AssemblyPaymentResponse = 20,

        TyroResponse = 30,

        EVOPayment = 21,

        VerifonePaymark = 22,

        PayJunction = 23,

        NorthAmericanBankcard = 24,

        AfterPay = 25,

        eConduit = 26,

        Fiska = 27,

        SquareTerminalPayment = 34,

        SquareTerminalRefund = 35,

        SquareReader = 36,

        LinklySessionId = 37,

        LinklyResponse = 38,

        Linkly = 30,

        NAB = 31,

        Fiserv = 32,

        Bendigo = 33,

        ANZ = 34,

        Clearant = 39,

        ClearentVoidResponse = 40,

        ClearentVoidCopy = 41,

        WindcaveResponse = 42,

        SquareCheckoutId = 43,

        CustomPaymentResponse = 29,

        OpeningCreditBalance = 47,
        ClosingCreditBalance = 48,

        SurchargeAmount = 49,
        CastleResponse = 53,
        CastleCustomerPrint = 54,
        CloverResponse = 55,
        CloverCustomerPrint = 56,

        TyroTapToPayResponse = 57,
        // Start #90942 iOS:FR Cheque number for sale by pratik
        CheckIDAtCheckout = 58,
        // End #90942 by pratik
        ApprovedByUser = 62,
        GiftCardOpeningBalance = 64,
        GiftCardClosingBalance = 65,

        HikePayResponse = 66,
        hikePayCustomerPrint = 67,
        hikePayMerchantPrint = 70,
        HikePaySaleResponseData = 69
    }
}
