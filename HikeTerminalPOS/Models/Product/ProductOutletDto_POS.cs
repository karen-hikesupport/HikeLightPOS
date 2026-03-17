
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public partial class ProductOutletDto_POS 
    {
        public int? ParentProductId { get; set; }
        public int ProductId { get; set; }
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Markup { get; set; }
        public decimal PriceExcludingTax { get; set; }
        public int? TaxID { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal AvgCostPrice { get; set; }
        public decimal OnHandstock { get; set; }
        public decimal Awaitingstock { get; set; }
        public decimal Committedstock { get; set; }

        //Start #84444 iOS: FR: remove item price at search bar that includes tax by Pratik
        [JsonIgnore]
        public decimal SearchPrice
        {
            get
            {
                return Helpers.Settings.StoreGeneralRule.TaxInclusive ? SellingPrice : PriceExcludingTax;
            }
        }
        //End #84444 by Pratik

        [JsonIgnore]
        public decimal Stock 
        {
            get
            {
                return OnHandstock - Committedstock;
            }
        }

        [JsonIgnore]
        public string Stock1
        {
            get
            {
                return Stock.ToString("0.##");
            }
        }

        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderValue { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }

        public ProductOutletDB_POS ToModel()
        {
            ProductOutletDB_POS model = new ProductOutletDB_POS
            {
                ParentProductId = ParentProductId,
                ProductId = ProductId,
                OutletId = OutletId,
                OutletName = OutletName,
                CostPrice = CostPrice,
                Markup = Markup,
                PriceExcludingTax = PriceExcludingTax,
                TaxID = TaxID,
                TaxName = TaxName,
                TaxRate = TaxRate,
                SellingPrice = SellingPrice,
                AvgCostPrice = AvgCostPrice,
                OnHandstock = OnHandstock,
                Awaitingstock = Awaitingstock,
                Committedstock = Committedstock,
                ReorderLevel = ReorderLevel,
                ReorderValue = ReorderValue,
                IsLocked = IsLocked,
                IsVisible = IsVisible
            };
            return model;
        }
        public static ProductOutletDto_POS FromModel(ProductOutletDB_POS dbmodel)
        {
            if (dbmodel == null)
                return null;
            ProductOutletDto_POS model = new ProductOutletDto_POS
            {
                ParentProductId = dbmodel.ParentProductId,
                ProductId = dbmodel.ProductId,
                OutletId = dbmodel.OutletId,
                OutletName = dbmodel.OutletName,
                CostPrice = dbmodel.CostPrice,
                Markup = dbmodel.Markup,
                PriceExcludingTax = dbmodel.PriceExcludingTax,
                TaxID = dbmodel.TaxID,
                TaxName = dbmodel.TaxName,
                TaxRate = dbmodel.TaxRate,
                SellingPrice = dbmodel.SellingPrice,
                AvgCostPrice = dbmodel.AvgCostPrice,
                OnHandstock = dbmodel.OnHandstock,
                Awaitingstock = dbmodel.Awaitingstock,
                Committedstock = dbmodel.Committedstock,
                ReorderLevel = dbmodel.ReorderLevel,
                ReorderValue = dbmodel.ReorderValue,
                IsLocked = dbmodel.IsLocked,
                IsVisible = dbmodel.IsVisible
            };
            return model;
        }
    }



    public partial class ProductOutletDB_POS : IRealmObject
    {
        [PrimaryKey]
        public string Id => OutletId + "_" + ProductId;
        public int? ParentProductId { get; set; }
        public int ProductId { get; set; }
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Markup { get; set; }
        public decimal PriceExcludingTax { get; set; }
        public int? TaxID { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal AvgCostPrice { get; set; }
        public decimal OnHandstock { get; set; }
        public decimal Awaitingstock { get; set; }
        public decimal Committedstock { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderValue { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
    }



    public class SignalRProductOutletDto_POS
    {
        public decimal AvgCostPrice { get; set; }
        public decimal Awaitingstock { get; set; }
        public decimal Committedstock { get; set; }
        public decimal CostPrice { get; set; }
        public bool IsVisible { get; set; }
        public decimal Markup { get; set; }
        public decimal MinimumSellingPrice { get; set; }

        public decimal OnHandstock { get; set; }
        public int OutletId { get; set; }
        public int? ParentProductId { get; set; }
        public decimal PriceExcludingTax { get; set; }
        public int ProductId { get; set; }
        public int? TaxID { get; set; }
        public decimal SellingPrice { get; set; }

        public decimal stock { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderValue { get; set; }
        public bool IsLocked { get; set; }
    }




    // Root myDeserializedClass = JsonConvert.DeserializeObject(myJsonResponse); 
    public class SubTax
    {
        public int groupTaxId { get; set; }
        public int taxId { get; set; }
        public string name { get; set; }
        public double rate { get; set; }
        public bool isActive { get; set; }
        public int id { get; set; }

    }

    public class Tax
    {
        public string name { get; set; }
        public int rate { get; set; }
        public object description { get; set; }
        public bool isDefault { get; set; }
        public bool isSystemDefault { get; set; }
        public bool isGroup { get; set; }
        public List<SubTax> subTaxes { get; set; }
        public bool isActive { get; set; }
        public int id { get; set; }

    }

    public class Stock
    {
        public int productId { get; set; }
        public int parentProductId { get; set; }
        public int outletId { get; set; }
        public string outletName { get; set; }
        public int costPrice { get; set; }
        public int markup { get; set; }
        public int priceExcludingTax { get; set; }
        public int taxID { get; set; }
        public string taxName { get; set; }
        public int taxRate { get; set; }
        public double sellingPrice { get; set; }
        public int minimumSellingPrice { get; set; }
        public int avgCostPrice { get; set; }
        public int onHandstock { get; set; }
        public int awaitingstock { get; set; }
        public int committedstock { get; set; }
        public int stock { get; set; }
        public object reorderLevel { get; set; }
        public object reorderValue { get; set; }
        public int spillageStock { get; set; }
        public bool isVisible { get; set; }
        public bool sentNotification { get; set; }
        public bool isLocked { get; set; }
        public Tax tax { get; set; }
        public DateTime lastModificationTime { get; set; }

    }

    public class Result
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public string sku { get; set; }
        public string barCode { get; set; }
        public object purchaseCode { get; set; }
        public int totalStock { get; set; }
        public int onHand { get; set; }
        public int awaiting { get; set; }
        public int committed { get; set; }
        public int unitCost { get; set; }
        public double sellingPrice { get; set; }
        public object supplierCode { get; set; }
        public List<Stock> stocks { get; set; }
        public object stockTakeDtl { get; set; }
        public object appliedPOReference { get; set; }
        public bool isBackOrder { get; set; }
        public int backOrderQuantity { get; set; }
        public int quantity { get; set; }
        public int taxID { get; set; }
        public object taxName { get; set; }
        public int taxRate { get; set; }
        public bool foundProductInStockTake { get; set; }

    }

    public class Root
    {
        public Result result { get; set; }
        public object targetUrl { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public bool unAuthorizedRequest { get; set; }
        public bool __abp { get; set; }

    }
}
