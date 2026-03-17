using System;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models.Payment;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public partial class CloverConfigurationViewModel : BaseViewModel,ICloverListener
    {
        public event EventHandler<CloverConfigurationDto> ConfigurationSuccessed;
        public CloverConfigurationDto ConfigurationModel { get; set; }
        string _applicationId { get; set; }
        public string ApplicationId { get { return _applicationId; } set { _applicationId = value; SetPropertyChanged(nameof(ApplicationId)); } }
        string _pOSName { get; set; }
        public string POSName { get { return _pOSName; } set { _pOSName = value; SetPropertyChanged(nameof(POSName)); } }
        string _pOSSerial { get; set; }
        public string POSSerial { get { return _pOSSerial; } set { _pOSSerial = value; SetPropertyChanged(nameof(POSSerial)); } }

        string _connectionURL { get; set; }
        public string ConnectionURL { get { return _connectionURL; } set { _connectionURL = value; SetPropertyChanged(nameof(ConnectionURL)); } }

        public string _iPAddress { get; set; }
        public string IPAddress { get { return _iPAddress; } set { _iPAddress = value; SetPropertyChanged(nameof(IPAddress)); } }

        public string _port { get; set; }
        public string Port { get { return _port; } set { _port = value; SetPropertyChanged(nameof(Port)); } }

        bool _isStatusDisplay { get; set; }
        public bool IsStatusDisplay { get { return _isStatusDisplay; } set { _isStatusDisplay = value; SetPropertyChanged(nameof(IsStatusDisplay)); } }

        string _status = "Please wait connecting...";
        public string Status
        { get { return _status; }
          set
            {
                _status = value;
                SetPropertyChanged(nameof(Status));
            }
        }
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

        string _pairUnpairText = "Pair";
        public string PairUnpairText { get { return _pairUnpairText; } set { _pairUnpairText = value; SetPropertyChanged(nameof(PairUnpairText)); } }

        public ICommand PairCommand { get; set; }

        public ICommand ClearConfigurationCommand { get; set; }
        IClover clover = null;
        bool Ready = false;
        public CloverConfigurationViewModel()
        {
            //ApplicationId = "SGB1HATS2Y1VE.P48RFXP03DPSW";
            
            //ApplicationId = "SGB1HATS2Y1VE.XTHDE169PZWVT"; //Test
            ApplicationId = "0KRR16N1Z7BW8.N2Z35VKZH44N6"; // Live
            POSName = "Hike POS";
            POSSerial = "POS-15";
            CheckCloverDetail();
            PairCommand = new Command(Pair);
            ClearConfigurationCommand = new Command(HandleTapped);
        }
        public override void OnAppearing()
        {
            base.OnAppearing();
            IsStatusDisplay = false;
            if (string.IsNullOrEmpty(ConfigurationModel?.AuthToken))
            {
                PairUnpairText = "Pair";
                Status = "";
            }
            else
            {
                PairUnpairText = "UnPair";
                Status = "Connected";

            }
            IPAddress = ConfigurationModel?.IPAddress;
            Port = ConfigurationModel?.Port;
            if (clover == null)
                 clover = DependencyService.Get<IClover>();

        }
        #region Methods
        void Pair()
        {
            if (PairUnpairText.Contains("Cancel"))
            {
                PairUnpairText = "Pair";
                IsStatusDisplay = false;
                PairingCodeText = string.Empty;
                clover?.Dispose();
            }
            else
            {
                CloverConfiguration();
            }
        }
        async void CloverConfiguration()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationModel?.AuthToken))
                {
                    //using (new Busy(this, true))
                    {
                        if (string.IsNullOrEmpty(ApplicationId) || string.IsNullOrWhiteSpace(ApplicationId))
                        {
                            App.Instance.Hud.DisplayToast("Please enter Application Id");
                            return;
                        }

                        if (string.IsNullOrEmpty(POSName) || string.IsNullOrWhiteSpace(POSName))
                        {
                            App.Instance.Hud.DisplayToast("Please enter POS Name");
                            return;
                        }

                        if (string.IsNullOrEmpty(POSSerial) || string.IsNullOrWhiteSpace(POSSerial))
                        {
                            App.Instance.Hud.DisplayToast("Please enter POS Serial");
                            return;
                        }

                        if (string.IsNullOrEmpty(ConnectionURL) || string.IsNullOrWhiteSpace(ConnectionURL))
                        {
                            App.Instance.Hud.DisplayToast("Please enter Connection URL");
                            return;
                        }

                        if (Uri.TryCreate(ConnectionURL, UriKind.Absolute, out Uri uri))
                        {
                            if (uri.Scheme == "wss" || uri.Scheme == "ws")
                            {
                                IPAddress = uri.Host;
                                Port = uri.Port.ToString();
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast("Invalid WebSocket scheme");
                            }
                        }
                        else
                        {
                             App.Instance.Hud.DisplayToast("Invalid URL format");
                        }
                        if (string.IsNullOrEmpty(IPAddress) || string.IsNullOrWhiteSpace(IPAddress))
                        {
                            App.Instance.Hud.DisplayToast("Please enter IPAddress in connection URL");
                            return;
                        }
                        if (string.IsNullOrEmpty(Port) || string.IsNullOrWhiteSpace(Port))
                        {
                            App.Instance.Hud.DisplayToast("Please enter Port in connection URL");
                            return;
                        }

                        //ConnectionURL = $"ws://{IPAddress}:{Port}/remote_pay";

                        if (ConfigurationModel == null)
                            ConfigurationModel = new CloverConfigurationDto();

                        ConfigurationModel.ApplicationId = ApplicationId;
                        ConfigurationModel.POSName = POSName;
                        ConfigurationModel.POSSerial = POSSerial;
                        ConfigurationModel.ConnectionURL = ConnectionURL;
                        ConfigurationModel.IPAddress = IPAddress;
                        ConfigurationModel.Port = Port;

                        IsStatusDisplay = true;
                        Status = "Please wait connecting...";
                        PairUnpairText = "Cancel";
                        clover.Cancel();
                        await Task.Delay(1);
                        bool result = await clover.DeviceConfigure(this, ConfigurationModel);
                        if (result)
                        {

                        }
                        // IsStatusDisplay = false;
                    }
                }
                else
                {
                    ConfigurationModel = null;
                    Settings.Cloversettings = null;
                    ConfigurationSuccessed?.Invoke(this, null);
                    IsStatusDisplay = false;
                    PairUnpairText = "Pair";
                    clover?.Cancel();
                    clover?.Dispose();

                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void CheckCloverDetail()
        {
            if (Settings.Cloversettings != null)
            {
                ApplicationId = Settings.Cloversettings.ApplicationId;
                POSName = Settings.Cloversettings.POSName;
                POSSerial = Settings.Cloversettings.POSSerial;
                ConnectionURL = Settings.Cloversettings.ConnectionURL;
                ConfigurationModel = Settings.Cloversettings;
            }
        }

        void HandleTapped()
        {
            ClearCloverSetting();
        }

        public async void ClearCloverSetting()
        {

            if (ApplicationId != null || POSName != null || POSSerial != null || ConnectionURL != null)
            {
                var result = await App.Alert.ShowAlert("Confirmation", "Are you sure you wish to remove existing and re-enter all configuration data?", "Yes", "No");
                if (result)
                {
                    ApplicationId = string.Empty;
                    POSName = string.Empty;
                    POSSerial = string.Empty;
                    ConnectionURL = string.Empty;
                }

            }
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
            clover?.Cancel();

        }
        public void OnDeviceError(string error)
        {
            Status = error;
        }
        public void OnDeviceConnected()
        {
            //Status = "Clover device is connected, but not available to process requests";
            Ready = false; // the device is connected, but not ready to communicate

        }

        public void OnDeviceDisconnected()
        {
            if (Ready) {
                Status = "Disconnected"; //"Clover device is not available";
            }
            Ready = false;

        }

        public void OnDeviceReady()
        {
            Status = "Connected";
            PairUnpairText = "UnPair";
            Settings.Cloversettings = ConfigurationModel;
            ConfigurationSuccessed?.Invoke(this, ConfigurationModel);
            Ready = true;

        }

        public void OnDeviceActivityEnd(string message)
        {
        }

        public void OnDeviceActivityStart(string message)
        {
        }

        public void OnSaleResponse(CloverPaymentResponse response)
        {
        }

        public void OnRefundPaymentResponse(CloverPaymentResponse response)
        {
        }

        public void OnTransactionTimedOut()
        {
        }

        public void OnDeviceReset()
        {
        }

        public void OnTransactionStart()
        {
        }

        public void OnManualPaymentResponse(CloverPaymentResponse response)
        {
        }


        #endregion
    }
}

