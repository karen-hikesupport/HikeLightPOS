using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Newtonsoft.Json;

namespace HikePOS.ViewModels
{
    public class VantivConfigurationViewModel: BaseViewModel
    {

        public event EventHandler<VantivConfigurationDto> ConfigurationSuccessed;

        string _AcceptorId { get; set; }
        public string AcceptorId { get { return _AcceptorId; } set { _AcceptorId = value; SetPropertyChanged(nameof(AcceptorId)); } }
        string _AccountId { get; set; }
        public string AccountId { get { return _AccountId; } set { _AccountId = value; SetPropertyChanged(nameof(AccountId)); } }
        string _AccountToken { get; set; }
        public string AccountToken { get { return _AccountToken; } set { _AccountToken = value; SetPropertyChanged(nameof(AccountToken)); } }

        string _TerminalId { get; set; }
        public string TerminalId { get { return _TerminalId; } set { _TerminalId = value; SetPropertyChanged(nameof(TerminalId)); } }




        bool _ArePartialApprovalsAllowed { get; set; }
        public bool ArePartialApprovalsAllowed { get { return _ArePartialApprovalsAllowed; } set { _ArePartialApprovalsAllowed = value;  SetPropertyChanged(nameof(ArePartialApprovalsAllowed));} }

        bool _AreDuplicateTransactionsAllowed { get; set; }
        public bool AreDuplicateTransactionsAllowed { get { return _AreDuplicateTransactionsAllowed; } set { _AreDuplicateTransactionsAllowed = value; SetPropertyChanged(nameof(AreDuplicateTransactionsAllowed));} }

        //bool _IsCashbackAllowed { get; set; }
        //public bool IsCashbackAllowed { get { return _IsCashbackAllowed; } set { _IsCashbackAllowed = value; SetPropertyChanged(nameof(IsCashbackAllowed));} }

        //double _CashbackAmount { get; set; }
        //public double CashbackAmount { get { return _CashbackAmount; } set { _CashbackAmount = value; SetPropertyChanged(nameof(CashbackAmount));} }

        bool _IsDebitAllowed { get; set; }
        public bool IsDebitAllowed { get { return _IsDebitAllowed; } set { _IsDebitAllowed = value; SetPropertyChanged(nameof(IsDebitAllowed));} }

        bool _IsEmvAllowed { get; set; }
        public bool IsEmvAllowed { get { return _IsEmvAllowed; } set { _IsEmvAllowed = value; SetPropertyChanged(nameof(IsEmvAllowed));} }



        //string _DeviceStatus { get; set; }
        //public string DeviceStatus { get { return _DeviceStatus; } set { _DeviceStatus = value; SetPropertyChanged(nameof(DeviceStatus)); } }


        public ICommand LoginCommand { get; set; }

        public ICommand HandleCommand { get; set; }

        public ICommand ClearSettingCommand { get; set; }

        public VantivConfigurationDto ConfigurationModel { get; set; }

        public VantivConfigurationViewModel()
        {
            //TerminalId= "8662";
            //AccountId = "1050045";
            //AcceptorId = "3928907";
            //AccountToken = "7FC4331ED4D5F07F469EE41970BB0FDA080FC2B34098BCFF86547153C47F175C720F0901";

            CheckVantivDetail();
           
            LoginCommand = new Command(VantivConfiguration);
            ClearSettingCommand = new Command(ClearVantivSetting);
            HandleCommand = new Command(HandleTapped);
        }

        #region Methods 
        async void VantivConfiguration()
        {
            try
            {
                using (new Busy(this, true))
                {
                    if (string.IsNullOrEmpty(AccountToken) || string.IsNullOrWhiteSpace(AccountToken))
                    {
                        App.Instance.Hud.DisplayToast("Please enter Account Token");
                        return;
                    }

                    if (string.IsNullOrEmpty(AccountId) || string.IsNullOrWhiteSpace(AccountId))
                    {
                        App.Instance.Hud.DisplayToast("Please enter Account Id");
                        return;
                    }

                    if (string.IsNullOrEmpty(AcceptorId) || string.IsNullOrWhiteSpace(AcceptorId))
                    {
                        App.Instance.Hud.DisplayToast("Please enter Acceptor Id");
                        return;
                    }

                    if (string.IsNullOrEmpty(TerminalId) || string.IsNullOrWhiteSpace(TerminalId))
                    {
                        App.Instance.Hud.DisplayToast("Please enter Terminal Id");
                        return;
                    }

                    //if (IsCashbackAllowed && CashbackAmount < 1)
                    //{
                    //    App.Instance.Hud.DisplayToast("Please enter valid Cashback Amount");
                    //    return;
                    //}

                    IVantiv vantiv = DependencyService.Get<IVantiv>();
                    VantivConfigurationDto configurationModel = new VantivConfigurationDto()
                    {
                        TerminalId = TerminalId,
                        AccountId = AccountId,
                        AcceptorId = AcceptorId,
                        AccountToken = AccountToken,
                        ArePartialApprovalsAllowed = ArePartialApprovalsAllowed,
                        AreDuplicateTransactionsAllowed = AreDuplicateTransactionsAllowed,
                        IsCashbackAllowed = false,// IsCashbackAllowed,
                       CashbackAmount = 0,//CashbackAmount,
                       IsDebitAllowed = IsDebitAllowed,
                       IsEmvAllowed = IsEmvAllowed
                    };
                    await Task.Delay(10);
                    bool result = await vantiv.DeviceConfigure(configurationModel);
                    if (result)
                    {
                        Settings.Vantivsettings = configurationModel;
                        ConfigurationSuccessed?.Invoke(this, configurationModel);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            //}

        }
  
        private void CheckVantivDetail()
        {
            // Zoho ticket : 8139 : Vantiv configuration issue by Nikhil
            if (Settings.Vantivsettings != null)
            {
                AccountToken = Settings.Vantivsettings.AccountToken;
                AccountId = Settings.Vantivsettings.AccountId;
                AcceptorId = Settings.Vantivsettings.AcceptorId;
                TerminalId = Settings.Vantivsettings.TerminalId;
                IsEmvAllowed = Settings.Vantivsettings.IsEmvAllowed;
                IsDebitAllowed = Settings.Vantivsettings.IsDebitAllowed;
                ArePartialApprovalsAllowed = Settings.Vantivsettings.ArePartialApprovalsAllowed;
                AreDuplicateTransactionsAllowed = Settings.Vantivsettings.AreDuplicateTransactionsAllowed;
            }
        }

        void HandleTapped()
        {
            ClearVantivSetting();
        }

        public async void ClearVantivSetting()
        {

            if (AccountId != null || AcceptorId != null || AccountToken != null || TerminalId != null ||
                IsEmvAllowed == true || IsDebitAllowed == true || ArePartialApprovalsAllowed == true ||
                AreDuplicateTransactionsAllowed == true)
            { 
            var result = await App.Alert.ShowAlert("Confirmation", "Are you sure you wish to remove existing and re-enter all configuration data?", "Yes", "No");

                if (result)
                {
                    AccountId = string.Empty;
                    AcceptorId = string.Empty;
                    AccountToken = string.Empty;
                    TerminalId = string.Empty;
                    IsEmvAllowed = false;
                    IsDebitAllowed = false;
                    ArePartialApprovalsAllowed = false;
                    AreDuplicateTransactionsAllowed = false;
                    Settings.Vantivsettings = null;
                }

            }
        }
        #endregion

    }
}
