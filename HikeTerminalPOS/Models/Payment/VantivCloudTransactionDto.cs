using System;
using System.Collections.Generic;

namespace HikePOS.Models.Payment
{

    #region VantivClould request 
    public class VantivClouldRequestObject
    {
    
        public int requestType { get; set; }

        public decimal transactionAmount { get; set; }

        public VantivClouldConfiguration vantivConfiguration { get; set; }
    }


    
    public class VantivClouldConfiguration
    {
       
        public string laneId { get; set; }

        public string accountId { get; set; }

        public string accountToken { get; set; }

        public string acceptorId { get; set; }
    }

    #endregion

    /*

    #region VantivClould response


    public class VantivClouldResponseObject
    {
        public VantivClouldResult result { get; set; }
        public object targetUrl { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public bool unAuthorizedRequest { get; set; }
        public bool __abp { get; set; }
    }


    public class VantivClouldResult
    {
        public string transactionReferenceNo { get; set; }
        public List<string> _errors { get; set; }
        public bool _hasErrors { get; set; }
        public List<Link> _links { get; set; }
        public List<object> _logs { get; set; }
        public Processor _processor { get; set; }
        public string _type { get; set; }
        public List<object> _warnings { get; set; }
        public string accountNumber { get; set; }
        public string approvalNumber { get; set; }
        public double approvedAmount { get; set; }
        public double balanceAmount { get; set; }
        public object balanceCurrencyCode { get; set; }
        public string binValue { get; set; }
        public string cardHolderName { get; set; }
        public string cardLogo { get; set; }
        public double cashbackAmount { get; set; }
        public double convenienceFeeAmount { get; set; }
        public string currencyCode { get; set; }
        public double debitSurchargeAmount { get; set; }
        public object ebtType { get; set; }
        public Emv emv { get; set; }
        public string entryMode { get; set; }
        public string expirationMonth { get; set; }
        public string expirationYear { get; set; }
        public string fsaCard { get; set; }
        public bool isApproved { get; set; }
        public bool isCardInserted { get; set; }
        public bool isOffline { get; set; }
        public string merchantId { get; set; }
        public object networkLabel { get; set; }
        public string paymentType { get; set; }
        public bool pinVerified { get; set; }
        public Signature signature { get; set; }
        public string statusCode { get; set; }
        public double subTotalAmount { get; set; }
        public string terminalId { get; set; }
        public double tipAmount { get; set; }
        public double totalAmount { get; set; }
        public DateTime transactionDateTime { get; set; }
        public string transactionId { get; set; }
    }


    public class Link
    {
        public string href { get; set; }
        public string method { get; set; }
        public string rel { get; set; }
    }


    public class Processor
    {
        public string expressResponseCode { get; set; }
        public string expressResponseMessage { get; set; }
        public string hostResponseCode { get; set; }
        public object hostResponseMessage { get; set; }
        public List<string> logs { get; set; }
        public List<string> processorLogs { get; set; }
        public string processorRawResponse { get; set; }
        public string processorReferenceNumber { get; set; }
        public bool processorRequestFailed { get; set; }
        public bool processorRequestWasApproved { get; set; }
        public string processorResponseCode { get; set; }
        public string processorResponseMessage { get; set; }
        public string rawResponse { get; set; }
    }

    public class Signature
    {
        public object data { get; set; }
        public object format { get; set; }
        public string statusCode { get; set; }
    }


    public class Emv
    {
       
        public string ApplicationIdentifier { get; set; }

       
        public string ApplicationLabel { get; set; }

        public string ApplicationPreferredName { get; set; }

        public string ApplicationTransactionCounter { get; set; }

        public string Cryptogram { get; set; }

        public string IssuerCodeTableIndex { get; set; }

        public bool PinBypassed { get; set; }
    }

    #endregion
    */


    public class Emv
    {

        public string ApplicationIdentifier { get; set; }


        public string ApplicationLabel { get; set; }

        public string ApplicationPreferredName { get; set; }

        public string ApplicationTransactionCounter { get; set; }

        public string Cryptogram { get; set; }

        public string IssuerCodeTableIndex { get; set; }

        public bool PinBypassed { get; set; }
    }


    public class Error
    {
        public string developerMessage { get; set; }
        public string errorType { get; set; }
        public string exceptionMessage { get; set; }
        public string exceptionTypeFullName { get; set; }
        public string exceptionTypeShortName { get; set; }
        public string userMessage { get; set; }
    }

    public class Processor
    {
        public object expressResponseCode { get; set; }
        public object expressResponseMessage { get; set; }
        public object hostResponseCode { get; set; }
        public object hostResponseMessage { get; set; }
        public List<string> logs { get; set; }
        public List<string> processorLogs { get; set; }
        public object processorRawResponse { get; set; }
        public object processorReferenceNumber { get; set; }
        public bool processorRequestFailed { get; set; }
        public bool processorRequestWasApproved { get; set; }
        public string processorResponseCode { get; set; }
        public object processorResponseMessage { get; set; }
        public object rawResponse { get; set; }
    }

    public class Signature
    {
        public object data { get; set; }
        public object format { get; set; }
        public string statusCode { get; set; }
    }

    public class VantivClouldResult
    {
        public string transactionReferenceNo { get; set; }
        public List<Error> _errors { get; set; }
        public bool _hasErrors { get; set; }
        public List<object> _links { get; set; }
        public List<object> _logs { get; set; }
        public Processor _processor { get; set; }
        public string _type { get; set; }
        public List<object> _warnings { get; set; }
        public object accountNumber { get; set; }
        public object approvalNumber { get; set; }
        public double approvedAmount { get; set; }
        public double balanceAmount { get; set; }
        public object balanceCurrencyCode { get; set; }
        public object binValue { get; set; }
        public object cardHolderName { get; set; }
        public object cardLogo { get; set; }
        public double cashbackAmount { get; set; }
        public double convenienceFeeAmount { get; set; }
        public string currencyCode { get; set; }
        public double debitSurchargeAmount { get; set; }
        public object ebtType { get; set; }
        public Emv emv { get; set; }
        public string entryMode { get; set; }
        public object expirationMonth { get; set; }
        public object expirationYear { get; set; }
        public string fsaCard { get; set; }
        public bool isApproved { get; set; }
        public bool isCardInserted { get; set; }
        public bool isOffline { get; set; }
        public string merchantId { get; set; }
        public object networkLabel { get; set; }
        public string paymentType { get; set; }
        public bool pinVerified { get; set; }
        public Signature signature { get; set; }
        public string statusCode { get; set; }
        public double subTotalAmount { get; set; }
        public string terminalId { get; set; }
        public double tipAmount { get; set; }
        public double totalAmount { get; set; }
        public DateTime transactionDateTime { get; set; }
        public object transactionId { get; set; }
    }

    public class VantivClouldResponseObject
    {
        public VantivClouldResult result { get; set; }
        public object targetUrl { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public bool unAuthorizedRequest { get; set; }
        public bool __abp { get; set; }
    }
}
