using System;
namespace HikePOS.Models
{
	public class TyroConfigurationDto
	{
        public int Id { get; set; }
        public string AccessToken { get; set; }
		public int PaymentOptionId { get; set; }
		public string MerchantId { get; set; }
		public string TerminalId { get; set; }
		public string ApiKey { get; set; }
		public bool IsActive { get; set; }
	}

	public class TyroPaymentInput {
		public double Amount { get; set; } = 0;
		public TyroPaymentAction PaymentAction { get; set; } = TyroPaymentAction.Purachse;
        public string AccessToken { get; set; }
		public Action HandleAction { get; set; }
	   public string TransactionId { get; set; }

	}

	public enum TyroPaymentAction
	{
		Purachse = 1,
		Refund = 2
	}
}
