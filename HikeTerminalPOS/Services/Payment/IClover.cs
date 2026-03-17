using System;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
    public interface IClover
    {
        void Sale(int amount, string externalId);
        void Refund(int amount, string orderId, string paymentId);
        void ManualRefund(int amount, string externalId);

        Task<bool> DeviceConfigure(ICloverListener listener, CloverConfigurationDto ConfigurationModel);
        void Cancel();
        void Dispose();
    }
    public interface ICloverListener
    {
        void OnPairingCode(string pairingCode);
        void OnPairingSuccess(string authToken);
        void OnDeviceError(string error);
        void OnDeviceReady();
        void OnDeviceConnected();
        void OnDeviceDisconnected();
        void OnDeviceActivityEnd(string message);
        void OnDeviceActivityStart(string message);
        void OnSaleResponse(CloverPaymentResponse response);
        void OnRefundPaymentResponse(CloverPaymentResponse response);
        void OnManualPaymentResponse(CloverPaymentResponse response);
        void OnTransactionTimedOut();
        void OnDeviceReset();
        void OnTransactionStart();

    }


}