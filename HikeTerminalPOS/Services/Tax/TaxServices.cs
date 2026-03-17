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
using HikePOS.Resx;
using HikePOS.Services;
using System.Diagnostics;

namespace HikePOS.Services
{
    public class TaxServices
    {
        private readonly IApiService<ITaxApi> _apiService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;

        public TaxServices(IApiService<ITaxApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        public async Task<ObservableCollection<TaxDto>> GetRemoteTaxes(Priority priority, bool syncLocal)
        {
            ListResponseModel<TaxDto> taxResponse = null;

            Task<ListResponseModel<TaxDto>> taxTask;
        Retry:
            switch (priority)
            {
                case Priority.Background:
                    taxTask = _apiService.Background.GetAll(Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    taxTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    taxTask = _apiService.Speculative.GetAll(Settings.AccessToken);
                    break;
                default:
                    taxTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    taxResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await taxTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    taxResponse =await ex.GetContentAsAsync<ListResponseModel<TaxDto>>();
                    if (taxResponse != null && taxResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting taxes.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (taxResponse.success)
            {
                if (syncLocal && taxResponse.result != null && taxResponse.result.items.Count > 0)
                {
                    UpdateLocalTaxes(taxResponse.result.items);
                }
                return taxResponse.result.items;
            }
            else
            {
                //await Application.Current.MainPage.DisplayAlert("Error", taxResponse.error.message, "Ok");
                //return null;
                if (priority != Priority.Background && taxResponse != null && taxResponse.error != null && taxResponse.error.message != null)
                {
                    Extensions.ServerMessage(taxResponse.error.message);
                }
                return null;
            }
        }

        public bool UpdateLocalTaxes(ObservableCollection<TaxDto> taxs)
        {
            try
            {
                var realm = RealmService.GetRealm();

                if (taxs != null)
                {
                    realm.Write(() =>
                    {
                        realm.RemoveAll<TaxDB>();
                    });
                }
                realm.Write(() =>
                {
                    realm.Add(taxs.Select(u => u.ToModel()), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public ObservableCollection<TaxDto> GetLocalTaxes()
        {
            var realm = RealmService.GetRealm();
            var taxes = realm.All<TaxDB>().ToList().Select(a => TaxDto.FromModel(a));
            return new ObservableCollection<TaxDto>(taxes);
            //return await CommonQueries.GetAllLocals<TaxDto>();
        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil 
        public TaxDto GetLocalTaxById(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var tax = realm.Find<TaxDB>(id);
                return TaxDto.FromModel(tax);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
        //Ticket #10921 End. By Nikhil
    }
}
