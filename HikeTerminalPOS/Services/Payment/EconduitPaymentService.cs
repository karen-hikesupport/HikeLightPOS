using System;
using System.Diagnostics;
using System.Drawing;
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
    public class EConduitPaymentService 
    {
        private readonly IApiService<IEconduitPaymentApi> _ieconduitPaymentAPi;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;
       

        public EConduitPaymentService(IApiService<IEconduitPaymentApi> apiService)
        {
            _ieconduitPaymentAPi = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        public async Task<EconduitResponse> CreateconduitPayment(Priority priority, EconduitRequestObject econduitRequest, [Header("Authorization")] string accessToken)
        {

            try
            {
               
                if (econduitRequest == null)
                {
                    return null;
                }
                ResponseModel<object> econduitResponse = null;
              
                Task<ResponseModel<object>> econduitTask;

               Debug.WriteLine("econduit Request:" + JsonConvert.SerializeObject(econduitRequest));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                econduitTask = _ieconduitPaymentAPi.Background.CreateconduitPayment(econduitRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                econduitTask = _ieconduitPaymentAPi.Background.CreateconduitPayment(econduitRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                econduitTask = _ieconduitPaymentAPi.Background.CreateconduitPayment(econduitRequest, Settings.AccessToken);
                                break;
                            default:
                                econduitTask = _ieconduitPaymentAPi.Background.CreateconduitPayment(econduitRequest, Settings.AccessToken);
                                break;
                        }

                       econduitResponse = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await econduitTask);

                       // object rs = econduitResponse.result;

                         EconduitResponse result = JsonConvert.DeserializeObject<EconduitResponse>(econduitResponse.result.ToString());
                        
                        Debug.WriteLine("econduitResponse payment Response: "  + JsonConvert.SerializeObject(result));
                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateconduitPayment Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        econduitResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (econduitResponse != null && econduitResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CreateconduitPayment Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_ieconduitPaymentAPi.ApiBaseAddress);
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
                Logger.SaleLogger("CreateconduitPayment Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }



      
        public async Task<EconduitResponse> CloseconduitBatchRequest(Priority priority, EconduitRequestObject econduitRequest, [Header("Authorization")] string accessToken)
        {
            try
            {

                if (econduitRequest == null)
                {
                    return null;
                }
                ResponseModel<object> econduitResponse = null;

                Task<ResponseModel<object>> econduitTask;

                Debug.WriteLine("econduit Request:" + JsonConvert.SerializeObject(econduitRequest));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                econduitTask = _ieconduitPaymentAPi.Background.CloseconduitBatchRequest(econduitRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                econduitTask = _ieconduitPaymentAPi.Background.CloseconduitBatchRequest(econduitRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                econduitTask = _ieconduitPaymentAPi.Background.CloseconduitBatchRequest(econduitRequest, Settings.AccessToken);
                                break;
                            default:
                                econduitTask = _ieconduitPaymentAPi.Background.CloseconduitBatchRequest(econduitRequest, Settings.AccessToken);
                                break;
                        }

                        econduitResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await econduitTask);

                        // object rs = econduitResponse.result;

                        EconduitResponse result = JsonConvert.DeserializeObject<EconduitResponse>(econduitResponse.result.ToString());

                        Debug.WriteLine("econduitResponse payment Response: " + JsonConvert.SerializeObject(result));
                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CloseconduitBatchRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        econduitResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (econduitResponse != null && econduitResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CloseconduitBatchRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_ieconduitPaymentAPi.ApiBaseAddress);
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
                Logger.SaleLogger("CloseconduitBatchRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }
    }
}
