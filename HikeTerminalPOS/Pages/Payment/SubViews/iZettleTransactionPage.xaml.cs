using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{
	public partial class iZettleTransactionPage : PopupBasePage<iZettleTransactionViewModel>
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		public event EventHandler<iZettlePaymentResult> PaymentSuccessed;
		IiZettle iZettle;

        //Ticket #11319 Start : Add logs for payment types. By Nikhil
        public PaymentOptionDto paymentOption { get; set; }
        public SubmitLogServices logService { get; set; }
        public Dictionary<string, string> logRequestDetails { get; set; }
        //Ticket #11319 End. By Nikhil

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;

        public iZettleTransactionPage()
		{
			InitializeComponent();
			Title = "iZettle payment";
            saleService = new SaleServices(saleApiService);
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
		}


		async void PayHandle_Clicked(object sender, System.EventArgs e)
        {
           // await Close();
            logRequestDetails.Clear();
            if (iZettle == null)
			{
				iZettle = DependencyService.Get<IiZettle>();
			}
            //Ticket #11319 Start : Add logs for payment types. By Nikhil
            logRequestDetails.Add("IsRefund", (ViewModel.Invoice.Status == InvoiceStatus.Refunded).ToString());
            logRequestDetails.Add("Amount", ViewModel.Invoice.TenderAmount.ToString());
            //Ticket start:#21625 Add Invoice number and Integrated payment response in API log.by rupesh
            logRequestDetails.Add("InvoiceNumber", ViewModel.Invoice.Number);
            //Ticket end:#21625 .by rupesh
            //Ticket #11319 End. By Nikhil

            var lastReference = Guid.NewGuid().ToString();

            if (ViewModel.Invoice.Status != InvoiceStatus.Refunded)
            {
                //Ticket #11319 Start : Add logs for payment types. By Nikhil 
                logRequestDetails.Add("Reference", lastReference); 
                Extensions.SendLogsToServer(logService, paymentOption, logRequestDetails);
                //Ticket #11319 End : By Nikhil

                string tempString = "Amount: " + ViewModel.Invoice.TenderAmount.ToString()
                        + "Last reference: " + lastReference.ToString();
                        
                SaveInLocalbeforePayment(tempString, false);

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                iZettle.ChargeAmount(ViewModel.Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0), null, lastReference, _navigationService.RootPage);
            }
            else
            {
                decimal tempAmount = System.Math.Abs(ViewModel.Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0));
                //End ticket #73190  By Rupesh
                //Ticket start:#29940 Not able to process refund using iZettle.by rupesh
                iZettlePaymentResult iZettleDetail = new iZettlePaymentResult();

                foreach (var item in ViewModel.Invoice.InvoiceRefundPayments)
                {
                    var paymentDetails = item.InvoicePaymentDetails;

                    foreach (var payment in paymentDetails)
                    {
                        string temps = payment.Value;

                        iZettleDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<iZettlePaymentResult>(temps) as iZettlePaymentResult;
                    }
                }

                if (iZettleDetail != null)
                {
                   // string refundReference = iZettleDetail.ReferenceNumber;
                    var refundReference = iZettleDetail.lastRefrence;
                    string invoiceReference = ViewModel.Invoice.InvoiceTempId;
                    //Ticket end:#29940 .by rupesh

                    //Ticket #11319 Start : Add logs for payment types. By Nikhil 
                    logRequestDetails.Add("Reference", invoiceReference);
                    logRequestDetails.Add("RefundReference", refundReference);
                    Extensions.SendLogsToServer(logService, paymentOption, logRequestDetails);
                    //Ticket #11319 End : By Nikhil

                    string tempString = "Amount: " + tempAmount.ToString()
                        + "Last reference: " + lastReference.ToString()
                        + "refundReference: " + refundReference.ToString();


                    SaveInLocalbeforePayment(tempString, true);

                    iZettle.RefundAmount(tempAmount,
                     lastReference, refundReference, _navigationService.RootPage);
                }
                else
                {
                    Debug.WriteLine("iZettle detail is not found");
                }

            }

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
            catch(Exception ex)
            {

                ex.Track();
            }
		}


        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
        {

            //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
            if (ViewModel.Invoice.Status == InvoiceStatus.Refunded || ViewModel.Invoice.Status == InvoiceStatus.Exchange)
            {
                return;
            }
            //Ticket end:#24764 .by rupesh

            string currentPaymentObject = string.Empty;
            if (isRefund)
                currentPaymentObject = "iZettle Refund : ";
            else
                currentPaymentObject = "iZettle Sale :  ";

           
            currentPaymentObject = currentPaymentObject + " : " + paymentObject;



            ViewModel.Invoice.CurrentPaymentObject = currentPaymentObject;
            ViewModel.Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;


            await saleService.UpdateLocalInvoice(ViewModel.Invoice, LocalInvoiceStatus.PaymentProcessing);

        }
        #endregion
    }
}
