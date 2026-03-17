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


    public class VantivCloudPaymentService
    {

        private readonly IApiService<IVantivCloudPaymentService> _iVantivCloudPaymentService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;


        public VantivCloudPaymentService(IApiService<IVantivCloudPaymentService> apiService)
        {

            _iVantivCloudPaymentService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }


        public async Task<VantivClouldResponseObject> CreatVantivClouldPayment(Priority priority, VantivClouldRequestObject vantivClouldRequestObject, [Header("Authorization")] string accessToken)
        {

            try
            {

                if (vantivClouldRequestObject == null)
                {
                    return null;
                }

                ResponseModel<object> VantivCloudResponse = null;

                Task<ResponseModel<object>> VantivCloudPayTask;

                Debug.WriteLine("Vantiv Cloud Request:" + (vantivClouldRequestObject.ToJson()));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                VantivCloudPayTask = _iVantivCloudPaymentService.Background.CreateVantivClouldPayment(vantivClouldRequestObject, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                VantivCloudPayTask = _iVantivCloudPaymentService.Background.CreateVantivClouldPayment(vantivClouldRequestObject, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                VantivCloudPayTask = _iVantivCloudPaymentService.Background.CreateVantivClouldPayment(vantivClouldRequestObject, Settings.AccessToken);
                                break;
                            default:
                                VantivCloudPayTask = _iVantivCloudPaymentService.Background.CreateVantivClouldPayment(vantivClouldRequestObject, Settings.AccessToken);
                                break;
                        }

                        //   App.Instance.Hud.DisplayToast("Swipe or insert your card please!");

                        VantivCloudResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await VantivCloudPayTask);


                        string serializeVantivCloudResponse = JsonConvert.SerializeObject(VantivCloudResponse);

                        Debug.WriteLine("VantivCloud response:" + serializeVantivCloudResponse);
                        VantivClouldResponseObject result = JsonConvert.DeserializeObject<VantivClouldResponseObject>(serializeVantivCloudResponse);


                        return result;
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreatVantivClouldPayment Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        VantivCloudResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (VantivCloudResponse != null && VantivCloudResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SaleLogger("CreatVantivClouldPayment Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_iVantivCloudPaymentService.ApiBaseAddress);
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
                ex.Track();
                return null;
            }

            return null;
        }
    }
}
