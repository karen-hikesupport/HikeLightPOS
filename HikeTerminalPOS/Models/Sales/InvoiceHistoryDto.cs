using System;
using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class InvoiceHistoryDto : FullAuditedPassiveEntityDto
    {
		public int InvoiceId { get; set; }
		public InvoiceStatus Status { get; set; }
		public string StatusName { get; set; }
		public DateTime? CreationTime { get; set; }
		public string ServerdBy { get; set; }
        public bool NotSynced { get; set; }
        [JsonIgnore]
        public DateTime? CreationStoreTime
        {
            get
            {
                if (CreationTime != null)
                {
                    return CreationTime.Value.ToStoreTime();
                }
                else
                {
                    return null;
                }
            }
        }
        //#32360 iPad :: Feature request :: Add Invoice Operation Source in Sales History of An Invoice
        public InvoiceFrom? InvoiceFrom { get; set; }
        public string InvoiceFromName { get; set; }
        //#32360 iPad :: Feature request :: Add Invoice Operation Source in Sales History of An Invoice
        public InvoiceHistoryDB ToModel()
        {
            InvoiceHistoryDB invoiceHistoryDB = new InvoiceHistoryDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceId = InvoiceId,
                Status = (int)Status,
                StatusName = StatusName,
                CreationTime = CreationTime,
                ServerdBy = ServerdBy,
                NotSynced = NotSynced,
                InvoiceFrom = (int)InvoiceFrom,
                InvoiceFromName = InvoiceFromName

            };
            return invoiceHistoryDB;
        }
        public static InvoiceHistoryDto FromModel(InvoiceHistoryDB invoiceHistoryDB)
        {
            if (invoiceHistoryDB == null)
                return null;
            InvoiceHistoryDto invoiceHistoryDto = new InvoiceHistoryDto
            {
                Id = invoiceHistoryDB.Id,
                IsActive = invoiceHistoryDB.IsActive,
                InvoiceId = invoiceHistoryDB.InvoiceId,
                Status = (InvoiceStatus)invoiceHistoryDB.Status,
                StatusName = invoiceHistoryDB.StatusName,
                CreationTime = invoiceHistoryDB.CreationTime?.UtcDateTime,
                ServerdBy = invoiceHistoryDB.ServerdBy,
                NotSynced = invoiceHistoryDB.NotSynced,
                InvoiceFrom = (InvoiceFrom)invoiceHistoryDB.InvoiceFrom,
                InvoiceFromName = invoiceHistoryDB.InvoiceFromName

            };
            return invoiceHistoryDto;

        }

    }
    public partial class InvoiceHistoryDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTimeOffset? CreationTime { get; set; }
        public string ServerdBy { get; set; }
        public bool NotSynced { get; set; }
        public int? InvoiceFrom { get; set; }
        public string InvoiceFromName { get; set; }
    }
    
}
