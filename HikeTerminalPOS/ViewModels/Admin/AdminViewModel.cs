
using System;
using System.Threading.Tasks;
using HikePOS.Resources;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public class AdminViewModel : BaseViewModel
	{
		ApiService<IAccountApi> accountApiService = new ApiService<IAccountApi>();
		AccountServices accountServices;

        System.Uri _webUrl { get; set; }
        public System.Uri webUrl { get { return _webUrl; } set { _webUrl = value; SetPropertyChanged(nameof(webUrl));} }

		public async Task<string> GetAdminURL(AdminInputDto objAdminInputDto)
		{
           // using (new Busy(this, true))
           // {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    if (accountServices == null)
                    {
                        accountServices = new AccountServices(accountApiService);
                    }
                    var urlResult = await accountServices.GetAdminUrl(Fusillade.Priority.UserInitiated, objAdminInputDto);
                    if (urlResult != null && !string.IsNullOrEmpty(urlResult))
                    {
                        return urlResult + "&openedBy=webview";
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
           // };


			return null;
		}
	}
}
