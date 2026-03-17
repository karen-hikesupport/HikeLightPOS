using System.Collections.Generic;

namespace HikePOS.Models
{
	public class iZettlePaymentResult
	{
        
		//public Dictionary<string, object> Dictionary { get; set; }
		public long Amount { get; set; }
		public long GratuityAmount { get; set; }
		public string ReferenceNumber { get; set; }
		public string EntryMode { get; set; }
		public string AuthorizationCode { get; set; }
		public string ObfuscatedPan { get; set; }
		public string PanHash { get; set; }
		public string CardBrand { get; set; }
		public string AID { get; set; }
		public string TSI { get; set; }
		public string TVR { get; set; }
		public string ApplicationName { get; set; }
		public int NumberOfInstallments { get; set; }
		public long InstallmentAmount { get; set; }
        public string MxFIID  { get; set; }
        public string MxCardType  { get; set; }
	    public string TransactionId { get; set; }
        public string CardIssuingBank { get;set; }
		public string lastRefrence { get; set; }

	}
}
