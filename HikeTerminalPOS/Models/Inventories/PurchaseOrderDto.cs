using System;
using HikePOS.Enums;
using System.Collections.ObjectModel;
using System.Linq;
using Realms;
namespace HikePOS.Models
{
	public class PurchaseOrderDto : FullAuditedPassiveEntityDto
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

		public EntityFrom OrderFrom { get; set; }

		public ObservableCollection<PurchaseOrderLineItemDto> PurchaseOrderLineItems { get; set; }

		public string ProductTitle
		{
			get
			{
				if (PurchaseOrderLineItems != null && PurchaseOrderLineItems.Any())
				{
					return Helpers.CommonMethods.GetHtmlData(14, string.Join(@"<br>", PurchaseOrderLineItems.Select(p => p.Title.ToString())));
				}
				else
				{
					return "";
				}
			}
		}
        public PurchaseOrderDB ToModel(int invoiceId)
        {
            PurchaseOrderDB purchaseOrderDB = new PurchaseOrderDB
            {
                Id = Id,
                IsActive = IsActive,
                CreatedBy = CreatedBy,
                OutletId = OutletId,
                OutletName = OutletName,
                OutletAddress = OutletAddress?.ToModel(),
                OutlelEmail = OutlelEmail,
                OutlelPhone = OutlelPhone,
                PONumber = PONumber,
                PODate = PODate,
                DueDate = DueDate,
                SupplierId = SupplierId,
                SupplierName = SupplierName,
                SupplierAddress = SupplierAddress?.ToModel(),
                SupplierPostalAddress = SupplierPostalAddress?.ToModel(),
                SupplierEmail = SupplierEmail,
                SupplierPhone = SupplierPhone,
                RefNumber = RefNumber,
                Status = (int)Status,
                StatusName = StatusName,
                TaxInclusive = TaxInclusive,
                ApplyTaxAfterDiscount = ApplyTaxAfterDiscount,
                DiscountIsAsPercentage = DiscountIsAsPercentage,
                DiscountValue = DiscountValue,
                TotalDiscount = TotalDiscount,
                TotalTax = TotalTax,
                TotalShippingCost = TotalShippingCost,
                ShippingTaxId = ShippingTaxId,
                ShippingTaxRate = ShippingTaxRate,
                ShippingTaxName = ShippingTaxName,
                ShippingTaxAmount = ShippingTaxAmount,
                OtherCharges = OtherCharges,
                RoundingAmount = RoundingAmount,
                SubTotal = SubTotal,
                NetAmount = NetAmount,
                Note = Note,
                CannotBeModified = CannotBeModified,
                CreationTime = CreationTime,
                LastModificationTime = LastModificationTime,
                ThirdPartySyncStatus = (int)ThirdPartySyncStatus,
                OrderFrom = (int)OrderFrom,
                InvoiceId = invoiceId

            };
            if (purchaseOrderDB.PurchaseOrderLineItems != null)
                PurchaseOrderLineItems.ForEach(i => purchaseOrderDB.PurchaseOrderLineItems.Add(i.ToModel()));

            return purchaseOrderDB;
        }
        public static PurchaseOrderDto FromModel(PurchaseOrderDB purchaseOrderDB)
        {
            PurchaseOrderDto purchaseOrderDto = new PurchaseOrderDto
            {
                Id = purchaseOrderDB.Id,
                IsActive = purchaseOrderDB.IsActive,
                CreatedBy = purchaseOrderDB.CreatedBy,
                OutletId = purchaseOrderDB.OutletId,
                OutletName = purchaseOrderDB.OutletName,
                OutletAddress = AddressDto.FromModel(purchaseOrderDB.OutletAddress),
                OutlelEmail = purchaseOrderDB.OutlelEmail,
                OutlelPhone = purchaseOrderDB.OutlelPhone,
                PONumber = purchaseOrderDB.PONumber,
                PODate = purchaseOrderDB.PODate.UtcDateTime,
                DueDate = purchaseOrderDB.DueDate?.UtcDateTime,
                SupplierId = purchaseOrderDB.SupplierId,
                SupplierName = purchaseOrderDB.SupplierName,
                SupplierAddress = AddressDto.FromModel(purchaseOrderDB.SupplierAddress),
                SupplierPostalAddress = AddressDto.FromModel(purchaseOrderDB.SupplierPostalAddress),
                SupplierEmail = purchaseOrderDB.SupplierEmail,
                SupplierPhone = purchaseOrderDB.SupplierPhone,
                RefNumber = purchaseOrderDB.RefNumber,
                Status = (OrderStatus)purchaseOrderDB.Status,
                StatusName = purchaseOrderDB.StatusName,
                TaxInclusive = purchaseOrderDB.TaxInclusive,
                ApplyTaxAfterDiscount = purchaseOrderDB.ApplyTaxAfterDiscount,
                DiscountIsAsPercentage = purchaseOrderDB.DiscountIsAsPercentage,
                DiscountValue = purchaseOrderDB.DiscountValue,
                TotalDiscount = purchaseOrderDB.TotalDiscount,
                TotalTax = purchaseOrderDB.TotalTax,
                TotalShippingCost = purchaseOrderDB.TotalShippingCost,
                ShippingTaxId = purchaseOrderDB.ShippingTaxId,
                ShippingTaxRate = purchaseOrderDB.ShippingTaxRate,
                ShippingTaxName = purchaseOrderDB.ShippingTaxName,
                ShippingTaxAmount = purchaseOrderDB.ShippingTaxAmount,
                OtherCharges = purchaseOrderDB.OtherCharges,
                RoundingAmount = purchaseOrderDB.RoundingAmount,
                SubTotal = purchaseOrderDB.SubTotal,
                NetAmount = purchaseOrderDB.NetAmount,
                Note = purchaseOrderDB.Note,
                CannotBeModified = purchaseOrderDB.CannotBeModified,
                CreationTime = purchaseOrderDB.CreationTime.UtcDateTime,
                LastModificationTime = purchaseOrderDB.LastModificationTime?.UtcDateTime,
                ThirdPartySyncStatus = (ThirdPartySyncStatus)purchaseOrderDB.ThirdPartySyncStatus,
                OrderFrom = (EntityFrom)purchaseOrderDB.OrderFrom

            };
            if (purchaseOrderDB.PurchaseOrderLineItems != null)
                purchaseOrderDto.PurchaseOrderLineItems = new ObservableCollection<PurchaseOrderLineItemDto>(purchaseOrderDB.PurchaseOrderLineItems.Select(a => PurchaseOrderLineItemDto.FromModel(a))?.ToList());

            return purchaseOrderDto;
        }

    }
    public partial class PurchaseOrderDB : IRealmObject
	{

        public int Id { get; set; }
        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }

        public int OutletId { get; set; }

        public string OutletName { get; set; }

        public AddressDB OutletAddress { get; set; }

        public string OutlelEmail { get; set; }
        public string OutlelPhone { get; set; }

        public string PONumber { get; set; }

        public DateTimeOffset PODate { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
        public AddressDB SupplierAddress { get; set; }

        public AddressDB SupplierPostalAddress { get; set; }

        public string SupplierEmail { get; set; }

        public string SupplierPhone { get; set; }

        public string RefNumber { get; set; }

        public int Status { get; set; }

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

        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset? LastModificationTime { get; set; }

        public int ThirdPartySyncStatus { get; set; }

        public int OrderFrom { get; set; }

        public IList<PurchaseOrderLineItemDB> PurchaseOrderLineItems { get;}

        public int InvoiceId { get; set; }
}
}