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
	public class GiftCardServices
	{
		readonly IApiService<IGiftCardApi> _apiService;
		readonly IApiService<IAccountApi> accountApiService;
		readonly AccountServices accountService;

		public GiftCardServices(IApiService<IGiftCardApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public async Task<ObservableCollection<GiftCardDto>> GetRemoteGiftCards(Priority priority, bool syncLocal, string filter = "")
		{
			try
            {
                ListResponseModel<GiftCardDto> giftCardResponse = null;
				if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
				{
                    Task<ListResponseModel<GiftCardDto>> giftCardTask;

	                PagedSortedAndFilteredInputDto filterObj = new PagedSortedAndFilteredInputDto
	                {
	                    maxResultCount = 100,
	                    skipCount = 0,
	                    filter = filter,
	                    sorting = "0"
	                };
	                int totalProducts = 0;
	                int step = 1;

	            Retry:
				
					try
					{
		                switch (priority)
		                {
		                    case Priority.Background:
		                        giftCardTask = _apiService.Background.GetAll(filterObj, Settings.AccessToken);
		                        break;
		                    case Priority.UserInitiated:
		                        giftCardTask = _apiService.UserInitiated.GetAll(filterObj, Settings.AccessToken);
		                        break;
		                    case Priority.Speculative:
		                        giftCardTask = _apiService.Speculative.GetAll(filterObj, Settings.AccessToken);
		                        break;
		                    default:
		                        giftCardTask = _apiService.UserInitiated.GetAll(filterObj, Settings.AccessToken);
		                        break;
		                }


                        List<GiftCardDto> tempgiftlist = new List<GiftCardDto>();
                        do
                        {
                            var temp_giftCardResponse = await Policy
                                .Handle<Exception>()
                                .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                                .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                                .ExecuteAsync(async () => await giftCardTask);

                            if (temp_giftCardResponse != null && temp_giftCardResponse.success && temp_giftCardResponse.result != null && temp_giftCardResponse.result.items != null && temp_giftCardResponse.result.items.Any())
                            {
                                tempgiftlist.AddRange(temp_giftCardResponse.result.items);
                                totalProducts = temp_giftCardResponse.result.totalCount;
                                filterObj.skipCount = filterObj.maxResultCount * step;
                                step++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        while (totalProducts >= filterObj.skipCount);
                        if (tempgiftlist.Count > 0)
                        {
                            if(giftCardResponse == null)
                            {
                                giftCardResponse = new ListResponseModel<GiftCardDto>();
                            }

                            giftCardResponse.result.items = new ObservableCollection<GiftCardDto>(tempgiftlist);
                            giftCardResponse.success = true;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        giftCardResponse = await ex.GetContentAsAsync<ListResponseModel<GiftCardDto>>();

                        if (giftCardResponse != null && giftCardResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting gift cards.", ex);
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

                if (giftCardResponse != null && giftCardResponse.success && giftCardResponse.result != null && giftCardResponse.result.items != null && giftCardResponse.result.items.Count > 0)
                {
                    return giftCardResponse.result.items;
                }
                else if (priority != Priority.Background && giftCardResponse != null && giftCardResponse.error != null && giftCardResponse.error.message != null && !string.IsNullOrEmpty(giftCardResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(giftCardResponse.error.message, Colors.Red, Colors.White);
                    return null;
                }
                else{
                    return null;
                }
            }
            catch(Exception ex)
            {
                ex.Track();
                return null;
            }
        }

		public async Task<GiftCardDto> GetRemoteGiftCard(Priority priority, bool syncLocal, string giftCardNumber)
		{
            try
            {
                ResponseModel<GiftCardDto> giftCardResponse = null;
                Task<ResponseModel<GiftCardDto>> giftCardTask;

            Retry1:
				if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
				{
					try
					{
		                switch (priority)
		                {
		                    case Priority.Background:
		                        giftCardTask = _apiService.Background.GetByNumber(Settings.AccessToken, giftCardNumber);
		                        break;
		                    case Priority.UserInitiated:
		                        giftCardTask = _apiService.UserInitiated.GetByNumber(Settings.AccessToken, giftCardNumber);
		                        break;
		                    case Priority.Speculative:
		                        giftCardTask = _apiService.Speculative.GetByNumber(Settings.AccessToken, giftCardNumber);
		                        break;
		                    default:
		                        giftCardTask = _apiService.UserInitiated.GetByNumber(Settings.AccessToken, giftCardNumber);
		                        break;
		                }

                        giftCardResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await giftCardTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content

                        giftCardResponse = await ex.GetContentAsAsync<ResponseModel<GiftCardDto>>();

                        if (giftCardResponse != null && giftCardResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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

					if (giftCardResponse != null && giftCardResponse.success)
					{
                        if (giftCardResponse.result == null)
		                {
		                    giftCardResponse.result = new GiftCardDto();
		                }
						return giftCardResponse.result;
					}
					else if (priority != Priority.Background && giftCardResponse != null && giftCardResponse.error != null && giftCardResponse.error.message != null && !string.IsNullOrEmpty(giftCardResponse.error.message))
					{
						App.Instance.Hud.DisplayToast(giftCardResponse.error.message, Colors.Red, Colors.White);
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
            catch(Exception ex)
            {
                ex.Track();
            }
            return null;
		}

        public async Task<GiftCardDto> ValidateGiftCard(Priority priority, bool syncLocal, string giftCardNumber)
        {
            try
            {
                ResponseModel<GiftCardDto> giftCardResponse = null;
                Task<ResponseModel<GiftCardDto>> giftCardTask;

            Retry1:
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                giftCardTask = _apiService.Background.ValidateGiftCard(Settings.AccessToken, giftCardNumber);
                                break;
                            case Priority.UserInitiated:
                                giftCardTask = _apiService.UserInitiated.ValidateGiftCard(Settings.AccessToken, giftCardNumber);
                                break;
                            case Priority.Speculative:
                                giftCardTask = _apiService.Speculative.ValidateGiftCard(Settings.AccessToken, giftCardNumber);
                                break;
                            default:
                                giftCardTask = _apiService.UserInitiated.ValidateGiftCard(Settings.AccessToken, giftCardNumber);
                                break;
                        }

                        giftCardResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await giftCardTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content

                        giftCardResponse = await ex.GetContentAsAsync<ResponseModel<GiftCardDto>>();

                        if (giftCardResponse != null && giftCardResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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

                    if (giftCardResponse != null && giftCardResponse.success)
                    {
                        if (giftCardResponse.result == null)
                        {
                            giftCardResponse.result = new GiftCardDto();
                        }
                        return giftCardResponse.result;
                    }
                    else if (priority != Priority.Background && giftCardResponse != null && giftCardResponse.error != null && giftCardResponse.error.message != null && !string.IsNullOrEmpty(giftCardResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(giftCardResponse.error.message, Colors.Red, Colors.White);
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
	}
}
