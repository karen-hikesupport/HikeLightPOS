using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HikePOS.Models
{
	public class InventoryDto
	{
		public Int32 ProductId { get; set; }
		public string ProductName { get; set; }
		public string Sku { get; set; }
		public string BarCode { get; set; }
		public string PurchaseCode { get; set; }
		public int TotalStock { get; set; }


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

        [JsonIgnore]
        public decimal Awaitingstock { get; set; }
        [JsonProperty("Awaitingstock")]
        public string RoundedAwaitingstock
        {
            get
            {
                return Awaitingstock.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                Awaitingstock = result;
            }
        }

        [JsonIgnore]
        public decimal Committedstock { get; set; }
        [JsonProperty("Committedstock")]
        public string RoundedCommittedstock
        {
            get
            {
                return Committedstock.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                Committedstock = result;
            }
        }


		public decimal UnitCost { get; set; }
		public decimal SellingPrice { get; set; }

		public string SupplierCode { get; set; }

        [JsonIgnore]
        public decimal stock { get; set; }
        [JsonProperty("stock")]
        public string Roundedstock
        {
            get
            {
                return stock.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                stock = result;
            }
        }

        [JsonIgnore]
        public decimal ReorderLevel { get; set; }
        [JsonProperty("ReorderLevel")]
        public string RoundedReorderLevel
        {
            get
            {
                return ReorderLevel.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                ReorderLevel = result;
            }
        }

        [JsonIgnore]
        public decimal ReorderValue { get; set; }
        [JsonProperty("ReorderValue")]
        public string RoundedReorderValue
        {
            get
            {
                return ReorderValue.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                ReorderValue = result;
            }
        }

        [JsonIgnore]
        public decimal BackOrderQuantity { get; set; }
        [JsonProperty("BackOrderQuantity")]
        public string RoundedBackOrderQuantity
        {
            get
            {
                return BackOrderQuantity.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                BackOrderQuantity = result;
            }
        }

		public ObservableCollection<ProductOutletDto_POS> Stocks { get; set; }

		//public ObservableCollection<StockTakeDetailDto> StockTakeDtl { get; set; }

		public string AppliedPOReference { get; set; }

		public bool IsBackOrder { get; set; }

	}
}
