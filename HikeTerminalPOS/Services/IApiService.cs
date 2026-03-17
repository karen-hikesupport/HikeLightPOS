using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using Refit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HikePOS.Services
{
    public interface IApiService<T>
    {
        string ApiBaseAddress { get; set; }
        T Speculative { get; }
        T UserInitiated { get; }
        T Background { get; }
    }

    public class ApiService<T> : IApiService<T>
    {

        public string ApiBaseAddress { get; set; }

        public ApiService()
        {

            Func<HttpMessageHandler, T> createClient = messageHandler =>
            {

                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                {
                    ApiBaseAddress = ServiceConfiguration.LiveProtocol + ServiceConfiguration.LivePrefix + ServiceConfiguration.LiveBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                {
                    ApiBaseAddress = ServiceConfiguration.DesignerProtocol + ServiceConfiguration.DesignerBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.HConnectTest)
                {
                    ApiBaseAddress = ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.StagingTest)
                {
                    ApiBaseAddress = ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl;
                }
                else
                {
                    ApiBaseAddress = ServiceConfiguration.AsyProtocol + ServiceConfiguration.AsyBaseUrl;
                }


                /*
				 Date : 12/06/2021
				 Note  : We have comment below client and created client using depedency service. Because timeout was not working
					properly with refit in common project

				 */

                // var client = new HttpClient(messageHandler)
                //{
                //	BaseAddress = new Uri(ApiBaseAddress),
                //	MaxResponseContentBufferSize = 25600000,
                //                Timeout = TimeSpan.FromHours(1)
                //};

                var httpService = new HttpService();
                var client = httpService.httpClient();

                client.BaseAddress = new Uri(ApiBaseAddress);
                client.MaxResponseContentBufferSize = 25600000;
                client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                client.DefaultRequestHeaders.Add("Keep-Alive", "200000");
                var requestfrom = DeviceInfo.Platform == DevicePlatform.iOS ? (DeviceInfo.Idiom == DeviceIdiom.Phone ? "iPhone" : "iPad") : (DeviceInfo.Idiom == DeviceIdiom.Phone ? "AndroidPhone" : "AndroidTab");
                client.DefaultRequestHeaders.Add("RequestFrom", requestfrom + " " + CommonMethods.GetDeviceIdentifierWithVersion()); //33964 iOS-Android : Add Header in API Request
                return RestService.For<T>(client, new RefitSettings(new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })));
                // return RestService.For<T>(client);
            };

#if IOS
            var configuration = Foundation.NSUrlSessionConfiguration.DefaultSessionConfiguration;
            configuration.TimeoutIntervalForRequest = 1200;
            MyHttpClientHandler msgHandler = new MyHttpClientHandler(configuration);
#elif ANDROID
            MyHttpClientHandler msgHandler = new MyHttpClientHandler();
            msgHandler.ReadTimeout = TimeSpan.FromMinutes(20.0);
#endif

            _background = new Lazy<T>(() => createClient(new RateLimitedHttpMessageHandler(msgHandler, Priority.Background)));
            _userInitiated = new Lazy<T>(() => createClient(new RateLimitedHttpMessageHandler(msgHandler, Priority.UserInitiated)));
            _speculative = new Lazy<T>(() => createClient(new RateLimitedHttpMessageHandler(msgHandler, Priority.Speculative)));
        }

        private readonly Lazy<T> _background;
        private readonly Lazy<T> _userInitiated;
        private readonly Lazy<T> _speculative;

        public T Background
        {
            get { return _background.Value; }
        }

        public T UserInitiated
        {
            get { return _userInitiated.Value; }
        }

        public T Speculative
        {
            get { return _speculative.Value; }
        }
    }
}

