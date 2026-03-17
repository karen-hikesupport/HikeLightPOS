using System;
using HikePOS.Models;
using HikePOS.Models.Enum;

namespace HikePOS.Services
{
    public interface IPaypalHere
    {
        void InitializeSDK(string token, string refreshUrl, string invoiceNumber, decimal amount, Action<Message<PaypalPaymentResult>> paymentResultAction, bool isRefund, string paypalInvoiceId, string transactionNumber, PaypalRetailInvoicePaymentMethod invoicePaymentMethod);

        void FindAndConnectDevice();

        void ChargeTransaction();

        //void ProvideRefund(decimal amount, string paypalInvoiceId, string transactionNumber, PaypalRetailInvoicePaymentMethod method);
    }
}
