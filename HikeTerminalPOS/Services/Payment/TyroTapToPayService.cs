using System;
using System.Diagnostics;
using System.Text;
using HikePOS.Helpers;
using HikePOS.Models;
using Newtonsoft.Json;
using Polly;
using PubnubApi;
using Refit;
namespace HikePOS.Services.Payment
{
    public class TyroTapToPayService
    {
        private JsonSerializer _serializer = new JsonSerializer();
        public async Task<Models.Payment.TyroTapToPayConnection> GetTyroTapToPayConnection(int paymentId)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                 string deviceID = "";
                #if ANDROID
                     deviceID = Android.Provider.Settings.Secure.GetString(Platform.CurrentActivity.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                #elif IOS
                      deviceID = UIKit.UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                #endif

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", ""));
                        httpClient.Timeout = new TimeSpan(0, 10, 0);
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

                        // url = url + $"/api/services/app/tyroManagerPayment/GetConnectionFromReaderId?paymentId={paymentId}&deviceId={deviceID}&deviceName='{DeviceInfo.Name}'";
                        var queryParameters = new Dictionary<string, string>
                        {
                           { "paymentId", paymentId.ToString() },
                           { "deviceId", deviceID},
                           { "deviceName", DeviceInfo.Name}
                        };
                        var dictFormUrlEncoded = new FormUrlEncodedContent(queryParameters);
                        var queryString = await dictFormUrlEncoded.ReadAsStringAsync();
                        url = url + $"/api/services/app/tyroManagerPayment/GetConnectionFromReaderId?{queryString}";

                        Logger.SyncLogger("----GetTyroTapToPayConnection url---\n" + url);
                        Logger.SyncLogger("----GetTyroTapToPayConnection queryString---\n" + queryString);
                        var response = await httpClient.PostAsync(url, null).ConfigureAwait(false);

                        
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Logger.SyncLogger("----GetTyroTapToPayConnection response---\n" + json);
                            var connectionResponse = JsonConvert.DeserializeObject<ResponseModel<Models.Payment.TyroTapToPayConnection>>(json);

                            if (connectionResponse != null && connectionResponse.success && connectionResponse.result != null)
                            {
                                return connectionResponse.result;
                            }
                            else if (connectionResponse != null && connectionResponse.error != null && connectionResponse.error.message != null)
                            {
                                App.Instance.Hud.DisplayToast(connectionResponse.error.message, Colors.Red, Colors.White);
                                return null;
                            }
                            else
                            {
                                return null;
                            }


                        }
                        else
                        {
                            Logger.SyncLogger("----GetTyroTapToPayConnection Reason Phrase---\n" + response?.ReasonPhrase);
                            App.Instance.Hud.DisplayToast(response?.ReasonPhrase, Colors.Red, Colors.White);
                            return null;
                        }
                    }

                }
                catch (Exception ex)
                {
                    ex.Track();
                    Logger.SyncLogger("----GetTyroTapToPayConnection Exception---\n" + ex.Message + "\n" + ex.StackTrace);
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        bool isReachable = await CommonMethods.ReachableCheck("https://api.tyro.com");
                        if (!isReachable)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            return null;
                        }
                    }
                    else
                    {

                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
                    }

                    return null;
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return null;
            }
        }
     /*   public async Task<string> GetClientConnectionSecret(string readerId)
        {
        Retry:
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        if (string.IsNullOrEmpty(Settings.TyroTapToPayAccessToken))
                            await GetRenewAccessToken();
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Settings.TyroTapToPayAccessToken);
                        var request = new ConnectionRequest { readerId = readerId };
                        var data = JsonConvert.SerializeObject(request);
                        var content = new StringContent(data, Encoding.UTF8, "application/json");
                        var urlPath = "https://api.tyro.com/connect/tap-to-pay/connections";
                        Logger.SyncLogger("----GetClientConnectionSecret url---\n" + urlPath);
                        Logger.SyncLogger("----GetClientConnectionSecret queryString---\n" + data);
                        var response = await httpClient.PostAsync(urlPath, content).ConfigureAwait(false);
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                           
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    var productStockResponse = _serializer.Deserialize<ConnectionResponse>(jsonSTR);
                                    var connectionSecret = productStockResponse?.connectionSecret;
                                    Logger.SyncLogger("----GetClientConnectionSecret response---\n" + connectionSecret ?? "Empty");
                                    Settings.TyroTapToPayConnectionSecret = connectionSecret;
                                    return connectionSecret;

                                }
                            }


                        }
                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            Logger.SyncLogger("----GetClientConnectionSecret response---\n" + "Forbidden");
                            bool res = await GetRenewAccessToken();
                            if (res)
                            {
                                goto Retry;
                            }
                        }
                        if (response.ReasonPhrase == "Unauthorized")
                        {
                            Logger.SyncLogger("----GetClientConnectionSecret response---\n" + "Unauthorized");
                            bool res = await GetRenewAccessToken();
                            if (res)
                            {
                                goto Retry;
                            }
                        }
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    ex.Track();
                    Logger.SyncLogger("----GetClientConnectionSecret Exception---\n" + ex.Message + "\n" + ex.StackTrace);
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        bool isReachable = await CommonMethods.ReachableCheck("https://api.tyro.com");
                        if (!isReachable)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            return null;
                        }
                    }
                    else
                    {

                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
                    }
                    return null;
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return null;
            }

        }
        public async Task<bool> GetRenewAccessToken()
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var request = new AccessTokenRequest { client_id = "DTKrmmz9pihldFD2bkyne9SJTHXqdSGg", client_secret = "JGf8TQFScDIhee37fSuk8EL6geOxs4nq-ShstDU2CLJhDMeIO0n6t-ZGEqT7MXuQ", grant_type = "client_credentials", audience = "https://pos.connect.tyro" };
                        var data = JsonConvert.SerializeObject(request);
                        var content = new StringContent(data, Encoding.UTF8, "application/json");
                        var urlPath = "https://auth.connect.tyro.com/oauth/token";
                        var chunkStartDateTime = DateTime.Now;
                        var response = await httpClient.PostAsync(urlPath, content).ConfigureAwait(false);
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    var accessToken = _serializer.Deserialize<AccessTockenDto>(jsonSTR);
                                    Logger.SyncLogger("----GetRenewAccessToken response---\n" + accessToken?.access_token == null ? "Failed" : "Success");
                                    Settings.TyroTapToPayAccessToken = accessToken?.access_token;
                                    return true;

                                }
                            }


                        }
                        else
                        { 
                            Logger.SyncLogger("----GetRenewAccessToken failed response---\n" + response?.StatusCode.ToString());
                        }
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    ex.Track();
                    Logger.SyncLogger("----GetRenewAccessToken Exception---\n" + ex.Message + "\n" + ex.StackTrace);
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        bool isReachable = await CommonMethods.ReachableCheck("https://api.tyro.com");
                        if (!isReachable)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            return false;
                        }
                    }
                    else
                    {

                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
                    }

                    return false;
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return false;
            }

        }*/

    }
    /*public class ConnectionRequest
    {
        public string readerId { get; set; }

    }

    public class ConnectionResponse
    {
        public int id { get; set; }
        public string connectionSecret { get; set; }
        public string createdDate { get; set; }
        public string readerId { get; set; }
        public string readerName { get; set; }
        public string locationId { get; set; }
        public string locationName { get; set; }

    }
    public class AccessTokenRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public string audience { get; set; }

    }*/
}

