using System;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
	public class InvoiceLineSubItemDto : FullAuditedPassiveEntityDto
	{
		public int InvoiceLineItemId { get; set; }
		public int InvoiceId { get; set; }
		public int ItemId { get; set; }
		public string ItemName { get; set; }
        //public decimal Quantity { get; set; }


        public decimal Quantity { get; set; }

        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public string Stock { get; set; }
        public bool TrackInventory { get; set; }
        //Ticket end:#20064.by rupesh

        //[JsonProperty("Quantity ")]
        //public int RoundedQuantity
        //{
        //    get
        //    {
        //        return Decimal.ToInt32(Quantity);
        //    }
        //    set
        //    {
        //        Quantity = value;
        //    }
        //}

        //Ticket start:#84291 iOS:FR: composite change. by rupesh
        public decimal Discount { get; set; }
        public decimal IndividualPrice { get; set; }
        //Ticket end:#84291 . by rupesh

        public InvoiceLineSubItemDB ToModel()
        {
            InvoiceLineSubItemDB invoiceLineSubItemDB = new InvoiceLineSubItemDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceLineItemId = InvoiceLineItemId,
                InvoiceId = InvoiceId,
                ItemId = ItemId,
                ItemName = ItemName,
                Quantity = Quantity,
                Barcode = Barcode,
                Sku = Sku,
                Stock = Stock,
                TrackInventory = TrackInventory,
                //Ticket start:#84291 iOS:FR: composite change. by rupesh
                Discount = Discount,
                IndividualPrice = IndividualPrice
                //Ticket end:#84291 . by rupesh

            };
            return invoiceLineSubItemDB;
        }
        public static InvoiceLineSubItemDto FromModel(InvoiceLineSubItemDB invoiceLineSubItemDB)
        {
            if (invoiceLineSubItemDB == null)
                return null;

            InvoiceLineSubItemDto invoiceLineSubItemDto = new InvoiceLineSubItemDto
            {
                Id = invoiceLineSubItemDB.Id,
                IsActive = invoiceLineSubItemDB.IsActive,
                InvoiceLineItemId = invoiceLineSubItemDB.InvoiceLineItemId,
                InvoiceId = invoiceLineSubItemDB.InvoiceId,
                ItemId = invoiceLineSubItemDB.ItemId,
                ItemName = invoiceLineSubItemDB.ItemName,
                Quantity = invoiceLineSubItemDB.Quantity,
                Barcode = invoiceLineSubItemDB.Barcode,
                Sku = invoiceLineSubItemDB.Sku,
                Stock = invoiceLineSubItemDB.Stock,
                TrackInventory = invoiceLineSubItemDB.TrackInventory,
                //Ticket start:#84291 iOS:FR: composite change. by rupesh
                Discount = invoiceLineSubItemDB.Discount,
                IndividualPrice = invoiceLineSubItemDB.IndividualPrice
                //Ticket end:#84291 . by rupesh

            };
            return invoiceLineSubItemDto;

        }

    }
    public partial class InvoiceLineSubItemDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceLineItemId { get; set; }
        public int InvoiceId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public string Stock { get; set; }
        public bool TrackInventory { get; set; }
        //Ticket start:#84291 iOS:FR: composite change. by rupesh
        public decimal Discount { get; set; }
        public decimal IndividualPrice { get; set; }
        //Ticket end:#84291 . by rupesh

    }

}
