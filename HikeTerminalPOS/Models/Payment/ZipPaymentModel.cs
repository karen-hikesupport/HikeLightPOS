using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{
    public class ZipPurchaseRequestObject
    {
        [JsonProperty("refCode")]
        public string RefCode { get; set; }

        [JsonProperty("payAmount")]
        public decimal PayAmount { get; set; }

        [JsonProperty("originator")]
        public Originator Originator { get; set; }

        [JsonProperty("accountIdentifier")]
        public AccountIdentifier AccountIdentifier { get; set; }

        [JsonProperty("interestFreeMonths")]
        public string InterestFreeMonths { get; set; }

        [JsonProperty("order")]
        public Order Order { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("requiresAck")]
        public bool RequiresAck { get; set; }

        [JsonProperty("zipConfiguration")]
        public ZipConfiguration ZipConfiguration { get; set; }
    }

    public class AccountIdentifier
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("partner")]
        public string Partner { get; set; }
    }

    public class Order
    {
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("shippingAmount")]
        public decimal ShippingAmount { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public class Originator
    {
        [JsonProperty("locationId")]
        public string LocationId { get; set; }

        [JsonProperty("deviceRefCode")]
        public string DeviceRefCode { get; set; }

        [JsonProperty("staffActor")]
        public StaffActor StaffActor { get; set; }
    }

    public class StaffActor
    {
        [JsonProperty("refCode")]
        public string RefCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public  class ZipConfiguration
    {
        [JsonProperty("locationID")]
        public string LocationId { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
    }

    public class ZipResponeObject : Object

    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("locationId")]
        public long LocationId { get; set; }

        [JsonProperty("refCode")]
        public string RefCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("receipt_number")]
        public long ReceiptNumber { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("items")]
        public ItemResponse[] Items { get; set; }
    }

    public  class ItemResponse
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
    public class ZipRefundRequestObject
    {
        [JsonProperty("refCode")]
        public string RefCode { get; set; }

        [JsonProperty("refundAmount")]
        public decimal RefundAmount { get; set; }

        [JsonProperty("originator")]
        public Originator Originator { get; set; }

        [JsonProperty("zipConfiguration")]
        public ZipConfiguration ZipConfiguration { get; set; }
    }

}
