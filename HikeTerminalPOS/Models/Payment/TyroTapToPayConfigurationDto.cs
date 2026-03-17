using System;
namespace HikePOS.Models.Payment
{
    public class TyroTapToPayConfigurationDto
    {
        public string Mid { get; set; }
        public string PosReference { get; set; }
        public string IsAuthorised { get; set; }
        public string DisplayName { get; set; }
        public string Access_token { get; set; }
        public string LocationId { get; set; }
        public string ReaderId { get; set; }
        public string ReaderName { get; set; }
        public string Refresh_token { get; set; }
        public string ReaderStatus { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }

    }
    public class TyroTapToPayConnection
    {
        public string Id { get; set; }
        public string ConnectionSecret { get; set; }
        public string CreatedDate { get; set; }
        public string ReaderId { get; set; }
        public string ReaderName { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public bool ExistDifferentPaymentOption { get; set; }
        public string DifferentPaymentName { get; set; }

    }

    public class TyroTapToPayResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string payment { get; set; }

    }
    public class TyroTapToPayPaymentResponse
    {
        public string cardType { get; set; }
        public string transactionTime { get; set; }
        public string amount { get; set; }
        public string elidedPan { get; set; }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public bool isSignatureRequired { get; set; }
        public string mid { get; set; }
        public string transactionReference { get; set; }
        public string transactionDate { get; set; }
        public string approvalCode { get; set; }
        public string transactionID { get; set; }
        public string customerReceipt { get; set; }
        public string surcharge { get; set; }
        public string merchantCategoryCode { get; set; }
        public string tid { get; set; }
        public string retrievalReferenceNumber { get; set; }
    }

}

