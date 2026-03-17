using System;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
    public interface IFiska
    {

        void ConfigureSDK(string MerchantId, string RegisterId);

        void InitializeSDK(string mechentID, string registerID, decimal amount, bool isRefund, Action<FiskaPaymentResult> result);
    }
}

