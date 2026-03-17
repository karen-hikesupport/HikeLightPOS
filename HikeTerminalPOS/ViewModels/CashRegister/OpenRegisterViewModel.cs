using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
	public class OpenRegisterViewModel : BaseViewModel
	{
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
		OutletServices outletService;

        ApiService<IShopApi> ShopApiService = new ApiService<IShopApi>();
		ShopServices shopService;

        #region Properties
        bool IsExecuting = false;

        public event EventHandler<bool> OpenRegisterResult;

        //Ticket start:#30955 iOS - Denomination Entering When Opening Registers.by rupesh
        OpenCashCalculatorPage openCashCalculatorPage;

        private string _note { get; set; }
        public string Note { get { return _note; } set { _note = value; SetPropertyChanged(nameof(Note)); } }

        string _OpeningAmount { get; set; }
        public string OpeningAmount { get { return _OpeningAmount; } set { _OpeningAmount = value; SetPropertyChanged(nameof(OpeningAmount)); } }

        
        RegisterclosureDto _Registerclosure { get; set; }
        public RegisterclosureDto Registerclosure { get { return _Registerclosure; } set { _Registerclosure = value; SetPropertyChanged(nameof(Registerclosure)); } }

        RegisterclosuresTallyDto _registerclosuresTallyDto { get; set; }
        public RegisterclosuresTallyDto RegisterclosuresTallyDto { get { return _registerclosuresTallyDto; } set { _registerclosuresTallyDto = value; SetPropertyChanged(nameof(RegisterclosuresTallyDto)); } }

        RegisterclosuresTallyDto tallyDto;
        //Ticket end:#30955 .by rupesh
        
        //Start #94427 Disable cash option on a single register By Pratik
        bool _isActiveCashPayment;
        public bool IsActiveCashPayment { get { return _isActiveCashPayment; } set { _isActiveCashPayment = value; SetPropertyChanged(nameof(IsActiveCashPayment)); } }
        //end #94427 By Pratik


        #endregion

        public OpenRegisterViewModel()
        {
            Title = "Open register";
            outletService = new OutletServices(outletApiService);
            shopService = new ShopServices(ShopApiService);
            Registerclosure = Settings.CurrentRegister.LastRegisterclosure;

        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            IsActiveCashPayment = Settings.CurrentRegister == null ? false : Settings.CurrentRegister.IsActiveCashPayment; //Start #94427 Disable cash option on a single register By Pratik
            if (IsClosePopup == true)
            {
                IsClosePopup = false;
                return;
            }
            
            Note = string.Empty;
            OpeningAmount = string.Empty;
            if (Registerclosure != null)
            {
                var registerclosuresTally = Registerclosure.RegisterclosuresTallys.FirstOrDefault();
                if (registerclosuresTally?.RegisterClosureTallyDenominations != null)
                    registerclosuresTally.RegisterClosureTallyDenominations = new ObservableCollection<RegisterClosureTallyDenominationDto>((registerclosuresTally.RegisterClosureTallyDenominations.Select(c => { c.Quantity = 0; return c; }).ToList()));

            }

        }

        #region Command
        public ICommand OpenCashCalculatorCommand => new Command<RegisterclosuresTallyDto>(OpenCashCalculator);
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand SubmitCommand => new Command(SubmitTapped);
        #endregion

        #region Command Execution

        async void OpenCashCalculator(RegisterclosuresTallyDto registerclosuresTally)
        {
            if (EventCallRunning && !IsActiveCashPayment)  //Start #94427 Disable cash option on a single register By Pratik
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(Microsoft.Maui.Devices.DeviceInfo.Platform == DevicePlatform.Android ? 1000 : 50).Wait();
                EventCallRunning = false;
            });
            if (App.Instance.IsInternetConnected)
            {
                try
                {

                    if (Registerclosure == null || (Registerclosure?.RegisterclosuresTallys != null && Registerclosure.RegisterclosuresTallys.Count <= 0))
                    {
                        var register = await outletService.GetRemoteRegisterById(Priority.UserInitiated, true, Settings.CurrentRegister.Id);

                        Registerclosure = register.LastRegisterclosure;
                        if (Registerclosure == null)
                        {
                            Registerclosure = new RegisterclosureDto();
                            Registerclosure.RegisterclosuresTallys = new ObservableCollection<RegisterclosuresTallyDto> { new RegisterclosuresTallyDto() };
                        }
                        Settings.CurrentRegister.LastRegisterclosure = Registerclosure;
                    }

                    if (Registerclosure.RegisterclosuresTallys == null || (Registerclosure?.RegisterclosuresTallys != null &&  Registerclosure?.RegisterclosuresTallys.Count <= 0))
                    {
                        Registerclosure.RegisterclosuresTallys = new ObservableCollection<RegisterclosuresTallyDto> { new RegisterclosuresTallyDto() };
                    }


                    if (openCashCalculatorPage == null)
                    {


                        openCashCalculatorPage = new OpenCashCalculatorPage();
                        openCashCalculatorPage.ViewModel.Closed += (sender, e) =>
                        {
                            IsClosePopup = true;
                            Registerclosure.RegisterclosuresTallys.Where(x => x.Id == tallyDto.Id).All(x =>
                            {
                                x.RegisterClosureTallyDenominations = tallyDto.RegisterClosureTallyDenominations;
                                return true;
                            });
                        };
                        openCashCalculatorPage.ViewModel.Saved += async (sender, e) =>
                        {
                            IsClosePopup = true;
                            if (e != null)
                            {
                                OpeningAmount = e.CashCalculatorTotal.ToString();
                                RegisterclosuresTallyDto = e;

                                Registerclosure.RegisterclosuresTallys.Where(x => x.Id == e.Id).All(x =>
                                {
                                    x = e;
                                    return true;
                                });
                            }
                        };


                    }

                    using (new Busy(this, true))
                    {

                        registerclosuresTally = Registerclosure.RegisterclosuresTallys.FirstOrDefault();

                        //var n = s.RegisterClosureTallyDenominations;

                        var lstRegisterClosureTallyDenomination = new ObservableCollection<RegisterClosureTallyDenominationDto>();
                        if (registerclosuresTally.RegisterClosureTallyDenominations != null)
                            lstRegisterClosureTallyDenomination = new ObservableCollection<RegisterClosureTallyDenominationDto>((registerclosuresTally.RegisterClosureTallyDenominations.OrderBy(x => x.DenominationValue).ToList()));

                        registerclosuresTally.RegisterClosureTallyDenominations = lstRegisterClosureTallyDenomination;
                        
                        openCashCalculatorPage.ViewModel.RegisterclosuresTally = registerclosuresTally;

                        //Ticket start:#30955 iOS - Denomination Entering When Opening Registers.by rupesh
                        openCashCalculatorPage.ViewModel.LoadDenomination(true);
                        //Ticket end:#30955 .by rupesh
                    }
                    var templst = openCashCalculatorPage.ViewModel.RegisterclosuresTally;
                    tallyDto = templst.Copy();
                    templst = null;
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

        public async void CloseTapped()
        {
            try
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
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void SubmitTapped()
        {
            if (IsExecuting)
                return;

            try
            {
                IsExecuting = true;
                if (App.Instance.IsInternetConnected)
                {
                    double famount = 0;
                    if ((!string.IsNullOrEmpty(OpeningAmount) && double.TryParse(OpeningAmount, out famount)) || OpeningAmount == "")
                    {
                        bool result = await OpenRegister(famount, Note);
                        if (result)
                        {
                            OpenRegisterResult?.Invoke(this, result);
                            CloseTapped();
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ValidAmount"));
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"));
                }
            }
            catch
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ValidAmount"));
            }
            finally
            {
                IsExecuting = false;
            }
        }

        #endregion

        #region Methods

        public async Task<bool> OpenRegister(double OpeningAmount, string note) {
			using (new Busy(this, true))
			{
                var res = await outletService.GetRemoteRegisterById(Fusillade.Priority.UserInitiated, true, Settings.CurrentRegister.Id);
				if (res != null)
				{
					//#30495 iOS -Change in Register API for display app option
					if (res != null)
					{
						Settings.CustomerAppConfigFrom = res.CustomerDisplayConfigureType.ToString();
						Settings.CustomerAppPin = res.CustomerDisplayConfigurePin;
					}
					//#30495 iOS -Change in Register API for display app option


					if (res.Registerclosure != null && res.Registerclosure.StartDateTime != null && res.Registerclosure.EndDateTime == null)
                    {
                        await App.Alert.ShowAlert(LanguageExtension.Localize("AlreadyOpenRegisterErrorMessage"),"", "Ok");
                        return true;
                    }
                    else
                    {
                        //Ticket start:#30955 iOS - Denomination Entering When Opening Registers.by rupesh
                       // var registerclosuresTally = Registerclosure?.RegisterclosuresTallys?.FirstOrDefault();
                        OpenRegisterInput objOpenRegisterInput = new OpenRegisterInput();
						objOpenRegisterInput.registerId = Settings.CurrentRegister.Id;
						objOpenRegisterInput.amount = OpeningAmount;
						objOpenRegisterInput.note = note;
                        if(RegisterclosuresTallyDto != null)
                        objOpenRegisterInput.registerClosureTallyDenominations = RegisterclosuresTallyDto.RegisterClosureTallyDenominations;
                        //Ticket end:#30955 .by rupesh
                        var registerDto = await outletService.OpenRegister(Fusillade.Priority.UserInitiated, objOpenRegisterInput);

                        if (registerDto != null)
                        {
                            Settings.CurrentRegister = registerDto;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
					}
				}
                else
                {
                    return false;
                }

            }
		}

		public async void Reset()
		{
			var decline = await App.Alert.ShowAlert("Reset", "Are you sure you want to reset all data? Reseting it will also delete unsync data", "Yes", "No");
			if (!decline)
				return;

			using (new Busy(this, true))
			{
				try
				{
                    await Task.Delay(10);
                    var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.RemoveAll();
                    });
                    //CommonQueries.InvalidateAll();
                    Settings.SelectedOutletId = 0;
					Settings.SelectedOutletName = string.Empty;
					Settings.CurrentRegister = null;
					///Settings.PaypalToken = string.Empty;
					await LoginViewModel.GetOutlets(shopService, outletService);
                }
				catch (Exception ex)
				{
					ex.Track();
				}
			};
		}

		public async Task SyncData()
		{
			using (new Busy(this, true))
			{
                var objOutletSync = new OutletSync();
				await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: true, ResetAfterUppdate: true);
				EnterSalePage.DataUpdated = true;
                WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All"));
            };
		}

        #endregion
    }
}
