using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#if ANDROID
using static Android.Provider.Settings;
#elif IOS
using Foundation;
#endif

using HikePOS.Services;


namespace HikePOS.Services
{
    public class HttpService 
    {
        public HttpClient httpClient()
        {
#if IOS
            var configuration = NSUrlSessionConfiguration.DefaultSessionConfiguration;
            configuration.TimeoutIntervalForRequest = 1200;
            var httpClient = new HttpClient(new MyHttpClientHandler(configuration));
            return httpClient;
#elif ANDROID
            MyHttpClientHandler httpHandler = new MyHttpClientHandler();
            httpHandler.ReadTimeout = TimeSpan.FromMinutes(20.0);
            var httpClient = new HttpClient(httpHandler);
            return httpClient;
#endif
        }



    }

    //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
#if IOS
    public class MyHttpClientHandler : NSUrlSessionHandler
    {
        public MyHttpClientHandler(NSUrlSessionConfiguration configuration) : base(configuration)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentCulture = DependencyService.Get<Interfaces.IMultilingual>().CurrentCultureInfo;
            var code = "en";
            if(!string.IsNullOrEmpty(currentCulture?.TwoLetterISOLanguageName))
                code = currentCulture.TwoLetterISOLanguageName;
            request.RequestUri = request.RequestUri.AddQuery("Abp.Localization.CultureName", code);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }
    }
#elif ANDROID
    public class MyHttpClientHandler : Xamarin.Android.Net.AndroidMessageHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentCulture = DependencyService.Get<Interfaces.IMultilingual>().CurrentCultureInfo;
            var code = "en";
            if(!string.IsNullOrEmpty(currentCulture?.TwoLetterISOLanguageName))
                code = currentCulture.TwoLetterISOLanguageName;
            request.RequestUri = request.RequestUri.AddQuery("Abp.Localization.CultureName", code);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }
    }
#endif
    //Ticket end:#34015 .by rupesh


}
