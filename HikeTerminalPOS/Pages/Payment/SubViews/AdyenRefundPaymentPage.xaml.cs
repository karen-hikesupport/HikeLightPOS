using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class AdyenRefundPaymentPage : PopupBasePage<AdyenRefundPaymentViewModel>
    {
        public EventHandler<RefundPaymentDto> Selected_InvoiceRefundPayments;

        public AdyenRefundPaymentPage()
        {
            InitializeComponent();
        }

        async void CloseHandle_Clicked(object sender, System.EventArgs e)
        {
            if(ViewModel.IsBusy)
                return;
            await Close();
        }

        void RefundHandel_Clicked(object sender, System.EventArgs e)
        {
            if(ViewModel.IsBusy)
                return;
            try
            {
                RefundPaymentDto Selected_Refund = (RefundPaymentDto)((Button)sender).BindingContext;
                ViewModel.IsBusy = true;
                Selected_Refund.TenderedAmount = Convert.ToDecimal(Selected_Refund.TenderedAmount).ToString($"N{Settings.StoreDecimalDigit}");
                Selected_InvoiceRefundPayments?.Invoke(this, Selected_Refund);
            }
            catch(Exception ex)
            {
                ViewModel.IsBusy = false;
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
