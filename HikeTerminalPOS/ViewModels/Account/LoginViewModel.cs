using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.UserControls;
using Newtonsoft.Json;

namespace HikePOS.ViewModels
{
	public class LoginViewModel : BaseViewModel
	{
		#region Declaration
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
		#endregion

		#region Properties
		public bool isOpenOutletRegister { get; set; } = false;
		string _storeWebAddress { get; set; }
		string _email { get; set; }
		string _password { get; set; }

		public string StoreWebAddress
		{
			get { return _storeWebAddress; }
			set { _storeWebAddress = value; SetPropertyChanged(nameof(StoreWebAddress)); }
		}

		public string Email
		{
			get { return _email; }
			set { _email = value; SetPropertyChanged(nameof(Email)); }
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; SetPropertyChanged(nameof(Password)); }
		}

		#endregion

		#region Command
		public ICommand LoginCommand { get; }
		public ICommand NavigateToForgotPasswordCommand { get; }
		public ICommand NavigateToRegisterCommand { get; }
		public ICommand ReturnCommand => new Command<BorderLessEntry>(FocusReturn);

		public void FocusReturn(BorderLessEntry borderLessEntry)
		{
			borderLessEntry.Focus();
		}
		#endregion

		#region Life Cycle
		public LoginViewModel()
		{
			//StoreWebAddress = "demoplumslingerie";
			//Email = "support@hikeup.com";
			//Password = "123qwe";

			EnterSalePage.DataUpdated = true;
			Title = LanguageExtension.Localize("Login_ButtonText");

			//Assigning all command here
			LoginCommand = new Command(async () => await Login(this, StoreWebAddress, Email, Password, subscriptionSevice, saleService, customerService, shopService, outletService, userService));
			NavigateToForgotPasswordCommand = new Command(NavigateToForgotPassword);
			NavigateToRegisterCommand = new Command(NavigateToRegister);
			shopService = new ShopServices(shopApiService);
			outletService = new OutletServices(outletApiService);
			subscriptionSevice = new SubscriptionSevice(SubscriptionApiService);
			userService = new UserServices(userApiService);
			customerService = new CustomerServices(customerApiService);
			saleService = new SaleServices(saleApiService);
		}

		public async override void OnAppearing()
		{
			base.OnAppearing();
			if (isOpenOutletRegister)
			{
				isOpenOutletRegister = false;
				await LoginViewModel.GetOutlets(shopService, outletService, true);
			}
		}
		#endregion

		#region Login
		public async Task Login(BaseViewModel viewmodel, string StoreWebAddress, string Email, string Password, SubscriptionSevice subscriptionSevice, SaleServices saleService, CustomerServices customerService, ShopServices shopService, OutletServices outletService, UserServices userService)
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

				if (string.IsNullOrEmpty(StoreWebAddress) || string.IsNullOrWhiteSpace(StoreWebAddress))
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter store name!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreName_EmptyMessage"));
					return;
				}


				if (!Regex.IsMatch(StoreWebAddress, RegxValues.DefaultRegX, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter valid store name!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreName_InValidMessage"));
					return;
				}

				if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter email!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_EmptyMessage"));
					return;
				}

				if (!Regex.IsMatch(Email, RegxValues.DefaultRegX, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter valid email!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
					return;
				}

				if (string.IsNullOrEmpty(Password) || string.IsNullOrWhiteSpace(Password))
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter password!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Password_EmptyMessage"));
					return;
				}
				#endregion

				if (StoreWebAddress == "HikeTest")
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.Test;
					Settings.AccessTokenClientId = ServiceConfiguration.ASYAccessToken_Client_Id;
					Settings.AccessTokenClientSecretId = ServiceConfiguration.ASYAccessToken_Client_Secret;
				}
				else if (StoreWebAddress.StartsWith("HikeAsyTest", StringComparison.OrdinalIgnoreCase)
					|| StoreWebAddress.StartsWith("HikeAsyTest0", StringComparison.OrdinalIgnoreCase))

				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.ASYTest;
					Settings.AccessTokenClientId = ServiceConfiguration.ASYAccessToken_Client_Id;
					Settings.AccessTokenClientSecretId = ServiceConfiguration.ASYAccessToken_Client_Secret;
				}
				else if (StoreWebAddress == "HikeBenTest")
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.DesignerTest;
					Settings.AccessTokenClientId = ServiceConfiguration.ASYAccessToken_Client_Id;
					Settings.AccessTokenClientSecretId = ServiceConfiguration.ASYAccessToken_Client_Secret;
				}
				else if (StoreWebAddress.StartsWith("HikeHConnect", StringComparison.OrdinalIgnoreCase))
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.HConnectTest;
					Settings.AccessTokenClientId = ServiceConfiguration.AccessToken_Client_Id;
					Settings.AccessTokenClientSecretId = ServiceConfiguration.AccessToken_Client_Secret;
				}
				else if (StoreWebAddress.StartsWith("HikeStaging", StringComparison.OrdinalIgnoreCase))
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.StagingTest;
					Settings.AccessTokenClientId = ServiceConfiguration.AccessToken_Client_Id;
					Settings.AccessTokenClientSecretId = ServiceConfiguration.AccessToken_Client_Secret;
				}
				else
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.Live;
					Settings.AccessTokenClientId = ServiceConfiguration.AccessToken_Client_Id;
					Settings.AccessTokenClientSecretId = ServiceConfiguration.AccessToken_Client_Secret;
				}


				//Request model object
				LoginModel loginRequestObject = new LoginModel()
				{
					TenancyName = StoreWebAddress,
					UsernameOrEmailAddress = Email,
					Password = Password,
					NotificationToken = App.Instance.NotificationToken
				};

				var accountApiService = new ApiService<IAccountApi>();
				var accountService = new AccountServices(accountApiService);
				ResponseModel<string> LoginResponse = new ResponseModel<string>
				{
					success = false,
					error = new Error { message = "Something went wrong" },
				};

				try
				{

					LoginResponse = await accountService.Login(Fusillade.Priority.UserInitiated, loginRequestObject);

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

							var UserDetail = await userService.GetRemoteUserByUserNameOrEmail(Priority.UserInitiated, Email, true);
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

							//if (Settings.TenantName.ToLower() != StoreWebAddress.ToLower())
							if ((!UserDetail.Outlets.Any(x => x.OutletId == Settings.SelectedOutletId)) || (Settings.TenantName.ToLower() != StoreWebAddress.ToLower()))
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

								Settings.TenantName = StoreWebAddress?.Trim();
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
					loginRequestObject = null;
					LoginResponse = null;
					accountApiService = null;
					accountService = null;
				}

			}
		}
		#endregion

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

		async void NavigateToForgotPassword()
		{
			try
			{
					await NavigationService.PushAsync(new ForgotPasswordPage());
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		void NavigateToRegister()
		{
			try
			{
				if (_navigationService.GetCurrentPage is NavigationPage)
				{
					_navigationService.GetCurrentPage.Navigation.PushAsync(new SignUpPage());
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

	}
}
