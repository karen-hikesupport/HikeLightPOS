
using Newtonsoft.Json;

namespace HikePOS.Models
{
    public class ProductOutletDto
    {
        public ProductOutletDto()
        {
            Tax = new TaxDto();
        }

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
        public decimal MinimumSellingPrice { get; set; }
        public decimal AvgCostPrice { get; set; }
        public decimal OnHandstock { get; set; }
        public decimal Awaitingstock { get; set; }
        public decimal Committedstock { get; set; }
        public decimal Stock
        {
            get
            {
                return OnHandstock - Committedstock;
            }
        }

        //Ticket #9271 Start: Inventory stock count issue. By Nikhil.
        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderValue { get; set; }
        //Ticket #9271 End:By Nikhil. 

        public bool IsLocked { get; set; }

        public bool IsVisible { get; set; }

        public TaxDto Tax { get; set; }
    }


    public class SignalRProductOutletDto
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
        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderValue { get; set; }
        public bool IsLocked { get; set; }
    }

    //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
    public class POSSerialNumberRequest
    {
        public string serialNumber { get; set; }
        public int productId { get; set; }
        public int outletId { get; set; }
    }

    public class POSSerialNumberDto
    {
        public string serialNumber { get; set; }
        public string reference { get; set; }
        public DateTime? soldDate { get; set; }
    }
    //End #90946 by Pratik
}
