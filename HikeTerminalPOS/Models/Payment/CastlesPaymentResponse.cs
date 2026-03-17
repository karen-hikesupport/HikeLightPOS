using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{
    public class CastlesSaleRequest
    {
        public string txnPosTxnId { get; set; }
        public string txnType { get; set; }
        public string txnAmtBase { get; set; }

    }
    public class CastlesRefundRequest
    {
        public string txnPosTxnId { get; set; }
        public string txnType { get; set; }
        public string txnAmtTrans { get; set; }

    }
    public class CastlesPaymentResponse
    {

        public string TxnAmtBase { get; set; }
        public string TxnAmtCashback { get; set; }
        public string TxnAmtTip { get; set; }
        public string TxnAmtTrans { get; set; }
        public string TxnApprovalCode { get; set; }
        public string TxnCardBrand { get; set; }
        public string TxnDateTime { get; set; }
        public string TxnEntryMode { get; set; }
        public string TxnHostMsg { get; set; }
        public string TxnMaskedCardNum { get; set; }
        public string TxnMid { get; set; }
        public string TxnPosTxnId { get; set; }
        public string TxnReturnCode { get; set; }
        public string TxnStan { get; set; }
        public string TxnTid { get; set; }
        public string TxnType { get; set; }

        public string TxnAID { get; set; }
        public string APPEffectiveDate { get; set; }
        public string APPExpiredDate { get; set; }


    }

    public class CastlesConfigurationDto
    {
        public string AuthToken { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }



    }
}
