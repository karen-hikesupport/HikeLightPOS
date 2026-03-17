using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik

    public class InvoiceItemFulfillmentDisplay
    {
        public int InvoiceFulfillmentId { get; set; }
        public int InvoiceId { get; set; }
        public int InvoiceLineItemId { get; set; }
        public string FulfillmentQuantity { get; set; }
        public string CreatedBy { get; set; }
        public string ItemName { get; set; }
        public string Ordered  { get; set; }
    }

    public class InvoiceItemFulfillmentDto : FullAuditedPassiveEntityDto
    {
        public int InvoiceFulfillmentId { get; set; }
        public int InvoiceId { get; set; }
        public int InvoiceLineItemId { get; set; }
        public decimal FulfillmentQuantity { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreationTime { get; set; }
        public int? CreatorUserId { get; set; }

        public InvoiceItemFulfillmentDB ToModel()
        {
            InvoiceItemFulfillmentDB modelDB = new InvoiceItemFulfillmentDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceLineItemId = InvoiceLineItemId,
                InvoiceId = InvoiceId,
                InvoiceFulfillmentId = InvoiceFulfillmentId,
                CreatedBy = CreatedBy,
                FulfillmentQuantity = FulfillmentQuantity,
                IsDeleted = IsDeleted,
                CreatorUserId = CreatorUserId,
                CreationTime = CreationTime

            };
            return modelDB;
        }

        public static InvoiceItemFulfillmentDto FromModel(InvoiceItemFulfillmentDB modelDB)
        {
            if (modelDB == null)
                return null;
            InvoiceItemFulfillmentDto modelDto = new InvoiceItemFulfillmentDto
            {
                Id = modelDB.Id,
                IsActive = modelDB.IsActive,
                InvoiceLineItemId = modelDB.InvoiceLineItemId,
                InvoiceId = modelDB.InvoiceId,
                InvoiceFulfillmentId = modelDB.InvoiceFulfillmentId,
                CreatedBy = modelDB.CreatedBy,
                FulfillmentQuantity = modelDB.FulfillmentQuantity,
                IsDeleted = modelDB.IsDeleted,
                CreatorUserId = modelDB.CreatorUserId,
                CreationTime = modelDB.CreationTime.UtcDateTime,
            };
            return modelDto;
        }
    }

    public partial class InvoiceItemFulfillmentDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceFulfillmentId { get; set; }
        public int InvoiceId { get; set; }
        public int InvoiceLineItemId { get; set; }
        public decimal FulfillmentQuantity { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public int? CreatorUserId { get; set; }
    }

    public class InvoiceFulfillmentDto : FullAuditedPassiveEntityDto
    {
        public int InvoiceId { get; set; }
        public int? TrackingId { get; set; }
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public string ShipmentOrderId { get; set; }
        public string ShippingTrackingNumber { get; set; }
        public string CarrierName { get; set; }
        public string CreatedBy { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreationTime { get; set; }
        [JsonIgnore]
        public DateTime CreationStoreTime
        {
            get
            {
               return CreationTime.ToStoreTime();
            }
        }
        public int? CreatorUserId { get; set; }

        public ICollection<InvoiceItemFulfillmentDto> InvoiceItemFulfillments { get; set; }

        [JsonIgnore]
        ObservableCollection<InvoiceItemFulfillmentDisplay> _invoiceLineItems;
        [JsonIgnore]
        public ObservableCollection<InvoiceItemFulfillmentDisplay> InvoiceLineItems { get { return _invoiceLineItems; } set { _invoiceLineItems = value; SetPropertyChanged(nameof(InvoiceLineItems)); } }

        public InvoiceFulfillmentDB ToModel()
        {
            InvoiceFulfillmentDB modelDB = new InvoiceFulfillmentDB
            {
                Id = Id,
                IsActive = IsActive,
                TrackingId = TrackingId,
                InvoiceId = InvoiceId,
                OutletId = OutletId,
                OutletName = OutletName,
                ShipmentOrderId = ShipmentOrderId,
                ShippingTrackingNumber = ShippingTrackingNumber,
                CarrierName = CarrierName,
                CreatedBy = CreatedBy,
                IsDeleted = IsDeleted,
                CreatorUserId = CreatorUserId,
                CreationTime = CreationTime

            };
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            InvoiceItemFulfillments?.ForEach(i => modelDB.InvoiceItemFulfillments.Add(i.ToModel()));
            //end #84293 by Pratik
            return modelDB;
        }

        public static InvoiceFulfillmentDto FromModel(InvoiceFulfillmentDB modelDB)
        {
            if (modelDB == null)
                return null;
            InvoiceFulfillmentDto modelDto = new InvoiceFulfillmentDto
            {
                Id = modelDB.Id,
                IsActive = modelDB.IsActive,
                TrackingId = modelDB.TrackingId,
                InvoiceId = modelDB.InvoiceId,
                OutletId = modelDB.OutletId,
                OutletName = modelDB.OutletName,
                ShipmentOrderId = modelDB.ShipmentOrderId,
                ShippingTrackingNumber = modelDB.ShippingTrackingNumber,
                CarrierName = modelDB.CarrierName,
                CreatedBy = modelDB.CreatedBy,
                IsDeleted = modelDB.IsDeleted,
                CreatorUserId = modelDB.CreatorUserId,
                CreationTime = modelDB.CreationTime.UtcDateTime,
            };

            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (modelDB.InvoiceItemFulfillments != null)
                modelDto.InvoiceItemFulfillments = new ObservableCollection<InvoiceItemFulfillmentDto>(modelDB.InvoiceItemFulfillments.Select(a => InvoiceItemFulfillmentDto.FromModel(a)));
            //End #84293 by Pratik

            return modelDto;
        }
    }

    public partial class InvoiceFulfillmentDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceId { get; set; }
        public int? TrackingId { get; set; }
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public string ShipmentOrderId { get; set; }
        public string ShippingTrackingNumber { get; set; }
        public string CarrierName { get; set; }
        public string CreatedBy { get; set; }
        public IList<InvoiceItemFulfillmentDB> InvoiceItemFulfillments { get; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public int? CreatorUserId { get; set; }
    }


    public class ProductAvailableStocks
    {
        public int invoiceItemId { get; set; }
        public int productId { get; set; }
        public int invoiceItemValue { get; set; }
        public int outletId { get; set; }
        public decimal stock { get; set; }
        public int invoiceItemType { get; set; }
        public bool trackInventory { get; set; }
        public bool allowOutOfStock { get; set; }
    }

    public class ProductAvailableStocksRequest
    {
        public int outletId { get; set; }
        public int invoiceId { get; set; }
    }
    //End #84293 by Pratik
}

