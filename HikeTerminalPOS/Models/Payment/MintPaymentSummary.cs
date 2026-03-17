using System;
namespace HikePOS.Models.Payment
{
    public class MintPaymentSummary
    {
		public string Status { get; set; }
		public string TransactionRequestId { get; set; }
		public string TransactionDate { get; set; }
		public string MaskedPan { get; set; }
		public string CardHolderName { get; set; }
		public string ApplicationLabel { get; set; }
        public string AmountAuthorized { get; set; }
        public string SurchargeAmount { get; set; }
        public string DeclinedReason { get; set; }
        public string AccountType { get; set; }
        public string MerchantTradingName { get; set; }
        public bool RequirendSignatureOnReceipt { get; set; }


        //CardScheme Scheme { get; set; }
        //NSData ReceiptData { get; set; }

    }
}
