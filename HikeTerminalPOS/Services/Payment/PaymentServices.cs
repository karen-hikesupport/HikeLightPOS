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
using Newtonsoft.Json;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
	public class PaymentServices
	{
		readonly IApiService<IPaymentApi> _apiService;
		readonly IApiService<IAccountApi> accountApiService;
		readonly AccountServices accountService;

		public PaymentServices(IApiService<IPaymentApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public ObservableCollection<PaymentOptionDto> GetLocalPaymentOptions()
		{
            try
            {
                using var realm = RealmService.GetRealm();
                var objects = realm.All<PaymentOptionDB>().ToList();
                if (objects != null)
                {
                    return new ObservableCollection<PaymentOptionDto>(objects.Select(a => PaymentOptionDto.FromModel(a)));
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            return null;
		}

		public PaymentOptionDto GetLocalPaymentOption(int Id)
		{
            try
            {
                using var realm = RealmService.GetRealm();
                var objects = realm.Find<PaymentOptionDB>(Id);
                return PaymentOptionDto.FromModel(objects);
               // return await CommonQueries.GetObject<PaymentOptionDto>(nameof(PaymentOptionDto) + "_" + Id.ToString());
            }
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            return null;
		}

		public ObservableCollection<PaymentOptionDto> GetLocalPaymentOptionsByRegister(int registerId)
		{
			try
			{
                using var realm = RealmService.GetRealm();
                var objects = realm.All<PaymentOptionDB>().ToList();
                if (objects != null)
				{
					return new ObservableCollection<PaymentOptionDto>(objects.Select(a=> PaymentOptionDto.FromModel(a)));
				}
			}
			catch (KeyNotFoundException ex)
			{
                Debug.WriteLine(ex.Message);
                return new ObservableCollection<PaymentOptionDto>();
			}
			catch (Exception ex)
			{
				ex.Track();
			}
			return new ObservableCollection<PaymentOptionDto>();
		}

		public async Task<ObservableCollection<PaymentOptionDto>> GetRemotePaymentOptions(Priority priority, bool syncLocal)
		{
            try
            {

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
				{
                    ListResponseModel<PaymentOptionDto> paymentOptionResponse = null;
					
                    Task<ListResponseModel<PaymentOptionDto>> paymentOptionTask;
		            Retry:
					try
					{
		                switch (priority)
		                {
		                    case Priority.Background:
		                        paymentOptionTask = _apiService.Background.GetPaymentOptions(Settings.AccessToken);
		                        break;
		                    case Priority.UserInitiated:
		                        paymentOptionTask = _apiService.UserInitiated.GetPaymentOptions(Settings.AccessToken);
		                        break;
		                    case Priority.Speculative:
		                        paymentOptionTask = _apiService.Speculative.GetPaymentOptions(Settings.AccessToken);
		                        break;
		                    default:
		                        paymentOptionTask = _apiService.UserInitiated.GetPaymentOptions(Settings.AccessToken);
		                        break;
		                }


                        paymentOptionResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await paymentOptionTask);
                        Debug.WriteLine("Getall paymentOptionResponse:" + JsonConvert.SerializeObject(paymentOptionResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        paymentOptionResponse = await ex.GetContentAsAsync<ListResponseModel<PaymentOptionDto>>();

                        if (paymentOptionResponse != null && paymentOptionResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting payment options.", ex);
                            }
                        }
                        return null;
                    }

                    if (paymentOptionResponse != null && paymentOptionResponse.success && paymentOptionResponse.result != null && paymentOptionResponse.result.items.Any())
					{
                        //Start:Tyro tap to pay remove saved payment
                        try
                        {
                            if (Settings.TyroTapToPayConfiguration != null)
                            {
                                Logger.SyncLogger("----GetTyroTapToConfiguration2---\n" + (Settings.TyroTapToPayConfiguration?.ConfigurationDetails ?? "Empty"));
                                var payment = paymentOptionResponse.result.items.FirstOrDefault(x => x.IsConfigered && x.Id == Settings.TyroTapToPayConfiguration.Id);
                                if (payment == null)
                                {
                                    Settings.TyroTapToPayConfiguration = null;
                                    Settings.TyroTapToPayConnectionSecret = "";
                                }
                                else
                                {
                                    var configuration1 = JsonConvert.DeserializeObject<TyroTapToPayConfigurationDto>(payment?.ConfigurationDetails);
                                    var configuration2 = JsonConvert.DeserializeObject<TyroTapToPayConfigurationDto>(Settings.TyroTapToPayConfiguration?.ConfigurationDetails);
                                    if (configuration1.ReaderId != configuration2.ReaderId)
                                    {
                                        Settings.TyroTapToPayConfiguration = null;
                                        Settings.TyroTapToPayConnectionSecret = "";

                                    }
                                    Logger.SyncLogger("---GetTyroTapToConfiguration1---\n" + (payment?.ConfigurationDetails ?? "Empty"));

                                }

                            }
                        }
                        catch (Exception)
                        {

                        }
                        //End:Tyro tap to pay remove saved payment
						if (syncLocal)
						{
							UpdateLocalPaymentOptions(paymentOptionResponse.result.items);
						}
						return paymentOptionResponse.result.items;
					}
					else if (priority != Priority.Background && paymentOptionResponse != null && paymentOptionResponse.error != null && paymentOptionResponse.error.message != null)
					{
                        Extensions.ServerMessage(paymentOptionResponse.error.message);  
                        return null;
					}
                    else
                    {
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
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            return null;
		}

		public bool UpdateLocalPaymentOptions(ObservableCollection<PaymentOptionDto> paymentOptions)
		{
			try
			{
               
                if (paymentOptions != null && paymentOptions.Any())
                {
                    foreach (var item in paymentOptions)
                    {

                        if (item.PaymentOptionType == Enums.PaymentOptionType.NAB ||
                            item.PaymentOptionType == Enums.PaymentOptionType.Fiserv ||
                            item.PaymentOptionType == Enums.PaymentOptionType.Bendigo ||
                            item.PaymentOptionType == Enums.PaymentOptionType.ANZ)
                        {
                            //item.DisplayName = item.PaymentOptionName + " (Powered by linkly)";
                            item.DisplayName = item.Name + " (Powered by linkly)";
                        }
                        else
                        {
                            item.DisplayName = item.Name;
                        }
                    }


                    Dictionary<string, PaymentOptionDto> paymentOptions_dictionary = paymentOptions.ToDictionary(e => nameof(PaymentOptionDto) + "_" + e.Id.ToString(), e => e);
                    if (paymentOptions_dictionary != null && paymentOptions_dictionary.Any())
                    {
                       // await BlobCache.LocalMachine.InsertObjects(paymentOptions_dictionary, DateTimeOffset.Now.AddYears(2));
                        using var realm = RealmService.GetRealm();
                        realm.Write(() =>
                        {
                            realm.Add(paymentOptions.Select(a=> a.ToModel()).ToList(), update: true);
                        });
                        return true;
                    }
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

		public bool UpdateLocalPaymentOption(PaymentOptionDto paymentOption)
		{
			try
			{
                if (paymentOption != null)
                {
                    if (string.IsNullOrEmpty(paymentOption.DisplayName))
                    {
                            paymentOption.DisplayName = paymentOption.Name;
                    }
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(paymentOption.ToModel(), update: true);
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

		// public async Task<PaymentOptionDto> UpdateRemotePaymentOption(Priority priority, bool syncLocal, PaymentOptionDto paymentOption)
		// {
        //     try
        //     {
        //         Analytics.TrackEvent("PaymentServices.cs UpdateRemotePaymentOption() start");
        //         if (paymentOption == null)
        //         {
        //             return null;
        //         }
		// 		ResponseModel<PaymentOptionDto> paymentOptionResponse = null;
        //         PaymentOptionDto result = paymentOption;
        //         Debug.WriteLine("PaymentOption CreateOrUpdate Request:" + JsonConvert.SerializeObject(result));
        //         if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
		// 		{
	    //             Task<ResponseModel<PaymentOptionDto>> paymentOptionTask;
	    //         Retry1:
				
		// 			try
		// 			{
		//                 switch (priority)
		//                 {
		//                     case Priority.Background:
		//                         paymentOptionTask = _apiService.Background.CreateOrUpdate(paymentOption, Settings.AccessToken);
		//                         break;
		//                     case Priority.UserInitiated:
		//                         paymentOptionTask = _apiService.UserInitiated.CreateOrUpdate(paymentOption, Settings.AccessToken);
		//                         break;
		//                     case Priority.Speculative:
		//                         paymentOptionTask = _apiService.Speculative.CreateOrUpdate(paymentOption, Settings.AccessToken);
		//                         break;
		//                     default:
		//                         paymentOptionTask = _apiService.UserInitiated.CreateOrUpdate(paymentOption, Settings.AccessToken);
		//                         break;
		//                 }

        //                 paymentOptionResponse = await Policy
        //                     .Handle<ApiException>()
        //                     .RetryAsync(retryCount: ServiceConfiguration.retryCount)
        //                     .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
        //                     .ExecuteAsync(async () => await paymentOptionTask);

        //                 Debug.WriteLine("PaymentOption CreateOrUpdate Respomse:" + JsonConvert.SerializeObject(paymentOptionResponse));
        //             }
        //             catch (ApiException ex)
        //             {
        //                 //Get Exception content
        //                 paymentOptionResponse = await ex.GetContentAsAsync<ResponseModel<PaymentOptionDto>>();
        //                 if (paymentOptionResponse != null && paymentOptionResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //                 {
        //                     bool res = await accountService.GetRenewAccessToken(priority);
        //                     if (res)
        //                     {
        //                         goto Retry1;
        //                     }
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        //                 if (priority != Priority.Background)
        //                 {
        //                     if (ex.Message == "An error occurred while sending the request")
        //                     {
        //                         bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
        //                         if (!isReachable)
        //                         {
        //                             App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
        //                         }
        //                     }
        //                     else
        //                     {
        //                         App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
        //                     }
        //                 }
        //             }

					
        //         }
        //         else
        //         {
        //             if (priority != Priority.Background)
        //             {
        //                 App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
        //             }
        //         }

		// 		if (paymentOptionResponse != null && paymentOptionResponse.success && paymentOptionResponse.result != null)
		// 		{
		// 			if (syncLocal)
		// 			{
        //                 if (string.IsNullOrEmpty(paymentOptionResponse.result.ConfigurationDetails))
		// 				{
        //                     paymentOptionResponse.result.ConfigurationDetails = paymentOption.ConfigurationDetails;
		// 				}
        //                 await UpdateLocalPaymentOption(paymentOptionResponse.result);
		// 			}
        //             return paymentOptionResponse.result;
		// 		}
		// 		else
		// 		{
        //             //Ticket #1491 Start:Below code commented to solve issue of payment type removal but still remain in local db. By Nikhil.
        //             //if (syncLocal)
        //             //{
        //             //	await UpdateLocalPaymentOption(paymentOption);
        //             //}
        //             //Ticket #1491 End:By Nikhil. 

        //             if (priority != Priority.Background && paymentOptionResponse != null && paymentOptionResponse.error != null && paymentOptionResponse.error.message != null)
		// 			{
		// 				App.Instance.Hud.DisplayToast(paymentOptionResponse.error.message, Colors.Red, Colors.White);
		// 			}
		// 			return result;
		// 		}
        //     }
        //     catch(Exception ex)
        //     {
        //         ex.Track();
		// 		return paymentOption;
        //     }
		// }
	        public async Task<PaymentOptionDto> UpdateRemotePaymentOption(Priority priority, bool syncLocal, PaymentOptionDto paymentOption)
		{
            try
            {
                if (paymentOption == null)
                {
                    return null;
                }
				ResponseModel<PaymentOptionDto> paymentOptionResponse = null;
                PaymentOptionDto result = paymentOption;
                Debug.WriteLine("PaymentOption CreateOrUpdate Request:" + JsonConvert.SerializeObject(result));
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
				{
	                Task<ResponseModel<PaymentOptionDto>> paymentOptionTask;
	            //Retry1:
				
					try
					{

                        using (var httpClient = new HttpClient())
                        {
                        AgainCall:
                            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                            httpClient.Timeout = new TimeSpan(0, 10, 0);
                            var data = JsonConvert.SerializeObject(paymentOption);
                            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");


                            string url;
                            if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                            {
                                url = ServiceConfiguration.LiveProtocol + ServiceConfiguration.LivePrefix + ServiceConfiguration.LiveBaseUrl;
                            }
                            else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                            {
                                url = ServiceConfiguration.DesignerProtocol + ServiceConfiguration.DesignerBaseUrl;
                            }
                            else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.HConnectTest)
                            {
                                url = ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl;
                            }
                            else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.StagingTest)
                            {
                                url = ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl;
                            }
                            else
                            {
                                url = ServiceConfiguration.AsyProtocol + ServiceConfiguration.AsyBaseUrl;
                            }

                            var response = await httpClient.PostAsync(url + "/api/services/app/paymentConfiguration/CreateOrUpdate", content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    paymentOptionResponse =  JsonConvert.DeserializeObject<ResponseModel<PaymentOptionDto>>(json);
                                }
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                               	bool res = await accountService.GetRenewAccessToken(priority);
								if (res)
								{
									goto AgainCall;
								}
                            }
                        }

/*
		                switch (priority)
		                {
		                    case Priority.Background:
		                        paymentOptionTask = _apiService.Background.CreateOrUpdate(paymentOption, Settings.AccessToken);
		                        break;
		                    case Priority.UserInitiated:
		                        paymentOptionTask = _apiService.UserInitiated.CreateOrUpdate(paymentOption, Settings.AccessToken);
		                        break;
		                    case Priority.Speculative:
		                        paymentOptionTask = _apiService.Speculative.CreateOrUpdate(paymentOption, Settings.AccessToken);
		                        break;
		                    default:
		                        paymentOptionTask = _apiService.UserInitiated.CreateOrUpdate(paymentOption, Settings.AccessToken);
		                        break;
		                }

                        paymentOptionResponse = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await paymentOptionTask); 

                        Debug.WriteLine("PaymentOption CreateOrUpdate Respomse:" + JsonConvert.SerializeObject(paymentOptionResponse));*/
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        paymentOptionResponse = await ex.GetContentAsAsync<ResponseModel<PaymentOptionDto>>();
                        if (paymentOptionResponse != null && paymentOptionResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                //goto Retry1;
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

				if (paymentOptionResponse != null && paymentOptionResponse.success && paymentOptionResponse.result != null)
				{
					if (syncLocal)
					{
                        if (string.IsNullOrEmpty(paymentOptionResponse.result.ConfigurationDetails))
						{
                            paymentOptionResponse.result.ConfigurationDetails = paymentOption.ConfigurationDetails;
						}
                        UpdateLocalPaymentOption(paymentOptionResponse.result);
					}
                    return paymentOptionResponse.result;
				}
				else
				{
                    //Ticket #1491 Start:Below code commented to solve issue of payment type removal but still remain in local db. By Nikhil.
                    //if (syncLocal)
                    //{
                    //	await UpdateLocalPaymentOption(paymentOption);
                    //}
                    //Ticket #1491 End:By Nikhil. 

                    if (priority != Priority.Background && paymentOptionResponse != null && paymentOptionResponse.error != null && paymentOptionResponse.error.message != null)
					{
						App.Instance.Hud.DisplayToast(paymentOptionResponse.error.message, Colors.Red, Colors.White);
					}
					return result;
				}
            }
            catch(Exception ex)
            {
                ex.Track();
				return paymentOption;
            }
		}
    
    }
}
