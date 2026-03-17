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
using System.Net.Http;
using Newtonsoft.Json;

namespace HikePOS.Services
{
	public class SubmitLogServices
	{
        private readonly IApiService<ISubmitLogApi> _apiService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;
        
        public SubmitLogServices(IApiService<ISubmitLogApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        public async Task<AjaxResponse> SubmitLogs(Priority priority, SubmitLogDto input)
        {
            AjaxResponse submitLogResponse = null;
            Task<AjaxResponse> submitLogTask;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        submitLogTask = _apiService.Background.SubmitLogs(input, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        submitLogTask = _apiService.UserInitiated.SubmitLogs(input, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        submitLogTask = _apiService.Speculative.SubmitLogs(input, Settings.AccessToken);
                        break;
                    default:
                        submitLogTask = _apiService.UserInitiated.SubmitLogs(input, Settings.AccessToken);
                        break;
                }


                try
                {
                    submitLogResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await submitLogTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    submitLogResponse = await ex.GetContentAsAsync<ResponseListModel<SubmitLogDto>>();

                    if (submitLogResponse != null && submitLogResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Console.WriteLine("Something went wrong while submitting logs.");
                            Debug.WriteLine("Something went wrong while submitting logs.");
                            //Need to log this error to backend
                            //Extensions.SomethingWentWrong("Getting purchase orders.");
                        }
                    }
                    return null;
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return null;
            }

            if (submitLogResponse != null && submitLogResponse.success)
            {
                Debug.WriteLine("SubmitLog successfull : " + submitLogResponse);
                return submitLogResponse;
            }
            else
            {
                Console.WriteLine("Error in SubmitLogService : " + submitLogResponse.error.message);
                Debug.WriteLine("Error in SubmitLogService : " + submitLogResponse.error.message);
                return null;
            }
        }

        //Ticket Start #61832 iPad:Create text file for invoice log by: Pratik
        public async Task<ResponseModel<object>> UpalodZipFile(Priority priority, System.IO.Stream stream, string filename)
        {
            ResponseModel<object> uploadzipResponse = null;
            stream.Position = 0;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", ""));



                        httpClient.Timeout = new TimeSpan(0, 10, 0);

                        var content = new MultipartFormDataContent();

                        content.Add(new StreamContent(stream),
                            "\"file\"",
                            $"\"{filename}\"");


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


                        var fullurl = string.Format("{0}/file/UpalodZipFile", url);
                        var response = await httpClient.PostAsync(fullurl, content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                uploadzipResponse = await Task.Run(() => JsonConvert.DeserializeObject<ResponseModel<object>>(json)).ConfigureAwait(false);
                            }
                            else
                            { App.Instance.Hud.DisplayToast("Something went wrong (internal error) when upload invoice log", Colors.Red, Colors.White); }
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when upload invoice log", Colors.Red, Colors.White);
                        }
                    }

                }
                catch (Exception ex)
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
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when upload invoice log", Colors.Red, Colors.White);
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }

            if (uploadzipResponse != null && uploadzipResponse.success)
            {
                App.Instance.Hud.DisplayToast("upload successfully.", Colors.Green, Colors.White);
                return uploadzipResponse;
            }
            else
            {
                if (uploadzipResponse?.error?.message != null)
                {
                    App.Instance.Hud.DisplayToast(uploadzipResponse.error.message, Colors.Red, Colors.White);
                }
                return uploadzipResponse;
            }
        }
        //End #61832 Pratik
    }
}

