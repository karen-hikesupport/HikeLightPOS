using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{
    public class Amount
    {
        [JsonProperty("amount")]
        public decimal Amount1 { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class Price
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class OrderItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sku")]
        public string Sku { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        [JsonProperty("price")]
        public Price Price { get; set; }
    }

    public class Request
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }
        [JsonProperty("requestedAt")]
        public DateTime RequestedAt { get; set; }
        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        //[JsonProperty("orderMerchantReference")]
        //public string OrderMerchantReference { get; set; } = null;
        [JsonProperty("preApprovalCode")]
        public string PreApprovalCode { get; set; }
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
        [JsonProperty("orderItems")]
        public List<OrderItem> OrderItems { get; set; }

    }

    public class AfterPayRequestObject
    {

        public int PaymentOptionId { get; set; }
        public Request Request { get; set; }
    }


  


    public class Result
    {
        [JsonProperty("orderId")]
        public object OrderId { get; set; }

        [JsonProperty("orderedAt")]
        public object OrderedAt { get; set; }
        [JsonProperty("orderItems")]
        public string ErrorCode { get; set; }
        [JsonProperty("errorId")]
        public string ErrorId { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("httpStatusCode")]
        public int HttpStatusCode { get; set; }


    }
    public class AfterPayResponseRootObject : Object
    {
        [JsonProperty("result")]
        public Result Result { get; set; }
        [JsonProperty("targetUrl")]
        public object TargetUrl { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error")]
        public object Error { get; set; }
        [JsonProperty("unAuthorizedRequest")]
        public bool UnAuthorizedRequest { get; set; }
        [JsonProperty("__abp")]
        public bool Abp { get; set; }



    }



    #region AfterPay Refund


    public class Request1
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("requestedAt")]
        public DateTime RequestedAt { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        //[JsonProperty("orderMerchantReference")]
        //public string OrderMerchantReference { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("amount")]
        public Amount Amount1 { get; set; }
    }

    public class AfterpayRefundRequestRootObject
    {
       
        public int PaymentOptionId { get; set; }


        public Request1 Request { get; set; }
    }


    public class AfterpayRefundResponseRootObject
    {

        [JsonProperty("result")]
        public Result1 Result { get; set; }

        [JsonProperty("targetUrl")]
        public object TargetUrl { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public object Error { get; set; }

        [JsonProperty("unAuthorizedRequest")]
        public bool UnAuthorizedRequest { get; set; }

        [JsonProperty("__abp")]
        public bool Abp { get; set; }

    }



    public class Result1
    {
        [JsonProperty("refundId")]
        public object RefundId { get; set; }

        [JsonProperty("refundedAt")]
        public object RefundedAt { get; set; }

        [JsonProperty("errorCode")]
        public object ErrorCode { get; set; }

        [JsonProperty("errorId")]
        public object ErrorId { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        [JsonProperty("httpStatusCode")]
        public int HttpStatusCode { get; set; }


    }
    #endregion


    //public class AfterpayPaymentValue
    //{
    //    public string orderId { get; set; }
    //    public string orderedAt { get; set; }
    //    public string APIKey { get; set; }
    //    public string ApiPassword { get; set; }
    //    public string TerminalID { get; set; }
    //    public bool IsPaired { get; set; } = false;

    //}

}
