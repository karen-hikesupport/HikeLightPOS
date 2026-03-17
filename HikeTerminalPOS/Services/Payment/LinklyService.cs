using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Newtonsoft.Json;
using Polly;
using Refit;

namespace HikePOS.Services.Payment
{
    public class LinklyPaymentService
    {
        private readonly IApiService<ILinklyAPI> linklyApiService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;
        double[] delayTimes = new double[] { 15, 15 };

        public string ErrorMessage { get; set; }

        public LinklyPaymentService(IApiService<ILinklyAPI> apiService)
        {
            linklyApiService = apiService;
            ErrorMessage = string.Empty;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        string GetCurrency()
        {
            return Settings.StoreZoneAndFormatDetail?.Currency;
        }

        public async Task<ResponseModel<LinklyResponseRoot>> CreateLinklySaleTransaction(Priority priority, PaymentOptionDto paymentOptionDto, LinklySaleRootRequest linklyRequestDto, string sessionId)
        {
            try
            {
               
                ResponseModel<LinklyResponseRoot> response = null;

                Task<ResponseModel<LinklyResponseRoot>> linklyTask;

                //var amountInCents = invoice.TenderAmount * 100;

                int paymentId = paymentOptionDto.Id;
                //string sessionId = Guid.NewGuid().ToString();

                Debug.WriteLine("paymentId : " + paymentId.ToString());
                Debug.WriteLine("sessionId : " + sessionId);
                Debug.WriteLine("Linkly invoice : " + JsonConvert.SerializeObject(linklyRequestDto));
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                linklyTask = linklyApiService.Background.LinklySaleTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                linklyTask = linklyApiService.UserInitiated.LinklySaleTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                linklyTask = linklyApiService.Speculative.LinklySaleTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                            default:
                                linklyTask = linklyApiService.Background.LinklySaleTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await linklyTask);

                        LinklyResponseRoot result = response.result;
                        int delayCounter = 0;
                        GetTransactionStatus:
                        Debug.WriteLine("LinklyResponseDto : " + JsonConvert.SerializeObject(result));

                        if (result != null && result.statusCode != null)
                        {

                            int statusCode = int.Parse(result.statusCode);

                           
                            if (statusCode == 200)
                            {
                                return response;
                            }

                            if (!string.IsNullOrEmpty(result.statusDescription))
                            {
                                ErrorMessage = result.statusDescription;
                            }

                            if (statusCode == 202 || statusCode == 408 ||
                               (statusCode >= 500
                                        && statusCode <= 599))
                            {
                                if (delayCounter > delayTimes.Length)
                                {
                                   
                                    App.Instance.Hud.DisplayToast(result.statusDescription, Colors.Red, Colors.White);
                                }
                                else
                                {
                                    
                                    await Task.Delay(TimeSpan.FromSeconds(delayTimes[delayCounter]));
                                    delayCounter++;
                                    Debug.WriteLine("delayCounter : " + delayCounter.ToString());
                                    result = await GetLinklyTransactionStatus(priority, paymentOptionDto, sessionId);
                                    goto GetTransactionStatus;
                                }
                            }
                            
                            return response;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateLinklySaleTransaction Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<LinklyResponseRoot>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry1;
                            }
                        }


                        
                    }
                    catch (Exception ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateLinklySaleTransaction Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {

                            if (!string.IsNullOrEmpty(ex.Message))
                            {
                                if (ex.Message == "An error occurred while sending the request")
                                {   
                                    bool isReachable = await CommonMethods.ReachableCheck(linklyApiService.ApiBaseAddress);
                                    if (!isReachable)
                                    {
                                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                    }
                                }
                                else
                                {


                                    ErrorMessage = (ex.Message.ToString());
                                    App.Instance.Hud.DisplayToast(ex.Message.ToString(), Colors.Red, Colors.White);
                                }
                            }

                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("CreateLinklySaleTransaction Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }

        public async Task<ResponseModel<LinklyResponseRoot>> CreateLinklyRefundTransaction(Priority priority, PaymentOptionDto paymentOptionDto, LinklyRefundRootRequest linklyRequestDto, string sessionId)
        {
            try
            {
                ResponseModel<LinklyResponseRoot> response = null;

                Task<ResponseModel<LinklyResponseRoot>> linklyTask;

               // var amountInCents = invoice.TenderAmount * 100;

                int paymentId = paymentOptionDto.Id;
                //string sessionId = Guid.NewGuid().ToString();



                Debug.WriteLine("paymentId : " + paymentId.ToString());
                Debug.WriteLine("sessionId : " + sessionId);
                Debug.WriteLine("Linkly invoice : " + JsonConvert.SerializeObject(linklyRequestDto));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                linklyTask = linklyApiService.Background.LinklyRefundTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                linklyTask = linklyApiService.UserInitiated.LinklyRefundTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                linklyTask = linklyApiService.Speculative.LinklyRefundTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                            default:
                                linklyTask = linklyApiService.Background.LinklyRefundTransaction(paymentId, sessionId, linklyRequestDto, Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await linklyTask);


                        LinklyResponseRoot result = response.result;
                        int delayCounter = 0;
                    GetTransactionStatus:
                        Debug.WriteLine("LinklyResponseDto : " + JsonConvert.SerializeObject(result));

                        if (result != null && result.statusCode != null)
                        {
                            int statusCode = int.Parse(result.statusCode);

                            if (!string.IsNullOrEmpty(result.statusDescription))
                            {
                                ErrorMessage = result.statusDescription;
                            }

                            if (statusCode == 202 || statusCode == 408 ||
                               (statusCode >= 500
                                        && statusCode <= 599))
                            {
                                if (delayCounter > delayTimes.Length)
                                {
                                    
                                    App.Instance.Hud.DisplayToast(result.statusDescription, Colors.Red, Colors.White);
                                }
                                else
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(delayTimes[delayCounter]));
                                    delayCounter++;
                                    result = await GetLinklyTransactionStatus(priority, paymentOptionDto, sessionId);
                                    //result = await GetLinklyTransactionStatus(priority, paymentOptionDto);
                                    goto GetTransactionStatus;
                                }
                            }
                            else
                            {

                            }
                            return response;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateLinklyRefundTransaction Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<LinklyResponseRoot>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateLinklyRefundTransaction Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(linklyApiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                                else
                                {


                                    ErrorMessage = (ex.Message.ToString());
                                    App.Instance.Hud.DisplayToast(ex.Message.ToString(), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("CreateLinklyRefundTransaction Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }

        public async Task<LinklyResponseRoot> GetLinklyTransactionStatus(Priority priority, PaymentOptionDto paymentOptionDto, string sessionId)
        {
            try
            {
                ResponseModel<LinklyResponseRoot> response = null;

                Task<ResponseModel<LinklyResponseRoot>> linklyTask;

                int paymentId = paymentOptionDto.Id;
                //string sessionId = Guid.NewGuid().ToString();

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                linklyTask = linklyApiService.Background.GetLinklyTransaction(paymentId, sessionId, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                linklyTask = linklyApiService.UserInitiated.GetLinklyTransaction(paymentId, sessionId, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                linklyTask = linklyApiService.Speculative.GetLinklyTransaction(paymentId, sessionId, Settings.AccessToken);
                                break;
                            default:
                                linklyTask = linklyApiService.Background.GetLinklyTransaction(paymentId, sessionId, Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await linklyTask);

                        LinklyResponseRoot result = response.result;
                        return result;

                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("GetLinklyTransactionStatus Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<LinklyResponseRoot>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("GetLinklyTransactionStatus Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(linklyApiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("GetLinklyTransactionStatus Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }


    }
}
