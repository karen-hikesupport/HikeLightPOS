using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public class SignUpViewModel : BaseViewModel
	{
		#region Declaration
		private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        ApiService<IShopApi> shopApiService = new ApiService<IShopApi>();
		public ShopServices shopService;

		ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
		public OutletServices outletService;

		ApiService<ISubscriptionAPI> SubscriptionApiService = new ApiService<ISubscriptionAPI>();
		SubscriptionSevice subscriptionSevice;

		ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
		SaleServices saleService;

		ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
		CustomerServices customerService;

		ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
		UserServices userService;
        #endregion

        #region Properties

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

        #region Life Cycle
        public SignUpViewModel()
		{
            Title = LanguageExtension.Localize("SignUpPageTitleText");
            SubmitButtonCommand = new Command(SingUp);
			NavigateToBackCommand = new Command(NavigateToBack);

            outletService = new OutletServices(outletApiService);
			shopService = new ShopServices(shopApiService);
			subscriptionSevice = new SubscriptionSevice(SubscriptionApiService);
			saleService = new SaleServices(saleApiService);
			userService = new UserServices(userApiService);
			customerService = new CustomerServices(customerApiService);

		}
        #endregion

        #region Command
        public ICommand SubmitButtonCommand { get; }
        public ICommand NavigateToBackCommand { get; }
        #endregion


        #region Command Execution
        async void SingUp()
		{




			using (new Busy(this, true))
			{

				#region Login_Validation
				if (string.IsNullOrEmpty(StoreWebAddress) || string.IsNullOrWhiteSpace(StoreWebAddress))
				{
					//await Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter store name!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreName_EmptyMessage"));
					return;
				}


				if (!Regex.IsMatch(StoreWebAddress, RegxValues.DefaultRegX, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
				{
					//await Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter valid store name!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreName_InValidMessage"));
					return;
				}

				if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
				{
					//await Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter email!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_EmptyMessage"));
					return;
				}

				if (!Regex.IsMatch(Email, RegxValues.EmailRegx, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
				{
					//await Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter valid email!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
					return;
				}

				if (string.IsNullOrEmpty(Password) || string.IsNullOrWhiteSpace(Password))
				{
					//await Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter password!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Password_EmptyMessage"));
					return;
				}
				#endregion

				if (StoreWebAddress == "HikeTest")
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.Test;
				}
				else if (StoreWebAddress == "HikeAsyTest" || StoreWebAddress.StartsWith("HikeAsyTest0"))
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.ASYTest;
				}
				else if (StoreWebAddress == "HikeBenTest")
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.DesignerTest;
				}
				else
				{
					Settings.AppEnvironment = (int)Models.Enum.AppEnvironment.Live;
				}

				//Request model object
				var CheckTenantRequestObject = new CheckTenantInputModel();
				CheckTenantRequestObject.Tenant = StoreWebAddress;


				var accountApiService = new ApiService<IAccountApi>();
				var accountService = new AccountServices(accountApiService);
				ResponseModel<object> CheckTenantResponse = new ResponseModel<object>
				{
					success = false,
					error = new Error { message = "Something went wrong" },
				};

				try
				{
					CheckTenantResponse = await accountService.CheckTenantAvalilable(Fusillade.Priority.UserInitiated, CheckTenantRequestObject);

					if (CheckTenantResponse != null)
					{
						if (CheckTenantResponse.success)
						{
							var TenantInput = new CreateTenantInputModel();
							TenantInput.TenancyName = StoreWebAddress;
							TenantInput.StoreName = StoreWebAddress;
							TenantInput.AdminEmailAddress = Email;
							TenantInput.AdminPassword = Password;

							var CreateTenantResponse = await accountService.CreateTenant(Fusillade.Priority.UserInitiated, TenantInput);

							if (CreateTenantResponse != null && CreateTenantResponse.success)
							{
								//await LoginViewModel.Login(this, StoreWebAddress, Email, Password, subscriptionSevice, saleService, customerService, shopService, outletService, userService);
							}
						}
						else
						{
							App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreNameAlreadyExistsMessage"), Colors.Red, Colors.White);
						}
					}
					else
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
					}
				}
				catch (Exception ex)
				{
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
					ex.Track();
				}


			}

		}

        void NavigateToBack()
        {
            if (_navigationService.GetCurrentPage is NavigationPage && NavigationService.NavigationStack != null && NavigationService.NavigationStack.Count > 0)
            {
                NavigationService.PopAsync();
            }
        }
        #endregion

    }
}
