using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.Services.Payment;

namespace HikePOS.ViewModels;

public class TyroTapToPayPaymentViewModel : BaseViewModel, ITyroTapToPayListener
{
    public event EventHandler<TyroTapToPayPaymentResponse> PaymentSuccessed;
    ITyroTapToPay iTyroTapToPay;
    InvoiceDto _Invoice { get; set; }
    public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }
    string _status { get; set; }
    public string Status { get { return _status; } set { _status = value; SetPropertyChanged(nameof(Status)); } }

    string _ReaderStatus = "";
    public string ReaderStatus { get { return _ReaderStatus; } set { _ReaderStatus = value; SetPropertyChanged(nameof(ReaderStatus)); } }

    bool _IsReaderConnected = false;
    public bool IsReaderConnected { get { return _IsReaderConnected; } set { _IsReaderConnected = value; SetPropertyChanged(nameof(IsReaderConnected)); } }
    bool _IsRefund = false;
    public bool IsRefund { get { return _IsRefund; } set { _IsRefund = value; SetPropertyChanged(nameof(IsRefund)); } }

    public PaymentOptionDto PaymentOption { get; set; }
    public TyroTapToPayConfigurationDto ConfigurationModel { get; set; }
    public SubmitLogServices logService { get; set; }
    public Dictionary<string, string> logRequestDetails { get; set; }
    public SaleServices SaleService { get; internal set; }
    public ICommand ChargeCommand { get; }
    public ICommand SettingCommand { get; }
    public ICommand ConnectCommand { get; }
    public string SaleCommand = string.Empty;
    TyroTapToPayService Service = new TyroTapToPayService();
    int RetryCount = 0;
    private ApproveAdminPage approveAdminPage;
    bool _isShowActivityIndicator = false;
    public bool IsShowActivityIndicator { get { return _isShowActivityIndicator; } set { _isShowActivityIndicator = value; SetPropertyChanged(nameof(IsShowActivityIndicator)); } }

    public TyroTapToPayPaymentViewModel()
    {
        ChargeCommand = new Command(Charge);
        SettingCommand = new Command(Setting);
        ConnectCommand = new Command(ConnectCallToReader);

    }

    public override void OnAppearing()
    {
        base.OnAppearing();
        OnAppearingCall();
    }
    public async void OnAppearingCall()
    {
        Status = "";
        RetryCount = 0;
        if (SaleCommand == "Sale")
            IsRefund = false;
        else
            IsRefund = true;
            
        if (Invoice?.Status == InvoiceStatus.RefundedAndDiscard)
            IsRefund = false;

        await CheckReaderConnectionStatus();
        if (DeviceInfo.Platform == DevicePlatform.Android)
            IsShowActivityIndicator = false;

    }
    async Task CheckReaderConnectionStatus()
    {
        // await Task.Delay(1000);
        // if(Settings.TyroTapToPayConfiguration != null && Settings.TyroTapToPayConfiguration.Id != 0 && PaymentOption.Id != Settings.TyroTapToPayConfiguration.Id)
        // {
        //     var result = await App.Instance.Windows[0].Page.DisplayAlert(LanguageExtension.Localize("Warning_Title"),$"You have already configured a payment {Settings.TyroTapToPayConfiguration.Name}. Do you still want to set up {PaymentOption.Name} payment?", "Yes","Cancel");
        //     if(result)
        //     {
        //         Settings.TyroTapToPayConnectionSecret = "";
        //         IsReaderConnected = false;
        //     }
        //     else
        //     {
        //         await Close();
        //         return;

        //     }
        // }
        if (string.IsNullOrEmpty(Settings.TyroTapToPayConnectionSecret))
        {
            ReaderStatus = "Connecting, please wait...";
            await GetConnection();
        }
        else if (!IsReaderConnected)
        {
            ReaderStatus = "Connecting, please wait...";
            await ConnectReader();
        }

    }
    async Task GetConnection()
    {
        RetryCount++;
        var response = await Service.GetTyroTapToPayConnection(PaymentOption.Id);
        if(response != null && response.ConnectionSecret != null)
        {
            Settings.TyroTapToPayConnectionSecret = response.ConnectionSecret;
            Settings.TyroTapToPayConfiguration = PaymentOption;
            await ConnectReader();

        }
        else if(response != null && response.ExistDifferentPaymentOption == true)
        {
             await App.Instance.Windows[0].Page.DisplayAlert(LanguageExtension.Localize("Warning_Title"),$"Unable to configure this payment method as the device is already associated with {Settings.TyroTapToPayConfiguration?.Name}. Please unauthorize or remove it from the cloud to continue.", "Ok");
             await Close();
        }
    }
    async Task ConnectReader()
    {
        iTyroTapToPay = DependencyService.Get<ITyroTapToPay>();
        var result = await iTyroTapToPay.DeviceConfigure(this, Settings.TyroTapToPayConnectionSecret);
        if (result.Success)
        {
            ReaderStatus = "Connected";
            IsReaderConnected = true;
        }
        else
        {
            ReaderStatus = result.ErrorMessage;
            if (DeviceInfo.Platform == DevicePlatform.Android)
                App.Instance.Hud.DisplayToast(ReaderStatus, Colors.Red, Colors.White);

            if (RetryCount < 1)
            {
                await Task.Delay(1000);
                await GetConnection();
            }
            else
            {
                IsReaderConnected = false;

            }

        }


    }
    private async void ConnectCallToReader()
    {
        await GetConnection();
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
    public async void ChargeStart()
    {
        await Task.Delay(100);
        IsShowActivityIndicator = true;
        while (true)
        {
            if (IsReaderConnected)
            {
                IsShowActivityIndicator = false;
                Charge();
                break;
            }

            Console.WriteLine("Reader is connected");
            await Task.Delay(100);

        }

    }
    public async void Charge()
    {
        try
        {

            logRequestDetails.TryAdd("TyroTapToPayConfigurationDto", PaymentOption.ConfigurationDetails);
            logRequestDetails.TryAdd("IsRefund", (Invoice.Status == InvoiceStatus.Refunded).ToString());
            logRequestDetails.TryAdd("Amount", Invoice.TenderAmount.ToString());
            logRequestDetails.TryAdd("InvoiceNumber", Invoice.Number);
            var lastReference = Guid.NewGuid().ToString();
            logRequestDetails.TryAdd("Reference", lastReference);
            Extensions.SendLogsToServer(logService, PaymentOption, logRequestDetails);
            var amount = (Invoice.TenderAmount + (PaymentOption.DisplaySurcharge ?? 0));
            string tempobject = "Sale: " + Newtonsoft.Json.JsonConvert.SerializeObject(ConfigurationModel) + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;
            IsShowActivityIndicator = true;
            TyroTapToPayResponse tyroTapToPayResponse;
            if (amount > 0)
            {
                Status = "Processing payment...";
                SaveInLocalbeforePayment(tempobject, false);
                tyroTapToPayResponse = await iTyroTapToPay.Sale(Decimal.ToDouble(amount), lastReference);
            }
            else
            {
                Status = "Processing refund...";
                if (Invoice.Status != InvoiceStatus.RefundedAndDiscard)
                    SaveInLocalbeforePayment(tempobject, true);
                tyroTapToPayResponse = await iTyroTapToPay.Refund(Decimal.ToDouble(amount.ToPositive()), lastReference);
            }
            if (tyroTapToPayResponse.Success)
            {
                var paymentResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TyroTapToPayPaymentResponse>(tyroTapToPayResponse.payment);
                if (paymentResponse.statusCode.ToLower() == "approved")
                {
                    Status = "Approved";
                    PaymentSuccessed.Invoke(this, paymentResponse);
                    // if (DeviceInfo.Platform == DevicePlatform.Android)
                    //     App.Instance.Hud.DisplayToast(Status, Colors.Green, Colors.White);

                }
                else
                {
                    Status = paymentResponse.statusMessage;
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                        App.Instance.Hud.DisplayToast(Status, Colors.Red, Colors.White);

                }

            }
            else
            {
                Status = tyroTapToPayResponse.ErrorMessage ?? "";
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    App.Instance.Hud.DisplayToast(Status, Colors.Red, Colors.White);

                if (Status.IndexOf("FailedToVerifyConnection", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    RetryCount = 0;
                    await GetConnection();
                }
                else if (Status.IndexOf("UnableToConnectReader", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    await ConnectReader();

                }

            }
            Logger.SyncLogger("----GetTapToPay Status---\n" + (Status ?? "Empty") + "\n----GetTapToPay response---\n" + tyroTapToPayResponse?.payment ?? "Empty");
            IsShowActivityIndicator = false;
        }
        catch (Exception ex)
        {
            IsShowActivityIndicator = false;
            Logger.SyncLogger("----GetTapToPay Exception---\n" + ex.Message ?? "Empty");


        }
    }



    #region Save invoice in local database before payment
    private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
    {

        // Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
        if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
         {
             return;
         }
       //Ticket end:#24764 .by rupesh
 
        string currentPaymentObject = string.Empty;
        if (isRefund)
            currentPaymentObject = "TyroTapToPay Refund : ";
        else
            currentPaymentObject = "TyroTapToPay Sale :  ";


        currentPaymentObject = currentPaymentObject + " : " + paymentObject;
        Invoice.CurrentPaymentObject = currentPaymentObject;
        Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;
        await SaleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);

    }

    public void OnReaderUpdate(string message)
    {
        ReaderStatus = message;
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            App.Instance.Hud.DisplayToast(ReaderStatus, Colors.Gray, Colors.White);
        }

    }




    #endregion
}
