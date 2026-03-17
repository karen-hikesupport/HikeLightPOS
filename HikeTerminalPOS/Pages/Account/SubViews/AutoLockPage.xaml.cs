using System;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Resources;
using HikePOS.Services;
using HikePOS.ViewModels;
using static HikePOS.ViewModels.BaseViewModel;

namespace HikePOS
{

	public partial class AutoLockPage : PopupBasePage<AutoLockViewModel> 
    {
        public bool RequiredUpdateAccessToken = true;

		public EventHandler<bool> AuthennticationSuccessed;

		public bool HasBackButtton = false;

		public bool HasSwitchButtton = false;
		
		private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();

		public AutoLockPage()
		{
			InitializeComponent();
			Title = LanguageExtension.Localize("AutolockPageTitle");

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			FirstDot.TextColor = AppColors.BordersColor;
			SecondDot.TextColor = AppColors.BordersColor;
			ThirdDot.TextColor = AppColors.BordersColor;
			FourthDot.TextColor = AppColors.BordersColor;
			Pin = "";
			if (DeviceInfo.Idiom != DeviceIdiom.Phone)
			{
				btnCancelButton.IsVisible = HasBackButtton;
				btnChangeUser.IsVisible = HasSwitchButtton;
			}
			else
			{
				btnCancelButtonPhone.IsVisible = HasBackButtton;
				btnChangeUserPhone.IsVisible = HasSwitchButtton;
			}
			if (!HasBackButtton)
			{
				Settings.IsAppLocked = true;
			}
			var height = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
			if (height <= 720)
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await Task.Delay(10);
					await scrollview.ScrollToAsync(stkview.Children.Last() as Element, ScrollToPosition.End, true);
				});
			}
		}

		string _Pin { get; set; } = "";
		public string Pin
		{
			get { return _Pin; }
			set
			{
				_Pin = value;

				FirstDot.TextColor = AppColors.BordersColor;
				SecondDot.TextColor = AppColors.BordersColor;
				ThirdDot.TextColor = AppColors.BordersColor;
				FourthDot.TextColor = AppColors.BordersColor;

				if (Pin.Length >= 4)
				{
					FourthDot.TextColor = AppColors.TitlebarTextColor;
				}

				if (Pin.Length >= 3)
				{
					ThirdDot.TextColor = AppColors.TitlebarTextColor;
				}

				if (Pin.Length >= 2)
				{
					SecondDot.TextColor = AppColors.TitlebarTextColor;
				}

				if (Pin.Length >= 1)
				{
					FirstDot.TextColor = AppColors.TitlebarTextColor;
				}
			}
		}

		async void CancelAuthenticationHandle_Clicked(object sender, System.EventArgs e)
		{
			try
			{
				if (HasBackButtton)
				{
					await Close();
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

        async void ChangeUserHandle_Clicked(object sender, System.EventArgs e)
		{
			try
			{
				if (HasSwitchButtton)
				{
					ChangeUserPage changeUserpage = new ChangeUserPage();
                    changeUserpage.ViewModel.ChangeUserSuccessed += ChangeUserpage_ChangeUserSuccessed;
                    await Navigation.PushModalAsync(changeUserpage);
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

        void ChangeUserpage_ChangeUserSuccessed(object sender, bool e)
        {
            AuthennticationSuccessed?.Invoke(this, e);
        }



		async void Button_Clicked(object sender, System.EventArgs e)
		{
			if (IsBusy)
				return;

            var btn = (Button)sender;
			if (btn.Text == LanguageExtension.Localize("ClearBtnText"))
			{
				if (Pin.Length > 0)
				{
					Pin = Pin.Remove(Pin.Length - 1);
				}
				else
				{
					await PinDotsLayout.TranslateTo(-10, 0, 30, Easing.Linear);
					await PinDotsLayout.TranslateTo(10, 0, 30, Easing.Linear);
					await PinDotsLayout.TranslateTo(0, 0, 30, Easing.Linear);
				}
			}
			else
			{
				if (Pin.Length < 4)
				{
					Pin = Pin.Insert(Pin.Length, btn.Text);
					if (Pin.Length == 4)
					{
                        //Ticket start:#72462 Need to enter the passcode five times to process the sale.by rupesh
                        using (new Busy(this.ViewModel, true))
                        {
                            try
                            {
                                IsBusy = true;
                                var res = ViewModel.CheckPin(Pin, Email.Text);
                                if (res)
                                {

                                    if (ViewModel.CurrentUser != null)
                                    {
                                        bool result = true;
                                        if (RequiredUpdateAccessToken)
                                        {
                                            result = await ViewModel.ChangeUser_GetNewAccessToken(ViewModel.CurrentUser.Id.ToString());
                                        }
                                        if (result)
                                        {
                                            AuthennticationSuccessed?.Invoke(this, result);
                                        }
                                    }
                                }
                                else
                                {

                                    await PinDotsLayout.TranslateTo(-10, 0, 30, Easing.Linear);
                                    await PinDotsLayout.TranslateTo(10, 0, 30, Easing.Linear);
                                    await PinDotsLayout.TranslateTo(0, 0, 30, Easing.Linear);
                                }
                                Pin = "";
                                IsBusy = false;
                            }
                            catch (Exception ex)
                            {
                                IsBusy = false;
                                ex.Track();
                            }

                        };
                        //Ticket end:#72462 .by rupesh

                    }
                }
				else
				{
					await PinDotsLayout.TranslateTo(-10, 0, 30, Easing.Linear);
					await PinDotsLayout.TranslateTo(10, 0, 30, Easing.Linear);
					await PinDotsLayout.TranslateTo(0, 0, 30, Easing.Linear);
				}
			}
		}

		async void ChangeLogoutHandle_Clicked(object sender, System.EventArgs e)
		{
			try
			{
               
				if (!HasBackButtton)
				{
					if ((AppShell)_navigationService.GetCurrentPage !=null)
                    {
                        var res = await ((AppShell)_navigationService.GetCurrentPage).LogoutUser();
                        if(res)
                        {
                            await Close();
                            Settings.IsAppLocked = false;
                        }
                    }
				}
                else
                    await Close();
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		

		public async Task Close()
		{
			try
			{
                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                    await Navigation.PopModalAsync();
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}
	}
}
