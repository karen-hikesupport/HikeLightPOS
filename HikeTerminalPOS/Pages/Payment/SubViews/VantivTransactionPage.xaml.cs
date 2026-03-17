using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;
using HikePOS.Enums;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS
{
    public partial class VantivTransactionPage : PopupBasePage<VantivTransactionViewModel>
    {
        public event EventHandler<VantivReceiptPrintModel> PaymentSuccessed;
        IVantiv vantiv;

        bool isActive = false;

        //Ticket #11319 Start : Add logs for payment types. By Nikhil
        public PaymentOptionDto paymentOption { get; set; }
        public SubmitLogServices logService { get; set; }
        public Dictionary<string, string> logRequestDetails { get; set; }


        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;
        //Ticket #11319 End. By Nikhil

        public VantivTransactionPage()
        {
            InitializeComponent();
            Title = "Vantiv payment";
            saleService = new SaleServices(saleApiService);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            DeviceStatusLabel.Text = string.Empty;
            DeviceStatusTitleLabel.IsVisible = false;

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.VantivTransactionCompleteCallbackMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.VantivTransactionCompleteCallbackMessenger>(this, (sender, arg) =>
                {
                    if (arg?.Value != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            DeviceStatusLabel.Text = arg.Value.TransactionStatus;
                            DeviceStatusTitleLabel.IsVisible = true;
                        });
                        PaymentSuccessed?.Invoke(this, arg.Value);
                    }
                    isPaymentProccessActive = false;
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.PosDeviceStatusMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.PosDeviceStatusMessenger>(this, (sender, arg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DeviceStatusLabel.Text = arg.Value;
                        DeviceStatusTitleLabel.IsVisible = true;
                    });
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.PosDeviceStatusMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.PosDeviceStatusMessenger>(this, (sender, arg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DeviceStatusLabel.Text = arg.Value;
                        DeviceStatusTitleLabel.IsVisible = true;
                    });
                });
            }

        }

        protected override void OnDisappearing()
        {
            isActive = false;
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messenger.PosDeviceStatusMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.VantivTransactionCompleteCallbackMessenger>(this);
        }


        bool isPaymentProccessActive = false;

        void PayHandle_Clicked(object sender, System.EventArgs e)
        {
            if (ViewModel == null || ViewModel.ConfigurationModel == null || string.IsNullOrEmpty(ViewModel.ConfigurationModel.AcceptorId) || string.IsNullOrEmpty(ViewModel.ConfigurationModel.AccountId) || string.IsNullOrEmpty(ViewModel.ConfigurationModel.AccountToken) || string.IsNullOrEmpty(ViewModel.ConfigurationModel.TerminalId))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("VantivConfigurationMessage"));
                return;
            }

            if (vantiv == null)
            {
                vantiv = DependencyService.Get<IVantiv>();
            }

            var Register = Settings.CurrentRegister;

            //Ticket #11319 Start : Add logs for payment types. By Nikhil 
            logRequestDetails.Add("IsRefund", (ViewModel.Invoice.Status == Enums.InvoiceStatus.Refunded).ToString());
            logRequestDetails.Add("RegisterId", Register.Id.ToString());
            logRequestDetails.Add("Amount", ViewModel.Invoice.TenderAmount.ToString());
            //Ticket start:#21625 Add Invoice number and Integrated payment response in API log.by rupesh
            logRequestDetails.Add("InvoiceNumber", ViewModel.Invoice.Number);
            //Ticket end:#21625.by rupesh
            Extensions.SendLogsToServer(logService, paymentOption, logRequestDetails);
            //Ticket #11319 End : By Nikhil

            if (ViewModel.Invoice.Status != Enums.InvoiceStatus.Refunded)
            {

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                string tenderAmount = (ViewModel.Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0)).ToString();
                //End ticket #73190  By Rupesh
                SaveInLocalbeforePayment(tenderAmount, false);
                vantiv.MakeSale(tenderAmount, Register.Id.ToString(), ViewModel.ConfigurationModel);
            }
            else
            {
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                var amount = (ViewModel.Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0)) * -1;
                //End ticket #73190  By Rupesh
                SaveInLocalbeforePayment(amount.ToString(), true);
                vantiv.MakeRefund(amount.ToString(), Register.Id.ToString(), ViewModel.ConfigurationModel);
            }
            isPaymentProccessActive = true;
        }

        async void CloseHandle_Clicked(object sender, System.EventArgs e)
        {
            if (isPaymentProccessActive)
            {
                if (vantiv == null)
                {
                    vantiv = DependencyService.Get<IVantiv>();
                }
                vantiv.Cancel();
                isPaymentProccessActive = false;
            }
            await Close();

        }

        public async Task Close()
        {
            try
            {
                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(string amount, bool isRefund)
        {
            //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
            if (ViewModel.Invoice.Status == InvoiceStatus.Refunded || ViewModel.Invoice.Status == InvoiceStatus.Exchange)
            {
                return;
            }
            //Ticket end:#24764 .by rupesh

            string currentPaymentObject = string.Empty;
            if (isRefund)
                currentPaymentObject = "Vantiv Refund : ";
            else
                currentPaymentObject = "Vantiv Sale :  ";

            // vantiv.MakeRefund(amount.ToString(), Register.Id.ToString(), ViewModel.ConfigurationModel);

            string configuration = Newtonsoft.Json.JsonConvert.SerializeObject(ViewModel.ConfigurationModel).ToString();

            currentPaymentObject = currentPaymentObject + " : "+ amount + " : " + configuration;



            ViewModel.Invoice.CurrentPaymentObject = currentPaymentObject;
            ViewModel.Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;


            await saleService.UpdateLocalInvoice(ViewModel.Invoice, LocalInvoiceStatus.PaymentProcessing);

        }
        #endregion
    }
}
