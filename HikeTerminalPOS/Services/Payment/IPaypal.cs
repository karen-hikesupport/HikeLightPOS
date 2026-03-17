using System;
using System.Threading.Tasks;
using HikePOS.Models;

namespace HikePOS.Services
{
	public interface IPaypal
	{
        void SetupPaypal();
		void Close();
		void UpdateReaderStatusWithReader(string AccessToken);
		void SetupSimpleInvoice(InvoiceDto _invoice);
		void RefundInvoiceByTransactionId(string TransactionID, decimal TenderAmount, string Currency);
		void Charge();
	}


	public interface IPaypalConnect {
		void SetupWithToken(string sdk_token);
	}
}

