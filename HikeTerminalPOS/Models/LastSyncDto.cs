using System;
using Realms;
namespace HikePOS.Models
{
	public class LastSyncDto
	{
        public int Id { get; set; }
        public DateTime? LastOutletDataSyncTime { get; set; }
		public DateTime? LastPaymentDataSyncTime { get; set; }
		public DateTime? LastCustomerDateSyncTime { get; set; }
		public DateTime? LastCustomerGroupDateSyncTime { get; set; }
		public DateTime? LastProductDataSyncTime { get; set; }
        public DateTime? LastProductStockDataSyncTime { get; set; }
		public DateTime? LastOfferDataSyncTime { get; set; }
		public DateTime? LastInvoiceDataSyncTime { get; set; }
        public DateTime? LastBackgroundProductStockDataSyncTime { get; set; }
		public DateTime? LastUnityOfMeasureSyncTime { get; set; }
		public DateTime? LastDeliveryAddressDateSyncTime { get; set; }
		public DateTime? LastProductTagsDataSyncTime { get; set; }

        public LastSyncDB ToModel()
        {
            LastSyncDB model = new LastSyncDB
            {
                Id = 1,
                LastOutletDataSyncTime = LastOutletDataSyncTime,
                LastPaymentDataSyncTime = LastPaymentDataSyncTime,
                LastCustomerDateSyncTime = LastCustomerDateSyncTime,
                LastCustomerGroupDateSyncTime = LastCustomerGroupDateSyncTime,
                LastProductDataSyncTime = LastProductDataSyncTime,
                LastProductStockDataSyncTime = LastProductStockDataSyncTime,
                LastOfferDataSyncTime = LastOfferDataSyncTime,
                LastInvoiceDataSyncTime = LastInvoiceDataSyncTime,
                LastBackgroundProductStockDataSyncTime = LastBackgroundProductStockDataSyncTime,
                LastUnityOfMeasureSyncTime = LastUnityOfMeasureSyncTime,
                LastDeliveryAddressDateSyncTime = LastDeliveryAddressDateSyncTime,
                LastProductTagsDataSyncTime = LastProductTagsDataSyncTime
            };

            return model;
        }

        public static LastSyncDto FromModel(LastSyncDB dbModel)
        {
            if (dbModel == null)
                return null;

            LastSyncDto model = new LastSyncDto
            {
                Id = 1,
                LastOutletDataSyncTime = dbModel.LastOutletDataSyncTime?.UtcDateTime,
                LastPaymentDataSyncTime = dbModel.LastPaymentDataSyncTime?.UtcDateTime,
                LastCustomerDateSyncTime = dbModel.LastCustomerDateSyncTime?.UtcDateTime,
                LastCustomerGroupDateSyncTime = dbModel.LastCustomerGroupDateSyncTime?.UtcDateTime,
                LastProductDataSyncTime = dbModel.LastProductDataSyncTime?.UtcDateTime,
                LastProductStockDataSyncTime = dbModel.LastProductStockDataSyncTime?.UtcDateTime,
                LastOfferDataSyncTime = dbModel.LastOfferDataSyncTime?.UtcDateTime,
                LastInvoiceDataSyncTime = dbModel.LastInvoiceDataSyncTime?.UtcDateTime,
                LastBackgroundProductStockDataSyncTime = dbModel.LastBackgroundProductStockDataSyncTime?.UtcDateTime,
                LastUnityOfMeasureSyncTime = dbModel.LastUnityOfMeasureSyncTime?.UtcDateTime,
                LastDeliveryAddressDateSyncTime = dbModel.LastDeliveryAddressDateSyncTime?.UtcDateTime,
                LastProductTagsDataSyncTime = dbModel.LastProductTagsDataSyncTime?.UtcDateTime
            };

            return model;
        }
    }

    public partial class LastSyncDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public DateTimeOffset? LastOutletDataSyncTime { get; set; }
        public DateTimeOffset? LastPaymentDataSyncTime { get; set; }
        public DateTimeOffset? LastCustomerDateSyncTime { get; set; }
        public DateTimeOffset? LastCustomerGroupDateSyncTime { get; set; }
        public DateTimeOffset? LastProductDataSyncTime { get; set; }
        public DateTimeOffset? LastProductStockDataSyncTime { get; set; }
        public DateTimeOffset? LastOfferDataSyncTime { get; set; }
        public DateTimeOffset? LastInvoiceDataSyncTime { get; set; }
        public DateTimeOffset? LastBackgroundProductStockDataSyncTime { get; set; }
        public DateTimeOffset? LastUnityOfMeasureSyncTime { get; set; }
        public DateTimeOffset? LastDeliveryAddressDateSyncTime { get; set; }
        public DateTimeOffset? LastProductTagsDataSyncTime { get; set; }
    }
}
