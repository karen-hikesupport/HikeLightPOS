using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS
{
    public partial class CustomPaymentTransactionPage : PopupBasePage<CustomPaymentTransactonViewModel>
    {
        public event EventHandler<CustomPaymentResult> PaymentSuccessed;

        public CustomPaymentTransactionPage()
        {
            InitializeComponent();
            Title = "Custom payment";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();


            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.CustomPaymentTransactionCompleteMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.CustomPaymentTransactionCompleteMessenger>(this, (sender, arg) =>
                {
                    PaymentSuccessed?.Invoke(this, arg.Value);
                });
            }
            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.CustomPaymentTransactionCancelMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.CustomPaymentTransactionCancelMessenger>(this, async (sender, arg) =>
                {
                    await Close();
                });
            }

            ViewModel.LoadPaymentInfo();
            //customPaymentWebView.UpdateWebUrl?.Invoke(this, ViewModel.WebURL);

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messenger.CustomPaymentTransactionCompleteMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.CustomPaymentTransactionCancelMessenger>(this);
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
                    await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


    }
}
