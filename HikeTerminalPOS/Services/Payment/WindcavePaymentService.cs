using System;
using System.Diagnostics;
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
    
    public class WindcavePaymentService //: IWindcavePaymentService
    {
      
        private readonly IApiService<IWindcavePaymentService> iWindcavePaymentService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;


        public WindcavePaymentService(IApiService<IWindcavePaymentService> apiService)
        {
            iWindcavePaymentService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }

        public async Task<WindcaveRoot> CheckWindcaveStatusCheck(Priority priority, [Body(BodySerializationMethod.Json)] WindcaveStatusCheckDTO input)
        {
            try
            {
                if (input == null)
                {
                    return null;
                }

                WindcaveRoot windcaveResponse = null;

                Task<WindcaveRoot> windcaveTask;

                Debug.WriteLine("CheckWindcaveStatusCheck Request:" + (input.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                windcaveTask = iWindcavePaymentService.Background.CheckWindcaveStatusCheck(input, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                windcaveTask = iWindcavePaymentService.Background.CheckWindcaveStatusCheck(input, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                windcaveTask = iWindcavePaymentService.Background.CheckWindcaveStatusCheck(input, Settings.AccessToken);
                                break;
                            default:
                                windcaveTask = iWindcavePaymentService.Background.CheckWindcaveStatusCheck(input, Settings.AccessToken);
                                break;
                        }


                        windcaveResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await windcaveTask);



                        Debug.WriteLine("windcave response: " + Newtonsoft.Json.JsonConvert.SerializeObject(windcaveResponse));

                      
                        return windcaveResponse;

                      
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CheckWindcaveStatusCheck Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        windcaveResponse = await ex.GetContentAsAsync<WindcaveRoot>();

                        if (windcaveResponse != null && windcaveResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CheckWindcaveStatusCheck Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(iWindcavePaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("CheckWindcaveStatusCheck Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }

        //public async Task<ResponseModel<WindcaveResult>> CreateWindcaveRefund([Body(BodySerializationMethod.Json)] WindcaveRequest input)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<WindcaveRoot> CreateWindcaveRefund(Priority priority, [Body(BodySerializationMethod.Json)] WindcaveRequest input)
        {
            try
            {
                if (input == null)
                {
                    return null;
                }

                WindcaveRoot windcaveResponse = null;

                Task<WindcaveRoot> windcaveTask;

                Debug.WriteLine("CreateWindcaveRefund Request:" + (input.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveRefund(input, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveRefund(input, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveRefund(input, Settings.AccessToken);
                                break;
                            default:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveRefund(input, Settings.AccessToken);
                                break;
                        }


                        windcaveResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await windcaveTask);



                        Debug.WriteLine("windcave response: " + Newtonsoft.Json.JsonConvert.SerializeObject(windcaveResponse));

                        //string s = JsonConvert.SerializeObject(windcaveResponse);
                        //WindcaveRoot result = JsonConvert.DeserializeObject<WindcaveRoot>(s);


                        return windcaveResponse;

                        //return windcaveResponse;// result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateWindcaveRefund Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        // windcaveResponse = await ex.GetContentAsAsync<ResponseModel<object>>();

                        windcaveResponse = await ex.GetContentAsAsync<WindcaveRoot>();

                        if (windcaveResponse != null && windcaveResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CreateWindcaveRefund Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(iWindcavePaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("CreateWindcaveRefund Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }


        //public async Task<AfterPayResponseRootObject> CreatAfterPayPayment(Priority priority, AfterPayRequestObject afterPayRequestObject, [Header("Authorization")] string accessToken)
        public async Task<WindcaveRoot> CreateWindcaveSale(Priority priority, [Body(BodySerializationMethod.Json)] WindcaveRequest input)
        {
            try
            {
                if (input == null)
                {
                    return null;
                }

                WindcaveRoot windcaveResponse = null;

                Task<WindcaveRoot> windcaveTask;

                Debug.WriteLine("CreateWindcaveSale Request:" + (input.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveSale(input, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveSale(input, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveSale(input, Settings.AccessToken);
                                break;
                            default:
                                windcaveTask = iWindcavePaymentService.Background.CreateWindcaveSale(input, Settings.AccessToken);
                                break;
                        }


                        windcaveResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await windcaveTask);



                        Debug.WriteLine("windcave response: " + Newtonsoft.Json.JsonConvert.SerializeObject(windcaveResponse));

                        //string s = JsonConvert.SerializeObject(windcaveResponse);
                        //WindcaveRoot result = JsonConvert.DeserializeObject<WindcaveRoot>(s);
                        

                        return windcaveResponse;

                        //return windcaveResponse;// result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateWindcaveSale Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        // windcaveResponse = await ex.GetContentAsAsync<ResponseModel<object>>();

                        windcaveResponse = await ex.GetContentAsAsync<WindcaveRoot>();

                        if (windcaveResponse != null && windcaveResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CreateWindcaveSale Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(iWindcavePaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("CreateWindcaveSale Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }


        public async Task<WindcaveButtonTransactionRoot> WindcaveButtonTransaction(Priority priority, [Body(BodySerializationMethod.Json)] WindcaveButtonTransactionRequest input)
        {
            try
            {
                if (input == null)
                {
                    return null;
                }

                WindcaveButtonTransactionRoot windcaveButtonTransactionRoot = null;

                Task<WindcaveButtonTransactionRoot> windcaveButtonTransactionTask;

                Debug.WriteLine("WindcaveButtonTransaction Request:" + (input.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                windcaveButtonTransactionTask = iWindcavePaymentService.Background.WindcaveButtonTransaction(input, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                windcaveButtonTransactionTask = iWindcavePaymentService.Background.WindcaveButtonTransaction(input, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                windcaveButtonTransactionTask = iWindcavePaymentService.Background.WindcaveButtonTransaction(input, Settings.AccessToken);
                                break;
                            default:
                                windcaveButtonTransactionTask = iWindcavePaymentService.Background.WindcaveButtonTransaction(input, Settings.AccessToken);
                                break;
                        }


                        windcaveButtonTransactionRoot = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await windcaveButtonTransactionTask);



                        Debug.WriteLine("WindcaveButtonTransaction response: " + Newtonsoft.Json.JsonConvert.SerializeObject(windcaveButtonTransactionRoot));

                
                        return windcaveButtonTransactionRoot;

                        //return windcaveResponse;// result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("WindcaveButtonTransaction Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        // windcaveResponse = await ex.GetContentAsAsync<ResponseModel<object>>();

                        windcaveButtonTransactionRoot = await ex.GetContentAsAsync<WindcaveButtonTransactionRoot>();

                        if (windcaveButtonTransactionRoot != null && windcaveButtonTransactionRoot.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("WindcaveButtonTransaction Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(iWindcavePaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("WindcaveButtonTransaction Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }
        
    }


}
