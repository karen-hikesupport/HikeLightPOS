using System;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
    public interface ITyroTapToPay
    {
        Task<TyroTapToPayResponse> Refund(double amount, string saleId);
        Task<TyroTapToPayResponse> Sale(double amount, string saleId);
        Task<TyroTapToPayResponse> DeviceConfigure(ITyroTapToPayListener listener,string connectionSecret);
        void Cancel();
        void OpenSetting();
    }
    public interface ITyroTapToPayListener
    {
        void OnReaderUpdate(string message);
    }
}
