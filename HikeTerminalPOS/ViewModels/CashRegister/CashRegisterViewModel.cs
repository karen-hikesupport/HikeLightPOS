using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Fusillade;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Model;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Models.Shop;
using HikePOS.Pages;
using HikePOS.Services;
using HikePOS.Services.Payment;
using Newtonsoft.Json;
using SPIClient;

namespace HikePOS.ViewModels
{
    public class CashRegisterViewModel : BaseViewModel
    {
        #region Declare Services
        private readonly  INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
        OutletServices outletServices;

        ApiService<IShopApi> ShopApiService = new ApiService<IShopApi>();
        ShopServices shopService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;


        #endregion

        #region Declare Sliders
        OpenRegisterPage openRegisterpage;
        AddCashInCashOutPage addcashincashoutpage;
        OpenCashCalculatorPage openCashCalculatorPage;
        public dynamic cashRegisterPage;
        #endregion

        #region Declare Commands
        public ICommand ChangeOutletRegisterCommand { get; }
        public ICommand AddRemoveCashCommand { get; }
        public ICommand CloseRegisterCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CashInOutCommand { get; }
        public ICommand PaymentSummaryCommand { get; }
        public ICommand SalesSummaryCommand { get; }
        public ICommand TaxSummaryCommand { get; }
        public ICommand PaymentTallyClickCommand { get; }
        public ICommand SliderMenuHandleClickedCommand => new Command(sliderMenuHandle_Clicked);
        public ICommand OpenCashCalculatorCommand { get; }

        #endregion

        #region Properties

        public string SalesColumn => LanguageExtension.Localize("SalesText") + " " + Settings.StoreCurrencySymbol;
        public string TaxColumn => LanguageExtension.Localize("TaxText") + " " + Settings.StoreCurrencySymbol;


        RegisterclosureDto _Registerclosure { get; set; }
        public RegisterclosureDto Registerclosure { get { return _Registerclosure; } set { _Registerclosure = value; SetPropertyChanged(nameof(Registerclosure)); } }

        bool _isOpenRegister { get; set; } = false;
        public bool IsOpenRegister { get { return _isOpenRegister; } set { _isOpenRegister = value; SetPropertyChanged(nameof(IsOpenRegister)); } }

        bool _isLastOpenRegister { get; set; } = true;
        public bool IsLastOpenRegister { get { return _isLastOpenRegister; } set { _isLastOpenRegister = value; SetPropertyChanged(nameof(IsLastOpenRegister)); } }

        bool _allowAddRemoveCash { get; set; } = false;
        public bool AllowAddRemoveCash { get { return _allowAddRemoveCash; } set { _allowAddRemoveCash = value; SetPropertyChanged(nameof(AllowAddRemoveCash)); } }

        bool _isCashInOut { get; set; } = false;
        public bool IsCashInOut { get { return _isCashInOut; } set { _isCashInOut = value; SetPropertyChanged(nameof(IsCashInOut)); } }

        bool _isPaymentSummary { get; set; } = false;
        public bool IsPaymentSummary { get { return _isPaymentSummary; } set { _isPaymentSummary = value; SetPropertyChanged(nameof(IsPaymentSummary)); } }

        bool _isSalseSummary { get; set; } = false;
        public bool IsSalesSummary { get { return _isSalseSummary; } set { _isSalseSummary = value; SetPropertyChanged(nameof(IsSalesSummary)); } }

        bool _isTaxSummary { get; set; } = false;
        public bool IsTaxSummary { get { return _isTaxSummary; } set { _isTaxSummary = value; SetPropertyChanged(nameof(IsTaxSummary)); } }

        bool _IsPaymentTally { get; set; } = false;
        public bool IsPaymentTally { get { return _IsPaymentTally; } set { _IsPaymentTally = value; SetPropertyChanged(nameof(IsPaymentTally)); } }

        bool _allowOpenCloseRegister { get; set; } = false;
        public bool AllowOpenCloseRegister { get { return _allowOpenCloseRegister; } set { _allowOpenCloseRegister = value; SetPropertyChanged(nameof(AllowOpenCloseRegister)); } }

        bool _allowPrint { get; set; } = false;
        public bool AllowPrint { get { return _allowPrint; } set { _allowPrint = value; SetPropertyChanged(nameof(AllowPrint)); } }

        //Ticket start:#14410 iOS - Register Summary Printed Wrongly.by rupesh
        bool _enablePrint { get; set; } = false;
        public bool EnablePrint { get { return _enablePrint; } set { _enablePrint = value; SetPropertyChanged(nameof(EnablePrint)); } }
        //Ticket end:#14410. by rupesh

        //Ticket start:#30341 Tyro payments are not recorded in the register causing discrepancy issue.by rupesh
        bool _EnableOpenCloseRegister { get; set; } = false;
        public bool EnableOpenCloseRegister { get { return _EnableOpenCloseRegister; } set { _EnableOpenCloseRegister = value; SetPropertyChanged(nameof(EnableOpenCloseRegister)); } }
        //Ticket end:#30341. by rupesh
        bool _notAllowOpenRegister { get; set; } = false;
        public bool NotAllowOpenRegister { get { return _notAllowOpenRegister; } set { _notAllowOpenRegister = value; SetPropertyChanged(nameof(NotAllowOpenRegister)); } }

        private string _note { get; set; }
        public string Note { get { return _note; } set { _note = value; SetPropertyChanged(nameof(Note)); } }

        // Start #91261 iOS:FR counted amount upon closing by Pratik
        bool _showExpectedColumn { get; set; } = false;
        public bool ShowExpectedColumn { get { return _showExpectedColumn; } set { _showExpectedColumn = value; SetPropertyChanged(nameof(ShowExpectedColumn)); } }
        // End #91261 by Pratik

        //Start #94427 Disable cash option on a single register By Pratik
        bool _isActiveCashPayment;
        public bool IsActiveCashPayment { get { return _isActiveCashPayment; } set { _isActiveCashPayment = value; SetPropertyChanged(nameof(IsActiveCashPayment)); } }
        //end #94427 By Pratik

        #endregion

        #region Constructor
        public CashRegisterViewModel()
        {

            #region Assign Services 
            outletServices = new OutletServices(OutletApiService);
            shopService = new ShopServices(ShopApiService);
            customerService = new CustomerServices(customerApiService);
            saleService = new SaleServices(saleApiService);
            paymentService = new PaymentServices(paymentApiService);

            #endregion

            #region Assign Commands
            ChangeOutletRegisterCommand = new Command(ChangeOutletRegister);
            AddRemoveCashCommand = new Command(AddRemoveCash);
            CloseRegisterCommand = new Command(CloseRegister);
            CashInOutCommand = new Command(CashInOutClick);
            PaymentSummaryCommand = new Command(PaymentSummaryClick);
            SalesSummaryCommand = new Command(SalesSummaryClick);
            TaxSummaryCommand = new Command(TaxSummaryClick);
            PaymentTallyClickCommand = new Command(PaymentTallyClick);
            OpenCashCalculatorCommand = new Command<RegisterclosuresTallyDto>(OpenCashCalculator);
            #endregion
        }
        #endregion

        #region On Page Appearing
        bool firsttimecall = true;
        public override void OnAppearing()
        {
            base.OnAppearing();
          
            if (firsttimecall)
            {
                firsttimecall = false;
               if (Microsoft.Maui.Devices.DeviceInfo.Idiom == DeviceIdiom.Tablet)
                  IsCashInOut = true;
               else
                  IsPaymentTally = true;
            }

            EventCallRunning = false;
            if (IsClosePopup == true)
            {
                IsClosePopup = false;
                return;
            }
            Task.Run(async () =>
            {
                await Task.Delay(500);
                MainThread.BeginInvokeOnMainThread(async ()=> 
                {

                    SetIsActiveCash();
                    //Ticket start:#14410 iOS - Register Summary Printed Wrongly.by rupesh
                    EnablePrint = false;
                    //Ticket end:#14410.by rupesh

                    SetPrinterButton();

                    //Ticket start:#30341 Tyro payments are not recorded in the register causing discrepancy issue.by rupesh
                    EnableOpenCloseRegister = false;
                    if (Settings.GrantedPermissionNames != null)
                    {
                        ShowExpectedColumn = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn"); // Start #91261 iOS:FR counted amount upon closing by Pratik
                        AllowOpenCloseRegister = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister");
                    }
                    LoadLocalCloseRegister();
                    await LoadCloseRegister();
                    // Start #91261 iOS:FR counted amount upon closing by Pratik
                    if (Registerclosure != null && Registerclosure.RegisterclosuresTallys != null 
                    && Registerclosure.RegisterclosuresTallys.Any(a=> a.paymentOptionType == Enums.PaymentOptionType.Cash  && string.IsNullOrEmpty(a.StrRegisteredTotal) && !ShowExpectedColumn))
                    {
                    EnableOpenCloseRegister = false;
                    }
                    else
                        EnableOpenCloseRegister = true;
                    // End #91261 by Pratik
                    //Ticket end:#30341. by rupesh
                    //Ticket start:#74634 iOS FR: Pin Restriction.by rupesh
                    NotAllowOpenRegister = (!IsOpenRegister && !Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.OpenRegister"));
                    //Ticket end:#74634 .by rupesh
                });
            });
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        //Start #94427 Disable cash option on a single register By Pratik
        void SetIsActiveCash()
        { 
            IsActiveCashPayment = Settings.CurrentRegister == null ? false : Settings.CurrentRegister.IsActiveCashPayment;
            if (Microsoft.Maui.Devices.DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsActiveCashPayment)
            {
                IsCashInOut = false;
                IsPaymentSummary = true;
            }
        }
        //end #94427 By Pratik


        // Start #91261 iOS:FR counted amount upon closing by Pratik     
        public void CheckAndSetEnableOpenCloseRegister()
        {
            EnableOpenCloseRegister = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.OpenRegister");
            if (IsOpenRegister && !ShowExpectedColumn)
            {
                if(Registerclosure?.RegisterclosuresTallys != null && Registerclosure.RegisterclosuresTallys.Any(a=>a.paymentOptionType == PaymentOptionType.Cash && a.StrRegisteredTotal == string.Empty))
                    EnableOpenCloseRegister = false;
                else
                    EnableOpenCloseRegister = true;
            }
        }
        // End #91261 by Pratik     
        #endregion

        #region Command Methods

        // Start #91261 iOS:FR counted amount upon closing by Pratik     
        public ICommand RegisteredTotalCommand => new Command<int>(RegisteredTotalTapped);
        public ICommand UnfocusedCommand => new Command<int>(UnfocusedTapped);
        
        public ICommand ShowSendEmailCommand => new Command(ShowSendEmail); //Start #92768 Pratik
        private void UnfocusedTapped(int PaymentOptionId)
        {
            if (IsOpenRegister && !ShowExpectedColumn)
            {
                if (Registerclosure?.RegisterclosuresTallys != null)
                {
                    var firstdata = Registerclosure.RegisterclosuresTallys.FirstOrDefault(a => a.paymentOptionType == PaymentOptionType.Cash && a.PaymentOptionId == PaymentOptionId);
                    if (firstdata != null)
                    {
                        if (string.IsNullOrEmpty(firstdata.StrRegisteredTotal))
                            EnableOpenCloseRegister = false;
                        else
                            EnableOpenCloseRegister = true;
                    }
                }
            }
            else if (IsOpenRegister)
                EnableOpenCloseRegister = true;
            else
                EnableOpenCloseRegister = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.OpenRegister");
        }
        private void RegisteredTotalTapped(int PaymentOptionId)
        {
            UnfocusedTapped(PaymentOptionId);
            UpadateRegisterclosuresTally(PaymentOptionId);
        }
        // End #91261 by Pratik     

        private void CashInOutClick()
        {
            IsCashInOut = !IsCashInOut;
        }
        private void PaymentSummaryClick()
        {
            IsPaymentSummary = !IsPaymentSummary;
        }
        private void SalesSummaryClick()
        {
            IsSalesSummary = !IsSalesSummary;
        }
        private void TaxSummaryClick()
        {
            IsTaxSummary = !IsTaxSummary;
        }
        private void PaymentTallyClick()
        {
            IsPaymentTally = !IsPaymentTally;
        }

        private void sliderMenuHandle_Clicked()
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
        async void OpenCashCalculator(RegisterclosuresTallyDto registerclosuresTally)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    if (openCashCalculatorPage == null)
                    {
                        openCashCalculatorPage = new OpenCashCalculatorPage();
                        openCashCalculatorPage.ViewModel.Saved += async (sender, e) =>
                        {
                            IsClosePopup = true;
                            if (e != null && Registerclosure != null && Registerclosure.RegisterclosuresTallys != null)
                            {
                                if(registerclosuresTally.CashCalculatorTotal != 0)
                                {
                                    registerclosuresTally = (RegisterclosuresTallyDto)e;
                                    registerclosuresTally.RegisteredTotal = registerclosuresTally.CashCalculatorTotal;
                                    //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                                    registerclosuresTally.StrRegisteredTotal = registerclosuresTally.CashCalculatorTotal.ToString("C");
                                    //Ticket end:#29444 .by rupesh

                                    Registerclosure.RegisterclosuresTallys.Where(x => x.Id == registerclosuresTally.Id).All(x =>
                                    {
                                        x = registerclosuresTally;
                                        return true;
                                    });
                                }
                            }

                           
                            using (new Busy(this, true))
                            {
                                await outletServices.CreateOrUpdateRegisterClosureDenomination(Fusillade.Priority.UserInitiated, registerclosuresTally.RegisterClosureTallyDenominations);
                            }
                        };
                    }
                    //openCashCalculatorPage.ViewModel.RegisterclosuresTally = registerclosuresTally;
                    using (new Busy(this, true))
                    {
                        var lstRegisterClosureTallyDenomination = new ObservableCollection<RegisterClosureTallyDenominationDto>();
                        if (registerclosuresTally.RegisterClosureTallyDenominations != null)
                            lstRegisterClosureTallyDenomination = new ObservableCollection<RegisterClosureTallyDenominationDto>((registerclosuresTally.RegisterClosureTallyDenominations.OrderBy(x => x.DenominationValue).ToList()));

                        registerclosuresTally.RegisterClosureTallyDenominations = lstRegisterClosureTallyDenomination;
                        openCashCalculatorPage.ViewModel.RegisterclosuresTally = registerclosuresTally;
                    }
                    IsOpenPopup = true;
                    IsClosePopup = true;
                    await NavigationService.PushModalAsync(openCashCalculatorPage);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }

        void OpenRegister()
        {
            try
            {
                //Ticket start:#74634 iOS FR: Pin Restriction.by rupesh
                if (NotAllowOpenRegister)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("YouDoNotHaveSufficientPermissionToOpenRegister"), Colors.Red, Colors.White);
                    return;
                }
                //Ticket end:#74634 .by rupesh
                if (openRegisterpage == null)
                {
                    openRegisterpage = new OpenRegisterPage();
                    //Ticket start:#39856 iPad: taking more time ton open register.by rupesh
                    openRegisterpage.ViewModel.OpenRegisterResult += (sender, e) =>
                    {
                        //LoadLocalCloseRegister();                        
                        _ = LoadCloseRegister();

                    };
                }
                NavigationService.PushModalAsync(openRegisterpage);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void CloseRegister()
        {
            if (App.Instance.IsInternetConnected)
            {
                if (!IsOpenRegister)
                {
                    OpenRegister();
                }
                else
                {
                    try
                    {
                           var confirmed = await App.Alert.ShowAlert("Close this register?","Just a reminder... If any discrepancies, don't forget to adjust actual counted box before closing the register.", "Continue", "Cancel");
                            if(!confirmed)
                            {
                                return;
                            }

                        using (new Busy(this, true))
                        {

                            var tmpInvoices = saleService.GetOfflineInvoices();

                            if (tmpInvoices != null && tmpInvoices.Count > 0)
                            {
                                var invoicenumbers = string.Join(", ", tmpInvoices.Select(x => x.Number));
                                var accept = await App.Alert.ShowAlert("Still, You have " + tmpInvoices.Count + " unsync sale data in your local database. Do you want to continue without sync? Continue will affect on close register summary.", "Unsync Sales: " + invoicenumbers, "Continue", "Cancel");
                                if (!accept)
                                {
                                    return;
                                }
                            }

                            var tmpCustomer = customerService.GetUnSyncCustomer();
                            if (tmpCustomer != null && tmpCustomer.Count > 0)
                            {
                                var customers = string.Join(", ", tmpCustomer.Select(x => x.FullName));
                                var accept = await App.Alert.ShowAlert("Still, You have " + tmpCustomer.Count + " unsync customer data in your local database. Do you want to continue without sync?", "Unsync Customers: " + customers, "Continue", "Cancel");
                                if (!accept)
                                {
                                    return;
                                }
                              
                            }


                            var result = await CallCloseRegister();
                            if (result)
                            {
                                IsOpenRegister = false;
                                AllowAddRemoveCash = false;
                                //Ticket  end:#44390 .by rupesh
                               CheckAndSetEnableOpenCloseRegister(); // Start #91261 iOS:FR counted amount upon closing by Pratik                            
                                _ = LoadCloseRegister(false);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }

        async void AddRemoveCash()
        {

            if (App.Instance.IsInternetConnected)
            {
                try
                {
                    if (addcashincashoutpage == null)
                    {
                        addcashincashoutpage = new AddCashInCashOutPage();
                        addcashincashoutpage.ViewModel.CashInCashOutAdded += async (sender, e) =>
                        {
                            AddCashInCashOutViewModel addCashInCashOutViewModel = (AddCashInCashOutViewModel)sender;
                            await addCashInCashOutViewModel.ClosePage();
                            await LoadCloseRegisterWithoutLoader();
                        };
                    }
                    addcashincashoutpage.ViewModel.Registerclosure = Registerclosure;
                    await NavigationService.PushModalAsync(addcashincashoutpage);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }

        }


        async void ChangeOutletRegister()
        {
            using (new Busy(this, true))
            {
                var confirmation = await App.Alert.ShowAlert("Confirmation", LanguageExtension.Localize("ResetAleartMessage"), "Yes", "No");
                if (confirmation)
                {
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {


                        /* Date: 17/02/2020
                         * Note : we add below new api call to associte iPad payment with other registers.
                         * After that we have got one ticket #9463: for multiple register with multiple payment option.
                         * as per this issue, we discussed with web team and decied to dont link iPad payment with all register.
                         * User has to configure payment register wise.
                         */

                        //if (Settings.IPadConfiguredPaymentID != null && Settings.IPadConfiguredPaymentID.Count > 0)
                        //{
                        //    bool isiPadPaymentlink = await IpadPaymentSyncWithServer();
                        //}


                        var tmpInvoices = saleService.GetOfflineInvoices();
                        var tmpCustomer = customerService.GetUnSyncCustomer();

                        if ((tmpInvoices != null && tmpInvoices.Count > 0) || (tmpCustomer != null && tmpCustomer.Count > 0))
                        {
                            var sync = await App.Alert.ShowAlert("Reset", LanguageExtension.Localize("DataSyncContinueMessage"), "Yes", "No");
                            if (sync)
                            {
                                OutletSync objOutletSync = new OutletSync();

                                await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: false, ResetAfterUppdate: false);
                            }
                        }

                        try
                        {


                            var realm = RealmService.GetRealm();
                            realm.Write(() =>
                            {
                                realm.RemoveAll();
                            });
                            //CommonQueries.InvalidateAll();
                            Settings.SelectedOutletId = 0;
                            Settings.SelectedOutletName = string.Empty;
                            Settings.CurrentRegister = null;

                            await LoginViewModel.GetOutlets(shopService, outletServices);
                            EnterSalePage.DataUpdated = true;
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }

        }
        #endregion

        #region Methods
        public void UpadateRegisterclosuresTally(int id)
        {
            try
            {
                RegisterclosuresTallyDto registerclosuresTally = Registerclosure?.RegisterclosuresTallys?.FirstOrDefault(x => x.PaymentOptionId == id);
                if (registerclosuresTally != null)
                {
                    if (!string.IsNullOrEmpty(registerclosuresTally.StrRegisteredTotal))
                        //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                        registerclosuresTally.RegisteredTotal = decimal.Parse(registerclosuresTally.StrRegisteredTotal, System.Globalization.NumberStyles.Currency);
                    //Ticket end:#29444 .by rupesh
                    else
                        registerclosuresTally.RegisteredTotal = 0;

                    Registerclosure.RegisterclosuresTallys.Where(x => x.Id == registerclosuresTally.Id).All(x =>
                    {
                        x = registerclosuresTally;
                        return true;
                    });
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        void LoadLocalCloseRegister()
        {
            using (new Busy(this, true))
            {
                try
                {
                    var res = Settings.CurrentRegister;
                    if (res != null)
                    {
                        if (res.Registerclosure != null && res.Registerclosure.StartDateTime != null && res.Registerclosure.EndDateTime == null)
                        {
                            Registerclosure = res.Registerclosure;
                            IsOpenRegister = true;
                        }
                        else
                        {
                            IsOpenRegister = false;
                            Registerclosure = res.LastRegisterclosure;
                        }
                    }
                    else
                    {
                        Registerclosure = new RegisterclosureDto();
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CloseRegisterLoadDataErrorMessage"), Colors.Red, Colors.White);
                    }
                }
                catch (Exception ex)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    ex.Track();
                }
                finally
                {
                     //Start #94427 Disable cash option on a single register By Pratik
                    if (Settings.GrantedPermissionNames != null && Settings.CurrentRegister != null)
                    {
                        AllowAddRemoveCash = IsOpenRegister && Settings.CurrentRegister.IsActiveCashPayment && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.CashInCashOut"));
                    }
                    if (!IsActiveCashPayment && Registerclosure?.RegisterclosuresTallys != null && Registerclosure.RegisterclosuresTallys.Any(a => a.paymentOptionType == PaymentOptionType.Cash))
                    {
                        Registerclosure.RegisterclosuresTallys.Remove(Registerclosure.RegisterclosuresTallys.First(a => a.paymentOptionType == PaymentOptionType.Cash));
                    }
                    //End #94427 By Pratik
                }
            }
        }

        async Task LoadCloseRegister(bool isLoadRegister = true)
        {
            using (new Busy(this, true))
            {
               await LoadCloseRegisterWithoutLoader(isLoadRegister);
            };
        }

        async Task LoadCloseRegisterWithoutLoader(bool isLoadRegister = true)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    var shouldUpdateRegisterClosure = false;
                    if (Registerclosure != null && Registerclosure.StartDateTime != null)
                    {
                        var curDateTime = DateTime.Now;
                        var diffDays = (curDateTime - Registerclosure.StartDateTime.Value).TotalDays;
                        shouldUpdateRegisterClosure = diffDays > 5;
                    }

                    if (shouldUpdateRegisterClosure)
                        await outletServices.UpdateRegisterClosure(Priority.Background, Registerclosure.Id);


                    //#36109 iPad: Tax collector was updated wrong in cash register.
                    RegisterclosureDto tempLastregisterclosureDto = null;
                    RegisterDto res = null;

                    if ((!IsOpenRegister || !isLoadRegister) && Registerclosure != null && Registerclosure.Id != 0)
                    {
                        tempLastregisterclosureDto = await outletServices.GetLastRegisterClosureById(Priority.UserInitiated, true, Registerclosure.Id);

                    }
                    //#36109 iPad: Tax collector was updated wrong in cash register.

                    res = await outletServices.GetRemoteRegisterById(Priority.UserInitiated, true, Settings.CurrentRegister.Id);
                    if (res != null)
                    {
                        //#36015 Ipad: missing information in register closer receipt.
                        //if (!isLoadRegister)
                        //    res.Registerclosure = Registerclosure;
                        //#36015 Ipad: missing information in register closer receipt.

                        if (tempLastregisterclosureDto != null)
                            res.LastRegisterclosure = tempLastregisterclosureDto;
                        //#29818 Net cash payment amount in Register summary

                        //first option



                        RegisterCashInOutDto openingAmount = null;
                        if (res.Registerclosure != null)
                        {
                            // openingAmount = res.Registerclosure.RegisterCashInOuts.Where(x => x.RegisterCashType == Enums.RegisterCashType.OpeningFloat).FirstOrDefault();
                            var registerTallys = res.Registerclosure.RegisterclosuresTallys.Where(x => !string.IsNullOrEmpty(x.PaymentOptionName));
                            res.Registerclosure.RegisterclosuresTallys = new ObservableCollection<RegisterclosuresTallyDto>(registerTallys);

                            foreach (var item in res.Registerclosure.RegisterclosuresTallys)
                            {
                                item.NetPaymentName = item.PaymentOptionName;
                                item.IsNetAmountDisplay = true;
                                if (item.paymentOptionType == Enums.PaymentOptionType.Cash)
                                {
                                    //Ticket start:#29818 Net cash payment amount in Register summary.by rupesh
                                    decimal netPayment = item.ActualTotal - res.Registerclosure.RegisterCashInOuts.Sum(x => x.Amount);
                                    //Ticket end:#29818 .by rupesh
                                    netPayment = Math.Round(netPayment, 2);

                                    item.NetPaymentName = item.NetPaymentName
                                        + " ( Net cash payment : "
                                        + Settings.StoreCurrencySymbol
                                        + netPayment.ToString() + " )";
                                }
                                // Start #91261 iOS:FR counted amount upon closing by Pratik
                                if (item.paymentOptionType == PaymentOptionType.Cash)
                                    item.RegisteredTotal = (item.RegisteredTotal == 0 && Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn")) ? item.ActualTotal : item.RegisteredTotal;
                                else
                                    item.RegisteredTotal = item.RegisteredTotal == 0 ? item.ActualTotal : item.RegisteredTotal;
                                //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                                // item.StrRegisteredTotal = item.RegisteredTotal.ToString("C");
                                if (Settings.GrantedPermissionNames != null && !Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn"))
                                {
                                    if (item.paymentOptionType == PaymentOptionType.Cash && !string.IsNullOrEmpty(item.RegisteredTotal.ToString("C")) && item.RegisteredTotal <= 0)
                                    {
                                        item.StrRegisteredTotal = string.Empty;
                                    }
                                    else
                                        item.StrRegisteredTotal = item.RegisteredTotal.ToString("C");
                                }
                                else
                                    item.StrRegisteredTotal = item.RegisteredTotal.ToString("C");
                                // End #91261 by Pratik
                                //Ticket end:#29444 .by rupesh

                            }

                        }
                        else
                        {
                            if (res.LastRegisterclosure != null)
                            {
                                //openingAmount = res.LastRegisterclosure.RegisterCashInOuts.Where(x => x.RegisterCashType == Enums.RegisterCashType.OpeningFloat).FirstOrDefault();

                                var registerTallys = res.LastRegisterclosure.RegisterclosuresTallys.Where(x => !string.IsNullOrEmpty(x.PaymentOptionName));
                                res.LastRegisterclosure.RegisterclosuresTallys = new ObservableCollection<RegisterclosuresTallyDto>(registerTallys);

                                foreach (var item in res.LastRegisterclosure.RegisterclosuresTallys)
                                {
                                    item.NetPaymentName = item.PaymentOptionName;
                                    item.IsNetAmountDisplay = true;
                                    if (item.paymentOptionType == Enums.PaymentOptionType.Cash)
                                    {
                                        //Ticket start:#29818 Net cash payment amount in Register summary.by rupesh
                                        decimal netPayment = item.ActualTotal - res.LastRegisterclosure.RegisterCashInOuts.Sum(x => x.Amount);
                                        //Ticket end:#29818 .by rupesh
                                        netPayment = Math.Round(netPayment, 2);

                                        item.NetPaymentName = item.NetPaymentName
                                            + " ( Net cash payment : "
                                            + Settings.StoreCurrencySymbol
                                            + netPayment.ToString() + " )";
                                    }
                                    //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                                    // Start #91261 iOS:FR counted amount upon closing by Pratik
                                    if (item.paymentOptionType == PaymentOptionType.Cash)
                                        item.RegisteredTotal = (item.RegisteredTotal == 0 && Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn")) ? item.ActualTotal : item.RegisteredTotal;
                                    else
                                        item.RegisteredTotal = item.RegisteredTotal == 0 ? item.ActualTotal : item.RegisteredTotal;
                                    if (Settings.GrantedPermissionNames != null && !Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn"))
                                    {
                                        if (item.paymentOptionType == PaymentOptionType.Cash && !string.IsNullOrEmpty(item.RegisteredTotal.ToString("C")) && item.RegisteredTotal <= 0)
                                        {
                                            item.StrRegisteredTotal = string.Empty;
                                        }
                                        else
                                            item.StrRegisteredTotal = item.RegisteredTotal.ToString("C");
                                    }
                                    else
                                        item.StrRegisteredTotal = item.RegisteredTotal.ToString("C");
                                    // End #91261 by Pratik
                                    //Ticket end:#29444 .by rupesh

                                }
                            }
                        }

                        //#29818 Net cash payment amount in Register summary
                        //#30495 iOS -Change in Register API for display app option
                        if (res != null)
                        {
                            Settings.CustomerAppConfigFrom = res.CustomerDisplayConfigureType.ToString();
                            Settings.CustomerAppPin = res.CustomerDisplayConfigurePin;

                            ////Settings.DisplayAppConfig = new Dictionary<string, string>();
                            //Settings.DisplayAppConfig.Add("DisplayConfigureType", res.CustomerDisplayConfigureType.ToString());
                            //Settings.DisplayAppConfig.Add("DisplayAppPin", res.CustomerDisplayConfigurePin);
                        }
                        //#30495 iOS -Change in Register API for display app option

                        if (res.Registerclosure != null && res.Registerclosure?.StartDateTime != null && res.Registerclosure?.EndDateTime == null)
                        {
                            Registerclosure = res.Registerclosure;
                            IsOpenRegister = true;
                            CheckAndSetEnableOpenCloseRegister(); // Start #91261 iOS:FR counted amount upon closing by Pratik
                        }
                        else
                        {
                            IsOpenRegister = false;
                            Registerclosure = res.LastRegisterclosure;
                            //Ticket start:#74634 iOS FR: Pin Restriction.by rupesh
                            NotAllowOpenRegister = (!IsOpenRegister && !Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.OpenRegister"));
                            //Ticket end:#74634 .by rupesh                           

                        }
                        Settings.CurrentRegister = res;
                        SetIsActiveCash();
                        
                        //Start #94427 Disable cash option on a single register By Pratik
                        if (!IsActiveCashPayment && res.Registerclosure?.RegisterclosuresTallys != null && res.Registerclosure.RegisterclosuresTallys.Any(a => a.paymentOptionType == PaymentOptionType.Cash))
                        {
                            res.Registerclosure.RegisterclosuresTallys.Remove(res.Registerclosure.RegisterclosuresTallys.First(a => a.paymentOptionType == PaymentOptionType.Cash));
                        }
                        //End #94427 By Pratik

                    }
                    else
                    {
                        Registerclosure = new RegisterclosureDto();
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    }

                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    LoadLocalCloseRegister();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception  while LoadCloseRegister : " + ex.ToString());
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
            }
            finally
            {
                //Start #94427 Disable cash option on a single register By Pratik
                if (Settings.GrantedPermissionNames != null && Settings.CurrentRegister != null)
                {
                    AllowAddRemoveCash = IsOpenRegister && Settings.CurrentRegister.IsActiveCashPayment && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.CashInCashOut"));
                }
                //end #94427 By Pratik
                if (Registerclosure?.RegisterclosuresTallys == null || (Registerclosure?.RegisterclosuresTallys != null && Registerclosure.RegisterclosuresTallys.Count <= 0))
                {
                    IsLastOpenRegister = false;
                    AllowPrint = false;
                }
                else
                {
                    IsLastOpenRegister = true;
                    SetPrinterButton();
                }

            }
        
        }

        void SetPrinterButton()
        {
            if (Settings.GetCachePrinters != null)
            {
                AllowPrint = Settings.GetCachePrinters.Any(x => x.PrimaryReceiptPrint);

                //Ticket start:#14410 iOS - Register Summary Printed Wrongly.by rupesh
                EnablePrint = AllowPrint;
                //Ticket end:#14410.by rupesh
            }
        }

        public async Task<bool> IpadPaymentSyncWithServer()
        {
            try
            {
                bool result = false;
                PaymentSyncDto paymentSyncDto = new PaymentSyncDto()
                {
                    registerClosureId = Settings.CurrentRegister.Registerclosure.Id,
                    paymentOptionIds = Settings.IPadConfiguredPaymentID
                };
                IpadPaymentSyncResponse ipadPaymentSyncResponse = await shopService.IpadPaymentSyncWithServer(Fusillade.Priority.Background, paymentSyncDto);

                if (ipadPaymentSyncResponse != null)
                {
                    result = ipadPaymentSyncResponse.success;
                    if (result)
                        Settings.IPadConfiguredPaymentID = null;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        //Start #92768 Pratik
        public async void ShowSendEmail()
        {
            try
            {
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EnableTenantEmail)
                {
                    // string body = "Dear, \n\nThis is the  register closure summary for Store " + Registerclosure?.RegisterName + "."
                    //     + "\n\nPlease find the attached file for detailed information regarding this closure.";
                    // if (Registerclosure.EndDateTime.HasValue)
                    // {
                    //     body = "Dear, \n\nThis is the  register closure summary for Store " + Registerclosure?.RegisterName + " from "
                    //     + Registerclosure.StartDateTime.Value.ToString("MMMM dd, yyyy, hh:mm tt") + " to "
                    //     + Registerclosure.EndDateTime.Value.ToString("MMMM dd, yyyy, hh:mm tt") + "."
                    //     + "\n\nPlease find the attached file for detailed information regarding this closure.";
                    // }
                    if (Registerclosure != null)
                    {
                        EmailSendPopupPage emailSendPopupPage = new EmailSendPopupPage("Register summary – " + Registerclosure.RegisterName);
                        emailSendPopupPage.FillFormed += FillFormedSendEmail;
                        await NavigationService.PushModalAsync(emailSendPopupPage);
                    }
                }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Your plan doesn't support this feature. To start using this feature, update to the Plus plan.", Colors.Red, Colors.White);
                    }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private async void FillFormedSendEmail(object sender, List<object> e)
        {
            try
            {
                if (Registerclosure == null)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    return;
                }

                ((EmailSendPopupPage)sender).ViewModel.ClosePopupCommand.Execute(null);
                using (new Busy(this, true))
                {
                    SendRegisterClosureDto sendRegisterClosure = new SendRegisterClosureDto();
                    sendRegisterClosure.registerClosureDetail = new RegisterClosureDetail()
                    {
                        registerCashInOuts = Registerclosure.RegisterCashInOuts.ToList(),
                        registerclosuresTallys = Registerclosure.RegisterclosuresTallys.ToList(),
                        taxList = Registerclosure.TaxList.ToList(),
                        closeBy = Registerclosure.CloseBy,
                        closeByUser = Registerclosure.CloseByUser,
                        difference = Registerclosure.Difference ?? 0,
                        id = Registerclosure.Id,
                        refNumber = Registerclosure.refNumber,
                        registerId = Registerclosure.RegisterId,
                        registerName = Registerclosure.RegisterName,
                        outletRegisterName = "",
                        registerClosureTransactionDetailsDto = Registerclosure.registerClosureTransactionDetailsDto,
                        totalCompletedSales = Registerclosure.TotalCompletedSales ?? 0,
                        totalOnAccountSales = Registerclosure.TotalOnAccountSales ?? 0,
                        totalDiscounts = Registerclosure.TotalDiscounts ?? 0,
                        totalLayBySales = Registerclosure.TotalLayBySales ?? 0,
                        totalParkedSales = Registerclosure.TotalParkedSales ?? 0,
                        totalPayments = Registerclosure.TotalPayments ?? 0,
                        totalRefunds = Registerclosure.TotalRefunds ?? 0,
                        totalSales = Registerclosure.TotalSales ?? 0,
                        totalTax = Registerclosure.TotalTax ?? 0,
                        totalTip = Registerclosure.TotalTip ?? 0,
                        transactionDetail = Registerclosure.registerClosureTransactionDetailsDto != null ? JsonConvert.SerializeObject(Registerclosure.registerClosureTransactionDetailsDto) : null,
                        endDateTime = Registerclosure.EndDateTime,
                        outletName = Registerclosure.OutletName,
                        isActive = Registerclosure.IsActive,
                        notes = Registerclosure.Notes,
                        startByUser = Registerclosure.StartByUser,
                        startBy = Registerclosure.StartBy,
                        startDateTime = Registerclosure.StartDateTime,
                        thirdPartySyncStatus = 0,
                        merchant_receipt = Registerclosure.merchant_receipt,
                        registerClosureTallyDenominations = null
                    };
                    sendRegisterClosure.body = string.Format(EmailTemplateHTML.EmailBody, Registerclosure.RegisterName, Registerclosure.StartDateTime.HasValue ? Registerclosure.StartDateTime.Value.ToString("MMMM dd, yyyy, hh:mm tt") : "", Registerclosure.EndDateTime.HasValue ? Registerclosure.EndDateTime.Value.ToString("MMMM dd, yyyy, hh:mm tt") : "");
                    sendRegisterClosure.subject = e[1].ToString();

                    sendRegisterClosure.emailTemplate = new EmailTemplate()
                    {
                        body = sendRegisterClosure.body,
                        subject = sendRegisterClosure.subject,
                        name = "Register summary",
                        isActive = true,  
                        templateTypeId = EmailTemplateType.RegisterReport,
                    };

                    sendRegisterClosure.emailList = new List<EmailList>();
                    var emailstrs = e[0] as List<string>;
                    foreach (var item in emailstrs)
                    {
                        sendRegisterClosure.emailList.Add(new EmailList() { emailId = item });
                    }

                    await outletServices.SendRegisterClosureEmail(Fusillade.Priority.UserInitiated, sendRegisterClosure);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //End #92768 Pratik

        public async Task<bool> CallCloseRegister()
        {
            bool result = false;
            using (new Busy(this, true))
            {
                try
                {
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {
                        var tmpInvoices = saleService.GetOfflineInvoices();
                        var tmpCustomer = customerService.GetUnSyncCustomer();

                        if ((tmpInvoices != null && tmpInvoices.Count > 0) || (tmpCustomer != null && tmpCustomer.Count > 0))
                        {
                            var decline = await App.Alert.ShowAlert("Close Register", LanguageExtension.Localize("DataSyncContinueMessage"), "Yes", "No");
                            if (decline)
                            {
                                OutletSync objOutletSync = new OutletSync();
                                await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: false, ResetAfterUppdate: false);
                            }
                        }
                        // Start #91261 iOS:FR counted amount upon closing by Pratik
                        if (Registerclosure?.RegisterclosuresTallys != null && Registerclosure.RegisterclosuresTallys.Any(a => a.StrRegisteredTotal == string.Empty))
                        {
                            Registerclosure.RegisterclosuresTallys.Where(a => a.StrRegisteredTotal == string.Empty).ForEach(a => a.StrRegisteredTotal = "0");
                        }
                        // End #91261 by Pratik

                        RegisterDto tempRegisterDto = Settings.CurrentRegister;
                        tempRegisterDto.Registerclosure = Registerclosure;
                        tempRegisterDto.LastCloseDateTime = DateTime.Now.ToUniversalTime();

                        RegisterDto registerDto = await outletServices.CloseRegister(Fusillade.Priority.UserInitiated, tempRegisterDto);
                        if (registerDto != null)
                        {
                            Settings.CurrentRegister = registerDto;

                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CloseRegisterSuccessMessage"), Colors.Green, Colors.White);
                            result = true;

                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            ;
            return result;
        }
        #endregion
    }




}
