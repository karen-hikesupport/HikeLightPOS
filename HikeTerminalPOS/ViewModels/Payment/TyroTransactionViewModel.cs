using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.UserControls;

namespace HikePOS.ViewModels
{
    public class TyroTransactionViewModel : BaseViewModel
    {
        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

        public string accessToken { get; set; }

        private Func<TyroPaymentInput, Task<PaymentResult>> _performTyroOperationAsync;
        public Func<TyroPaymentInput, Task<PaymentResult>> PerformTyroOperationAsync
        {
            get { return _performTyroOperationAsync; }
            set { _performTyroOperationAsync = value; }
        }

        private Func<bool, Task<string>> _CancelPayment;
        public Func<bool, Task<string>> CancelPayment
        {
            get { return _CancelPayment; }
            set { _CancelPayment = value; SetPropertyChanged(nameof(CancelPayment)); }
        }


        string _WebURL { get; set; }
        public string WebURL { get { return _WebURL; } set { _WebURL = value; SetPropertyChanged(nameof(WebURL)); } }

        //Ticket #11319 Start : Add logs for payment types. By Nikhil
        public PaymentOptionDto paymentOption { get; set; }
        public SubmitLogServices logService { get; set; }
        public Dictionary<string, string> logRequestDetails { get; set; }
        //Ticket #11319 End. By Nikhil

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;
        public TyroWebView TyroWebView;
        public TyroTransactionViewModel()
        {
            saleService = new SaleServices(saleApiService);
        }

        public async void LoadPaymentInfo()
        {
            //Ticket #11319 Start : Add logs for payment types. By Nikhil 
            logRequestDetails.Add("IsRefund", (Invoice.TenderAmount < 0).ToString());
            logRequestDetails.Add("Amount", Invoice.TenderAmount.ToString());
            //Ticket start:#21625 Add Invoice number and Integrated payment response in API log.by rupesh
            logRequestDetails.Add("InvoiceNumber", Invoice.Number);
            //Ticket end:#21625 .by rupesh
            Extensions.SendLogsToServer(logService,paymentOption, logRequestDetails);
            //Ticket #11319 End : By Nikhil
         //   await Task.Run(async () => {

                await Task.Delay(1000);
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                if (Invoice.TenderAmount < 0)
                {
                    await PerformTyroRefundPaymentAsync((double)(Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0)) * (-1));
                }
                else
                {
                    await PerformTyroPaymentAsync((double)(Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0)));
                }
                //End ticket #73190 By Rupesh

         //   });
        }

        public Task<PaymentResult> PerformTyroRefundPaymentAsync(double amount)
        {
            //Ticket start:#86302 Sale payment not updated on the sale.by rupesh
            var transactionId = Guid.NewGuid().ToString();
            string tempobject = "Amount:" + amount.ToString()
                + ",AccessToken:" + accessToken +  ",TransactionId:" + transactionId;
            SaveInLocalbeforePayment(tempobject, true);
            return TyroWebView.PerformTyroOperationAsync(new TyroPaymentInput
            {
                Amount = amount,
                PaymentAction = TyroPaymentAction.Refund,
                HandleAction = HandleAction,
                AccessToken = accessToken,
                TransactionId = transactionId
            });
           //Ticket end:#86302.by rupesh
        }

        public Task<PaymentResult> PerformTyroPaymentAsync(double amount)
        {
            //Ticket start:#86302 Sale payment not updated on the sale.by rupesh
            var transactionId = Guid.NewGuid().ToString();
            string tempobject = "Amount:" + amount.ToString()
                + ",AccessToken:" + accessToken +  ",TransactionId:" + transactionId;
            SaveInLocalbeforePayment(tempobject, false);

            return TyroWebView.PerformTyroOperationAsync(new TyroPaymentInput
            {
                Amount = amount,
                PaymentAction = TyroPaymentAction.Purachse,
                HandleAction = HandleAction,
                AccessToken = accessToken,
                TransactionId = transactionId
            });
            //Ticket end:#86302.by rupesh

        }

        async void HandleAction()
        {
            try
            {
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    await NavigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
        {
            try
            {
                //Ticket start:#86302 Sale payment not updated on the sale.by rupesh
               // Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
                if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
                {
                    return;
                }
                //Ticket end:#24764 .by rupesh
 
                string currentPaymentObject = string.Empty;
                if (isRefund)
                    currentPaymentObject = "Tyro Refund:";
                else
                    currentPaymentObject = "Tyro Sale:";


                currentPaymentObject = currentPaymentObject + paymentObject;


                if(string.IsNullOrEmpty(Invoice.CurrentPaymentObject))
                   Invoice.CurrentPaymentObject = currentPaymentObject;
                else
                  Invoice.CurrentPaymentObject = Invoice.CurrentPaymentObject + " " + currentPaymentObject;
                //Ticket end:#86302 .by rupesh

                Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;


                await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while saving offline : " + ex.ToString());

            }

        }
        #endregion
    }
}
