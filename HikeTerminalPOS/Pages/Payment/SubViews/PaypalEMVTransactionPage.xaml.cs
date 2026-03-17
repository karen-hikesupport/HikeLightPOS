using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{
	public partial class PaypalEMVTransactionPage : PopupBasePage<PaypalEMVTransactionViewModel>
    {

		//public event EventHandler<string> PaymentSuccessed;
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		public PaypalEMVTransactionPage()
		{
			InitializeComponent();
			Title = "Paypal payment";
		}


		async void CloseHandle_Clicked(object sender, System.EventArgs e)
		{
            await Close();
		}

        public async Task Close()
        {
            try
            {
				if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                {
                    await _navigationService.Navigation.PopModalAsync();
                }
                
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }
	}

}
