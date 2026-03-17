using HikePOS.Enums;
using Realms;
namespace HikePOS.Models
{
	public class InvoicePaymentDetailDto : FullAuditedPassiveEntityDto
	{
		public int InvoiceId { get; set; }
		public int InvoicePaymentID { get; set; }
		public InvoicePaymentKey Key { get; set; }
		public string Value { get; set; }
        public InvoicePaymentDetailDB ToModel()
        {
            InvoicePaymentDetailDB invoicePaymentDetailDB = new InvoicePaymentDetailDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceId = InvoiceId,
                InvoicePaymentID = InvoicePaymentID,
                Key = (int)Key,
                Value = Value

            };
            return invoicePaymentDetailDB;
        }
        public static InvoicePaymentDetailDto FromModel(InvoicePaymentDetailDB invoicePaymentDetailDB)
        {
            if (invoicePaymentDetailDB == null)
                return null;
            InvoicePaymentDetailDto invoicePaymentDetailDto = new InvoicePaymentDetailDto
            {
                Id = invoicePaymentDetailDB.Id,
                IsActive = invoicePaymentDetailDB.IsActive,
                InvoiceId = invoicePaymentDetailDB.InvoiceId,
                InvoicePaymentID = invoicePaymentDetailDB.InvoicePaymentID,
                Key = (InvoicePaymentKey)invoicePaymentDetailDB.Key,
                Value = invoicePaymentDetailDB.Value

            };
            return invoicePaymentDetailDto;

        }

    }
    public partial class InvoicePaymentDetailDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceId { get; set; }
        public int InvoicePaymentID { get; set; }
        public int Key { get; set; }
        public string Value { get; set; }
    }

}
