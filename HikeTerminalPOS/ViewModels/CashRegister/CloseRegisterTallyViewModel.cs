using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using System.Linq;
using System.Collections.ObjectModel;
using SPIClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HikePOS.Enums;
using HikePOS.Services.Payment;
using HikePOS.Models.Payment;
using Newtonsoft.Json;
using Fusillade;

namespace HikePOS.ViewModels
{
    public class CloseRegisterTallyViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        public EventHandler<bool> Closed;

        ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
		OutletServices outletServices;

		ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
		SaleServices saleService;

		ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
		CustomerServices customerService;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        private string _note { get; set; }
        public string Note { get { return _note; } set { _note = value; SetPropertyChanged(nameof(Note)); } }

        RegisterclosureDto _Registerclosure { get; set; }
		public RegisterclosureDto Registerclosure { get { return _Registerclosure; } set { _Registerclosure = value; SetPropertyChanged(nameof(Registerclosure)); } }
        OpenCashCalculatorPage openCashCalculatorPage;
        

        public ICommand OpenCashCalculatorCommand { get; }

        #region LifeCycle
        public CloseRegisterTallyViewModel()
        {
            outletServices = new OutletServices(OutletApiService);
			saleService = new SaleServices(saleApiService);
			customerService = new CustomerServices(customerApiService);
            paymentService = new PaymentServices(paymentApiService);

            OpenCashCalculatorCommand = new Command<RegisterclosuresTallyDto>(OpenCashCalculator);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            if (IsClosePopup == true)
            {
                IsClosePopup = false;
                return;
            }
           
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsOpenPopup == true)
            {
                IsOpenPopup = false;
                return;
            }

        }

        #endregion


        #region Command
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand CloseRegisterCommand => new Command(CloseRegisterTapped);
        public ICommand RegisteredTotalCommand => new Command<int>(RegisteredTotalTapped);

        #endregion

        #region Command Execution

        private void RegisteredTotalTapped(int PaymentOptionId)
        {
             UpadateRegisterclosuresTally(PaymentOptionId);
        }

        public void CloseTapped()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    {
                        if (_navigationService.IsFlyoutPage)
                        {
                            var lastpage = _navigationService.NavigatedPage;
                            if (lastpage != null && lastpage is CashRegisterPage baseContentPage)
                            {
                                baseContentPage.ViewModel.IsClosePopup = true;
                            }
                        }
                        await NavigationService.PopModalAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void CloseRegisterTapped()
        {
            if (!string.IsNullOrWhiteSpace(Note))
            {
                Registerclosure.Notes = Note;
            }

            var result = await CloseRegister();
            if (result)
            {
                Closed?.Invoke(this, result);
                CloseTapped();
            }
        }

        #endregion

        public async Task<bool> CloseRegister(){
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

                        RegisterDto tempRegisterDto = Settings.CurrentRegister;
                        tempRegisterDto.Registerclosure = Registerclosure;
                        tempRegisterDto.LastCloseDateTime = DateTime.Now.ToUniversalTime();

                        RegisterDto registerDto =  await outletServices.CloseRegister(Fusillade.Priority.UserInitiated, tempRegisterDto);
                        if (registerDto != null)
                        {
                            Settings.CurrentRegister = registerDto;

                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CloseRegisterSuccessMessage"), Colors.Green, Colors.White);

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
                catch(Exception ex)
                {
                    ex.Track();
                }
            };
            return result;
        }
    
        async void OpenCashCalculator(RegisterclosuresTallyDto registerclosuresTally)
        {
            if (App.Instance.IsInternetConnected)
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
                                registerclosuresTally = (RegisterclosuresTallyDto)e;
                                registerclosuresTally.RegisteredTotal = registerclosuresTally.CashCalculatorTotal;
                                //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                                registerclosuresTally.StrRegisteredTotal = registerclosuresTally.CashCalculatorTotal.ToString("C");
                                //Ticket end:#29444 .by rupesh
                            }

                            Registerclosure.RegisterclosuresTallys.Where(x => x.Id == registerclosuresTally.Id).All(x =>
                              {

                                  x = registerclosuresTally;
                                  return true;
                              });
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
    
        public void UpadateRegisterclosuresTally(int id)
        {
            try
            {

                RegisterclosuresTallyDto registerclosuresTally = Registerclosure?.RegisterclosuresTallys?.FirstOrDefault(x => x.PaymentOptionId == id);
                if (registerclosuresTally != null)
                {
                    if(!string.IsNullOrEmpty(registerclosuresTally.StrRegisteredTotal))
                        //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                        registerclosuresTally.RegisteredTotal = decimal.Parse(registerclosuresTally.StrRegisteredTotal,System.Globalization.NumberStyles.Currency);
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
            catch(Exception ex)
            {
                ex.Track();
            }
        }

    }
}
