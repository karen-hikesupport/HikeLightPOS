using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{

    #region Payment Request

    public class RegisterPaymentOption
    {
        public string registerName { get; set; }
        public int registerId { get; set; }
        public string paymentOptionName { get; set; }
        public int paymentOptionId { get; set; }
    }

  
    public class EconduitRequestObject
    {
        public PaymentOptionDto paymentOption { get; set; }
        public string refID { get; set; }
        public decimal amount { get; set; }
        public string command { get; set; }
    }

    #endregion



    #region Response Models

    public class JsonSerializer
    {
        public object dateFormat { get; set; }
        public object rootElement { get; set; }
        public object @namespace { get; set; }
        public string contentType { get; set; }
    }

    public class XmlSerializer
    {
        public object rootElement { get; set; }
        public object @namespace { get; set; }
        public object dateFormat { get; set; }
        public string contentType { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public object value { get; set; }
        public int type { get; set; }
        public object contentType { get; set; }
    }

    public class Method
    {
        public string Name { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string Signature { get; set; }
        public string Signature2 { get; set; }
        public int MemberType { get; set; }
        public object GenericArguments { get; set; }
    }

    public class Target
    {
    }

    public class OnBeforeDeserialization
    {
        public Method method { get; set; }
        public Target target { get; set; }
    }

 

    public class Cooky
    {
        public string comment { get; set; }
        public object commentUri { get; set; }
        public bool discard { get; set; }
        public string domain { get; set; }
        public bool expired { get; set; }
        public DateTime expires { get; set; }
        public bool httpOnly { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string port { get; set; }
        public bool secure { get; set; }
        public DateTime timeStamp { get; set; }
        public string value { get; set; }
        public int version { get; set; }
    }

    public class Header
    {
        public string name { get; set; }
        public string value { get; set; }
        public int type { get; set; }
        public object contentType { get; set; }
    }

    public class ProtocolVersion
    {
        public int major { get; set; }
        public int minor { get; set; }
        public int build { get; set; }
        public int revision { get; set; }
        public int majorRevision { get; set; }
        public int minorRevision { get; set; }
    }

    public class econduitPaymentRequest
    {
        public object userState { get; set; }
        public bool alwaysMultipartFormData { get; set; }
        public JsonSerializer jsonSerializer { get; set; }
        public XmlSerializer xmlSerializer { get; set; }
        public object responseWriter { get; set; }
        public bool useDefaultCredentials { get; set; }
        public List<Parameter> parameters { get; set; }
        public List<object> files { get; set; }
        public int method { get; set; }
        public object resource { get; set; }
        public int requestFormat { get; set; }
        public object rootElement { get; set; }
        public OnBeforeDeserialization onBeforeDeserialization { get; set; }
        public object dateFormat { get; set; }
        public object xmlNamespace { get; set; }
        public object credentials { get; set; }
        public int timeout { get; set; }
        public int readWriteTimeout { get; set; }
        public int attempts { get; set; }
    }

    public class EconduitResponse : Object
    {
        //Ticket start:#55427 Zara Haghi - Eftpos Problem UPDATED.by rupesh
        //public econduitPaymentRequest request { get; set; }
        //public string contentType { get; set; }
        //public int contentLength { get; set; }
        //public string contentEncoding { get; set; }
        public string content { get; set; }
        //public int statusCode { get; set; }
        //public bool isSuccessful { get; set; }
        //public string statusDescription { get; set; }
        //public string rawBytes { get; set; }
        //public string responseUri { get; set; }
        //public string server { get; set; }
        //public List<Cooky> cookies { get; set; }
        //public List<Header> headers { get; set; }
        //public int responseStatus { get; set; }
        //public object errorMessage { get; set; }
        //public object errorException { get; set; }
        //public ProtocolVersion protocolVersion { get; set; }
        //Ticket end:#55427 .by rupesh
    }


    #endregion


    public class EconduitContentResponse
    {

        public int TerminalID { get; set; }
        public bool ResultFinal { get; set; }
        public object AuthCode { get; set; }
        public string TransType { get; set; }
        public double Amount { get; set; }
        public string AmountString { get; set; }
        public string CardType { get; set; }
        public object Last4 { get; set; }
        public string Name { get; set; }
        public int CashBack { get; set; }
        public string CashBackString { get; set; }
        public string RefID { get; set; }
        public string receiptTerminal { get; set; }
        public object merchantReceipt { get; set; }
        public object CardToken { get; set; }
        public int GiftCardBalance { get; set; }
        public string GiftCardBalanceString { get; set; }
        public object ProcessorExtraData1 { get; set; }
        public object ProcessorExtraData2 { get; set; }
        public object ProcessorExtraData3 { get; set; }
        public object SignatureData { get; set; }
        public object Track1 { get; set; }
        public object Track2 { get; set; }
        public object EMV_TC { get; set; }
        public object EMV_TVR { get; set; }
        public object EMV_AID { get; set; }
        public object EMV_TSI { get; set; }
        public object EMV_ATC { get; set; }
        public object EMV_App_Label { get; set; }
        public object EMV_App_Name { get; set; }
        public object EMV_ARC { get; set; }
        public int EMV_CVM { get; set; }
        public object CardBin { get; set; }
        public string EntryMethod { get; set; }
        public object PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime TransactionDateUtc { get; set; }
        public object InvoiceNumber { get; set; }
        public object ExpDate { get; set; }
        public object GiftPAN { get; set; }
        public string ResultCode { get; set; }
        public string Message { get; set; }
    }

    public class EconduitCloseRootObject
    {
        public int TerminalID { get; set; }
        public string RefID { get; set; }
        public string ResultCodeCredit { get; set; }
        public string ResultCodeGift { get; set; }
        public string MessageCredit { get; set; }
        public string MessageGift { get; set; }
        public string BatchNumCredit { get; set; }
        public string BatchNumGift { get; set; }
        public int TransCntTotal { get; set; }
        public int AmtTotal { get; set; }
        public string AmtTotalString { get; set; }
        public int TransCntCredit { get; set; }
        public int AmtCredit { get; set; }
        public string AmtCreditString { get; set; }
        public int TransCntGift { get; set; }
        public int AmtGift { get; set; }
        public string AmtGiftString { get; set; }
    }

}
