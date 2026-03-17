
namespace HikePOS.Services
{
	public interface IiZettle
	{
		void ChargeAmount(decimal amount, string currency, string lastReference, Page page);
		void RefundAmount(decimal _amount, string reference, string refundReference, Page page);

        void Login(bool isLogin);
        void PrepareZettleActivityLauncher();
    }


    public class IZettleDetail
    {
        public double Amount { get; set; }
        public double GratuityAmount { get; set; }
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
        public double InstallmentAmount { get; set; }
    }
}
