using HikePOS.ViewModels;
using HikePOS.Helpers;
using System.Reflection;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.Resources;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Messenger;
using System.Diagnostics;
using Microsoft.Maui.Devices;
namespace HikePOS;

public partial class AppShell : Shell
{
   private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
    ChangeUserPage changeUserpage;
    ChangePasswordPage changePasswordPage;
    ClockInOutPage clockInOutPage;
    MenuViewModel menuviewmodel;


    ApiService<IProfileApi> profileApiService = new ApiService<IProfileApi>();
    ProfileServices profileService;

    ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
    UserServices userService;

   public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(CheckOutPage), typeof(CheckOutPage));
        Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));
		profileService = new ProfileServices(profileApiService);
		userService = new UserServices(userApiService);
        UpdateMenuPageAsync("All", false);

          
        if (!WeakReferenceMessenger.Default.IsRegistered<MenuDataUpdatedMessenger>(this))
        {
            WeakReferenceMessenger.Default.Register<MenuDataUpdatedMessenger>(this, (sender, arg) =>
            {
                if (arg.Value == "All-Data")
                    UpdateMenuPageAsync("All");
                else
                    UpdateMenuPageAsync(arg.Value, false);
            });
        }
	}

    async void LogoutHandle_Clicked(object sender, System.EventArgs e)
    {
        await LogoutUser();
    }

	void ChangeUserHandle_Clicked(object sender, System.EventArgs e)
	{
		try
		{
			changeUserpage = new ChangeUserPage();
			SetIsOpenToFalse();
            FlyoutIsPresented = false;
			Navigation.PushModalAsync(changeUserpage);
		}
		catch (Exception ex)
		{
			ex.Track();
		}
	}

	public async Task<bool> LogoutUser()
	{
		var decline = await DisplayAlert("Are you sure you want to log out?", null, "Yes", "No");

		if (!decline)
			return false;

		Settings.AccessToken = string.Empty;


		App.Instance.MainPage = new NavigationPage(new LoginUserPage());
        // if (entersalepage.RootPage is BaseContentPage<EnterSaleViewModel>)
        //     ((BaseContentPage<EnterSaleViewModel>)(entersalepage.RootPage)).ViewModel?.ReleaseAbly();
        // entersalepage = null;
        // parksalepage = null;
        // settingpage = null;
        // adminpage = null;
        return true;
	}

	void UpdateProfilePictureHandle_Tapped(object sender, System.EventArgs e)
	{
		try
		{
			//Start #83353 Pratik
			new ImageCropper.Maui.ImageCropper()
			{
				AspectRatioX = 1,
				AspectRatioY = 1,
				Success = async (imageFile) =>
				{
					try
					{
						byte[] bytes = File.ReadAllBytes(imageFile);
						string base64 = Convert.ToBase64String(bytes);
						UpdateProfilePictureModel UpdateProfilePicture = new UpdateProfilePictureModel();
						UpdateProfilePicture.imagesrc = base64;

						string ProfilePictureId = await profileService.UpdateProfilePicture(Fusillade.Priority.UserInitiated, true, UpdateProfilePicture);

						if (!string.IsNullOrEmpty(ProfilePictureId))
						{
							var currentUser =  userService.GetLocalUserById(Settings.CurrentUser.Id);
							currentUser.ProfilePictureId = new Guid(ProfilePictureId);
							Settings.CurrentUser = currentUser;
							UpdateMenuPageAsync("User");
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
				},
				Failure = () =>
				{
					Console.WriteLine("Error capturing an image to crop.");
				}
			}.Show(this);

			//End #83353 Pratik
		}
		catch (Exception ex)
		{
			ex.Track();
		}
	}

	void UpdateMenuPageAsync(string argument,  bool setpage = true)
    {
        try
        {
            var Persmissions = Settings.GrantedPermissionNames;

            if (argument == "All")
            {
                var currentregister = Settings.CurrentRegister;
                if (currentregister != null)
                {
                    OutletName.Text = currentregister.OutletName;
                    RegisterName.Text = currentregister.Name;
                }
                var zohoSalesIQ = DependencyService.Get<IZohoSalesIQService>();
                if(zohoSalesIQ != null)
                    zohoSalesIQ.LoginUser();
            }
            if (argument == "All" || argument == "User")
            {
                var currentuser = Settings.CurrentUser;
                if (currentuser != null)
                {
                    AvtratName.Text = currentuser.FullName.StringToAvtarName();
                    string url;
                    if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                    {
                        url = ServiceConfiguration.LiveProtocol + ServiceConfiguration.LivePrefix + ServiceConfiguration.LiveBaseUrl;
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                    {
                        url = ServiceConfiguration.DesignerProtocol + ServiceConfiguration.DesignerBaseUrl;
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.HConnectTest)
                    {
                        url = ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl;
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.StagingTest)
                    {
                        url = ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl;
                    }
                    else
                    {
                        url = ServiceConfiguration.AsyProtocol + ServiceConfiguration.AsyBaseUrl;
                    }
                    ProfilePicture.Source = new Uri(url + "/Profile/GetUserProfilePicture?size=240&tenantId=" + Settings.TenantId + "&id=" + currentuser.ProfilePictureId);
                    UserName.Text = currentuser.FullName;

                    if (userService == null)
                        userService = new UserServices(userApiService);

                    var UserClockINOut =  userService.GetClockInOutUsersById(Settings.CurrentUser.Id);
                    if (UserClockINOut != null)
                        StatusIcon.IsVisible = (UserClockINOut.InTime != null);
                    else
                        StatusIcon.IsVisible = false;
                }
            }

            // if (argument == "All" || argument == "Internet")
            // {
            //     if (App.Instance.IsInternetConnected)
            //     {
            //         InternetIndicator.TextColor = AppColors.OnlineColor;
            //         InternetIndicator.Text = LanguageExtension.Localize("Online_Text");
            //     }
            //     else
            //     {
            //         InternetIndicator.TextColor = AppColors.SliderStatusColor;
            //         InternetIndicator.Text = LanguageExtension.Localize("Offline_Text");
            //     }
            // }

//             if (argument == "All" || argument == "Printer")
//             {
//                 if (Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => x.ActiveDocketPrint || x.PrimaryReceiptPrint))
//                 {
// #if IOS
//                     PrinterIndicator.TextColor = AppColors.OnlineColor;
//                     PrinterIndicator.Text = LanguageExtension.Localize("Connected_Text");
// #elif ANDROID
//                         PrinterIndicator.TextColor = AppColors.OnlineColor;
//                         PrinterIndicator.Text = LanguageExtension.Localize("Connected_Text");
                
// #endif
//                 }
//                 else
//                 {
//                     PrinterIndicator.TextColor = AppColors.SliderStatusColor;
//                     PrinterIndicator.Text = LanguageExtension.Localize("NotFound_Text");
//                 }
//             }

            // if (argument == "All" || argument == "Barcode")
            // {
            //     if (App.Instance.IsBarcodeScannerConnected)
            //     {
            //         BarcodeIndicator.TextColor = AppColors.OnlineColor;
            //         BarcodeIndicator.Text = LanguageExtension.Localize("Connected_Text");
            //     }
            //     else
            //     {
            //         BarcodeIndicator.TextColor = AppColors.SliderStatusColor;
            //         BarcodeIndicator.Text = LanguageExtension.Localize("NotFound_Text");
            //     }
            // }

            // if (argument == "All" || argument == "AllowSwitchBetweenUser")
            // {
            //     if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.ToAllowSwitchBetweenUser)
            //     {
            //         ChangeUserbtn.IsVisible = true;
            //     }
            //     else
            //     {
            //         ChangeUserbtn.IsVisible = false;
            //     }
            // }

            if (argument == "All" || argument == "EnterSalePermission")
            {
                if (Persmissions != null && Persmissions.Any(s => s == "Pages.Tenant.POS.EnterSale"))
                {
                    EnterSaleMenuBtn.IsVisible = true;
                }
                else
                {
                    if (setpage && EnterSaleMenuBtn.IsVisible && this.CurrentItem.Title == LanguageExtension.Localize("EnterSale_TitleText"))
                    {
                        App.Current.MainPage = new NavigationPage(new NoPermissionPage(this.CurrentItem.Title));
                    }
                    else
                        EnterSaleMenuBtn.IsVisible = false;
                }
            }

            if (argument == "All" || argument == "SaleHistoryPermission")
            {
                if (Persmissions != null && Persmissions.Any(s => s == "Pages.Tenant.POS.SalesHistory"))
                {
                    SaleHistoryMenuBtn.IsVisible = true;
                }
                else
                {
                    if (setpage && SaleHistoryMenuBtn.IsVisible && this.CurrentItem.Title == LanguageExtension.Localize("SalesHistoryText"))
                    {
                        App.Current.MainPage = new NavigationPage(new NoPermissionPage(this.CurrentItem.Title));
                        return;
                    }
                    else
                        SaleHistoryMenuBtn.IsVisible = false;
                }
            }

            // if (argument == "All" || argument == "AdminPermission")
            // {
            //     if (Persmissions != null && Persmissions.Any(s => s == "Pages.Tenant.Can.Show.Backoffice.Ipad"))
            //     {
            //         AdminMenuBtn.IsVisible = true;
            //     }
            //     else
            //     {
            //        if (setpage && AdminMenuBtn.IsVisible && this.CurrentItem.Title == LanguageExtension.Localize("Admin_TitleText"))
            //         {
            //             App.Current.MainPage = new NavigationPage(new NoPermissionPage(this.CurrentItem.Title));
            //             return;
            //         }
            //         else
            //             AdminMenuBtn.IsVisible = false;
            //     }
            // }

            if (argument == "All" || argument == "CloseRegisterPermission")
            {
                if (Persmissions != null && Persmissions.Any(s => s == "Pages.Tenant.POS.CloseRegister"))
                {
                    CashRegisterMenuBtn.IsVisible = true;
                }
                else
                {
                    if (setpage && CashRegisterMenuBtn.IsVisible && this.CurrentItem.Title == LanguageExtension.Localize("CashRegister_TitileText"))
                    {
                        App.Current.MainPage = new NavigationPage(new NoPermissionPage(this.CurrentItem.Title));
                        return;
                    }
                    else
                        CashRegisterMenuBtn.IsVisible = false;
                }
            }
        }
        catch (Exception ex)
        {
            ex.Track();
        }
    }

    void ChangePasswordHandle_Clicked(object sender, System.EventArgs e)
    {
        if (changePasswordPage == null)
        {
            changePasswordPage = new ChangePasswordPage();
        }
        SetIsOpenToFalse();
        FlyoutIsPresented = false;
        Navigation.PushModalAsync(changePasswordPage);
    }
    void ClockInOutHandle_Clicked(object sender, System.EventArgs e)
    {
        try
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {   //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.


                PropertyInfo myPropInfo;
                bool result = false;

                myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeClockInClockOutFeature");

                bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);

                if (!result)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);

                    return;
                }
                //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.

                if (clockInOutPage == null)
                {
                    clockInOutPage = new ClockInOutPage();

                }
                SetIsOpenToFalse();
                FlyoutIsPresented = false;
                Navigation.PushModalAsync(clockInOutPage);

            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    void SetIsOpenToFalse()
    {
        // if (this is BaseContentPage<EnterSaleViewModel> enterSalePage)
        //    enterSalePage.ViewModel.IsOpenPopup = true;
    }

    void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
    }

      void ZohoSalesIQHandle_Tapped(object sender, EventArgs e)
     {
            FlyoutIsPresented = false;
            var zohoSalesIQ = DependencyService.Get<IZohoSalesIQService>();
            zohoSalesIQ.OpenMessenger();
     }


}

