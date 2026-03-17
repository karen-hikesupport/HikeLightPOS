namespace HikePOS.Services
{
    public interface IMintPayment
    {
		void Login(string userId, string userPin);
        void ResetPin(string UserId);
		void PaymentRequest(string amount, string email, string authToken);
		void MakeRefund(string amount, string requestId, string authToken);
        void UpdateReader(string authToken);
        void Cancel();
    }
}
