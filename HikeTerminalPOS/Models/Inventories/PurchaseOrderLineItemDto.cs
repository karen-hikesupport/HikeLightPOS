using System;
using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
	public class PurchaseOrderLineItemDto : FullAuditedPassiveEntityDto
	{
		public int PurchaseOrderId { get; set; }

		public int ProductId { get; set; }

		public int? ParentProductId { get; set; }

		public int Sequence { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

        [JsonIgnore]
        public decimal Quantity { get; set; }
        [JsonProperty("Quantity")]
        public string RoundedQuantity
        {
            get
            {
                return Quantity.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                Quantity = result;
            }
        }

        [JsonIgnore]
        public decimal ReceivedQty { get; set; }
        [JsonProperty("ReceivedQty")]
        public string RoundedReceivedQty
        {
            get
            {
                return ReceivedQty.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                ReceivedQty = result;
            }
        }

        [JsonIgnore]
        public decimal UpdatedReceivedQty { get; set; }
        [JsonProperty("UpdatedReceivedQty")]
        public string RoundedUpdatedReceivedQty
        {
            get
            {
                return UpdatedReceivedQty.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                UpdatedReceivedQty = result;
            }
        }

        [JsonIgnore]
        public decimal RemainingQty { get; set; }
        [JsonProperty("RemainingQty")]
        public string RoundedRemainingQty
        {
            get
            {
                return RemainingQty.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                RemainingQty = result;
            }
        }


		public decimal Price { get; set; }

		public decimal ActualPrice { get; set; }

		public decimal TotalAmount { get; set; }

		public int TaxId { get; set; }

		public string TaxName { get; set; }

		public decimal TaxRate { get; set; }

		public decimal TaxAmount { get; set; }

		public bool DiscountIsAsPercentage { get; set; }

		public decimal DiscountValue { get; set; }

		public decimal DiscountAmount { get; set; }

		public decimal EffectiveAmount { get; set; }

		public string PurchaseCode { get; set; }

		public string Sku { get; set; }

        [JsonIgnore]
        public decimal OnHandstock { get; set; }
        [JsonProperty("OnHandstock")]
        public string RoundedOnHandstock
        {
            get
            {
                return OnHandstock.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                OnHandstock = result;
            }
        }

		public PriceUpdateOptionType PriceUpdateOptionType { get; set; }

		public bool IsItemReceived { get; set; }

		public string SupplierCode { get; set; }

		public string POReferenceNo { get; set; }

		public string BarCode { get; set; }

		public int? SaleReferenceNo { get; set; }

        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        public PurchaseOrderItemType ItemType { get; set; }

        //Ticket start:#84288 iOS: FR:Backorder Management.by rupesh
        [JsonIgnore]
		private string _POCreatedNumber { get; set; }
        public string POCreatedNumber { get { return _POCreatedNumber; } set { _POCreatedNumber = value; SetPropertyChanged(nameof(POCreatedNumber)); } }
        [JsonIgnore]
		private OrderStatus _POLineStatus { get; set; }
        public OrderStatus POLineStatus { get { return _POLineStatus; } set { _POLineStatus = value; SetPropertyChanged(nameof(POLineStatus)); } }
        //Ticket end:#84288 .by rupesh

        public PurchaseOrderLineItemDB ToModel()
        {
            PurchaseOrderLineItemDB purchaseOrderLineItemDB = new PurchaseOrderLineItemDB
            {
                Id = Id,
                IsActive = IsActive,
                PurchaseOrderId = PurchaseOrderId,
                ProductId = ProductId,
                ParentProductId = ParentProductId,
                Sequence = Sequence,
                Title = Title,
                Description = Description,
                Price = Price,
                ActualPrice = ActualPrice,
                TotalAmount = TotalAmount,
                TaxId = TaxId,
                TaxName = TaxName,
                TaxRate = TaxRate,
                TaxAmount = TaxAmount,
                DiscountIsAsPercentage = DiscountIsAsPercentage,
                DiscountValue = DiscountValue,
                DiscountAmount = DiscountAmount,
                EffectiveAmount = EffectiveAmount,
                PurchaseCode = PurchaseCode,
                Sku = Sku,
                PriceUpdateOptionType = (int)PriceUpdateOptionType,
                IsItemReceived = IsItemReceived,
                SupplierCode = SupplierCode,
                POReferenceNo = POReferenceNo,
                BarCode = BarCode,
                SaleReferenceNo = SaleReferenceNo,
                ItemType = (int)ItemType,
                POCreatedNumber = POCreatedNumber,
                POLineStatus = (int)POLineStatus

            };
            return purchaseOrderLineItemDB;
        }
        public static PurchaseOrderLineItemDto FromModel(PurchaseOrderLineItemDB purchaseOrderLineItemDB)
        {
            PurchaseOrderLineItemDto purchaseOrderLineItemDto = new PurchaseOrderLineItemDto
            {
                Id = purchaseOrderLineItemDB.Id,
                IsActive = purchaseOrderLineItemDB.IsActive,
                PurchaseOrderId = purchaseOrderLineItemDB.PurchaseOrderId,
                ProductId = purchaseOrderLineItemDB.ProductId,
                ParentProductId = purchaseOrderLineItemDB.ParentProductId,
                Sequence = purchaseOrderLineItemDB.Sequence,
                Title = purchaseOrderLineItemDB.Title,
                Description = purchaseOrderLineItemDB.Description,
                Price = purchaseOrderLineItemDB.Price,
                ActualPrice = purchaseOrderLineItemDB.ActualPrice,
                TotalAmount = purchaseOrderLineItemDB.TotalAmount,
                TaxId = purchaseOrderLineItemDB.TaxId,
                TaxName = purchaseOrderLineItemDB.TaxName,
                TaxRate = purchaseOrderLineItemDB.TaxRate,
                TaxAmount = purchaseOrderLineItemDB.TaxAmount,
                DiscountIsAsPercentage = purchaseOrderLineItemDB.DiscountIsAsPercentage,
                DiscountValue = purchaseOrderLineItemDB.DiscountValue,
                DiscountAmount = purchaseOrderLineItemDB.DiscountAmount,
                EffectiveAmount = purchaseOrderLineItemDB.EffectiveAmount,
                PurchaseCode = purchaseOrderLineItemDB.PurchaseCode,
                Sku = purchaseOrderLineItemDB.Sku,
                PriceUpdateOptionType = (PriceUpdateOptionType) purchaseOrderLineItemDB.PriceUpdateOptionType,
                IsItemReceived = purchaseOrderLineItemDB.IsItemReceived,
                SupplierCode = purchaseOrderLineItemDB.SupplierCode,
                POReferenceNo = purchaseOrderLineItemDB.POReferenceNo,
                BarCode = purchaseOrderLineItemDB.BarCode,
                SaleReferenceNo = purchaseOrderLineItemDB.SaleReferenceNo,
                ItemType = (PurchaseOrderItemType)purchaseOrderLineItemDB.ItemType,
                POCreatedNumber = purchaseOrderLineItemDB.POCreatedNumber,
                POLineStatus = (OrderStatus)purchaseOrderLineItemDB.POLineStatus

            };
            return purchaseOrderLineItemDto;
        }


    }
    public enum PurchaseOrderItemType
    {
        Standard = 0,
        UnityOfMeasure = 1,
    }
    //Ticket end:#20064 .by rupesh
    public partial class PurchaseOrderLineItemDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public int? ParentProductId { get; set; }
        public int Sequence { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public int TaxId { get; set; }
        public string TaxName { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public bool DiscountIsAsPercentage { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal EffectiveAmount { get; set; }
        public string PurchaseCode { get; set; }
        public string Sku { get; set; }
        public int PriceUpdateOptionType { get; set; }
        public bool IsItemReceived { get; set; }
        public string SupplierCode { get; set; }
        public string POReferenceNo { get; set; }
        public string BarCode { get; set; }
        public int? SaleReferenceNo { get; set; }
        public int ItemType { get; set; }
        public string POCreatedNumber { get; set; }
        public int POLineStatus { get; set; }


    }

}
