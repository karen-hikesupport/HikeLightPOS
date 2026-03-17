using System;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class paymentConfigurationPage : PopupBasePage<PaymentConfigurationViewModel>
    {
		public paymentConfigurationPage()
		{
			InitializeComponent();
            ViewModel.PaymentWebView = paymentWebView;

            paymentWebView.WebNavigating += (sender, e) =>
			{
				try
				{
					if (e)
					{
						App.Instance.Hud.DisplayProgress(LanguageExtension.Localize("Progress0_Text"));
					}
					else
					{
						App.Instance.Hud.Dismiss();
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
