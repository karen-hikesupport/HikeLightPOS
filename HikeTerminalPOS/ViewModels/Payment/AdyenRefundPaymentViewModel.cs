using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Newtonsoft.Json;

namespace HikePOS.ViewModels
{
    public class AdyenRefundPaymentViewModel : BaseViewModel
    {
        private ObservableCollection<RefundPaymentDto> _InvoiceRefundPayments;
        public ObservableCollection<RefundPaymentDto> InvoiceRefundPayments
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
        public AdyenRefundPaymentViewModel()
        {
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }
  }
}
