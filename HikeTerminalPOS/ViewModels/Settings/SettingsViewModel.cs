using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Enum;
using HikePOS.Models.Payment;
using HikePOS.Services;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Pages.Settings;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;
using Newtonsoft.Json.Serialization;

namespace HikePOS.ViewModels
{

    public class SettingsViewModel : BaseViewModel
    {

        #region Declaration
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        paymentConfigurationPage configurationpage;
        HikePayConfigurationPage hikePayConfigurationPage;


        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;
        //Ticket Start #61832 iPad:Create text file for invoice log by: Pratik-
        ApiService<ISubmitLogApi> logApiService = new ApiService<ISubmitLogApi>();
        SubmitLogServices logService;
        //End #61832 Pratik

        IPrintService printService;


        #endregion

        #region properties


        bool _IsIntegratedPaymentAvailable { get; set; } = true;
        public bool IsIntegratedPaymentAvailable { get { return _IsIntegratedPaymentAvailable; } set { _IsIntegratedPaymentAvailable = value; SetPropertyChanged(nameof(IsIntegratedPaymentAvailable)); } }

        string _SelectedMenuItem { get; set; } = "GENERAL";
        public string SelectedMenuItem { get { return _SelectedMenuItem; } set { _SelectedMenuItem = value; SetPropertyChanged(nameof(SelectedMenuItem)); } }

        bool _ActiveMarkPaidOrderAsFulfilled { get; set; } = true;
        public bool ActiveMarkPaidOrderAsFulfilled { get { return _ActiveMarkPaidOrderAsFulfilled; } set { _ActiveMarkPaidOrderAsFulfilled = value; SetPropertyChanged(nameof(ActiveMarkPaidOrderAsFulfilled)); } }

        string _AutoLockDuration { get; set; }
        public string AutoLockDuration { get { return _AutoLockDuration; } set { _AutoLockDuration = value; SetPropertyChanged(nameof(AutoLockDuration)); } }

        ObservableCollection<AutoLockDurationModel> _AutoLockDurationList { get; set; }
        public ObservableCollection<AutoLockDurationModel> AutoLockDurationList { get { return _AutoLockDurationList; } set { _AutoLockDurationList = value; SetPropertyChanged(nameof(AutoLockDurationList)); } }

        bool _ActivePrinter { get; set; }
        public bool ActivePrinter { get { return _ActivePrinter; } set { _ActivePrinter = value; SetPropertyChanged(nameof(ActivePrinter)); } }

        bool _ActivecustomerDisplay { get; set; }
        public bool ActivecustomerDisplay
        {
            get
            {
                return _ActivecustomerDisplay;
            }
            set
            {
                _ActivecustomerDisplay = value;
                SetPropertyChanged(nameof(ActivecustomerDisplay));

            }
        }

        bool _paymentOptionBtnVisible;
        public bool PaymentOptionBtnVisible
        {
            get
            {
                return _paymentOptionBtnVisible;
            }
            set
            {
                _paymentOptionBtnVisible = value;

                SetPropertyChanged(nameof(PaymentOptionBtnVisible));

            }
        }

        double _paymentOptionHeightRequest;
        public double PaymentOptionHeightRequest
        {
            get
            {
                return _paymentOptionHeightRequest;
            }
            set
            {
                _paymentOptionHeightRequest = value;

                SetPropertyChanged(nameof(PaymentOptionHeightRequest));

            }
        }


        string _CustomerAppCode { get; set; }
        public string CustomerAppCode { get { return _CustomerAppCode; } set { _CustomerAppCode = value; SetPropertyChanged(nameof(CustomerAppCode)); } }


        private ObservableCollection<Printer> _PrinterList { get; set; }
        public ObservableCollection<Printer> PrinterList { get { return _PrinterList; } set { _PrinterList = value; SetPropertyChanged(nameof(PrinterList)); } }

        ObservableCollection<AutoLockDurationModel> _NoOfCopiesList { get; set; }
        public ObservableCollection<AutoLockDurationModel> NoOfCopiesList { get { return _NoOfCopiesList; } set { _NoOfCopiesList = value; SetPropertyChanged(nameof(NoOfCopiesList)); } }

        private Printer _SelectedPrinter { get; set; }
        public Printer SelectedPrinter { get { return _SelectedPrinter; } set { _SelectedPrinter = value; SetPropertyChanged(nameof(SelectedPrinter)); } }

        bool _ActiveCustomerQueueDocketPrint { get; set; } = false;
        public bool ActiveCustomerQueueDocketPrint { get { return _ActiveCustomerQueueDocketPrint; } set { _ActiveCustomerQueueDocketPrint = value; SetPropertyChanged(nameof(ActiveCustomerQueueDocketPrint)); } }


        ObservableCollection<AutoLockDurationModel> _DocketNumberRangeList { get; set; }
        public ObservableCollection<AutoLockDurationModel> DocketNumberRangeList { get { return _DocketNumberRangeList; } set { _DocketNumberRangeList = value; SetPropertyChanged(nameof(DocketNumberRangeList)); } }

        int _DocketNumberRange { get; set; }
        public int DocketNumberRange { get { return _DocketNumberRange; } set { _DocketNumberRange = value; SetPropertyChanged(nameof(DocketNumberRange)); } }

        bool _ActiveAllCoudReceipt { get; set; }
        public bool ActiveAllCoudReceipt { get { return _ActiveAllCoudReceipt; } set { _ActiveAllCoudReceipt = value; SetPropertyChanged(nameof(ActiveAllCoudReceipt)); } }

        ObservableCollection<PaymentOptionDto> _PaymentOptionList { get; set; } = new ObservableCollection<PaymentOptionDto>();
        public ObservableCollection<PaymentOptionDto> PaymentOptionList { get { return _PaymentOptionList; } set { _PaymentOptionList = value; SetPropertyChanged(nameof(PaymentOptionList)); } }

        ObservableCollection<PaymentOptionDto> _IntegratedPaymentOptionList { get; set; } = new ObservableCollection<PaymentOptionDto>();
        public ObservableCollection<PaymentOptionDto> IntegratedPaymentOptionList { get { return _IntegratedPaymentOptionList; } set { _IntegratedPaymentOptionList = value; SetPropertyChanged(nameof(IntegratedPaymentOptionList)); } }

        public IEnumerable<PaymentOptionDto> All_PaymentOptionList { get; set; }

        public string AppVesion => "Version: " + AppInfo.Current.VersionString;
        public PaymentOptionDto CurrentConfigurePaymentOption;

        #endregion

        #region Life Cycle
        public SettingsViewModel()
        {

            Title = "Settings";
            paymentService = new PaymentServices(paymentApiService);
            //Ticket Start #61832 iPad:Create text file for invoice log by: Pratik
            logService = new SubmitLogServices(logApiService);
            //End #61832 Pratik
            MenuItemSelectedCommand = new Command<string>(MenuItemSelected);
            PrinterList = new ObservableCollection<Printer>();
            AutoLockDurationList = new ObservableCollection<AutoLockDurationModel>() {
                new AutoLockDurationModel(){ Title="Never", Value = 0, IsSelected = true},
                new AutoLockDurationModel(){ Title="After 2 Minutes", Value = 2, IsSelected = false},
                new AutoLockDurationModel(){ Title="After 4 Minutes", Value = 4, IsSelected = false},
                new AutoLockDurationModel(){ Title="After 6 Minutes", Value = 6, IsSelected = false},
                new AutoLockDurationModel(){ Title="After 8 Minutes", Value = 8, IsSelected = false},
                new AutoLockDurationModel(){ Title="After 10 Minutes", Value = 10, IsSelected = false, IsNotLast = false},
            };

            NoOfCopiesList = new ObservableCollection<AutoLockDurationModel>() {
                new AutoLockDurationModel(){ Title="1 copy", Value = 1, IsSelected = true},
                new AutoLockDurationModel(){ Title="2 copy", Value = 2, IsSelected = false},
                new AutoLockDurationModel(){ Title="3 copy", Value = 3, IsSelected = false},
                new AutoLockDurationModel(){ Title="4 copy", Value = 4, IsSelected = false},
                new AutoLockDurationModel(){ Title="5 copy", Value = 5, IsSelected = false, IsNotLast = false}
            };

            DocketNumberRangeList = new ObservableCollection<AutoLockDurationModel>() {
                new AutoLockDurationModel(){ Title="1 to 100", Value = 100, IsSelected = true},
                new AutoLockDurationModel(){ Title="1 to 200", Value = 200, IsSelected = false},
                new AutoLockDurationModel(){ Title="1 to 300", Value = 300, IsSelected = false},
                new AutoLockDurationModel(){ Title="1 to 400", Value = 400, IsSelected = false},
                new AutoLockDurationModel(){ Title="1 to 500", Value = 500, IsSelected = false, IsNotLast = false}
            };

            PaymentActiveChangedCommand = new Command<PaymentOptionDto>(async (obj) => await PaymentActiveChanged_Call(obj));
            ConfigurePaymentCommand = new Command<PaymentOptionDto>(OpenPaymentConfigurationView);

            PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(PaymentOptionList))
                {
                    try
                    {
                        if (PaymentOptionList != null)
                        {
                            PaymentOptionHeightRequest = (PaymentOptionList.Count <= 4 ? PaymentOptionList.Count : 4) * 70;
                        }
                        else
                        {
                            PaymentOptionHeightRequest = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                }
            };
            ActivePrinter = Settings.GetCachePrinters.Any();
            ActiveAllCoudReceipt = Settings.IsAllReceiptRegisterActive;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.Settings.PaymentTypes") > 0))
                    {
                        PaymentOptionBtnVisible = true;
                    }
                    else
                    {
                        PaymentOptionBtnVisible = false;
                        if (SelectedMenuItem == "PAYMENT OPTIONS")
                        {
                            MenuItemSelected("PRINTER");
                        }
                    }

                    UpdateAutoLockDurationByValue(Settings.AutoLockDelay);

                    if (Settings.StoreGeneralRule != null)
                        ActiveMarkPaidOrderAsFulfilled = !Settings.StoreGeneralRule.ParkingPaidOrder;

                    ActiveCustomerQueueDocketPrint = Settings.PrintCustomerNumberReceipt;
                    DocketNumberRange = Settings.PrintCustomerEndingNumber;

                    PrinterList = Settings.GetCachePrinters;
                    if (!ActivePrinter)
                    {
                        ActivePrinter = PrinterList.Any();
                    }

                    if (!HasInitialized)
                    {
                        HasInitialized = true;
                        OnLoaded();
                    }
                    IsPaymentClicked = false;
                });
                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.AllReceiptRegisteredMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.AllReceiptRegisteredMessenger>(this, (sender, arg) =>
                    {
                        ActiveAllCoudReceipt = Settings.IsAllReceiptRegisterActive;
                    });
                }

                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.EpsonPrinterFindMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.EpsonPrinterFindMessenger>(this, (sender, arg) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            //Ticket start:#46678 Hike on iPad crashes when trying to add printer.by rupesh
                            try
                            {
                                if (arg?.Value != null)
                                {
                                    if (PrinterList == null)
                                        PrinterList = new ObservableCollection<Printer>();
                                    var AllPrinter = PrinterList.Copy();

                                    if (!AllPrinter.Any(x => x.ModelName == arg.Value.ModelName && x.Port == arg.Value.Port))
                                    {
                                        AllPrinter.Add(arg.Value);
                                        PrinterList = AllPrinter;
                                        Settings.GetCachePrinters = PrinterList;

                                    }
                                    AllPrinter = null;
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                            //Ticket end:#46678 .by rupesh

                        });
                    });
                }

                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.PosDeviceStatusMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.PosDeviceStatusMessenger>(this, (sender, arg) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast(arg.Value);
                        });
                    });
                }
            });
        }


        public override void OnDisappearing()
        {
            base.OnDisappearing();
            // BackCommandEvent();
            WeakReferenceMessenger.Default.Unregister<Messenger.AllReceiptRegisteredMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.EpsonPrinterFindMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.PosDeviceStatusMessenger>(this);
        }
        #endregion

        #region commands
        public ICommand MenuItemSelectedCommand { get; }
        ICommand _PaymentActiveChangedCommand { get; set; }
        public ICommand PaymentActiveChangedCommand { get { return _PaymentActiveChangedCommand; } set { _PaymentActiveChangedCommand = value; SetPropertyChanged(nameof(PaymentActiveChangedCommand)); } }
        public ICommand ConfigurePaymentCommand { get; }
        public ICommand ChangeAutoLockDurationTappedCommand { get; }
        public ICommand ExportLogCommand => new Command<View>(ExportLog);
        public ICommand ShareLogCommand => new Command<View>(ShareLog);
        public ICommand SliderMenuCommand => new Command(SliderMenuTapped);
        public ICommand BackHandleCommand => new Command(BackHandleTapped);
        public ICommand PaymentDescriptionLinkCommand => new Command(PaymentDescriptionLink);
        public ICommand PrinterDescriptionLinkCommand => new Command(PrinterDescriptionLink);
        public ICommand AllReceiptDescriptionLinkCommand => new Command(AllReceiptDescriptionLink);
        public ICommand ClearAndSyncDataCommand => new Command(ClearAndSyncDataTapped);
        public ICommand SyncDataCommand => new Command(SyncDataTapped);
        public ICommand MarkAsPaidSwitchToggleCommand => new Command(MarkAsPaidSwitchToggle);
        public ICommand ActiveCustomerQueueDocketPrintCommand => new Command(ActiveCustomerQueueDocketPrintToggled);
        public ICommand ActivePrinterToggleCommand => new Command(ActivePrinterToggle);
        public ICommand UpdatePrinterToggledCommand => new Command(UpdatePrinterToggled);
        // public ICommand DisplayAppSwitchCommand { get; set; }

        #endregion

        #region Command Execution

        private void UpdatePrinterToggled(object obj)
        {
            UpdateSelectedPrinter();
        }

        private async void ActivePrinterToggle(object obj)
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });

            try
            {
                if (ActivePrinter && !Settings.GetCachePrinters.Any())
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        var permmition = await CheckAndRequestCameraPermission();
                        if (permmition != PermissionStatus.Denied)
                        {
                            await FindPrinter();
                        }
                        else
                        {
                            var ans = await Permissions.RequestAsync<Permissions.Bluetooth>();
                            if (ans == PermissionStatus.Granted)
                            {
                                await FindPrinter();
                            }
                        }

                    }
                    else
                    {
                        await FindPrinter();
                    }
                }
                else if (!ActivePrinter)
                {
                    Settings.GetCachePrinters = new ObservableCollection<Printer>();
                }

                WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("Printer"));
            }
            catch (Exception ex)
            {
                ex.Track();
                IsLoad = false;
            }

        }


        private void SliderMenuTapped()
        {
            try
            {
                Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
                //_navigationService.MainPage.IsPresented = !_navigationService.MainPage.IsPresented;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void BackHandleTapped()
        {
            try
            {

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        #region GeneralTab

        private async void ClearAndSyncDataTapped()
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {              
                //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
                App.Instance.Issync = true;
                //End Ticket #72507 By: Pratik
                await ClearAndSyncData();

                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("DataSyncSuccess"), Colors.Green, Colors.White);
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }

        private async void SyncDataTapped()
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {               
                //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
                App.Instance.Issync = true;
                //End Ticket #72507 By: Pratik
                await SyncData();

                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("DataSyncSuccess"), Colors.Green, Colors.White);
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }

        private void MarkAsPaidSwitchToggle(object obj)
        {
            var tmpStoreGeneralRule = Settings.StoreGeneralRule;
            if (tmpStoreGeneralRule != null)
            {
                tmpStoreGeneralRule.ParkingPaidOrder = !ActiveMarkPaidOrderAsFulfilled;
                Settings.StoreGeneralRule = tmpStoreGeneralRule;
            }
        }
        #endregion

        #region PrinterTab
        private void PaymentDescriptionLink()
        {
            Browser.OpenAsync(new Uri(ServiceConfiguration.PaymentDescriptionLink));
        }

        private void PrinterDescriptionLink()
        {
            Browser.OpenAsync(new Uri(ServiceConfiguration.PrinterDescriptionLink));
        }

        private void AllReceiptDescriptionLink()
        {
            Browser.OpenAsync(new Uri(ServiceConfiguration.AllReceiptDescriptionLink));
        }

        private void ActiveCustomerQueueDocketPrintToggled()
        {
            Settings.PrintCustomerNumberReceipt = ActiveCustomerQueueDocketPrint;

        }

        #endregion

        private async void ExportLog(View element)
        {
            using (new Busy(this, true))
            {
                try
                {
                    var filename = $"Salelog_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.zip";
                    var folderpath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "salelog");
                    if (Directory.Exists(folderpath))
                    {
                        var data = Logger.CompressAndgetSteam(folderpath, filename);
                        await logService.UpalodZipFile(Fusillade.Priority.UserInitiated, data, filename);
                    }
                    else
                    { 
                        App.Instance.Hud.DisplayToast("The directory does not contain any sale logs.", Colors.Red, Colors.White);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }

        private async void ShareLog(View element)
        {
            try
            {
                if (element == null)
                    return;
                var bounds = element.GetAbsoluteBounds();
                var folderpath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "salelog");
                await Logger.CompressAndExportFolder(folderpath, bounds, $"Salelog_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.zip");
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        #endregion


        void InitializeConfigurationpage()
        {

            if (configurationpage == null)
            {
                configurationpage = new paymentConfigurationPage();

                configurationpage.ViewModel.ConfigurationSuccessed += async (object sender, PaymentConfigurationModel e) =>
                {
                    if (e != null && CurrentConfigurePaymentOption != null)
                    {
                        if (CurrentConfigurePaymentOption.PaymentOptionType == e.Type && string.IsNullOrEmpty(e.AccessToken))
                        {
                            CurrentConfigurePaymentOption.IsConfigered = false;
                            CurrentConfigurePaymentOption.ConfigurationDetails = null;
                        }
                        else
                        {

                            if (e.Type == PaymentOptionType.Tyro)
                            {

                                TyroConfigurationDto tyroDetails = new TyroConfigurationDto()
                                {
                                    AccessToken = e.AccessToken,
                                    TerminalId = e.TerminalId,
                                    MerchantId = e.MerchantId,
                                };
                                CurrentConfigurePaymentOption.ConfigurationDetails = JsonConvert.SerializeObject(tyroDetails);
                            }
                            else
                            {
                                PaypalConfigurationDto paypalConfiguration = new PaypalConfigurationDto()
                                {
                                    AccessToken = e.AccessToken,
                                    RefreshUrl = e.RefreshUrl
                                };
                                CurrentConfigurePaymentOption.ConfigurationDetails = JsonConvert.SerializeObject(paypalConfiguration);
                            }

                            CurrentConfigurePaymentOption.IsConfigered = true;
                        }
                        await PaymentActiveChanged(CurrentConfigurePaymentOption);
                    }
                    await configurationpage.ViewModel.Close();
                };
            }

        }

        #region GeneralTab
        public void MenuItemSelected(string selectedItem)
        {
            if (SelectedMenuItem == selectedItem)
            {
                return;
            }
            SelectedMenuItem = selectedItem;

            if (selectedItem == "PRINTER" && ActivePrinter && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                PrinterList = new ObservableCollection<Printer>();
                PrinterList = Settings.GetCachePrinters;
            }

            //Ticket start:#11981 assigning PaymentActiveChangedCommand back after reseting to null after sync.
            if (selectedItem == "PAYMENT OPTIONS" && PaymentActiveChangedCommand == null)
            {
                PaymentActiveChangedCommand = new Command<PaymentOptionDto>(async (obj) => await PaymentActiveChanged(obj));
            }
        }

        public void UpdateAutoLockDurationByValue(double duration)
        {

            AutoLockDurationList.Where(x => x.Value != duration).All(x =>
                  {
                      x.IsSelected = false;
                      return true;
                  });
            AutoLockDurationList.Where(x => x.Value == duration).All(x =>
                  {
                      x.IsSelected = true;
                      return true;
                  });
            if (AutoLockDurationList.Count(x => x.Value == duration) > 0)
            {
                AutoLockDuration = AutoLockDurationList.FirstOrDefault(x => x.Value == duration).Title;
            }

        }

        public void ChangeAutoLockDurationTapped(AutoLockDurationModel autolockdata)
        {

            AutoLockDurationList.Where(x => x.Title != autolockdata.Title).All(x =>
                  {
                      x.IsSelected = false;
                      return true;
                  });
            AutoLockDuration = autolockdata.Title;
            Settings.AutoLockDelay = autolockdata.Value;
            App.Instance.Start();

        }

        public async Task SyncData()
        {

            using (new Busy(this, true))
            {
                try
                {
                    if (!string.IsNullOrEmpty(Settings.HikePayStoreId))
                    {
                        var nadaTapToPay = DependencyService.Get<INadaTapToPay>();
                        var diagnosisResponse = await nadaTapToPay.Diagnosis();
                        //SentrySdk.CaptureMessage(diagnosisResponse);

                    }

                    //var oldCualture = Settings.StoreCulture;
                    var objOutletSync = new OutletSync();
                    await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: true, ResetAfterUppdate: false);
                    //refreshData(oldCualture != Settings.StoreCulture);
                    refreshData(false);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            };

        }

        public async Task ClearAndSyncData()
        {

            using (new Busy(this, true))
            {
                try
                {
                    if (!string.IsNullOrEmpty(Settings.HikePayStoreId))
                    {
                        var nadaTapToPay = DependencyService.Get<INadaTapToPay>();
                        nadaTapToPay.ClearSession();
                    }

                    var startDateTime = DateTime.Now;
                    Debug.WriteLine("Reset Data Starts at : " + startDateTime);

                    //var oldCualture = Settings.StoreCulture;
                    var objOutletSync = new OutletSync();
                    await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: true, ResetAfterUppdate: true);
                    //refreshData(oldCualture != Settings.StoreCulture);
                    refreshData(false);
                    WeakReferenceMessenger.Default.Send(new Messenger.ResetDataMessenger(true));
                    var endDateTime = DateTime.Now;
                    Debug.WriteLine("Reset Data Ends at : " + endDateTime);
                    Debug.WriteLine("Reset Data Total Time : " + (endDateTime - startDateTime));


                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            };

        }

        async void refreshData(bool LangaugeChanged)
        {
            //Ticket start:#11981 Disabled Payments Still Showing on Payment Page
            //PaymentActiveChangedCommand reset to PaymentActiveChangedCommand null to avoid firing PaymentActiveChangedCommand command when data assigned again after api response on sync. It is switch toggle issue
            PaymentActiveChangedCommand = null;
            try
            {
                EnterSalePage.DataUpdated = true;
                if (LangaugeChanged)
                {
                    var rejected = await App.Alert.ShowAlert("Language is changed. So, Do you want to reload application for better result?", "Don't reload app if your sale is in process. It will lost your current running sale.", "Not now", "Reload App");
                    if (!rejected)
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All"));

                        // if (MainPage.entersalepage != null)
                        //     MainPage.entersalepage = null;

                        // if (MainPage.parksalepage != null)
                        //     MainPage.parksalepage = null;

                        // if (MainPage.settingpage != null)
                        //     MainPage.settingpage = null;

                        // if (MainPage.adminpage != null)
                        //     MainPage.adminpage = null;

                        // if (MainPage.cashRegisterpage != null)
                        //     MainPage.cashRegisterpage = null;

                        App.Instance.MainPage = new AppShell();
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All"));

                        LoadPaymentOptions();

                        if (Settings.StoreGeneralRule != null)
                            ActiveMarkPaidOrderAsFulfilled = !Settings.StoreGeneralRule.ParkingPaidOrder;
                    }
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All"));

                    LoadPaymentOptions();

                    if (Settings.StoreGeneralRule != null)
                        ActiveMarkPaidOrderAsFulfilled = !Settings.StoreGeneralRule.ParkingPaidOrder;
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        #endregion

        #region PrinterTab
        public async Task FindPrinter()
        {
            IsLoad = true;
            await Task.Delay(10);

            if (printService == null)
            {
                printService = DependencyService.Get<IPrintService>();
            }
            printService.stopDiscovery();
            var printerList = printService.DecoveryAsync();

            //printService.DecoveryAsync(SearchingCompleted);

            printerList.All(x =>
            {
                if (x.ModelName.Contains("TSP") || x.ModelName.Contains("Star Micronics") || x.ModelName.Contains("TM")
                    || x.ModelName.Contains("MCP") || x.ModelName.Contains("SM-T"))
                {
                    x.width = 576;
                }
                else
                {
                    x.width = 384;
                }
                return true;
            });
            Settings.GetCachePrinters = printerList;
            PrinterList = printerList;
            IsLoad = false;

            string printerlabel = string.Empty;
            foreach (var item in PrinterList)
            {
                printerlabel = printerlabel + item.Port.ToString() + " " + item.ModelName.ToString();
            }

            
        }

        public void loadPrinters()
        {
            if (Settings.GetCachePrinters != null)
                PrinterList = Settings.GetCachePrinters;
            else
                PrinterList = new ObservableCollection<Printer>();
        }


        public void UpdateSelectedPrinter()
        {

            if (SelectedPrinter != null)
            {
                using (new Busy(this, true))
                {
                    var tmpAllPrinter = new ObservableCollection<Printer>(PrinterList.Where(x => x.ModelName != SelectedPrinter.ModelName).ToList())
                    {
                        SelectedPrinter
                    };
                    Settings.GetCachePrinters = tmpAllPrinter;
                    WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("Printer"));
                };
            }

        }

        public void UpdateNoOfCopiesByValue()
        {

            if (SelectedPrinter != null)
            {
                NoOfCopiesList.Where(x => (int)x.Value != SelectedPrinter.NumberOfPrintCopy).All(x =>
                      {
                          x.IsSelected = false;
                          return true;
                      });
                NoOfCopiesList.Where(x => (int)x.Value == SelectedPrinter.NumberOfPrintCopy).All(x =>
                      {
                          x.IsSelected = true;
                          return true;
                      });
            }

        }

        public void ChangeNoOfCopiesTapped(AutoLockDurationModel autolockdata)
        {

            if (SelectedPrinter != null)
            {
                NoOfCopiesList.Where(x => x.Title != autolockdata.Title).All(x =>
                      {
                          x.IsSelected = false;
                          return true;
                      });
                SelectedPrinter.NumberOfPrintCopy = (int)autolockdata.Value;
                Settings.NumberOfPrintCopy = (int)autolockdata.Value;
                UpdateSelectedPrinter();
            }

        }



        public void UpdateDocketRange()
        {
            DocketNumberRangeList.Where(x => (int)x.Value != DocketNumberRange).All(x =>
                  {
                      x.IsSelected = false;
                      return true;
                  });
            DocketNumberRangeList.Where(x => (int)x.Value == DocketNumberRange).All(x =>
                  {
                      x.IsSelected = true;
                      return true;
                  });
        }

        public void ChangeDocketRangeTapped(AutoLockDurationModel autolockdata)
        {

            if (autolockdata != null)
            {
                DocketNumberRangeList.Where(x => x.Title != autolockdata.Title).All(x =>
                      {
                          x.IsSelected = false;
                          return true;
                      });
                DocketNumberRange = (int)autolockdata.Value;
                Settings.PrintCustomerCurrentNumber = 1;
                Settings.PrintCustomerStartingNumber = 1;
                Settings.PrintCustomerEndingNumber = (int)autolockdata.Value;
            }

        }

        #endregion

        #region PaymentTab

        private async Task PaymentActiveChanged_Call(PaymentOptionDto obj)
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });
            await PaymentActiveChanged(obj);

        }


        public void LoadPaymentOptions()
        {

            using (new Busy(this, true))
            {
                try
                {

                    PaymentOptionList.Clear();
                    //if (PaymentOptionList == null || PaymentOptionList.Count < 1)
                    //{
                    int registerId = 0;
                    var register = Settings.CurrentRegister;
                    if (register != null)
                    {
                        registerId = register.Id;
                    }

                    var tmpAll_PaymentOptionList = paymentService.GetLocalPaymentOptionsByRegister(registerId);
                    if (tmpAll_PaymentOptionList != null)
                    {
                        All_PaymentOptionList = tmpAll_PaymentOptionList?.Where(item => !item.IsDeleted
                        && (item.PaymentOptionType == PaymentOptionType.Cash
                        || item.PaymentOptionType == PaymentOptionType.Card
                        || item.PaymentOptionType == PaymentOptionType.Tyro
                        || item.PaymentOptionType == PaymentOptionType.iZettle
                        || item.PaymentOptionType == PaymentOptionType.PayPal
                        || item.PaymentOptionType == PaymentOptionType.PayPalhere
                        || item.PaymentOptionType == PaymentOptionType.VantivIpad
                        || item.PaymentOptionType == PaymentOptionType.VantivCloud
                        || item.PaymentOptionType == PaymentOptionType.Mint
                        || item.PaymentOptionType == PaymentOptionType.AssemblyPayment
                        || item.PaymentOptionType == PaymentOptionType.EVOPayment
                        || item.PaymentOptionType == PaymentOptionType.VerifonePaymark
                        || item.PaymentOptionType == PaymentOptionType.PayJunction
                        || item.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard
                        || item.PaymentOptionType == PaymentOptionType.eConduit
                        || item.PaymentOptionType == PaymentOptionType.Moneris
                        //|| item.PaymentOptionType == PaymentOptionType.Afterpay
                        || item.PaymentOptionType == PaymentOptionType.Zip
                        || item.PaymentOptionType == PaymentOptionType.Square
                        || item.PaymentOptionType == PaymentOptionType.TD
                        || item.PaymentOptionType == PaymentOptionType.Elavon
                        || item.PaymentOptionType == PaymentOptionType.Linkly
                        || item.PaymentOptionType == PaymentOptionType.NAB
                        || item.PaymentOptionType == PaymentOptionType.Fiserv
                        || item.PaymentOptionType == PaymentOptionType.Bendigo
                        || item.PaymentOptionType == PaymentOptionType.ANZ
                        || item.PaymentOptionType == PaymentOptionType.Clearent
                        || item.PaymentOptionType == PaymentOptionType.Windcave
                        || item.PaymentOptionType == PaymentOptionType.CustomPayment
                        || item.PaymentOptionType == PaymentOptionType.VerifoneVcloud
                        || item.PaymentOptionType == PaymentOptionType.Castle
                        || item.PaymentOptionType == PaymentOptionType.Clover
                        || item.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                        || item.PaymentOptionType == PaymentOptionType.HikePay
                        || (item.PaymentOptionType == PaymentOptionType.TyroTapToPay && (Settings.TyroTapToPayConfiguration == null || (Settings.TyroTapToPayConfiguration != null && item.Id == Settings.TyroTapToPayConfiguration.Id)))
                        ));
                    }
                    else
                    {
                        All_PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                    }

                    var tmpPaymentOptionList = new ObservableCollection<PaymentOptionDto>(All_PaymentOptionList.Where(x => (x.PaymentOptionType == Enums.PaymentOptionType.Cash || x.PaymentOptionType == Enums.PaymentOptionType.Card) && (x.RegisterPaymentOptions == null || x.RegisterPaymentOptions.Count < 1 || x.RegisterPaymentOptions.Any(y => y.RegisterId == registerId))));
                    var last = tmpPaymentOptionList.LastOrDefault();
                    if (last != null)
                    {
                        last.IsNotLast = false;
                    }
                    PaymentOptionList = tmpPaymentOptionList;

                    var tempIntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(All_PaymentOptionList.Where(
                         x =>
                         (!x.RegisterPaymentOptions.Any() || x.RegisterPaymentOptions.Any(r => r.RegisterId == Settings.CurrentRegister.Id))
                         && (x.PaymentOptionType == Enums.PaymentOptionType.iZettle
                             || x.PaymentOptionType == Enums.PaymentOptionType.Tyro
                             || x.PaymentOptionType == Enums.PaymentOptionType.PayPal
                             || x.PaymentOptionType == Enums.PaymentOptionType.PayPalhere
                             || x.PaymentOptionType == Enums.PaymentOptionType.Mint
                             || x.PaymentOptionType == Enums.PaymentOptionType.VantivIpad
                             || x.PaymentOptionType == Enums.PaymentOptionType.VantivCloud
                             || x.PaymentOptionType == Enums.PaymentOptionType.AssemblyPayment
                             || x.PaymentOptionType == Enums.PaymentOptionType.PayJunction
                             || x.PaymentOptionType == Enums.PaymentOptionType.EVOPayment
                             || x.PaymentOptionType == Enums.PaymentOptionType.VerifonePaymark
                             || x.PaymentOptionType == Enums.PaymentOptionType.NorthAmericanBankcard
                             || x.PaymentOptionType == PaymentOptionType.eConduit
                             || x.PaymentOptionType == PaymentOptionType.Moneris
                             //|| x.PaymentOptionType == PaymentOptionType.Afterpay
                             || x.PaymentOptionType == PaymentOptionType.Zip
                             || x.PaymentOptionType == PaymentOptionType.Square
                             || x.PaymentOptionType == PaymentOptionType.TD
                             || x.PaymentOptionType == PaymentOptionType.Elavon
                             || x.PaymentOptionType == PaymentOptionType.Linkly
                             || x.PaymentOptionType == PaymentOptionType.NAB
                             || x.PaymentOptionType == PaymentOptionType.Fiserv
                             || x.PaymentOptionType == PaymentOptionType.Bendigo
                             || x.PaymentOptionType == PaymentOptionType.ANZ
                             || x.PaymentOptionType == PaymentOptionType.Clearent
                             || x.PaymentOptionType == PaymentOptionType.Windcave
                             || x.PaymentOptionType == PaymentOptionType.CustomPayment
                             || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud
                             || x.PaymentOptionType == PaymentOptionType.Castle
                             || x.PaymentOptionType == PaymentOptionType.Clover
                             || x.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                             || x.PaymentOptionType == PaymentOptionType.HikePay
                             || x.PaymentOptionType == PaymentOptionType.TyroTapToPay
                             )));

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.PayPal))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.PayPal,
                            PaymentOptionName = "PayPal",
                            DisplayName = "PayPal",
                            Name = "PayPal",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Tyro))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Tyro,
                            PaymentOptionName = "Tyro",
                            DisplayName = "Tyro",
                            Name = "Tyro",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.HikePayTapToPay))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.HikePayTapToPay,
                            PaymentOptionName = "NadaPay",
                            DisplayName = "NadaPay",
                            Name = "NadaPay",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.HikePay))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.HikePay,
                            PaymentOptionName = "HikePay",
                            DisplayName = "HikePay",
                            Name = "HikePay",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }
                    //#33485 iOS - Make changes for iZettle (name change from iZettle to Zettle)
                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.iZettle))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.iZettle,
                            PaymentOptionName = "PayPal Zettle",
                            DisplayName = "PayPal Zettle",
                            Name = "PayPal Zettle",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.VantivCloud))
                    {
                        var vantivoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.VantivCloud,
                            PaymentOptionName = "Vantiv Cloud",
                            DisplayName = "Vantiv Cloud",
                            Name = "Vantiv Cloud",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(vantivoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.VantivIpad))
                    {
                        var vantivoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.VantivIpad,
                            PaymentOptionName = "Vantiv",
                            DisplayName = "Vantiv",
                            Name = "Vantiv",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(vantivoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.AssemblyPayment))
                    {
                        /*
                         Note : previoulsy this payment name was "Assembly payment" after that "Westpac Group Payment"
                        and now new name is "Simple Payments Integrarion" (Mx51)
                         
                        */
                        var assemblyPaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.AssemblyPayment,
                            PaymentOptionName = "Simple Payments Integration",
                            DisplayName = "Simple Payments Integration",
                            Name = "Simple Payments Integration",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(assemblyPaymentoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Mint))
                    {
                        var mintoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Mint,
                            PaymentOptionName = "Mint",
                            DisplayName = "Mint",
                            Name = "Mint",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(mintoption);
                    }




                    #region econduit payment options

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.EVOPayment))
                    {
                        var EVOPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.EVOPayment,
                            PaymentOptionName = "EVOPayment",
                            DisplayName = "EVOPayment",
                            Name = "EVOPayment",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(EVOPaymentOption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.VerifonePaymark))
                    {
                        var VerifonePaymarkPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.VerifonePaymark,
                            PaymentOptionName = "Verifone",
                            DisplayName = "Verifone",
                            Name = "Verifone",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(VerifonePaymarkPaymentOption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.PayJunction))
                    {
                        var PayJunctionPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.PayJunction,
                            PaymentOptionName = "PayJunction",
                            Name = "PayJunction",
                            DisplayName = "PayJunction",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(PayJunctionPaymentOption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.NorthAmericanBankcard))
                    {
                        var NorthAmericanBankcardPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.NorthAmericanBankcard,
                            PaymentOptionName = "NorthAmericanBankcard",
                            Name = "NorthAmericanBankcard",
                            DisplayName = "NorthAmericanBankcard",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(NorthAmericanBankcardPaymentOption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.eConduit))
                    {
                        var eConduitPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.eConduit,
                            PaymentOptionName = "eConduit",
                            Name = "eConduit",
                            DisplayName = "eConduit",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(eConduitPaymentOption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Moneris))
                    {
                        var monerisPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Moneris,
                            PaymentOptionName = "Moneris",
                            Name = "Moneris",
                            DisplayName = "Moneris",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(monerisPaymentOption);
                    }
                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.CustomPayment))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.CustomPayment,
                            PaymentOptionName = "Other (Integrated using APIs)",
                            DisplayName = "Other",
                            Name = "Other",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }
                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.VerifoneVcloud))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.VerifoneVcloud,
                            PaymentOptionName = "Verifone–Vcloud",
                            DisplayName = "Verifone–Vcloud",
                            Name = "Verifone–Vcloud",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }


                    #endregion econduit paymentoptions

                    // Note : As per Alex, we need to remove AfterPay from app
                    /*
                     
                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Afterpay))
                    {
                        var afterpayPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Afterpay,
                            PaymentOptionName = "Afterpay",
                            Name = "Afterpay",
                            DisplayName = "Afterpay",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>()
                            {
                                 new RegisterPaymentOptionDto()
                                    {
                                        RegisterId = Settings.CurrentRegister.Id,
                                        RegisterName = Settings.CurrentRegister.Name
                                    }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(afterpayPaymentOption);
                    }

                    */
                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Zip))
                    {
                        var zipPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Zip,
                            PaymentOptionName = "Zip",
                            Name = "Zip",
                            DisplayName = "Zip",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>()
                            {
                                 new RegisterPaymentOptionDto()
                                    {
                                        RegisterId = Settings.CurrentRegister.Id,
                                        RegisterName = Settings.CurrentRegister.Name
                                    }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(zipPaymentOption);
                    }


                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.TD))
                    {
                        var Fiskaoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = true,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.TD,
                            PaymentOptionName = "TD Bank",
                            Name = "TD Bank",
                            DisplayName = "TD Bank",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(Fiskaoption);
                    }


                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Elavon))
                    {
                        var ElavonOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = true,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Elavon,
                            PaymentOptionName = "Elavon",
                            Name = "Elavon",
                            DisplayName = "Elavon",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(ElavonOption);
                    }


                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Square))
                    {
                        var squarePaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            IsActive = false,
                            //Ticket #12742 Square Shall Not Be Ready to Use. By Nikhil	 
                            IsConfigered = false,
                            //Ticket #12742 End. By Nikhil
                            PaymentOptionType = Enums.PaymentOptionType.Square,
                            PaymentOptionName = "Square",
                            Name = "Square",
                            DisplayName = "Square",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(squarePaymentoption);
                    }


                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Windcave))
                    {
                        var squarePaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            IsActive = false,
                            //Ticket #12742 Square Shall Not Be Ready to Use. By Nikhil	 
                            IsConfigered = false,
                            //Ticket #12742 End. By Nikhil
                            PaymentOptionType = Enums.PaymentOptionType.Windcave,
                            PaymentOptionName = "Windcave",
                            Name = "Windcave",
                            DisplayName = "Windcave",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(squarePaymentoption);
                    }

                    #region Linkly related Payments

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Linkly))
                    {
                        var linklyPaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            DisplayName = "Linkly",
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Linkly,
                            PaymentOptionName = "Linkly",
                            Name = "Linkly",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(linklyPaymentoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.NAB))
                    {
                        var NABPaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.NAB,
                            PaymentOptionName = "NAB",
                            Name = "NAB",
                            DisplayName = "NAB (powered by linkly)",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(NABPaymentoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Fiserv))
                    {
                        var FiservPaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Fiserv,
                            PaymentOptionName = "Fiserv",
                            Name = "Fiserv",
                            DisplayName = "Fiserv (powered by linkly)",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(FiservPaymentoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Bendigo))
                    {
                        var BendigoPaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Bendigo,
                            PaymentOptionName = "Bendigo",
                            Name = "Bendigo",
                            DisplayName = "Bendigo (powered by linkly)",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(BendigoPaymentoption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.ANZ))
                    {
                        var ANZPaymentoption = new PaymentOptionDto()
                        {
                            CanBeConfigered = false,
                            IsCustomerAccount = true,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.ANZ,
                            PaymentOptionName = "ANZ",
                            Name = "ANZ",
                            DisplayName = "ANZ (powered by linkly)",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(ANZPaymentoption);
                    }

                    #endregion


                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.TD
                    || x.PaymentOptionType == Enums.PaymentOptionType.Elavon
                    ))
                    {
                        tempIntegratedPaymentOptionList.All(x =>
                        {
                            x.IsConfigered = !string.IsNullOrEmpty(x.ConfigurationDetails);
                            return true;
                        });

                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Clearent))
                    {
                        var clearentPaymentOption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Clearent,
                            PaymentOptionName = "Clearent",
                            Name = "Clearent",
                            DisplayName = "Clearent",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>()
                            {
                                 new RegisterPaymentOptionDto()
                                    {
                                        RegisterId = Settings.CurrentRegister.Id,
                                        RegisterName = Settings.CurrentRegister.Name
                                    }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(clearentPaymentOption);
                    }

                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Castle))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Castle,
                            PaymentOptionName = "Lloyds Bank Cardnet - Castles",
                            DisplayName = "Lloyds Bank Cardnet - Castles",
                            Name = "Lloyds Bank Cardnet - Castles",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }
                    if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Clover))
                    {
                        var paypaloption = new PaymentOptionDto()
                        {
                            CanBeConfigered = true,
                            IsCustomerAccount = false,
                            IsActive = false,
                            IsConfigered = false,
                            PaymentOptionType = Enums.PaymentOptionType.Clover,
                            PaymentOptionName = "Lloyds Bank Cardnet – Clover",
                            DisplayName = "Lloyds Bank Cardnet – Clover",
                            Name = "Lloyds Bank Cardnet – Clover",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            }
                        };
                        tempIntegratedPaymentOptionList.Add(paypaloption);
                    }

                    // if (!tempIntegratedPaymentOptionList.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.TyroTapToPay))
                    // {
                    //     var paypaloption = new PaymentOptionDto()
                    //     {
                    //         CanBeConfigered = true,
                    //         IsCustomerAccount = false,
                    //         IsActive = false,
                    //         IsConfigered = false,
                    //         PaymentOptionType = Enums.PaymentOptionType.TyroTapToPay,
                    //         PaymentOptionName = "TyroTapToPay",
                    //         DisplayName = "TapToPay",
                    //         Name = "TapToPay",
                    //         RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                    //             new RegisterPaymentOptionDto(){
                    //                 RegisterId = Settings.CurrentRegister.Id,
                    //                 RegisterName = Settings.CurrentRegister.Name
                    //             }
                    //         }
                    //     };
                    //     tempIntegratedPaymentOptionList.Add(paypaloption);
                    // }
                   
                    var intlast = tempIntegratedPaymentOptionList.LastOrDefault();
                    if (intlast != null)
                    {
                        intlast.IsNotLast = false;
                    }
                    DisplayCountrySpecificPayment(tempIntegratedPaymentOptionList);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }


        private void DisplayCountrySpecificPayment(ObservableCollection<PaymentOptionDto> payments)
        {
            var countryCode = Settings.CountryCode.ToUpper();
            //countryCode = "AT";

            switch (countryCode)
            {
                case "AU":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x =>
                        x.PaymentOptionType == PaymentOptionType.AssemblyPayment
                        || x.PaymentOptionType == PaymentOptionType.Tyro
                        || x.PaymentOptionType == PaymentOptionType.Afterpay
                        || x.PaymentOptionType == PaymentOptionType.Zip
                        || x.PaymentOptionType == PaymentOptionType.PayPal
                        || x.PaymentOptionType == PaymentOptionType.PayPalhere
                        || x.PaymentOptionType == PaymentOptionType.Square
                        || x.PaymentOptionType == PaymentOptionType.Linkly
                        || x.PaymentOptionType == PaymentOptionType.NAB
                        || x.PaymentOptionType == PaymentOptionType.Fiserv
                        || x.PaymentOptionType == PaymentOptionType.Bendigo
                        || x.PaymentOptionType == PaymentOptionType.ANZ
                        || x.PaymentOptionType == PaymentOptionType.Windcave
                        || x.PaymentOptionType == PaymentOptionType.CustomPayment
                        || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud
                        || x.PaymentOptionType == PaymentOptionType.TyroTapToPay
                        || x.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                        || x.PaymentOptionType == PaymentOptionType.HikePay
                        ));
                    break;
                case "CA":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x =>
                       x.PaymentOptionType == PaymentOptionType.TD
                       || x.PaymentOptionType == PaymentOptionType.Moneris
                       || x.PaymentOptionType == PaymentOptionType.Elavon
                       || x.PaymentOptionType == PaymentOptionType.CustomPayment
                       || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud));
                    IsIntegratedPaymentAvailable = true;
                    break;
                case "NZ":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x =>
                        x.PaymentOptionType == PaymentOptionType.Afterpay || x.PaymentOptionType == PaymentOptionType.Zip
                        || x.PaymentOptionType == PaymentOptionType.VerifonePaymark
                        || x.PaymentOptionType == PaymentOptionType.Windcave
                        || x.PaymentOptionType == PaymentOptionType.CustomPayment
                        || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud));
                    break;
                case "GB":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x =>
                        x.PaymentOptionType == PaymentOptionType.iZettle
                        || x.PaymentOptionType == PaymentOptionType.PayPal
                        || x.PaymentOptionType == PaymentOptionType.PayPalhere
                        || x.PaymentOptionType == PaymentOptionType.Windcave
                        || x.PaymentOptionType == PaymentOptionType.CustomPayment
                        || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud
                        // || x.PaymentOptionType == PaymentOptionType.NadaPay
                         || x.PaymentOptionType == PaymentOptionType.Castle
                         || x.PaymentOptionType == PaymentOptionType.Clover
                        ));
                    break;
                //Ticket start:#54684 Zettle Integration.by rupesh
                case "US":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x =>
                        x.PaymentOptionType == PaymentOptionType.VantivIpad
                     || x.PaymentOptionType == PaymentOptionType.VantivCloud
                     || x.PaymentOptionType == PaymentOptionType.PayPal
                     || x.PaymentOptionType == PaymentOptionType.PayPalhere
                     || x.PaymentOptionType == PaymentOptionType.PayJunction
                     || x.PaymentOptionType == PaymentOptionType.EVOPayment
                     || x.PaymentOptionType == PaymentOptionType.eConduit

                     //#27187 Remove North American Bancard from Payment Type Dropdown List
                     //|| x.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard
                     || x.PaymentOptionType == PaymentOptionType.Ecommerce
                     || x.PaymentOptionType == PaymentOptionType.Square
                     || x.PaymentOptionType == PaymentOptionType.Elavon
                     || x.PaymentOptionType == PaymentOptionType.Linkly
                     || x.PaymentOptionType == PaymentOptionType.NAB
                     || x.PaymentOptionType == PaymentOptionType.Fiserv
                     || x.PaymentOptionType == PaymentOptionType.Bendigo
                     || x.PaymentOptionType == PaymentOptionType.ANZ
                     || x.PaymentOptionType == PaymentOptionType.Clearent
                     || x.PaymentOptionType == PaymentOptionType.Windcave
                     || x.PaymentOptionType == PaymentOptionType.CustomPayment
                     || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud
                     || x.PaymentOptionType == PaymentOptionType.iZettle
                     || x.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                     || x.PaymentOptionType == PaymentOptionType.HikePay
                     ));
                    break;
                case "IN":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>
                        (payments.Where(x => x.PaymentOptionType != PaymentOptionType.Mint));
                    break;
                case "VI":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x => x.PaymentOptionType == PaymentOptionType.eConduit));
                    break;
                case "BM":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x => x.PaymentOptionType == PaymentOptionType.eConduit));
                    break;
                case "PR":
                    IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x => x.PaymentOptionType == PaymentOptionType.eConduit));
                    break;
                default:

                    var tempcountry = new CountryCode();
                    string code = tempcountry.GetCountryCode(countryCode.ToUpper());

                    if (code != null && code == "Europe")
                        IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x => x.PaymentOptionType == PaymentOptionType.iZettle || x.PaymentOptionType == PaymentOptionType.Clearent));
                    else
                    {
                        IntegratedPaymentOptionList = new ObservableCollection<PaymentOptionDto>(payments.Where(x => x.PaymentOptionType == PaymentOptionType.Square || x.PaymentOptionType == PaymentOptionType.VerifoneVcloud));
                        IsIntegratedPaymentAvailable = false;
                    }
                    //Ticket endd:#54684 .by rupesh
                    break;
            }
        }

        public async Task<bool> PaymentActiveChanged(PaymentOptionDto paymentoption)
        {
            if (paymentoption != null)
            {
                //Start :Do not allow to change for TyroTapToPay at all
                if(paymentoption.PaymentOptionType == PaymentOptionType.TyroTapToPay)
                {
                    return true;
                }
                //End :Do not allow to change for TyroTapToPay at all

                using (new Busy(this, true))
                {

                    Debug.WriteLine("paymentoption :: " + paymentoption.ToJson());


                    if (paymentoption.PaymentOptionType == PaymentOptionType.TD
                        || paymentoption.PaymentOptionType == PaymentOptionType.Elavon)
                    {
                        if (!paymentoption.IsConfigered)
                        {
                            paymentoption.IsActive = false;
                            App.Instance.Hud.DisplayToast("First, cofigure " + paymentoption.Name + " from web" + " and sync data from General Settings", Colors.Red, Colors.White);
                            return true;
                        }
                    }


                    var updatedoption = paymentoption;
                    //Ticket start: #12744 iOS - Configuration Not Saved on Server by rupesh
                    if (!paymentoption.RegisterPaymentOptions.Any() || paymentoption.RegisterPaymentOptions.Any(X => X.RegisterId == Settings.CurrentRegister.Id))
                    {

                        updatedoption = await paymentService.UpdateRemotePaymentOption(Fusillade.Priority.UserInitiated, true, paymentoption);
                    }
                    //Ticket end:#12744

                    /* Date: 17/02/2020
                        * Note : we add below code to store payment id in list and send it to server while change register.
                        * After that we have got one ticket #9463: for multiple register with multiple payment option.
                        * as per this issue, we discussed with web team and decied to dont link iPad payment with all register.
                        * User has to configure payment register wise.
                    */
                    /*
                     try {

                         if(paymentoption.IsConfigered)
                         {
                             if (Settings.IPadConfiguredPaymentID != null)
                             {

                                if (!Settings.IPadConfiguredPaymentID.Any(x => x == paymentoption.Id))
                                 {
                                     Settings.IPadConfiguredPaymentID.Add(paymentoption.Id);
                                 }

                             }
                             else
                             {
                                 Settings.IPadConfiguredPaymentID = new List<int>();
                                 Settings.IPadConfiguredPaymentID.Add(paymentoption.Id);
                             }
                         }
                     } catch (Exception ex)
                     {

                     }
                     */

                    if (paymentoption.PaymentOptionType == PaymentOptionType.iZettle
                        || paymentoption.PaymentOptionType == PaymentOptionType.PayPal
                        || paymentoption.PaymentOptionType == PaymentOptionType.PayPalhere
                        || paymentoption.PaymentOptionType == PaymentOptionType.Tyro
                        || paymentoption.PaymentOptionType == PaymentOptionType.VantivIpad
                        || paymentoption.PaymentOptionType == PaymentOptionType.Mint
                        || paymentoption.PaymentOptionType == PaymentOptionType.AssemblyPayment
                        || paymentoption.PaymentOptionType == PaymentOptionType.VerifonePaymark
                        || paymentoption.PaymentOptionType == PaymentOptionType.EVOPayment
                        || paymentoption.PaymentOptionType == PaymentOptionType.PayJunction
                        || paymentoption.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard
                        || paymentoption.PaymentOptionType == PaymentOptionType.eConduit
                        || paymentoption.PaymentOptionType == PaymentOptionType.Afterpay
                        || paymentoption.PaymentOptionType == PaymentOptionType.Zip
                        || paymentoption.PaymentOptionType == PaymentOptionType.TD
                        || paymentoption.PaymentOptionType == PaymentOptionType.Moneris
                        || paymentoption.PaymentOptionType == PaymentOptionType.Square
                        || paymentoption.PaymentOptionType == PaymentOptionType.Linkly
                        || paymentoption.PaymentOptionType == PaymentOptionType.NAB
                        || paymentoption.PaymentOptionType == PaymentOptionType.Fiserv
                        || paymentoption.PaymentOptionType == PaymentOptionType.Bendigo
                        || paymentoption.PaymentOptionType == PaymentOptionType.ANZ
                        || paymentoption.PaymentOptionType == PaymentOptionType.Clearent
                        || paymentoption.PaymentOptionType == PaymentOptionType.CustomPayment
                        || paymentoption.PaymentOptionType == PaymentOptionType.VerifoneVcloud
                        || paymentoption.PaymentOptionType == PaymentOptionType.Castle
                        || paymentoption.PaymentOptionType == PaymentOptionType.Clover
                        || paymentoption.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                        || paymentoption.PaymentOptionType == PaymentOptionType.HikePay
                        || paymentoption.PaymentOptionType == PaymentOptionType.TyroTapToPay)
                    {
                        var oldOption = IntegratedPaymentOptionList.FirstOrDefault(x => x.PaymentOptionType == paymentoption.PaymentOptionType && x.Name == paymentoption.Name && x.Id == paymentoption.Id);
                        if (oldOption != null)
                        {
                            {
                                oldOption.Id = updatedoption.Id;
                                oldOption.IsActive = updatedoption.IsActive;
                                oldOption.Name = updatedoption.Name;
                                oldOption.PaymentOptionType = updatedoption.PaymentOptionType;
                                oldOption.PaymentOptionName = updatedoption.PaymentOptionName;
                                oldOption.IsDefault = updatedoption.IsDefault;
                                oldOption.CanBeConfigered = updatedoption.CanBeConfigered;
                                oldOption.IsConfigered = updatedoption.IsConfigered;
                                oldOption.IsCustomerAccount = updatedoption.IsCustomerAccount;
                                oldOption.RegisterPaymentOptions = updatedoption.RegisterPaymentOptions;
                                oldOption.ConfigurationDetails = updatedoption.ConfigurationDetails;
                                SetPropertyChanged(nameof(IntegratedPaymentOptionList));
                            }
                        }
                    }
                    else
                    {
                        var oldOption = PaymentOptionList.FirstOrDefault(x => x.PaymentOptionType == paymentoption.PaymentOptionType && x.Id == paymentoption.Id);
                        if (oldOption != null)
                        {
                            oldOption.Id = updatedoption.Id;
                            oldOption.IsActive = updatedoption.IsActive;
                            oldOption.Name = updatedoption.Name;
                            oldOption.PaymentOptionType = updatedoption.PaymentOptionType;
                            oldOption.PaymentOptionName = updatedoption.PaymentOptionName;
                            oldOption.IsDefault = updatedoption.IsDefault;
                            oldOption.CanBeConfigered = updatedoption.CanBeConfigered;
                            oldOption.IsConfigered = updatedoption.IsConfigered;
                            oldOption.IsCustomerAccount = updatedoption.IsCustomerAccount;
                            oldOption.RegisterPaymentOptions = updatedoption.RegisterPaymentOptions;
                            oldOption.ConfigurationDetails = updatedoption.ConfigurationDetails;
                            SetPropertyChanged(nameof(PaymentOptionList));
                        }
                    }

                    EnterSalePage.PaymentActiveUpdated = true;
                    if(!paymentoption.IsConfigered || !paymentoption.IsActive)
                        App.Instance.Issync = true;
                    // if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel>)
                    // {
                    //     ((BaseContentPage<EnterSaleViewModel>)_navigationService.NavigatedPage).ViewModel.loadPayments();
                    // }
                };
            }
            return true;
        }


        //public PaymentOptionDto SelectedPaymentoptionForConfiguration = null;
        private bool IsPaymentClicked;
        async void OpenPaymentConfigurationView(PaymentOptionDto paymentoption)
        {
            if (IsPaymentClicked)
                return;
            IsPaymentClicked = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? 2000 : 1000).Wait();
                IsPaymentClicked = false;
            });

            CurrentConfigurePaymentOption = paymentoption;
            if (paymentoption == null)
            {
                return;
            }


            if (paymentoption.IsConfigered && paymentoption.PaymentOptionType != PaymentOptionType.AssemblyPayment && paymentoption.PaymentOptionType != PaymentOptionType.TyroTapToPay)
            {
                try
                {
                    if (paymentoption.PaymentOptionType == PaymentOptionType.EVOPayment ||
                   paymentoption.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard ||
                   paymentoption.PaymentOptionType == PaymentOptionType.PayJunction ||
                   paymentoption.PaymentOptionType == PaymentOptionType.eConduit ||
                   paymentoption.PaymentOptionType == PaymentOptionType.VerifonePaymark ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Afterpay ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Zip ||
                   paymentoption.PaymentOptionType == PaymentOptionType.VantivCloud ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Moneris ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Square ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Elavon ||
                   paymentoption.PaymentOptionType == PaymentOptionType.TD ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Linkly ||
                   paymentoption.PaymentOptionType == PaymentOptionType.NAB ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Fiserv ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Bendigo ||
                   paymentoption.PaymentOptionType == PaymentOptionType.ANZ ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Clearent ||
                   paymentoption.PaymentOptionType == PaymentOptionType.Windcave ||
                   paymentoption.PaymentOptionType == PaymentOptionType.CustomPayment ||
                   paymentoption.PaymentOptionType == PaymentOptionType.VerifoneVcloud ||
                   paymentoption.PaymentOptionType == PaymentOptionType.HikePayTapToPay ||
                   paymentoption.PaymentOptionType == PaymentOptionType.HikePay)
                    {
                        App.Instance.Hud.DisplayToast("Please remove configuration " + paymentoption.Name + " from web" + " and sync data from General Settings", Colors.Red, Colors.White);
                        return;
                   }

                    var confirmed = await App.Alert.ShowAlert("", "Disconnect this payment device?", "Confirm", "Cancel");
                    if (!confirmed)
                    {
                        return;
                    }

                    paymentoption.IsConfigered = false;

                    if (paymentoption.PaymentOptionType == PaymentOptionType.Tyro)
                    {
                        if (configurationpage == null)
                        {
                            InitializeConfigurationpage();
                        }
                        if (configurationpage.ViewModel.EvaluateJavascript != null)
                        {
                            var TyroIntegrationkey = await configurationpage.ViewModel.EvaluateJavascript("localStorage.removeItem('webTta.integrationKey')");
                        }
                        paymentoption.ConfigurationDetails = null;
                    }
                    else if (paymentoption.PaymentOptionType == PaymentOptionType.PayPal || paymentoption.PaymentOptionType == PaymentOptionType.PayPalhere || paymentoption.PaymentOptionType == PaymentOptionType.Mint || paymentoption.PaymentOptionType == PaymentOptionType.VantivIpad)
                    {
                        paymentoption.ConfigurationDetails = null;
                    }
                   
                    else if (paymentoption.PaymentOptionType == PaymentOptionType.Castle)
                    {
                        try
                        {
                            Settings.Castlessettings = null;
                            paymentoption.IsConfigered = false;
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }

                    IsBusy = false;
                    await PaymentActiveChanged(paymentoption);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
                return;
            }


            if (paymentoption.PaymentOptionType == PaymentOptionType.Tyro ||
                paymentoption.PaymentOptionType == PaymentOptionType.PayPal ||
                paymentoption.PaymentOptionType == PaymentOptionType.PayPalhere)
            {
                if (configurationpage == null)
                {
                    InitializeConfigurationpage();
                }

                switch (paymentoption.PaymentOptionType)
                {
                    case PaymentOptionType.Tyro:
                        if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live)
                        {
                            configurationpage.ViewModel.WebURL = ServiceConfiguration.TyroLiveUrl + "/configuration.html";
                        }
                        else
                        {
                            configurationpage.ViewModel.WebURL = ServiceConfiguration.TyroTestUrl + "/configuration.html";
                        }
                        break;
                    case PaymentOptionType.PayPal:
                        if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live)
                        {
                            configurationpage.ViewModel.WebURL = ServiceConfiguration.PaypalLiveUrl;
                        }
                        else
                        {
                            configurationpage.ViewModel.WebURL = ServiceConfiguration.PaypalSandboxUrl;
                        }
                        break;
                    case PaymentOptionType.PayPalhere:
                        if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live)
                        {
                            configurationpage.ViewModel.WebURL = ServiceConfiguration.PaypalLiveUrl;
                        }
                        else
                        {
                            configurationpage.ViewModel.WebURL = ServiceConfiguration.PaypalSandboxUrl;
                        }
                        break;
                }


                try
                {
                    configurationpage.ViewModel.paymenttype = paymentoption.PaymentOptionType;

                    await NavigationService.PushModalAsync(configurationpage);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            
            else if (paymentoption.PaymentOptionType == PaymentOptionType.HikePay)
            {
                try
                {
                    if (hikePayConfigurationPage == null)
                    {
                        hikePayConfigurationPage = new HikePayConfigurationPage();
                        if (!string.IsNullOrEmpty(CurrentConfigurePaymentOption.ConfigurationDetails))
                        {
                            hikePayConfigurationPage.ViewModel.ConfigurationModel = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(CurrentConfigurePaymentOption.ConfigurationDetails);
                        }
                        hikePayConfigurationPage.ViewModel.ConfigurationSuccessed += async (object sender, NadaPayConfigurationDto configurationModel) =>
                        {
                            if (configurationModel != null && CurrentConfigurePaymentOption != null && CurrentConfigurePaymentOption.PaymentOptionType == PaymentOptionType.HikePay)
                            {
                                CurrentConfigurePaymentOption.ConfigurationDetails = JsonConvert.SerializeObject(configurationModel, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                CurrentConfigurePaymentOption.IsConfigered = true;
                                await PaymentActiveChanged(CurrentConfigurePaymentOption);
                            }
                            else
                            {
                                CurrentConfigurePaymentOption.IsConfigered = false;
                                CurrentConfigurePaymentOption.ConfigurationDetails = null;
                                await PaymentActiveChanged(CurrentConfigurePaymentOption);
                            }
                        };
                    }
                    await NavigationService.PushModalAsync(hikePayConfigurationPage);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            else if (paymentoption.PaymentOptionType == PaymentOptionType.EVOPayment ||
                    paymentoption.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard ||
                    paymentoption.PaymentOptionType == PaymentOptionType.PayJunction ||
                    paymentoption.PaymentOptionType == PaymentOptionType.eConduit ||
                    paymentoption.PaymentOptionType == PaymentOptionType.VerifonePaymark ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Afterpay ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Zip ||
                    paymentoption.PaymentOptionType == PaymentOptionType.VantivCloud ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Moneris ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Square ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Elavon ||
                    paymentoption.PaymentOptionType == PaymentOptionType.TD ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Linkly ||
                    paymentoption.PaymentOptionType == PaymentOptionType.NAB ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Fiserv ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Bendigo ||
                    paymentoption.PaymentOptionType == PaymentOptionType.ANZ ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Clearent ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Windcave ||
                    paymentoption.PaymentOptionType == PaymentOptionType.CustomPayment ||
                    paymentoption.PaymentOptionType == PaymentOptionType.VerifoneVcloud ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Castle ||
                    paymentoption.PaymentOptionType == PaymentOptionType.Clover ||
                    paymentoption.PaymentOptionType == PaymentOptionType.HikePayTapToPay ||
                    paymentoption.PaymentOptionType == PaymentOptionType.HikePay ||
                    paymentoption.PaymentOptionType == PaymentOptionType.TyroTapToPay)
            {
                App.Instance.Hud.DisplayToast("Please configure " + paymentoption.Name + " from web " + " and sync data from General Settings", Colors.Red, Colors.White);
                return;
            }

        }
        #endregion

        #region Methods

        public static async Task<PermissionStatus> CheckAndRequestCameraPermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return status;
            }

            return status;
        }

        public void OnLoaded()
        {
            #region Printertab
            loadPrinters();
            LoadPaymentOptions();
            #endregion


            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.PaymentOptionConfiguredMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.PaymentOptionConfiguredMessenger>(this, async (sender, arg) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(arg.Value) && CurrentConfigurePaymentOption != null)
                        {
                            CurrentConfigurePaymentOption.IsConfigered = true;

                            CurrentConfigurePaymentOption.ConfigurationDetails = arg.Value;

                            await PaymentActiveChanged(CurrentConfigurePaymentOption);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.SignalRPaymentTypesMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.SignalRPaymentTypesMessenger>(this, (sender, arg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (arg.Value.PaymentOptionType == PaymentOptionType.Cash || arg.Value.PaymentOptionType == PaymentOptionType.Card)
                        {
                            if (PaymentOptionList != null && PaymentOptionList.Count > 0 && !PaymentOptionList.Any(x => x.Id == arg.Value.Id))
                            {
                                PaymentOptionList.Add(arg.Value);
                                SetPropertyChanged(nameof(PaymentOptionList));
                            }
                        }
                        else
                        {
                            //Note : Below code is commented because we are not getting signalR call in live server.
                            // and in ASY app is crash.

                            //if (ViewModel.IntegratedPaymentOptionList != null && ViewModel.IntegratedPaymentOptionList.Count > 0 && !ViewModel.IntegratedPaymentOptionList.Any(x => x.Id == arg.Value.Id))
                            //{

                            //    if (arg.Value.RegisterPaymentOptions.Any(x => x.RegisterId == Settings.CurrentRegister.Id))
                            //    {
                            //        ViewModel.IntegratedPaymentOptionList.Add(arg.Value);
                            //        ViewModel.SetPropertyChanged(nameof(ViewModel.IntegratedPaymentOptionList));
                            //    }
                            //}
                        }
                    });
                });
            }

        }


        private string GenerateCustomerAppCode()
        {
            Random random = new Random();
            CustomerAppCode = random.Next(10000).ToString();
            return CustomerAppCode;
        }

        private async Task<CustomerDisplayCodeResponseModel> CreateOrUpdateCustomerDisplayCode(string customerCode)
        {
            if (customerCode != null)
            {
                var accountApiService = new ApiService<IAccountApi>();
                var accountService = new AccountServices(accountApiService);

                Debug.WriteLine("Unique id : " + Settings.UniqueDeviceID.ToString());

                

                CustomerDisplayCodeRequestModel requestModel = new CustomerDisplayCodeRequestModel()
                {
                    deviceId = Settings.UniqueDeviceID != string.Empty ? Settings.UniqueDeviceID : Settings.TenantId.ToString(),
                    devicePairPin = customerCode,
                    id = 0
                };

                var result =  await accountService.CreateOrUpdateCustomerDisplayCode(Fusillade.Priority.UserInitiated, requestModel);

                //Debug.WriteLine("Customer code api response : " + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result.result;
            }

            
            return null;
            
        }

        #endregion

    }
}
