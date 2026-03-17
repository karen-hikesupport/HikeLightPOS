using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class EconduitRefundPaymentPage : PopupBasePage<EconduitRefundPaymentViewModel>
    {
        public EventHandler<InvoicePaymentDto> Selected_InvoiceRefundPayments;

        private ObservableCollection<InvoicePaymentDto> InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>();

        public EconduitRefundPaymentPage()
        {
            InitializeComponent();
        }

        async void CloseHandle_Clicked(object sender, System.EventArgs e)
        {
            await Close();
        }

        void RefundHandel_Clicked(object sender, System.EventArgs e)
        {
            InvoicePaymentDto Selected_Refund = (InvoicePaymentDto)((Button)sender).BindingContext;
            Selected_InvoiceRefundPayments?.Invoke(this, Selected_Refund);
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
