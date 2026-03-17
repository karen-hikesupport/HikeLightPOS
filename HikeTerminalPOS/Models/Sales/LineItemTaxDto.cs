
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Realms;
namespace HikePOS.Models
{
	public class LineItemTaxDto : FullAuditedPassiveEntityDto
	{
		public int InvoiceLineItemId { get; set; }
		public int InvoiceId { get; set; }
		public int TaxId { get; set; }
		public decimal TaxRate { get; set; }
        [JsonIgnore]
        decimal _taxAmount { get; set; }
        public decimal TaxAmount { get { return _taxAmount; } set { _taxAmount = value; SetPropertyChanged(nameof(TaxAmount)); } }
		public string TaxName { get; set; }
        public string MainTaxId { get; set; }

        public int AssignSubTax { get; set; }

        //only for receipt use
        [JsonIgnore]
        public ObservableCollection<LineItemTaxDto> SubTaxes { get; set; }

        //only for receipt use
        // Ticket Start #63111 iOS: Separate line items for Tax Group format need to match with web By: Pratik
        [JsonIgnore]
        public bool IsGroupTax => (SubTaxes != null && SubTaxes.Count > 0) ? true : false;
        // Ticket end #63111 By: Pratik

        public LineItemTaxDB ToModel()
        {
            LineItemTaxDB lineItemTaxDB = new LineItemTaxDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceLineItemId = InvoiceLineItemId,
                InvoiceId = InvoiceId,
                TaxId = TaxId,
                TaxRate = TaxRate,
                TaxAmount = TaxAmount,
                TaxName = TaxName,
                MainTaxId = MainTaxId,
                AssignSubTax = AssignSubTax

            };
            return lineItemTaxDB;
        }
        public static LineItemTaxDto FromModel(LineItemTaxDB lineItemTaxDB)
        {
            if (lineItemTaxDB == null)
                return null;
            LineItemTaxDto lineItemTaxDto = new LineItemTaxDto
            {
                Id = lineItemTaxDB.Id,
                IsActive = lineItemTaxDB.IsActive,
                InvoiceLineItemId = lineItemTaxDB.InvoiceLineItemId,
                InvoiceId = lineItemTaxDB.InvoiceId,
                TaxId = lineItemTaxDB.TaxId,
                TaxRate = lineItemTaxDB.TaxRate,
                TaxAmount = lineItemTaxDB.TaxAmount,
                TaxName = lineItemTaxDB.TaxName,
                MainTaxId = lineItemTaxDB.MainTaxId,
                AssignSubTax = lineItemTaxDB.AssignSubTax

            };
            return lineItemTaxDto;

        }

    }
    public partial class LineItemTaxDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceLineItemId { get; set; }
        public int InvoiceId { get; set; }
        public int TaxId { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxName { get; set; }
        public string MainTaxId { get; set; }
        public int AssignSubTax { get; set; }

    }

}
