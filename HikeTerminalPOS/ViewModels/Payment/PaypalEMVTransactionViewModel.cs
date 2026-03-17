using System;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.Resources;
using HikePOS.Services;
using HikePOS.Services.Payment;

namespace HikePOS.ViewModels
{
	public class PaypalEMVTransactionViewModel : BaseViewModel
	{
        public EventHandler<PaypalPaymentResult> PaymentSuccessed;

        public IPaypalHere paypal;
		
        public string InvoiceNumber { get; set; } 

        decimal _TenderAmount { get; set; }
        public decimal TenderAmount { get { return _TenderAmount; } set { _TenderAmount = value; SetPropertyChanged(nameof(TenderAmount)); } }

        public string accessToken { get; set; } 
        public string refreshUrl { get; set; } 

        string _PaypalHereMessage { get; set; } = LanguageExtension.Localize("NoReaderFoundMessage");
        public string PaypalHereMessage { get{ return _PaypalHereMessage;} set { _PaypalHereMessage = value; SetPropertyChanged(nameof(PaypalHereMessage));  }  }

        public ICommand ChargeCommand { get; set; }

        public PaypalEMVTransactionViewModel(){
            ChargeCommand = new Command(PaypalCharge);

        }


		public override void OnAppearing()
		{
            base.OnAppearing();
            if (paypal == null)
            {
                paypal = DependencyService.Get<IPaypalHere>();
            }
            paypal.FindAndConnectDevice();
		}

		public override void OnDisappearing()
		{
            base.OnDisappearing();
		}

		public void PaypalCharge()
		{
			try
			{
				if (paypal == null)
				{
					paypal = DependencyService.Get<IPaypalHere>();
				}
               // paypal.InitializeSDK(accessToken, refreshUrl, InvoiceNumber, TenderAmount);
			}
			catch (Exception ex)
			{
                ex.Track();
			}
		}
	
	}
}
