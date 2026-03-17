using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.Services.Payment;

namespace HikePOS.ViewModels
{
    public class TyroTapToPayConfigurationViewModel : BaseViewModel, ITyroTapToPayListener
    {
        public event EventHandler<TyroTapToPayConfigurationDto> ConfigurationSuccessed;
        public TyroTapToPayConfigurationDto ConfigurationModel { get; set; }
        public PaymentOptionDto PaymentOption { get; set; }

        string _ReaderStatus = "Please connect the reader";
        public string ReaderStatus { get { return _ReaderStatus; } set { _ReaderStatus = value; SetPropertyChanged(nameof(ReaderStatus)); } }

        bool _IsReaderConnected = false;
        public bool IsReaderConnected { get { return _IsReaderConnected; } set { _IsReaderConnected = value; SetPropertyChanged(nameof(IsReaderConnected)); } }

        bool _IsReaderConnecting = false;
        public bool IsReaderConnecting { get { return _IsReaderConnecting; } set { _IsReaderConnecting = value; SetPropertyChanged(nameof(IsReaderConnecting)); } }

        string _PaymentOptionName = "";
        public string PaymentOptionName { get { return _PaymentOptionName; } set { _PaymentOptionName = value; SetPropertyChanged(nameof(PaymentOptionName)); } }

        public ICommand ConnectCommand { get; set; }
        public ICommand SettingCommand { get; }

        ITyroTapToPay iTyroTapToPay;
        TyroTapToPayService Service = new TyroTapToPayService();
        private ApproveAdminPage approveAdminPage;

        public TyroTapToPayConfigurationViewModel()
        {
            ConnectCommand = new Command(Connect);
            SettingCommand = new Command(Setting);

        }
        public override async void OnAppearing()
        {
            base.OnAppearing();
            PaymentOptionName = PaymentOption.DisplayName;
            await CheckReaderConncetionStatus();
        }
        #region Methods

        async Task CheckReaderConncetionStatus()
        {
            if (Settings.TyroTapToPayConfiguration != null && Settings.TyroTapToPayConfiguration.Id != 0 && PaymentOption.Id != Settings.TyroTapToPayConfiguration.Id)
            {
                IsReaderConnected = false;
                ReaderStatus = $"You've already set up {Settings.TyroTapToPayConfiguration.Name}. Would you still like to configure {PaymentOption.Name}?";
            }
            else if (!string.IsNullOrEmpty(Settings.TyroTapToPayConnectionSecret))
            {
                // IsReaderConnected = true;
                ReaderStatus = "This payment is configured successfully and you are ready to recieve payment from the checkout page";
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    await ConnectReader();
                else
                    IsReaderConnected = true;

            }
            else
            {
                IsReaderConnected = false;
                ReaderStatus = "Please connect the reader";
            }
        }
        async Task GetConnection()
        {
            var response = await Service.GetTyroTapToPayConnection(PaymentOption.Id);
            if (response != null && response.ConnectionSecret != null)
            {
                Settings.TyroTapToPayConnectionSecret = response.ConnectionSecret;
                Settings.TyroTapToPayConfiguration = PaymentOption;
                await ConnectReader();
            }
            else if (response != null && response.ExistDifferentPaymentOption == true)
            {
                ReaderStatus = $"Unable to configure this payment method as the device is already associated with {response.DifferentPaymentName}. Please unauthorize or remove it from the cloud to continue.";
            }
            else
            {
                ReaderStatus = "Failed to connect";
            }
        }
        async Task ConnectReader()
        {
            iTyroTapToPay = DependencyService.Get<ITyroTapToPay>();
            var result = await iTyroTapToPay.DeviceConfigure(this, Settings.TyroTapToPayConnectionSecret);
            if (result.Success)
            {
                ReaderStatus = "This payment is configured successfully and you are ready to recieve payment from the checkout page";
                IsReaderConnected = true;
            }
            else
            {
                ReaderStatus = result.ErrorMessage;
                IsReaderConnected = false;
            }
        }

        async void Connect()
        {
            IsReaderConnecting = true;
            Settings.TyroTapToPayConnectionSecret = "";
            ReaderStatus = "Connecting, please wait...";
            await GetConnection();
            if (IsReaderConnected)
                ConfigurationSuccessed?.Invoke(this, ConfigurationModel);
            IsReaderConnecting = false;

        }
        public void OnReaderUpdate(string message)
        {
            ReaderStatus = message;
        }
        async void Setting()
        {
            if (approveAdminPage == null)
            {
                approveAdminPage = new ApproveAdminPage();
                approveAdminPage.ViewModel.Users = new ObservableCollection<UserListDto>(approveAdminPage.ViewModel.Users);
                approveAdminPage.ViewModel.IsDescriptionNotShown = true;
                approveAdminPage.SelectedUser += async (object sender, UserListDto e) =>
                {
                    Settings.TyroTapToPayRefundPasscodeApprovedBy = e.UserName;
                    await approveAdminPage.Close();
                    iTyroTapToPay.OpenSetting();

                };
            }
            await App.Instance.MainPage.Navigation.PushModalAsync(approveAdminPage);
        }

        #endregion
    }
}

