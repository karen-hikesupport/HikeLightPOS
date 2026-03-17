
/// <summary>
/// This class is used to update payment locally before integrated payment.
/// Note : Same code is also added in payment view model and other payments which are integrated previoulsy.
/// As of now, we have used it only for windcave payment. later on, we will use for all payments
/// </summary>


using System;
using System.Diagnostics;
using HikePOS.Enums;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class LocalPaymentViewModel
    {
       

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;


        public LocalPaymentViewModel()
        {
            saleService = new SaleServices(saleApiService);
        }



        #region Save invoice in local database before payment
        public async void SaveInLocalbeforePayment(InvoiceDto Invoice, PaymentOptionDto paymentOption, string currentPaymentObject)
        {

            try
            {
                //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
                if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
                {
                    return;
                }
                //Ticket end:#24764 .by rupesh

                Invoice.CurrentPaymentObject = paymentOption.PaymentOptionName + " : " + currentPaymentObject;
                Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;

                
                await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveInLocalbeforePayment : " + ex.ToString());
            }

        }
        #endregion
    }
}
