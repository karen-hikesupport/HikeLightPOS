using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;

namespace HikePOS.Models
{
    public class PurchaseOrderListDto : FullAuditedPassiveEntityDto
    {
        public string CreatedBy { get; set; }

        public int OutletId { get; set; }

        public string OutletName { get; set; }

        public AddressDto OutletAddress { get; set; }

        public string OutlelEmail { get; set; }
        public string OutlelPhone { get; set; }

        public string PONumber { get; set; }

        public DateTime PODate { get; set; }

        public DateTime? DueDate { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
        public AddressDto SupplierAddress { get; set; }

        public AddressDto SupplierPostalAddress { get; set; }

        public string SupplierEmail { get; set; }

        public string SupplierPhone { get; set; }

        public string RefNumber { get; set; }

        public OrderStatus Status { get; set; }

        public string StatusName { get; set; }

        public bool TaxInclusive { get; set; }

        public bool ApplyTaxAfterDiscount { get; set; }

        public bool DiscountIsAsPercentage { get; set; }

        public decimal DiscountValue { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalTax { get; set; }

        public decimal TotalShippingCost { get; set; }

        public int? ShippingTaxId { get; set; }
        public decimal? ShippingTaxRate { get; set; }
        public string ShippingTaxName { get; set; }
        public decimal ShippingTaxAmount { get; set; }

        public decimal OtherCharges { get; set; }

        public decimal RoundingAmount { get; set; }

        public decimal SubTotal { get; set; }

        public decimal NetAmount { get; set; }

        public string Note { get; set; }

        public bool CannotBeModified { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }
        public ThirdPartySyncStatus ThirdPartySyncStatus { get; set; }

        public decimal SumOftaxAmount { get; set; }

        public decimal SumOftotalAmount { get; set; }

        public decimal SumOfeffectiveAmount { get; set; }

        public decimal SumOfPrice { get; set; }

        public decimal SumOfQty { get; set; }

        public int TobeSentOrders { get; set; }

        public int TobeRecieveOrders { get; set; }
    }
    public class PurchaseOrderListResponseObjectResult
    {
        public ObservableCollection<PurchaseOrderListDto> items { get; set; }
    }
}
