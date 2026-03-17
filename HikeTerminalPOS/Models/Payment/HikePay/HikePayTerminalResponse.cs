using System;
using Newtonsoft.Json;
using System.Web;
using HikePOS.Models.Payment;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text;

namespace HikePOS.Models.Payment
{
    public class HikePayTerminalResponse
    {
        public bool PaymentStatusSuccess { get; set; }
        public string PoiTransactionId { get; set; }
        public DateTime? PoiTransactionTimeStamp { get; set; }
        public string SaleTransactionId { get; set; }
        public DateTime? SaleTransactionTimeStamp { get; set; }
        public string RefusalReason { get; set; }
        public string AdditonalResponse { get; set; }
        public AdditionalDataResponse DecodeAdditonalResponse => string.IsNullOrEmpty(AdditonalResponse) ? null : JsonConvert.DeserializeObject<AdditionalDataResponse>(AdditonalResponse);
        public object PaymentReceipt { get; set; }
        public bool IsTaskCanceledException { get; set; }
        public bool IsInProgress { get; set; }
        public ErrorConditionType ErrorConditionType { get; set; }

        public string CustomerReceipt { get; set; }
        public string MerchantReceipt { get; set; }

    }

    public enum ErrorConditionType
    {
        Aborted,
        Busy,
        Cancel,
        DeviceOut,
        InsertedCard,
        InProgress,
        LoggedOut,
        MessageFormat,
        NotAllowed,
        NotFound,
        PaymentRestriction,
        Refusal,
        UnavailableDevice,
        UnavailableService,
        InvalidCard,
        UnreachableHost,
        WrongPIN
    }

    public class AdditionalData
    {
        public string AID { get; set; }
        public string acquirerResponseCode { get; set; }
        public string applicationLabel { get; set; }
        public string applicationPreferredName { get; set; }
        public string backendGiftcardIndicator { get; set; }
        public string batteryLevel { get; set; }
        public string cardBin { get; set; }
        public string cardHolderVerificationMethodResults { get; set; }
        public string cardIssueNumber { get; set; }
        public string cardIssuerCountryId { get; set; }
        public string cardScheme { get; set; }
        public string cardSummary { get; set; }
        public string cardType { get; set; }
        public string expiryMonth { get; set; }
        public string expiryYear { get; set; }
        public string fundingSource { get; set; }
        public string giftcardIndicator { get; set; }
        public string isCardCommercial { get; set; }
        public DateTime iso8601TxDate { get; set; }
        public string merchantReference { get; set; }
        public string mid { get; set; }
        public string networkTxReference { get; set; }
        public string offline { get; set; }
        public string paymentMethod { get; set; }
        public string paymentMethodVariant { get; set; }
        public string posAmountCashbackValue { get; set; }
        public string posAmountGratuityValue { get; set; }
        public string posAuthAmountCurrency { get; set; }
        public string posAuthAmountValue { get; set; }
        public string posEntryMode { get; set; }
        public string posOriginalAmountValue { get; set; }
        public string pspReference { get; set; }
        public string shopperCountry { get; set; }
        public string surchargeAmount { get; set; }
        public string tc { get; set; }
        public string tid { get; set; }
        public string transactionReferenceNumber { get; set; }
        public string transactionType { get; set; }
        public string txdate { get; set; }
        public string txtime { get; set; }
    }

    public class AdditionalDataResponse
    {
        public AdditionalData additionalData { get; set; }
        public string store { get; set; }
        public string message { get; set; }
        public string note { get; set; }
        public string refusalReason { get; set; }

    }
}
