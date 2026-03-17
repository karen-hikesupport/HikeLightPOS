using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace HikePOS.Services
{
	public class CommonLookupServices
	{
		
		readonly IApiService<ICommonLookupAPI> _apiService;
		readonly AccountServices accountService;

		public CommonLookupServices(IApiService<ICommonLookupAPI> apiService)
		{
			_apiService = apiService;
			accountService = new AccountServices(new ApiService<IAccountApi>());
		}

		public ObservableCollection<CountriesDto> GetLocalAllCountries()
		{
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.All<CountriesDB>().ToList();
                return new ObservableCollection<CountriesDto>(data.Select(a => CountriesDto.FromModel(a)).ToList());
            }
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            catch(Exception ex)
            {
                ex.Track();
                return null;
            }

		}

		public async Task<ObservableCollection<CountriesDto>> GetRemoteAllCountries(Priority priority, bool syncLocal)
		{
            try
            {
                ListResponseModel<CountriesDto> CountriesResponse = null;
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<CountriesDto>> CountriesTask;
                Retry:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                CountriesTask = _apiService.Background.GetAllCountries(Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                CountriesTask = _apiService.UserInitiated.GetAllCountries(Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                CountriesTask = _apiService.Speculative.GetAllCountries(Settings.AccessToken);
                                break;
                            default:
                                CountriesTask = _apiService.UserInitiated.GetAllCountries(Settings.AccessToken);
                                break;
                        }

                        CountriesResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await CountriesTask);
                    }
                    catch (ApiException ex)
                    {

                        //Get Exception content
                        CountriesResponse = await ex.GetContentAsAsync<ListResponseModel<CountriesDto>>();

                        if (CountriesResponse != null && CountriesResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting countries.", ex);
                        }
                        return null;
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return null;
                }

                if (CountriesResponse != null && CountriesResponse.success && CountriesResponse.result != null && CountriesResponse.result.items != null && CountriesResponse.result.items.Any())
                {
                    if (syncLocal)
                    {
                        UpdateLocalCountries(CountriesResponse.result.items);
                    }
                    return CountriesResponse.result.items;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                ex.Track();
                return null;
            }
		}

		public bool UpdateLocalCountries(ObservableCollection<CountriesDto> Countries)
		{
			try
			{
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(Countries.Select(a => a.ToModel()), update: true);
                });
                return true;
			}
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
			catch (Exception ex)
			{
                ex.Track();
				return false;
			}
		}
	}
}
