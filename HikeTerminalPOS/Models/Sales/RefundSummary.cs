using System;
using Newtonsoft.Json;

namespace HikePOS.Models
{
	public class RefundSummary
	{
		public int InvoiceId { get; set; }
        public int MainInvoiceId { get; set; }

		public string Number { get; set; }
		public string Title { get; set; }

        [JsonIgnore]
        public decimal Quantity { get; set; }
        [JsonProperty("Quantity")]
        public string RoundedQuantity
        {
            get
            {
                return Quantity.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                Quantity = result;
            }
        }

		public string UserBy { get; set; }
		public decimal TotalAmount { get; set; }
        [JsonIgnore]
        public DateTime _TransactionDate { get; set; }
        public DateTime TransactionDate
        {
            get
            {
                return _TransactionDate;
            }
            set
            {
                _TransactionDate = value;
                TransactionStoreDate = TransactionDate.ToStoreTime();
            }
        }
        [JsonIgnore]
        public DateTime TransactionStoreDate { get; set; }

    }
}
