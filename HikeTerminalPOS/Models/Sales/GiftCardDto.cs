
using Newtonsoft.Json;

namespace HikePOS.Models
{
	public class GiftCardDto : FullAuditedPassiveEntityDto
	{
		public string Number { get; set; }
		public decimal Amount { get; set; }
		public decimal SpentAmount { get; set; }
		public bool IsTopupGiftCard { get; set; }

		//#28721 Email for Sending Gift Cards
		public string RecipientName { get; set; }
		public string RecipientEmail { get; set; }
		public string RecipientMessage { get; set; }
		public string FromName { get; set; }
		public string FromEmail { get; set; }
        //#28721 Email for Sending Gift Cards


        //Start #90944 iOS:FR Gift cards expiry date by Pratik
        public DateTime? ExpiryDate { get; set; }

		[JsonIgnore]
		 public string ExpiryMsg => (ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow.ToStoreTime()) ? LanguageExtension.Localize("ThisGiftCardIsExpired") : string.Empty;
        //End #90944 by Pratik

    }
    public class GiftCardInput
	{
		public GiftCardDto input { get; set; }
	}
	//Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
	public class GiftCardDetail
	{
		public string Number { get; set; }
		public decimal OpeningBalance { get; set; }
		public decimal UsedBalance { get; set; }
		public decimal ClosingBalance { get; set; }
	}
	//Ticket end:#94420.by rupesh

}
