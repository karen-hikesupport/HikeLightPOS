using System.Diagnostics;
using System.Windows.Input;
using HikePOS.Services;
using HikePOS.Helpers;
using HikePOS.Models;
using Fusillade;
using System.Collections.ObjectModel;
using System.Web;

namespace HikePOS.ViewModels
{

	public class LoginUserViewModel : BaseViewModel
	{

		private readonly static INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		ApiService<IShopApi> shopApiService = new ApiService<IShopApi>();
		public ShopServices shopService;

		ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
		public OutletServices outletService;

		ApiService<ISubscriptionAPI> SubscriptionApiService = new ApiService<ISubscriptionAPI>();
		SubscriptionSevice subscriptionSevice;

		ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
		UserServices userService;

		ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
		SaleServices saleService;

		ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
		CustomerServices customerService;
		ApiService<IAccountApi> accountApiService = new ApiService<IAccountApi>();
		AccountServices accountService;

		public Entry DigitOne { get; set; }
		private string _oTPEntry;
		public string OTPEntry
		{
			get { return _oTPEntry; }
			set
			{
				_oTPEntry = value;
				SetPropertyChanged(nameof(OTPEntry));
			}
		}

		private string _oTPOne;
		public string OTPOne
		{
			get { return _oTPOne; }
			set
			{
				_oTPOne = value;
				SetPropertyChanged(nameof(OTPOne));
			}
		}

		private string _oTPTwo;
		public string OTPTwo
		{
			get { return _oTPTwo; }
			set
			{
				_oTPTwo = value;
				SetPropertyChanged(nameof(OTPTwo));
			}
		}

		private string _oTPThree;
		public string OTPThree
		{
			get { return _oTPThree; }
			set
			{
				_oTPThree = value;
				SetPropertyChanged(nameof(OTPThree));
			}
		}

		private string _oTPFour;
		public string OTPFour
		{
			get { return _oTPFour; }
			set
			{
				_oTPFour = value;
				SetPropertyChanged(nameof(OTPFour));
			}
		}
		// private string _oTPFive;
		// public string OTPFive
		// {
		// 	get { return _oTPFive; }
		// 	set
		// 	{
		// 		_oTPFive = value;
		// 		SetPropertyChanged(nameof(OTPFive));
		// 	}
		// }

		// private string _oTPSix;
		// public string OTPSix
		// {
		// 	get { return _oTPSix; }
		// 	set
		// 	{
		// 		_oTPSix = value;
		// 		SetPropertyChanged(nameof(OTPSix));
		// 	}
		// }

		public ICommand OTPCommand { get; }

		public LoginUserViewModel()
		{
			EnterSalePage.DataUpdated = true;
			OTPCommand = new Command(async () => await ValidateOTP(this));
			accountService = new AccountServices(accountApiService);
			if (true)
			{
				Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.ASYTest;
				Settings.AccessTokenClientId = ServiceConfiguration.ASYAccessToken_Client_Id;
				Settings.AccessTokenClientSecretId = ServiceConfiguration.ASYAccessToken_Client_Secret;
			}
			else
			{
				Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.Live;
				Settings.AccessTokenClientId = ServiceConfiguration.AccessToken_Client_Id;
				Settings.AccessTokenClientSecretId = ServiceConfiguration.AccessToken_Client_Secret;
			}
		}
		public override void OnAppearing()
		{
			base.OnAppearing();
			GetMerchantCode();
		}
		private async Task ValidateOTP(BaseViewModel viewmodel)
		{
			try
			{
				using (new Busy(this, true))
				{
					OTPEntry = OTPEntry?.Trim();
					if (!string.IsNullOrEmpty(OTPEntry))
					{
						//int pin = Convert.ToInt32(OTPEntry);
						/*if (!string.IsNullOrEmpty(Settings.TempTenantId))
						{

							//Settings.IsAppVerified = true;
							//Settings.IsLoginPage = false;

							var result = await storeVerificationRepository.VeifyStorePin(pin);

							if (result != null && result.result.verification && result.success)
							{
								Settings.IsAppVerified = true;
								Settings.TenantId = Settings.TempTenantId;
								Settings.PairPin = OTPEntry;
								Settings.RegisterId = result.result.registerId.ToString();
								if (DeviceInfo.Idiom == DeviceIdiom.Phone)
									Application.Current.Windows[0].Page = new EnterSalePhonePage();
								else
									Application.Current.Windows[0].Page = new EnterSalePage();

							}
							else if (result != null && !result.success && string.IsNullOrEmpty(result.error?.message))
							{
								ClearOTP();
								//Settings.IsAppVerified = false;
								App.Instance.Hud.DisplayToast(result.error.message, Colors.Red, Colors.White);
								return;
							}
							else
							{
								ClearOTP();
								//Settings.IsAppVerified = false;
								App.Instance.Hud.DisplayToast("You have entered wrong pairing code.", Colors.Red, Colors.White);
								return;
							}
						}
						else
						{
							ClearOTP();
							App.Instance.Hud.DisplayToast("Tenant Not found", Colors.Red, Colors.White);
							return;
						}*/
						shopService = new ShopServices(shopApiService);
						outletService = new OutletServices(outletApiService);
						subscriptionSevice = new SubscriptionSevice(SubscriptionApiService);
						userService = new UserServices(userApiService);
						customerService = new CustomerServices(customerApiService);
						saleService = new SaleServices(saleApiService);
                       
						await Login(this, OTPEntry, subscriptionSevice, saleService, customerService, shopService, outletService, userService);

					}
					else
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("VerificationCode_Empty"), Colors.Red, Colors.White);
						return;
					}
				}
			}
			catch (Exception ex)
			{
				ClearOTP();
				ex.Track();
				App.Instance.Hud.DisplayToast("Something went wrong", Colors.Red, Colors.White);
				Debug.WriteLine("ValidateOTP" + ex.ToString());
			}
			finally
			{
				DigitOne?.Unfocus();
			}
		}

		void ClearOTP()
		{
			OTPOne = string.Empty;
			OTPTwo = string.Empty;
			OTPThree = string.Empty;
			OTPFour = string.Empty;
			// OTPFive = string.Empty;
			// OTPSix = string.Empty;
		}

		public async Task Login(BaseViewModel viewmodel, string userPin, SubscriptionSevice subscriptionSevice, SaleServices saleService, CustomerServices customerService, ShopServices shopService, OutletServices outletService, UserServices userService)
		{

			using (new Busy(viewmodel, true))
			{
				#region Login_Validation

				if (!App.Instance.IsInternetConnected)
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter store name!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
					return;
				}
				if (Settings.TenantId == 0)
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter password!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Verify that your store is registered with HikePay"));
					return;
				}
				#endregion


				//Request model object
				LoginByPinModel loginByPinModel = new LoginByPinModel()
				{
					TenantId = Settings.TenantId,
					UserPin = userPin,
					VerifyKey = ServiceConfiguration.ASY_To_GetAccessToken_VerifyKey
				};

				ResponseModel<string> LoginResponse = new ResponseModel<string>
				{
					success = false,
					error = new Error { message = "Something went wrong" },
				};

				try
				{

					LoginResponse = await accountService.LoginByPin(Fusillade.Priority.UserInitiated, loginByPinModel);

					if (LoginResponse != null)
					{
						if (LoginResponse.success)
						{
							//Navigate to outlet register screen						
							Settings.AccessToken = "bearer " + LoginResponse.result;

							// if(!Settings.RememberBrowser || (Settings.RememberBrowser && Settings.TenantName != StoreWebAddress?.Trim()))
							// {
							// 	await NavigationService.PushAsync(new TwoFactorPage(StoreWebAddress,Email, LoginResponse.result));
							// 	return;
							// }
							// else
							// {
							var Subscription = await subscriptionSevice.GetRemoteAccountDetail(Priority.UserInitiated, true);
							Settings.Subscription = Subscription;

							var isExpired = await Subscription.IsExpired();
							if (isExpired)
							{
								return;
							}

							var UserDetail = await userService.GetRemoteUserByUserPin(Priority.UserInitiated, userPin, true);
							if (UserDetail == null || UserDetail.Id < 1 || string.IsNullOrEmpty(UserDetail.EmailAddress))
							{
								return;
							}
							var loginInformation = await accountService.GetCurrentLoginInformations(Priority.UserInitiated);
							if (loginInformation != null && loginInformation.success && loginInformation.result?.tenant?.databaseName != null)
							{
								Settings.CurrentDatabaseName = loginInformation.result.tenant.databaseName;
								Settings.CurrentDatabaseType = loginInformation.result.tenant.databaseType;
							}

							if ((!UserDetail.Outlets.Any(x => x.OutletId == Settings.SelectedOutletId)) || (Settings.LoginUserPin == userPin))
							{
								var tmpInvoices = saleService.GetOfflineInvoices();
								var tmpCustomer = customerService.GetUnSyncCustomer();

								if ((tmpInvoices != null && tmpInvoices.Count > 0) || (tmpCustomer != null && tmpCustomer.Count > 0))
								{
									var username = "";
									if (Settings.CurrentUser != null && !string.IsNullOrEmpty(Settings.CurrentUser.FullName))
									{
										username = Settings.CurrentUser.FullName;
									}
									else
									{
										username = Settings.CurrentUserEmail;
									}

									var decline = await App.Alert.ShowAlert("Warning", "Still, You have some unsync data in your previous store \"" + Settings.TenantName + "\". Which is logged by \"" + Settings.CurrentUser.FullName + "\". Continue will lost your unsync data.", "Continue", "Cancel");
									if (!decline)
										return;
								}

								var realm = RealmService.GetRealm();
								realm.Write(() =>
								{
									realm.RemoveAll();
								});
								//CommonQueries.InvalidateAll();
								//BlobCache.ApplicationName = "HikePOS_" + StoreWebAddress;
								Settings.CurrentUserEmail = UserDetail.EmailAddress;
								Settings.SelectedOutletId = 0;
								Settings.SelectedOutletName = string.Empty;
								Settings.CurrentRegister = null;
								//Settings.PaypalToken = string.Empty;
								//Settings.TyroIntegrationkey= string.Empty;
								Settings.CurrentUser = UserDetail;
								await GetOutlets(shopService, outletService, true);
								Settings.LoginUserPin = userPin;
							}
							else
							{
								Settings.CurrentUser = UserDetail;
								//BlobCache.ApplicationName = "HikePOS_" + StoreWebAddress;
								await AutoLogin(shopService, outletService);
							}
							//}
						}

						if (LoginResponse.error != null)
						{
							//Notify error to user
							//Application.Current.MainPage.DisplayAlert("Error", LoginResponse.error.message, "Ok");
							App.Instance.Hud.DisplayToast(LoginResponse.error.message, Colors.Red, Colors.White);
						}
					}
					else
					{
						//Application.Current.MainPage.DisplayAlert("Error", "Something went wrong", "Ok");
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
					}
				}
				catch (Exception ex)
				{
					ex.Track();
				}
				finally
				{
					loginByPinModel = null;
					LoginResponse = null;
					// accountApiService = null;
					// accountService = null;
				}

			}
		}

		public static async Task AutoLogin(ShopServices shopService, OutletServices outletService)
		{
			try
			{


				if (!App.Instance.IsInternetConnected)
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter store name!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
					return;
				}
				if (!string.IsNullOrEmpty(Settings.AccessToken))
				{
					//BlobCache.ApplicationName = "HikePOS_" + Settings.TenantName;
					if (Settings.SelectedOutletId != 0 && !string.IsNullOrEmpty(Settings.SelectedOutletName) && Settings.CurrentRegister != null && Settings.CurrentRegister.Id != 0 && !string.IsNullOrEmpty(Settings.CurrentRegister.Name))
					{

						var shops = shopService.GetLocalShops();
						if (shops != null && shops.Shop != null && shops.Shop.TenantId != 0)
						{
							// if (MainPage.entersalepage != null)
							// 	MainPage.entersalepage = null;

							// if (MainPage.parksalepage != null)
							// 	MainPage.parksalepage = null;

							// if (MainPage.settingpage != null)
							// 	MainPage.settingpage = null;

							// if (MainPage.adminpage != null)
							// 	MainPage.adminpage = null;

							// if (MainPage.cashRegisterpage != null)
							// 	MainPage.cashRegisterpage = null;



							if (string.IsNullOrEmpty(Settings.StoreCulture))
							{
								Settings.StoreCulture = "en";
							}

							if (string.IsNullOrEmpty(Settings.StoreCurrencySymbol))
							{
								Settings.StoreCurrencySymbol = "$";
							}

							//Ticket start:#33790 iPad :: Feature request :: User specific language.by rupesh
							if (Settings.CurrentUser.AllowOverrideLangugeSettingOverGeneralSetting)
							{
								Settings.StoreCulture = Settings.CurrentUser.Language;
								var objGetTimeZoneService = DependencyService.Get<IGetTimeZoneService>();
								Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.CurrentUser.IANATimeZone);
								Settings.StoreTimeZoneInfoId = Settings.CurrentUser.IANATimeZone;

								if (Settings.StoreCulture.ToLower() == "ar" || Settings.StoreCulture.ToLower() == "ar-kw")
								{
									Settings.SymbolForDecimalSeperatorForNonDot = ",";
								}

							}
							//Ticket end:#33790 .by rupesh

							Extensions.SetCulture(Settings.StoreCulture.ToLower());

							App.Instance.MainPage = new AppShell();
						}
						else
						{
							if (_navigationService.GetCurrentPage is NavigationPage)
							{
								await ((NavigationPage)_navigationService.GetCurrentPage).PushAsync(new SettingUpOutletPage());
							}
							else
							{
								App.Instance.MainPage = new NavigationPage(new SettingUpOutletPage());
							}
						}
					}
					else
					{
						await GetOutlets(shopService, outletService, true);
					}
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}

		}

		public static async Task GetOutlets(ShopServices shopService, OutletServices outletService, bool checkShopBasicDetail = false)
		{
			try
			{
				if (checkShopBasicDetail)
				{
					ShopGeneralDto shopResponse = await shopService.GetRemoteShops(Priority.UserInitiated, false);
					if (shopResponse == null || shopResponse.Shop == null || string.IsNullOrEmpty(shopResponse.Shop.FirstName) || string.IsNullOrEmpty(shopResponse.Shop.LastName))
					{


						var accountBasicDetailPage = new AccountBasicDetailPage();

						await _navigationService.GetCurrentPage.Navigation.PushModalAsync(accountBasicDetailPage);

						accountBasicDetailPage.ViewModel.AccountBasicDetailAdded += async (object sender, bool e) =>
						{
							await accountBasicDetailPage.ViewModel.Close();
							await GetOutletsData(shopService, outletService);
						};
					}
					else
					{
						Settings.CountryCode = shopResponse?.Address?.Country;
						await GetOutletsData(shopService, outletService);
					}
				}
				else
				{

					await GetOutletsData(shopService, outletService);
				}

			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		static async Task GetOutletsData(ShopServices shopService, OutletServices outletService)
		{
			try
			{
				//Outlet details
				ObservableCollection<OutletDto_POS> outletResponse = await outletService.GetRemoteOutlets(Priority.UserInitiated, true);
				if (outletResponse != null)
				{
					if (outletResponse.Count == 1 && outletResponse[0] != null && outletResponse[0].OutletRegisters != null && outletResponse[0].OutletRegisters.Count == 1)
					{
						Settings.SelectedOutletId = outletResponse[0].Id;
						Settings.SelectedOutletName = outletResponse[0].Title;
						//Ticket start:#38783 iPad: Feature request - Register's Name in Process Sale.by rupesh
						Settings.SelectedOutlet = outletResponse[0];
						//Ticket end:#38783 .by rupesh
						Settings.CurrentRegister = outletResponse[0].OutletRegisters[0];

						var shop = shopService.GetLocalShops();
						if (shop != null && shop.Shop != null && shop.Shop.Id != 0)
						{
							Settings.TenantId = shop.Shop.TenantId;
							Settings.StoreId = shop.Shop.Id;
							Settings.StoreName = shop.Shop.TradingName;
							if (shop.GeneralRule != null)
							{
								Settings.StoreGeneralRule = shop.GeneralRule;
								Settings.StoreShopDto = shop.Shop;
							}
							if (shop.ZoneAndFormat != null)
							{
								Settings.StoreZoneAndFormatDetail = shop.ZoneAndFormat;
								var objGetTimeZoneService = DependencyService.Get<IGetTimeZoneService>();
								Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(shop.ZoneAndFormat.IanaTimeZone);
								Settings.StoreTimeZoneInfoId = shop.ZoneAndFormat.IanaTimeZone;
								Settings.StoreCurrencySymbol = shop.ZoneAndFormat.CurrencySymbol;
								Settings.StoreCurrencyCode = shop.ZoneAndFormat.Currency;
								//Ticket start:#26913 iOS - Separator (comma) Not Applied.by rupesh
								Settings.SymbolForDecimalSeperatorForNonDot = shop.ZoneAndFormat.SymbolForDecimalSeperatorForNonDot;
								//Ticket start:#26913 .by rupesh

								Settings.StoreCulture = shop.ZoneAndFormat.Language;
								if (!string.IsNullOrEmpty(shop.ZoneAndFormat.Language) && shop.ZoneAndFormat.Language.Contains("3d"))
								{
									shop.ZoneAndFormat.Language = shop.ZoneAndFormat.Language.Replace("3d", "");
								}

								if (string.IsNullOrEmpty(Settings.StoreCulture))
								{
									Settings.StoreCulture = "en";
								}

								if (string.IsNullOrEmpty(Settings.StoreCurrencySymbol))
								{
									Settings.StoreCurrencySymbol = "$";
								}
								//Ticket start:#33790 iPad :: Feature request :: User specific language.by rupesh
								if (Settings.CurrentUser.AllowOverrideLangugeSettingOverGeneralSetting)
								{
									Settings.StoreCulture = Settings.CurrentUser.Language;
									Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.CurrentUser.IANATimeZone);
									Settings.StoreTimeZoneInfoId = Settings.CurrentUser.IANATimeZone;

									if (Settings.StoreCulture.ToLower() == "ar" || Settings.StoreCulture.ToLower() == "ar-kw")
									{
										Settings.SymbolForDecimalSeperatorForNonDot = ",";
									}

								}
								//Ticket end:#33790 .by rupesh

								Extensions.SetCulture(Settings.StoreCulture.ToLower());
							}

							// if (MainPage.entersalepage != null)
							// 	MainPage.entersalepage = null;

							// if (MainPage.parksalepage != null)
							// 	MainPage.parksalepage = null;

							// if (MainPage.settingpage != null)
							// 	MainPage.settingpage = null;

							// if (MainPage.adminpage != null)
							// 	MainPage.adminpage = null;

							// if (MainPage.cashRegisterpage != null)
							// 	MainPage.cashRegisterpage = null;

							App.Instance.MainPage = new AppShell();
						}
						else
						{

							if (_navigationService.GetCurrentPage == null || _navigationService.IsFlyoutPage)
							{
								App.Instance.MainPage = new NavigationPage(new SettingUpOutletPage());
							}
							else
							{
								if (_navigationService.GetCurrentPage is NavigationPage)
								{
									await ((NavigationPage)_navigationService.GetCurrentPage).PushAsync(new SettingUpOutletPage());
								}
								else
								{
									App.Instance.MainPage = new NavigationPage(new SettingUpOutletPage());
								}

							}
						}
					}
					else
					{

						var selectOutletRegisterPage = new SelectOutletRegisterPage();
						selectOutletRegisterPage.ViewModel.RegisterIsSelected += async (sender, e) =>
						{
							if (e)
							{
								await selectOutletRegisterPage.ViewModel.Close();
								//App.Instance.MainPage = new SettingUpOutletPage();

								if (_navigationService.GetCurrentPage == null || _navigationService.IsFlyoutPage)
								{
									App.Instance.MainPage = new NavigationPage(new SettingUpOutletPage());
								}
								else
								{
									if (_navigationService.GetCurrentPage is NavigationPage)
									{
										await ((NavigationPage)_navigationService.GetCurrentPage).PushAsync(new SettingUpOutletPage());
									}
									else
									{
										App.Instance.MainPage = new NavigationPage(new SettingUpOutletPage());
									}
								}

							}
						};
						//await Task.Delay(10);
						selectOutletRegisterPage.ViewModel.SetStores(outletResponse);
						if (_navigationService.GetCurrentPage.Navigation.ModalStack != null && _navigationService.GetCurrentPage.Navigation.ModalStack.Count > 0 && _navigationService.GetCurrentPage.Navigation.ModalStack.Last() is SelectOutletRegisterPage)
							return;
						await _navigationService.GetCurrentPage.Navigation.PushModalAsync(selectOutletRegisterPage);
					}

				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}
		private async void GetMerchantCode()
		{
			try
			{
				try
				{
					var context = Android.App.Application.Context;

					string deviceName = Android.Provider.Settings.Global.GetString(
						context.ContentResolver,
						Android.Provider.Settings.Global.DeviceName
					);
					HikePOS.Helpers.Settings.TerminalId = deviceName;

				}
				catch (Exception ex)
				{
					SentrySdk.CaptureException(ex);

				}
			
				var terminalId = Settings.TerminalId;// "AMS1-000168232403847";
				Settings.TenantId = 8008;

				// var response = await DependencyService.Get<INadaPayTerminalLocalAppService>().DiagnosisRequestToTerminal(terminalId);
				// if (response?.PaymentStatusSuccess == true)
				// {
				// 	string additionalResponse = response.AdditonalResponse;
				// 	var parsed = HttpUtility.ParseQueryString(additionalResponse);
				// 	string merchantCode = parsed["merchantAccount"];
				// 	var merchantInfo = await accountService.GetTenantInfo(merchantCode, terminalId);
				// 	if (merchantInfo != null)
				// 		Settings.TenantId = merchantInfo.TenantId;
				// }

			}
			catch (Exception ex)
			{
				ex.Track();
				SentrySdk.CaptureException(ex);

			}

		}
	}

}
