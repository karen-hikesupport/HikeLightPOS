using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace HikePOS.Services
{
    public class RestaurantService
    {
        readonly IApiService<IRestaurantApi> _apiService;
        private readonly IApiService<IAccountApi> accountApiService;
        readonly AccountServices accountService;

        public RestaurantService(IApiService<IRestaurantApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);

        }

        public async Task<FloorDto> GetFloor(Priority priority, int floorId)
        {
            try
            {
                ResponseModel<FloorDto> floorResponse = null;
                Task<ResponseModel<FloorDto>> floorTask;

            Retry1:
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                floorTask = _apiService.Background.GetFloor(Settings.AccessToken, floorId);
                                break;
                            case Priority.UserInitiated:
                                floorTask = _apiService.UserInitiated.GetFloor(Settings.AccessToken, floorId);
                                break;
                            case Priority.Speculative:
                                floorTask = _apiService.Speculative.GetFloor(Settings.AccessToken, floorId);
                                break;
                            default:
                                floorTask = _apiService.UserInitiated.GetFloor(Settings.AccessToken, floorId);
                                break;
                        }

                        floorResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await floorTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content

                        floorResponse = await ex.GetContentAsAsync<ResponseModel<FloorDto>>();

                        if (floorResponse != null && floorResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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

                        ex.Track();
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                if (priority != Priority.Background)
                                {
                                    Extensions.SomethingWentWrong("Getting gift card.", ex);
                                }
                            }
                        }
                        return null;
                    }

                    if (floorResponse != null && floorResponse.success)
                    {
                        return floorResponse.result;
                    }
                    else if (priority != Priority.Background && floorResponse != null && floorResponse.error != null && floorResponse.error.message != null && !string.IsNullOrEmpty(floorResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(floorResponse.error.message, Colors.Red, Colors.White);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    //await Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not connected", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public async Task GetFloors(Priority priority, bool syncLocal, GetFloorInput filter)
        {
            try
            {
                Settings.SelectedFloorID = 0;
                ListResponseModel<FloorDto> tmpremoteFloorResponse = new ListResponseModel<FloorDto>();
                Task<ListResponseModel<FloorDto>> floorTask;
                Debug.WriteLine("GetFloors request : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        floorTask = _apiService.Background.GetFloors(filter, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        floorTask = _apiService.UserInitiated.GetFloors(filter, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        floorTask = _apiService.Speculative.GetFloors(filter, Settings.AccessToken);
                        break;
                    default:
                        floorTask = _apiService.UserInitiated.GetFloors(filter, Settings.AccessToken);
                        break;
                }

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        tmpremoteFloorResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds * 2))
                            .ExecuteAsync(async () => await floorTask);


                        Debug.WriteLine("GetFloors response : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        tmpremoteFloorResponse = await ex.GetContentAsAsync<ListResponseModel<FloorDto>>();

                        if (tmpremoteFloorResponse != null && tmpremoteFloorResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting invoice and storing locally.", ex);
                            }
                        }
                        return;
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                    return;
                }

                if (tmpremoteFloorResponse != null && tmpremoteFloorResponse.success && tmpremoteFloorResponse.result != null && tmpremoteFloorResponse.result.totalCount > 0)
                {
                    if (syncLocal)
                        await UpdateLocalFloor(tmpremoteFloorResponse.result.items);
                    return;
                }
                else
                {
                    if (priority != Priority.Background && tmpremoteFloorResponse != null && tmpremoteFloorResponse.error != null && tmpremoteFloorResponse.error.message != null)
                    {
                        App.Instance.Hud.DisplayToast(tmpremoteFloorResponse.error.message, Colors.Red, Colors.White);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<bool> UpdateLocalFloor(ObservableCollection<FloorDto> floors)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                await realm.WriteAsync(() =>
                {
                    realm.Add(floors.Select(a => a.ToModel()).ToList(), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public List<FloorDto> GetLocalFloors(int outletId)
        {
            try
            {

                using var realm = RealmService.GetRealm();
                return realm.All<FloorDB>().Where(x => x.OutletId == outletId).ToList().Select(a => FloorDto.FromModel(a))?.ToList();
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public FloorDto GetLocalFloorById(int id)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var floor = FloorDto.FromModel(realm.Find<FloorDB>(id));
                return floor;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

    }
}
