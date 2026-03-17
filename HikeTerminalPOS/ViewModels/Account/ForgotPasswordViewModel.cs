using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.UserControls;

namespace HikePOS.ViewModels
{
	public class ForgotPasswordViewModel: BaseViewModel
	{
		private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		public ICommand SubmitButtonCommand { get; }
		public ICommand NavigateToBackCommand { get; }
        //public ICommand NavigateToRegisterCommand { get; }
        public ICommand ReturnCommand => new Command<BorderLessEntry>(FocusReturn);

        public void FocusReturn(BorderLessEntry borderLessEntry)
        {
            borderLessEntry.Focus();
        }

        public ForgotPasswordViewModel()
		{
            Title = LanguageExtension.Localize("ForgotPasswordTitleText"); 
            SubmitButtonCommand = new Command(NavigateToSubmit);
			NavigateToBackCommand = new Command(NavigateToBack);
			//NavigateToRegisterCommand = new Command(NavigateToRegister);
		}

		string _email { get; set; }

		public string Email
		{
			get { return _email; }
			set { _email = value; SetPropertyChanged(nameof(Email)); }
		}

		string _storeWebAddress { get; set; }

		public string StoreWebAddress
		{
			get { return _storeWebAddress; }
			set { _storeWebAddress = value; SetPropertyChanged(nameof(StoreWebAddress)); }
		}

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

				if (!Regex.IsMatch(Email, RegxValues.EmailRegx, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
				{
					//Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter valid email!", "Ok");
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
					return;
				}

				
				#endregion


				//Request model object
				SendPasswordResetLinkModel sendPasswordResetLinkModelObject = new SendPasswordResetLinkModel();
				sendPasswordResetLinkModelObject.TenancyName = StoreWebAddress;
                sendPasswordResetLinkModelObject.EmailAddress = Email;
				
				var accountApiService = new ApiService<IAccountApi>();
				var accountService = new AccountServices(accountApiService);
				ResponseModel<object> LoginResponse = new ResponseModel<object>
				{
					success = false,
					error = new Error { message = "Something went wrong" },
				};

				try
				{

                    LoginResponse = await accountService.SendPasswordResetLink(Fusillade.Priority.UserInitiated, sendPasswordResetLinkModelObject);

					if (LoginResponse != null)
					{
						if (LoginResponse.success)
						{
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SendResetPasswordSuccessMessage"), Colors.Green, Colors.White);
							NavigateToBack();
                            Email = string.Empty;
                            StoreWebAddress = string.Empty;
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

			};
		}
	}
}
