using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.UserControls;

namespace HikePOS.ViewModels
{
	public class TwoFactorViewModel: BaseViewModel
	{
		private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
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

		public ICommand SubmitButtonCommand { get; }
		public ICommand NavigateToBackCommand { get; }
        public ICommand ReturnCommand => new Command<BorderLessEntry>(FocusReturn);

        public void FocusReturn(BorderLessEntry borderLessEntry)
        {
            borderLessEntry.Focus();
        }

        public TwoFactorViewModel()
		{
            Title = "Verify Security Code"; 
            SubmitButtonCommand = new Command(NavigateToSubmit);
			NavigateToBackCommand = new Command(NavigateToBack);
			shopService = new ShopServices(shopApiService);
			outletService = new OutletServices(outletApiService);
			subscriptionSevice = new SubscriptionSevice(SubscriptionApiService);
			userService = new UserServices(userApiService);
			customerService = new CustomerServices(customerApiService);
			saleService = new SaleServices(saleApiService);
		}

		string _code;
		public string Code
		{
			get { return _code; }
			set { _code = value; SetPropertyChanged(nameof(Code)); }
		}

		bool _isRemember;
		public bool IsRemember
		{
			get { return _isRemember; }
			set { _isRemember = value; SetPropertyChanged(nameof(IsRemember)); }
		}

		public string StoreWebAddress {get;set;}

		public string Email { get; set; }

		public string Token { get; set; }

		void NavigateToBack()
		{
            if (_navigationService.GetCurrentPage is NavigationPage && NavigationService.NavigationStack != null && NavigationService.NavigationStack.Count > 0)
			{
                NavigationService.PopAsync();
			}
		}

		async void NavigateToSubmit()
		{
			using (new Busy(this, true))
			{
				#region Login_Validation
				if (!App.Instance.IsInternetConnected)
				{
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
					return;
				}

				if (string.IsNullOrEmpty(Code) || string.IsNullOrWhiteSpace(Code))
				{
					App.Instance.Hud.DisplayToast("Please enter verification code");
					Code = string.Empty;
					return;
				}


				if (!System.Text.RegularExpressions.Regex.IsMatch(Code, "^[0-9]*$"))
				{
					App.Instance.Hud.DisplayToast("Please enter valid verification code");
					Code = string.Empty;
					return;
				}
				
				#endregion


				var accountApiService = new ApiService<IAccountApi>();
				var accountService = new AccountServices(accountApiService);
				ResponseModel<object> LoginResponse = new ResponseModel<object>
				{
					success = false,
					error = new Error { message = "Something went wrong" },
				};

				try
				{
					VerifySecurityCodeDto verifySecurityCodeDto = new VerifySecurityCodeDto()
					{
						Code = Code,
						Provider = "email",
						RememberBrowser = IsRemember,
						RememberMe = false,						
					};

                    LoginResponse = await accountService.VerifySecurityCode(Fusillade.Priority.UserInitiated, verifySecurityCodeDto);

					if (LoginResponse != null)
					{
						if (LoginResponse.success) 
						{
							Settings.AccessToken = "bearer " + LoginResponse.result;
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
								Settings.RememberBrowser = IsRemember;
								//Settings.PaypalToken = string.Empty;
								//Settings.TyroIntegrationkey= string.Empty;
								Settings.CurrentUser = UserDetail;
								await LoginViewModel.GetOutlets(shopService, outletService, true);
							}
							else
							{
								Settings.CurrentUser = UserDetail;
								Settings.RememberBrowser = IsRemember;
								//BlobCache.ApplicationName = "HikePOS_" + StoreWebAddress;
								await LoginViewModel.AutoLogin(shopService, outletService);
							}
						}
					}
					if (LoginResponse?.error != null)
					{
						App.Instance.Hud.DisplayToast(LoginResponse.error.message, Colors.Red, Colors.White);
					}
					else
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
					}
				}
				catch (Exception ex)
				{
					ex.Track();
				}

			};
		}
	}
}
