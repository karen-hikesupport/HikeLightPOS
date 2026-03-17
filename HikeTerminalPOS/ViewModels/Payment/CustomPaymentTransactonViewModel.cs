using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class CustomPaymentTransactonViewModel : BaseViewModel
    {
        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

        public string gatewayUrl { get; set; }

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
        public string HtmlText { get; set; }
        private string _HeaderText { get; set; }
        public string HeaderText { get { return _HeaderText; } set { _HeaderText = value; SetPropertyChanged(nameof(HeaderText)); } }
        public CustomPaymentTransactonViewModel()
        {
            saleService = new SaleServices(saleApiService);
        }

        public void LoadPaymentInfo()
        {
            HeaderText = paymentOption.PaymentOptionType == PaymentOptionType.CustomPayment ? "Other Payment" : "Verifone–Vcloud Payment";
            //Ticket #11319 Start : Add logs for payment types. By Nikhil 
            logRequestDetails.Add("IsRefund", (Invoice.TenderAmount < 0).ToString());
            logRequestDetails.Add("Amount", Invoice.TenderAmount.ToString());
            //Ticket start:#21625 Add Invoice number and Integrated payment response in API log.by rupesh
            logRequestDetails.Add("InvoiceNumber", Invoice.Number);
            //Ticket end:#21625 .by rupesh
            //Ticket start:#63864 Hike app is crashing when processing payment through EFTPOS.by rupesh
            //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
            var url = string.Format("{0}?amount={1}&origin=https://{2}.hikeup.com&reference_id={3}&register_id={4}&tenant_id={5}&requestFrom=webview", gatewayUrl, (Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0)).ToString(), Settings.TenantName, Guid.NewGuid().ToString(), Settings.CurrentRegister.Id, Settings.TenantId);
            //End ticket #73190 By Rupesh
            //Ticket end:#63864 .by rupesh
            WebURL = url;
            logRequestDetails.Add("paymentURL", url);
            Extensions.SendLogsToServer(logService, paymentOption, logRequestDetails);
            //var text = "<html><body><script type='text/javascript'>window.addEventListener('message', function(event) {alert('This is response');}, false);</script><iframe src='https://hikepayment.hikeup.com/pay/example?amount=1.00&origin=https://devanghike3.hikeup.com&reference_id=2a376242-7717-400a-b3c1-18543b7a5eab'></iframe></body></html>";
            //HtmlText = text;
            //Ticket #11319 End : By Nikhil

            //if (Invoice.TenderAmount < 0)
            //{
            //    await PerformTyroRefundPaymentAsync((double)Invoice.TenderAmount * (-1));
            //}
            //else
            //{
            //    await PerformTyroPaymentAsync((double)Invoice.TenderAmount);
            //}
        }

        /* public Task<PaymentResult> PerformTyroRefundPaymentAsync(double amount)
         {
             string tempobject = "Amount: " + amount.ToString()
                 + " AccessToken: " + accessToken;
             SaveInLocalbeforePayment(tempobject, true);
             return PerformTyroOperationAsync(new TyroPaymentInput
             {
                 Amount = amount,
                 PaymentAction = TyroPaymentAction.Refund,
                 HandleAction = HandleAction,
                 AccessToken = accessToken
             });
         }

         public Task<PaymentResult> PerformTyroPaymentAsync(double amount)
         {

             string tempobject = "Amount: " + amount.ToString()
                 + " AccessToken: " + accessToken;
             SaveInLocalbeforePayment(tempobject, false);

             return PerformTyroOperationAsync(new TyroPaymentInput
             {
                 Amount = amount,
                 PaymentAction = TyroPaymentAction.Purachse,
                 HandleAction = HandleAction,
                 AccessToken = accessToken
             });
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
         }*/


        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
        {
            try
            {
                //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
                if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
                {
                    return;
                }
                //Ticket end:#24764 .by rupesh

                string currentPaymentObject = string.Empty;
                if (isRefund)
                    currentPaymentObject = "Custom Payment Refund : ";
                else
                    currentPaymentObject = "Custom Payment Sale :  ";


                currentPaymentObject = currentPaymentObject + " : " + paymentObject;



                Invoice.CurrentPaymentObject = currentPaymentObject;
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
