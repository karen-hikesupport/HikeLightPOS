using System;
using System.Linq;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models.Payment;
using SPIClient;
using HikePOS.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using HikePOS.Models;
using System.Collections.ObjectModel;
using SPIClient.Service;
using Serilog;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public class AssemblyPaymentConfigurationViewModel : BaseViewModel
    {
        public event EventHandler<AssemblyPaymentConfigurationDto> ConfigurationSuccessed;
        public Regex validIpV4AddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexOptions.IgnoreCase);

        #region Proprties

        string _posId { get; set; }
        public string PosId
        {
            get
                { return _posId; }
            set
            { _posId = value; SetPropertyChanged(nameof(PosId)); } }


        string _eftposAddress { get; set; } //= "192.168.1.8";
        bool _IsAllowPrintOnEFTPOS { get; set; } //= false;
        
        public string EftposAddress { get { return _eftposAddress; } set { _eftposAddress = value; SetPropertyChanged(nameof(EftposAddress)); } }

        //#23024 iOS - Westpac Acquirer
        bool _IsProviderAvailable { get; set; } //= false;
        public bool IsProviderAvailable { get { return _IsProviderAvailable; } set { _IsProviderAvailable = value; SetPropertyChanged(nameof(IsProviderAvailable)); } }

        string _SelectedProviderName { get; set; } 
        public string SelectedProviderName { get { return _SelectedProviderName; } set { _SelectedProviderName = value; SetPropertyChanged(nameof(SelectedProviderName)); } }
        ////#23024 iOS - Westpac Acquirer
        public bool AllowPrintOnEFTPOS { get { return _IsAllowPrintOnEFTPOS; } set { _IsAllowPrintOnEFTPOS = value; SetPropertyChanged(nameof(AllowPrintOnEFTPOS)); } }


        //#35436 iOS- mx51 Suggested changes
        string _serialNumber { get; set; }
        public string SerialNumber { get { return _serialNumber; } set { _serialNumber = value; SetPropertyChanged(nameof(SerialNumber)); } }

        bool _IsTestMode { get; set; }
        public bool IsTestMode { get { return _IsTestMode; } set { _IsTestMode = value; SetPropertyChanged(nameof(IsTestMode)); } }
        //#35436 iOS- mx51 Suggested changes


        // Temprory code
        string _pairUnpairText { get; set; } = "Pair";
        public string PairUnpairText { get { return _pairUnpairText; } set { _pairUnpairText = value; SetPropertyChanged(nameof(PairUnpairText)); } }
       

        bool _isEftposAddressEnalble { get; set; }
        public bool IsEftposAddressEnalble { get { return _isEftposAddressEnalble; } set { _isEftposAddressEnalble = value; SetPropertyChanged(nameof(IsEftposAddressEnalble)); } }

        //bool _isPairEnabled { get; set; }
        //public bool IsPairEnabled { get { return _isPairEnabled; } set { _isPairEnabled = value; SetPropertyChanged(nameof(IsPairEnabled)); } }



        public bool _isotherProvider { get; set; } = false;
        public bool IsOtherProvider
        { get
            { return
                    _isotherProvider;
            }
            set
            { _isotherProvider = value;
                SetPropertyChanged(nameof(IsOtherProvider));


            } }


        string _otherProviderName { get; set; }
        public string OtherProviderName
        {
            get
            {
                return _otherProviderName;
            }
            set
            {
                _otherProviderName = value; SetPropertyChanged(nameof(OtherProviderName)); } }

        bool _action1Visible { get; set; }
        public bool Action1Visible { get { return _action1Visible; } set { _action1Visible = value; SetPropertyChanged(nameof(Action1Visible)); } }

        bool _action2Visible { get; set; }
        public bool Action2Visible { get { return _action2Visible; } set { _action2Visible = value; SetPropertyChanged(nameof(Action2Visible)); } }

        bool _action3Visible { get; set; }
        public bool Action3Visible { get { return _action3Visible; } set { _action3Visible = value; SetPropertyChanged(nameof(Action3Visible)); } }

        //Ticket #9484 Start: Westpac payment issue. By Nikhil.
        AssemblyPaymentConfigurationDto _configurationModel;
        //private TenantDetails provider;

        public AssemblyPaymentConfigurationDto ConfigurationModel
        {
            get
            { return _configurationModel; }
            set
            {
                _configurationModel = value;
                AllowPrintOnEFTPOS = _configurationModel.ReceiptFromEFTPOS;
            }
        }
        //Ticket #9484 End:By Nikhil.  

        bool _pairVisible { get; set; } = true;
        public bool PairVisible { get { return _pairVisible; } set { _pairVisible = value; SetPropertyChanged(nameof(PairVisible)); } }

        bool _isUnpaired { get; set; } = true;
        public bool IsUnpaired { get { return _isUnpaired; } set { _isUnpaired = value; SetPropertyChanged(nameof(IsUnpaired)); } }

       

        string _action1Text { get; set; }
        public string Action1Text { get { return _action1Text; } set { _action1Text = value; SetPropertyChanged(nameof(Action1Text)); } }

        string _action2Text { get; set; }
        public string Action2Text { get { return _action2Text; } set { _action2Text = value; SetPropertyChanged(nameof(Action2Text)); } }

        string _action3Text { get; set; }
        public string Action3Text { get { return _action3Text; } set { _action3Text = value; SetPropertyChanged(nameof(Action3Text)); } }


        string _processtatus { get; set; }
        public string ProcessStatus { get { return _processtatus; } set { _processtatus = value; SetPropertyChanged(nameof(ProcessStatus)); } }


        string _confirmationCode { get; set; }
        public string ConfirmationCode { get { return _confirmationCode; } set { _confirmationCode = value; SetPropertyChanged(nameof(ConfirmationCode)); } }


        string _processStatus2 { get; set; }
        public string ProcessStatus2 { get { return _processStatus2; } set { _processStatus2 = value; SetPropertyChanged(nameof(ProcessStatus2)); } }



        string _status { get; set; }
        public string Status { get { return _status; } set { _status = value; SetPropertyChanged(nameof(Status)); } }


        bool _chargeVisible { get; set; }
        public bool ChargeVisible { get { return _chargeVisible; } set { _chargeVisible = value; SetPropertyChanged(nameof(ChargeVisible)); } }



        //Ticket start:#23024 iOS - Westpac Acquirer.by rupesh
        ObservableCollection<TenantDetails> _TenantsList { get; set; }
        public ObservableCollection<TenantDetails> TenantsList
        {
            get { return _TenantsList; }
            set { _TenantsList = value; SetPropertyChanged(nameof(TenantsList)); }
        }
        public TenantDetails _SelectedTenant { get; set; }
        public TenantDetails SelectedTenant
        {
            get {
                return _SelectedTenant; }
            set
            {
                _SelectedTenant = value;


                Debug.WriteLine("Selected tenenat : " + _SelectedTenant.Code.ToString());
                // ChangeOtherTenent(_SelectedTenant);
                if (_SelectedTenant.Code == "Other")
                {
                    IsOtherProvider = true;
                    //SetAcquirerCode(OtherProviderName);
                }
                else
                {
                    IsOtherProvider = false;
                    
                    //SetAcquirerCode(SelectedTenant.Code);

                }


                SetPropertyChanged(nameof(SelectedTenant));
            }
        }
        //Ticket end:#23024.by rupesh
        ////

        #endregion

        #region Life Cycle
        public AssemblyPaymentConfigurationViewModel()
        {
            PosId = "HIKEPOS";
            IsProviderAvailable = false;
            SelectedProviderName = string.Empty;
            IsEftposAddressEnalble = true;
            IsOtherProvider = false;
            OtherProviderName = string.Empty;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            ProcessStatus = "";
            ConfirmationCode = "";
            ProcessStatus2 = "";
            Status = "";

            if (SPICommonViewModel._spi != null)
            {
                //if (!string.IsNullOrEmpty(ConfigurationModel.PosId))
                //{
                //  _spi.SetPosId(ConfigurationModel.PosId);
                //}

                if (ConfigurationModel != null)
                {
                    if (!string.IsNullOrEmpty(Settings.EFTPOSAddress))
                    {
                        SPICommonViewModel._spi.SetEftposAddress(Settings.EFTPOSAddress);
                        EftposAddress = Settings.EFTPOSAddress;
                        SerialNumber = Settings.SerialNumber;
                        IsEftposAddressEnalble = false;
                        //IsPairEnabled = true;




                        if (!string.IsNullOrEmpty(ConfigurationModel.AcquirerName) && !string.IsNullOrEmpty(ConfigurationModel.AcquirerCode))
                        {
                            Settings.AcquirerName = ConfigurationModel.AcquirerName;
                            Settings.AcquirerCode = ConfigurationModel.AcquirerCode;

                            SelectedProviderName = Settings.AcquirerName;
                            IsProviderAvailable = true;
                        }
                        else
                        {

                            IsProviderAvailable = false;
                        }

                    }
                    else
                    {
                        Status = "Unpaired";
                        IsEftposAddressEnalble = true;
                        //IsPairEnabled = true;
                    }
                    AllowPrintOnEFTPOS = ConfigurationModel.ReceiptFromEFTPOS;


                    IsTestMode = ConfigurationModel.IsTestMode; //#35436 iOS- mx51 Suggested changes
                    SPICommonViewModel._spi.Config.PromptForCustomerCopyOnEftpos = ConfigurationModel.ReceiptFromEFTPOS;
                    SPICommonViewModel._spi.Config.SignatureFlowOnEftpos = ConfigurationModel.ReceiptFromEFTPOS;
                }
                //SPICommonViewModel._spi.DeviceAddressChanged -= DeviceAddressChanged;
                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress

                //SPICommonViewModel._spi.DeviceAddressChanged += DeviceAddressChanged1;

                SPICommonViewModel._spi.StatusChanged += OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress
                //SPICommonViewModel._spi.DeviceAddressChanged += DeviceAddressChanged;
                PrintStatusAndActions();
            }
            else
            {
                Start();
            }

            // Get below information from mx51 team 
            //this.GetAvailableTenants("HIKEPOS1", "3TGxBItHM5BoBjImGEeohulSCqjZB0uc", Settings.CountryCode);

            // From web

            if (string.IsNullOrEmpty(Settings.AcquirerName) && string.IsNullOrEmpty(Settings.AcquirerCode))
            {
                this.GetAvailableTenants(PosId, ServiceConfiguration.DeviceApiKey, Settings.CountryCode);
            }



        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            if (SPICommonViewModel._spi != null)
            {
                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress
                                                                                    //SPICommonViewModel._spi.DeviceAddressChanged -= DeviceAddressChanged;

                SPICommonViewModel._spi.StatusChanged += OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress
                //SPICommonViewModel._spi.DeviceAddressChanged += DeviceAddressChanged1;
            }
        }
        #endregion

        #region Command

        public ICommand SaveCommand => new Command(Save);
        public ICommand ExportCommand => new Command(ExportTapped);
        public ICommand Action1Command => new Command(Action1);
        public ICommand Action2Command => new Command(Action2);
        public ICommand Action3Command => new Command(Action3);
        public ICommand PairUnpairCommand => new Command(Pair);
        #endregion

        #region Command Execution

        private void ExportTapped(object obj)
        {
            try
            {
                var bounds = ((Button)obj).Bounds;
                MainThread.BeginInvokeOnMainThread(async () => {
                    var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var path = Path.Combine(docFolder, "SpiLog");
                    if (Directory.Exists(path))
                    {
                        var files = Directory.EnumerateFiles(path);
                        var request = new ShareMultipleFilesRequest();
                        request.Title = "Share document";
                        var shareFiles = new List<ShareFile>();
                        foreach (var file in files)
                        {
                            shareFiles.Add(new ShareFile(file));
                        }
                        request.Files = shareFiles;
                        request.PresentationSourceBounds = new Microsoft.Maui.Graphics.Rect((int)bounds.Left, (int)bounds.Top, 10, 10);
                        await Share.RequestAsync(request);
                    }
                });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        void Save()
        {

            {

                if (SelectedTenant == null)
                {
                    App.Instance.Hud.DisplayToast("You must select a payment type before proceeding to use Hike POS");
                    return;
                }
                else
                {
                    if (SelectedTenant.Code == "Other" && string.IsNullOrEmpty(OtherProviderName))
                    {

                        App.Instance.Hud.DisplayToast("You must add a payment type before proceeding to use Hike POS");
                        return;
                    }
                    else
                    {
                        SetAcquirerCode(OtherProviderName);
                    }
                }
            }


            if (string.IsNullOrEmpty(PosId) || string.IsNullOrWhiteSpace(PosId))
            {
                App.Instance.Hud.DisplayToast("Please enter POS ID");
                return;
            }
            else
            {

                if (IsValidInput("PosId", PosId))
                {
                    App.Instance.Hud.DisplayToast("Please enter valid POS ID");
                    return;
                }
            }
            if (string.IsNullOrEmpty(SerialNumber) || string.IsNullOrWhiteSpace(SerialNumber))
            {
                App.Instance.Hud.DisplayToast("Please enter EFTPOS Serial Number");
                return;
            }
            else
            {

                if (!IsValidInput("SerialNumber", SerialNumber))
                {
                    App.Instance.Hud.DisplayToast("Please enter valid SerialNumber");
                    return;
                }
            }

            if (string.IsNullOrEmpty(EftposAddress) || string.IsNullOrWhiteSpace(EftposAddress))
            {
                App.Instance.Hud.DisplayToast("Please enter EFTPOS Address");
                return;
            }
            if (string.IsNullOrWhiteSpace(EftposAddress) || !validIpV4AddressRegex.IsMatch(EftposAddress))
            {
                App.Instance.Hud.DisplayToast("Please enter valid EFTPOS Address");
                return;
            }

            Settings.EFTPOSAddress = EftposAddress;

            IsEftposAddressEnalble = true;
            PairVisible = true;
            //IsPairEnabled = true;





            if (SPICommonViewModel._spi == null)
            {
                Start();
            }

        }

        async void Pair()
        {
            try
            {
                if (SPICommonViewModel._spi == null)
                {
                    Start();
                }

                if (PairUnpairText == "Pair")
                {
                    if (SelectedTenant == null)
                    {
                        App.Instance.Hud.DisplayToast("You must select a payment type before proceeding to use Hike POS");
                        return;
                    }
                    else
                    {
                        if (SelectedTenant.Code != "Other")
                        {
                            SetAcquirerCode(SelectedTenant.Code);
                        }
                        else
                        {

                            if (SelectedTenant.Code == "Other" && string.IsNullOrEmpty(OtherProviderName))
                            {

                                App.Instance.Hud.DisplayToast("You must add a payment type before proceeding to use Hike POS");
                                return;
                            }
                            else
                            {
                                SetAcquirerCode(OtherProviderName);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(PosId) || string.IsNullOrWhiteSpace(PosId))
                    {
                        App.Instance.Hud.DisplayToast("Please enter POS ID");
                        return;
                    }
                    else
                    {

                        if (IsValidInput("PosId", PosId) || PosId.Length > 16)
                        {
                            App.Instance.Hud.DisplayToast("Please enter valid POS ID");
                            return;
                        }
                    }



                    if (string.IsNullOrEmpty(SerialNumber) || string.IsNullOrWhiteSpace(SerialNumber))
                    {
                        App.Instance.Hud.DisplayToast("Please enter EFTPOS Serial Number");
                        return;
                    }
                    else
                    {

                        if (!IsValidInput("SerialNumber", SerialNumber))
                        {
                            App.Instance.Hud.DisplayToast("Please enter valid SerialNumber");
                            return;
                        }
                    }


                    if (string.IsNullOrEmpty(EftposAddress) || string.IsNullOrWhiteSpace(EftposAddress))
                    {
                        App.Instance.Hud.DisplayToast("Please enter EFTPOS Address");
                        return;
                    }
                    else if (string.IsNullOrWhiteSpace(EftposAddress) || !validIpV4AddressRegex.IsMatch(EftposAddress.Trim()))
                    {
                        App.Instance.Hud.DisplayToast("Please enter valid EFTPOS Address");
                        return;
                    }

                    else
                    {
                        var deviceAddressService = new DeviceAddressService();
                        var response = await deviceAddressService.RetrieveDeviceAddress(SerialNumber, ServiceConfiguration.DeviceApiKey, IsOtherProvider ? OtherProviderName : SelectedTenant.Code, false);
                        if (response?.Data == null)
                        {
                            App.Instance.Hud.DisplayToast("Something wrong with provided credentials");
                            return;

                        }
                        Settings.SerialNumber = SerialNumber;
                        Settings.EFTPOSAddress = EftposAddress;
                        //_spi.SetPosId(ConfigurationModel.PosId);
                        SPICommonViewModel._spi.SetEftposAddress(EftposAddress);
                        SPICommonViewModel._spi.Pair();
                        PairVisible = false;
                    }
                }
                else if (PairUnpairText == "UnPair")
                {
                    Status = "Unpairing...";
                    await Task.Delay(1000);
                    SPICommonViewModel._spi.Unpair();
                    PairUnpairText = "Pair";
                    Action1Visible = false;
                    Action2Visible = false;
                    Action3Visible = false;
                    IsUnpaired = true;


                    //#35436 iOS- mx51 Suggested changes
                    //ProcessStatus = "Should the EFTPOS terminal remain paired, press Enter +3 on the EFTPOS terminal to complete unpairing process.";
                    //Status = "Unpairing Successful!";
                    Status = "Successfully Unpaired!";
                    ProcessStatus = "Unpair from the terminal if it remains paired.";
                    ////#35436 iOS- mx51 Suggested changes

                    ConfirmationCode = "";
                    ProcessStatus2 = "";

                    ConfigurationSuccessed?.Invoke(this, null);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while pairing... : " + ex.ToString());
            }

        }

        void Action1()
        {
            if (Action1Text == "YES IT MATCHES")
            {
                SPICommonViewModel._spi.PairingConfirmCode();
                ConfirmationCode = string.Empty;
                ProcessStatus2 = string.Empty;
                ProcessStatus = string.Empty;
            }

            else if (Action1Text == "NO, IT DOES NOT")
            {
                SPICommonViewModel._spi.PairingCancel();
                // _frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action1Text == "Cancel Pairing")
            {
                SPICommonViewModel._spi.PairingCancel();
                // _frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action1Text == "Cancel")
            {
                SPICommonViewModel._spi.PairingCancel();
            }
            else if (Action1Text == "OK")
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                ProcessStatus = "";
                ConfirmationCode = "";
                ProcessStatus2 = "";
                PrintStatusAndActions();
                PairVisible = true;

            }
            else if (Action1Text == "Accept Signature")
            {
                SPICommonViewModel._spi.AcceptSignature(true);
            }
            else if (Action1Text == "Retry")
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                ProcessStatus = "";
                ConfirmationCode = "";
                ProcessStatus2 = "";
                if (SPICommonViewModel._spi.CurrentTxFlowState.Type == TransactionType.Purchase)
                {
                    //  Charge();
                }
                else
                {
                    //  Status = "Retry by selecting from the options";
                    PrintStatusAndActions();
                }
            }
        }

        public void Action2()
        {
            if (Action2Text == "Cancel Pairing")
            {
                SPICommonViewModel._spi.PairingCancel();
                //_frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action2Text == "NO, IT DOES NOT")
            {
                SPICommonViewModel._spi.PairingCancel();
                // _frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action2Text == "Decline Signature")
            {
                SPICommonViewModel._spi.AcceptSignature(false);
            }
            else if (Action2Text == "Cancel")
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                ProcessStatus = "";
                ConfirmationCode = "";
                ProcessStatus2 = "";
                PrintStatusAndActions();
            }
        }

        public void Action3()
        {
            if (Action3Text == "Cancel")
            {
                SPICommonViewModel._spi.PairingCancel();
                PrintStatusAndActions();
            }
        }

        #endregion

        #region Methods
        private void ChangeOtherTenent(TenantDetails tenantDetails)
        {

            if (tenantDetails.Code == "Other")
            {
                IsOtherProvider = true;
                SetAcquirerCode(OtherProviderName);
            }
            else
            {
                IsOtherProvider = false;
                SetAcquirerCode(SelectedTenant.Code);
            }
        }

        

        private void Start()
        {

            try
            {
                // This is where you load your state - like the pos_id, eftpos address and secrets - from your file system or database
                #region Spi Setup



                if (!string.IsNullOrEmpty(Settings.EFTPOSAddress))
                {
                    if (!string.IsNullOrEmpty(Settings.EFTPOSAddress) || !string.IsNullOrEmpty(Settings.SerialNumber))
                    {
                        EftposAddress = Settings.EFTPOSAddress;
                        SerialNumber = Settings.SerialNumber;
                        IsEftposAddressEnalble = false;


                    }
                    //else if (!string.IsNullOrEmpty(Settings.SerialNumber))//#35436 iOS- mx51 Suggested changes
                    //{
                    //    SerialNumber = Settings.SerialNumber;
                    //    IsEftposAddressEnalble = false;

                    //}
                    else
                    {
                        Status = "Unpaired";
                        IsEftposAddressEnalble = true;
                        //IsPairEnabled = true;
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(SerialNumber))
                        Settings.SerialNumber = SerialNumber;
                    else
                        return;

                    if (!string.IsNullOrEmpty(EftposAddress))
                        Settings.EFTPOSAddress = EftposAddress;
                    else
                        return;
                }


                if (!string.IsNullOrEmpty(Settings.APEncKey) && !string.IsNullOrEmpty(Settings.APHmacKey))
                {
                    SPICommonViewModel._spiSecrets = new Secrets(Settings.APEncKey, Settings.APHmacKey);
                }


                
                // This is how you instantiate Spi.

                SPICommonViewModel._spi = new Spi(PosId + Settings.CurrentRegister.Id, Settings.SerialNumber, Settings.EFTPOSAddress, SPICommonViewModel._spiSecrets); // It is ok to not have the secrets yet to start with.
                SPICommonViewModel._spi.SetPosInfo(PosId, "1.0");
                SPICommonViewModel._spi.SetDeviceApiKey(ServiceConfiguration.DeviceApiKey);
                SPICommonViewModel._spi.SetAutoAddressResolution(true);
                var result = SPICommonViewModel._spi.SetTenantCode(Settings.AcquirerCode);
                //SPICommonViewModel._spi.SetTestMode(IsTestMode);
                //SPICommonViewModel._spi.DeviceAddressChanged -= DeviceAddressChanged;
                /* SPICommonViewModel._spi.SetPosInfo("HIKEPOS" + Settings.CurrentRegister.Id,"1.0");*/
                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress
               // SPICommonViewModel._spi.DeviceAddressChanged -= DeviceAddressChanged1;

                SPICommonViewModel._spi.StatusChanged += OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress
                //SPICommonViewModel._spi.DeviceAddressChanged += DeviceAddressChanged;
                SPICommonViewModel._spi.Start();
                var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                docFolder = System.IO.Path.Combine(docFolder, "SpiLog");
                if (!Directory.Exists(docFolder))
                {
                    Directory.CreateDirectory(docFolder);

                }
                var path = System.IO.Path.Combine(docFolder, "spiLog.log");
                Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                    .WriteTo.Logger(config => config
                        .WriteTo.File(path, rollingInterval: RollingInterval.Day,retainedFileCountLimit:5))
                    .CreateLogger();



                #endregion

                PrintStatusAndActions();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void PrintStatusAndActions()
        {
            //lblStatus.Text = _spi.CurrentStatus + ":" + _spi.CurrentFlow;

            switch (SPICommonViewModel._spi.CurrentStatus)
            {
                case SpiStatus.Unpaired:
                    switch (SPICommonViewModel._spi.CurrentFlow)
                    {
                        case SpiFlow.Idle:
                            Status = "Unpaired";

                            PairUnpairText = "Pair";
                            PairVisible = true;
                            IsUnpaired = true;
                            ChargeVisible = false;
                            IsEftposAddressEnalble = true;
                            //Action1Visible = false;
                            //Action2Visible = false;
                            //Action3Visible = false;

                            //#23024 iOS - Westpac Acquirer


                            if (TenantsList == null)
                            {
                                this.GetAvailableTenants(PosId, ServiceConfiguration.DeviceApiKey, Settings.CountryCode);
                            }

                            Settings.AcquirerName = string.Empty;
                            Settings.AcquirerCode = string.Empty;

                            SelectedProviderName = string.Empty;
                            IsProviderAvailable = false;
                            //#23024 iOS - Westpac Acquirer



                            break;

                        case SpiFlow.Pairing:
                            if (SPICommonViewModel._spi.CurrentPairingFlowState.AwaitingCheckFromPos)
                            {
                                Action1Visible = true;
                                Action1Text = "YES IT MATCHES";
                                Action2Visible = true;
                                Action2Text = "NO, IT DOES NOT";
                                Action3Visible = false;
                                ChargeVisible = false;
                                IsUnpaired = false;
                            }
                            else if (!SPICommonViewModel._spi.CurrentPairingFlowState.Finished)
                            {
                                Action1Visible = true;
                                Action1Text = "Cancel";
                                Action2Visible = false;
                                Action3Visible = false;
                                ChargeVisible = false;
                                IsEftposAddressEnalble = false;
                                IsUnpaired = false;
                            }
                            else
                            {
                                Action1Visible = true;
                                Action1Text = "OK";
                                Action2Visible = false;
                                Action3Visible = false;
                                ChargeVisible = false;
                                IsUnpaired = false;
                                IsEftposAddressEnalble = true;

                            }

                            break;

                        case SpiFlow.Transaction:
                            break;

                        default:
                            Action1Visible = true;
                            Action1Text = "OK";
                            Action2Visible = false;
                            Action3Visible = false;
                            ChargeVisible = false;
                            IsUnpaired = false;
                            ProcessStatus = "# .. Unexpected Flow .. " + SPICommonViewModel._spi.CurrentFlow;
                            ConfirmationCode = "";
                            ProcessStatus2 = "";
                            break;
                    }
                    break;

                case SpiStatus.PairedConnecting:
                    PairUnpairText = "UnPair";
                    IsUnpaired = true;
                    Action1Visible = false;
                    Action2Visible = false;
                    Action3Visible = false;
                    ChargeVisible = false;
                    Status = "Paired But still trying to connect";
                    ProcessStatus = "(Check network and EFTPOS IP Address)";
                    IsEftposAddressEnalble = true;

                    break;

                case SpiStatus.PairedConnected:
                    switch (SPICommonViewModel._spi.CurrentFlow)
                    {
                        case SpiFlow.Idle:
                            {
                                PairUnpairText = "UnPair";
                                Action1Visible = false;
                                Action2Visible = false;
                                Action3Visible = false;
                                IsUnpaired = true;
                                Status = "Paired and connected";
                                IsEftposAddressEnalble = false;
                                //pnlActions.Visible = true;
                                //lblStatus.BackColor = Color.Green
                                ChargeVisible = true;


                              

                                ConfigurationSuccessed?.Invoke(this, new AssemblyPaymentConfigurationDto()
                                {
                                    PosId = PosId,
                                    EftposAddress = EftposAddress,
                                    EncKey = PosId,
                                    HmacKey = PosId,
                                    ReceiptFromEFTPOS = AllowPrintOnEFTPOS,
                                    AcquirerName = Settings.AcquirerName,// SelectedTenant.Name,
                                    AcquirerCode = Settings.AcquirerCode//SelectedTenant.Code,

                                });

                                if (SelectedTenant != null)
                                {
                                    Settings.AcquirerName = SelectedTenant.Name;
                                    Settings.AcquirerCode = SelectedTenant.Code;
                                }


                                SelectedProviderName = Settings.AcquirerName;
                                IsProviderAvailable = true;

                            }
                            break;

                        case SpiFlow.Transaction:
                            if (SPICommonViewModel._spi.CurrentTxFlowState.AwaitingSignatureCheck)
                            {
                                Action1Visible = true;
                                Action1Text = "Accept Signature";
                                Action2Visible = true;
                                Action2Text = "Decline Signature";
                                Action3Visible = true;
                                Action3Text = "Cancel";
                                ChargeVisible = false;
                                IsUnpaired = false;
                            }
                            else if (!SPICommonViewModel._spi.CurrentTxFlowState.Finished)
                            {
                                Action1Visible = true;
                                Action1Text = "Cancel";
                                Action2Visible = false;
                                Action3Visible = false;
                                ChargeVisible = false;
                                IsUnpaired = false;
                            }
                            else
                            {
                                switch (SPICommonViewModel._spi.CurrentTxFlowState.Success)
                                {
                                    case SPIClient.Message.SuccessState.Success:
                                        Action1Visible = true;
                                        Action1Text = "OK";
                                        Action2Visible = false;
                                        Action3Visible = false;
                                        ChargeVisible = false;
                                        IsUnpaired = false;
                                        break;

                                    case SPIClient.Message.SuccessState.Failed:
                                        Action1Visible = true;
                                        Action1Text = "Retry";
                                        Action2Visible = true;
                                        Action2Text = "Cancel";
                                        Action3Visible = false;
                                        ChargeVisible = false;
                                        IsUnpaired = false;
                                        break;

                                    default:
                                        Action1Visible = true;
                                        Action1Text = "OK";
                                        Action2Visible = false;
                                        Action3Visible = false;
                                        ChargeVisible = false;
                                        IsUnpaired = false;
                                        break;
                                }
                            }
                            break;

                        case SpiFlow.Pairing:
                            Action1Visible = true;
                            Action1Text = "OK";
                            Action2Visible = false;
                            Action3Visible = false;
                            ChargeVisible = false;
                            IsUnpaired = false;
                            break;

                        default:
                            Action1Visible = true;
                            Action1Text = "OK";
                            Action2Visible = false;
                            Action3Visible = false;
                            ChargeVisible = false;
                            ProcessStatus = "# .. Unexpected Flow .. " + SPICommonViewModel._spi.CurrentFlow;
                            ConfirmationCode = "";
                            ProcessStatus2 = "";
                            IsUnpaired = false;
                            break;
                    }
                    break;

                default:
                    Action1Visible = true;
                    Action1Text = "OK";
                    Action2Visible = false;
                    Action3Visible = false;
                    ChargeVisible = false;
                    ProcessStatus = "# .. Unexpected Flow .. " + SPICommonViewModel._spi.CurrentFlow;
                    ConfirmationCode = "";
                    ProcessStatus2 = "";
                    IsUnpaired = false;
                    break;
            }
        }


        private void OnStatusChanged1(object sender, SpiStatusEventArgs spiStatus)
        {
        }
        private void OnStatusChanged(object sender, SpiStatusEventArgs spiStatus)
        {

            if (SPICommonViewModel._spi.CurrentFlow == SpiFlow.Idle)
            {
                ProcessStatus = "";
                ConfirmationCode = "";
                ProcessStatus2 = "";
            }
            PrintStatusAndActions();
        }

        private void OnPairingFlowStateChanged1(object sender, PairingFlowState pairingFlowState)
        {
        }


        private void OnPairingFlowStateChanged(object sender, PairingFlowState pairingFlowState)
        {
            ProcessStatus = "";
            ConfirmationCode = "";
            ProcessStatus2 = "";
            Status = pairingFlowState.Message;
            Debug.WriteLine("Status ----- " + pairingFlowState.Message.ToString());

            if (pairingFlowState.ConfirmationCode != "" && pairingFlowState.Message != "Pairing Successful!" && pairingFlowState.Message != "Pairing Failed")
            {
                ProcessStatus = "MATCH THE FOLLOWING CODE WITH THEEFTPOS.";
                ConfirmationCode = pairingFlowState.ConfirmationCode;
                ProcessStatus2 = "DOES CODE MATCH?";
                Status = "";
            }

            PrintStatusAndActions();
            
        }

        private void OnTxFlowStateChanged1(object sender, TransactionFlowState txFlowState)
        {

        }
        private void OnTxFlowStateChanged(object sender, TransactionFlowState txFlowState)
        {
            
            ProcessStatus = "";
            ConfirmationCode = "";
            ProcessStatus2 = "";
            Status = txFlowState.DisplayMessage;
            //ProcessStatus = " # Id: " + txFlowState.Id;
            //ProcessStatus = ProcessStatus + "\n # Type: " + txFlowState.Type;
            //ProcessStatus = ProcessStatus + "\n # RequestSent: " + txFlowState.RequestSent;
            //ProcessStatus = ProcessStatus + "\n # WaitingForSignature: " + txFlowState.AwaitingSignatureCheck;
            //ProcessStatus = ProcessStatus + "\n # Attempting to Cancel : " + txFlowState.AttemptingToCancel;
            //ProcessStatus = ProcessStatus + "\n # Finished: " + txFlowState.Finished;
            //ProcessStatus = ProcessStatus + "\n # Outcome: " + txFlowState.Success;
            //ProcessStatus = ProcessStatus + "\n # Display Message: " + txFlowState.DisplayMessage;

            if (txFlowState.AwaitingSignatureCheck && ConfigurationModel != null && !ConfigurationModel.ReceiptFromEFTPOS)
            {
                //We need to print the receipt for the customer to sign.
                var data = txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                var AvailablePrinter = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                if (AvailablePrinter != null && AvailablePrinter.Count() > 0)
                {
                    WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = new List<string>() { data }, OnlyAssemblyPayment = true }));
                }
            }

            //If the transaction is finished, we take some extra steps.
            if (txFlowState.Finished)
            {
                if (txFlowState.Success == SPIClient.Message.SuccessState.Unknown)
                {
                    //TH-4T, TH-4N, TH-2T - This is the dge case when we can't be sure what happened to the transaction.
                    //Invite the merchant to look at the last transaction on the EFTPOS using the dicumented shortcuts.
                    //Now offer your merchant user the options to:
                    //A. Retry the transaction from scratch or pay using a different method - If Merchant is confident that tx didn't go through.
                    //B. Override Order as Paid in you POS - If Merchant is confident that payment went through.
                    //C. Cancel out of the order all together - If the customer has left / given up without paying
                    //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                    ProcessStatus = ProcessStatus + "\n Not sure if we got paid or not. Check last transaction manually on EFTPOS!";
                    ConfirmationCode = "";
                    ProcessStatus2 = "";
                    //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                }
                else
                {
                    //We have a result...
                    switch (txFlowState.Type)
                    {
                        //Depending on what type of transaction it was, we might act diffeently or use different data.
                        case TransactionType.Purchase:
                            if (txFlowState.Success == SPIClient.Message.SuccessState.Success)
                            {
                                //TH-6A
                                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                                ProcessStatus = ProcessStatus + "\n Hooray! We got paid. Close the order!";
                                ConfirmationCode = "";
                                ProcessStatus2 = "";
                                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                            }
                            else
                            {
                                //TH-6E
                                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                                ProcessStatus = ProcessStatus + "\n We didn't get paid. Retry payment or give up!";
                                ConfirmationCode = "";
                                ProcessStatus2 = "";
                                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                            }

                            if (txFlowState.Response != null)
                            {
                                var purchaseResponse = new PurchaseResponse(txFlowState.Response);

                                if (purchaseResponse != null)
                                {
                                    //ProcessStatus = ProcessStatus + "\n # Scheme: " + purchaseResponse.SchemeName;
                                    //ProcessStatus = ProcessStatus + "\n # Response: " + purchaseResponse.GetResponseText();
                                    //ProcessStatus = ProcessStatus + "\n # RRN: " + purchaseResponse.GetRRN();
                                    //ProcessStatus = ProcessStatus + "\n # Customer Receipt:";

                                    if (purchaseResponse.Success)
                                    {
                                        /*
                                         _spi.AckFlowEndedAndBackToIdle();
                                         PaymentSuccessed?.Invoke(this, new AssemblyPaymentResponse()
                                         {
                                             Success = purchaseResponse.Success,
                                             RequestId = purchaseResponse.RequestId,
                                             PosRefId = purchaseResponse.PosRefId,
                                             SchemeName = purchaseResponse.SchemeName,
                                             SchemeAppName = purchaseResponse.SchemeAppName,
                                             RRN = purchaseResponse.GetRRN(),
                                             PurchaseAmount = purchaseResponse.GetPurchaseAmount(),
                                             TipAmount = purchaseResponse.GetTipAmount(),
                                             CashoutAmount = purchaseResponse.GetCashoutAmount(),
                                             BankNonCashAmount = purchaseResponse.GetBankNonCashAmount(),
                                             BankCashAmount = purchaseResponse.GetBankCashAmount(),
                                             CustomerReceipt = purchaseResponse.GetCustomerReceipt(),
                                             MerchantReceipt = purchaseResponse.GetMerchantReceipt(),
                                             ResponseText = purchaseResponse.GetResponseText(),
                                             ResponseCode = purchaseResponse.GetResponseCode(),
                                             TerminalReferenceId = purchaseResponse.GetTerminalId(),
                                             CardEntry = purchaseResponse.GetCardEntry(),
                                             AccountType = purchaseResponse.GetAccountType(),
                                             AuthCode = purchaseResponse.GetAuthCode(),
                                             BankDate = purchaseResponse.GetBankDate(),
                                             BankTime = purchaseResponse.GetBankTime(),
                                             MaskedPan = purchaseResponse.GetMaskedPan(),
                                             TerminalId = purchaseResponse.GetTerminalId(),
                                             MerchantReceiptPrinted = purchaseResponse.WasMerchantReceiptPrinted(),
                                             CustomerReceiptPrinted = purchaseResponse.WasCustomerReceiptPrinted(),
                                             SettlementDate = purchaseResponse.GetSettlementDate()
                                         });

                                         */
                                    }
                                }
                                ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();

                                ConfirmationCode = "";
                                ProcessStatus2 = "";
                                //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + purchaseResponse.GetCustomerReceipt().TrimEnd();

                                if (ConfigurationModel != null && !ConfigurationModel.ReceiptFromEFTPOS)
                                {
                                    //We need to print the receipt for the customer to sign.
                                    var data = purchaseResponse.GetCustomerReceipt().TrimEnd();
                                    //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                                    var AvailablePrinter = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                                    if (AvailablePrinter != null && AvailablePrinter.Count() > 0)
                                    {
                                        WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = new List<string>() { data }, OnlyAssemblyPayment = true }));
                                    }
                                }


                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;

                        case TransactionType.Refund:
                            if (txFlowState.Response != null)
                            {
                                var refundResponse = new RefundResponse(txFlowState.Response);
                                if (refundResponse != null)
                                {
                                    //ProcessStatus = ProcessStatus + "\n # Scheme: " + refundResponse.SchemeName;
                                    //ProcessStatus = ProcessStatus + "\n # Response: " + refundResponse.GetResponseText();
                                    //ProcessStatus = ProcessStatus + "\n # RRN: " + refundResponse.GetRRN();
                                    //ProcessStatus = ProcessStatus + "\n # Customer Receipt:";

                                    if (refundResponse.Success)
                                    {
                                        /*
                                        _spi.AckFlowEndedAndBackToIdle();
                                        PaymentSuccessed?.Invoke(this, new AssemblyPaymentResponse()
                                        {
                                            Success = refundResponse.Success,
                                            RequestId = refundResponse.RequestId,
                                            PosRefId = refundResponse.PosRefId,
                                            SchemeName = refundResponse.SchemeName,
                                            SchemeAppName = refundResponse.SchemeAppName,
                                            RRN = refundResponse.GetRRN(),
                                            PurchaseAmount = refundResponse.GetRefundAmount(),
                                            TipAmount = 0,
                                            CashoutAmount = 0,
                                            BankNonCashAmount = 0,
                                            BankCashAmount = 0,
                                            CustomerReceipt = refundResponse.GetCustomerReceipt(),
                                            MerchantReceipt = refundResponse.GetMerchantReceipt(),
                                            ResponseText = refundResponse.GetResponseText(),
                                            ResponseCode = refundResponse.GetResponseCode(),
                                            TerminalReferenceId = refundResponse.GetTerminalId(),
                                            CardEntry = refundResponse.GetCardEntry(),
                                            AccountType = refundResponse.GetAccountType(),
                                            AuthCode = refundResponse.GetAuthCode(),
                                            BankDate = refundResponse.GetBankDate(),
                                            BankTime = refundResponse.GetBankTime(),
                                            MaskedPan = refundResponse.GetMaskedPan(),
                                            TerminalId = refundResponse.GetTerminalId(),
                                            MerchantReceiptPrinted = refundResponse.WasMerchantReceiptPrinted(),
                                            CustomerReceiptPrinted = refundResponse.WasCustomerReceiptPrinted(),
                                            SettlementDate = refundResponse.GetSettlementDate()
                                        });
                                        */
                                    }


                                }
                                ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();

                                ConfirmationCode = "";
                                ProcessStatus2 = "";

                                //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + refundResponse.GetCustomerReceipt().TrimEnd();
                                if (ConfigurationModel != null && !ConfigurationModel.ReceiptFromEFTPOS)
                                {
                                    //We need to print the receipt for the customer to sign.
                                    var data = refundResponse.GetCustomerReceipt().TrimEnd();
                                    //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                                    var AvailablePrinter = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                                    if (AvailablePrinter != null && AvailablePrinter.Count() > 0)
                                    {
                                        WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = new List<string>() { data }, OnlyAssemblyPayment = true }));
                                    }
                                }
                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;

                        case TransactionType.Settle:
                            if (txFlowState.Response != null)
                            {
                                var settleResponse = new Settlement(txFlowState.Response);
                                //ProcessStatus = ProcessStatus + "\n # Response: " + settleResponse.GetResponseText();
                                ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();

                                ConfirmationCode = "";
                                ProcessStatus2 = "";
                                //ProcessStatus = ProcessStatus + "\n # Merchant Receipt:";
                                //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + settleResponse.GetReceipt().TrimEnd();
                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;

                        case TransactionType.GetLastTransaction:
                            if (txFlowState.Response != null)
                            {
                                var gltResponse = new GetLastTransactionResponse(GetTxFlowState(txFlowState).Response);
                                //ProcessStatus = ProcessStatus + "\n # Checking to see if it matches the $100.00 purchase we did 1 minute ago :)";
                                //                       var success = this._spi.GltMatch(gltResponse, TransactionType.Purchase, 10000, DateTime.Now.Subtract(TimeSpan.FromMinutes(1)), "MYORDER123");

                                //                       if (success == SPIClient.Message.SuccessState.Unknown)
                                //                       {
                                //ProcessStatus = ProcessStatus + "\n # Did not retrieve Expected Transaction.";
                                //                       }
                                //                       else
                                //                       {
                                //                           ProcessStatus = ProcessStatus + "\n # Tx Matched Expected Purchase Request.";
                                //                           ProcessStatus = ProcessStatus + "\n # Result: " + success;

                                //                           var purchaseResponse = new PurchaseResponse(txFlowState.Response);
                                //ProcessStatus = ProcessStatus + "\n # Scheme: " + purchaseResponse.SchemeName;
                                //    ProcessStatus = ProcessStatus + "\n # Response: " + purchaseResponse.GetResponseText();
                                //    ProcessStatus = ProcessStatus + "\n # RRN: " + purchaseResponse.GetRRN();
                                //    ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();
                                //    ProcessStatus = ProcessStatus + "\n # Customer Receipt:";
                                //    //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + purchaseResponse.GetMerchantReceipt().TrimEnd();
                                //}
                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;
                    }
                }
            }

            PrintStatusAndActions();
        }

        private static TransactionFlowState GetTxFlowState(TransactionFlowState txFlowState)
        {
            return txFlowState;
        }

        private void OnSecretsChanged1(object sender, Secrets newSecrets)
        {
        }
        private void OnSecretsChanged(object sender, Secrets newSecrets)
        {
            SPICommonViewModel._spiSecrets = newSecrets;
            if (newSecrets != null)
            {
                Settings.APEncKey = newSecrets.EncKey;
                Settings.APHmacKey = newSecrets.HmacKey;
            }
            else
            {
                Settings.APEncKey = string.Empty;
                Settings.APHmacKey = string.Empty;
            }
        }

       
        async void GetAvailableTenants(string posVendorId, string apiKey, string countryCode)
        {

            try
            {

                using (new Busy(this, true))
                {
                    //Ticket start:#32342 iOS - Westpac Tenant Keeps Changing.by rupesh

                    var tenants = await Spi.GetAvailableTenants(posVendorId, apiKey, countryCode);
                    ObservableCollection<TenantDetails> tempTenantsList = null;

                    //if (TenantsList != null)
                    //{
                    //    TenantsList = null;
                    //    //TenantsList.Clear();
                    //}

                    if (tenants.Data != null)
                    {


                        tempTenantsList = new ObservableCollection<TenantDetails>(tenants.Data);
                        tempTenantsList.Add(new TenantDetails { Code = "Other", Name = "Other" });
                        IsOtherProvider = false;

                    }
                    else
                    {
                        tempTenantsList = new ObservableCollection<TenantDetails>();
                        tempTenantsList.Add(new TenantDetails { Code = "Other", Name = "Other" });

                        IsOtherProvider = true;
                        SelectedTenant = tempTenantsList.FirstOrDefault();
                     

                    }
                    TenantsList = tempTenantsList;
                    // SelectedTenant = TenantsList.FirstOrDefault();
                    //Ticket end:#32342 .by rupesh


                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
        void SetAcquirerCode(string acquirerCode)
        {
            // acquirerCode = "wbc"; // For testing purpose other code is wbc


            if (SPICommonViewModel._spi != null)
            {
                bool isVaildCode = SPICommonViewModel._spi.SetAcquirerCode(acquirerCode);

            }

        //    SPICommonViewModel._spi.DeviceAddressChanged += DeviceAddressChanged;

            

        }

        //#38987 iOS : mx51 Feedback
        private bool IsValidInput( string type, string value)
        {
             Regex inputRegex = new Regex("[^A-Za-z0-9]");
            bool isvalid = true;
            //public Regex validIpV4AddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexOptions.IgnoreCase);
            //string pattern = @"(?<=[<](body|BODY|Body)[>].*)(?<!-)\b(TEST)\b(?!-)";

            if (type == "PosId")
                isvalid= Regex.IsMatch(value, "[^A-Za-z0-9]", RegexOptions.IgnoreCase);
            else if (type == "SerialNumber")
                isvalid =Regex.IsMatch(value, @"^[0-9-]*$", RegexOptions.IgnoreCase);
           

            return isvalid;
        }


        #endregion
    }


}
