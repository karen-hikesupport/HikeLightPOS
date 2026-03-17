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
    public class ClearantPaymentService
    {
        private readonly IApiService<IClearantPaymentService> _clearantPaymentService;
        readonly IApiService<IAccountApi> accountApiService;
        readonly AccountServices accountService;

        public ClearantPaymentService(IApiService<IClearantPaymentService> apiService)
        {

            _clearantPaymentService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }


        public async Task<ClearantResponse> SendClearantSaleRequest(Priority priority, string apiKey, ClearantRequest clearantSaleRequest, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (clearantSaleRequest == null)
                {
                    return null;
                }

                ResponseModel<object> clearantPayResponse = null;

                Task<ResponseModel<object>> clearantPayTask;

                Debug.WriteLine("ClearantPayTask Request:" + (clearantSaleRequest.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                clearantPayTask = _clearantPaymentService.Background.ClearantSaleRequest(apiKey, clearantSaleRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearantSaleRequest(apiKey, clearantSaleRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                clearantPayTask = _clearantPaymentService.Speculative.ClearantSaleRequest(apiKey, clearantSaleRequest, Settings.AccessToken);
                                break;
                            default:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearantSaleRequest(apiKey, clearantSaleRequest, Settings.AccessToken);
                                break;
                        }


                        clearantPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await clearantPayTask);

                        string s = JsonConvert.SerializeObject(clearantPayResponse.result);
                        ClearantResponse result = JsonConvert.DeserializeObject<ClearantResponse>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendClearantSaleRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        // Get Exception content
                        clearantPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (clearantPayResponse != null && clearantPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendClearantSaleRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_clearantPaymentService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FailTitle"), Colors.Red, Colors.White);
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
                Logger.SaleLogger("SendClearantSaleRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh
                ex.Track();
                return null;
            }

            return null;
        }



        public async Task<ClearantResponse> SendClearantRefundRequest(Priority priority, string apiKey, ClearantRequest clearantRefundRequest, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (clearantRefundRequest == null)
                {
                    return null;
                }

                ResponseModel<object> clearantPayResponse = null;

                Task<ResponseModel<object>> clearantPayTask;

                Debug.WriteLine("ClearantPayTask Request:" + (clearantRefundRequest.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                clearantPayTask = _clearantPaymentService.Background.ClearentRefundRequest(apiKey, clearantRefundRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentRefundRequest(apiKey, clearantRefundRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                clearantPayTask = _clearantPaymentService.Speculative.ClearentRefundRequest(apiKey, clearantRefundRequest, Settings.AccessToken);
                                break;
                            default:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentRefundRequest(apiKey, clearantRefundRequest, Settings.AccessToken);
                                break;
                        }

                        clearantPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await clearantPayTask);


                        string s = JsonConvert.SerializeObject(clearantPayResponse.result);
                        ClearantResponse result = JsonConvert.DeserializeObject<ClearantResponse>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendClearantRefundRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        // Get Exception content
                        clearantPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (clearantPayResponse != null && clearantPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendClearantRefundRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_clearantPaymentService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FailTitle"), Colors.Red, Colors.White);
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
                Logger.SaleLogger("SendClearantRefundRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh
                ex.Track();
                return null;
            }

            return null;
        }

        public async Task<ClearantResponse> SendClearentMatchRefundRequest(Priority priority, string apiKey, ClearantRequest clearantMatchRefundRequest, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (clearantMatchRefundRequest == null)
                {
                    return null;
                }

                ResponseModel<object> clearantPayResponse = null;

                Task<ResponseModel<object>> clearantPayTask;

                Debug.WriteLine("ClearantPayTask Request:" + (clearantMatchRefundRequest.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                clearantPayTask = _clearantPaymentService.Background.ClearentMatchRefundRequest(apiKey, clearantMatchRefundRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentMatchRefundRequest(apiKey, clearantMatchRefundRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                clearantPayTask = _clearantPaymentService.Speculative.ClearentMatchRefundRequest(apiKey, clearantMatchRefundRequest, Settings.AccessToken);
                                break;
                            default:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentMatchRefundRequest(apiKey, clearantMatchRefundRequest, Settings.AccessToken);
                                break;
                        }

                        clearantPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await clearantPayTask);


                        string s = JsonConvert.SerializeObject(clearantPayResponse.result);
                        ClearantResponse result = JsonConvert.DeserializeObject<ClearantResponse>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendClearentMatchRefundRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        clearantPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (clearantPayResponse != null && clearantPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendClearentMatchRefundRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_clearantPaymentService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FailTitle"), Colors.Red, Colors.White);
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
                Logger.SaleLogger("SendClearentMatchRefundRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }

        public async Task<ClearantResponse> SendClearentVoidRequest(Priority priority, string apiKey, ClearantRequest clearantVoidRequest, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (clearantVoidRequest == null)
                {
                    return null;
                }

                ResponseModel<object> clearantPayResponse = null;

                Task<ResponseModel<object>> clearantPayTask;

                Debug.WriteLine("ClearantPayTask Request:" + (clearantVoidRequest.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                clearantPayTask = _clearantPaymentService.Background.ClearentVoidRequest(apiKey, clearantVoidRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentVoidRequest(apiKey, clearantVoidRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                clearantPayTask = _clearantPaymentService.Speculative.ClearentVoidRequest(apiKey, clearantVoidRequest, Settings.AccessToken);
                                break;
                            default:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentVoidRequest(apiKey, clearantVoidRequest, Settings.AccessToken);
                                break;
                        }

                        clearantPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await clearantPayTask);


                        string s = JsonConvert.SerializeObject(clearantPayResponse.result);
                        ClearantResponse result = JsonConvert.DeserializeObject<ClearantResponse>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendClearentVoidRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        clearantPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (clearantPayResponse != null && clearantPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendClearentVoidRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_clearantPaymentService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FailTitle"), Colors.Red, Colors.White);
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
                Logger.SaleLogger("SendClearentVoidRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }

        public async Task<ClearantBatchResponse> SendClearantBatchRequest(Priority priority, string apiKey, ClearantBatchRequest clearantBatchRequest, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (clearantBatchRequest == null)
                {
                    return null;
                }

                ResponseModel<object> clearantPayResponse = null;

                Task<ResponseModel<object>> clearantPayTask;

                Debug.WriteLine("ClearantPayTask Request:" + (clearantBatchRequest.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                clearantPayTask = _clearantPaymentService.Background.ClearentBatchRequest(apiKey, clearantBatchRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentBatchRequest(apiKey, clearantBatchRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                clearantPayTask = _clearantPaymentService.Speculative.ClearentBatchRequest(apiKey, clearantBatchRequest, Settings.AccessToken);
                                break;
                            default:
                                clearantPayTask = _clearantPaymentService.UserInitiated.ClearentBatchRequest(apiKey, clearantBatchRequest, Settings.AccessToken);
                                break;
                        }

                        clearantPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await clearantPayTask);


                        string s = JsonConvert.SerializeObject(clearantPayResponse.result);
                        ClearantBatchResponse result = JsonConvert.DeserializeObject<ClearantBatchResponse>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendClearantBatchRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        clearantPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (clearantPayResponse != null && clearantPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendClearantBatchRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_clearantPaymentService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FailTitle"), Colors.Red, Colors.White);
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
                Logger.SaleLogger("SendClearantBatchRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }
    }



}
