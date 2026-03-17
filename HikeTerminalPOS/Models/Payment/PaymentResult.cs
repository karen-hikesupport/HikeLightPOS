using HikePOS.Models.Enum;

namespace HikePOS.Models
{
	public class PaymentResult
	{
		public double Amount { get; set; }
		public string MerchantReceipt { get; set; }
		public string TransactionResult { get; set; }
        public object Info { get; set; }
		public bool IsFailed { get; set; }
		public bool _signatureRequired { get; set; }
        public string CardType { get; set; }
        public string ElidedPan { get; set; }
        public string Rrn { get; set; }
        public string AuthorisationCode { get; set; }
        public string TransactionReference { get; set; }

        public string TerminalId { get; set; }
        public string MerchantId { get; set; }
	}




    public class PaypalPaymentResult
    {
        public string TransactionNumber { get; set; }
        public string InvoiceId { get; set; }
        public PaypalRetailInvoicePaymentMethod PaymentMethod { get; set; }
        public string AuthCode { get; set; }
        public string TransactionHandle { get; set; }
        public string ResponseCode { get; set; }
        public string Payer { get; set; }
        public string CorrelationId { get; set; }
        public string Card { get; set; }
        public string ReceiptDestination { get; set; }
    }
}
