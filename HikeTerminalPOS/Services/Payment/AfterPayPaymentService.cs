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
    public class AfterPayPaymentService
    {

        private readonly IApiService<IAfterPayPaymentService> _iAfterPayPaymentService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;

        public AfterPayPaymentService(IApiService<IAfterPayPaymentService> apiService)
        {

            _iAfterPayPaymentService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }


        public async Task<AfterPayResponseRootObject> CreatAfterPayPayment(Priority priority, AfterPayRequestObject afterPayRequestObject, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (afterPayRequestObject == null)
                {
                    return null;
                }
              
                ResponseModel<object> afterPayResponse = null;

                Task<ResponseModel<object>> afterPayTask;

                Debug.WriteLine("afterPayTask Request:" + (afterPayRequestObject.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterpayPayment(afterPayRequestObject, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterpayPayment(afterPayRequestObject, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterpayPayment(afterPayRequestObject, Settings.AccessToken);
                                break;
                            default:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterpayPayment(afterPayRequestObject, Settings.AccessToken);
                                break;
                        }

                        //var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>("{\"result\":{\"orderId\":\"24387013\",\"orderedAt\":\"2019-12-05T03:33:23.348Z\",\"errorCode\":null,\"errorId\":null,\"message\":null,\"httpStatusCode\":0},\"targetUrl\":null,\"success\":true,\"error\":null,\"unAuthorizedRequest\":false,\"__abp\":true}");
                        //var temp = (RootObject)Newtonsoft.Json.JsonConvert.DeserializeObject("{\"result\":{\"orderId\":\"24387013\",\"orderedAt\":\"2019-12-05T03:33:23.348Z\",\"errorCode\":\"sdfd\",\"errorId\":\"sdfd\",\"message\":\"message\",\"httpStatusCode\":0},\"targetUrl\":\"sdf\",\"success\":true,\"error\":\"Fdsfds\",\"unAuthorizedRequest\":false,\"__abp\":true}");

                        afterPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await afterPayTask);

                        // object rs = econduitResponse.result;


                        // EconduitResponse result = JsonConvert.DeserializeObject<EconduitResponse>(econduitResponse.result.ToString());

                 
                       // RootObject result = JsonConvert.DeserializeObject(<RootObject> afterPayResponse.ToString());

                       //RootObject result = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(afterPayResponse.ToString());
                      //  Debug.WriteLine("econduitResponse payment Response: " + JsonConvert.SerializeObject(result));

                        string s = JsonConvert.SerializeObject(afterPayResponse);
                        AfterPayResponseRootObject result = JsonConvert.DeserializeObject<AfterPayResponseRootObject>(s);
                       //RootObject result = afterPayResponse.ToString();

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        // Get Exception content
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreatAfterPayPayment Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        afterPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (afterPayResponse != null && afterPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CreatAfterPayPayment Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_iAfterPayPaymentService.ApiBaseAddress);
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
                Logger.SaleLogger("CreatAfterPayPayment Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh
                ex.Track();
                return null;
            }

            return null;
        }



        public async Task<AfterpayRefundResponseRootObject> CreatAfterPayRefund(Priority priority, AfterpayRefundRequestRootObject afterpayRefundRequestRootObject, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (afterpayRefundRequestRootObject == null)
                {
                    return null;
                }

                ResponseModel<object> afterPayResponse = null;

                Task<ResponseModel<object>> afterPayTask;

                Debug.WriteLine("afterPayTask Request:" + (afterpayRefundRequestRootObject.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterPayRefundOrder(afterpayRefundRequestRootObject, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterPayRefundOrder(afterpayRefundRequestRootObject, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterPayRefundOrder(afterpayRefundRequestRootObject, Settings.AccessToken);
                                break;
                            default:
                                afterPayTask = _iAfterPayPaymentService.Background.CreateAfterPayRefundOrder(afterpayRefundRequestRootObject, Settings.AccessToken);
                                break;
                        }

                        //var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>("{\"result\":{\"orderId\":\"24387013\",\"orderedAt\":\"2019-12-05T03:33:23.348Z\",\"errorCode\":null,\"errorId\":null,\"message\":null,\"httpStatusCode\":0},\"targetUrl\":null,\"success\":true,\"error\":null,\"unAuthorizedRequest\":false,\"__abp\":true}");
                        //var temp = (RootObject)Newtonsoft.Json.JsonConvert.DeserializeObject("{\"result\":{\"orderId\":\"24387013\",\"orderedAt\":\"2019-12-05T03:33:23.348Z\",\"errorCode\":\"sdfd\",\"errorId\":\"sdfd\",\"message\":\"message\",\"httpStatusCode\":0},\"targetUrl\":\"sdf\",\"success\":true,\"error\":\"Fdsfds\",\"unAuthorizedRequest\":false,\"__abp\":true}");

                        afterPayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await afterPayTask);

                        // object rs = econduitResponse.result;


                        // EconduitResponse result = JsonConvert.DeserializeObject<EconduitResponse>(econduitResponse.result.ToString());


                        // RootObject result = JsonConvert.DeserializeObject(<RootObject> afterPayResponse.ToString());

                        //RootObject result = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(afterPayResponse.ToString());
                        //  Debug.WriteLine("econduitResponse payment Response: " + JsonConvert.SerializeObject(result));

                        string s = JsonConvert.SerializeObject(afterPayResponse);
                        AfterpayRefundResponseRootObject result = JsonConvert.DeserializeObject<AfterpayRefundResponseRootObject>(s);
                        //RootObject result = afterPayResponse.ToString();

                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreatAfterPayRefund Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        // Get Exception content
                        afterPayResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (afterPayResponse != null && afterPayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CreatAfterPayRefund Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_iAfterPayPaymentService.ApiBaseAddress);
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
                //Ticket start:#61832 iPad:Create text file for invoice log.by pratik
                Logger.SaleLogger("CreatAfterPayRefund Exception Msg - " + ex.Message);
                //Ticket end:#61832.by pratik
                ex.Track();
                return null;
            }

            return null;
        }


    }
}
