using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Models.Payment;
using HikePOS.UserControls;

namespace HikePOS.ViewModels
{
	public class PaymentConfigurationViewModel : BaseViewModel
	{

        public event EventHandler<PaymentConfigurationModel> ConfigurationSuccessed;

        public PaymentOptionType paymenttype { get; set; }

        public PaymentWebView PaymentWebView { get; set; }

        string _WebURL { get; set; }
		public string WebURL { get { return _WebURL; } set { _WebURL = value; SetPropertyChanged(nameof(WebURL)); } }

		private Func<string, Task<string>> _evaluateJavascript;
		public Func<string, Task<string>> EvaluateJavascript
		{
			get { return _evaluateJavascript; }
			set { _evaluateJavascript = value; SetPropertyChanged(nameof(EvaluateJavascript));}
		}
		
		public PaymentConfigurationViewModel()
		{
            Title = "Payment configuration page";
        }

        public ICommand CloseCommand => new Command(CloseTapped);

        private async void CloseTapped()
        {
            string TyroIntegrationkey;
            string TyroMId;
            string TyroTId;
            string currentWebLocation;

            if (paymenttype == PaymentOptionType.Tyro)
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    TyroIntegrationkey = await PaymentWebView.EvaluateJavascript("localStorage.getItem('webTta.integrationKey')");
                    TyroMId = await PaymentWebView.EvaluateJavascript("localStorage.getItem('webTta.mid')");
                    TyroTId = await PaymentWebView.EvaluateJavascript("localStorage.getItem('webTta.tid')");
                }
                else
                {
                    TyroIntegrationkey = await PaymentWebView.EvaluateJavaScriptAsync("localStorage.getItem('webTta.integrationKey')");
                    TyroMId = await PaymentWebView.EvaluateJavaScriptAsync("localStorage.getItem('webTta.mid')");
                    TyroTId = await PaymentWebView.EvaluateJavaScriptAsync("localStorage.getItem('webTta.tid')");
                }

                if (!string.IsNullOrEmpty(TyroIntegrationkey))
                {
                    ConfigurationSuccessed?.Invoke(this, new PaymentConfigurationModel()
                    {
                        AccessToken = TyroIntegrationkey,
                        Type = paymenttype,
                        MerchantId = TyroMId,
                        TerminalId = TyroTId
                    });
                    return;
                }

            }

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                currentWebLocation = await PaymentWebView.EvaluateJavascript("window.location.href");
            }
            else
            {
                currentWebLocation = await PaymentWebView.EvaluateJavaScriptAsync("window.location.href");
            }

            if ((paymenttype == PaymentOptionType.PayPal || paymenttype == PaymentOptionType.PayPalhere) && currentWebLocation.IndexOf("access_token") != -1)
            {
                var QueryDictionary = new Uri(currentWebLocation).ParseQueryString();
                var accessToken = QueryDictionary["access_token"];
                var refreshUrl = QueryDictionary["refresh_url"];
                ConfigurationSuccessed?.Invoke(this, new PaymentConfigurationModel()
                {
                    AccessToken = accessToken,
                    RefreshUrl = refreshUrl,
                    Type = paymenttype
                });
                return;

            }
            await Close();
        }
    }
}
