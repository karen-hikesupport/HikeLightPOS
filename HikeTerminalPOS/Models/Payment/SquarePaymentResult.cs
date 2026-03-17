using System;
namespace HikePOS.Models.Payment
{
    public class SquarePaymentResult
    {
        public bool IsTransactionSuccess { get; set; }
        public string ClientTransactionID { get; set; }
        public string TransactionID { get; set; }
        public string UserInfo { get; set; }

        public string ResponseErrorDescription { get; set; }

        public SCCAPIResponseStatus ResponseStatus { get; set; }

        public SquareResponseErrorCodes SquareResponseError { get; set; }

        public SquareAPIErrorCodes SquareAPIError { get; set; }

    }

    public enum SCCAPIResponseStatus : int
    {
        Unknown = 0,
        Ok,
        Error
    }

    public enum SquareResponseErrorCodes : int
    {
        Unknown = 0,
        MissingCurrencyCode,
        UnsupportedCurrencyCode_Deprecated,
        MissingRequestClientID,
        InvalidRequestCallbackURL,
        InvalidRequestAmount,
        CannotOpenApplication,
        UnableToGenerateRequestJSON,
        MissingOrInvalidResponseData,
        MissingOrInvalidResponseJSONData,
        MissingOrInvalidResponseStatus,
        MissingOrInvalidResponseErrorCode
    }

    public enum SquareAPIErrorCodes : int
    {
        Unknown = 0,
        PaymentCanceled,
        PayloadMissingOrInvalid,
        AppNotLoggedIn,
        Unused,
        LocationIDMismatch,
        MerchantIDMismatch = LocationIDMismatch,
        UserNotActivated,
        CurrencyMissingOrInvalid,
        CurrencyUnsupported,
        CurrencyMismatch,
        AmountMissingOrInvalid,
        AmountTooSmall,
        AmountTooLarge,
        InvalidTenderType,
        UnsupportedTenderType,
        CouldNotPerform,
        NoNetworkConnection,
        ClientNotAuthorizedForUser,
        UnsupportedAPIVersion,
        InvalidVersionNumber,
        CustomerManagementNotSupported,
        InvalidCustomerID
    }
}
