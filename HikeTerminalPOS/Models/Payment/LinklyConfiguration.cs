using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HikePOS.Models.Payment
{


    #region linkly Configuration
    public class LinklyConfigurationDTO
    {
        public string pairCode { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string secret { get; set; }
        public string linklyToken { get; set; }
    }
    #endregion


    #region Linkly sale request

    public class LinklyItem
    {
        public string id { get; set; }
        public string sku { get; set; }
        public int qty { get; set; }
        public int amt { get; set; }
        public int tax { get; set; }
        public int dis { get; set; }
        public string name { get; set; }
    }

    public class Basket
    {
        public string id { get; set; }
        public int amt { get; set; }
        public int tax { get; set; }
        public int dis { get; set; }
        public int sur { get; set; }
        public List<LinklyItem> items { get; set; }
    }

    public class PurchaseAnalysisData
    {
        public string OPR { get; set; }
        public int AMT { get; set; }
        public string PCM { get; set; }
    }

    public class LinklyRequest
    {
        public bool EnableTip { get; set; }
        public string Merchant { get; set; }
        public string TxnType { get; set; }
        public int AmtPurchase { get; set; }
        public long TxnRef { get; set; }
        public string CurrencyCode { get; set; }
        public int CutReceipt { get; set; }
        public int ReceiptAutoPrint { get; set; }
        public string Application { get; set; }
        public Basket Basket { get; set; }
        public PurchaseAnalysisData PurchaseAnalysisData { get; set; }
    }

    public class LinklySaleRootRequest
    {
        public LinklyRequest Request { get; set; }
    }

    #endregion


    #region linkly Response request
    
    public class LinklyRefundRequest
    {
        public bool EnableTip { get; set; }
        public string Merchant { get; set; }
        public string TxnType { get; set; }
        public int AmtPurchase { get; set; }
        public string TxnRef { get; set; }
        public string CurrencyCode { get; set; }
        public string CutReceipt { get; set; }
        public string ReceiptAutoPrint { get; set; }
        public string App { get; set; }
    }

    public class LinklyRefundRootRequest
    {
        public LinklyRefundRequest Request { get; set; }
    }


    #endregion

    #region Linkly response
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TxnFlags
    {
        public string offline { get; set; }
        public string receiptPrinted { get; set; }
        public string cardEntry { get; set; }
        public string commsMethod { get; set; }
        public string currency { get; set; }
        public string payPass { get; set; }
        public string undefinedFlag6 { get; set; }
        public string undefinedFlag7 { get; set; }
    }

    public class LinklyResponsePurchaseAnalysisData
    {
        public object rfn { get; set; }
        public object @ref { get; set; }
        public object hrc { get; set; }
        public object hrt { get; set; }
        public string sur { get; set; }
        public string amt { get; set; }
    }

    public class LinklyResponse
    {
        public string txnType { get; set; }
        public string merchant { get; set; }
        public string cardType { get; set; }
        public string cardName { get; set; }
        public string rrn { get; set; }
        public DateTime dateSettlement { get; set; }
        public int amtCash { get; set; }
        public decimal amtPurchase { get; set; }
        public int amtTip { get; set; }
        public int authCode { get; set; }
        public string txnRef { get; set; }
        public string pan { get; set; }
        public string dateExpiry { get; set; }
        public string track2 { get; set; }
        public string accountType { get; set; }
        public TxnFlags txnFlags { get; set; }
        public bool balanceReceived { get; set; }
        public int availableBalance { get; set; }
        public int clearedFundsBalance { get; set; }
        public bool success { get; set; }
        public string responseCode { get; set; }
        public string responseText { get; set; }
        public DateTime date { get; set; }
        public string catid { get; set; }
        public string caid { get; set; }
        public int stan { get; set; }
        public LinklyResponsePurchaseAnalysisData purchaseAnalysisData { get; set; }
    }

    public class LinklyResponseRoot
    {
        public string sessionId { get; set; }
        public string responseType { get; set; }
        public LinklyResponse response { get; set; }
        public string statusCode { get; set; }
        public string statusDescription { get; set; }
        public object modelErrors { get; set; }
    }



    #endregion

}
