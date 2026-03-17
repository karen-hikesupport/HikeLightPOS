using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{
    public class ClearantRequest
    {
        public string amount { get; set; }
        public string type { get; set; }
        [JsonProperty("software-type")]
        public string SoftwareType { get; set; }
        public string invoice { get; set; }
        public string id { get; set; }
    }
    public class ClearantBatchRequest
    {
        public bool lastBatchClosedRequiredInResponse { get; set; }
        [JsonProperty("software-type")]
        public string SoftwareType { get; set; }
    }
    public class Link
    {
        public string rel { get; set; }
        public string href { get; set; }
        public string id { get; set; }
    }

    public class ClearantEmv
    {
        [JsonProperty("application-name")]
        public string ApplicationName { get; set; }
        [JsonProperty("application-id")]
        public string ApplicationId { get; set; }
        [JsonProperty("terminal-verification-results")]
        public string TerminalVerificationResults { get; set; }
        [JsonProperty("transaction-status-information")]
        public string TransactionStatusInformation { get; set; }
        public string iad { get; set; }
        [JsonProperty("transaction-certificate")]
        public string TransactionCertificate { get; set; }
        [JsonProperty("application-transaction-counter")]
        public string ApplicationTransactionCounter { get; set; }
    }

    public class Transaction
    {
        public string amount { get; set; }
        public string invoice { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string result { get; set; }
        [JsonProperty("authorization-code")]
        public string AuthorizationCode { get; set; }
        [JsonProperty("tip-amount")]
        public string TipAmount { get; set; }
        [JsonProperty("ref-id")]
        public string RefId { get; set; }
        [JsonProperty("batch-string-id")]
        public string BatchStringId { get; set; }
        [JsonProperty("display-message")]
        public string DisplayMessage { get; set; }
        [JsonProperty("result-code")]
        public string ResultCode { get; set; }
        [JsonProperty("exp-date")]
        public string ExpDate { get; set; }
        [JsonProperty("emv-entry-method")]
        public string EmvEntryMethod { get; set; }
        [JsonProperty("card-type")]
        public string CardType { get; set; }
        [JsonProperty("sales-tax-amount")]
        public string SalesTaxAmount { get; set; }
        [JsonProperty("last-four")]
        public string LastFour { get; set; }
        [JsonProperty("service-fee")]
        public string ServiceFee { get; set; }
        [JsonProperty("merchant-id")]
        public string MerchantId { get; set; }
        [JsonProperty("terminal-id")]
        public string TerminalId { get; set; }
        public ClearantEmv emv { get; set; }
        [JsonProperty("software-type")]
        public string SoftwareType { get; set; }
        public string cvm { get; set; }
    }

    public class Payload
    {
        public Transaction transaction { get; set; }
        public string payloadType { get; set; }
        public ClearantError error { get; set; }
    }
    public class ClearantError
    {
        [JsonProperty("result-code")]
        public string ResultCode { get; set; }

        [JsonProperty("error-message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("time-stamp")]
        public string TimeStamp { get; set; }

    }

    public class ClearantResponse
    {
        public string code { get; set; }
        public string status { get; set; }
        [JsonProperty("exchange-id")]
        public string ExchangeId { get; set; }
        public List<Link> links { get; set; }
        public Payload payload { get; set; }
    }
    public class ClearantConfiguration
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
        public bool ToCloseRegister { get; set; }

    }
    public class Batch
    {
        public string id { get; set; }
        public string status { get; set; }
        [JsonProperty("refund-count")]
        public string RefundCount { get; set; }
        [JsonProperty("refund-total")]
        public string RefundTotal { get; set; }
        [JsonProperty("sales-count")]
        public string SalesCount { get; set; }
        [JsonProperty("sales-total")]
        public string SalesTotal { get; set; }
        [JsonProperty("net-amount")]
        public string NetAmount { get; set; }
        [JsonProperty("total-count")]
        public string TotalCount { get; set; }
        [JsonProperty("merchant-id")]
        public string MerchantId { get; set; }
        [JsonProperty("terminal-id")]
        public string TerminalId { get; set; }
        [JsonProperty("date-opened")]
        public DateTime DateOpened { get; set; }
    }

    public class BatchPayload
    {
        public Batch batch { get; set; }
        public string payloadType { get; set; }
    }

    public class ClearantBatchResponse
    {
        public string code { get; set; }
        public string status { get; set; }
        [JsonProperty("exchange-id")]
        public string ExchangeId { get; set; }
        public BatchPayload payload { get; set; }
    }


}
