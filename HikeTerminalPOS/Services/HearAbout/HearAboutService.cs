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
	public class HearAboutService
	{
		readonly IApiService<IHearAboutApi> _apiService;
		readonly AccountServices accountService;

		public HearAboutService(IApiService<IHearAboutApi> apiService)
		{
			_apiService = apiService;
			accountService = new AccountServices(new ApiService<IAccountApi>());
		}

		public async Task<ObservableCollection<HearAboutDto>> GetRemoteAllHearAbout(Priority priority, bool syncLocal)
		{
			try
			{
                ListResponseModel<HearAboutDto> HearAboutResponse = null;
				if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
				{
					Task<ListResponseModel<HearAboutDto>> HearAboutTask;

					PagedSortedAndFilteredInputDto Filter = new PagedSortedAndFilteredInputDto
					{
						skipCount = 0,
						filter = "",
						sorting = "0",
						maxResultCount = 1000
					};


				Retry:
					try
					{

                        switch (priority)
						{
							case Priority.Background:
								HearAboutTask = _apiService.Background.GetAllHearAbout(Filter, Settings.AccessToken);
								break;
							case Priority.UserInitiated:
								HearAboutTask = _apiService.UserInitiated.GetAllHearAbout(Filter, Settings.AccessToken);
								break;
							case Priority.Speculative:
								HearAboutTask = _apiService.Speculative.GetAllHearAbout(Filter, Settings.AccessToken);
								break;
							default:
								HearAboutTask = _apiService.UserInitiated.GetAllHearAbout(Filter, Settings.AccessToken);
								break;
						}

						HearAboutResponse = await Policy
							.Handle<Exception>()
							.RetryAsync(retryCount: ServiceConfiguration.retryCount)
							.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
							.ExecuteAsync(async () => await HearAboutTask);
					}
					catch (ApiException ex)
					{
                        //Get Exception content
                        HearAboutResponse =await ex.GetContentAsAsync<ListResponseModel<HearAboutDto>>();

						if (HearAboutResponse != null && HearAboutResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting hear about us.");
                        }
						return null;
					}
				}
				else
				{
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
					return null;
				}

				if (HearAboutResponse != null && HearAboutResponse.success && HearAboutResponse.result != null && HearAboutResponse.result.items != null && HearAboutResponse.result.items.Any())
				{
					//if (syncLocal)
					//{
					//	await UpdateAllLocalHearAbouts(HearAboutResponse.result.items);
					//}
					return HearAboutResponse.result.items;
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
                ex.Track();
				return null;
			}
		}

		public async Task<HearAboutDto> UpdateRemoteHearAbout(Priority priority, bool syncLocal, HearAboutDto hearAboutDto)
		{
			ResponseModel<HearAboutDto> hearAboutResponse = null;
			Task<ResponseModel<HearAboutDto>> hearAboutTask;

        Retry2:
			switch (priority)
			{
				case Priority.Background:
					hearAboutTask = _apiService.Background.CreateOrUpdateHearAbout(hearAboutDto, Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					hearAboutTask = _apiService.UserInitiated.CreateOrUpdateHearAbout(hearAboutDto, Settings.AccessToken);
					break;
				case Priority.Speculative:
					hearAboutTask = _apiService.Speculative.CreateOrUpdateHearAbout(hearAboutDto, Settings.AccessToken);
					break;
				default:
					hearAboutTask = _apiService.UserInitiated.CreateOrUpdateHearAbout(hearAboutDto, Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
                    hearAboutResponse = await Policy
						.Handle<ApiException>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
						.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await hearAboutTask);

				}
				catch (ApiException ex)
				{
                    //invoice.isSync = false;
                    hearAboutResponse = await ex.GetContentAsAsync<ResponseModel<HearAboutDto>>();
					if (hearAboutResponse != null && hearAboutResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						bool res = await accountService.GetRenewAccessToken(priority);
						if (res)
						{
							goto Retry2;
						}
					}
				}
				catch (Exception ex)
				{
                    //invoice.isSync = false;
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
							App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
						}
					}
				}
			}
			else
			{
				//invoice.isSync = false;
				App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
			}



			if (hearAboutResponse != null && hearAboutResponse.success && hearAboutResponse.result != null)
			{
				//if (syncLocal)
				//{
				//	await UpdateSingleLocalHearAbout(hearAboutResponse.result);
				//}
                return hearAboutResponse.result;
			}
			else
			{
				if (priority != Priority.Background && hearAboutResponse != null && hearAboutResponse.error != null && hearAboutResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(hearAboutResponse.error.message, Colors.Red, Colors.White);
				}

                return null;
			}
		}

		//public async Task<bool> UpdateAllLocalHearAbouts(ObservableCollection<HearAboutDto> HearAbouts)
		//{
		//	try
		//	{
  //              if (HearAbouts == null || !HearAbouts.Any())
		//		{
		//			return false;
		//		}

		//		Dictionary<string, HearAboutDto> hearAbouts_dictionary = HearAbouts.ToDictionary(e => nameof(HearAboutDto) + "_" + e.Id.ToString(), e => e);
		//		if (hearAbouts_dictionary != null)
		//		{
		//			await BlobCache.LocalMachine.InsertObjects(hearAbouts_dictionary, DateTimeOffset.Now.AddYears(2));
		//			return true;
		//		}
		//	}
		//	catch (KeyNotFoundException ex)
		//	{
  //              Debug.WriteLine(ex.Message);
		//	}
		//	catch (Exception ex)
		//	{
  //              ex.Track();
		//	}

  //          return false;
		//}

		//public async Task<bool> UpdateSingleLocalHearAbout(HearAboutDto hearAbout)
		//{
		//	try
  //          {

  //              if (hearAbout == null)
		//		{
		//			return false;
		//		}

		//		string docId = "";
		//		if (hearAbout.Id == 0)
		//		{
		//			if (string.IsNullOrEmpty(hearAbout.TempId))
		//			{
  //                      docId = nameof(HearAboutDto) + "_" + Guid.NewGuid().ToString();
  //                      hearAbout.TempId = docId;
		//			}
		//			else
		//			{
		//				docId = hearAbout.TempId;
		//			}
		//		}
		//		else
		//		{

  //                  if (!string.IsNullOrEmpty(hearAbout.TempId))
		//			{
		//				try
		//				{
		//					await BlobCache.LocalMachine.Invalidate(hearAbout.TempId);
		//				}
		//				catch (KeyNotFoundException ex)
		//				{
		//					Debug.WriteLine(ex.Message);
		//				}
		//				catch (Exception ex)
		//				{
		//					ex.Track();
		//				}
		//			}

		//			docId = nameof(HearAboutDto) + "_" + hearAbout.Id.ToString();
		//		}
		//		try
		//		{
		//			return true;
		//		}
		//		catch (KeyNotFoundException ex)
		//		{
  //                  Debug.WriteLine(ex.Message);
		//		}
		//		catch (Exception ex)
		//		{
		//			ex.Track();
		//		}

		//	}
		//	catch (Exception ex)
		//	{
  //              ex.Track();

		//	}
		//	return false;
		//}

		//public async Task<ObservableCollection<HearAboutDto>> GetLocalAllHearAbouts()
		//{
		//	try
		//	{
  //              return await CommonQueries.GetAllLocals<HearAboutDto>();
		//	}
		//	catch (KeyNotFoundException ex)
		//	{
  //              Debug.WriteLine(ex.Message);
		//		return null;
		//	}
		//	catch (Exception ex)
		//	{
  //              ex.Track();
		//		return null;
		//	}
		//}
	}
}
