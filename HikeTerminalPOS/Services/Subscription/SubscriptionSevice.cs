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
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Diagnostics;

namespace HikePOS.Services
{
	public class SubscriptionSevice
	{
		private readonly IApiService<ISubscriptionAPI> _apiService;
		private readonly IApiService<IAccountApi> accountApiService;
		private readonly AccountServices accountService;

		public SubscriptionSevice(IApiService<ISubscriptionAPI> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public async Task<SubscriptionDto> GetRemoteAccountDetail(Priority priority, bool syncLocal)
		{
            var AccountDetailResponse = new ResponseModel<SubscriptionDto>();
			Task<ResponseModel<SubscriptionDto>> SubscriptionTask;
			
		Retry:
			switch (priority)
			{
				case Priority.Background:
					SubscriptionTask = _apiService.Background.GetAccountDetail(Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					SubscriptionTask = _apiService.UserInitiated.GetAccountDetail(Settings.AccessToken);
					break;
				case Priority.Speculative:
					SubscriptionTask = _apiService.Speculative.GetAccountDetail(Settings.AccessToken);
					break;
				default:
					SubscriptionTask = _apiService.UserInitiated.GetAccountDetail(Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					AccountDetailResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await SubscriptionTask);


                    Debug.WriteLine("AccountDetailResponse : " + Newtonsoft.Json.JsonConvert.SerializeObject(AccountDetailResponse).ToString());

                }
				catch (ApiException ex)
				{
					//Get Exception content
					AccountDetailResponse = await ex.GetContentAsAsync<ResponseModel<SubscriptionDto>>();
					if (AccountDetailResponse != null && AccountDetailResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
							}
						}
						else
						{
                            Extensions.SomethingWentWrong("Getting account detail.", ex);
                        }
					}
                    return null;
				}
			}
			else
			{
				if (priority != Priority.Background)
				{
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"),Colors.Red,Colors.White);
				}
                return null;
			}

			if (syncLocal && AccountDetailResponse.success && AccountDetailResponse.result != null)
			{

				await GetStorewiseFeatures(priority, syncLocal);
				UpdateLocalAccountDetail(AccountDetailResponse.result);
				return AccountDetailResponse.result;
			}
			else
			{
				if (priority != Priority.Background && AccountDetailResponse != null && AccountDetailResponse.error != null && AccountDetailResponse.error.message != null)
				{
                    Extensions.ServerMessage(AccountDetailResponse.error.message);  
				}
                return null;
			}
		}
	
		public bool UpdateLocalAccountDetail(SubscriptionDto Subscription)
		{
			try
			{
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(Subscription.ToModel(), update: true);
                });
                return true;
			}
			catch (Exception ex)
			{
				ex.Track();
                return false;
			}
		}

		public SubscriptionDto GetLocalSubscription()
		{
            using var realm = RealmService.GetRealm();
			var result = realm.All<SubscriptionDB>().ToList().FirstOrDefault();
			return SubscriptionDto.FromModel(result);
        }


		//#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
		public async Task<ShopFeature> GetStorewiseFeatures(Priority priority, bool syncLocal)
		{
			
			var shopFeatureResponse = new ResponseModel<ShopFeature>();
			Task<ResponseModel<ShopFeature>> shopFeatureTask;

		Retry:
			switch (priority)
			{
				case Priority.Background:
					shopFeatureTask = _apiService.Background.GetStorewiseFeatures(Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					shopFeatureTask = _apiService.UserInitiated.GetStorewiseFeatures(Settings.AccessToken);
					break;
				case Priority.Speculative:
					shopFeatureTask = _apiService.Speculative.GetStorewiseFeatures(Settings.AccessToken);
					break;
				default:
					shopFeatureTask = _apiService.UserInitiated.GetStorewiseFeatures(Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					shopFeatureResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
						.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await shopFeatureTask);


					Debug.WriteLine("AccountDetailResponse : " + Newtonsoft.Json.JsonConvert.SerializeObject(shopFeatureResponse).ToString());

				}
				catch (ApiException ex)
				{
					//Get Exception content
					shopFeatureResponse = await ex.GetContentAsAsync<ResponseModel<ShopFeature>>();
					if (shopFeatureResponse != null && shopFeatureResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
								
								return null;
							}
						}
						else
						{
							Extensions.SomethingWentWrong("Getting account detail.", ex);
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


			if (syncLocal && shopFeatureResponse.success && shopFeatureResponse.result != null)
			{
				Settings.ShopFeatures = shopFeatureResponse.result;
				return shopFeatureResponse.result;
			}
			else
			{
				if (priority != Priority.Background && shopFeatureResponse != null && shopFeatureResponse.error != null && shopFeatureResponse.error.message != null)
				{
					Extensions.ServerMessage(shopFeatureResponse.error.message);
				}
				
				return null;
			}
			

            
		}

		//#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
	}
}
