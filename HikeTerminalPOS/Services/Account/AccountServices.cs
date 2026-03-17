using System;
using Refit;
using System.Threading.Tasks;
using Fusillade;
using Polly;
using System.Collections.Generic;
using HikePOS.Helpers;
using HikePOS.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace HikePOS.Services
{
    public class AccountServices
    {
        readonly IApiService<IAccountApi> _apiService;

        public AccountServices(IApiService<IAccountApi> apiService)
        {
            _apiService = apiService;
        }

        public async Task<ResponseModel<string>> Login(Priority priority, LoginModel loginmodel)
        {

            ResponseModel<string> loginresponse = null;
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<string>> loginTask;
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                loginTask = _apiService.Background.Login(loginmodel);
                                break;
                            case Priority.UserInitiated:
                                loginTask = _apiService.UserInitiated.Login(loginmodel);
                                break;
                            case Priority.Speculative:
                                loginTask = _apiService.Speculative.Login(loginmodel);
                                break;
                            default:
                                loginTask = _apiService.UserInitiated.Login(loginmodel);
                                break;
                        }

                        loginresponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await loginTask);
                    }
                    catch (ApiException ex)
                    {
                        loginresponse = await ex.GetContentAsAsync<ResponseModel<string>>();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            ex.Track();
                            //Application.Current.MainPage.DisplayAlert("Error", "Sorry, Something went wrong..", "Ok");
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                        return null;
                    }
                }
                else
                {
                    //Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not connected", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return loginresponse;
        }

        public async Task<bool> GetRenewAccessToken(Priority priority, string userid = null)
        {
            try
            {

                AccessTockenDto AccessTockenDtoResponse = null;
                Task<AccessTockenDto> RenewAccessTockenTask;

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    try
                    {
                        string tmpuserid = "test";
                        if (userid == null)
                        {
                            if (Settings.CurrentUser != null)
                            {
                                tmpuserid = Settings.CurrentUser.Id.ToString();
                            }
                        }
                        else
                        {
                            tmpuserid = userid;
                        }


                        Dictionary<string, object> AccessTokenInput = new Dictionary<string, object>();
                        AccessTokenInput = new Dictionary<string, object> {
                                {"grant_type", "client_credentials"},
                                {"storename", Settings.TenantName},
                                {"user_id", tmpuserid},
                                {"client_id", Settings.AccessTokenClientId},        //#37229 iPad: Login session expired
                                {"client_secret", Settings.AccessTokenClientSecretId}}; //#37229 iPad: Login session expired
                                                                                        //}



                        switch (priority)
                        {
                            case Priority.Background:
                                RenewAccessTockenTask = _apiService.Background.RenewAccessToken(AccessTokenInput, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                RenewAccessTockenTask = _apiService.UserInitiated.RenewAccessToken(AccessTokenInput, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                RenewAccessTockenTask = _apiService.Speculative.RenewAccessToken(AccessTokenInput, Settings.AccessToken);
                                break;
                            default:
                                RenewAccessTockenTask = _apiService.UserInitiated.RenewAccessToken(AccessTokenInput, Settings.AccessToken);
                                break;
                        }


                        AccessTockenDtoResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RenewAccessTockenTask);
                    }
                    catch (ApiException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return false;
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
                            //Need to log this error to backend
                            ex.Track();
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            //await Application.Current.MainPage.DisplayAlert("Error", "Sorry, Something went wrong..", "Ok");
                        }
                        return false;
                    }
                }
                else
                {
                    //await Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not connected", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return false;
                }

                if (AccessTockenDtoResponse != null)
                {
                    Settings.AccessToken = AccessTockenDtoResponse.token_type + " " + AccessTockenDtoResponse.access_token;
                    Settings.RefreshToken = AccessTockenDtoResponse.refresh_token;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public async Task<string> GetAdminUrl(Priority priority, AdminInputDto AdminInput)
        {
            try
            {
                AjaxResponse AdminUrlResponse = new AjaxResponse();

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                        AgainCall:
                            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                            httpClient.Timeout = new TimeSpan(0, 10, 0);
                            var data = JsonConvert.SerializeObject(AdminInput);
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

                            var response = await httpClient.PostAsync(url + "/Account/SwitchToUserAccount", content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    AdminUrlResponse = await Task.Run(() => JsonConvert.DeserializeObject<AjaxResponse>(json)).ConfigureAwait(false);
                                }
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                await GetRenewAccessToken(priority);
                                goto AgainCall;
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
                            //Need to log this error to backend
                            ex.Track();
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                        return null;
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return null;
                }

                if (AdminUrlResponse != null && AdminUrlResponse.success)
                {
                    return AdminUrlResponse.targetUrl;
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


        public async Task<ResponseModel<object>> SendPasswordResetLink(Priority priority, SendPasswordResetLinkModel sendPasswordResetLinkModel)
        {
            ResponseModel<object> sendPasswordResetLinkresponse = null;
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<object>> sendPasswordResetLinkTask;
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                sendPasswordResetLinkTask = _apiService.Background.SendPasswordResetLink(sendPasswordResetLinkModel);
                                break;
                            case Priority.UserInitiated:
                                sendPasswordResetLinkTask = _apiService.UserInitiated.SendPasswordResetLink(sendPasswordResetLinkModel);
                                break;
                            case Priority.Speculative:
                                sendPasswordResetLinkTask = _apiService.Speculative.SendPasswordResetLink(sendPasswordResetLinkModel);
                                break;
                            default:
                                sendPasswordResetLinkTask = _apiService.UserInitiated.SendPasswordResetLink(sendPasswordResetLinkModel);
                                break;
                        }

                        sendPasswordResetLinkresponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await sendPasswordResetLinkTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        sendPasswordResetLinkresponse = await ex.GetContentAsAsync<ResponseModel<object>>();
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
                            //Need to log this error to backend
                            ex.Track();
                            //Application.Current.MainPage.DisplayAlert("Error", "Sorry, Something went wrong..", "Ok");
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                        return null;
                    }
                }
                else
                {
                    //Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not connected", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return sendPasswordResetLinkresponse;
        }

        public async Task<ResponseModel<object>> VerifySecurityCode(Priority priority, VerifySecurityCodeDto verifySecurityCodeDto)
        {
            ResponseModel<object> verifySecurityCodeResponse = null;
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<object>> sendPasswordResetLinkTask;
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                sendPasswordResetLinkTask = _apiService.Background.VerifySecurityCode(verifySecurityCodeDto,Settings.AccessToken);                                
                                break;
                            case Priority.UserInitiated:
                                sendPasswordResetLinkTask = _apiService.UserInitiated.VerifySecurityCode(verifySecurityCodeDto,Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                sendPasswordResetLinkTask = _apiService.Speculative.VerifySecurityCode(verifySecurityCodeDto,Settings.AccessToken);
                                break;
                            default:
                                sendPasswordResetLinkTask = _apiService.UserInitiated.VerifySecurityCode(verifySecurityCodeDto,Settings.AccessToken);
                                break;
                        }

                        verifySecurityCodeResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await sendPasswordResetLinkTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        verifySecurityCodeResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
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
                            ex.Track();
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                ex.Track();
            }
            return verifySecurityCodeResponse;
        }

        public async Task<ResponseModel<object>> CheckTenantAvalilable(Priority priority, CheckTenantInputModel checkTenantInputModel)
        {
            ResponseModel<object> checkTenantresponse = null;
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<object>> checkTenantTask;
                    switch (priority)
                    {
                        case Priority.Background:
                            checkTenantTask = _apiService.Background.CheckTenantAvalilable(checkTenantInputModel);
                            break;
                        case Priority.UserInitiated:
                            checkTenantTask = _apiService.UserInitiated.CheckTenantAvalilable(checkTenantInputModel);
                            break;
                        case Priority.Speculative:
                            checkTenantTask = _apiService.Speculative.CheckTenantAvalilable(checkTenantInputModel);
                            break;
                        default:
                            checkTenantTask = _apiService.UserInitiated.CheckTenantAvalilable(checkTenantInputModel);
                            break;
                    }

                    try
                    {
                        checkTenantresponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await checkTenantTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        checkTenantresponse = await ex.GetContentAsAsync<ResponseModel<object>>();
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
                            ex.Track();
                            Extensions.SomethingWentWrong("Checking tenant availibilty.", ex);
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
            catch (Exception ex)
            {
                ex.Track();
            }
            return checkTenantresponse;
        }


        public async Task<ResponseModel<object>> CreateTenant(Priority priority, CreateTenantInputModel createTenantInputModel)
        {
            ResponseModel<object> CreateTenantresponse = null;
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<object>> CreateTenantTask;
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                CreateTenantTask = _apiService.Background.CreateTenant(createTenantInputModel);
                                break;
                            case Priority.UserInitiated:
                                CreateTenantTask = _apiService.UserInitiated.CreateTenant(createTenantInputModel);
                                break;
                            case Priority.Speculative:
                                CreateTenantTask = _apiService.Speculative.CreateTenant(createTenantInputModel);
                                break;
                            default:
                                CreateTenantTask = _apiService.UserInitiated.CreateTenant(createTenantInputModel);
                                break;
                        }

                        CreateTenantresponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await CreateTenantTask);
                    }
                    catch (ApiException ex)
                    {
                        CreateTenantresponse = await ex.GetContentAsAsync<ResponseModel<object>>();
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
                            ex.Track();
                            Extensions.SomethingWentWrong("Creating tenant.", ex);
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
            catch (Exception ex)
            {
                ex.Track();
            }
            return CreateTenantresponse;
        }



        public async Task<ResponseModel<CustomerDisplayCodeResponseModel>> CreateOrUpdateCustomerDisplayCode(Priority priority, CustomerDisplayCodeRequestModel customerDisplayCodeRequestModel)
        {


            ResponseModel<CustomerDisplayCodeResponseModel> customerDisplayCoderesponse = null;
            Task<ResponseModel<CustomerDisplayCodeResponseModel>> customerDisplayCoderesponseTask;
            try
            {

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerDisplayCoderesponseTask = _apiService.Background.CreateOrUpdate(customerDisplayCodeRequestModel, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerDisplayCoderesponseTask = _apiService.UserInitiated.CreateOrUpdate(customerDisplayCodeRequestModel, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerDisplayCoderesponseTask = _apiService.Speculative.CreateOrUpdate(customerDisplayCodeRequestModel, Settings.AccessToken);
                                break;
                            default:
                                customerDisplayCoderesponseTask = _apiService.UserInitiated.CreateOrUpdate(customerDisplayCodeRequestModel, Settings.AccessToken);
                                break;
                        }

                        customerDisplayCoderesponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerDisplayCoderesponseTask);
                    }
                    catch (ApiException ex)
                    {

                        customerDisplayCoderesponse = await ex.GetContentAsAsync<ResponseModel<CustomerDisplayCodeResponseModel>>();
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
                            ex.Track();
                            Extensions.SomethingWentWrong("Error while sending customer display app code...", ex);
                        }
                        return null;
                    }
                }
                else
                {

                    return null;
                }
            }
            catch (Exception ex)
            {

                ex.Track();
            }

            return customerDisplayCoderesponse;
        }

        public async Task<ResponseModel<LoginInformation>> GetCurrentLoginInformations(Priority priority)
        {

            ResponseModel<LoginInformation> loginInformationResponse = null;
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<LoginInformation>> loginTask;
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                loginTask = _apiService.Background.GetCurrentLoginInformations(Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                loginTask = _apiService.Background.GetCurrentLoginInformations(Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                loginTask = _apiService.Background.GetCurrentLoginInformations(Settings.AccessToken);
                                break;
                            default:
                                loginTask = _apiService.Background.GetCurrentLoginInformations(Settings.AccessToken);
                                break;
                        }

                        loginInformationResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await loginTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        loginInformationResponse = await ex.GetContentAsAsync<ResponseModel<LoginInformation>>();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            ex.Track();
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                ex.Track();
            }

            return loginInformationResponse;
        }


    }
}
