using System.Diagnostics;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Newtonsoft.Json;
using Polly;
using Refit;
namespace HikePOS.Services.Payment
{
    public class HikePayService
    {
        private readonly IApiService<IHikePayService> _hikePayService;
        readonly IApiService<IAccountApi> accountApiService;
        readonly AccountServices accountService;

        public HikePayService(IApiService<IHikePayService> apiService)
        {

            _hikePayService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }
        public async Task<ResponseModel<HikePaySplitProfile>> GetSplitProfile(Priority priority)
        {
            try
            {

                ResponseModel<HikePaySplitProfile> hikePayResponse = null;
                Task<ResponseModel<HikePaySplitProfile>> hikePayTask;
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                hikePayTask = _hikePayService.Background.GetSplitProfile(Settings.TenantId, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                hikePayTask = _hikePayService.UserInitiated.GetSplitProfile(Settings.TenantId, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                hikePayTask = _hikePayService.Speculative.GetSplitProfile(Settings.TenantId, Settings.AccessToken);
                                break;
                            default:
                                hikePayTask = _hikePayService.UserInitiated.GetSplitProfile(Settings.TenantId, Settings.AccessToken);
                                break;
                        }


                        hikePayResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await hikePayTask);

                        string s = JsonConvert.SerializeObject(hikePayResponse);
                        ResponseModel<HikePaySplitProfile> result = JsonConvert.DeserializeObject<ResponseModel<HikePaySplitProfile>>(s);
                        UpdateHikePaySplitProfile(result.result);
                        return result;
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("GetSplitProfileRequest Exception Msg - " + ex.Message);

                        // Get Exception content
                        hikePayResponse = await ex.GetContentAsAsync<ResponseModel<HikePaySplitProfile>>();
                        if (hikePayResponse != null && hikePayResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("GetSplitProfileRequest Exception Msg - " + ex.Message);

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_hikePayService.ApiBaseAddress);
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
                Logger.SyncLogger("GetSplitProfileRequest Exception Msg - " + ex.Message);

                ex.Track();
                return null;
            }

            return null;
        }
        public bool UpdateHikePaySplitProfile(HikePaySplitProfile hikePaySplitProfile)
        {
            try
            {
                if (hikePaySplitProfile != null)
                {

                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(hikePaySplitProfile.ToModel());
                    });
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }
        public HikePaySplitProfile GetLocalHikePaySplitProfile()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var hikePaySplitProfile = realm.All<HikePaySplitProfileDB>().FirstOrDefault();
                return HikePaySplitProfile.FromModel(hikePaySplitProfile);

            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public async Task<ResponseModel<HikePayTerminalDetailData>> GetHikePayTerminals(Priority priority,string serialNumber)
        {
            try
            {

                ResponseModel<HikePayTerminalDetailData> hikePayTerminalResponse = null;
                Task<ResponseModel<HikePayTerminalDetailData>> hikePayTerminalTask;
                var request = new HikePayTerminalDetailRequest
                {
                    serialNumber = serialNumber,
                    pageNumber = 1,
                    pageSize = 1
                };
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                hikePayTerminalTask = _hikePayService.Background.GetHikePayTerminals(request, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                hikePayTerminalTask = _hikePayService.UserInitiated.GetHikePayTerminals(request, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                hikePayTerminalTask = _hikePayService.Speculative.GetHikePayTerminals(request, Settings.AccessToken);
                                break;
                            default:
                                hikePayTerminalTask = _hikePayService.UserInitiated.GetHikePayTerminals(request, Settings.AccessToken);
                                break;
                        }


                        hikePayTerminalResponse = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await hikePayTerminalTask);

                        string s = JsonConvert.SerializeObject(hikePayTerminalResponse);
                        ResponseModel<HikePayTerminalDetailData> result = JsonConvert.DeserializeObject<ResponseModel<HikePayTerminalDetailData>>(s);
                        return result;
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("GetHikePayTerminals Exception Msg - " + ex.Message);

                        // Get Exception content
                        hikePayTerminalResponse = await ex.GetContentAsAsync<ResponseModel<HikePayTerminalDetailData>>();
                        if (hikePayTerminalResponse != null && hikePayTerminalResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("GetSplitProfileRequest Exception Msg - " + ex.Message);

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_hikePayService.ApiBaseAddress);
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
                Logger.SyncLogger("GetSplitProfileRequest Exception Msg - " + ex.Message);

                ex.Track();
                return null;
            }

            return null;
        }
    }
}
