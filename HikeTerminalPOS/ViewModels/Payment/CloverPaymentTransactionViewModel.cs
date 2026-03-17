using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class CloverPaymentTransactionViewModel : BaseViewModel,ICloverListener
    {
        public event EventHandler<CloverPaymentResponse> PaymentSuccessed;
        IClover iClover;
        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }
        string _status { get; set; }
        public string Status { get { return _status; } set { _status = value; SetPropertyChanged(nameof(Status)); } }
        private string _pairingCodeText = "";
        public string PairingCodeText
        {
            get { return _pairingCodeText; }
            set
            {
                _pairingCodeText = value;
                SetPropertyChanged(nameof(PairingCodeText));
            }
        }

        bool _chargeVisible { get; set; }
        public bool ChargeVisible { get { return _chargeVisible; }
            set {
                _chargeVisible = value;
                if (Invoice.Status != InvoiceStatus.Refunded)
                    CancelVisible = !value;
                else
                    CancelVisible = false;
                SetPropertyChanged(nameof(ChargeVisible));
            }
        }
        bool _cancelVisible { get; set; }
        public bool CancelVisible { get { return _cancelVisible; } set { _cancelVisible = value; SetPropertyChanged(nameof(CancelVisible)); } }
        string _cancelText { get; set; }
        public string CancelText { get { return _cancelText; } set { _cancelText = value; SetPropertyChanged(nameof(CancelText)); } }
        public PaymentOptionDto PaymentOption { get; set; }
        public CloverConfigurationDto ConfigurationModel { get; set; }
        public SubmitLogServices logService { get; set; }
        public Dictionary<string, string> logRequestDetails { get; set; }
        public SaleServices SaleService { get; internal set; }
        public ICommand ChargeCommand { get; }
        public ICommand CancelCommand { get; }
        public bool IsTransactionProcessing = false;
        public bool IsRetried = false;
        bool Ready = false;

        public CloverPaymentTransactionViewModel()
        {
            ChargeCommand = new Command(Charge);
            CancelCommand = new Command(Cancel);

        }
        public override void OnAppearing()
        {
            base.OnAppearing();
            ChargeVisible = true;
            IsTransactionProcessing = false;
            IsRetried = false;
            Status = "";
            CancelText = "Cancel";
            if (iClover == null)
            {
                iClover = DependencyService.Get<IClover>();
                iClover.DeviceConfigure(this, ConfigurationModel);
            }
            iClover.Cancel();

        }
        void Cancel()
        {
            iClover?.Cancel();
            if (CancelText == "Retry")
            {
                CancelText = "Cancel";
                IsRetried = true;
            }
            else
            {
                ChargeVisible = true;

            }

        }
        void Charge()
        {
            CancelText = "Cancel";
            ChargeVisible = false;
            IsTransactionProcessing = true;
            logRequestDetails.TryAdd("IsRefund", (Invoice.Status == InvoiceStatus.Refunded).ToString());
            logRequestDetails.TryAdd("Amount", Invoice.TenderAmount.ToString());
            logRequestDetails.TryAdd("InvoiceNumber", Invoice.Number);

            var lastReference = RandomString(14);
            decimal tempAmount = Invoice.TenderAmount + (PaymentOption.DisplaySurcharge ?? 0);
            var amountInCents = Convert.ToInt32(tempAmount * 100);

            if (amountInCents > 0)
            {
                logRequestDetails.TryAdd("Reference", lastReference);
                Extensions.SendLogsToServer(logService, PaymentOption, logRequestDetails);
                string tempString = "Sale: " + Newtonsoft.Json.JsonConvert.SerializeObject(ConfigurationModel) + " tempInvoiceID: " + Invoice.InvoiceTempId + "AmountInCents: " + amountInCents.ToString()
                        + "Last reference: " + lastReference.ToString() + " Invoice.Note: " + Invoice.Note;
                SaveInLocalbeforePayment(tempString, false);
                iClover.Sale(amountInCents, lastReference);
                Status = "Sale initiated..";

            }
            else
            {
                amountInCents = System.Math.Abs(amountInCents);
                CloverPaymentResponse cloverPaymentResponse = new CloverPaymentResponse();

                foreach (var item in Invoice.InvoiceRefundPayments ?? Enumerable.Empty<InvoicePaymentDto>())
                {
                    var paymentDetails = item.InvoicePaymentDetails;

                    foreach (var payment in paymentDetails)
                    {
                        string temps = payment.Value;
                        if(payment.Key == InvoicePaymentKey.CloverResponse)
                          cloverPaymentResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<CloverPaymentResponse>(temps) as CloverPaymentResponse;
                    }
                }

                if (cloverPaymentResponse != null && !string.IsNullOrEmpty(cloverPaymentResponse.OrderId) && !string.IsNullOrEmpty(cloverPaymentResponse.PaymentId) && cloverPaymentResponse.Amount == amountInCents.ToString())
                {
                    var orderId = cloverPaymentResponse.OrderId;
                    var paymentId = cloverPaymentResponse.PaymentId;
                    string invoiceReference = Invoice.InvoiceTempId;
                    logRequestDetails.TryAdd("Reference", invoiceReference);
                    logRequestDetails.TryAdd("orderId", orderId);
                    logRequestDetails.TryAdd("paymentId", paymentId);
                    Extensions.SendLogsToServer(logService, PaymentOption, logRequestDetails);
                    string tempString = "Refund: " + Newtonsoft.Json.JsonConvert.SerializeObject(cloverPaymentResponse) + " tempInvoiceID: " + Invoice.InvoiceTempId + "AmountInCents: " + tempAmount.ToString()
                        + "orderId: " + orderId.ToString()
                        + "paymentId: " + paymentId.ToString() + " Invoice.Note: " + Invoice.Note;
                    SaveInLocalbeforePayment(tempString, true);
                    iClover.Refund(amountInCents, orderId, paymentId);
                    Status = "Refund initiated..";
                }
                else if(!string.IsNullOrEmpty(lastReference))
                {
                    logRequestDetails.TryAdd("Reference", lastReference);
                    Extensions.SendLogsToServer(logService, PaymentOption, logRequestDetails);
                    string tempString = "Refund: " + Newtonsoft.Json.JsonConvert.SerializeObject(ConfigurationModel) + " tempInvoiceID: " + Invoice.InvoiceTempId + "AmountInCents: " + amountInCents.ToString()
                            + "Last reference: " + lastReference.ToString() + " Invoice.Note: " + Invoice.Note;
                    SaveInLocalbeforePayment(tempString, false);
                    iClover.ManualRefund(amountInCents, lastReference);
                    Status = "Refund initiated..";

                }
                else
                {
                    Debug.WriteLine("Unable to retrieve Clover details");
                }

            }

        }



        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
        {
            //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
             if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
             {
                  return;
             }
             //Ticket end:#24764 .by rupesh

            string currentPaymentObject = string.Empty;
            if (isRefund)
                currentPaymentObject = "Clover Refund : ";
            else
                currentPaymentObject = "Clover Sale :  ";


            currentPaymentObject = currentPaymentObject + " : " + paymentObject;
            Invoice.CurrentPaymentObject = currentPaymentObject;
            Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;
            await SaleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);

        }

        public void OnPairingCode(string pairingCode)
        {
            PairingCodeText = "Pairing code: " + pairingCode;

        }

        public void OnPairingSuccess(string authToken)
        {
            ConfigurationModel.AuthToken = authToken;
            Settings.Cloversettings = ConfigurationModel;
            PairingCodeText = "";

        }

        public void OnDeviceError(string error)
        {
            Logger.SaleLogger("Clover Payment Error- " + error);
            Status = error;
            ChargeVisible = true;
            IsTransactionProcessing = false;

        }

        public void OnDeviceReady()
        {
            Status = "Connected";
            Ready = true;

        }
        public void OnDeviceConnected()
        {
            //Status = "Clover device is connected, but not available to process requests";
            Ready = false;
        }

        public void OnDeviceDisconnected()
        {
            if (Ready)
            {
                Status = "Disconnected"; // "Clover device is not available";
            }
            Ready = false;
            IsTransactionProcessing = false;
        }

        public void OnDeviceActivityEnd(string message)
        {
            if(IsTransactionProcessing)
                Status = message;

        }
        public void OnDeviceActivityStart(string message)
        {
            if (IsTransactionProcessing)
                Status = message;
            IsTransactionProcessing = true;
        }
        public void OnSaleResponse(CloverPaymentResponse response)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PaymentSuccessed.Invoke(this, response);

            });
            Status = "Completed";
            IsTransactionProcessing = false;
            try
            {
                HikePOS.Helpers.Logger.SaleLogger("Clover Success Payment- " + Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


        }
        public void OnRefundPaymentResponse(CloverPaymentResponse response)
        {
             MainThread.BeginInvokeOnMainThread(() =>
            {
                PaymentSuccessed.Invoke(this, response);

            });

            Status = "Completed";
            IsTransactionProcessing = false;
            try
            {
                HikePOS.Helpers.Logger.SaleLogger("Clover Success Refund- " + Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
        public void OnTransactionTimedOut()
        {
            if (IsTransactionProcessing)
            {
                CancelText = "Retry";
                IsTransactionProcessing = false;
                ChargeVisible = false;
            }


        }
        private Random random = new Random();

        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void OnDeviceReset()
        {
            if (IsRetried)
            {
                IsRetried = false;
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Task.Delay(500).Wait();
                    Charge();

                });
            }
            else
            {
                Status = "Connected";
                IsTransactionProcessing = false;
                ChargeVisible = true;
            }
            

        }

        public void OnTransactionStart()
        {
            CancelVisible = false;
        }
        public void OnManualPaymentResponse(CloverPaymentResponse response)
        {
        }


        #endregion
    }
}
