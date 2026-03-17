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
	public class OfferServices
	{

		readonly IApiService<IOfferApi> _apiService;
		readonly IApiService<IAccountApi> accountApiService;
		readonly AccountServices accountService;

		public OfferServices(IApiService<IOfferApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public ObservableCollection<OfferDto> GetLocalOffers()
		{
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.All<OfferDB>().ToList();
				return new ObservableCollection<OfferDto>(data.Select(a=> OfferDto.FromModel(a)));
               // return await CommonQueries.GetAllLocals<OfferDto>();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
		}

        //Start #84438 iOS : FR :add discount offers on product tag by Pratik
        public OfferDto GetLocalOffer(int id)
		{
            try{
                using var realm = RealmService.GetRealm();
                var data = realm.Find<OfferDB>(Convert.ToInt32(id));
				return OfferDto.FromModel(data);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return null;
		}
        //End #84438 by Pratik

        public async Task<ObservableCollection<OfferDto>> GetRemoteOffers(Priority priority, bool syncLocal)
        {
            try{
                ListResponseModel<OfferDto> offerResponse = null;

				Task<ListResponseModel<OfferDto>> offerTask;
			Retry:
				if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
				{
					try
					{
						switch (priority)
						{
							case Priority.Background:
								offerTask = _apiService.Background.GetAll(Settings.AccessToken);
								break;
							case Priority.UserInitiated:
								offerTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
								break;
							case Priority.Speculative:
								offerTask = _apiService.Speculative.GetAll(Settings.AccessToken);
								break;
							default:
								offerTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
								break;
						}
				
						offerResponse = await Policy
							.Handle<Exception>()
							.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            				.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
							.ExecuteAsync(async () => await offerTask);
					}
					catch (ApiException ex)
					{
						//Get Exception content
						offerResponse =await ex.GetContentAsAsync<ListResponseModel<OfferDto>>();
						if (offerResponse != null && offerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting offers.");
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

                if (offerResponse != null && offerResponse.success && offerResponse.result != null && offerResponse.result.items != null && offerResponse.result.items.Any())
				{
					if (syncLocal)
					{
						UpdateLocalOffers(offerResponse.result.items);
					}
					return offerResponse.result.items;
				}
                else if (priority != Priority.Background && offerResponse != null && offerResponse.error != null && !string.IsNullOrEmpty(offerResponse.error.message))
				{
                    Extensions.ServerMessage(offerResponse.error.message);
				}
                else{
					return null;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return null;
		}

		public bool UpdateLocalOffers(ObservableCollection<OfferDto> offers)
		{
			try
			{
                if (offers != null && offers.Any())
                {
					using var realm = RealmService.GetRealm();
					realm.Write(() =>
					{
						realm.RemoveAll<OfferDB>();

					});
					realm.Write(() =>
					{
						realm.Add(offers.Select(a => a.ToModel()), update: true);
					});
					return true;
                }
			}
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
			catch (Exception ex)
			{
				ex.Track();
			}
            return false;
		}

		public bool UpdateLocalOffer(OfferDto offer)
		{
			try
			{
                if (offer != null)
                {
                   // await BlobCache.LocalMachine.InsertObject(nameof(OfferDto) + "_" + offer.Id.ToString(), offer, DateTimeOffset.Now.AddYears(2));
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(offer.ToModel(), update: true);
                    });
                    return true;
                }
			}
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
			catch (Exception ex)
			{
				ex.Track();
			}
            return false;
		}

		public bool DeleteLocalOffer(string id)
		{
			try
			{
               // await BlobCache.LocalMachine.InvalidateObject<ProductDto_POS>(nameof(OfferDto) + "_" + id);
                using var realm = RealmService.GetRealm();
				var data = realm.Find<OfferDB>(Convert.ToInt32(id));
				if (data != null)
				{
					realm.Write(() =>
					{
						realm.Remove(data);
						data = null;
                    });
				}
                return true;
			}
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
			catch (Exception ex)
			{
				ex.Track();
			}
			return false;
		}
	}
}
