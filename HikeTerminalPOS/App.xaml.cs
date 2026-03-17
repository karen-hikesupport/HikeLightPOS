
using HikePOS;
using System.Threading;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;
using System.Collections.ObjectModel;
using HikePOS.Interfaces;
using CommunityToolkit.Mvvm.Messaging;
using static HikePOS.ViewModels.BaseViewModel;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
#if ANDROID
using HikePOS.Droid.DependencyServices;
using HikePOS.Droid;
#endif

namespace HikePOS;


public partial class App : Application
{

    static App _instance;
    public static App Instance { get { return _instance; } }

    public IHUDProvider _hud;
    public IHUDProvider Hud { get { return _hud ?? (_hud = DependencyService.Get<IHUDProvider>()); } }
    private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();  
    public static IAlertService Alert { get { return ServiceLocator.Get<IAlertService>(); } }

    public static float FontIncrAmount { get; set; }

    public bool IsTimerActive = false;
    public bool IsInternetConnected = false;
    public bool IsBarcodeScannerConnected = false;
    public bool IsRequiredEnterSaleDataReload = false;

    //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
    public bool Issync;
    //End Ticket #72507 Pratik

    public string NotificationToken = "";

    public TimeSpan timespan { get; set; }
    CancellationTokenSource cancellation;
    AutoLockPage autolockpage;

    ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
    OutletServices outletServices;

    public App()
    {
        _instance = this;
        InitializeComponent();
        
        RegisterAppServices();
        IsInternetConnected = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        LoadApp();
        Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

        //timespan = TimeSpan.FromSeconds(30);
        cancellation = new CancellationTokenSource();
        Start();

        if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.TouchEventFiredMessenger>(this))
        {
            WeakReferenceMessenger.Default.Register<Messenger.TouchEventFiredMessenger>(this, (sender, arg) =>
            {
                Stop();
                Start();
            });
        }

    }

    private void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        IsInternetConnected = e.NetworkAccess == NetworkAccess.Internet;
        WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("Internet"));

        if (!string.IsNullOrEmpty(Settings.AccessToken))
        {
            try
            {
                Task.Run(() =>
                {
                    OutletSync objOutletSync = new OutletSync();
                    Task.Run(() => objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: true, RequiredAllData: false, ResetAfterUppdate: false, onlyUpload: true));
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }

    public void Start()
    {
        if (Settings.AutoLockDelay != 0)
        {
            if (Settings.IsAppLocked)
            {
                timespan = TimeSpan.FromMinutes(0);
            }
            else
            {
                timespan = TimeSpan.FromMinutes(Settings.AutoLockDelay);
            }
            StartTimerFunc(timespan);
        }
    }

    public void StartTimerFunc(TimeSpan timespan)
    {
        CancellationTokenSource cts = this.cancellation; // safe copy
        Dispatcher.StartTimer(timespan,
            () =>
            {
                if (cts.IsCancellationRequested)
                    return false;
                try
                {
                    timespan = TimeSpan.FromMinutes(Settings.AutoLockDelay);

                    if (autolockpage == null && !string.IsNullOrEmpty(Settings.AccessToken))
                    {
                        autolockpage = new AutoLockPage();
                        autolockpage.RequiredUpdateAccessToken = false;
                        autolockpage.AuthennticationSuccessed += AuthennticationSuccessed;
                        autolockpage.HasBackButtton = false;
                        autolockpage.HasSwitchButtton = true;
                        autolockpage.ViewModel.CurrentUser = Settings.CurrentUser;

                        _navigationService.GetCurrentPage.Navigation.PushModalAsync(autolockpage);
                    }
                    else if (autolockpage != null && !Settings.IsAppLocked && !string.IsNullOrEmpty(Settings.AccessToken))
                    {
                        autolockpage.ViewModel.CurrentUser = Settings.CurrentUser;

                        _navigationService.GetCurrentPage.Navigation.PushModalAsync(autolockpage);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
                return false; // or true for periodic behavior
            });
    }

    public async void AuthennticationSuccessed(object sender, bool e)
    {
        if (e && sender != null && sender is AutoLockPage)
        {
            var Autolockpage = (AutoLockPage)sender;

            await Autolockpage.Close();
            Settings.IsAppLocked = false;
            if (Settings.IsAppLocked)
            {
                timespan = TimeSpan.FromMinutes(0);
            }
            else
            {
                timespan = TimeSpan.FromMinutes(Settings.AutoLockDelay);
            }
            StartTimerFunc(timespan);
        }
    }

    public async void LoadApp()
    {

        if (!string.IsNullOrEmpty(Settings.AccessToken))
        {
            bool isExpired = false;

            bool isLocalSubscriptionExpired = false;// await Settings.Subscription.IsExpired(false);
            if (isLocalSubscriptionExpired)
            {
                ApiService<ISubscriptionAPI> SubscriptionApiService = new ApiService<ISubscriptionAPI>();
                SubscriptionSevice subscriptionSevice = new SubscriptionSevice(SubscriptionApiService);
                SubscriptionDto Subscription = null; 
                //Task.Run(async () =>
                //{
                    Subscription =  subscriptionSevice.GetLocalSubscription();
                //}).Wait();
                Settings.Subscription = Subscription;
                isExpired = await Subscription.IsExpired(false);
            }

            if (!isExpired)
            {
                ApiService<IShopApi> shopApiService = new ApiService<IShopApi>();
                ShopServices shopService = new ShopServices(shopApiService);
                ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();

                //BlobCache.ApplicationName = "HikePOS_" + Settings.TenantName;
                if (Settings.SelectedOutletId != 0 && !string.IsNullOrEmpty(Settings.SelectedOutletName) && Settings.CurrentRegister != null && Settings.CurrentRegister.Id != 0 && !string.IsNullOrEmpty(Settings.CurrentRegister.Name))
                {
                    ShopGeneralDto shops = shopService.GetLocalShops();
                    //Task.Run(async () => { shops = await shopService.GetLocalShops(); }).Wait();

                    if (shops != null && shops.Shop != null && shops.Shop.TenantId != 0)
                    {
                        App.Instance.IsRequiredEnterSaleDataReload = true;

                        // if (HikePOS.MainPage.entersalepage != null)
                        //     HikePOS.MainPage.entersalepage = null;

                        // if (HikePOS.MainPage.parksalepage != null)
                        //     HikePOS.MainPage.parksalepage = null;

                        // if (HikePOS.MainPage.settingpage != null)
                        //     HikePOS.MainPage.settingpage = null;

                        // if (HikePOS.MainPage.adminpage != null)
                        //     HikePOS.MainPage.adminpage = null;

                        // if (HikePOS.MainPage.cashRegisterpage != null)
                        //     HikePOS.MainPage.cashRegisterpage = null;


                        if (string.IsNullOrEmpty(Settings.StoreCulture))
                        {
                            Settings.StoreCulture = "en";
                        }

                        if (string.IsNullOrEmpty(Settings.StoreCurrencySymbol))
                        {
                            Settings.StoreCurrencySymbol = "$";
                        }

                        Extensions.SetCulture(Settings.StoreCulture.ToLower());
                        MainPage = new AppShell();
                    }
                    else
                    {
                        MainPage = new NavigationPage(new SettingUpOutletPage());
                    }
                }
                else
                {
                    LoginUserPage loginpage = new LoginUserPage();
                    //loginpage.ViewModel.isOpenOutletRegister = true;
                    MainPage = new NavigationPage(loginpage);
                }
            }
            else
            {
                MainPage = new NavigationPage(new LoginUserPage());
            }
        }
        else
        {
            MainPage = new NavigationPage(new LoginUserPage());

        }
    }

    public void Stop()
    {
        IsTimerActive = false;
        Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
    }

    protected override void OnStart()
    {
#if DEBUG
        // App center app name: Hike APP
        AppCenter.Start("ios=d7ab1572-89ca-40a7-bc85-f07517e335dd;" + "uwp={Your UWP App secret here};" +
               "android=321b3cfb-cb40-4107-9446-2f7e0cc90f6e",
                typeof(Crashes));
#else
        AppCenter.Start("ios=6d08b666-46de-42c5-904d-202786d0a1f8;" + "uwp={Your UWP App secret here};" +
            "android=39ac137d-8813-493d-9308-216bf577563a",
            typeof(Crashes));
#endif


        //Ticket:#10295
        Task.Run(async () =>
        {
            await UpdateStocksAsync();
            await CheckIfRegisterClosed();
        });

        Task.Run(async () =>
       {
           if (!string.IsNullOrEmpty(Settings.HikePayStoreId))
           {
               var nadaTapToPay = DependencyService.Get<INadaTapToPay>();
               nadaTapToPay.AuthorizeSdk(Settings.HikePayStoreId);
               var intializeResponse = await nadaTapToPay.InitializeSdkManually();
               if (!nadaTapToPay.IsInitialized)
               {
                   App.Instance.Hud.DisplayToast(intializeResponse.SaleToPOIResponse.TransactionResponse.Response.ErrorMessage, Colors.Red, Colors.White);
               }
            //    else
            //    {
            //        var diagnosisResponse = await nadaTapToPay.Diagnosis();
            //    }
           }
       });

    }

    protected override void OnSleep()
    {
        
    }

    protected override void OnResume()
    {
        try
        {
            if (_navigationService.IsFlyoutPage && _navigationService.Navigation.ModalStack.Count > 0)
            {
                if (_navigationService.Navigation.ModalStack.Last() is CameraScanPage scanPage)
                {
                    scanPage.SetCamera();
                }
            }

            //Method Was commented when paypal crash issue fixed
            Task.Run(async () =>
            {
                await UpdateStocksAsync(false);
                await CheckIfRegisterClosed();
            });



        }
        catch (Exception ex)
        {
            ex.Track();
        }
    }

    //Start.Added by rupesh to stock update when app enter in foreground
    //Ticket start:#10295
    //Ticket start:#91894 Sync only new data.by rupesh
    private async Task UpdateStocksAsync(bool NotOnlyUpdateStock = true)
    {
        try
        {
            //Ticket start:#27164 iOS - Unauthorised Login.by rupesh
            if (string.IsNullOrEmpty(Settings.AccessToken))
            {
                return;
            }
            //Ticket start:#27164 .by rupesh

            //ProductSocks
            ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
            ProductServices productService = new ProductServices(productApiService);
            LastSyncService objLastSyncService = new LastSyncService();
            LastSyncDto UpdatedLastSyncDto = objLastSyncService.GetLastSyncTime();
            if (UpdatedLastSyncDto == null)
            {
                UpdatedLastSyncDto = new LastSyncDto();

            }
            //Ticket start:#14146 Product stock displaying 0 though available in product listing page. by rupesh
            //Ticket start :#14312,#14272 iOS - Products showing 0 price in iPad.by rupesh
            // var beforeTimeofBackgroundSyncProductStock = DateTime.UtcNow;
            // var productstocks = await productService.GetRemoteProductStocks(Fusillade.Priority.Background, true, Settings.SelectedOutletId, "", UpdatedLastSyncDto.LastBackgroundProductStockDataSyncTime?.ToUniversalTime());
            //Ticket End:#14312,#14272. by rupesh
            //Ticket End:#14146. by rupesh

            ApiService<ITaxApi> TaxApiService = new ApiService<ITaxApi>();
            var taxServices = new TaxServices(TaxApiService);
            var taxes = taxServices.GetLocalTaxes();


            //ProductSocks
            ObservableCollection<ProductOutletDto_POS> productstocks;
            var beforeTimeofSyncProductStock = DateTime.UtcNow;
            if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
            {
                productstocks = await productService.GetRemoteProductStocks(Fusillade.Priority.Background, true, Settings.SelectedOutletId, "", UpdatedLastSyncDto.LastProductStockDataSyncTime?.ToUniversalTime());
            }
            else
            {
                productstocks = await productService.GetRemoteProductStocksLambda(Fusillade.Priority.Background, true, Settings.SelectedOutletId, "", UpdatedLastSyncDto.LastProductStockDataSyncTime?.ToUniversalTime());
            }
            //Set updated product stock to respective products
            // await productService.UpdateProductStocks(productStocks,taxes);
            UpdatedLastSyncDto.LastProductStockDataSyncTime = beforeTimeofSyncProductStock;
            if (NotOnlyUpdateStock)
            {
                //Products
                var beforeTimeodSyncProduct = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    var products = await productService.GetRemoteProducts(Fusillade.Priority.Background, true, Settings.SelectedOutletId, "", UpdatedLastSyncDto.LastProductDataSyncTime?.ToUniversalTime());
                }
                else
                {

                    var products = await productService.GetRemoteProductsLambda(Fusillade.Priority.Background, true, Settings.SelectedOutletId, "", UpdatedLastSyncDto.LastProductDataSyncTime?.ToUniversalTime(), taxes);
                }
                UpdatedLastSyncDto.LastProductDataSyncTime = beforeTimeodSyncProduct;
            }

            if (productstocks != null && productstocks.Count > 0)
            {
                await productService.UpdateProductStocks(productstocks, taxes);
                if (_navigationService.CurrentPage != null)
                {
                    //Ticket start:#14146 Product stock displaying 0 though available in product listing page. by rupesh
                    //Ticket start :#14312,#14272 iOS - Products showing 0 price in iPad.by rupesh
                    // UpdatedLastSyncDto.LastBackgroundProductStockDataSyncTime = beforeTimeofBackgroundSyncProductStock;
                    objLastSyncService.CreateUpdateLastSyncData(UpdatedLastSyncDto);
                    //Ticket End:#14312,#14272. by rupesh
                    //Ticket End:#14146. by rupesh
                }
            }
        }
        catch (Exception ex)
        {
            ex.Track();
        }




    }
    //Ticket end:#91894 .by rupesh
    //Edn.Added by rupesh

    //Ticket start:#55624 Integrate API to check reregister is opened or closed.by rupesh
    private async Task CheckIfRegisterClosed()
    {
        try
        {
            //Ticket start:#27164 iOS - Unauthorised Login.by rupesh
            if (string.IsNullOrEmpty(Settings.AccessToken))
            {
                return;
            }
            //Ticket start:#27164 .by rupesh
            if (Settings.CurrentRegister?.Registerclosure == null)
            {
                return;
            }

            if (outletServices == null)
                outletServices = new OutletServices(outletApiService);
            var response = await outletServices.CheckIfRegisterClosed(Fusillade.Priority.Background, Settings.CurrentRegister.Registerclosure.Id);
            if (response)
            {
                Application.Current.Dispatcher.Dispatch(async () =>
                {
                    await App.Alert.ShowAlert(LanguageExtension.Localize("Warning_Title"), LanguageExtension.Localize("RegisterCloserModifiedText"), "Ok");

                    if (_navigationService.RootPage != null || _navigationService.CurrentPage != null)
                    {
                        var page = _navigationService.RootPage;
                        if(page == null)
                            page = _navigationService.CurrentPage;
                        BaseViewModel pageModel = (BaseViewModel)page.BindingContext;
                        using (new Busy(pageModel, true))
                        {
                            try
                            {
                                var objOutletSync = new OutletSync();
                                await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: true, ResetAfterUppdate: false);
                                EnterSalePage.DataUpdated = true;
                                if (_navigationService.RootPage.BindingContext is EnterSaleViewModel vm)
                                {
                                    vm.IsOpenRegister = Settings.CurrentRegister.IsOpened;
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                        };
                    }
                    else if (!_navigationService.IsFlyoutPage && _navigationService.GetCurrentPage != null)
                    { 
                        BaseViewModel pageModel = (BaseViewModel)_navigationService.GetCurrentPage.BindingContext;
                        using (new Busy(pageModel, true))
                        {
                            try
                            {
                                var objOutletSync = new OutletSync();
                                await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: true, ResetAfterUppdate: false);
                                EnterSalePage.DataUpdated = true;
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                        };
                    }
                });

            }

        }
        catch (Exception ex)
        {
            ex.Track();
        }




    }
    //Ticket end:#55624 .by rupesh

    public void RegisterAppServices()
    {
        DependencyService.Register<IKeyboardHelper>();
        DependencyService.Register<IScanApiService, ScanApiService>();
        DependencyService.Register<INadaPayTerminalRemoteAppService, NadaPayTerminalRemoteAppService>();
        DependencyService.Register<INadaPayTerminalLocalAppService, NadaPayTerminalLocalAppService>();
#if ANDROID
        DependencyService.Register<IMultilingual, MultilingualImplementation>();
        DependencyService.Register<IHUDProvider, HUDProvider>();
        DependencyService.Register<IKeyboardHelper, iOSKeyboardHelper>();
        DependencyService.Register<IImageServices, ImageServices>();
        DependencyService.Register<IGetTimeZoneService, GetTimeZoneInfo>();
        DependencyService.Register<IExceptionParsing, ExceptionParsing>();
        DependencyService.Register<IPlayAndVibrate, PlayAndVibrate>();
#endif
    }
}
