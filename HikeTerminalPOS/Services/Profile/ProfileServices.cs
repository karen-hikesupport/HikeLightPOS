using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Reactive.Linq;

namespace HikePOS.Services
{
	public class ProfileServices
	{

		private readonly IApiService<IProfileApi> _apiService;
		private readonly IApiService<IAccountApi> accountApiService;
		private readonly AccountServices accountService;

		public ProfileServices(IApiService<IProfileApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public async Task<string> UpdateProfilePicture(Priority priority, bool syncLocal, UpdateProfilePictureModel updateProfilePictureRequest)
		{
            ResponseModel<string> UpdateProfilePictureResponse = null;

			Task<ResponseModel<string>> UpdateProfilePictureTask;
		Retry2:
			switch (priority)
			{
				case Priority.Background:
					UpdateProfilePictureTask = _apiService.Background.UpdateProfilePicture(Settings.AccessToken, updateProfilePictureRequest);
					break;
				case Priority.UserInitiated:
					UpdateProfilePictureTask = _apiService.UserInitiated.UpdateProfilePicture(Settings.AccessToken, updateProfilePictureRequest);
					break;
				case Priority.Speculative:
					UpdateProfilePictureTask = _apiService.Speculative.UpdateProfilePicture(Settings.AccessToken, updateProfilePictureRequest);
					break;
				default:
					UpdateProfilePictureTask = _apiService.UserInitiated.UpdateProfilePicture(Settings.AccessToken, updateProfilePictureRequest);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					UpdateProfilePictureResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await UpdateProfilePictureTask);
				}
				catch (ApiException ex)
				{
					UpdateProfilePictureResponse = await ex.GetContentAsAsync<ResponseModel<string>>();
					if (UpdateProfilePictureResponse != null && UpdateProfilePictureResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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

			if (UpdateProfilePictureResponse.success)
			{
                //if (syncLocal && UpdateProfilePictureResponse.result != null && roleResponse.result.items.Count > 0)
                //{
                //	await UpdateLocalRoles(roleResponse.result.items);
                //}
                return UpdateProfilePictureResponse.result;
			}
			else
			{
				
				if (priority != Priority.Background && UpdateProfilePictureResponse != null && UpdateProfilePictureResponse.error != null && UpdateProfilePictureResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(UpdateProfilePictureResponse.error.message, Colors.Red, Colors.White);
				}
                return null;
			}
		}



		public async Task<AjaxResponse> UpdatePassword(Priority priority, bool syncLocal, ChangePasswordModel ChangePasswordRequest)
		{
            AjaxResponse ChangePasswordResponse = null;

			Task<AjaxResponse> ChangePasswordTask;
		Retry2:
			switch (priority)
			{
				case Priority.Background:
					ChangePasswordTask = _apiService.Background.UpdatePassword(Settings.AccessToken, ChangePasswordRequest);
					break;
				case Priority.UserInitiated:
					ChangePasswordTask = _apiService.UserInitiated.UpdatePassword(Settings.AccessToken, ChangePasswordRequest);
					break;
				case Priority.Speculative:
					ChangePasswordTask = _apiService.Speculative.UpdatePassword(Settings.AccessToken, ChangePasswordRequest);
					break;
				default:
					ChangePasswordTask = _apiService.UserInitiated.UpdatePassword(Settings.AccessToken, ChangePasswordRequest);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					ChangePasswordResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
						.ExecuteAsync(async () => await ChangePasswordTask);
				}
				catch (ApiException ex)
				{
					ChangePasswordResponse = await ex.GetContentAsAsync<AjaxResponse>();
					if (ChangePasswordResponse != null && ChangePasswordResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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

			if (ChangePasswordResponse.success)
			{
                //if (syncLocal && UpdateProfilePictureResponse.result != null && roleResponse.result.items.Count > 0)
                //{
                //	await UpdateLocalRoles(roleResponse.result.items);
                //}
                return ChangePasswordResponse;
			}
			else
			{

				if (priority != Priority.Background && ChangePasswordResponse != null && ChangePasswordResponse.error != null && ChangePasswordResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(ChangePasswordResponse.error.message, Colors.Red, Colors.White);
				}
                return null;
			}
		}
	

	}
}
