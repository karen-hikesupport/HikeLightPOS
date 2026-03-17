using System;
using System.Collections.ObjectModel;
using HikePOS.Models;

namespace HikePOS.ViewModels
{
    public class EconduitRefundPaymentViewModel : BaseViewModel
    {
        private ObservableCollection<InvoicePaymentDto> _InvoiceRefundPayments;
        public ObservableCollection<InvoicePaymentDto> InvoiceRefundPayments
        {
            get
            {
                return _InvoiceRefundPayments;
            }
            set
            {
                _InvoiceRefundPayments = value;
                SetPropertyChanged(nameof(InvoiceRefundPayments));
            }
        }
        public EconduitRefundPaymentViewModel()
        {
            //
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        
    }
}
