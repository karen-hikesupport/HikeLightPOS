using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{


    #region Windcave configuration
    public class WindcaveConfiguration
    {
        public string User { get; set; }
        public string Key { get; set; }
        public string StationId { get; set; }
        public bool integratedReceipt { get; set; }
    }

    #endregion

    #region Windcave request
    public class WindcaveRequest
    {
        public decimal amount { get; set; }
        public string cur { get; set; }
        public string txnType { get; set; }
        public string station { get; set; }
        public string txnRef { get; set; }
        public string deviceId { get; set; }
        public string posName { get; set; }
        public string posVersion { get; set; }
        public string vendorId { get; set; }
        public string mRef { get; set; }
        public string action { get; set; }
        public string user { get; set; }
        public string key { get; set; }
    }




    #endregion


    #region Windcave response


    /// <summary>
    ///  Same DTO is used for status check 
    /// </summary>
    ///

    public class B1
    {
        public string en { get; set; }
        public object text { get; set; }
    }

    public class B2
    {
        public string en { get; set; }
        public string text { get; set; }
    }

  
    public class WindcaveDetailResult
    {
        public object ac { get; set; }
        public string ap { get; set; }
        public object cn { get; set; }
        public object ct { get; set; }
        public object ch { get; set; }
        public object dt { get; set; }
        public object dT_TZ { get; set; }
        public object ds { get; set; }
        public object dS_TZ { get; set; }
        public object pix { get; set; }
        public object rid { get; set; }
        public object rrn { get; set; }
        public object st { get; set; }
        public object tr { get; set; }
        public object dbid { get; set; }
        public string rc { get; set; }
        public string rt { get; set; }
        public object rtt { get; set; }
        public object amtA { get; set; }
        public object amtS { get; set; }
        public object amtT { get; set; }
        public object amtC { get; set; }
        public object mid { get; set; }
        public object tid { get; set; }
        public object autoSig { get; set; }
        public object caStan { get; set; }
    }

    public class WindcaveResult
    {

        [JsonIgnore]
        public bool IsintegratedReceipt { get; set; }
        public object txnType { get; set; }
        public string statusId { get; set; }
        public object txnStatusId { get; set; }
        public string complete { get; set; }
        public object rcptW { get; set; }
        public string rcpt { get; set; }
        public string reCo { get; set; }
        public object tmo { get; set; }
        public string txnRef { get; set; }
        public WindcaveDetailResult result { get; set; }
        public string dL1 { get; set; }
        public string dL2 { get; set; }
        public B1 b1 { get; set; }
        public B2 b2 { get; set; }
       
    }


   
    public class WindcaveRoot  
    {
        
        public WindcaveResult result { get; set; }
        public object targetUrl { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public bool unAuthorizedRequest { get; set; }
        public bool __abp { get; set; }
    }



    #endregion



    #region Windcave status check


    public class WindcaveStatusCheckDTO
    {
        public string station { get; set; }
        public string txnType { get; set; }
        public string txnRef { get; set; }
        public string action { get; set; }
        public string user { get; set; }
        public string key { get; set; }
    }

    #endregion

    #region WindcaveButtonTransaction

    public class WindcaveButtonTransactionRequest 
    {
        public string station { get; set; }
        public string txnType { get; set; }
        public string uiType { get; set; }
        public string name { get; set; }
        public string val { get; set; }
        public string txnRef { get; set; }
        public string action { get; set; }
        public string user { get; set; }
        public string key { get; set; }
    }


    public class WindcaveButtonTransactionResult
    {
        public string txnType { get; set; }
        public string txnRef { get; set; }
        public string success { get; set; }
        public string rc { get; set; }
        public object result { get; set; }
    }

    public class WindcaveButtonTransactionRoot : AjaxResponse
    {
        public WindcaveButtonTransactionResult result { get; set; }
        
    }

    #endregion
}
