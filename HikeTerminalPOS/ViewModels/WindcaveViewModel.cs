using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.Services.Payment;
using Newtonsoft.Json;

namespace HikePOS.ViewModels
{
    public class WindcaveViewModel
    {

        ApiService<IWindcavePaymentService> windcavePaymentApi = new ApiService<IWindcavePaymentService>();
        public WindcavePaymentService windcavePaymentService;


        LocalPaymentViewModel localPaymentViewModel;
        private WindcaveConfiguration windcaveConfiguration;


        string UserMessage = string.Empty;
        string TxnRef = string.Empty;


        public WindcaveViewModel()
        {

            windcavePaymentService = new WindcavePaymentService(windcavePaymentApi);
            localPaymentViewModel = new LocalPaymentViewModel();
        }

        
        public async Task<WindcaveRoot> WindcaveTransaction(PaymentOptionDto paymentOption, InvoiceDto invoice,  bool isRefund)
        {

            try
            {

                
                TxnRef = Guid.NewGuid().ToString();

                windcaveConfiguration = JsonConvert.DeserializeObject<WindcaveConfiguration>(paymentOption.ConfigurationDetails);

                string transactionType = string.Empty;
                if (isRefund)
                    transactionType = "Refund";
                else
                    transactionType = "Purchase";





                WindcaveRequest request = new WindcaveRequest()
                {
                    station = windcaveConfiguration.StationId,
                    key = windcaveConfiguration.Key,
                    user = windcaveConfiguration.User,
                    deviceId = "device1",
                    posName = "hikepos",
                    vendorId= "PXVendor",
                    posVersion= "3.1",
                    //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                    amount = invoice.TenderAmount.ToPositive() + (paymentOption.DisplaySurcharge ?? 0),
                    //End ticket #73190. Rupesh
                    action = "doScrHIT",
                    mRef = string.IsNullOrEmpty(invoice.Number) ? invoice.InvoiceTempId :  invoice.Number,
                    cur = invoice.Currency,
                    txnType = transactionType,
                    txnRef = TxnRef
                };
                WindcaveRoot result = null;


                string tempPaymentObject = paymentOption.PaymentOptionName + " : " + Newtonsoft.Json.JsonConvert.SerializeObject(request);

                localPaymentViewModel.SaveInLocalbeforePayment(invoice, paymentOption, tempPaymentObject);

                if (!isRefund)
                    result = await windcavePaymentService.CreateWindcaveSale(Priority.UserInitiated, request);
                else
                    result = await windcavePaymentService.CreateWindcaveRefund(Priority.UserInitiated, request);


                if (result != null)
                {

                    Retry:

                    if (result.result.complete == "0")
                    {
                       
                        if (!string.IsNullOrEmpty(result.result.dL1))
                            UserMessage = result.result.dL1;
                        if (!string.IsNullOrEmpty(result.result.dL2))
                            UserMessage = UserMessage + "  " +result.result.dL2;

                        App.Instance.Hud.DisplayToast(UserMessage,Colors.Gray, Colors.Blue);

              
                        result = await CheckTransctionStatus();
                        if (result != null)
                        {

                            if (result.result.complete == "0")
                            {
                                if (result.result.b1.en == "0" || result.result.b2.en == "0")
                                {
                                    goto Retry;
                                }
                                
                            }
                            else
                            {

                                if (!string.IsNullOrEmpty(result.result.dL1))
                                    UserMessage = result.result.dL1;
                                if (!string.IsNullOrEmpty(result.result.dL2))
                                    UserMessage = UserMessage + "  " + result.result.dL2;

                                App.Instance.Hud.DisplayToast(UserMessage, Colors.Gray, Colors.Blue);

                                result.result.IsintegratedReceipt = windcaveConfiguration.integratedReceipt;
                                result.result.rcpt = CreateReceipt(result.result.rcpt, Convert.ToInt32(result.result.rcptW));

                                //if (result.result.result.ap == "1" && result.result.result.rt == "APPROVED")
                                //{
                                //    return result;
                                //}
                                //else
                                {
                                    return result;
                                }
                            }
                        }

                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(result.result.dL1))
                            UserMessage = result.result.dL1;
                        if (!string.IsNullOrEmpty(result.result.dL2))
                            UserMessage = UserMessage + "  " + result.result.dL2;

                        App.Instance.Hud.DisplayToast(UserMessage, Colors.Gray, Colors.Blue);

                        result.result.IsintegratedReceipt = windcaveConfiguration.integratedReceipt;
                        result.result.rcpt = CreateReceipt(result.result.rcpt, Convert.ToInt32(result.result.rcptW));

                        if (result.result.result.ap == "1"  && result.result.result.rt == "APPROVED")
                        {
                            return result;
                        }
                        else
                        {
                            return result;
                        }


                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return null;
        }



       private async Task<WindcaveRoot> CheckTransctionStatus()
        {
            try
            {


                WindcaveStatusCheckDTO statusRequest = new WindcaveStatusCheckDTO()
                {
                    action = "doScrHIT",
                    key = windcaveConfiguration.Key,
                    station = windcaveConfiguration.StationId,
                    txnType = "Status",
                    txnRef = TxnRef,
                    user = windcaveConfiguration.User

                };


                return await windcavePaymentService.CheckWindcaveStatusCheck(Priority.UserInitiated, statusRequest);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

       private string CreateReceipt(string rcpt, int length)
        {

            string result = string.Empty;
            try
            {

                var i = 0;
                while (rcpt.Length > 0)
                {
                    if (i == 0)
                    {
                        result += result += "\r\n\r\n";
                    }


                    if (rcpt.Length >= length)
                        result += rcpt.Substring(0, length) + "\n";

                    if (i == 1)
                    {
                        result += "\r\n\r\n";
                        //result +=  "\n" + "\n" ;
                    }

                    if (rcpt.Length >= length)
                    {
                        rcpt = rcpt.Substring(length);
                        i++;
                    }
                    else
                    {
                        result += rcpt;
                        break;

                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return result;
        }

       public async Task<WindcaveRoot> WindcaveButtonAction(WindcaveRoot windcaveResult, string butonValue)
       {

            try
            {

                if (windcaveResult == null || windcaveConfiguration == null)
                    return null;


                string buttonName = string.Empty;
                if (windcaveResult.result.b1.text.ToString().ToLower() == butonValue.ToLower())
                {
                    buttonName = "B1";
                }
                else
                {
                    buttonName = "B2";
                }

                WindcaveButtonTransactionRequest request = new WindcaveButtonTransactionRequest()
                {
                    action = "doScrHIT",
                    key = windcaveConfiguration.Key,
                    name = buttonName,
                    station = windcaveConfiguration.StationId,
                    txnRef = windcaveResult.result.txnRef,
                    txnType = "UI",
                    uiType = "Bn",
                    user = windcaveConfiguration.User,
                    val = butonValue.ToUpper()
                };

                var result = await windcavePaymentService.WindcaveButtonTransaction(Priority.UserInitiated, request);

                if (result != null)
                {

                    if (result.success)
                    {
                            Retry:
                            var statusResponse = await CheckTransctionStatus();


                            Debug.WriteLine("result status  after button : " + Newtonsoft.Json.JsonConvert.SerializeObject(statusResponse));

                        if (statusResponse.result.complete == "0")
                        {
                            if (statusResponse.result.b1.en == "0" && statusResponse.result.b1.en == "0")
                            {
                                goto Retry;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(statusResponse.result.dL1))
                                UserMessage = statusResponse.result.dL1;
                            if (!string.IsNullOrEmpty(statusResponse.result.dL2))
                                UserMessage = UserMessage + "  " + statusResponse.result.dL2;

                            App.Instance.Hud.DisplayToast(UserMessage, Colors.Gray, Colors.Blue);

                            statusResponse.result.IsintegratedReceipt = windcaveConfiguration.integratedReceipt;
                            statusResponse.result.rcpt = CreateReceipt(statusResponse.result.rcpt, Convert.ToInt32(statusResponse.result.rcptW));

                            
                                return statusResponse;
                            
                        }
                            Debug.WriteLine("result :" + Newtonsoft.Json.JsonConvert.SerializeObject(statusResponse));
                 
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;

            
        }

    }
}
