using System;
using System.Runtime.CompilerServices;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
    public interface INadaTapToPay
    {
        Task<NadaTapToPayDto> Refund(bool isPartial,decimal amount, string currency, string saleId, string merchantReference, string poiTransactionId, DateTime poiTransactionTimeStamp, string balanceAccount, decimal surcharge, decimal fixedCommission, decimal variableCommissionPercentage,decimal paymentFee);
        Task<NadaTapToPayDto> Sale(decimal amount, string currency, string saleId);
        public void AuthorizeSdk(string storeId);

        Task<NadaTapToPayDto> InitializeSdkManually();
        Task<NadaTapToPayDto> ManualRefund(decimal amount, string currency, string saleId);

        void InitializePaymentLauncher();
        void ClearSession();
        void WarmUp();

        Task<string> Diagnosis();

        bool IsInitialized { get; set; }

    }
}
