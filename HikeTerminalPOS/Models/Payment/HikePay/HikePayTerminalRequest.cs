using System;
using Newtonsoft.Json;
using System.Web;
using HikePOS.Models.Payment;
using System.Globalization;
namespace HikePOS.Models.Payment
{
    public class HikePayTerminalRequest
    {
        public SaleToPOIRequest SaleToPOIRequest { get; set; }
        public int PaymentId { get; set; }
        public string SaleRefundTransactionId { get; set; }
        public string PoiTransactionId { get; set; }
        public int TenantId { get; set; }
        public string StoreId { get; set; }
        public string BalanceAccountId { get; set; }
        public string Reference { get; set; }
        public string MainSaleServiceId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal SurchargeAmount { get; set; }
        public bool isFullRefund { get; set; }
    }

    public class SaleToPOIRequest
    {
        public PaymentRequest MessagePayload { get; set; }
        public MessageHeaderTerminal MessageHeader { get; set; }
    }

    public class PaymentRequest
    {
        public SaleData SaleData { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; }
        public string AbortReason { get; set; }
        public MessageReference MessageReference { get; set; }
    }

    public class MessageReference
    {
        public string MessageCategory { get; set; }
        public string SaleID { get; set; }
        public string ServiceID { get; set; }
    }

    public class PaymentTransaction
    {
        public AmountsReq AmountsReq { get; set; }
        public object OriginalPOITransaction { get; set; }
        public object TransactionConditions { get; set; }
        public object SaleItem { get; set; }
    }

    public class AmountsReq
    {
        public string Currency { get; set; }
        public decimal RequestedAmount { get; set; }
        public bool? RequestedAmountSpecified { get; set; }
        public double? CashBackAmount { get; set; }
        public bool? CashBackAmountSpecified { get; set; }
        public double? TipAmount { get; set; }
        public bool? TipAmountSpecified { get; set; }
        public double? PaidAmount { get; set; }
        public bool? PaidAmountSpecified { get; set; }
        public double? MinimumAmountToDeliver { get; set; }
        public bool? MinimumAmountToDeliverSpecified { get; set; }
        public double? MaximumCashBackAmount { get; set; }
        public bool? MaximumCashBackAmountSpecified { get; set; }
        public double? MinimumSplitAmount { get; set; }
        public bool? MinimumSplitAmountSpecified { get; set; }
    }

    public class MessageHeaderTerminal
    {
        public string ProtocolVersion { get; set; }
        public string ServiceID { get; set; }
        public int MessageClass { get; set; }
        public int MessageCategory { get; set; }
        public string SaleID { get; set; }
        public int MessageType { get; set; }
        public string POIID { get; set; }
        public string DeviceID { get; set; }
    }
    public enum MessageCategoryType
    {
        Abort,
        Admin,
        BalanceInquiry,
        Batch,
        CardAcquisition,
        CardReaderAPDU,
        CardReaderInit,
        CardReaderPowerOff,
        Diagnosis,
        Display,
        EnableService,
        Event,
        GetTotals,
        Input,
        InputUpdate,
        Login,
        Logout,
        Loyalty,
        Payment,
        PIN,
        Print,
        Reconciliation,
        Reversal,
        Sound,
        StoredValue,
        TransactionStatus,
        Transmit
    }
    public enum MessageClassType
    {
        Service,
        Device,
        Event
    }
    public enum MessageHeaderTerminalMessageType
    {
        Request,
        Response,
        Notification
    }
    public class HikePayVerify
    {
        public string InvoiceSyncReference { get; set; }
        public string ServiceId { get; set; }
        public string SaleReference { get; set; }

        public int PaymentId { get; set; }


    }

}
