using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Collections.Generic;
using System.Reactive.Linq;
using HikePOS.Interfaces;
using System.Linq;
using System.Globalization;
using HikePOS.Models.Shop;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Text;

namespace HikePOS.Services
{
	public class ShopServices
	{
		private readonly IApiService<IShopApi> _apiService;
		private readonly IApiService<IAccountApi> accountApiService;
		private readonly AccountServices accountService;

		public ShopServices(IApiService<IShopApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public ShopGeneralDto GetLocalShops()
		{
			try
			{
                var realm = RealmService.GetRealm();
                var shps = realm.All<ShopGeneralDB>().ToList().FirstOrDefault();
                return ShopGeneralDto.FromModel(shps);
               // return await BlobCache.LocalMachine.GetObject<ShopGeneralDto>(nameof(ShopGeneralDto));
            }
			catch (KeyNotFoundException ex)
			{
				Debug.WriteLine(ex.Message);
                return null;
			}
			catch (Exception ex)
			{
				ex.Track();
                return null;
			}
		}

		public async Task<ShopGeneralDto> GetRemoteShops(Priority priority, bool syncLocal)
		{
            ResponseModel<ShopGeneralDto> shopResponse = null;

			Task<ResponseModel<ShopGeneralDto>> shopTask;
		Retry:
			switch (priority)
			{
				case Priority.Background:
					shopTask = _apiService.Background.GetDetail(Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					shopTask = _apiService.UserInitiated.GetDetail(Settings.AccessToken);
					break;
				case Priority.Speculative:
					shopTask = _apiService.Speculative.GetDetail(Settings.AccessToken);
					break;
				default:
					shopTask = _apiService.UserInitiated.GetDetail(Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					shopResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await shopTask);
				}
				catch (ApiException ex)
				{
					//Get Exception content
					shopResponse = await ex.GetContentAsAsync<ResponseModel<ShopGeneralDto>>();
					if (shopResponse != null && shopResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                    Logger.SyncLogger("\n ===GetRemoteShops===1");
                    Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
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
                            Extensions.SomethingWentWrong("Getting shops.", ex);
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

			if (shopResponse.success)
			{
				if (syncLocal && shopResponse.result != null)
				{
					UpdateLocalShop(shopResponse.result);
				}

				if (shopResponse.result != null)
				{
					if (shopResponse.result.Shop != null)
					{
						Settings.StoreId = shopResponse.result.Shop.Id;
						Settings.TenantId = shopResponse.result.Shop.TenantId;
						Settings.StoreName = shopResponse.result.Shop.TradingName;
						Settings.StoreShopDto = shopResponse.result.Shop;
					}
					if (shopResponse.result.GeneralRule != null)
					{
						Settings.StoreGeneralRule = shopResponse.result.GeneralRule;
                        if(Settings.IsAblyAsRealTime != shopResponse.result.GeneralRule.IsAblyAsRealTime)
                        {
                            Settings.IsAblyAsRealTime = shopResponse.result.GeneralRule.IsAblyAsRealTime;
                            WeakReferenceMessenger.Default.Send(new Messenger.DataStreamNetworkChangeMessenger(Settings.IsAblyAsRealTime));
                        }
                    }
					if (shopResponse.result.ZoneAndFormat != null)
					{
						Settings.StoreZoneAndFormatDetail = shopResponse.result.ZoneAndFormat;
						var objGetTimeZoneService = DependencyService.Get<IGetTimeZoneService>();
						Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(shopResponse.result.ZoneAndFormat.IanaTimeZone); //TimeZoneInfo.Local;
						Settings.StoreTimeZoneInfoId = shopResponse.result.ZoneAndFormat.IanaTimeZone;
                        Settings.StoreCurrencySymbol = shopResponse.result.ZoneAndFormat.CurrencySymbol;
                        Settings.StoreCurrencyCode = shopResponse.result.ZoneAndFormat.Currency;
						//Ticket start:#26913 iOS - Separator (comma) Not Applied.by rupesh
						Settings.SymbolForDecimalSeperatorForNonDot = shopResponse.result.ZoneAndFormat.SymbolForDecimalSeperatorForNonDot;
						//Ticket start:#26913 .by rupesh

						Settings.StoreCulture = shopResponse.result.ZoneAndFormat.Language;
                        if (!string.IsNullOrEmpty(shopResponse.result.ZoneAndFormat.Language) && shopResponse.result.ZoneAndFormat.Language.Contains("3d"))
                        {
                            shopResponse.result.ZoneAndFormat.Language = shopResponse.result.ZoneAndFormat.Language.Replace("3d", "");
                        }

                        if (string.IsNullOrEmpty(Settings.StoreCulture))
                        {
                            Settings.StoreCulture = "en";
                        }

                        if (string.IsNullOrEmpty(Settings.StoreCurrencySymbol))
                        {
                            Settings.StoreCurrencySymbol = "$";
                        }

						//Ticket start:#33790 iPad :: Feature request :: User specific language.by rupesh
						if (Settings.CurrentUser.AllowOverrideLangugeSettingOverGeneralSetting)
						{
							Settings.StoreCulture = Settings.CurrentUser.Language;
							Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.CurrentUser.IANATimeZone);
							Settings.StoreTimeZoneInfoId = Settings.CurrentUser.IANATimeZone;
                            if (Settings.StoreCulture.ToLower() == "ar" || Settings.StoreCulture.ToLower() == "ar-kw")
							{
								Settings.SymbolForDecimalSeperatorForNonDot = ",";
							}

						}
						//Ticket end:#33790 .by rupesh
						Extensions.SetCulture(Settings.StoreCulture.ToLower());


                        //if (string.IsNullOrEmpty(Settings.StoreCulture))
                        //{
                        //    Settings.StoreCulture = "en";
                        //}

                        //if (string.IsNullOrEmpty(Settings.StoreCurrencySymbol))
                        //{
                        //    Settings.StoreCurrencySymbol = "$";
                        //}

                        //var CrossMultilingual = DependencyService.Get<IMultilingual>();
                        //var culture = CrossMultilingual.NeutralCultureInfoList.ToList().FirstOrDefault(element => element.Name.ToLower().Contains(Settings.StoreCulture.ToLower()));
                        //if (culture == null)
                        //{
                        //    culture = new CultureInfo(CrossMultilingual.CurrentCultureInfo.Name);
                        //}
                        //culture.NumberFormat.CurrencySymbol = Settings.StoreCurrencySymbol;
                        //culture.NumberFormat.CurrencyNegativePattern = 1;
                        //culture.NumberFormat.CurrencyDecimalDigits = Settings.StoreDecimalDigit;
                        //CrossMultilingual.CurrentCultureInfo = culture;
                        //HikePOS.Resx.AppResources.Culture = CrossMultilingual.CurrentCultureInfo;
                    }
				}
                return shopResponse.result;
			}
			else
			{
				//await Application.Current.MainPage.DisplayAlert("Error", shopResponse.error.message, "Ok");
				if (priority != Priority.Background && shopResponse != null && shopResponse.error != null && shopResponse.error.message != null)
				{
                    Extensions.ServerMessage(shopResponse.error.message); 
				}
                return null;

			}
		}

		public bool UpdateLocalShop(ShopGeneralDto shopGeneral)
		{

			try
			{
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(shopGeneral.ToModel(), update: true);
                });

                //await BlobCache.LocalMachine.InsertObject(nameof(ShopGeneralDto), shopGeneral, DateTimeOffset.Now.AddYears(2));
                Settings.StoreGeneralRule = shopGeneral.GeneralRule;
				Settings.StoreShopDto = shopGeneral.Shop;
                return true;
			}
			catch (Exception ex)
			{
				ex.Track();
                return false;
			}
		}
		public async Task<GeneralRuleDto> UpdateRemoteRuleDetail(Priority priority, bool syncLocal, GeneralRuleDto generalRule)
		{
            ResponseModel<GeneralRuleDto> generalRuleResponse = null;

			Task<ResponseModel<GeneralRuleDto>> generalRuleTask;
		Retry1:
			switch (priority)
			{
				case Priority.Background:
					generalRuleTask = _apiService.Background.UpdateRuleDetail(generalRule, Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					generalRuleTask = _apiService.UserInitiated.UpdateRuleDetail(generalRule, Settings.AccessToken);
					break;
				case Priority.Speculative:
					generalRuleTask = _apiService.Speculative.UpdateRuleDetail(generalRule, Settings.AccessToken);
					break;
				default:
					generalRuleTask = _apiService.UserInitiated.UpdateRuleDetail(generalRule, Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					generalRuleResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await generalRuleTask);
				}
				catch (ApiException ex)
				{
					//Get Exception content
					generalRuleResponse = await ex.GetContentAsAsync<ResponseModel<GeneralRuleDto>>();
					if (generalRuleResponse != null && generalRuleResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                    Logger.SyncLogger("\n ===UpdateLocalShop===1");
                    Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                    if (priority != Priority.Background)
					{
						if (ex.Message == "An error occurred while sending the request")
						{
							bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
							if (!isReachable)
							{
								App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
							}
                            return null;
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

			if (generalRuleResponse.success)
			{
				if (syncLocal && generalRuleResponse.result != null)
				{
					UpdateLocalRuleDetail(generalRuleResponse.result);
				}
                return generalRuleResponse.result;
			}
			else
			{
				//await Application.Current.MainPage.DisplayAlert("Error", generalRuleResponse.error.message, "Ok");
				//return null;
				if (priority != Priority.Background && generalRuleResponse != null && generalRuleResponse.error != null && generalRuleResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(generalRuleResponse.error.message, Colors.Red, Colors.White);
				}
                return null;
			}

		}
		public bool UpdateLocalRuleDetail(GeneralRuleDto generalRule)
		{
			try
			{
                if (generalRule != null)
				{
					var shopdetail = GetLocalShops();
					shopdetail.GeneralRule = generalRule;
					UpdateLocalShop(shopdetail);
					Settings.StoreGeneralRule = generalRule;
				}
                return true;
			}
			catch (Exception ex)
			{
				ex.Track();
                return false;
			}
		}


		public async Task<AjaxResponse> UpdateAccountBasicInfo(Priority priority, BasicShopInfo basicShopInfo)
		{
            ResponseModel<BasicShopInfo> basicShopInfoResponse = null;
			Task<ResponseModel<BasicShopInfo>> basicShopInfoTask;
		// Retry1:
		// 	switch (priority)
		// 	{
		// 		case Priority.Background:
		// 			basicShopInfoTask = _apiService.Background.UpdateBasicInfo(basicShopInfo, Settings.AccessToken);
		// 			break;
		// 		case Priority.UserInitiated:
		// 			basicShopInfoTask = _apiService.UserInitiated.UpdateBasicInfo(basicShopInfo, Settings.AccessToken);
		// 			break;
		// 		case Priority.Speculative:
		// 			basicShopInfoTask = _apiService.Speculative.UpdateBasicInfo(basicShopInfo, Settings.AccessToken);
		// 			break;
		// 		default:
		// 			basicShopInfoTask = _apiService.UserInitiated.UpdateBasicInfo(basicShopInfo, Settings.AccessToken);
		// 			break;
		// 	}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
					   using (var httpClient = new HttpClient())
                        {
                        AgainCall:
                            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                            httpClient.Timeout = new TimeSpan(0, 10, 0);
                            var data = JsonConvert.SerializeObject(basicShopInfo);
                            var content = new StringContent(data, Encoding.UTF8, "application/json");


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

                            var response = await httpClient.PostAsync(url + "/api/services/app/shop/UpdateBasicInfo", content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    basicShopInfoResponse = JsonConvert.DeserializeObject<ResponseModel<BasicShopInfo>>(json);
                                }
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                await accountService.GetRenewAccessToken(priority);
                                goto AgainCall;
                            }
                        }


					// basicShopInfoResponse = await Policy
					// 	.Handle<Exception>()
					// 	.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                    //     .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
					// 	.ExecuteAsync(async () => await basicShopInfoTask);
				}
				catch (ApiException ex)
				{
					//Get Exception content
					basicShopInfoResponse = await ex.GetContentAsAsync<ResponseModel<BasicShopInfo>>();
					if (basicShopInfoResponse != null && basicShopInfoResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                    Logger.SyncLogger("\n ===UpdateAccountBasicInfo===1");
                    Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                    if (priority != Priority.Background)
					{
						if (ex.Message == "An error occurred while sending the request")
						{
							bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
							if (!isReachable)
							{
								App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
							}
                            return null;
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

			if (basicShopInfoResponse.success)
			{
                return basicShopInfoResponse;
			}
			else
			{
				if (priority != Priority.Background && basicShopInfoResponse != null && basicShopInfoResponse.error != null && basicShopInfoResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(basicShopInfoResponse.error.message, Colors.Red, Colors.White);
				}
                return null;
			}

		}


        public async Task<IpadPaymentSyncResponse> IpadPaymentSyncWithServer(Priority priority, PaymentSyncDto paymentSyncDto)
        {
            

            IpadPaymentSyncResponse ipadPaymentSyncResponse = null;

          //   paymentSyncDto

            Task<IpadPaymentSyncResponse> IpadPaymentSyncResponseTask = null;

            Debug.WriteLine("paymentSyncDto ipad sync request : " + paymentSyncDto.ToJson());
        
            switch (priority)
            {
                case Priority.Background:
                    IpadPaymentSyncResponseTask = _apiService.Background.IpadPaymentSyncWithServer(paymentSyncDto, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    IpadPaymentSyncResponseTask = _apiService.UserInitiated.IpadPaymentSyncWithServer(paymentSyncDto, Settings.AccessToken);
                    break;
                
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
				try
				{
					ipadPaymentSyncResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
						.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await IpadPaymentSyncResponseTask);


					Debug.WriteLine("ipad sync response : " + paymentSyncDto.ToJson());
				}
				catch (ApiException ex)
				{
					Debug.WriteLine(ex.Message);
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
							return null;
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

           
            return ipadPaymentSyncResponse;
        }

        

       
    }
}
