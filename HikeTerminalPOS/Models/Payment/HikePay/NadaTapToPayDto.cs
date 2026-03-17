using System;
using Newtonsoft.Json;
using System.Web;
using HikePOS.Models.Payment;
using System.Globalization;
namespace HikePOS.Models.Payment
{
    public class AcquirerTransactionID
    {
        public string TransactionID { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class AmountsResp
    {
        public string Currency { get; set; }
        public decimal AuthorizedAmount { get; set; }
        public decimal TotalRebatesAmount { get; set; }
        public decimal TotalFeesAmount { get; set; }
        public decimal CashBackAmount { get; set; }
        public decimal TipAmount { get; set; }
    }

    public class CardData
    {
        public SensitiveCardData SensitiveCardData { get; set; }
        public string PaymentBrand { get; set; }
        public List<string> EntryMode { get; set; }
        public string MaskedPan { get; set; }
        public string CardCountryCode { get; set; }
    }

    public class MessageHeader
    {
        public string ProtocolVersion { get; set; }
        public string ServiceID { get; set; }
        public string MessageClass { get; set; }
        public string MessageCategory { get; set; }
        public string SaleID { get; set; }
        public string MessageType { get; set; }
        public string POIID { get; set; }
    }
    

    public class OutputContent
    {
        public List<OutputText> OutputText { get; set; }
        public string OutputFormat { get; set; }
    }

    public class OutputText
    {
        public string Text { get; set; }
        public string CharacterStyle { get; set; }
        public bool EndOfLineFlag { get; set; }
    }

    public class PaymentAcquirerData
    {
        public string MerchantID { get; set; }
        public string AcquirerPOIID { get; set; }
        public AcquirerTransactionID AcquirerTransactionID { get; set; }
        public string ApprovalCode { get; set; }
    }

    public class PaymentInstrumentData
    {
        public string PaymentInstrumentType { get; set; }
        public CardData CardData { get; set; }
    }

    public class PaymentReceipt
    {
        public bool? RequiredSignatureFlag { get; set; }
        public string DocumentQualifier { get; set; }
        public OutputContent OutputContent { get; set; }
    }

    public class PaymentResponse
    {
        public POIData POIData { get; set; }
        public SaleData SaleData { get; set; }
        public List<PaymentReceipt> PaymentReceipt { get; set; }
        public PaymentAdditionaResponse Response { get; set; }
        public NadaPaymentResult PaymentResult { get; set; }
        public decimal ReversedAmount { get; set; }

    }

    public class NadaPaymentResult
    {
        public bool OnlineFlag { get; set; }
        public AmountsResp AmountsResp { get; set; }
        public PaymentAcquirerData PaymentAcquirerData { get; set; }
        public PaymentInstrumentData PaymentInstrumentData { get; set; }
    }

    public class POIData
    {
        public string POIReconciliationID { get; set; }
        public POITransactionID POITransactionID { get; set; }
    }

    public class POITransactionID
    {
        public string TransactionID { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class PaymentAdditionaResponse
    {
        public string AdditionalResponse { get; set; }
        public string ErrorCondition { get; set; }
        public string Result { get; set; }
        [JsonIgnore]
        public string ErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(AdditionalResponse))
                    return ErrorCondition;

                var query = HttpUtility.ParseQueryString(AdditionalResponse);
                var message = query["message"];

                return string.IsNullOrEmpty(message) ? ErrorCondition : $"{ErrorCondition}: {message}";
            }
        }
        [JsonIgnore]
        public string MerchantReference
        {
            get
            {

                var query = HttpUtility.ParseQueryString(AdditionalResponse);
                var merchantReference = query["merchantReference"];
                return merchantReference;
            }
        }
        [JsonIgnore]
        public string PSPReference
        {
            get
            {

                var query = HttpUtility.ParseQueryString(AdditionalResponse);
                var pspReference = query["pspReference"];
                return pspReference;
            }
        }
        [JsonIgnore]
        public decimal SurchargeAmount
        {
            get
            {
                var query = HttpUtility.ParseQueryString(AdditionalResponse);
                var surchargeMinor = query["surchargeAmount"];

                if (string.IsNullOrWhiteSpace(surchargeMinor))
                    return 0m;

                if (!long.TryParse(surchargeMinor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minorUnits))
                {
                    return 0m;
                }

                // Convert cents → dollars
                return minorUnits / 100m;
            }
        }
        [JsonIgnore]
        public string PaymentMethod
        {
            get
            {

                var query = HttpUtility.ParseQueryString(AdditionalResponse);
                var paymentMethod = query["paymentMethod"];
                return paymentMethod;
            }
        }
    }
   
    public class SaleData
    {
        public SaleTransactionID SaleTransactionID { get; set; }
        public string SaleToAcquirerData { get; set; }
    }

    public class SaleToPOIResponse
    {
        [JsonIgnore]
        public PaymentResponse TransactionResponse
        {
            get
            {
                return PaymentResponse != null ? PaymentResponse : ReversalResponse;
            }
        }
        public PaymentResponse PaymentResponse { get; set; }

        public PaymentResponse ReversalResponse { get; set; }

        public MessageHeader MessageHeader { get; set; }
    }

    public class SaleTransactionID
    {
        public string TransactionID { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class SensitiveCardData
    {
        public string CardSeqNumb { get; set; }
        public string ExpiryDate { get; set; }
    }

    public class NadaTapToPayDto
    {
        public SaleToPOIResponse SaleToPOIResponse { get; set; }
    }
}

public class HikePayFromCloud
{
    public bool paymentStatusSuccess { get; set; }
    public string poiTransactionId { get; set; }
    public DateTime poiTransactionTimeStamp { get; set; }
    public object saleTransactionId { get; set; }
    public object saleTransactionTimeStamp { get; set; }
    public string refusalReason { get; set; }
    public string additonalResponse { get; set; }
    public object paymentReceipt { get; set; }
    public static NadaTapToPayDto MapToNada(HikePayFromCloud cloud)
    {
        if (cloud == null) return null;

        var dto = JsonConvert.DeserializeObject<AdditionalResponse>(cloud.additonalResponse);
        var additionalData = dto?.additionalData;

        var nada = new NadaTapToPayDto
        {
            SaleToPOIResponse = new SaleToPOIResponse
            {
                MessageHeader = new MessageHeader
                {
                    ProtocolVersion = "3.0",
                    MessageClass = "Service",
                    MessageCategory = "Payment",
                    MessageType = "Response",
                    SaleID = cloud.saleTransactionId?.ToString(),
                    POIID = "CloudPOI",
                    ServiceID = Guid.NewGuid().ToString()
                },
                PaymentResponse = new PaymentResponse
                {
                    POIData = new POIData
                    {
                        POITransactionID = new POITransactionID
                        {
                            TransactionID = cloud.poiTransactionId,
                            TimeStamp = cloud.poiTransactionTimeStamp
                        }
                    },
                    SaleData = new SaleData
                    {
                        SaleTransactionID = new SaleTransactionID
                        {
                            TransactionID = cloud.saleTransactionId?.ToString(),
                            TimeStamp = cloud.saleTransactionTimeStamp is DateTime dt ? dt : DateTime.MinValue
                        }
                    },
                    Response = new PaymentAdditionaResponse
                    {
                        Result = cloud.paymentStatusSuccess ? "Success" : "Failure",
                        ErrorCondition = cloud.refusalReason,
                        AdditionalResponse = additionalData != null
                            ? JsonConvert.SerializeObject(additionalData)
                            : null
                    },
                    PaymentResult = new NadaPaymentResult
                    {
                        OnlineFlag = cloud.paymentStatusSuccess,
                        AmountsResp = MapAmounts(additionalData),
                        PaymentAcquirerData = new PaymentAcquirerData
                        {
                            MerchantID = additionalData?.mid,
                            AcquirerPOIID = additionalData?.tid,
                            AcquirerTransactionID = new AcquirerTransactionID
                            {
                                TransactionID = additionalData?.transactionReferenceNumber,
                                TimeStamp = additionalData?.iso8601TxDate ?? DateTime.MinValue
                            },
                            ApprovalCode = additionalData?.acquirerResponseCode
                        },
                        PaymentInstrumentData = new PaymentInstrumentData
                        {
                            PaymentInstrumentType = additionalData?.paymentMethod,
                            CardData = new CardData
                            {
                                MaskedPan = additionalData?.cardSummary,
                                PaymentBrand = additionalData?.cardScheme,
                                CardCountryCode = additionalData?.cardIssuerCountryId,
                                EntryMode = new List<string> { additionalData?.posEntryMode },
                                SensitiveCardData = new SensitiveCardData
                                {
                                    CardSeqNumb = additionalData?.cardIssueNumber,
                                    ExpiryDate = $"{additionalData?.expiryMonth}/{additionalData?.expiryYear}"
                                }
                            }
                        }
                    },
                    PaymentReceipt = cloud.paymentReceipt != null
                        ? new List<PaymentReceipt>
                        {
                            new PaymentReceipt
                            {
                                DocumentQualifier = "CloudReceipt",
                                RequiredSignatureFlag = false,
                                OutputContent = new OutputContent
                                {
                                    OutputFormat = "PlainText",
                                    OutputText = new List<OutputText>
                                    {
                                        new OutputText
                                        {
                                            Text = cloud.paymentReceipt.ToString(),
                                            CharacterStyle = "Normal",
                                            EndOfLineFlag = true
                                        }
                                    }
                                }
                            }
                        }
                        : null
                }
            }
        };

        return nada;
    }

    private static AmountsResp MapAmounts(AdditionalData data)
    {
        if (data == null) return null;

        return new AmountsResp
        {
            Currency = data.posAuthAmountCurrency,
            AuthorizedAmount = decimal.TryParse(data.posAuthAmountValue, out var authAmt) ? authAmt : 0,
            TipAmount = decimal.TryParse(data.posAmountGratuityValue, out var tipAmt) ? tipAmt : 0,
            CashBackAmount = decimal.TryParse(data.posAmountCashbackValue, out var cashAmt) ? cashAmt : 0,
            TotalFeesAmount = 0, // Map if available
            TotalRebatesAmount = 0 // Map if available
        };
    }
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

    [JsonProperty("posadditionalamounts.originalAmountCurrency")]
    public string posadditionalamountsoriginalAmountCurrency { get; set; }

    [JsonProperty("posadditionalamounts.originalAmountValue")]
    public string posadditionalamountsoriginalAmountValue { get; set; }
    public string pspReference { get; set; }
    public string shopperCountry { get; set; }
    public string tc { get; set; }
    public string tid { get; set; }
    public string transactionReferenceNumber { get; set; }
    public string transactionType { get; set; }
    public string txdate { get; set; }
    public string txtime { get; set; }
    public string unconfirmedBatchCount { get; set; }
}

public class AdditionalResponse
{
    public AdditionalData additionalData { get; set; }
    public string store { get; set; }
}
