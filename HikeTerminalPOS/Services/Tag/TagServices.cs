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

namespace HikePOS.Services
{
	public class TagServices 
	{
		private readonly IApiService<ITagApi> _apiService;
		private readonly IApiService<IAccountApi> accountApiService;
		private readonly AccountServices accountService;

		public TagServices(IApiService<ITagApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}


		public ObservableCollection<TagDto> GetLocalTags()
		{
			return null;
			//return await CommonQueries.GetAllLocals<TagDto>();
		}

		public async  Task<ObservableCollection<TagDto>> GetRemoteTags(Priority priority, bool syncLocal)
		{
            ListResponseModel<TagDto> tagResponse = null;

			Task<ListResponseModel<TagDto>> tagTask;
		Retry:
			switch (priority)
			{
				case Priority.Background:
					tagTask = _apiService.Background.GetAll(Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					tagTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
					break;
				case Priority.Speculative:
					tagTask = _apiService.Speculative.GetAll(Settings.AccessToken);
					break;
				default:
					tagTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					tagResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await tagTask);
				}
				catch (ApiException ex)
				{
					//Get Exception content
					tagResponse = await ex.GetContentAsAsync<ListResponseModel<TagDto>>();
					if (tagResponse != null && tagResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting tags.");
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

			if (tagResponse.success)
			{
				if (syncLocal && tagResponse.result != null && tagResponse.result.items.Count > 0)
				{
					UpdateLocalTags(tagResponse.result.items);
				}
                return tagResponse.result.items;
			}
			else
			{
				if (priority != Priority.Background && tagResponse != null && tagResponse.error != null && tagResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(tagResponse.error.message, Colors.Red, Colors.White);
				}
                return null;
			}
		}

		public bool UpdateLocalTags(ObservableCollection<TagDto> tags)
		{
			try
			{
                Dictionary<string, TagDto> users_dictionary = tags.ToDictionary(e => nameof(TagDto) + "_" + e.Id.ToString(), e => e);
				//await BlobCache.LocalMachine.InsertObjects(users_dictionary, DateTimeOffset.Now.AddYears(2));
                return true;
			}
			catch (Exception ex)
			{
				ex.Track();
                return false;
			}
		}
	}
}
