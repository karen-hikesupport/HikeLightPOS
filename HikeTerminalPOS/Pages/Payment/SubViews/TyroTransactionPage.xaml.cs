using System;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS
{
	
	public partial class TyroTransactionPage : PopupBasePage<TyroTransactionViewModel>
    {
		public event EventHandler<PaymentResult> PaymentSuccessed;

		public TyroTransactionPage()
		{
			InitializeComponent();
			Title = "Tyro payment";
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();



            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.TyroTransactionCompleteMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.TyroTransactionCompleteMessenger>(this, (sender, arg) =>
                {
                    PaymentSuccessed?.Invoke(this, arg.Value);
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.TyroTransactionCancelMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.TyroTransactionCancelMessenger>(this,async (sender, arg) =>
                {
                    if (arg.Value)
                    {
                        await Close();
                    }
                });
            }

            if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live)
            {
                tyroWebView.Source = ServiceConfiguration.TyroLiveUrl + "/Configuration.html";
            }
            else
            {
                tyroWebView.Source = ServiceConfiguration.TyroTestUrl + "/Configuration.html";
            }

            ViewModel.TyroWebView = tyroWebView;
            ViewModel.LoadPaymentInfo();


        }

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messenger.TyroTransactionCancelMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.TyroTransactionCompleteMessenger>(this);
        }

		async void CloseHandle_Clicked(object sender, System.EventArgs e)
		{
            var result = await tyroWebView.CheckPaymentProcessIsActive(true);
            if (result.ToLower() == "true")
            {
                await ViewModel.CancelPayment(true); 
            }
            else
            {
                await Close();
            }

        }

        public async Task Close(){
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
