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
using HikePOS.Enums;
using System.Net.Http;
using HikePOS.Model;
using Newtonsoft.Json;
using System.Text;
using HikePOS.Models.Log;
using System.Diagnostics;
using HikePOS.Models.Sales;
using CommunityToolkit.Mvvm.Messaging;
using RestSharp.Serializers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace HikePOS.Services
{
    public class SaleServices
    {

        private readonly IApiService<ISaleApi> _apiService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;

        private static readonly SerialQueue _invoiceSyncQueue = new();

        CustomerServices customerService;

        //Ticket #11319 Start : Send logs to server. By Nikhil
        ApiService<ISubmitLogApi> logApiService = new ApiService<ISubmitLogApi>();
        SubmitLogServices logService;

        Dictionary<string, string> logRequestDetails = new Dictionary<string, string>();
        //Ticket #11319 End. By Nikhil

        public SaleServices(IApiService<ISaleApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
            logService = new SubmitLogServices(logApiService);
        }

        public LocalInvoiceDto GetPendingInvoiceFromDB()
        {
            try
            {
                Debug.WriteLine("Pending invoice start : " + System.DateTime.Now);
                //var Invoices = await BlobCache.LocalMachine.GetAllObjects<InvoiceDto>();
                var realm = RealmService.GetRealm();
                var inv = realm.All<LocalInvoiceDB>().Where(x => (x.Status == -1
                    || x.Status == 0)).ToList();
                var invoice = inv.Select(a => LocalInvoiceDto.FromModel(a)).LastOrDefault();

                Debug.WriteLine("Pending invoice end : " + System.DateTime.Now);
                Debug.WriteLine("Pending invoice  : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoice));
                return invoice;
            }
            catch (Exception ex)
            {

                ex.Track();
                return null;
            }
        }

        public ObservableCollection<InvoiceDto> GetLocalInvoices()
        {

            Debug.WriteLine("GetLocalInvoices 1: " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
            var realm = RealmService.GetRealm();
            var inv = realm.All<InvoiceDB>().ToList();
            var invoices = inv.Select(a => InvoiceDto.FromModel(a));
            return new ObservableCollection<InvoiceDto>(invoices.Where(x =>x.Status != InvoiceStatus.Pending && x.Status != InvoiceStatus.initial).OrderByDescending(x => x.TransactionDate));
        }

        public ObservableCollection<InvoiceDto> GetLocalInvoicesByCustomerId(int CustomerId)
        {
            var realm = RealmService.GetRealm();
            var inv = realm.All<InvoiceDB>().Where(x => x.CustomerId == CustomerId && !string.IsNullOrEmpty(x.Number)).ToList();
            var invoices = inv.Select(a => InvoiceDto.FromModel(a));
            return new ObservableCollection<InvoiceDto>(invoices);
        }

        public InvoiceDto GetLocalInvoicesByInvoieNumber(string InvoieNumber, string InvoiceId)
        {
            var realm = RealmService.GetRealm();
            try
            {
                var inv = realm.All<InvoiceDB>().Where(x => x.Id == Int32.Parse(InvoiceId)).ToList();
                var invoices = inv.Select(a => InvoiceDto.FromModel(a));
                return invoices.First();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                var inv = realm.All<InvoiceDB>().Where(x => x.Number == InvoieNumber).ToList();
                var invoices = inv.Select(a => InvoiceDto.FromModel(a));
                return invoices.First();
            }
        }

        public InvoiceDto GetLocalInvoice(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var inv = realm.Find<InvoiceDB>(id.ToString());
                var invoice = InvoiceDto.FromModel(inv);
                return invoice;
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

        //#94565
        public List<InvoiceDto> GetLocalInvoicesListByInvoieNumber(string InvoieNumber)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var inv = realm.All<InvoiceDB>().Where(x => x.Number == InvoieNumber).ToList().Select(a => InvoiceDto.FromModel(a)).ToList();
                return inv;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public InvoiceDto GetLocalInvoiceByTableId(int tabelid)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var inv = realm.All<InvoiceDB>().Where(x => x.TableId == tabelid && (x.Status == (int)InvoiceStatus.OnGoing || x.Status == (int)InvoiceStatus.initial || x.Status == (int)InvoiceStatus.Pending)).ToList();
                if (inv.Count > 1)
                {

                }
                var invoice = InvoiceDto.FromModel(inv?.FirstOrDefault());
                return invoice;
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

        public int GetOccupiedTableCount()
        {
            try
            {
                if (!Settings.IsRestaurantPOS)
                    return 0;
                using var realm = RealmService.GetRealm();
                var inv = realm.All<InvoiceDB>().Where(x => x.TableId > 0 && (x.Status == (int)InvoiceStatus.OnGoing || x.Status == (int)InvoiceStatus.initial || x.Status == (int)InvoiceStatus.Pending)).Count();
                return inv;
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
                return 0;
            }
            catch (Exception ex)
            {
                ex.Track();
                return 0;
            }
        }

        public List<InvoiceDto> GetOccupiedTables()
        {
            try
            {
                if (!Settings.IsRestaurantPOS)
                    return new List<InvoiceDto>();
                using var realm = RealmService.GetRealm();
                var inv = realm.All<InvoiceDB>().Where(x => x.TableId > 0 && (x.Status == (int)InvoiceStatus.OnGoing || x.Status == (int)InvoiceStatus.initial || x.Status == (int)InvoiceStatus.Pending)).ToList();
                var invoices = inv.Select(a => InvoiceDto.FromModel(a));
                return invoices.ToList();
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
                return new List<InvoiceDto>();
            }
            catch (Exception ex)
            {
                ex.Track();
                return new List<InvoiceDto>();
            }
        }
       
         public async Task<ObservableCollection<OccupideTableDto>> GetFloorSalesList(Priority priority)
        {

            Task<ResponseListModel<OccupideTableDto>> hasFloorSalesTask;
            ResponseListModel<OccupideTableDto> hasFloorSalesResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        hasFloorSalesTask = _apiService.Background.GetFloorSalesList(Settings.SelectedOutletId, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        hasFloorSalesTask = _apiService.UserInitiated.GetFloorSalesList(Settings.SelectedOutletId, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        hasFloorSalesTask = _apiService.Speculative.GetFloorSalesList(Settings.SelectedOutletId, Settings.AccessToken);
                        break;
                    default:
                        hasFloorSalesTask = _apiService.UserInitiated.GetFloorSalesList(Settings.SelectedOutletId, Settings.AccessToken);
                        break;
                }

                try
                {
                    hasFloorSalesResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .ExecuteAsync(async () => await hasFloorSalesTask);
                }
                catch (ApiException ex)
                {
                    hasFloorSalesResponse = await ex.GetContentAsAsync<ResponseListModel<OccupideTableDto>>();
                    if (hasFloorSalesResponse != null && hasFloorSalesResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("GetFloorSalesList");
                        }
                    }
                    ;
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }

            if (hasFloorSalesResponse != null && hasFloorSalesResponse.success)
            {
                return hasFloorSalesResponse.result;
            }
            else
            {
                if (priority != Priority.Background && hasFloorSalesResponse != null && hasFloorSalesResponse.error != null && hasFloorSalesResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hasFloorSalesResponse.error.message, Colors.Red, Colors.White);
                }
                return null;
            }

        }    
       
        //#94565

        public InvoiceDto GetLocalInvoiceByTempId(string InvoiceTempId)
        {
            try
            {
                //return await CommonQueries.GetObject<InvoiceDto>(InvoiceTempId);
                var realm = RealmService.GetRealm();
                var inv = realm.All<InvoiceDB>().Where(x => x.InvoiceTempId == InvoiceTempId).ToList();
                var invoices = inv.Select(a => InvoiceDto.FromModel(a));
                return invoices.LastOrDefault();

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

        public ObservableCollection<InvoiceDto> GetLocalSearchInvoices(string keyword)
        {
            var realm = RealmService.GetRealm();
            var inv = realm.All<InvoiceDB>().Where(x => x.CustomerName.Contains(keyword) || x.ServedByName.Contains(keyword) || x.Number.Contains(keyword)).ToList();
            var invoices = inv.Select(a => InvoiceDto.FromModel(a));
            return new ObservableCollection<InvoiceDto>(invoices);
        }

        ListResponseModel<InvoiceDto> remoteinvoiceResponse;

        public async Task<ObservableCollection<InvoiceDto>> GetRemoteInvoices(Priority priority, bool syncLocal, int outletId, DateTime? FromDate, DateTime? ToDate, List<InvoiceStatus> status, string filter = "")
        {
            remoteinvoiceResponse = new ListResponseModel<InvoiceDto>();
            Task<ListResponseModel<InvoiceDto>> invoiceTask;

            var maxResult = 1;

            GetInvoiceInput invoiceFilter = new GetInvoiceInput
            {
                toDate = ToDate,
                maxResultCount = maxResult,
                skipCount = 0,
                outletId = outletId,
                filter = filter,
                sorting = "0",
                fromDate = FromDate,
                status = status //new List<InvoiceStatus>() { InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy }
            };

            Debug.WriteLine("GetRemoteInvoices request : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceFilter).ToJson());
        Retry:
            switch (priority)
            {
                case Priority.Background:
                    invoiceTask = _apiService.Background.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    invoiceTask = _apiService.UserInitiated.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    invoiceTask = _apiService.Speculative.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                    break;
                default:
                    invoiceTask = _apiService.UserInitiated.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    remoteinvoiceResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await invoiceTask);

                    Debug.WriteLine("remoteinvoiceResponse : " + JsonConvert.SerializeObject(remoteinvoiceResponse));

                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    remoteinvoiceResponse = await ex.GetContentAsAsync<ListResponseModel<InvoiceDto>>();

                    if (remoteinvoiceResponse != null && remoteinvoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting invoices.", ex);
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

            if (remoteinvoiceResponse != null && remoteinvoiceResponse.success && remoteinvoiceResponse.result != null && remoteinvoiceResponse.result.totalCount > 0)
            {
                //remoteinvoiceResponse.result.items = new ObservableCollection<InvoiceDto>(remoteinvoiceResponse.result.items.Where
                //    (x => x.Status != InvoiceStatus.Pending));

                remoteinvoiceResponse.result.items.All
                    (s =>
                    {
                        s.isSync = true;
                        return true;
                    });
                var totalProduct = remoteinvoiceResponse.result.totalCount;
                int totalStep = 1;
                maxResult = 100;
                if (totalProduct > 100)
                {
                    double tstep = (Convert.ToDouble(totalProduct) / maxResult);
                    totalStep = (int)Math.Ceiling(tstep);
                }

                var tasks = new List<Task>();
                //totalStep = 100;
                //var watch = System.Diagnostics.Stopwatch.StartNew();

                Debug.WriteLine("totalStep : " + totalProduct.ToString());

                for (var i = 0; i <= totalStep; i++)
                {
                    var _invoiceFilter = invoiceFilter.Copy();
                    _invoiceFilter.skipCount = (i * maxResult) + 1;
                    _invoiceFilter.maxResultCount = maxResult;
                    Debug.WriteLine("before GetRemoteInvoiceAndStoreLocal : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    Task tmptask = GetRemoteInvoiceAndStoreLocal(priority, false, _invoiceFilter);
                    Debug.WriteLine("after GetRemoteInvoiceAndStoreLocal : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    tasks.Add(tmptask);

                }
                await Task.WhenAll(tasks.ToArray());
                // await Task.WhenAll(tasks.ToArray());
                Debug.WriteLine("Task.WhenAll : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH: mm:ss"));
                if (syncLocal && remoteinvoiceResponse.result != null && remoteinvoiceResponse.result.items.Count > 0)
                {
                    remoteinvoiceResponse.result.items.All(c => { c.isSync = true; return true; });

                    //Added by rupesh.Filter by outlet. Only allow local saving if filtered outlet is same as selected outlet
                    Debug.WriteLine("before UpdateLocalInvoices : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    if (outletId == Settings.SelectedOutletId)
                        UpdateLocalInvoices(remoteinvoiceResponse.result.items);
                    Debug.WriteLine("after UpdateLocalInvoices : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                }
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                return remoteinvoiceResponse.result.items;
            }
            else
            {
                if (priority != Priority.Background && remoteinvoiceResponse != null && remoteinvoiceResponse.error != null && remoteinvoiceResponse.error.message != null)
                {
                    Extensions.ServerMessage(remoteinvoiceResponse.error.message);
                }
                return null;
            }
        }

        public async Task GetRemoteInvoiceAndStoreLocal(Priority priority, bool syncLocal, GetInvoiceInput invoiceFilter)
        {

            try
            {
                ListResponseModel<InvoiceDto> tmpremoteinvoiceResponse = new ListResponseModel<InvoiceDto>();
                Task<ListResponseModel<InvoiceDto>> invoiceTask;

                Debug.WriteLine("GetRemoteInvoiceAndStoreLocal request : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        invoiceTask = _apiService.Background.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        invoiceTask = _apiService.UserInitiated.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        invoiceTask = _apiService.Speculative.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                    default:
                        invoiceTask = _apiService.UserInitiated.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                }

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        tmpremoteinvoiceResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds * 2))
                            .ExecuteAsync(async () => await invoiceTask);


                        Debug.WriteLine("GetRemoteInvoiceAndStoreLocal response : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        tmpremoteinvoiceResponse = await ex.GetContentAsAsync<ListResponseModel<InvoiceDto>>();

                        if (tmpremoteinvoiceResponse != null && tmpremoteinvoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting invoice and storing locally.", ex);
                            }
                        }
                        return;
                    }
                }
                else
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                    return;
                }

                if (tmpremoteinvoiceResponse != null && tmpremoteinvoiceResponse.success && tmpremoteinvoiceResponse.result != null && tmpremoteinvoiceResponse.result.totalCount > 0)
                {
                    Debug.WriteLine("remoteinvoiceResponse Concat start : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    if (remoteinvoiceResponse != null && remoteinvoiceResponse.result != null && remoteinvoiceResponse.result.items != null)
                    {
                        remoteinvoiceResponse.result.items = new ObservableCollection<InvoiceDto>(remoteinvoiceResponse.result.items.Concat(tmpremoteinvoiceResponse.result.items));
                    }
                    Debug.WriteLine("remoteinvoiceResponse Concat end : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    return;
                }
                else
                {
                    if (priority != Priority.Background && tmpremoteinvoiceResponse != null && tmpremoteinvoiceResponse.error != null && tmpremoteinvoiceResponse.error.message != null)
                    {
                        App.Instance.Hud.DisplayToast(tmpremoteinvoiceResponse.error.message, Colors.Red, Colors.White);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<ListResponseModel<InvoiceDto>> GetRemoteInvoiceAndUpdateInLocal(Priority priority, GetInvoiceInput invoiceFilter)
        {

            try
            {
                ListResponseModel<InvoiceDto> tmpremoteinvoiceResponse = new ListResponseModel<InvoiceDto>();
                Task<ListResponseModel<InvoiceDto>> invoiceTask;
                Debug.WriteLine("GetRemoteInvoiceAndUpdateInLocal request : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceFilter).ToJson());

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        invoiceTask = _apiService.Background.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        invoiceTask = _apiService.UserInitiated.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        invoiceTask = _apiService.Speculative.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                    default:
                        invoiceTask = _apiService.UserInitiated.GetSalesInvoices(invoiceFilter, Settings.AccessToken);
                        break;
                }

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        tmpremoteinvoiceResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds * 2))
                            .ExecuteAsync(async () => await invoiceTask);


                        Debug.WriteLine("GetRemoteInvoiceAndStoreLocal response : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        tmpremoteinvoiceResponse = await ex.GetContentAsAsync<ListResponseModel<InvoiceDto>>();

                        if (tmpremoteinvoiceResponse != null && tmpremoteinvoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting invoice and storing locally.", ex);
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

                if (tmpremoteinvoiceResponse != null && tmpremoteinvoiceResponse.success && tmpremoteinvoiceResponse.result != null && tmpremoteinvoiceResponse.result.totalCount > 0)
                {
                    tmpremoteinvoiceResponse.result.items.All(c => { c.isSync = true; return true; });

                    if (invoiceFilter.outletId == Settings.SelectedOutletId)
                    {
                        _= Task.Run(() =>
                        {
                            UpdateLocalInvoices(tmpremoteinvoiceResponse.result.items);
                        });
                    }
                    return tmpremoteinvoiceResponse;
                }
                else
                {
                    if (priority != Priority.Background && tmpremoteinvoiceResponse != null && tmpremoteinvoiceResponse.error != null && tmpremoteinvoiceResponse.error.message != null)
                    {
                        App.Instance.Hud.DisplayToast(tmpremoteinvoiceResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<ObservableCollection<InvoiceHistoryDto>> GetRemoteInvoiceHistories(Priority priority, bool syncLocal, int InvoiceId)
        {
            ListResponseModel<InvoiceHistoryDto> invoiceHistoryResponse = null;

            Task<ListResponseModel<InvoiceHistoryDto>> invoiceHistoryTask;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        invoiceHistoryTask = _apiService.Background.GetHistories(InvoiceId, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        invoiceHistoryTask = _apiService.UserInitiated.GetHistories(InvoiceId, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        invoiceHistoryTask = _apiService.Speculative.GetHistories(InvoiceId, Settings.AccessToken);
                        break;
                    default:
                        invoiceHistoryTask = _apiService.UserInitiated.GetHistories(InvoiceId, Settings.AccessToken);
                        break;
                }

                try
                {
                    invoiceHistoryResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .ExecuteAsync(async () => await invoiceHistoryTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    invoiceHistoryResponse = await ex.GetContentAsAsync<ListResponseModel<InvoiceHistoryDto>>();

                    if (invoiceHistoryResponse != null && invoiceHistoryResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting invoice and storing locally.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                //await Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not connected", "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return null;
            }

            if (invoiceHistoryResponse.success)
            {
                //if (syncLocal && invoiceHistoryResponse.result != null && invoiceHistoryResponse.result.items.Count > 0)
                //{
                //	await UpdateLocalInvoiceHistories(outletResponse.result.items);
                //}
                return invoiceHistoryResponse.result.items;
            }
            else
            {
                if (priority != Priority.Background && invoiceHistoryResponse != null && invoiceHistoryResponse.error != null && invoiceHistoryResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(invoiceHistoryResponse.error.message, Colors.Red, Colors.White);
                }
                return null;
            }

        }

        public async Task<InvoiceDto> UpdateRemoteInvoice(Priority priority, bool syncLocal, InvoiceDto invoice, bool isOfflineCreated = false)
        {
            try
            {
                //Ticket start:#86610 Stock Discrepancy.by rupesh
                //Start #92570 By Pratik
                if (((invoice.Status == InvoiceStatus.Exchange || invoice.Status == InvoiceStatus.Refunded) && invoice.Id != 0)
                || (string.IsNullOrEmpty(invoice.Number) && (invoice.InvoicePayments == null || invoice.InvoicePayments.Count == 0) && invoice.Status == InvoiceStatus.Completed))
                {
                    invoice.isSync = true;
                    await UpdateLocalInvoice(invoice);
                    return invoice;
                }
                //End #92570 By Pratik
                //Ticket end:#86610 .by rupesh


                Debug.WriteLine("step 1");
                ResponseModel<InvoiceDto> invoiceResponse = null;


                Task<ResponseModel<InvoiceDto>> invoiceTask;
                Debug.WriteLine("Invoice request : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoice).ToString());
                invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                var deviceInfo = Extensions.GetDeviceInfo();

            Retry2:

                switch (priority)
                {
                    case Priority.Background:
                        invoiceTask = _apiService.Background.CreateorUpdateInvoice(invoice, Settings.AccessToken, deviceInfo);
                        break;
                    case Priority.UserInitiated:
                        invoiceTask = _apiService.UserInitiated.CreateorUpdateInvoice(invoice, Settings.AccessToken, deviceInfo);
                        break;
                    case Priority.Speculative:
                        invoiceTask = _apiService.Speculative.CreateorUpdateInvoice(invoice, Settings.AccessToken, deviceInfo);
                        break;
                    default:
                        invoiceTask = _apiService.UserInitiated.CreateorUpdateInvoice(invoice, Settings.AccessToken, deviceInfo);
                        break;
                }


                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        Debug.WriteLine("step 2");                        
                        invoiceResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await invoiceTask);


                        Debug.WriteLine("invoiceResponse : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceResponse).ToString());
                        //   Task.Delay(TimeSpan.FromSeconds(5)).Wait();

                    }
                    catch (ApiException ex)
                    {
                        SetLoginTxt("UpdateRemoteInvoice", invoice, ex);
                        Debug.WriteLine("step 3");
                        invoice.isSync = false;
                        invoiceResponse = await ex.GetContentAsAsync<ResponseModel<InvoiceDto>>();

                        Debug.WriteLine("ApiException invoice response : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceResponse).ToString());

                        if (invoiceResponse != null && invoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            SendLogsToServer(ex, "UnAuthorizedRequest", invoice);

                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                Debug.WriteLine("step 4");
                                Debug.WriteLine("ApiException invoice response  Retry2: ");
                                goto Retry2;
                            }
                        }
                        var title = "";

                        if (ex != null)
                        {
                            Debug.WriteLine("step 5");
                            title = ex.Message;
                        }

                        if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                        {
                            title = invoiceResponse.error.message;
                        }
                        Debug.WriteLine("step 6");
                        invoice.HasError = true;

                        SendLogsToServer(ex, title, invoice);

                        LogErrors(ex, title, "sale", "CreateorUpdateInvoice", invoice);
                        Debug.WriteLine("step 7");
                        if (priority == Priority.Background)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                            });
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetLoginTxt("UpdateAssociateCustomerRemoteInvoice", invoice, ex);
                        Debug.WriteLine("step 8");
                        invoice.isSync = false;
                        // Debug.WriteLine("Exception invoice response : " + Newtonsoft.Json.JsonConvert.SerializeObject(ex).ToString());
                        Debug.WriteLine("Exception invoice response : " + ex.Message);

                        ex.Track();
                        if (priority != Priority.Background)
                        {
                            Debug.WriteLine("step 9");
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

                        var title = "";
                        if (ex != null)
                        {
                            Debug.WriteLine("step 10");
                            title = ex.Message;
                        }

                        if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                        {
                            title = invoiceResponse.error.message;
                        }

                        invoice.HasError = true;
                        SendLogsToServer(ex, title, invoice);
                        LogErrors(ex, title, "sale", "CreateorUpdateInvoice", invoice);
                        Debug.WriteLine("step 11");
                        if (priority == Priority.Background)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                            });
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        }
                    }
                }
                else
                {
                    invoice.isSync = false;
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }


                Debug.WriteLine("step 12");
                if (invoiceResponse != null && invoiceResponse.success && invoiceResponse.result != null)
                {
                    invoice.LocalInvoiceStatus = LocalInvoiceStatus.Completed;
                    if (syncLocal)
                    {
                        Debug.WriteLine("step 13");
                        invoiceResponse.result.isSync = true;

                        if (!invoice.isSync && !string.IsNullOrEmpty(invoice.InvoiceTempId))
                        {
                            try
                            {
                                //await BlobCache.LocalMachine.Invalidate(invoice.InvoiceTempId);
                                invoiceResponse.result.InvoiceTempId = invoice.InvoiceTempId;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("step 14");
                                ex.Track();
                            }
                        }

                        //#29657 Seach customer using phone number
                        // Note : we are not getting customer detail from the api response so we are adding from the request. Which would help
                        // while searching in sale history
                        if (invoice.CustomerDetail != null)
                        {
                            invoiceResponse.result.CustomerDetail = invoice.CustomerDetail;
                        }
                        //#29657 Seach customer using phone number

                        await UpdateLocalInvoice(invoiceResponse.result);
                        Debug.WriteLine("step 15");
                        // Note : Below code is added to resolve Zendesk ticket : 6571


                        if (invoiceResponse.result.CustomerId > 0)
                        {
                            if (customerService == null)
                            {
                                customerService = new CustomerServices(new ApiService<ICustomerApi>());
                            }
                            Debug.WriteLine("step 16");
                            //Ticket start:#42068 iPad: multiple entry updated on sales history.by rupesh
                            _ = Task.Run(async () =>
                             {
                                 //Ticket start:#22068 iOS - Sales Not Syncing for Offline On-account Sales.by rupesh
                                 var customer = customerService.GetLocalCustomerById(invoiceResponse.result.CustomerId.Value);
                                 if (customer != null && !customer.IsSync)
                                 {
                                     await customerService.UpdateRemoteCustomer(Priority.Background, true, customer);
                                 }
                                 //Start #85382 iOS: Loyalty point print calculation issue By Pratik
                                 else if (customer != null && customer.Id == invoiceResponse.result.CustomerId && !isOfflineCreated && customer.AllowLoyalty)
                                 {
                                     decimal LoyaltyRedeemed = 0;
                                     if (invoiceResponse.result.InvoicePayments != null && invoiceResponse.result.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty))
                                     {
                                         LoyaltyRedeemed = invoiceResponse.result.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty).Sum(x => x.Amount) * Settings.StoreGeneralRule.LoyaltyPointsValue;
                                     }
                                     customer.CurrentLoyaltyBalance = invoiceResponse.result.CustomerCurrentLoyaltyPoints + invoiceResponse.result.LoyaltyPoints - LoyaltyRedeemed;
                                     customerService.UpdateLocalCustomer(customer);
                                 }
                                 //End #85382
                                 //Ticket end:#22068 .by rupesh

                                 // await customerService.GetRemoteCustomers(Priority.Background, true);

                                 //Ticket #12318 Start : Able to process on account sale more than account limit assigned to the customer. By Nikhil
                                 //Invoice customer outstading balance got from server because its not coming in new optimized API.
                                 //   int invoiceCustomerId = invoiceResponse.result.CustomerId.Value;
                                 //   await customerService.GetRemoteCustomerDetail(Priority.Background, invoiceCustomerId);
                                 //Ticket #12318 End. By Nikhil 
                             });
                            //Ticket end:#42068 .by rupesh
                            Debug.WriteLine("step 17");
                        }

                    }
                    Debug.WriteLine("step 18");
                    //Ticket start:#41496 Duplicate sales issues for some customers.by rupesh
                    if (!isOfflineCreated)
                        await PushAllUnsyncInvoiceOnRemote(true, false, false, true);
                    //Ticket end:#41496 .by rupesh
                    Debug.WriteLine("step 19");

                    if (invoiceResponse.result.Id != 0)
                    {
                        //Ticket start:#42068 iPad: multiple entry updated on sales history.by rupesh
                        invoice.InvoiceHistories = invoiceResponse.result.InvoiceHistories;
                        //Ticket end:#42068 .by rupesh
                        //    Debug.WriteLine("from respomse invoice : " + Newtonsoft.Json.JsonConvert.SerializeObject(newInvoice).ToString());
                        WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(invoiceResponse.result));
                    }

                    return invoiceResponse.result;

                }
                else
                {
                    if (priority != Priority.Background && invoiceResponse != null && invoiceResponse.error != null && invoiceResponse.error.message != null)
                    {
                        App.Instance.Hud.DisplayToast(invoiceResponse.error.message, Colors.Red, Colors.White);
                    }

                    if (syncLocal)
                    {
                        Debug.WriteLine("step 19");
                        invoice.isSync = false;
                        await UpdateLocalInvoice(invoice);
                        Debug.WriteLine("step 20");
                    }
                    return invoice;
                }
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("UpdateRemoteInvoice invoice" + invoice?.Number + " Exception Msg - " + ex.Message);
                Debug.WriteLine("step 21" + ex.Message);
                return null;
            }
        }

        public async Task<InvoiceDto> UpdateAssociateCustomerRemoteInvoice(Priority priority, bool syncLocal, InvoiceDto invoice)
        {
            //var data = JsonConvert.SerializeObject(invoice);
            ResponseModel<InvoiceDto> invoiceResponse = null;
            Task<ResponseModel<InvoiceDto>> invoiceTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    invoiceTask = _apiService.Background.AssociateCustomerToInvoice(invoice, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    invoiceTask = _apiService.UserInitiated.AssociateCustomerToInvoice(invoice, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    invoiceTask = _apiService.Speculative.AssociateCustomerToInvoice(invoice, Settings.AccessToken);
                    break;
                default:
                    invoiceTask = _apiService.UserInitiated.AssociateCustomerToInvoice(invoice, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    invoiceResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await invoiceTask);


                }
                catch (ApiException ex)
                {
                    SetLoginTxt("UpdateAssociateCustomerRemoteInvoice", invoice, ex);
                    invoiceResponse = await ex.GetContentAsAsync<ResponseModel<InvoiceDto>>();
                    if (invoiceResponse != null && invoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                    var title = "";

                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                    {
                        title = invoiceResponse.error.message;
                    }

                    invoice.HasError = true;
                    LogErrors(ex, title, "sale", "CreateorUpdateInvoice", invoice);

                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                    }
                }
                catch (Exception ex)
                {
                    SetLoginTxt("UpdateAssociateCustomerRemoteInvoice", invoice, ex);
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

                    var title = "";
                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                    {
                        title = invoiceResponse.error.message;
                    }

                    invoice.HasError = true;
                    LogErrors(ex, title, "sale", "CreateorUpdateInvoice", invoice);

                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }



            if (invoiceResponse != null && invoiceResponse.success && invoiceResponse.result != null)
            {

                if (syncLocal)
                {
                    invoiceResponse.result.IsCustomerChange = false;
                    invoiceResponse.result.isSync = true;

                    if (!string.IsNullOrEmpty(invoice.InvoiceTempId))
                    {
                        try
                        {
                            invoiceResponse.result.InvoiceTempId = invoice.InvoiceTempId;
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                    await UpdateLocalInvoice(invoiceResponse.result);
                }
                if (invoiceResponse.result.Id != 0)
                {
                    //Ticket start:#42068 iPad: multiple entry updated on sales history.by rupesh
                    invoice.InvoiceHistories = invoiceResponse.result.InvoiceHistories;
                    //Ticket end:#42068 .by rupesh
                    //    Debug.WriteLine("from respomse invoice : " + Newtonsoft.Json.JsonConvert.SerializeObject(newInvoice).ToString());
                    WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(invoiceResponse.result));
                }
                return invoiceResponse.result;
            }
            else
            {
                if (priority != Priority.Background && invoiceResponse != null && invoiceResponse.error != null && invoiceResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(invoiceResponse.error.message, Colors.Red, Colors.White);
                }

                if (syncLocal)
                {
                    invoice.IsCustomerChange = true;
                    invoice.isSync = true;
                    await UpdateLocalInvoice(invoice);
                }
                return invoice;
            }
        }

        public void SaveLocalPendingInvoice(InvoiceDto tempInvoice, LocalInvoiceStatus localInvoiceStatus = LocalInvoiceStatus.Completed, string docId = "", bool islog = false)
        {
            try
            {
                docId = "LocalInvoiceDto" + docId;
                var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(tempInvoice);

              /*  LocalInvoiceDto invoice = JsonConvert.DeserializeObject<LocalInvoiceDto>(jsonString);

                if (localInvoiceStatus == LocalInvoiceStatus.Pending
                    || localInvoiceStatus == LocalInvoiceStatus.Processing)
                {
                    Debug.WriteLine("invoice.LocalInvoiceStatus : " + invoice.LocalInvoiceStatus);

                    if (invoice.isSync)
                    {
                        invoice.HasError = false;
                        RemoveErrors(invoice.Id, invoice.InvoiceTempId);
                    }

                    var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.RemoveAll<LocalInvoiceDB>();
                    });

                    var realm1 = RealmService.GetRealm();
                    realm1.Write(() =>
                    {
                        realm1.Add(invoice.ToModel(), update: true);
                    });
                }
                else
                {
                    var realm2 = RealmService.GetRealm();
                    realm2.Write(() =>
                    {
                        realm2.RemoveAll<LocalInvoiceDB>();
                    });
                }*/

                LocalInvoiceDto invoice = JsonConvert.DeserializeObject<LocalInvoiceDto>(jsonString);
                using var realm = RealmService.GetRealm();

                if (localInvoiceStatus == LocalInvoiceStatus.Pending
                    || localInvoiceStatus == LocalInvoiceStatus.Processing)
                {
                    Debug.WriteLine("invoice.LocalInvoiceStatus : " + invoice.LocalInvoiceStatus);

                    realm.Write(() =>
                    {
                        // Handle sync-related cleanup
                        if (invoice.isSync)
                        {
                            invoice.HasError = false;
                            RemoveErrors(invoice.Id, invoice.InvoiceTempId);
                        }

                        // Remove only matching invoices
                        var existingInvoices = realm.All<LocalInvoiceDB>()
                            .Where(x => x.Id == invoice.Id || x.InvoiceTempId == invoice.InvoiceTempId)
                            .ToList();

                        foreach (var existing in existingInvoices)
                            realm.Remove(existing);

                        // Add or update this invoice
                        realm.Add(invoice.ToModel(), update: true);
                    });
                }
                else
                {
                    realm.Write(() =>
                    {
                        var existingInvoices = realm.All<LocalInvoiceDB>()
                            .Where(x => x.Id == invoice.Id || x.InvoiceTempId == invoice.InvoiceTempId)
                            .ToList();

                        foreach (var existing in existingInvoices)
                            realm.Remove(existing);
                    });
                }

            }
            catch (Exception ex)
            {
                if (islog)
                    Logger.SaleLogger("Exception Msg while Save Local Pending Invoice - " + ex.StackTrace);
                Debug.WriteLine("Update local invoice1 : " + ex.ToString());
                ex.Track();

            }
        }

        public async Task<InvoiceDto> UpdateLocalInvoice(InvoiceDto invoice, LocalInvoiceStatus localInvoiceStatus = LocalInvoiceStatus.Completed, bool islog = false)
        {
            try
            {
                await Task.Delay(1);
                Debug.WriteLine("InvoiceStatus123 : " + invoice.Status.ToString());
                string docId = "";

                if ((invoice.Status != InvoiceStatus.Pending) && (invoice.Status != InvoiceStatus.initial))
                {
                    invoice.LocalInvoiceStatus = LocalInvoiceStatus.Completed;
                }

                Debug.WriteLine("invoice.LocalInvoiceStatus : " + invoice.LocalInvoiceStatus);

                if (invoice.isSync)
                {
                    invoice.HasError = false;
                    RemoveErrors(invoice.Id, invoice.InvoiceTempId);
                }

                try
                {
                    Debug.WriteLine("UpdateLocalInvoice..  : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoice));
                    if (!string.IsNullOrEmpty(invoice.InvoiceTempId))
                    {
                        using (var realm = RealmService.GetRealm())
                        {
                            var data = realm.Find<InvoiceDB>(invoice.InvoiceTempId);
                            if (data != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(data);
                                });
                            }
                        }
                        ;
                    }

                    if (invoice.Id != 0)
                    {
                        using (var realm = RealmService.GetRealm())
                        {
                            var data = realm.Find<InvoiceDB>(invoice.Id.ToString());
                            if (data != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(data);
                                });
                            }
                        }
                        ;
                    }
                }
                catch (Exception e)
                {
                    if (islog)
                        Logger.SaleLogger("Exception1 Msg while Update Local Invoice - " + e?.StackTrace);
                    if (e != null)
                        Debug.WriteLine("UpdateLocalInvoice exception : " + e.ToString());
                    else
                        Debug.WriteLine("UpdateLocalInvoice exception null: ");
                }

                if (invoice.Id == 0)
                {
                    if (string.IsNullOrEmpty(invoice.InvoiceTempId))
                    {
                        docId = nameof(InvoiceDto) + "_" + Guid.NewGuid().ToString();
                    }
                    else
                    {
                        docId = invoice.InvoiceTempId;
                    }
                    invoice.InvoiceTempId = docId;
                }

                if (!invoice.isSync && Settings.CurrentRegister != null && invoice.CurrentRegister <= 0)
                {
                    invoice.CurrentRegister = Settings.CurrentRegister.Id;
                    if (Settings.CurrentRegister.Registerclosure != null && Settings.CurrentRegister.Registerclosure.Id > 0)
                    {

                        var status = invoice.InvoiceHistories.FirstOrDefault().Status;
                        if ((status != InvoiceStatus.LayBy) && (status != InvoiceStatus.Parked) && (status != InvoiceStatus.OnGoing)) //#94565
                            invoice.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;

                        if (invoice.Status == InvoiceStatus.Exchange || invoice.Status == InvoiceStatus.Refunded)
                        {
                            invoice.RegisterId = Settings.CurrentRegister.Id;
                            invoice.RegisterName = Settings.CurrentRegister.Name;
                            invoice.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;

                            foreach (var lineItem in invoice.InvoiceLineItems)
                            {
                                lineItem.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                                lineItem.RegisterId = Settings.CurrentRegister.Id;
                                lineItem.RegisterName = Settings.CurrentRegister.Name;
                            }

                        }
                    }
                }

                if (invoice.OutletId == 0)
                {
                    invoice.OutletId = 1;
                    var outlet = Settings.CurrentUser.Outlets.Where(x => x.OutletId == 1).FirstOrDefault();
                    App.Instance.Hud.DisplayToast("This sale will be added to outlet:" + outlet.OutletName, Colors.Green, Colors.White);
                }

                Debug.WriteLine("Local invoice123 : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoice));

                using (var realm1 = RealmService.GetRealm())
                {
                    realm1.Write(() =>
                    {
                        realm1.Add(invoice.ToModel(), update: true);
                    });
                }
                ;

                SaveLocalPendingInvoice(invoice, invoice.LocalInvoiceStatus, docId, islog);

                return invoice;
            }
            catch (Exception ex)
            {
                ex.Track();
                if (islog)
                    Logger.SaleLogger("Exception3 Msg while Update Local Invoice - " + ex.StackTrace);
                return null;
            }
        }

        public Task<bool> UpdateLocalInvoiceThenSendItToServer(ObservableCollection<InvoiceDto> invoices)
        {

            var snapshot = invoices
            .Select(i => JsonConvert.DeserializeObject<InvoiceDto>(
                JsonConvert.SerializeObject(i)))
            .ToList();
           _ = _invoiceSyncQueue.Enqueue(async () =>
            {
                try
                {                       
                    await PushUnsyncInvoicesOnRemote(snapshot, false);
                }
                catch (Exception ex)
                {
                    Logger.SaleLogger("Invoice sync failed: " + ex);
                }
            });

            return Task.FromResult(true);
        }

        public async Task PushUnsyncInvoicesOnRemote(List<InvoiceDto> invoices, bool inBackgroundMode = true)
        {
             if (invoices == null || invoices.Count == 0)
                return;

            foreach (InvoiceDto invoicet in invoices)
            {

                //Ticket start:#22406 Quote sale.(Extra invoice history with null creation time causing issue).by rupesh
                var toRemove = invoicet.InvoiceHistories?.Where(x => x.CreationTime == null).ToList();
                foreach (var item in toRemove)
                    invoicet.InvoiceHistories?.Remove(item);
                //Ticket end:#22406 .by rupesh

                //Ticket start:#22406 Quote sale.by rupesh
                if (Settings.IsQuoteSale && invoicet.Status == InvoiceStatus.Completed)
                    invoicet.Status = InvoiceStatus.Quote;
                //Ticket end:#22406 .by rupesh

                //Ticket start:#31727 iOS - Action Reverted During Network Fluctuation.by rupesh
                var invoiceHistory = invoicet.InvoiceHistories.LastOrDefault(x=>x.Status != InvoiceStatus.EmailSent);
                if (invoiceHistory?.Status != invoicet.Status)
                {
                    var newInvoiceHistory = new InvoiceHistoryDto();
                    newInvoiceHistory.Status = invoicet.Status;
                    newInvoiceHistory.StatusName = invoicet.Status.ToString();
                    newInvoiceHistory.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                    newInvoiceHistory.ServerdBy = invoicet.ServedByName;
                    newInvoiceHistory.CreationTime = DateTime.UtcNow;
                    newInvoiceHistory.NotSynced = true;
                    invoicet.InvoiceHistories.Add(newInvoiceHistory);
                }
                //Ticket end:#31727 .by rupesh

                var invoice = await UpdateLocalInvoice(
                    invoicet,
                    LocalInvoiceStatus.Completed,
                    true);

                if (invoice == null)
                    continue;

                if (invoice != null)
                {

                    //result.isSync = false;
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet && !string.IsNullOrEmpty(Settings.AccessToken))
                    {
                        //get offline data
                        Fusillade.Priority processmode = Priority.Background;

                        if (!inBackgroundMode)
                        {
                            processmode = Priority.UserInitiated;
                        }

                        if (!string.IsNullOrEmpty(Settings.AccessToken))
                        {
                            if (invoice != null && invoice.CustomerId < 1 && !string.IsNullOrEmpty(invoice.CustomerTempId))
                            {
                                if (customerService == null)
                                {
                                    customerService = new CustomerServices(new ApiService<ICustomerApi>());
                                }
                                var lstCustomer = customerService.GetLocalCustomers();
                                if (lstCustomer == null)
                                {
                                    lstCustomer = new ObservableCollection<CustomerDto_POS>();
                                }

                                var customer = lstCustomer.FirstOrDefault(x => x.TempId == invoice.CustomerTempId);
                                if (customer != null)
                                {
                                    if (customer.Id == 0)
                                    {
                                        var customerResult = await customerService.UpdateRemoteCustomer(processmode, true, customer);
                                        if (customerResult != null && customerResult.Id != 0)
                                        {
                                            invoice.CustomerId = customerResult.Id;
                                        }
                                    }
                                    else
                                    {
                                        invoice.CustomerId = customer.Id;
                                    }
                                }
                            }
                        }
                        if (invoice.IsCustomerChange)
                            await UpdateAssociateCustomerRemoteInvoice(processmode, true, invoice);
                        else
                            await UpdateRemoteInvoice(processmode, true, invoice);

                    }
                    else if (invoice != null)
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(invoice));
                    }
                }
            }
        }

        public bool UpdateLocalInvoices(ObservableCollection<InvoiceDto> invoices)
        {
            try
            {
                var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(invoices.Select(u => u.ToModel()), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public string GetInvoiceDetailsValueExist(int Id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                if (Id > 0)
                {
                    var data = realm.All<OnAccountPONumberRequestDB>().ToList().FirstOrDefault(x => x.InvoiceId == Id);
                    if (!string.IsNullOrEmpty(data?.Value))
                    {
                        return data.Value;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        /*
         Date : 29Jan2021
         Note : Below method is used to delete invoice permenently from localdatabse 
         which has LocalInvoiceStatus = Pending.
         because those type of sales should not be show in sale history with voide status.
         */
        public bool RemoveLocalPendingInvoice(InvoiceDto invoice, bool iSDiscard)
        {

            try
            {
                if (iSDiscard)
                {
                    using (var realm = RealmService.GetRealm())
                    {
                        var data = realm.Find<InvoiceDB>(invoice.KeyId ?? invoice.InvoiceTempId);
                        if (data != null)
                        {
                            realm.Write(() =>
                            {
                                realm.Remove(data);
                            });
                        }
                        else
                        { 

                        }
                    }
                    ;
                    using (var realm1 = RealmService.GetRealm())
                    {
                        realm1.Write(() =>
                        {
                            realm1.RemoveAll<LocalInvoiceDB>();
                        });
                    }
                    ;
                }
                else
                {
                    if (invoice.InvoiceLineItems.Count() <= 0)
                    {
                        using (var realm = RealmService.GetRealm())
                        {
                            var data = realm.Find<InvoiceDB>(invoice.KeyId ?? invoice.InvoiceTempId);
                            if (data != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(data);
                                });
                            }
                        }
                        ;
                        using (var realm1 = RealmService.GetRealm())
                        {
                            realm1.Write(() =>
                            {
                                realm1.RemoveAll<LocalInvoiceDB>();
                            });
                        }
                        ;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool RemovePendingInvoice(InvoiceDto invoice)
        {
            try
            {
                if (!string.IsNullOrEmpty(invoice.InvoiceTempId))
                {
                    using (var realm = RealmService.GetRealm())
                    {
                        var data = realm.Find<InvoiceDB>(invoice.KeyId ?? invoice.InvoiceTempId);
                        if (data != null)
                        {
                            realm.Write(() =>
                            {
                                realm.Remove(data);
                            });
                        }
                    }
                    ;
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public ObservableCollection<InvoiceDto> GetInvoicesWhichHasUnSyncCustomer()
        {
            var realm = RealmService.GetRealm();
            var Invoices = realm.All<InvoiceDB>().ToList().Where(x => x.CustomerId == 0 && !string.IsNullOrEmpty(x.CustomerTempId)).Select(a => InvoiceDto.FromModel(a)).ToList();
            return new ObservableCollection<InvoiceDto>(Invoices);
        }

        public ObservableCollection<InvoiceDto> GetOfflineInvoices()
        {
            var realm = RealmService.GetRealm();
            var Invoices = realm.All<InvoiceDB>().Where(x => x.Status != -1
            && x.isSync == false).ToList().Select(a => InvoiceDto.FromModel(a)).ToList();

            var result = new ObservableCollection<InvoiceDto>(Invoices);
            return result;
        }

        public ObservableCollection<SaleInvoiceEmailInput> GetLocalInvoiceEmails()
        {
            try
            {
                var realm = RealmService.GetRealm();
                var saleInvoiceEmailInputs = realm.All<SaleInvoiceEmailInputDB>().ToList().Select(a => SaleInvoiceEmailInput.FromModel(a)).ToList();
                return new ObservableCollection<SaleInvoiceEmailInput>(saleInvoiceEmailInputs);
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public void SendInvoiceEmails(ObservableCollection<SaleInvoiceEmailInput> invoiceEmails)
        {
            try
            {
                var tasks = new List<Task>();
                foreach (var item in invoiceEmails)
                {
                    Task tmptask = SendInvoiceEmail(item);
                    tasks.Add(tmptask);
                }
                Task.WhenAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task<string> SendInvoiceEmail(SaleInvoiceEmailInput saleInvoiceEmailInput)
        {
            var result = "";
            try
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
						httpClient.Timeout = new TimeSpan(0, 10, 0);
                        var requestfrom = DeviceInfo.Platform == DevicePlatform.iOS ? (DeviceInfo.Idiom == DeviceIdiom.Phone ? "iPhone" : "iPad") : (DeviceInfo.Idiom == DeviceIdiom.Phone ? "AndroidPhone" : "AndroidTab");
                        httpClient.DefaultRequestHeaders.Add("RequestFrom", requestfrom + " " + CommonMethods.GetDeviceIdentifierWithVersion());
                        var data = JsonConvert.SerializeObject(saleInvoiceEmailInput);
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


                        var fullurl = string.Format("{0}/api/services/app/sale/SendInvoiceToCustomer", url);
                        var response = await httpClient.PostAsync(fullurl, content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var EmailResponse = await Task.Run(() => JsonConvert.DeserializeObject<ResponseModel<object>>(json)).ConfigureAwait(false);
                                if (EmailResponse != null && EmailResponse.success)
                                {
                                    //if (!string.IsNullOrEmpty(saleInvoiceEmailInput.Id))
                                    //{
                                    //	await DeleteLocalInvoiceEmail(saleInvoiceEmailInput.Id);
                                    //}
                                    result = LanguageExtension.Localize("EmailSuccess");
                                }
                                else
                                {
                                    if (EmailResponse != null && EmailResponse.error != null)
                                    {
                                        result = EmailResponse.error.message;
                                    }
                                    else
                                    {
                                        result = LanguageExtension.Localize("EmailFailed");
                                    }
                                }
                            }
                            else
                            {
                                result = LanguageExtension.Localize("EmailFailed");
                            }
                        }
                        else
                        {
                            result = LanguageExtension.Localize("EmailFailed");
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
                            result = LanguageExtension.Localize("NoInternetMessage");
                        }
                    }
                    else
                    {
                        //Need to log this error to backend
                        ex.Track();
                        result = LanguageExtension.Localize("SomethingWrong");
                    }

                }
            }
            catch (Exception ex)
            {
                ex.Track();
                result = LanguageExtension.Localize("SomethingWrong");
            }
            return result;
        }

        public void LogErrors(Exception exception, string title, string serviceName, string methodName, InvoiceDto invoice)
        {
            try
            {
                string customdata = JsonConvert.SerializeObject(invoice);
                AuditInfo auditInfo = new AuditInfo()
                {
                    TenantId = Settings.TenantId,
                    UserId = Settings.CurrentUser.Id,
                    ImpersonatorUserId = 0,
                    ImpersonatorTenantId = 0,
                    ServiceName = serviceName,
                    MethodName = methodName,
                    Parameters = exception.Message,
                    ExecutionTime = DateTime.UtcNow,
                    ExecutionDuration = 0,
                    ClientIpAddress = "",
                    ClientName = "",
                    BrowserInfo = DeviceInfo.Platform == DevicePlatform.iOS ? (DeviceInfo.Idiom == DeviceIdiom.Phone ? "iPhone" : "iPad") : (DeviceInfo.Idiom == DeviceIdiom.Phone ? "AndroidPhone" : "AndroidTab"),
                    CustomData = customdata,
                    Exception = exception,
                    Title = title
                };

                List<HikeAuditLog> loglist = null;

                loglist = GetErrors(invoice.Id, invoice.InvoiceTempId);

                if (loglist == null || loglist.Count < 1)
                {
                    loglist = new List<HikeAuditLog>();

                    if (invoice.Id > 0 && string.IsNullOrEmpty(invoice.InvoiceTempId))
                    {
                        auditInfo.Id = "Invoice_Error_ID" + invoice.Id;
                    }
                    else
                    {
                        auditInfo.Id = "Invoice_Error_TempID" + invoice.InvoiceTempId;
                    }
                }
                else
                {
                    auditInfo.Id = loglist.First().Id;
                }

                var log = HikeAuditLog.CreateFromAuditInfo(auditInfo);
                log.InvoiceTempId = auditInfo.Id;
                try
                {
                    loglist.Add(log);
                    //await BlobCache.LocalMachine.InsertObject<List<HikeAuditLog>>(log.Id, loglist, DateTimeOffset.Now.AddYears(2));
                    var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(loglist, update: true);
                    });


                }
                catch (KeyNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception exq)
                {
                    exq.Track();
                }
            }
            catch (Exception ecx)
            {
                ecx.Track();
            }
        }

        public List<HikeAuditLog> GetErrors(int invoiceId = 0, string invoiceTempId = null)
        {
            List<HikeAuditLog> logs = new List<HikeAuditLog>();
            var realm = RealmService.GetRealm();
            try
            {
                if (invoiceId > 0)
                {
                    var auditInfoId = "Invoice_Error_ID" + invoiceId;
                    if (!string.IsNullOrEmpty(auditInfoId))
                    {
                        try
                        {
                            //var tmplogs = await CommonQueries.GetObject<List<HikeAuditLog>>(auditInfoId);
                            var tmplogs = realm.All<HikeAuditLog>().Where(x => x.InvoiceTempId == auditInfoId).ToList();
                            if (tmplogs != null && tmplogs.Count > 0)
                            {
                                logs.AddRange(tmplogs);
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                        }
                        catch (Exception exq)
                        {
                            exq.Track();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(invoiceTempId))
                {
                    var auditInfoId = "Invoice_Error_TempID" + invoiceTempId;
                    if (!string.IsNullOrEmpty(auditInfoId))
                    {
                        try
                        {
                            //var tmplogs = await CommonQueries.GetObject<List<HikeAuditLog>>(auditInfoId);
                            var tmplogs = realm.All<HikeAuditLog>().Where(x => x.InvoiceTempId == auditInfoId).ToList();
                            if (tmplogs != null && tmplogs.Count > 0)
                            {
                                logs.AddRange(tmplogs);
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        catch (Exception exq)
                        {
                            exq.Track();
                        }
                    }
                }
            }
            catch (Exception ecx)
            {
                ecx.Track();
            }
            return logs;
        }
        public void RemoveErrors(int invoiceId = 0, string invoiceTempId = null)
        {
            try
            {
                if (invoiceId > 0 && string.IsNullOrEmpty(invoiceTempId))
                {
                    string auditInfoId = "Invoice_Error_ID" + invoiceId;
                    if (!string.IsNullOrEmpty(auditInfoId))
                    {
                        try
                        {
                            var realm = RealmService.GetRealm();
                            var auditLog = realm.All<HikeAuditLog>().Where(x => x.InvoiceTempId == auditInfoId).LastOrDefault();
                            if (auditLog != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(auditLog);
                                });
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        catch (Exception exq)
                        {
                            exq.Track();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(invoiceTempId))
                {
                    string auditInfoId = "Invoice_Error_TempID" + invoiceTempId;
                    if (!string.IsNullOrEmpty(auditInfoId))
                    {
                        try
                        {
                            var realm = RealmService.GetRealm();
                            var auditLog = realm.All<HikeAuditLog>().Where(x => x.InvoiceTempId == auditInfoId).LastOrDefault();
                            if (auditLog != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(auditLog);
                                });
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        catch (Exception exq)
                        {
                            exq.Track();
                        }
                    }
                }
            }
            catch (Exception ecx)
            {
                ecx.Track();
            }
        }

        public async Task<bool> AddCashDrawerLog(Priority priority, bool syncLocal, CashDrawerLogInput cashDrawerLogInput)
        {
            ResponseModel<object> CashDrawerLogResponse = null;
            Task<ResponseModel<object>> CashDrawerLogTask;

        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    CashDrawerLogTask = _apiService.Background.AddCashDrawerLog(cashDrawerLogInput, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    CashDrawerLogTask = _apiService.UserInitiated.AddCashDrawerLog(cashDrawerLogInput, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    CashDrawerLogTask = _apiService.Speculative.AddCashDrawerLog(cashDrawerLogInput, Settings.AccessToken);
                    break;
                default:
                    CashDrawerLogTask = _apiService.UserInitiated.AddCashDrawerLog(cashDrawerLogInput, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    CashDrawerLogResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await CashDrawerLogTask);

                }
                catch (ApiException ex)
                {
                    //invoice.isSync = false;
                    CashDrawerLogResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                    if (CashDrawerLogResponse != null && CashDrawerLogResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                    //invoice.isSync = false;
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            }
                        }
                        else
                        {
                            //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                    }
                }
            }
            else
            {
                cashDrawerLogInput.isSync = false;
                //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }



            if (CashDrawerLogResponse != null && CashDrawerLogResponse.success && CashDrawerLogResponse.result != null)
            {
                if (syncLocal)
                {
                    cashDrawerLogInput.isSync = true;
                    //await UpdateSingleLocalHearAbout(CashDrawerLogResponse.result);
                }
                return true;
            }
            else
            {
                if (priority != Priority.Background && CashDrawerLogResponse != null && CashDrawerLogResponse.error != null && CashDrawerLogResponse.error.message != null)
                {
                    //App.Instance.Hud.DisplayToast(CashDrawerLogResponse.error.message, Colors.Red, Colors.White);
                }

                //if (syncLocal)
                //{
                //  await UpdateLocalInvoice(invoice);
                //}
                //return invoice;

                return false;
            }
        }

        public async Task<InvoiceDto> UpdateInvoiceNote(Priority priority, bool syncLocal, InvoiceDto invoice)
        {
            //var data = JsonConvert.SerializeObject(invoice);
            ResponseModel<InvoiceDto> invoiceResponse = null;
            Task<ResponseModel<InvoiceDto>> invoiceTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    invoiceTask = _apiService.Background.UpdateInvoiceNote(invoice, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    invoiceTask = _apiService.UserInitiated.UpdateInvoiceNote(invoice, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    invoiceTask = _apiService.Speculative.UpdateInvoiceNote(invoice, Settings.AccessToken);
                    break;
                default:
                    invoiceTask = _apiService.UserInitiated.UpdateInvoiceNote(invoice, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    invoiceResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await invoiceTask);


                }
                catch (ApiException ex)
                {
                    invoiceResponse = await ex.GetContentAsAsync<ResponseModel<InvoiceDto>>();
                    if (invoiceResponse != null && invoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                    var title = "";

                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                    {
                        title = invoiceResponse.error.message;
                    }

                    invoice.HasError = true;
                    LogErrors(ex, title, "sale", "CreateorUpdateInvoice", invoice);
                    SetLoginTxt("UpdateInvoiceNote", invoice, ex);
                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
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

                    var title = "";
                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                    {
                        title = invoiceResponse.error.message;
                    }

                    invoice.HasError = true;
                    LogErrors(ex, title, "sale", "CreateorUpdateInvoice", invoice);
                    SetLoginTxt("UpdateInvoiceNote", invoice, ex);
                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }



            if (invoiceResponse != null && invoiceResponse.success && invoiceResponse.result != null)
            {
                if (syncLocal)
                {
                    invoiceResponse.result.isSync = true;

                    if (!string.IsNullOrEmpty(invoice.InvoiceTempId))
                    {
                        try
                        {
                            invoiceResponse.result.InvoiceTempId = invoice.InvoiceTempId;
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }

                    await UpdateLocalInvoice(invoiceResponse.result);
                }
                //Ticket start:#28252 decimal value stocks are updated in completed sale, when i added notes in the sales history.by rupesh
                invoiceResponse.result.InvoiceLineItems = invoice.InvoiceLineItems;
                //Ticket end:#28252 .by rupesh
                return invoiceResponse.result;
            }
            else
            {
                if (priority != Priority.Background && invoiceResponse != null && invoiceResponse.error != null && invoiceResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(invoiceResponse.error.message, Colors.Red, Colors.White);
                }

                if (syncLocal)
                {
                    invoice.isSync = true;

                    await UpdateLocalInvoice(invoice);
                }
                return invoice;
            }
        }

        /// <summary>
        /// Removes the local invoice.
        /// When customer remove all line item from the pedning invoice, we will also remove
        /// entire sale from the local database.
        /// </summary>
        /// <returns>The local invoice.</returns>
        /// <param name="invoice">Invoice.</param>
        public bool RemovePendingLocalInvoice(InvoiceDto invoice)
        {
            try
            {
                if (!string.IsNullOrEmpty(invoice.InvoiceTempId))
                {
                    var realm = RealmService.GetRealm();
                    var data = realm.Find<InvoiceDB>(invoice.KeyId ?? invoice.InvoiceTempId);
                    if (data != null)
                    {
                        realm.Write(() =>
                        {
                            realm.Remove(data);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                e.Track();
                Debug.WriteLine("Error while delete pending invoice : " + e.ToString());
            }
            return true;
        }

        public async Task<bool> HasInStock(Priority priority, int InvoiceId)
        {

            Task<ResponseModel<bool>> hasInStockTask;
            ResponseModel<bool> hasInStockResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        hasInStockTask = _apiService.Background.HasInStock(InvoiceId, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        hasInStockTask = _apiService.UserInitiated.HasInStock(InvoiceId, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        hasInStockTask = _apiService.Speculative.HasInStock(InvoiceId, Settings.AccessToken);
                        break;
                    default:
                        hasInStockTask = _apiService.UserInitiated.HasInStock(InvoiceId, Settings.AccessToken);
                        break;
                }

                try
                {
                    hasInStockResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .ExecuteAsync(async () => await hasInStockTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    hasInStockResponse = await ex.GetContentAsAsync<ResponseModel<bool>>();

                    if (hasInStockResponse != null && hasInStockResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Checking hasInStock", ex);
                        }
                    }
                    ;
                    return false;
                }
            }
            else
            {
                //await Application.Current.MainPage.DisplayAlert("Internet Connectivity", "Sorry, Internet is not connected", "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return false;
            }

            if (hasInStockResponse != null)
                return hasInStockResponse.result;

            else
            {
                if (priority != Priority.Background && hasInStockResponse != null && hasInStockResponse.error != null && hasInStockResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hasInStockResponse.error.message, Colors.Red, Colors.White);
                }
                return false;
            }

        }

        //Ticket #11319 Start : Send logs to server.By Nikhil
        void SendLogsToServer(Exception ex, string errorMsg, InvoiceDto invoice)
        {
            try
            {
                logRequestDetails.Clear();
                logRequestDetails.Add("empty", "empty");
                var error = errorMsg;
                if (ex != null) error += "\n" + JsonConvert.SerializeObject(ex);
                Extensions.SendLogsToServer(logService, invoice, logRequestDetails, error);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in SendLogsToServer : " + e.Message + " : " + e.StackTrace);
            }
        }
        //Ticket #11319 End. By Nikhil

        public async Task PushAllUnsyncInvoiceOnRemote(bool inBackgroundMode = true, bool RequiredAllData = false, bool ResetAfterUppdate = false, bool onlyUpload = false)
        {
            Debug.WriteLine("PushAllUnsyncInvoiceOnRemote 1");
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet && !string.IsNullOrEmpty(Settings.AccessToken))
                {
                    Fusillade.Priority processmode = Priority.Background;

                    if (!string.IsNullOrEmpty(Settings.AccessToken))
                    {
                        var lstInvoices = GetOfflineInvoices();
                        if (lstInvoices != null)
                        {
                            var lstMainInvoices = lstInvoices.Where(x => x.ReferenceTempInvoiceId == null && x.ReferenceInvoiceId == null);
                            var lstSubInvoices = lstInvoices.Where(x => x.ReferenceTempInvoiceId != null || x.ReferenceInvoiceId != null);

                            //save offline data 
                            foreach (InvoiceDto item in lstMainInvoices)
                            {

                                //#94565
                                if (Settings.IsRestaurantPOS && !string.IsNullOrEmpty(item.Number))
                                {
                                    var data = GetLocalInvoicesListByInvoieNumber(item.Number);
                                    if (data != null && data.Count > 1)
                                    {
                                        var fdata = data.FirstOrDefault(a => a.Id == 0);
                                        if (fdata != null)
                                        {
                                            RemovePendingInvoice(fdata);
                                            continue;
                                        }
                                    }
                                }
                                //#94565

                                if ((item.CustomerId == null || item.CustomerId.Value == 0) && !string.IsNullOrEmpty(item.CustomerTempId))
                                {
                                    //Ticket start:#22068 iOS - Sales Not Syncing for Offline On-account Sales.by rupesh
                                    if (customerService == null)
                                    {
                                        customerService = new CustomerServices(new ApiService<ICustomerApi>());
                                    }
                                    //Ticket end:#22068 .by rupesh

                                    var customer = customerService.GetLocalCustomerByTempId(item.CustomerTempId);
                                    if (customer != null)
                                    {
                                        if (customer.Id < 1)
                                        {
                                            var customerResult = await customerService.UpdateRemoteCustomer(processmode, true, customer);
                                            if (customerResult != null && customerResult.Id != 0)
                                            {
                                                item.CustomerId = customerResult.Id;
                                            }
                                        }
                                        else
                                        {
                                            item.CustomerId = customer.Id;
                                        }
                                    }
                                }

                                //Start #92570 By Pratik
                                if (((item.Status == InvoiceStatus.initial || item.Status == InvoiceStatus.Pending) && item.InvoiceLineItems.Count == 0)
                                || (string.IsNullOrEmpty(item.Number) && (item.InvoicePayments == null || item.InvoicePayments.Count == 0) && item.Status == InvoiceStatus.Completed))
                                {
                                     //End #92570 By Pratik
                                    if (!string.IsNullOrEmpty(item.InvoiceTempId))
                                    {
                                        var realm = RealmService.GetRealm();
                                        var data = realm.Find<InvoiceDB>(item.InvoiceTempId);
                                        if (data != null)
                                        {
                                            realm.Write(() =>
                                            {
                                                realm.Remove(data);
                                            });
                                        }
                                    }

                                    continue;
                                }

                                InvoiceDto invoice;
                                if (item.IsCustomerChange)
                                    invoice = await UpdateAssociateCustomerRemoteInvoice(processmode, true, item);
                                else
                                {
                                    await Task.Delay(500);
                                    invoice = await UpdateRemoteInvoice(processmode, true, item, true);
                                }

                                if (invoice != null && invoice.Id != 0)
                                {
                                    WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(invoice));
                                }
                                if (string.IsNullOrEmpty(invoice.InvoiceTempId))
                                {
                                    lstSubInvoices.Where(x => x.ReferenceTempInvoiceId == invoice.InvoiceTempId).All(s =>
                                    {
                                        s.ReferenceInvoiceId = invoice.Id;
                                        return true;
                                    });
                                }
                            }

                            foreach (InvoiceDto item in lstSubInvoices)
                            {
                                //#94565
                                if (Settings.IsRestaurantPOS && !string.IsNullOrEmpty(item.Number))
                                {
                                    var data = GetLocalInvoicesListByInvoieNumber(item.Number);
                                    if (data != null && data.Count > 1)
                                    {
                                        var fdata = data.FirstOrDefault(a => a.Id == 0);
                                        if (fdata != null)
                                        {
                                            RemovePendingInvoice(fdata);
                                            continue;
                                        }
                                    }
                                }
                                //#94565

                                if (customerService == null)
                                {
                                    customerService = new CustomerServices(new ApiService<ICustomerApi>());
                                }
                                var customer = customerService.GetLocalCustomerByTempId(item.CustomerTempId);
                                if (customer != null)
                                {
                                    if (customer.Id < 1)
                                    {
                                        var customerResult = await customerService.UpdateRemoteCustomer(processmode, true, customer);
                                        if (customerResult != null && customerResult.Id != 0)
                                        {
                                            item.CustomerId = customerResult.Id;
                                        }
                                    }
                                    else
                                    {
                                        item.CustomerId = customer.Id;
                                    }
                                }

                                InvoiceDto invoice;
                                if (item.IsCustomerChange)
                                    invoice = await UpdateAssociateCustomerRemoteInvoice(processmode, true, item);
                                else
                                {
                                    await Task.Delay(500);
                                    //Ticket start:#41496 Duplicate sales issues for some customers.by rupesh
                                    invoice = await UpdateRemoteInvoice(processmode, true, item, true);
                                    //Ticket end:#41496 .by rupesh
                                }

                                if (invoice != null && invoice.Id != 0)
                                {
                                    WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(invoice));
                                }
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            Debug.WriteLine("PushAllUnsyncInvoiceOnRemote 2");
        }

        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.
        public async Task<AjaxResponse> AddUpdateSerialNumberFromSaleHistory(Priority priority, bool syncLocal, SerialNumberDto serialNumberDto, InvoiceDto invoice)
        {

            ResponseModel<AjaxResponse> serialNumberResponse = null;
            Task<ResponseModel<AjaxResponse>> serialNumberTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    serialNumberTask = _apiService.Background.AddUpdateSerialNumberFromSaleHistory(serialNumberDto, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    serialNumberTask = _apiService.UserInitiated.AddUpdateSerialNumberFromSaleHistory(serialNumberDto, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    serialNumberTask = _apiService.Speculative.AddUpdateSerialNumberFromSaleHistory(serialNumberDto, Settings.AccessToken);
                    break;
                default:
                    serialNumberTask = _apiService.UserInitiated.AddUpdateSerialNumberFromSaleHistory(serialNumberDto, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    serialNumberResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await serialNumberTask);


                }
                catch (ApiException ex)
                {
                    serialNumberResponse = await ex.GetContentAsAsync<ResponseModel<AjaxResponse>>();
                    if (serialNumberResponse != null && serialNumberResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                    var title = "";

                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (serialNumberResponse != null && serialNumberResponse.error != null && !string.IsNullOrEmpty(serialNumberResponse.error.message))
                    {
                        title = serialNumberResponse.error.message;
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

                    var title = "";
                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (serialNumberResponse != null && serialNumberResponse.error != null && !string.IsNullOrEmpty(serialNumberResponse.error.message))
                    {
                        title = serialNumberResponse.error.message;
                    }

                    SetLoginTxt("AddUpdateSerialNumberFromSaleHistory", invoice, ex);

                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }



            if (serialNumberResponse != null && serialNumberResponse.success)
            {
                if (syncLocal)
                {
                    invoice.isSync = true;



                    await UpdateLocalInvoice(invoice);
                }


                return serialNumberResponse;
            }
            else
            {
                if (priority != Priority.Background && serialNumberResponse != null && serialNumberResponse.error != null && serialNumberResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(serialNumberResponse.error.message, Colors.Red, Colors.White);
                }

                if (syncLocal)
                {
                    //invoice.isSync = true;

                    //await UpdateLocalInvoice(invoice);
                }
                return serialNumberResponse.result;
            }

        }
        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.
        //Ticket start:#45653 iPad: FR - create a new customer from the payment slider.by rupesh
        public async Task<InvoiceDto> GetRemoteInvoice(Priority priority, bool syncLocal, int invoiceId)
        {
            try
            {
                ResponseModel<InvoiceDto> invoiceResponse = null;
                Task<ResponseModel<InvoiceDto>> invoiceTask;
                GetInvoiceDetailInput invoiceDetailInput = new GetInvoiceDetailInput
                {
                    id = invoiceId
                };

                Debug.WriteLine("GetRemoteInvoices request : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceDetailInput).ToJson());
                Debug.WriteLine("Invoice request : " + invoiceId);
                var deviceInfo = Extensions.GetDeviceInfo();

            Retry2:

                switch (priority)
                {
                    case Priority.Background:
                        invoiceTask = _apiService.Background.GetInvoice(invoiceDetailInput, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        invoiceTask = _apiService.UserInitiated.GetInvoice(invoiceDetailInput, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        invoiceTask = _apiService.Speculative.GetInvoice(invoiceDetailInput, Settings.AccessToken);
                        break;
                    default:
                        invoiceTask = _apiService.UserInitiated.GetInvoice(invoiceDetailInput, Settings.AccessToken);
                        break;
                }


                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        invoiceResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await invoiceTask);


                        Debug.WriteLine("invoiceResponse : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceResponse).ToString());
                        //   Task.Delay(TimeSpan.FromSeconds(5)).Wait();

                    }
                    catch (ApiException ex)
                    {
                        invoiceResponse = await ex.GetContentAsAsync<ResponseModel<InvoiceDto>>();

                        Debug.WriteLine("ApiException invoice response : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceResponse).ToString());

                        if (invoiceResponse != null && invoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry2;
                            }
                        }
                        SetLoginTxt("GetRemoteInvoice \n " + invoiceId, null, ex);
                        if (priority == Priority.Background)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoiceId + " with cloud.", Colors.Red, Colors.White);
                            });
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoiceId + " with cloud.", Colors.Red, Colors.White);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception invoice response : " + ex.Message);

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
                        SetLoginTxt("GetRemoteInvoice \n " + invoiceId, null, ex);
                        if (priority == Priority.Background)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoiceId + " with cloud.", Colors.Red, Colors.White);
                            });
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoiceId + " with cloud.", Colors.Red, Colors.White);
                        }
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }


                if (invoiceResponse != null && invoiceResponse.success && invoiceResponse.result != null)
                {
                    invoiceResponse.result.LocalInvoiceStatus = LocalInvoiceStatus.Completed;
                    if (syncLocal)
                    {
                        invoiceResponse.result.isSync = true;

                        await UpdateLocalInvoice(invoiceResponse.result);
                        // Note : Below code is added to resolve Zendesk ticket : 6571


                        if (invoiceResponse.result.CustomerId > 0)
                        {
                            if (customerService == null)
                            {
                                customerService = new CustomerServices(new ApiService<ICustomerApi>());
                            }
                            //Ticket start:#42068 iPad: multiple entry updated on sales history.by rupesh
                            _ = Task.Run(async () =>
                            {
                                //Ticket start:#22068 iOS - Sales Not Syncing for Offline On-account Sales.by rupesh
                                var customer = customerService.GetLocalCustomerById(invoiceResponse.result.CustomerId.Value);
                                if (customer != null && !customer.IsSync)
                                {
                                    var customerResult = await customerService.UpdateRemoteCustomer(Priority.Background, true, customer);


                                }
                                //Ticket end:#22068 .by rupesh

                                //Ticket #12318 Start : Able to process on account sale more than account limit assigned to the customer. By Nikhil
                                //Invoice customer outstading balance got from server because its not coming in new optimized API.
                                int invoiceCustomerId = invoiceResponse.result.CustomerId.Value;
                                await customerService.GetRemoteCustomerDetail(Priority.Background, invoiceCustomerId);
                                //Ticket #12318 End. By Nikhil 
                            });
                            //Ticket end:#42068 .by rupesh
                        }

                    }
                    return invoiceResponse.result;

                }
                else
                {
                    if (priority != Priority.Background && invoiceResponse != null && invoiceResponse.error != null && invoiceResponse.error.message != null)
                    {
                        App.Instance.Hud.DisplayToast(invoiceResponse.error.message, Colors.Red, Colors.White);
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
        //Ticket end:#45653 .by rupesh

        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
        public async Task<ResponseModel<AjaxResponse>> UpdateCustomerName(Priority priority, bool syncLocal, InvoiceDto invoice)
        {
            //var data = JsonConvert.SerializeObject(invoice);
            UpdateCustomerDetailInput updateCustomerDetailInput = new UpdateCustomerDetailInput
            {
                id = invoice.Id,
                name = invoice.CustomerName
            };
            ResponseModel<AjaxResponse> updateCustomerResponse = null;
            Task<ResponseModel<AjaxResponse>> updateCustomerTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    updateCustomerTask = _apiService.Background.UpdateCustomerName(updateCustomerDetailInput, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    updateCustomerTask = _apiService.UserInitiated.UpdateCustomerName(updateCustomerDetailInput, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    updateCustomerTask = _apiService.Speculative.UpdateCustomerName(updateCustomerDetailInput, Settings.AccessToken);
                    break;
                default:
                    updateCustomerTask = _apiService.UserInitiated.UpdateCustomerName(updateCustomerDetailInput, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    updateCustomerResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await updateCustomerTask);


                }
                catch (ApiException ex)
                {
                    updateCustomerResponse = await ex.GetContentAsAsync<ResponseModel<AjaxResponse>>();
                    if (updateCustomerResponse != null && updateCustomerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                    var title = "";

                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (updateCustomerResponse != null && updateCustomerResponse.error != null && !string.IsNullOrEmpty(updateCustomerResponse.error.message))
                    {
                        title = updateCustomerResponse.error.message;
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

                    var title = "";
                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (updateCustomerResponse != null && updateCustomerResponse.error != null && !string.IsNullOrEmpty(updateCustomerResponse.error.message))
                    {
                        title = updateCustomerResponse.error.message;
                    }

                    SetLoginTxt("UpdateCustomerName", invoice, ex);

                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }

            if (updateCustomerResponse != null && updateCustomerResponse.success)
            {

                await UpdateLocalInvoice(invoice);

                return updateCustomerResponse;
            }
            else
            {
                if (priority != Priority.Background && updateCustomerResponse != null && updateCustomerResponse.error != null && updateCustomerResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(updateCustomerResponse.error.message, Colors.Red, Colors.White);
                }

                return updateCustomerResponse;
            }

        }
        //Ticket end:#45648 .by rupesh

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public async Task<string> UpdateInvoiceDueDate(Priority priority, bool syncLocal, InvoiceDto Invoice, DateTime InvoiceDueDate)
        {

            Task<ResponseModel<string>> hasInStockTask;
            ResponseModel<string> hasInStockResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        hasInStockTask = _apiService.Background.UpdateInvoiceDueDate(Invoice.Id, InvoiceDueDate, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        hasInStockTask = _apiService.UserInitiated.UpdateInvoiceDueDate(Invoice.Id, InvoiceDueDate, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        hasInStockTask = _apiService.Speculative.UpdateInvoiceDueDate(Invoice.Id, InvoiceDueDate, Settings.AccessToken);
                        break;
                    default:
                        hasInStockTask = _apiService.UserInitiated.UpdateInvoiceDueDate(Invoice.Id, InvoiceDueDate, Settings.AccessToken);
                        break;
                }

                try
                {
                    hasInStockResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .ExecuteAsync(async () => await hasInStockTask);
                }
                catch (ApiException ex)
                {
                    hasInStockResponse = await ex.GetContentAsAsync<ResponseModel<string>>();
                    if (hasInStockResponse != null && hasInStockResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Update Invoice DueDate");
                        }
                    }
                    ;
                    return "";
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return "";
            }

            if (hasInStockResponse != null && hasInStockResponse.success)
            {
                if (syncLocal)
                {
                    Invoice.isSync = true;
                    Invoice.InvoiceDueDate = InvoiceDueDate;
                    await UpdateLocalInvoice(Invoice);
                }
                App.Instance.Hud.DisplayToast("Update invoice duedate successfully.", Colors.Green, Colors.White);
                return hasInStockResponse.result;
            }
            else
            {
                if (priority != Priority.Background && hasInStockResponse != null && hasInStockResponse.error != null && hasInStockResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hasInStockResponse.error.message, Colors.Red, Colors.White);
                }
                if (syncLocal)
                {
                    Invoice.isSync = true;
                    Invoice.InvoiceDueDate = InvoiceDueDate;
                    await UpdateLocalInvoice(Invoice);
                }
                return "";
            }

        }
        //END ticket #76208 by Pratik


        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public async Task<InvoiceDto> CreateOrUpdateInvoicefulfillment(Priority priority, bool syncLocal, InvoiceDto invoice, bool isOfflineCreated = false)
        {
            try
            {
                Debug.WriteLine("step 1");

                if(invoice.InvoiceHistories != null && invoice.InvoiceHistories.Any(a=>a.Id == 0))
                {
                    invoice.InvoiceHistories.Remove(invoice.InvoiceHistories.First(a => a.Id == 0));
                }
                ResponseModel<InvoiceDto> invoiceResponse = null;
                invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                var data = JsonConvert.SerializeObject(invoice);
                Debug.WriteLine("Invoice request : " + data);
            Retry2:
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        var httpService = new HttpService();
                        var httpClient = httpService.httpClient();

                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", ""));
                        httpClient.Timeout = new TimeSpan(0, 20, 0);
                        var content = new StringContent(data, Encoding.UTF8, "application/json");

                        var url = "";
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
                        var response = await httpClient.PostAsync(url + "/api/services/app/sale/CreateOrUpdateInvoicefulfillment", content).ConfigureAwait(false);

                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    JsonSerializer _serializer = new JsonSerializer();
                                    invoiceResponse = _serializer.Deserialize<ResponseModel<InvoiceDto>>(jsonSTR);
                                }
                            }
                        }
                        else if (response != null && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                Debug.WriteLine("step 4");
                                Debug.WriteLine("ApiException invoice response  Retry2: ");
                                goto Retry2;
                            }
                        }
                        else if (response != null)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    JsonSerializer _serializer = new JsonSerializer();
                                    invoiceResponse = _serializer.Deserialize<ResponseModel<InvoiceDto>>(jsonSTR);
                                }
                            }
                        }


                        Debug.WriteLine("step 2");
                        //invoiceResponse = await Policy
                        //.Handle<ApiException>()
                        //.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        //.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        //.ExecuteAsync(async () => await invoiceTask);

                        Debug.WriteLine("invoiceResponse : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceResponse).ToString());
                    }
                    catch (ApiException ex)
                    {
                        SetLoginTxt("CreateOrUpdateInvoicefulfillment", invoice, ex);
                        Debug.WriteLine("step 3");
                        invoice.isSync = false;
                        invoiceResponse = await ex.GetContentAsAsync<ResponseModel<InvoiceDto>>();

                        Debug.WriteLine("ApiException invoice response : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoiceResponse).ToString());

                        if (invoiceResponse != null && invoiceResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            SendLogsToServer(ex, "UnAuthorizedRequest - CreateOrUpdateInvoicefulfillment", invoice);

                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                Debug.WriteLine("step 4");
                                Debug.WriteLine("ApiException invoice response  Retry2: ");
                                goto Retry2;
                            }
                        }
                        var title = "";

                        if (ex != null)
                        {
                            Debug.WriteLine("step 5");
                            title = ex.Message;
                        }

                        if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                        {
                            title = invoiceResponse.error.message;
                        }
                        Debug.WriteLine("step 6");
                        invoice.HasError = true;

                        SendLogsToServer(ex, title + " - CreateOrUpdateInvoicefulfillment", invoice);

                        LogErrors(ex, title, "sale", "CreateOrUpdateInvoicefulfillment", invoice);
                        Debug.WriteLine("step 7");
                        if (priority == Priority.Background)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                            });
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetLoginTxt("CreateOrUpdateInvoicefulfillment", invoice, ex);
                        Debug.WriteLine("step 8");
                        invoice.isSync = false;
                        // Debug.WriteLine("Exception invoice response : " + Newtonsoft.Json.JsonConvert.SerializeObject(ex).ToString());
                        Debug.WriteLine("Exception invoice response : " + ex.Message);

                        ex.Track();
                        if (priority != Priority.Background)
                        {
                            Debug.WriteLine("step 9");
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

                        var title = "";
                        if (ex != null)
                        {
                            Debug.WriteLine("step 10");
                            title = ex.Message;
                        }

                        if (invoiceResponse != null && invoiceResponse.error != null && !string.IsNullOrEmpty(invoiceResponse.error.message))
                        {
                            title = invoiceResponse.error.message;
                        }

                        invoice.HasError = true;
                        SendLogsToServer(ex, title + " - CreateOrUpdateInvoicefulfillment", invoice);
                        LogErrors(ex, title, "sale", "CreateOrUpdateInvoicefulfillment", invoice);
                        Debug.WriteLine("step 11");
                        if (priority == Priority.Background)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                            });
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                        }
                    }
                }
                else
                {
                    invoice.isSync = false;
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }


                Debug.WriteLine("step 12");
                if (invoiceResponse != null && invoiceResponse.success && invoiceResponse.result != null)
                {
                    invoice.LocalInvoiceStatus = LocalInvoiceStatus.Completed;
                    if (syncLocal)
                    {
                        Debug.WriteLine("step 13");
                        invoiceResponse.result.isSync = true;

                        if (!invoice.isSync && !string.IsNullOrEmpty(invoice.InvoiceTempId))
                        {
                            try
                            {
                                //await BlobCache.LocalMachine.Invalidate(invoice.InvoiceTempId);
                                invoiceResponse.result.InvoiceTempId = invoice.InvoiceTempId;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("step 14");
                                ex.Track();
                            }
                        }

                        //#29657 Seach customer using phone number
                        // Note : we are not getting customer detail from the api response so we are adding from the request. Which would help
                        // while searching in sale history
                        if (invoice.CustomerDetail != null)
                            invoiceResponse.result.CustomerDetail = invoice.CustomerDetail;
                        //#29657 Seach customer using phone number

                        await UpdateLocalInvoice(invoiceResponse.result);
                        Debug.WriteLine("step 15");
                    }
                    Debug.WriteLine("step 18");
                    App.Instance.Hud.DisplayToast("Done! You're awesome!!", Colors.Green, Colors.White);
                    return invoiceResponse.result;

                }
                else
                {
                    if (priority != Priority.Background && invoiceResponse != null && invoiceResponse.error != null && invoiceResponse.error.message != null)
                    {
                        App.Instance.Hud.DisplayToast(invoiceResponse.error.message, Colors.Red, Colors.White);
                    }
                    else
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) when synchronizing invoice #" + invoice.Number + " with cloud.", Colors.Red, Colors.White);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("step 21" + ex.Message);
                return null;
            }
        }
        //End #84293 by Pratik

        //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
        public async Task<POSSerialNumberDto> GetPOSSerialNumber(Priority priority, POSSerialNumberRequest request)
        {
            ResponseModel<POSSerialNumberDto> pOSSerialResponse = null;
            Task<ResponseModel<POSSerialNumberDto>> pOSSerialTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    pOSSerialTask = _apiService.Background.GetPOSSerialNumber(request, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    pOSSerialTask = _apiService.UserInitiated.GetPOSSerialNumber(request, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    pOSSerialTask = _apiService.Speculative.GetPOSSerialNumber(request, Settings.AccessToken);
                    break;
                default:
                    pOSSerialTask = _apiService.UserInitiated.GetPOSSerialNumber(request, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    pOSSerialResponse = await Policy
                        .Handle<ApiException>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await pOSSerialTask);


                }
                catch (ApiException ex)
                {
                    pOSSerialResponse = await ex.GetContentAsAsync<ResponseModel<POSSerialNumberDto>>();
                    if (pOSSerialResponse != null && pOSSerialResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                    var title = "";

                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (pOSSerialResponse != null && pOSSerialResponse.error != null && !string.IsNullOrEmpty(pOSSerialResponse.error.message))
                    {
                        title = pOSSerialResponse.error.message;
                    }

                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) with cloud.", Colors.Red, Colors.White);
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

                    var title = "";
                    if (ex != null)
                    {
                        title = ex.Message;
                    }

                    if (pOSSerialResponse != null && pOSSerialResponse.error != null && !string.IsNullOrEmpty(pOSSerialResponse.error.message))
                    {
                        title = pOSSerialResponse.error.message;
                    }
                    if (priority == Priority.Background)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Instance.Hud.DisplayToast("Something went wrong (internal error) with cloud.", Colors.Red, Colors.White);
                        });
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast("Something went wrong (internal error) with cloud.", Colors.Red, Colors.White);
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }

            if (pOSSerialResponse != null && pOSSerialResponse.success && pOSSerialResponse.result != null)
            {
                return pOSSerialResponse?.result;
            }
            else
            {
                if (priority != Priority.Background && pOSSerialResponse != null && pOSSerialResponse.error != null && pOSSerialResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(pOSSerialResponse.error.message, Colors.Red, Colors.White);
                }
                return pOSSerialResponse?.result;
            }
        }
        //End #90946 by Pratik


        void SetLoginTxt(string api, InvoiceDto invoice, Exception exception)
        {
            Logger.SyncLogger("----\n" + api);
            if (invoice != null)
                Logger.SyncLogger("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(invoice).ToJson());
            Logger.SyncLogger("----\n" + exception.Message + "\n" + exception.StackTrace);
            Logger.SyncLogger("\n=======================");
        }
    }
}
