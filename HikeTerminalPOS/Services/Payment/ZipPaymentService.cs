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
    public class ZipPaymentService
    {
        private readonly IApiService<IZipPaymentService> _zipPaymentService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;

        public ZipPaymentService(IApiService<IZipPaymentService> apiService)
        {

            _zipPaymentService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }


        public async Task<ZipResponeObject> SendZipPurchaseRequest(Priority priority, ZipPurchaseRequestObject zipPurchaseRequestObject, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (zipPurchaseRequestObject == null)
                {
                    return null;
                }

                ResponseModel<object> zipPayResponse = null;

                Task<ResponseModel<object>> zipPayTask;

                Debug.WriteLine("ZipPayTask Request:" + (zipPurchaseRequestObject.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                zipPayTask = _zipPaymentService.Background.SendZipPurchaseRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                zipPayTask = _zipPaymentService.UserInitiated.SendZipPurchaseRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                zipPayTask = _zipPaymentService.Speculative.SendZipPurchaseRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                            default:
                                zipPayTask = _zipPaymentService.UserInitiated.SendZipPurchaseRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                        }


                        zipPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await zipPayTask);

                        string s = JsonConvert.SerializeObject(zipPayResponse.result);
                        ZipResponeObject result = JsonConvert.DeserializeObject<ZipResponeObject>(s);
                        //RootObject result = afterPayResponse.ToString();

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendZipPurchaseRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        zipPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (zipPayResponse != null && zipPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendZipPurchaseRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_zipPaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("SendZipPurchaseRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }



        public async Task<ZipResponeObject> SendZipRefundRequest(Priority priority, long zipId,ZipRefundRequestObject zipRefundRequestObject, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (zipRefundRequestObject == null)
                {
                    return null;
                }

                ResponseModel<object> zipPayResponse = null;

                Task<ResponseModel<object>> zipPayTask;

                Debug.WriteLine("ZipPayTask Request:" + (zipRefundRequestObject.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                zipPayTask = _zipPaymentService.Background.SendZipRefundRequest(zipId,zipRefundRequestObject, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                zipPayTask = _zipPaymentService.UserInitiated.SendZipRefundRequest(zipId,zipRefundRequestObject, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                zipPayTask = _zipPaymentService.Speculative.SendZipRefundRequest(zipId,zipRefundRequestObject, Settings.AccessToken);
                                break;
                            default:
                                zipPayTask = _zipPaymentService.UserInitiated.SendZipRefundRequest(zipId,zipRefundRequestObject, Settings.AccessToken);
                                break;
                        }

                        zipPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await zipPayTask);


                        string s = JsonConvert.SerializeObject(zipPayResponse.result);
                        ZipResponeObject result = JsonConvert.DeserializeObject<ZipResponeObject>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendZipRefundRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        zipPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (zipPayResponse != null && zipPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendZipRefundRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_zipPaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("SendZipRefundRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }

        public async Task<ZipResponeObject> GetZipPurchaseRequest(Priority priority,long zipId, ZipConfiguration zipConfiguration, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (zipConfiguration == null)
                {
                    return null;
                }

                ResponseModel<object> zipPayResponse = null;

                Task<ResponseModel<object>> zipPayTask;

                Debug.WriteLine("ZipPayTask Request:" + (zipConfiguration.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                zipPayTask = _zipPaymentService.Background.GetZipPurchaseRequest(zipId,zipConfiguration, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                zipPayTask = _zipPaymentService.UserInitiated.GetZipPurchaseRequest(zipId, zipConfiguration, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                zipPayTask = _zipPaymentService.Speculative.GetZipPurchaseRequest(zipId,zipConfiguration, Settings.AccessToken);
                                break;
                            default:
                                zipPayTask = _zipPaymentService.UserInitiated.GetZipPurchaseRequest(zipId,zipConfiguration, Settings.AccessToken);
                                break;
                        }

                        zipPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await zipPayTask);


                        string s = JsonConvert.SerializeObject(zipPayResponse.result);
                        ZipResponeObject result = JsonConvert.DeserializeObject<ZipResponeObject>(s);

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("GetZipPurchaseRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh


                        // Get Exception content
                        zipPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (zipPayResponse != null && zipPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("GetZipPurchaseRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_zipPaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("GetZipPurchaseRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }

            return null;
        }

        public async Task<bool> SendZipVoidRequest(Priority priority, ZipPurchaseRequestObject zipPurchaseRequestObject, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (zipPurchaseRequestObject == null)
                {
                    return false;
                }

                ResponseModel<object> zipPayResponse = null;

                Task<ResponseModel<object>> zipPayTask;

                Debug.WriteLine("ZipPayTask Request:" + (zipPurchaseRequestObject.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                zipPayTask = _zipPaymentService.Background.SendZipVoidRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                zipPayTask = _zipPaymentService.UserInitiated.SendZipVoidRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                zipPayTask = _zipPaymentService.Speculative.SendZipVoidRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                            default:
                                zipPayTask = _zipPaymentService.UserInitiated.SendZipVoidRequest(zipPurchaseRequestObject, Settings.AccessToken);
                                break;
                        }


                        zipPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await zipPayTask);

                        string s = JsonConvert.SerializeObject(zipPayResponse);
                        return zipPayResponse.success;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("SendZipVoidRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        zipPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (zipPayResponse != null && zipPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("SendZipVoidRequest Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_zipPaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("SendZipVoidRequest Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return false;
            }

            return false; 
        }

    }
}
