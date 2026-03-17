using HikePOS.ViewModels;
using System;
using HikePOS.Services;
using HikePOS.Helpers;
using System.Threading.Tasks;

namespace HikePOS
{
    public partial class AdminPage : MainBaseContentPage
	{
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		ApiService<IAccountApi> AccountApiService = new ApiService<IAccountApi>();
		AccountServices accountService;

	    public AdminPage()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            Title = "Admin";
            accountService = new AccountServices(AccountApiService);


            WvAdminDashboard.WebNavigating += (sender, e) => {
                try
                {
                    if (e)
                    {
                        WebViewActivityIndicatior.IsRunning = true;
                        WebViewActivityIndicatior.IsVisible = true;
                    }
                    else
                    {
                        WebViewActivityIndicatior.IsRunning = false;
                        WebViewActivityIndicatior.IsVisible = false;
                    }
                }
                catch(Exception ex)
                {
                    ex.Track();
                }
            };
		}

  		protected override void OnLoaded()
        {
            base.OnLoaded();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                LoadWebView();
            });
        }



        async void LoadWebView(bool reload = false){
            try
            {
                if (App.Instance.IsInternetConnected)
                {
                   
                    if (reload)
                    {
                        try
                        {
                            WebViewActivityIndicatior.IsRunning = false;
                            WebViewActivityIndicatior.IsVisible = false;
                            await RenewAccessToken(Settings.CurrentUser.Id.ToString());
                            WebViewActivityIndicatior.IsRunning = true;
                            WebViewActivityIndicatior.IsVisible = true;
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                    if (Settings.CurrentUser != null)
                    {
                        
                        string weburl;

                        if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                        {
                            weburl = ServiceConfiguration.LiveProtocol + Settings.TenantName + "." + ServiceConfiguration.LiveBaseUrl + "/account/SwitchToIPadUserAccount?TargetTenantId=" + Settings.TenantId + "&TargetUserId=" + Settings.CurrentUser.Id;
                        }
                        else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                        {
                            weburl = ServiceConfiguration.DesignerProtocol + Settings.TenantName + "." + ServiceConfiguration.DesignerBaseUrl + "/account/SwitchToIPadUserAccount?TargetTenantId=" + Settings.TenantId + "&TargetUserId=" + Settings.CurrentUser.Id;
                        }
                        else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.HConnectTest)
                        {
                            weburl = ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl + "/account/SwitchToIPadUserAccount?TargetTenantId=" + Settings.TenantId + "&TargetUserId=" + Settings.CurrentUser.Id;

                        }
                        else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.StagingTest)
                        {
                            weburl = ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl + "/account/SwitchToIPadUserAccount?TargetTenantId=" + Settings.TenantId + "&TargetUserId=" + Settings.CurrentUser.Id;
                        }
                        else
                        {
                            //ServiceConfiguration.AsyBaseUrl
                            weburl = ServiceConfiguration.LiveProtocol + Settings.TenantName + "." + "asy.io" + "/account/SwitchToIPadUserAccount?TargetTenantId=" + Settings.TenantId + "&TargetUserId=" + Settings.CurrentUser.Id;
                        }

                        //Temp For Ben design changes
                        //string weburl = ServiceConfiguration.Protocol + ServiceConfiguration.BaseUrl + "/account/SwitchToIPadUserAccount?TargetTenantId=" + Settings.TenantId + "&TargetUserId=" + Settings.CurrentUser.Id;

                        //Added quer parameter in url to detect source
                        weburl += "&source=mobile_app";
                        WvAdminDashboard.UpdateWebUrl?.Invoke(this,weburl);
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
           
        }

		public async Task<bool> RenewAccessToken(string userId)
		{
            using (new BaseViewModel.Busy(new BaseViewModel(), true))
			{
				try
				{
                    await Task.Delay(2);
					return await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated, userId);
				}
				catch (Exception ex)
				{
					ex.Track();
					return false;
				}
			};
		}




		void SliderMenuHandle_Clicked(object sender, System.EventArgs e)
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

        void ReloadHandle_Clicked(object sender, System.EventArgs e)
        {
			var imageservice = DependencyService.Get<IImageServices>();
            imageservice.ClearWKWebsiteCaches();
			LoadWebView(true);
        }
    }
}