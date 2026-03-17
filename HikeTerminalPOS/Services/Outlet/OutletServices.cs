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
namespace HikePOS.Services
{
    public class OutletServices
    {

        readonly IApiService<IOutletApi> _apiService;
        readonly IApiService<IAccountApi> accountApiService;
        readonly AccountServices accountService;

        public OutletServices(IApiService<IOutletApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        public async Task<ObservableCollection<OutletDto_POS>> GetRemoteOutlets(Priority priority, bool syncLocal)
        {
            try
            {
                ListResponseModel<OutletDto_POS> outletResponse = null;
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<OutletDto_POS>> outletTask;
                Retry:
                    switch (priority)
                    {
                        case Priority.Background:
                            outletTask = _apiService.Background.GetAll(Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            outletTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            outletTask = _apiService.Speculative.GetAll(Settings.AccessToken);
                            break;
                        default:
                            outletTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
                            break;
                    }
                    Debug.WriteLine("GetOutlets API : api/services/app/outlet/GetAll_POS");
                    try
                    {
                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetOutlets API Request Starts at: " + startDateTime);
                        outletResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await outletTask);
                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetOutlets API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetOutlets API Request Total Time : " + (endDateTime - startDateTime));
                        Debug.WriteLine("GetOutlets API Response : " + JsonConvert.SerializeObject(outletResponse));
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===GetRemoteOutlets===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        outletResponse = await ex.GetContentAsAsync<ListResponseModel<OutletDto_POS>>();

                        if (outletResponse != null && outletResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("\n ===GetRemoteOutlets===1");
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

                            }
                            else
                            {
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting outlets.", ex);
                            }
                        }
                        return null;
                    }

                    if (outletResponse != null && outletResponse.success && outletResponse.result != null && outletResponse.result.items != null && outletResponse.result.items.Any())
                    {
                        if (syncLocal)
                        {
                            UpdateLocalOutlets(outletResponse.result.items);
                        }
                        return outletResponse.result.items;
                    }
                    //Ticket start: #18096 iOS - When Logging in With A No-Outlet User. By rupesh.
                    else if (outletResponse != null && outletResponse.success && outletResponse.result != null && outletResponse.result.items != null && outletResponse.result.items.Count == 0)
                    {
                        App.Instance.Hud.DisplayToast("Your user has no outlet access. Please ask your admin to assign at least one outlet to you.");
                        return null;
                    }
                    //Ticket end:#18096 . By rupesh.

                    else if (priority != Priority.Background && outletResponse != null && outletResponse.error != null && !string.IsNullOrEmpty(outletResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(outletResponse.error.message, Colors.Red, Colors.White);
                        return null;
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
                Logger.SyncLogger("\n ===GetRemoteOutlets===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
                return null;
            }
        }

        public bool UpdateLocalOutlets(ObservableCollection<OutletDto_POS> outlets)
        {
            try
            {
                if (outlets != null && outlets.Any())
                {
                    //await BlobCache.LocalMachine.InsertObjects(outlet_dictionary, DateTimeOffset.Now.AddYears(2));
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(outlets.Select(a => a.ToModel()), update: true);
                    });
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public bool UpdateLocalOutlet(OutletDto_POS outlet)
        {
            try
            {
                if (outlet != null)
                {
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(outlet.ToModel(), update: true);
                    });
                    // await BlobCache.LocalMachine.InsertObject(nameof(OutletDto_POS) + "_" + outlet.Id.ToString(), outlet, DateTimeOffset.Now.AddYears(2));
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }


        public ObservableCollection<OutletDto_POS> GetLocalOutlets()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.All<OutletDB_POS>().ToList();
                var result = new ObservableCollection<OutletDto_POS>(data.Select(a => OutletDto_POS.FromModel(a)));
                return result;
                // return await CommonQueries.GetAllLocals<OutletDto_POS>();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public OutletDto_POS GetLocalOutletById(int id)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.Find<OutletDB_POS>(id);
                return OutletDto_POS.FromModel(data);
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public ObservableCollection<RegisterDto> GetAllLocalRegisterByOutletId(int outletId)
        {
            //Response model object
            try
            {
                var outlet = GetLocalOutletById(outletId);
                if (outlet != null && outlet.OutletRegisters != null && outlet.OutletRegisters.Any())
                {
                    return outlet.OutletRegisters;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return null;
        }


        public async Task<OutletDto_POS> GetRemoteOutletById(Priority priority, bool syncLocal, int OutletId)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    FullAuditedPassiveEntityDto input = new FullAuditedPassiveEntityDto()
                    {
                        Id = OutletId
                    };

                    ResponseModel<OutletDto_POS> OutletResponse = null;
                    Task<ResponseModel<OutletDto_POS>> OutletTask;
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                OutletTask = _apiService.Background.GetDetailById(input, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                OutletTask = _apiService.UserInitiated.GetDetailById(input, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                OutletTask = _apiService.Speculative.GetDetailById(input, Settings.AccessToken);
                                break;
                            default:
                                OutletTask = _apiService.UserInitiated.GetDetailById(input, Settings.AccessToken);
                                break;
                        }

                        OutletResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await OutletTask);
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===GetRemoteOutletById===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        OutletResponse = await ex.GetContentAsAsync<ResponseModel<OutletDto_POS>>();

                        if (OutletResponse != null && OutletResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("\n ===GetRemoteOutletById===1");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        ex.Track();
                        Debug.WriteLine("Exception in GetRemoteOutletById : " + ex.Message + " : " + ex.StackTrace);
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
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting outlet.", ex);
                        }
                        return null;
                    }

                    if (OutletResponse != null && OutletResponse.success && OutletResponse.result != null)
                    {
                        if (syncLocal)
                        {
                            //Ticket #9042 Start : Customer country field added. By Nikhil.
                            var outlet = OutletResponse.result;
                            UpdateLocalOutlet(outlet);
                            Settings.StoreCountryCode = outlet?.Address?.Country;
                            //Ticket #9042 End : By Nikhil.
                        }
                        return OutletResponse.result;
                    }
                    else if (priority != Priority.Background && OutletResponse != null && OutletResponse.error != null && !string.IsNullOrEmpty(OutletResponse.error.message))
                    {
                        Extensions.ServerMessage(OutletResponse.error.message);
                        return null;
                    }
                    else
                    {
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
                Logger.SyncLogger("\n ===GetRemoteOutletById===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);

            }
            return null;
        }

        public async Task<RegisterDto> GetRemoteRegisterById(Priority priority, bool syncLocal, int registerId)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    FullAuditedPassiveEntityDto input = new FullAuditedPassiveEntityDto()
                    {
                        Id = registerId
                    };

                    ResponseModel<RegisterDto> registerResponse = null;
                    Task<ResponseModel<RegisterDto>> registerTask;
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                registerTask = _apiService.Background.GetRegisterById(input, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                registerTask = _apiService.UserInitiated.GetRegisterById(input, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                registerTask = _apiService.Speculative.GetRegisterById(input, Settings.AccessToken);
                                break;
                            default:
                                registerTask = _apiService.UserInitiated.GetRegisterById(input, Settings.AccessToken);
                                break;
                        }


                        registerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await registerTask);
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===GetRegisterById===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        registerResponse = await ex.GetContentAsAsync<ResponseModel<RegisterDto>>();

                        if (registerResponse != null && registerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("\n ===GetRegisterById===1");
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting register.", ex);
                        }
                        return null;
                    }

                    if (registerResponse != null && registerResponse.success && registerResponse.result != null)
                    {
                        if (syncLocal)
                        {
                            UpdateLocalRegister(registerResponse.result);
                        }
                        return registerResponse.result;
                    }
                    else if (priority != Priority.Background && registerResponse != null && registerResponse.error != null && !string.IsNullOrEmpty(registerResponse.error.message))
                    {
                        Extensions.ServerMessage(registerResponse.error.message);
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
                Logger.SyncLogger("\n ===GetRegisterById===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);

            }
            return null;
        }

        public async Task<ListResponseModel<ReceiptTemplateDto>> GetAllTemplatesReceipt(Priority priority, bool syncLocal)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ListResponseModel<ReceiptTemplateDto> registerResponse = null;
                    Task<ListResponseModel<ReceiptTemplateDto>> registerTask;
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                registerTask = _apiService.Background.GetAllTemplatesReceipt(Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                registerTask = _apiService.UserInitiated.GetAllTemplatesReceipt(Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                registerTask = _apiService.Speculative.GetAllTemplatesReceipt(Settings.AccessToken);
                                break;
                            default:
                                registerTask = _apiService.UserInitiated.GetAllTemplatesReceipt(Settings.AccessToken);
                                break;
                        }


                        registerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await registerTask);
                    }
                    catch (ApiException ex)
                    {
                        registerResponse = await ex.GetContentAsAsync<ListResponseModel<ReceiptTemplateDto>>();

                        if (registerResponse != null && registerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting register.", ex);
                        }
                        return null;
                    }

                    if (registerResponse != null && registerResponse.success && registerResponse.result.items != null)
                    {
                        if (syncLocal)
                        {
                            UpdateTemplatesReceipts(registerResponse.result.items);
                        }
                        return registerResponse;
                    }
                    else if (priority != Priority.Background && registerResponse != null && registerResponse.error != null && !string.IsNullOrEmpty(registerResponse.error.message))
                    {
                        Extensions.ServerMessage(registerResponse.error.message);
                    }
                    else
                    {
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
            return null;
        }

        public bool UpdateTemplatesReceipts(ObservableCollection<ReceiptTemplateDto> receiptTemplates)
        {
            try
            {
                if (receiptTemplates != null && receiptTemplates.Any())
                {
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(receiptTemplates.Select(a => a.ToModel()), update: true);
                    });
                    return true;
                }

            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public bool UpdateTemplatesReceipt(ReceiptTemplateDto receiptTemplate)
        {
            try
            {
                if (receiptTemplate != null)
                {
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(receiptTemplate.ToModel(), update: true);
                    });
                    return true;
                }

            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public ReceiptTemplateDto GetReceiptTemplateById(int id)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.Find<ReceiptTemplateDB>(id);
                return ReceiptTemplateDto.FromModel(data);
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public bool ReceiptTemplateExist()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.All<ReceiptTemplateDB>().ToList();
                if (data == null) return false;
                return data.Any();
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }


        //#36109 iPad: Tax collector was updated wrong in cash register.

        public async Task<RegisterclosureDto> GetLastRegisterClosureById(Priority priority, bool syncLocal, int registerId)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    //FullAuditedPassiveEntityDto input = new FullAuditedPassiveEntityDto()
                    //{
                    //    Id = registerId
                    //};

                    ResponseModel<RegisterclosureDto> registerResponse = null;
                    Task<ResponseModel<RegisterclosureDto>> registerTask;
                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                registerTask = _apiService.Background.GetLastRegisterClosureById(registerId, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                registerTask = _apiService.UserInitiated.GetLastRegisterClosureById(registerId, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                registerTask = _apiService.Speculative.GetLastRegisterClosureById(registerId, Settings.AccessToken);
                                break;
                            default:
                                registerTask = _apiService.UserInitiated.GetLastRegisterClosureById(registerId, Settings.AccessToken);
                                break;
                        }


                        registerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await registerTask);
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===GetLastRegisterClosureById===2 = registerId - " + registerId);
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        registerResponse = await ex.GetContentAsAsync<ResponseModel<RegisterclosureDto>>();

                        if (registerResponse != null && registerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("\n ===GetLastRegisterClosureById===1");
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting register.", ex);
                        }
                        return null;
                    }

                    if (registerResponse != null && registerResponse.success && registerResponse.result != null)
                    {
                        //if (syncLocal)
                        //{
                        //    await UpdateLocalRegister(registerResponse.result);
                        //}
                        return registerResponse.result;
                    }
                    else if (priority != Priority.Background && registerResponse != null && registerResponse.error != null && !string.IsNullOrEmpty(registerResponse.error.message))
                    {
                        Extensions.ServerMessage(registerResponse.error.message);
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
                Logger.SyncLogger("\n ===GetLastRegisterClosureById===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
            }
            return null;
        }


        //#36109 iPad: Tax collector was updated wrong in cash register.

        public RegisterDto GetLocalRegisterById(int registerId, int outletId)
        {
            try
            {
                var outlet = GetLocalOutletById(outletId);
                if (outlet != null)
                {
                    return outlet.OutletRegisters.FirstOrDefault(x => x.Id == registerId);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public bool UpdateLocalRegister(RegisterDto register)
        {
            try
            {
                if (register == null)
                {
                    return false;
                }

                var MyOutlet = GetLocalOutletById(register.OutletID);
                if (MyOutlet == null || MyOutlet.OutletRegisters == null || MyOutlet.OutletRegisters.Count < 1 || MyOutlet.OutletRegisters.Where(c => c.Id == register.Id) == null || MyOutlet.OutletRegisters.Where(c => c.Id == register.Id).Count() < 1)
                {
                    return false;
                }

                MyOutlet.OutletRegisters.All(s =>
                {
                    if (s.Id == register.Id)
                    {
                        s.Name = register.Name;
                        s.OutletID = register.OutletID;
                        s.OutletName = register.OutletName;
                        s.ReceiptNumber = register.ReceiptNumber;
                        s.LastCloseDateTime = register.LastCloseDateTime;
                        s.Prefix = register.Prefix;
                        s.Suffix = register.Suffix;
                        s.EmailReceipt = register.EmailReceipt;
                        s.PrintReceipt = register.PrintReceipt;
                        s.IsOpened = register.IsOpened;
                        s.StripeSubscriptionID = register.StripeSubscriptionID;
                        s.DefaultTax = register.DefaultTax;
                        s.DefaultTaxName = register.DefaultTaxName;
                        s.DefaultTaxRate = register.DefaultTaxRate;
                        s.Registerclosure = register.Registerclosure;
                        s.LastRegisterclosure = register.LastRegisterclosure;
                        s.ReceiptTemplateId = register.ReceiptTemplateId;
                        //s.ReceiptTemplate = register.ReceiptTemplate;                    

                        s.GiftReceiptTemplateId = register.GiftReceiptTemplateId;
                        //s.GiftReceiptTemplate = register.GiftReceiptTemplate;
                        s.DocketReceiptTemplateId = register.DocketReceiptTemplateId;
                        //Ticket #9180 Start:Docket Print Note Printing issue. By Nikhil. 
                        //s.DocketReceiptTemplate = register.DocketReceiptTemplate;
                        //Ticket #9180 End:By Nikhil.

                        //Ticket start:#22406 Quote sale.by rupesh
                        s.QuoteReceiptNumber = register.QuoteReceiptNumber;
                        s.QuotePrefix = register.QuotePrefix;
                        s.QuoteSuffix = register.QuoteSuffix;
                        s.QuoteReceiptTemplateId = register.QuoteReceiptTemplateId;
                        //s.QuoteReceiptTemplate = register.QuoteReceiptTemplate;
                        //Ticket end:#22406 .by rupesh

                        //Ticket start:#32377 iPad :: Feature request :: Add Receipt Template for On-account Sales.by rupesh
                        s.OnAccountReceiptTemplateId = register.OnAccountReceiptTemplateId;
                        //s.OnAccountReceiptTemplate = register.OnAccountReceiptTemplate;
                        //Ticket end:#32377 .by rupesh
                        //Ticket start:#62545 Store Credit Not Working In One Scenario.by rupesh
                        s.LayByReceiptTemplateId = register.LayByReceiptTemplateId;
                        //s.LayByReceiptTemplate = register.LayByReceiptTemplate;
                        //Ticket end:#62545.by rupesh
                        //Ticket start:#63873 iOS: Park receipt template not saved locally.by rupesh
                        s.ParkOrderReceiptTemplateId = register.ParkOrderReceiptTemplateId;
                        //s.ParkOrderReceiptTemplate = register.ParkOrderReceiptTemplate;
                        //Ticket end:#63873 .by rupesh
                        //Ticket start:#92763 iOS:FR Backorder Receipts.by rupesh
                        s.BackorderReceiptTemplateId = register.BackorderReceiptTemplateId;
                        //Ticket end:#92763 .by rupesh

                        s.IsActiveCashPayment = register.IsActiveCashPayment;

                    }
                    return true;
                });

                if (Settings.CurrentRegister != null)
                {
                    if (register.Id == Settings.CurrentRegister.Id)
                    {
                        Settings.CurrentRegister = register;
                        //Ticket start:#38783 iPad: Feature request - Register's Name in Process Sale.by rupesh
                        if (Settings.SelectedOutlet != null)
                        {
                            var outlet = Settings.SelectedOutlet;
                            var registerIndex = outlet.OutletRegisters.IndexOf(outlet.OutletRegisters.FirstOrDefault(x => x.Id == register.Id));
                            if (registerIndex >= 0)
                            {
                                outlet.OutletRegisters[registerIndex] = register;
                                Settings.SelectedOutlet = outlet;
                            }
                        }
                        //Ticket end:#38783 iPad.by rupesh

                    }
                }

                UpdateLocalOutlet(MyOutlet);
                //await BlobCache.LocalMachine.InsertObject(nameof(OutletListDto) + "_" + MyOutlet.Id.ToString(), MyOutlet, DateTimeOffset.Now.AddYears(2));
                return true;

            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool UpdateLocalRegisterReceiptNumber(int registerId, int outletId)
        {
            try
            {
                RegisterDto register = GetLocalRegisterById(registerId, outletId);
                if (register != null)
                {
                    //Ticket start:#22406 Quote sale.by rupesh
                    if (Settings.IsQuoteSale)
                        register.QuoteReceiptNumber += 1;
                    else
                        register.ReceiptNumber += 1;
                    //Ticket end:#22406 .by rupesh
                    //register.IsOpened = true;
                    return UpdateLocalRegister(register);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public async Task<RegisterDto> OpenRegister(Priority priority, OpenRegisterInput objOpenRegisterInput)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<RegisterDto> RegisterResponse = null;
                    Task<ResponseModel<RegisterDto>> RegisterOpenTask;
                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                RegisterOpenTask = _apiService.Background.OpenRegister(objOpenRegisterInput, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                RegisterOpenTask = _apiService.UserInitiated.OpenRegister(objOpenRegisterInput, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                RegisterOpenTask = _apiService.Speculative.OpenRegister(objOpenRegisterInput, Settings.AccessToken);
                                break;
                            default:
                                RegisterOpenTask = _apiService.UserInitiated.OpenRegister(objOpenRegisterInput, Settings.AccessToken);
                                break;
                        }


                        RegisterResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RegisterOpenTask);
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===OpenRegister===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        RegisterResponse = await ex.GetContentAsAsync<ResponseModel<RegisterDto>>();
                        if (RegisterResponse != null && RegisterResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("\n ===OpenRegister===1");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);

                        if (priority != Priority.Background)
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
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                        return null;
                    }

                    if (RegisterResponse != null && RegisterResponse.success)
                    {
                        if (RegisterResponse.result != null && RegisterResponse.result.Id > 0 && RegisterResponse.result.OutletID > 0)
                        {
                            //RegisterResponse.result.IsOpened = true;
                            UpdateLocalRegister(RegisterResponse.result);
                        }
                        return RegisterResponse.result;
                    }
                    else if (priority != Priority.Background & RegisterResponse != null && RegisterResponse.error != null && !string.IsNullOrEmpty(RegisterResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(RegisterResponse.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                ex.Track();
                Logger.SyncLogger("\n ===OpenRegister===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
            }
            return null;
        }

        public async Task<RegisterDto> CreateOrUpdateRegisterClosureDenomination(Priority priority, ObservableCollection<RegisterClosureTallyDenominationDto> lstregisterDto)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<RegisterDto> RegisterResponse = null;
                    Task<ResponseModel<RegisterDto>> RegisterTask;
                Retry3:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                RegisterTask = _apiService.Background.CreateOrUpdateRegisterClosureDenomination(lstregisterDto, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                RegisterTask = _apiService.UserInitiated.CreateOrUpdateRegisterClosureDenomination(lstregisterDto, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                RegisterTask = _apiService.Speculative.CreateOrUpdateRegisterClosureDenomination(lstregisterDto, Settings.AccessToken);
                                break;
                            default:
                                RegisterTask = _apiService.UserInitiated.CreateOrUpdateRegisterClosureDenomination(lstregisterDto, Settings.AccessToken);
                                break;
                        }

                        RegisterResponse = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RegisterTask);

                    }
                    catch (ApiException ex)
                    {
                        RegisterResponse = await ex.GetContentAsAsync<ResponseModel<RegisterDto>>();
                        if (RegisterResponse != null && RegisterResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry3;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();

                        Logger.SyncLogger("\n ===CreateOrUpdateRegisterClosureDenomination===1");
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
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }

                    if (RegisterResponse != null && RegisterResponse.success && RegisterResponse.result != null && RegisterResponse.result.Id > 0 && RegisterResponse.result.OutletID > 0)
                    {
                        //RegisterResponse.result.IsOpened = false;
                        UpdateLocalRegister(RegisterResponse.result);
                        return RegisterResponse.result;
                    }
                    else if (priority != Priority.Background && RegisterResponse != null && RegisterResponse.error != null && !string.IsNullOrEmpty(RegisterResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(RegisterResponse.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                Logger.SyncLogger("\n ===CreateOrUpdateRegisterClosureDenomination===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
            }
            return null;
        }


        public async Task<RegisterDto> CloseRegister(Priority priority, RegisterDto registerDto)
        {
            try
            {
                Debug.WriteLine("registerdto  : " + Newtonsoft.Json.JsonConvert.SerializeObject(registerDto));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<RegisterDto> RegisterResponse = null;
                    Task<ResponseModel<RegisterDto>> RegisterTask;
                Retry3:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                RegisterTask = _apiService.Background.CloseRegister(registerDto, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                RegisterTask = _apiService.UserInitiated.CloseRegister(registerDto, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                RegisterTask = _apiService.Speculative.CloseRegister(registerDto, Settings.AccessToken);
                                break;
                            default:
                                RegisterTask = _apiService.UserInitiated.CloseRegister(registerDto, Settings.AccessToken);
                                break;
                        }

                        RegisterResponse = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RegisterTask);

                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===CloseRegister===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        RegisterResponse = await ex.GetContentAsAsync<ResponseModel<RegisterDto>>();
                        if (RegisterResponse != null && RegisterResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry3;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                        Logger.SyncLogger("\n ===CloseRegister===1");
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
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }

                    if (RegisterResponse != null && RegisterResponse.success && RegisterResponse.result != null && RegisterResponse.result.Id > 0 && RegisterResponse.result.OutletID > 0)
                    {
                        //RegisterResponse.result.IsOpened = false;
                        UpdateLocalRegister(RegisterResponse.result);
                        return RegisterResponse.result;
                    }
                    else if (priority != Priority.Background && RegisterResponse != null && RegisterResponse.error != null && !string.IsNullOrEmpty(RegisterResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(RegisterResponse.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                ex.Track();
                Logger.SyncLogger("\n ===CloseRegister===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
            }
            return null;
        }

        public async Task<RegisterclosureDto> GetRegisterClosure(Priority priority, int RegisterId)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<RegisterclosureDto> RegisterclosureResponse;
                    Task<ResponseModel<RegisterclosureDto>> RegisterclosureTask;
                Retry4:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                RegisterclosureTask = _apiService.Background.GetRegisterclosure(Settings.AccessToken, RegisterId);
                                break;
                            case Priority.UserInitiated:
                                RegisterclosureTask = _apiService.UserInitiated.GetRegisterclosure(Settings.AccessToken, RegisterId);
                                break;
                            case Priority.Speculative:
                                RegisterclosureTask = _apiService.Speculative.GetRegisterclosure(Settings.AccessToken, RegisterId);
                                break;
                            default:
                                RegisterclosureTask = _apiService.UserInitiated.GetRegisterclosure(Settings.AccessToken, RegisterId);
                                break;
                        }


                        RegisterclosureResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RegisterclosureTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        Logger.SyncLogger("\n ===GetRegisterclosure===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        RegisterclosureResponse = await ex.GetContentAsAsync<ResponseModel<RegisterclosureDto>>();
                        if (RegisterclosureResponse != null && RegisterclosureResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry4;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SyncLogger("\n ===GetRegisterclosure===1");
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
                            }
                            else
                            {
                                Extensions.SomethingWentWrong("Getting register closure.", ex);
                            }
                        }
                        return null;
                    }

                    if (RegisterclosureResponse != null && RegisterclosureResponse.success && RegisterclosureResponse.result != null)
                    {
                        return RegisterclosureResponse.result;
                    }
                    else if (priority != Priority.Background && RegisterclosureResponse != null && RegisterclosureResponse.error != null && !string.IsNullOrEmpty(RegisterclosureResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(RegisterclosureResponse.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                Logger.SyncLogger("\n ===GetRegisterclosure===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
            }
            return null;
        }

        public async Task<AjaxResponse> UpdateRegisterClosure(Priority priority, int RegisterClosureId)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<AjaxResponse> UpdateRegisterClosureResponse;
                    Task<ResponseModel<AjaxResponse>> UpdateRegisterClosureTask;
                Retry4:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                UpdateRegisterClosureTask = _apiService.Background.UpdateRegisterClosure(Settings.AccessToken, RegisterClosureId);
                                break;
                            case Priority.UserInitiated:
                                UpdateRegisterClosureTask = _apiService.UserInitiated.UpdateRegisterClosure(Settings.AccessToken, RegisterClosureId);
                                break;
                            case Priority.Speculative:
                                UpdateRegisterClosureTask = _apiService.Speculative.UpdateRegisterClosure(Settings.AccessToken, RegisterClosureId);
                                break;
                            default:
                                UpdateRegisterClosureTask = _apiService.UserInitiated.UpdateRegisterClosure(Settings.AccessToken, RegisterClosureId);
                                break;
                        }


                        UpdateRegisterClosureResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await UpdateRegisterClosureTask);

                        Debug.WriteLine("UpdateRegisterClosureResponse  : " + Newtonsoft.Json.JsonConvert.SerializeObject(UpdateRegisterClosureResponse));
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===UpdateRegisterClosure===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        UpdateRegisterClosureResponse = await ex.GetContentAsAsync<ResponseModel<AjaxResponse>>();
                        if (UpdateRegisterClosureResponse != null && UpdateRegisterClosureResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry4;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (priority != Priority.Background)
                        {
                            Logger.SyncLogger("\n ===UpdateRegisterClosure===1");
                            Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
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
                                Extensions.SomethingWentWrong("Getting register closure.", ex);
                            }
                        }
                        return null;
                    }

                    if (UpdateRegisterClosureResponse != null && UpdateRegisterClosureResponse.success)
                    {
                        return UpdateRegisterClosureResponse;
                    }
                    else if (priority != Priority.Background && UpdateRegisterClosureResponse != null && UpdateRegisterClosureResponse.error != null && !string.IsNullOrEmpty(UpdateRegisterClosureResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(UpdateRegisterClosureResponse.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                Logger.SyncLogger("\n ===UpdateRegisterClosure===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
            }
            return null;
        }


        public async Task<RegisterCashInOutDto> AddCashInOutRegister(Priority priority, RegisterCashInOutDto registerCashInOutDto)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<RegisterCashInOutDto> RegisterCashInOut = null;
                    Task<ResponseModel<RegisterCashInOutDto>> RegisterCashInOutTask;
                Retry5:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                RegisterCashInOutTask = _apiService.Background.CreateOrUpdateRegisterCashInOut(registerCashInOutDto, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                RegisterCashInOutTask = _apiService.UserInitiated.CreateOrUpdateRegisterCashInOut(registerCashInOutDto, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                RegisterCashInOutTask = _apiService.Speculative.CreateOrUpdateRegisterCashInOut(registerCashInOutDto, Settings.AccessToken);
                                break;
                            default:
                                RegisterCashInOutTask = _apiService.UserInitiated.CreateOrUpdateRegisterCashInOut(registerCashInOutDto, Settings.AccessToken);
                                break;
                        }

                        RegisterCashInOut = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RegisterCashInOutTask);

                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===CreateOrUpdateRegisterCashInOut===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        RegisterCashInOut = await ex.GetContentAsAsync<ResponseModel<RegisterCashInOutDto>>();
                        if (RegisterCashInOut != null && RegisterCashInOut.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry5;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SyncLogger("\n ===CreateOrUpdateRegisterCashInOut===1");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        ex.Track();
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    //Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not working", "Ok");
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }

                    if (RegisterCashInOut != null && RegisterCashInOut.success && RegisterCashInOut.result != null)
                    {
                        return RegisterCashInOut.result;
                    }
                    else if (priority != Priority.Background && RegisterCashInOut != null && RegisterCashInOut.error != null && !string.IsNullOrEmpty(RegisterCashInOut.error.message))
                    {
                        App.Instance.Hud.DisplayToast(RegisterCashInOut.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                Logger.SyncLogger("\n ===CreateOrUpdateRegisterCashInOut===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
            }
            return null;
        }

        public async Task<ObservableCollection<DenominationDto>> GetRemoteDenomination(Priority priority, bool syncLocal)
        {
            try
            {
                ListResponseModel<DenominationDto> DenominationResponse = null;
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<DenominationDto>> DenominationTask;
                Retry:
                    switch (priority)
                    {
                        case Priority.Background:
                            DenominationTask = _apiService.Background.GetAllDenomination(Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            DenominationTask = _apiService.UserInitiated.GetAllDenomination(Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            DenominationTask = _apiService.Speculative.GetAllDenomination(Settings.AccessToken);
                            break;
                        default:
                            DenominationTask = _apiService.UserInitiated.GetAllDenomination(Settings.AccessToken);
                            break;
                    }

                    try
                    {
                        DenominationResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await DenominationTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        Logger.SyncLogger("\n ===GetAllDenomination===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        DenominationResponse = await ex.GetContentAsAsync<ListResponseModel<DenominationDto>>();

                        if (DenominationResponse != null && DenominationResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Logger.SyncLogger("\n ===GetAllDenomination===1");
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

                            }
                            else
                            {
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting denominations.", ex);
                            }
                        }
                        return null;
                    }
                    //Ticket start:#30955 iOS - Denomination Entering When Opening Registers.by rupesh
                    if (DenominationResponse != null && DenominationResponse.success && DenominationResponse.result != null && DenominationResponse.result.items != null)
                    {
                        //Ticket end:#30955 .by rupesh
                        if (syncLocal)
                        {
                            UpdateAllLocalDenominations(DenominationResponse.result.items);
                        }
                        return DenominationResponse.result.items;
                    }
                    else if (priority != Priority.Background && DenominationResponse != null && DenominationResponse.error != null && !string.IsNullOrEmpty(DenominationResponse.error.message))
                    {
                        Extensions.ServerMessage(DenominationResponse.error.message);
                        return null;
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
                Logger.SyncLogger("\n ===GetAllDenomination===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                ex.Track();
                return null;
            }
        }


        public ObservableCollection<DenominationDto> GetLocalDenomination()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var data = realm.All<DenominationDB>().ToList();
                if (data != null)
                    return new ObservableCollection<DenominationDto>(data.Select(a => DenominationDto.FromModel(a)).OrderBy(x => x.Value).ToList());
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }
        public bool UpdateAllLocalDenominations(ObservableCollection<DenominationDto> denominations)
        {
            try
            {
                if (denominations != null)
                {
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(denominations.Select(a => a.ToModel()).ToList(), update: true);
                    });
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public async Task<DenominationDto> CreateOrUpdateRemoteDenomination(Priority priority, DenominationDto Denomination)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<DenominationDto> ResponseDenomination = null;
                    Task<ResponseModel<DenominationDto>> DenominationTask;
                Retry5:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                DenominationTask = _apiService.Background.CreateOrUpdateDenomination(Denomination, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                DenominationTask = _apiService.UserInitiated.CreateOrUpdateDenomination(Denomination, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                DenominationTask = _apiService.Speculative.CreateOrUpdateDenomination(Denomination, Settings.AccessToken);
                                break;
                            default:
                                DenominationTask = _apiService.UserInitiated.CreateOrUpdateDenomination(Denomination, Settings.AccessToken);
                                break;
                        }

                        ResponseDenomination = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await DenominationTask);

                    }
                    catch (ApiException ex)
                    {
                        ResponseDenomination = await ex.GetContentAsAsync<ResponseModel<DenominationDto>>();
                        if (ResponseDenomination != null && ResponseDenomination.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry5;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                        Logger.SyncLogger("\n ===CreateOrUpdateRemoteDenomination===1");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    //Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not working", "Ok");
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }

                    if (ResponseDenomination != null && ResponseDenomination.success && ResponseDenomination.result != null)
                    {
                        UpdateLocalDenomination(ResponseDenomination.result);
                        return ResponseDenomination.result;
                    }
                    else if (priority != Priority.Background && ResponseDenomination != null && ResponseDenomination.error != null && !string.IsNullOrEmpty(ResponseDenomination.error.message))
                    {
                        App.Instance.Hud.DisplayToast(ResponseDenomination.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }


        public bool UpdateLocalDenomination(DenominationDto Denomination)
        {
            try
            {
                if (Denomination != null)
                {
                    // await BlobCache.LocalMachine.InsertObject(nameof(DenominationDto) + "_" + Denomination.Id.ToString(), Denomination, DateTimeOffset.Now.AddYears(2));

                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(Denomination.ToModel(), update: true);
                    });
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public async Task<bool> RemoveDenomination(Priority priority, DenominationDto Denomination)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<DenominationDto> ResponseDenomination = null;
                    Task<ResponseModel<DenominationDto>> DenominationTask;
                Retry5:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                DenominationTask = _apiService.Background.DeleteDenomination(Denomination, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                DenominationTask = _apiService.UserInitiated.DeleteDenomination(Denomination, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                DenominationTask = _apiService.Speculative.DeleteDenomination(Denomination, Settings.AccessToken);
                                break;
                            default:
                                DenominationTask = _apiService.UserInitiated.DeleteDenomination(Denomination, Settings.AccessToken);
                                break;
                        }

                        ResponseDenomination = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await DenominationTask);

                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===DeleteDenomination===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        ResponseDenomination = await ex.GetContentAsAsync<ResponseModel<DenominationDto>>();
                        if (ResponseDenomination != null && ResponseDenomination.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry5;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SyncLogger("\n ===DeleteDenomination===1");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        ex.Track();
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                                if (!isReachable)
                                {
                                    //Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not working", "Ok");
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                }
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                    }

                    if (ResponseDenomination != null && ResponseDenomination.success)
                    {
                        // await BlobCache.LocalMachine.Invalidate(nameof(DenominationDto) + "_" + Denomination.Id.ToString());

                        using var realm = RealmService.GetRealm();
                        var data = realm.Find<DenominationDB>(Denomination.Id);
                        if (data != null)
                        {
                            realm.Write(() =>
                            {
                                realm.Remove(data);
                            });
                        }
                        return true;
                    }
                    else if (priority != Priority.Background && ResponseDenomination != null && ResponseDenomination.error != null && !string.IsNullOrEmpty(ResponseDenomination.error.message))
                    {
                        App.Instance.Hud.DisplayToast(ResponseDenomination.error.message, Colors.Red, Colors.White);
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Logger.SyncLogger("\n ===DeleteDenomination===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);

            }
            return true;
        }

        public async Task<ResponseModel<AjaxResponse>> UpdateRegisterClosureMerchantSettleReciept(Priority priority, RecieptDataRequest recieptDataRequest)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<AjaxResponse> UpdateRegisterClosureMerchantSettleRecieptResponse;
                    Task<ResponseModel<AjaxResponse>> UpdateRegisterClosureMerchantSettleRecieptTask;
                Retry4:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                UpdateRegisterClosureMerchantSettleRecieptTask = _apiService.Background.UpdateRegisterClosureMerchantSettleReciept(recieptDataRequest, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                UpdateRegisterClosureMerchantSettleRecieptTask = _apiService.UserInitiated.UpdateRegisterClosureMerchantSettleReciept(recieptDataRequest, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                UpdateRegisterClosureMerchantSettleRecieptTask = _apiService.Speculative.UpdateRegisterClosureMerchantSettleReciept(recieptDataRequest, Settings.AccessToken);
                                break;
                            default:
                                UpdateRegisterClosureMerchantSettleRecieptTask = _apiService.UserInitiated.UpdateRegisterClosureMerchantSettleReciept(recieptDataRequest, Settings.AccessToken); break;
                        }


                        UpdateRegisterClosureMerchantSettleRecieptResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await UpdateRegisterClosureMerchantSettleRecieptTask);

                        Debug.WriteLine("UpdateRegisterClosureMerchantSettleRecieptResponse  : " + Newtonsoft.Json.JsonConvert.SerializeObject(UpdateRegisterClosureMerchantSettleRecieptResponse));
                    }
                    catch (ApiException ex)
                    {
                        Logger.SyncLogger("\n ===UpdateRegisterClosureMerchantSettleReciept===2");
                        Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                        //Get Exception content
                        UpdateRegisterClosureMerchantSettleRecieptResponse = await ex.GetContentAsAsync<ResponseModel<AjaxResponse>>();
                        if (UpdateRegisterClosureMerchantSettleRecieptResponse != null && UpdateRegisterClosureMerchantSettleRecieptResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry4;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SyncLogger("\n ===UpdateRegisterClosureMerchantSettleReciept===1");
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
                            }
                            else
                            {
                                Extensions.SomethingWentWrong("Getting register closure.", ex);
                            }
                        }
                        return null;
                    }

                    if (UpdateRegisterClosureMerchantSettleRecieptResponse != null && UpdateRegisterClosureMerchantSettleRecieptResponse.success)
                    {
                        return UpdateRegisterClosureMerchantSettleRecieptResponse;
                    }
                    else if (priority != Priority.Background && UpdateRegisterClosureMerchantSettleRecieptResponse != null && UpdateRegisterClosureMerchantSettleRecieptResponse.error != null && !string.IsNullOrEmpty(UpdateRegisterClosureMerchantSettleRecieptResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(UpdateRegisterClosureMerchantSettleRecieptResponse.error.message, Colors.Red, Colors.White);
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
            catch (Exception ex)
            {
                ex.Track();
                Logger.SyncLogger("\n ===UpdateRegisterClosureMerchantSettleReciept===3");
                Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);

            }
            return null;
        }
        //Ticket start:#55624 Integrate API to check reregister is opened or closed.by rupesh
        public async Task<bool> CheckIfRegisterClosed(Priority priority, int ClosureId)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<object> CheckIfRegisterClosedResponse;
                    Task<ResponseModel<object>> CheckIfRegisterClosedTask;
                Retry4:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                CheckIfRegisterClosedTask = _apiService.Background.CheckIfRegisterClosed(Settings.AccessToken, ClosureId);
                                break;
                            case Priority.UserInitiated:
                                CheckIfRegisterClosedTask = _apiService.UserInitiated.CheckIfRegisterClosed(Settings.AccessToken, ClosureId);
                                break;
                            case Priority.Speculative:
                                CheckIfRegisterClosedTask = _apiService.Speculative.CheckIfRegisterClosed(Settings.AccessToken, ClosureId);
                                break;
                            default:
                                CheckIfRegisterClosedTask = _apiService.UserInitiated.CheckIfRegisterClosed(Settings.AccessToken, ClosureId);
                                break;
                        }


                        CheckIfRegisterClosedResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await CheckIfRegisterClosedTask);

                        Debug.WriteLine("CheckIsOpenRegisterResponse  : " + Newtonsoft.Json.JsonConvert.SerializeObject(CheckIfRegisterClosedResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        CheckIfRegisterClosedResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (CheckIfRegisterClosedResponse != null && CheckIfRegisterClosedResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry4;
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
                                Extensions.SomethingWentWrong("Getting register closure.", ex);
                            }
                        }
                        return false;
                    }

                    if (CheckIfRegisterClosedResponse != null && CheckIfRegisterClosedResponse.success)
                    {
                        return (bool)CheckIfRegisterClosedResponse.result;
                    }
                    else if (priority != Priority.Background && CheckIfRegisterClosedResponse != null && CheckIfRegisterClosedResponse.error != null && !string.IsNullOrEmpty(CheckIfRegisterClosedResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(CheckIfRegisterClosedResponse.error.message, Colors.Red, Colors.White);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }
        //Ticket end:#55624 .by rupesh
        
        //Start #92768 Pratik
        public async Task<object> SendRegisterClosureEmail(Priority priority, SendRegisterClosureDto registerDto)
        {
            try
            {
                Debug.WriteLine("registerdto  : " + Newtonsoft.Json.JsonConvert.SerializeObject(registerDto));
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<object> RegisterResponse = null;
                    Task<ResponseModel<object>> RegisterTask;
                Retry3:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                RegisterTask = _apiService.Background.SendRegisterClosureEmail(registerDto, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                RegisterTask = _apiService.UserInitiated.SendRegisterClosureEmail(registerDto, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                RegisterTask = _apiService.Speculative.SendRegisterClosureEmail(registerDto, Settings.AccessToken);
                                break;
                            default:
                                RegisterTask = _apiService.UserInitiated.SendRegisterClosureEmail(registerDto, Settings.AccessToken);
                                break;
                        }
                        RegisterResponse = await Policy
                            .Handle<ApiException>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await RegisterTask);

                    }
                    catch (ApiException ex)
                    {
                        RegisterResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (RegisterResponse != null && RegisterResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry3;
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
                    }

                    if (RegisterResponse != null && RegisterResponse.success)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EmailSuccess"), Colors.Green, Colors.White);
                    }
                    else if (priority != Priority.Background && RegisterResponse != null && RegisterResponse.error != null && !string.IsNullOrEmpty(RegisterResponse.error.message))
                    {
                        App.Instance.Hud.DisplayToast(RegisterResponse.error.message, Colors.Red, Colors.White);
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }
        //End #92768 Pratik


    }
}
