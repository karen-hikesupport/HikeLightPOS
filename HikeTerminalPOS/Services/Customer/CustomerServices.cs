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
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Diagnostics;
using HikePOS.Enums;
using HikePOS.Models.Log;
using HikePOS.Models.Customer;
using System.Globalization;

namespace HikePOS.Services
{
    public class CustomerServices
    {
        readonly IApiService<ICustomerApi> _apiService;
        readonly AccountServices accountService;

        public CustomerServices(IApiService<ICustomerApi> apiService)
        {
            _apiService = apiService;
            accountService = new AccountServices(new ApiService<IAccountApi>());
        }


        #region Customer 
        ListResponseModel<CustomerDto_POS> remoteCustomerResponse;

        ListResponseModel<CustomerAddressDto> remoteCustomerAddressResponse;
        public async Task<ObservableCollection<CustomerDto_POS>> GetRemoteCustomers(Priority priority, bool syncLocal, string filter = "", DateTime? lastSyncDate = null)
        {
            try
            {
                remoteCustomerResponse = new ListResponseModel<CustomerDto_POS>();

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<CustomerDto_POS>> customerTask;
                    DateTime CurrentTime = DateTime.Now.ToUniversalTime();

                    if (lastSyncDate == null)
                    {
                        lastSyncDate = DateTime.MinValue;
                    }

                    var maxResult = 1;

                    GetCustomerInput customerFilter = new GetCustomerInput
                    {
                        //Ticket start:#20998 feature request for IPAD (#20096).by rupesh
                        OutletId = Settings.StoreGeneralRule.EnableCustomerMultiOutlet ? Settings.SelectedOutletId:0,
                        //Ticket end:#20998.by rupesh
                        modifiedDateTime = lastSyncDate.Value,
                        maxResultCount = maxResult,
                        skipCount = 0,
                        filter = filter,
                        sorting = "0"
                    };

                Retry:
                    try
                    {

                        switch (priority)
                        {
                            case Priority.Background:
                                customerTask = _apiService.Background.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerTask = _apiService.UserInitiated.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerTask = _apiService.Speculative.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            default:
                                customerTask = _apiService.UserInitiated.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                        }
                        Debug.WriteLine("GetCustomers API : /api/services/app/customer/GetAll_For_POS_ByPaging");

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers API Request Starts at: " + startDateTime);

                        remoteCustomerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerTask);

                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetCustomers API Request Total Time : " + (endDateTime - startDateTime));

                        //await AssignCustomerGroups(remoteCustomerResponse); 

                        Debug.WriteLine("GetCustomers API Response : " + JsonConvert.SerializeObject(remoteCustomerResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content

                        remoteCustomerResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerDto_POS>>();

                        if (remoteCustomerResponse != null && remoteCustomerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customers.",ex);
                            }
                        }
                        return null;
                    }

                    if (remoteCustomerResponse != null && remoteCustomerResponse.success && remoteCustomerResponse.result != null && remoteCustomerResponse.result.totalCount > 0)
                    {
                        var totalcustomer = remoteCustomerResponse.result.totalCount;
                        int totalStep = 1;
                        maxResult = 100;

                        //Ticket #11184 Start : Large Customers reading error. By Nikhil
                        if (5000 < totalcustomer && totalcustomer < 10000)
                        {
                            maxResult = 400;
                        }
                        else if (totalcustomer > 10000)
                        {

                            maxResult = 1000;
                        }

                        if (totalcustomer > maxResult)
                        {
                            double tstep = (Convert.ToDouble(totalcustomer) / maxResult);
                            totalStep = (int)Math.Ceiling(tstep);
                        }

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers Chunks API Request Starts at: " + startDateTime);

                        int startpoint = 0;
                        do
                        {
                            var tasks = new List<Task>();
                            int endpoint = Math.Min(totalStep, startpoint + 10);// startpoint + 100;
                            for (var i = startpoint; i < endpoint; i++)
                            {
                                var skipCustomerCount = (i * maxResult);

                                if (skipCustomerCount > totalcustomer)
                                {
                                    break;
                                }
                                Debug.WriteLine("Customer Request : " + i);
                                var _customerFilter = customerFilter.Copy();
                                _customerFilter.skipCount = skipCustomerCount + 1;
                                _customerFilter.maxResultCount = maxResult;

                                Task tmptask = GetRemoteCustomerAndStoreInLocal(priority, _customerFilter);
                                tasks.Add(tmptask);
                            }
                            await Task.WhenAll(tasks.ToArray());
                            startpoint = endpoint;
                        } while (startpoint < totalStep);
                        //Ticket #11184 End : By Nikhil

                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers Chunks API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetCustomers Chunks API Request Total Time : " + (endDateTime - startDateTime));

                        if (remoteCustomerResponse != null && remoteCustomerResponse.result != null && remoteCustomerResponse.result.items != null && remoteCustomerResponse.result.items.Any())
                        {
                            //Debug.WriteLine("all customer : " + Newtonsoft.Json.JsonConvert.SerializeObject(remoteCustomerResponse));
                            if (syncLocal)
                            {
                                remoteCustomerResponse.result.items.All(c => { c.IsSync = true; return true; });
                                 UpdateLocalCustomers(remoteCustomerResponse.result.items);
                            }
                            return remoteCustomerResponse.result.items;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (priority != Priority.Background && remoteCustomerResponse != null && remoteCustomerResponse.error != null && remoteCustomerResponse.error.message != null && !string.IsNullOrEmpty(remoteCustomerResponse.error.message))
                        {
                            Extensions.ServerMessage(remoteCustomerResponse.error.message);
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
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public async Task<ObservableCollection<CustomerDto_POS>> GetRemoteCustomersLemda(Priority priority, bool syncLocal, string filter = "", DateTime? lastSyncDate = null)
        {
            try
            {
                remoteCustomerResponse = new ListResponseModel<CustomerDto_POS>();
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<CustomerDto_POS>> customerTask;
                    DateTime CurrentTime = DateTime.Now.ToUniversalTime();

                    if (lastSyncDate == null)
                    {
                        lastSyncDate = DateTime.MinValue;
                    }
                    var maxResult = 1;
                    GetCustomerInput customerFilter = new GetCustomerInput
                    {
                        OutletId = Settings.StoreGeneralRule.EnableCustomerMultiOutlet ? Settings.SelectedOutletId:0,
                        modifiedDateTime = lastSyncDate.Value,
                        maxResultCount = maxResult,
                        skipCount = 0,
                        filter = filter,
                        sorting = "0"
                    };

                Retry:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerTask = _apiService.Background.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerTask = _apiService.UserInitiated.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerTask = _apiService.Speculative.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            default:
                                customerTask = _apiService.UserInitiated.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                        }
                        Debug.WriteLine("GetCustomers API : /api/services/app/customer/GetAll_For_POS_ByPaging");

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers API Request Starts at: " + startDateTime);

                        remoteCustomerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerTask);

                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers API Request Total Time : " + (endDateTime - startDateTime));
                        Debug.WriteLine("GetCustomers API Response : " + JsonConvert.SerializeObject(remoteCustomerResponse));
                    }
                    catch (ApiException ex)
                    {
                        remoteCustomerResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerDto_POS>>();

                        if (remoteCustomerResponse != null && remoteCustomerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customers.",ex);
                            }
                        }
                        return null;
                    }

                    if (remoteCustomerResponse != null && remoteCustomerResponse.success && remoteCustomerResponse.result != null && remoteCustomerResponse.result.totalCount > 0)
                    {
                        var totalcustomer = remoteCustomerResponse.result.totalCount;
                        int totalStep = 1;
                        maxResult = 100;

                        //Ticket #11184 Start : Large Customers reading error. By Nikhil
                        if (5000 < totalcustomer && totalcustomer < 10000)
                        {
                            maxResult = 400;
                        }
                        else if (totalcustomer > 10000)
                        {

                            maxResult = 1000;
                        }

                        if (totalcustomer > maxResult)
                        {
                            double tstep = (Convert.ToDouble(totalcustomer) / maxResult);
                            totalStep = (int)Math.Ceiling(tstep);
                        }

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers Chunks API Request Starts at: " + startDateTime);

                        int startpoint = 0;
                        do
                        {
                            var tasks = new List<Task>();
                            int endpoint = Math.Min(totalStep, startpoint + 10);// startpoint + 100;
                            for (var i = startpoint; i < endpoint; i++)
                            {
                                var skipCustomerCount = (i * maxResult);

                                if (skipCustomerCount > totalcustomer)
                                {
                                    break;
                                }
                                Debug.WriteLine("Customer Request : " + i);
                                var _customerFilter = customerFilter.Copy();
                                _customerFilter.skipCount = skipCustomerCount + 1;
                                _customerFilter.maxResultCount = maxResult;

                                Task tmptask = GetRemoteCustomerLemdaAndStoreInLocal(priority, _customerFilter,syncLocal);
                                tasks.Add(tmptask);
                            }
                            await Task.WhenAll(tasks.ToArray());
                            startpoint = endpoint;
                        } while (startpoint < totalStep);
                        //Ticket #11184 End : By Nikhil

                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomers Chunks API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetCustomers Chunks API Request Total Time : " + (endDateTime - startDateTime));

                        if (remoteCustomerResponse != null && remoteCustomerResponse.result != null && remoteCustomerResponse.result.items != null && remoteCustomerResponse.result.items.Any())
                        {
                            // //Debug.WriteLine("all customer : " + Newtonsoft.Json.JsonConvert.SerializeObject(remoteCustomerResponse));
                            // if (syncLocal)
                            // {
                            //     remoteCustomerResponse.result.items.All(c => { c.IsSync = true; return true; });
                            //      UpdateLocalCustomers(remoteCustomerResponse.result.items);
                            // }
                            return remoteCustomerResponse.result.items;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (priority != Priority.Background && remoteCustomerResponse != null && remoteCustomerResponse.error != null && remoteCustomerResponse.error.message != null && !string.IsNullOrEmpty(remoteCustomerResponse.error.message))
                        {
                            Extensions.ServerMessage(remoteCustomerResponse.error.message);
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
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        private JsonSerializer _serializer = new JsonSerializer();
        public async Task GetRemoteCustomerLemdaAndStoreInLocal(Priority priority, GetCustomerInput customerFilter, bool syncLocal)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(
                        LanguageExtension.Localize("NoInternetMessage"),
                        Colors.Red,
                        Colors.White);
                }
                return;
            }

            ListResponseModel<CustomerDto_POS> responseModel = null;

            var httpService = new HttpService();
            var httpClient = httpService.httpClient();
            var urlPath = string.Format("https://y0a4jgvqo6.execute-api.us-west-2.amazonaws.com/Prod/GetHikeCustomers?dbType={0}&dbname={1}&lastSyncDate={2}&skip={3}&take={4}&outletId={5}&liveMode=true", Settings.CurrentDatabaseType, Settings.CurrentDatabaseName, customerFilter.modifiedDateTime.ToString(CultureInfo.InvariantCulture), customerFilter.skipCount, customerFilter.maxResultCount, customerFilter.OutletId);

            const int maxRetryCount = 1;
            for (int attempt = 0; attempt <= maxRetryCount; attempt++)
            {
                try
                {
                    var startTime = DateTime.Now;

                    using var response = await httpClient
                        .GetAsync(urlPath)
                        .ConfigureAwait(false);

                    Debug.WriteLine(
                        $"GetCustomer Chunks API Request Total Time : {customerFilter.maxResultCount} : {DateTime.Now - startTime}"
                    );

                    response.EnsureSuccessStatusCode();

                    await using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);

                    var customerResponse =
                        _serializer.Deserialize<ListResult<CustomerDto_POS>>(jsonReader);

                    responseModel = new ListResponseModel<CustomerDto_POS>
                    {
                        success = true,
                        result = new ListResultDto<CustomerDto_POS>
                        {
                            totalCount = customerResponse.Count,
                            items = customerResponse.items
                        }
                    };

                    break;
                }
                catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    responseModel = await ex.GetContentAsAsync<ListResponseModel<CustomerDto_POS>>();

                    if (responseModel?.unAuthorizedRequest == true)
                    {
                        bool renewed = await accountService.GetRenewAccessToken(priority);
                        if (renewed && attempt < maxRetryCount)
                            continue;
                    }
                }                
                catch (HttpRequestException ex)
                {
                    ex.Track();
                    if (attempt < maxRetryCount)
                    {
                        await Task.Delay(800);
                        continue;
                    }
                    else
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
                                Extensions.SomethingWentWrong("Getting customers and storing locally.", ex);
                            }
                        }
                    }
                }
                catch (TimeoutException ex)
                {
                    ex.Track();
                    if (attempt < maxRetryCount)
                        continue;
                    //else
                    //    App.Instance.Hud.DisplayToast("TimeoutException:- " + ex.Message, Colors.Red, Colors.White);
                }
                catch (Exception ex)
                {
                    ex.Track();
                    //App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
                }
            }

            if (responseModel?.success == true &&
                responseModel.result?.totalCount > 0)
            {
                if (syncLocal)
                {
                    foreach (var customer in responseModel.result.items)
                        customer.IsSync = true;

                    UpdateLocalCustomers(responseModel.result.items);
                }

                return;
            }

            if (priority != Priority.Background &&
                responseModel?.error?.message is { Length: > 0 })
            {
                Extensions.ServerMessage(responseModel.error.message);
            }
        }

        public async Task GetRemoteCustomerAndStoreInLocal(Priority priority, GetCustomerInput customerFilter)
        {
            try
            {
                ListResponseModel<CustomerDto_POS> tmpremoteCustomerResponse = new ListResponseModel<CustomerDto_POS>();
                
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<CustomerDto_POS>> customerTask;

                Retry:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerTask = _apiService.Background.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerTask = _apiService.UserInitiated.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerTask = _apiService.Speculative.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                            default:
                                customerTask = _apiService.UserInitiated.GetCustomers(customerFilter, Settings.AccessToken);
                                break;
                        }

                        var chunkStartDateTime = DateTime.Now;
                       
                        var retryPolicy = Policy
                                .Handle<HttpRequestException>()
                                .Or<TaskCanceledException>()  // timeout / request aborted
                                .WaitAndRetryAsync(
                                    retryCount: 2,
                                    sleepDurationProvider: retry =>
                                        TimeSpan.FromSeconds(1), // wait 1s between retries
                                    onRetry: (ex, delay, retry, context) =>
                                    {
                                        ex.Track(); 
                                    });

                        var timeoutPolicy =
                            Policy.TimeoutAsync(TimeSpan.FromSeconds(ServiceConfiguration.ServiceTimeoutSeconds * 2));

                        tmpremoteCustomerResponse =
                            await Policy
                                .WrapAsync(retryPolicy, timeoutPolicy)
                                .ExecuteAsync(() => customerTask);

                        //await AssignCustomerGroups(tmpremoteCustomerResponse);
                        Debug.WriteLine("GetCustomer Chunks API Request Total Time : " + customerFilter.maxResultCount + " : " + (DateTime.Now - chunkStartDateTime));

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        tmpremoteCustomerResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerDto_POS>>();

                        if (tmpremoteCustomerResponse != null && tmpremoteCustomerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
                            }
                        }
                        return;
                    }

                    if (tmpremoteCustomerResponse != null && tmpremoteCustomerResponse.success && tmpremoteCustomerResponse.result != null && tmpremoteCustomerResponse.result.totalCount > 0)
                    {
                        if (remoteCustomerResponse != null && remoteCustomerResponse.result != null && remoteCustomerResponse.result.items != null)
                        {
                            remoteCustomerResponse.result.items = new ObservableCollection<CustomerDto_POS>(remoteCustomerResponse.result.items.Concat(tmpremoteCustomerResponse.result.items));
                        }
                        return;
                    }
                    else
                    {
                        if (priority != Priority.Background && tmpremoteCustomerResponse != null && tmpremoteCustomerResponse.error != null && tmpremoteCustomerResponse.error.message != null && !string.IsNullOrEmpty(tmpremoteCustomerResponse.error.message))
                        {
                            Extensions.ServerMessage(tmpremoteCustomerResponse.error.message);
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
            }
            catch (Exception ex)
            {
                ex.Track();
                return;
            }
        }

        public ObservableCollection<CustomerDto_POS> GetLocalCustomers()
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                var addresses = realm1.All<CustomerDB_POS>().ToList();
                return new ObservableCollection<CustomerDto_POS>(addresses.Select(a => CustomerDto_POS.FromModel(a)));
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public ObservableCollection<CustomerDto_POS> GetLocalCustomerByKeyword(string keyword)
        {
            try
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    //var customers = await BlobCache.LocalMachine.GetAllObjects<CustomerDto_POS>();
                    var realm = RealmService.GetRealm();
                    var customers = realm.All<CustomerDB_POS>().Where(x => (x.FirstName + " " + x.LastName).ToLower().Contains(keyword.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(keyword.ToLower()))).Select(a => CustomerDto_POS.FromModel(a)).ToList();
                    if (customers != null)// && customers.Any(x => (x.FirstName + " " + x.LastName).ToLower().Contains(keyword.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(keyword.ToLower()))))
                    {
                        return new ObservableCollection<CustomerDto_POS>(customers);
                    }
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
            return null;
        }

        public bool CheckLocalCustomerIsExistByEmail(string email, int CustomerId)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    //var customers = await BlobCache.LocalMachine.GetAllObjects<CustomerDto_POS>();
                    var realm = RealmService.GetRealm();
                    var customers = realm.All<CustomerDB_POS>().ToList().Any(x => !string.IsNullOrEmpty(x.Email) && x.Email.ToLower() == email.ToLower() && x.Id != CustomerId);

                    if (customers != null)
                    {
                        return customers;
                    }
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

        public ObservableCollection<CustomerDto_POS> GetUnSyncCustomer()
        {
            try
            {
               // var customers = await BlobCache.LocalMachine.GetAllObjects<CustomerDto_POS>();
                using var realm1 = RealmService.GetRealm();
                var customers = realm1.All<CustomerDB_POS>().ToList();
                if (customers != null && customers.Any(x => x.IsSync == false))
                {
                    return new ObservableCollection<CustomerDto_POS>(customers.Where(x => x.IsSync == false).Select(a=> CustomerDto_POS.FromModel(a)));
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
            return null;
        }

        public CustomerDto_POS GetLocalCustomerById(int Id)
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                var addresses = realm1.All<CustomerDB_POS>().FirstOrDefault(a => a.Id == Id);
                return CustomerDto_POS.FromModel(addresses);
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

        public CustomerDto_POS GetLocalCustomerByTempId(string TempId)
        {
            try
            {
                if (string.IsNullOrEmpty(TempId))
                    return null;
                //return await CommonQueries.GetObject<CustomerDto_POS>(TempId);
                using var realm1 = RealmService.GetRealm();
                var addresses = realm1.All<CustomerDB_POS>().FirstOrDefault(a => a.TempId == TempId);
                return CustomerDto_POS.FromModel(addresses);
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

        public bool UpdateLocalCustomers(ObservableCollection<CustomerDto_POS> customers)
        {
            try
            {
                if (customers == null || !customers.Any())
                {
                    return false;
                }
               
                using var realm1 = RealmService.GetRealm();
                realm1.Write(() =>
                {
                    realm1.Add(customers.Select(a => a.ToModel()), update: true);
                });
                return true;
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

        public bool UpdateLocalCustomer(CustomerDto_POS customer)
        {
            try
            {
                if (customer == null)
                {
                    return false;
                }

                string docId = "";
                if (customer.Id == 0)
                {
                    if (string.IsNullOrEmpty(customer.TempId))
                    {
                        docId = nameof(CustomerDto_POS) + "_" + Guid.NewGuid().ToString();
                        customer.TempId = docId;
                    }
                    else
                    {
                        docId = customer.TempId;
                    }
                }
                else
                {

                    if (!string.IsNullOrEmpty(customer.TempId))
                    {
                        try
                        {
                            using var realm1 = RealmService.GetRealm();
                            var data = realm1.All<CustomerDB_POS>().Where(a => a.TempId == customer.TempId);
                            realm1.Write(() =>
                            {
                                realm1.RemoveRange<CustomerDB_POS>(data);
                            });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }

                    docId = nameof(CustomerDto_POS) + "_" + customer.Id.ToString();
                }
                try
                {
                    if (customer.Id == 0)
                    {
                        using (var realm = RealmService.GetRealm())
                        {
                            var data = realm.Find<CustomerDB_POS>(customer.ToModel().KeyId);
                            if (data != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(data);
                                    data = null;
                                });
                            }
                        };
                    }

                    using var realm1 = RealmService.GetRealm();
                    realm1.Write(() =>
                    {
                        realm1.Add(customer.ToModel(), update: true);
                    });
                   // await BlobCache.LocalMachine.InsertObject<CustomerDto_POS>(docId, customer, DateTimeOffset.Now.AddYears(2));
                    return true;
                }
                catch (KeyNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }

            }
            catch (Exception ex)
            {
                ex.Track();

            }
            return false;
        }

        public bool DeleteLocalCustomer(string customerId)
        {
            try
            {
                if (!string.IsNullOrEmpty(customerId))
                {
                    using var realm1 = RealmService.GetRealm();
                    var data = realm1.Find<CustomerDB_POS>(customerId);
                    if (data != null)
                    {
                        realm1.Write(() =>
                        {
                            realm1.Remove(data);
                            data = null;
                        });
                    }
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

        public async Task<CustomerDto_POS> UpdateRemoteCustomer(Priority priority, bool syncLocal, CustomerDto_POS customer)
        {
            try
            {
                if (customer == null)
                {
                    return null;
                }
                else
                {
                    if (customer.CustomerTags != null && customer.CustomerTags.Count > 0)
                    {
                        int val = 0;
                        if (Int32.TryParse(customer.CustomerTags[0].ToString(), out val))
                            customer.CustomerTags = new ObservableCollection<object>(customer.CustomerTags.Select(a => new CustomerTagsDto { tagId = Convert.ToInt32(a.ToString()) }));
                    }
                }

                //Ticket #12521 Add New client don’t show in Dashboard. By Nikhil	
                customer.IsActive = true;
                //Ticket #12521 End.By Nikhil
                Debug.WriteLine("customer dto : " + Newtonsoft.Json.JsonConvert.SerializeObject(customer));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<CustomerDto_POS> customerResponse = new ResponseModel<CustomerDto_POS>();
                    Task<ResponseModel<CustomerDto_POS>> customerTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            customerTask = _apiService.Background.AddOrUpadateCustomer(customer, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            customerTask = _apiService.UserInitiated.AddOrUpadateCustomer(customer, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            customerTask = _apiService.Speculative.AddOrUpadateCustomer(customer, Settings.AccessToken);
                            break;
                        default:
                            customerTask = _apiService.UserInitiated.AddOrUpadateCustomer(customer, Settings.AccessToken);
                            break;
                    }

                    try
                    {

                        customerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerTask);

                        Debug.WriteLine("customer response : " + Newtonsoft.Json.JsonConvert.SerializeObject(customerResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customerResponse = await ex.GetContentAsAsync<ResponseModel<CustomerDto_POS>>();
                        if (customerResponse != null && customerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry1;
                            }
                        }
                        else if (customerResponse != null && customerResponse.error != null && (ex.StatusCode == System.Net.HttpStatusCode.Conflict || customerResponse.error.message == "Email already exist, please try with other option"))
                        {

                            var existingcustomerResponse = await GetRemoteCustomerDetailByEmailId(priority, customer.Email, false);
                            if (existingcustomerResponse != null && existingcustomerResponse.success && existingcustomerResponse.result != null)
                            {
                                existingcustomerResponse.result.TempId = customer.TempId;
                                customerResponse = existingcustomerResponse;
                            }
                        }
                        //ex.LogError("customer","CreateOrUpdate",customer);
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
                                Extensions.SomethingWentWrong("Updaing customers.",ex);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return null;
                    }


                    if (customerResponse != null && customerResponse.success && customerResponse.result != null)
                    {
                        customerResponse.result.PickerDate = customerResponse.result.PickerDate.Value;

                        if (customerResponse.result.BirthDate != null)
                            customerResponse.result.BirthDate = customerResponse.result.BirthDate.Value;

                        if (syncLocal)
                        {
                            customerResponse.result.IsSync = true;
                            if (!string.IsNullOrEmpty(customer.TempId))
                            {
                                customerResponse.result.TempId = customer.TempId;
                            }
                            UpdateLocalCustomer(customerResponse.result);
                        }


                        return customerResponse.result;
                    }
                    else if (customerResponse != null && customerResponse.error != null && customerResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(customerResponse.error.message, Colors.Red, Colors.White);
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {

                    if (syncLocal)
                    {
                        customer.IsSync = false;
                        UpdateLocalCustomer(customer);
                        return customer;
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
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public async Task<ResponseModel<CustomerDto_POS>> GetRemoteCustomerDetail(Priority priority, int id, bool IsSync = true)
        {
            ResponseModel<CustomerDto_POS> customerResponse = new ResponseModel<CustomerDto_POS>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<CustomerDto_POS>> customerTask;

                    GetCustomerDetailInput customerFilter = new GetCustomerDetailInput
                    {
                        id = id
                    };
                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerTask = _apiService.Background.GetCustomerDetail(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerTask = _apiService.UserInitiated.GetCustomerDetail(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerTask = _apiService.Speculative.GetCustomerDetail(customerFilter, Settings.AccessToken);
                                break;
                            default:
                                customerTask = _apiService.UserInitiated.GetCustomerDetail(customerFilter, Settings.AccessToken);
                                break;
                        }


                        customerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerTask);

                        Debug.WriteLine("Get CustomerDetails API Response : " + JsonConvert.SerializeObject(customerResponse));
                    }
                    catch (ApiException ex)
                    {

                        //Get Exception content
                        customerResponse = await ex.GetContentAsAsync<ResponseModel<CustomerDto_POS>>();
                        if (customerResponse != null && customerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customer details.",ex);
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

                if (customerResponse != null && customerResponse.success && customerResponse.result != null)
                {
                    if (IsSync)
                    {
                        UpdateLocalCustomer(customerResponse.result);
                    }
                    return customerResponse;
                }
                else if (priority != Priority.Background && customerResponse != null && customerResponse.error != null && customerResponse.error.message != null && !string.IsNullOrEmpty(customerResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(customerResponse.error.message, Colors.Red, Colors.White);
                    return null;
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
            return null;
        }

        public async Task<ResponseModel<CustomerDto_POS>> GetRemoteCustomerDetailByEmailId(Priority priority, string emailId, bool IsSync = true)
        {
            ResponseModel<CustomerDto_POS> customerResponse = new ResponseModel<CustomerDto_POS>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<CustomerDto_POS>> customerTask;


                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerTask = _apiService.Background.GetCustomerDetailByEmail(emailId, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerTask = _apiService.UserInitiated.GetCustomerDetailByEmail(emailId, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerTask = _apiService.Speculative.GetCustomerDetailByEmail(emailId, Settings.AccessToken);
                                break;
                            default:
                                customerTask = _apiService.UserInitiated.GetCustomerDetailByEmail(emailId, Settings.AccessToken);
                                break;
                        }


                        customerResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content

                        customerResponse = await ex.GetContentAsAsync<ResponseModel<CustomerDto_POS>>();
                        if (customerResponse != null && customerResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customer by email id.",ex);
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

                if (customerResponse != null && customerResponse.success && customerResponse.result != null)
                {
                    if (IsSync)
                    {
                        UpdateLocalCustomer(customerResponse.result);
                    }
                    return customerResponse;
                }
                else if (priority != Priority.Background && customerResponse != null && customerResponse.error != null && customerResponse.error.message != null && !string.IsNullOrEmpty(customerResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(customerResponse.error.message, Colors.Red, Colors.White);
                    return null;
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

            return null;
        }

        #endregion

        public async Task<ObservableCollection<InvoiceDto>> GetRemoteCustomerInvoices(Priority priority, bool syncLocal, int CustomerId)
        {
            try
            {
                ListResponseModel<InvoiceDto> customerInvoiceResponse = new ListResponseModel<InvoiceDto>();
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    Task<ListResponseModel<InvoiceDto>> customerInvoiceTask;


                    GetCustomerInvoicesInput customerFilter = new GetCustomerInvoicesInput
                    {
                        customerId = CustomerId,
                        status = new List<InvoiceStatus>() { InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy , InvoiceStatus.Quote },
                        maxResultCount = 100,
                        skipCount = 0,
                        filter = "",
                        sorting = "0"
                    };

                Retry4:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerInvoiceTask = _apiService.Background.GetCustomerInvoices(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerInvoiceTask = _apiService.UserInitiated.GetCustomerInvoices(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerInvoiceTask = _apiService.Speculative.GetCustomerInvoices(customerFilter, Settings.AccessToken);
                                break;
                            default:
                                customerInvoiceTask = _apiService.UserInitiated.GetCustomerInvoices(customerFilter, Settings.AccessToken);
                                break;
                        }

                        //List<CustomerInvoices> tempCustomerInvoices = new List<CustomerInvoices>();

                        customerInvoiceResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerInvoiceTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customerInvoiceResponse = await ex.GetContentAsAsync<ListResponseModel<InvoiceDto>>();
                        if (customerInvoiceResponse != null && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customer invoices.",ex);
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
                if (customerInvoiceResponse != null && customerInvoiceResponse.success &&
                    customerInvoiceResponse.result != null && customerInvoiceResponse.result.totalCount > 0 && customerInvoiceResponse.result.items.Any())
                {
                    if (syncLocal)
                    {
                        //await UpdateLocalCustomerInvoices(customerInvoiceResponse.result.items);
                    }
                    return customerInvoiceResponse.result.items;
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
            return null;
        }

        #region CustomField
        public async Task<ObservableCollection<CustomField>> GetRemoteCustomerCustomFields(Priority priority, bool IsSync = true)
        {
            ResponseListModel<CustomField> CustomFieldsResponse = new ResponseListModel<CustomField>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseListModel<CustomField>> customerTask;

                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerTask = _apiService.Background.GetCustomFields(Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerTask = _apiService.UserInitiated.GetCustomFields(Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerTask = _apiService.Speculative.GetCustomFields(Settings.AccessToken);
                                break;
                            default:
                                customerTask = _apiService.UserInitiated.GetCustomFields(Settings.AccessToken);
                                break;
                        }


                        CustomFieldsResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content

                        CustomFieldsResponse = await ex.GetContentAsAsync<ResponseListModel<CustomField>>();
                        if (CustomFieldsResponse != null && CustomFieldsResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customer's custom fields.",ex);
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

                if (CustomFieldsResponse != null && CustomFieldsResponse.success && CustomFieldsResponse.result != null)
                {
                    if (IsSync)
                    {
                        if (CustomFieldsResponse.result != null)
                        {
                            UpdateLocalCustomerCustomFields(CustomFieldsResponse.result);
                        }
                    }
                    return CustomFieldsResponse.result;
                }
                else if (priority != Priority.Background && CustomFieldsResponse != null && CustomFieldsResponse.error != null && CustomFieldsResponse.error.message != null && !string.IsNullOrEmpty(CustomFieldsResponse.error.message))
                {
                    Extensions.ServerMessage(CustomFieldsResponse.error.message);
                    return null;
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

            return null;
        }

        public ObservableCollection<CustomField> GetAllLocalCustomerCustomFields()
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                return new ObservableCollection<CustomField>(realm1.All<CustomFieldDB>().ToList().Select(a=> CustomField.FromModel(a)));
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public bool UpdateLocalCustomerCustomFields(ObservableCollection<CustomField> customField)
        {
            try
            {
                if (customField == null)
                {
                    return false;
                }
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.RemoveAll<CustomFieldDB>();
                });
                if (customField.Count > 0)
                {
                    using var realm1 = RealmService.GetRealm();
                    realm1.Write(() =>
                    {
                        realm1.Add(customField.Select(a=>a.ToModel()), update: true);
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.Track();

            }
            return false;
        }

        public async Task<ObservableCollection<CustomField>> UpdateRemoteCustomFields(Priority priority, bool syncLocal, ObservableCollection<CustomField> customField)
        {
            try
            {
                if (customField == null)
                {
                    return null;
                }

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    ResponseListModel<CustomField> customFieldResponse = new ResponseListModel<CustomField>();
                    Task<ResponseListModel<CustomField>> customFieldTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            customFieldTask = _apiService.Background.AddOrUpadateCustomField(customField, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            customFieldTask = _apiService.UserInitiated.AddOrUpadateCustomField(customField, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            customFieldTask = _apiService.Speculative.AddOrUpadateCustomField(customField, Settings.AccessToken);
                            break;
                        default:
                            customFieldTask = _apiService.UserInitiated.AddOrUpadateCustomField(customField, Settings.AccessToken);
                            break;
                    }

                    try
                    {
                        customFieldResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customFieldTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customFieldResponse = await ex.GetContentAsAsync<ResponseListModel<CustomField>>();
                        if (customFieldResponse != null && customFieldResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return null;
                    }


                    if (customFieldResponse != null && customFieldResponse.success && customFieldResponse.result != null)
                    {
                        if (syncLocal)
                        {
                            if (customFieldResponse.result != null)
                            {
                                UpdateLocalCustomerCustomFields(customFieldResponse.result);
                            }
                        }
                        return customFieldResponse.result;
                    }
                    else if (customFieldResponse != null && customFieldResponse.error != null && customFieldResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(customFieldResponse.error.message, Colors.Red, Colors.White);
                        }
                        return null;
                    }
                    else
                    {
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
            return null;
        }

        #endregion

        #region Customer Groups

        public async Task<ObservableCollection<CustomerGroupDto>> GetRemoteCustomerGroups(Priority priority, bool syncLocal, string filter = "", DateTime? lastSyncDate = null)
        {
            try
            {
                ListResponseModel<CustomerGroupDto> customerGroupResponse = new ListResponseModel<CustomerGroupDto>();
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    Task<ListResponseModel<CustomerGroupDto>> customerGroupTask;


                Retry3:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerGroupTask = _apiService.Background.GetCustomerGroups(Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerGroupTask = _apiService.UserInitiated.GetCustomerGroups(Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerGroupTask = _apiService.Speculative.GetCustomerGroups(Settings.AccessToken);
                                break;
                            default:
                                customerGroupTask = _apiService.UserInitiated.GetCustomerGroups(Settings.AccessToken);
                                break;
                        }

                        Debug.WriteLine("GetCustomerGroups API : /api/services/app/customerGroup/GetAll_For_POS");

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomerGroups API Request Starts at: " + startDateTime);

                        customerGroupResponse = await Policy
                          .Handle<Exception>()
                          .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                          .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                          .ExecuteAsync(async () => await customerGroupTask);

                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetCustomerGroups API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetCustomerGroups API Request Total Time : " + (endDateTime - startDateTime));

                       // Debug.WriteLine("customerGroupResponse : " + JsonConvert.SerializeObject(customerGroupResponse));

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customerGroupResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerGroupDto>>();
                        if (customerGroupResponse != null && customerGroupResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        Debug.WriteLine("Exception in GetRemoteCustomerGroups : " + ex.Message);
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
                                Extensions.SomethingWentWrong("Getting customer groups.",ex);
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

                if (customerGroupResponse != null && customerGroupResponse.success && customerGroupResponse.result != null && customerGroupResponse.result.items != null && customerGroupResponse.result.items.Any())
                {
                    if (syncLocal)
                    {
                        UpdateLocalCustomerGroups(customerGroupResponse.result.items);
                    }
                    return customerGroupResponse.result.items;
                }
                else if (priority != Priority.Background && customerGroupResponse != null && customerGroupResponse.error != null && customerGroupResponse.error.message != null && !string.IsNullOrEmpty(customerGroupResponse.error.message))
                {
                    Extensions.ServerMessage(customerGroupResponse.error.message);
                    return null;
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
            return null;
        }

        public CustomerGroupDto GetLocalCustomerGroupById(int Id)
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                return CustomerGroupDto.FromModel(realm1.All<CustomerGroupDB>().ToList().FirstOrDefault(a => a.Id == Id));
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

        public ObservableCollection<CustomerGroupDto> GetLocalCustomerGroups()
        {
            try
            {
               // return await CommonQueries.GetAllLocals<CustomerGroupDto>();
                using var realm1 = RealmService.GetRealm();
                return new ObservableCollection<CustomerGroupDto>(realm1.All<CustomerGroupDB>().ToList().Select(a => CustomerGroupDto.FromModel(a)));
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public bool UpdateLocalCustomerGroups(ObservableCollection<CustomerGroupDto> customerGroups)
        {
            try
            {
                if (customerGroups == null || !customerGroups.Any())
                {
                    return false;
                }

                using var realm1 = RealmService.GetRealm();
                realm1.Write(() =>
                {
                    realm1.Add(customerGroups.Select(a => a.ToModel()), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        #endregion

        #region CreditBalanceHistoryDto

        public bool UpdateLocalCustomerCreditBalance(ObservableCollection<CreditBalanceHistoryDto> customerCreditss)
        {
            try
            {
                if (customerCreditss == null || !customerCreditss.Any())
                {
                    return false;
                }
                using var realm1 = RealmService.GetRealm();
                realm1.Write(() =>
                {
                    realm1.Add(customerCreditss.Select(a => a.ToModel()), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public async Task<ObservableCollection<CreditBalanceHistoryDto>> GetRemoteCustomerCreditBalance(Priority priority, bool syncLocal, string filter = "", int customerId = 0)
        {
            try
            {

                ListResponseModel<CreditBalanceHistoryDto> customerCreditResponse = new ListResponseModel<CreditBalanceHistoryDto>();
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                    Task<ListResponseModel<CreditBalanceHistoryDto>> customerCreditTask;

                    GetCustomerCreditBalanceInput customerFilter = new GetCustomerCreditBalanceInput
                    {
                        customerId = customerId,
                        maxResultCount = 100,
                        skipCount = 0,
                        filter = filter,
                        sorting = "0"
                    };
                    int totalCustomers = 0;
                    int step = 1;

                Retry3:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerCreditTask = _apiService.Background.GetCustomerCreditBalance(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerCreditTask = _apiService.UserInitiated.GetCustomerCreditBalance(customerFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerCreditTask = _apiService.Speculative.GetCustomerCreditBalance(customerFilter, Settings.AccessToken);
                                break;
                            default:
                                customerCreditTask = _apiService.UserInitiated.GetCustomerCreditBalance(customerFilter, Settings.AccessToken);
                                break;
                        }

                        List<CreditBalanceHistoryDto> tempCustomerCreditList = new List<CreditBalanceHistoryDto>();
                        do
                        {

                            var temp_customerResponse = await Policy
                                .Handle<Exception>()
                                .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                                .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                                .ExecuteAsync(async () => await customerCreditTask);

                            if (temp_customerResponse != null && temp_customerResponse.success && temp_customerResponse.result != null && temp_customerResponse.result.items != null)
                            {
                                tempCustomerCreditList.AddRange(temp_customerResponse.result.items);
                                totalCustomers = temp_customerResponse.result.totalCount;
                                customerFilter.skipCount = customerFilter.maxResultCount * step;
                                step++;
                            }
                        }
                        while (totalCustomers >= customerFilter.skipCount);

                        if (customerCreditResponse == null)
                        {
                            customerCreditResponse = new ListResponseModel<CreditBalanceHistoryDto>();
                        }


                        if (tempCustomerCreditList.Count > 0)
                        {
                            customerCreditResponse.result.items = new ObservableCollection<CreditBalanceHistoryDto>(tempCustomerCreditList);
                            customerCreditResponse.success = true;
                        }
                        else
                        {
                            customerCreditResponse.result.items = new ObservableCollection<CreditBalanceHistoryDto>();
                            customerCreditResponse.success = true;
                        }

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customerCreditResponse = await ex.GetContentAsAsync<ListResponseModel<CreditBalanceHistoryDto>>();
                        if (customerCreditResponse != null && customerCreditResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                //Need to log this error to backend
                                Extensions.SomethingWentWrong("Getting customer credit balance.",ex);
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

                if (customerCreditResponse != null && customerCreditResponse.success && customerCreditResponse.result != null && customerCreditResponse.result.items != null && customerCreditResponse.result.items.Any())
                {
                    if (syncLocal)
                    {
                        UpdateLocalCustomerCreditBalance(customerCreditResponse.result.items);
                    }
                    return customerCreditResponse.result.items;
                }
                else if (priority != Priority.Background && customerCreditResponse != null && customerCreditResponse.error != null && customerCreditResponse.error.message != null && !string.IsNullOrEmpty(customerCreditResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(customerCreditResponse.error.message, Colors.Red, Colors.White);
                    return null;
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
            return null;
        }

        public ObservableCollection<CreditBalanceHistoryDto> GetLocalCustomerCreditBalance(int customerId)
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                var filtereddata = realm1.All<CreditBalanceHistoryDB>().ToList().Where(a => a.CustomerId == customerId);
                if (filtereddata != null)
                {
                    return new ObservableCollection<CreditBalanceHistoryDto>(filtereddata.Select(a=> CreditBalanceHistoryDto.FromModel(a)));
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }
        public async Task<CreditBalanceHistoryDto> UpdateRemoteCustomerCredits(Priority priority, bool syncLocal, CreditBalanceHistoryDto input)
        {
            try
            {
                if (input == null)
                {
                    return null;
                }

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<CreditBalanceHistoryDto> creditsResponse = new ResponseModel<CreditBalanceHistoryDto>();
                    Task<ResponseModel<CreditBalanceHistoryDto>> creditTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            creditTask = _apiService.Background.AddCustomerCreditIssued(input, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            creditTask = _apiService.UserInitiated.AddCustomerCreditIssued(input, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            creditTask = _apiService.Speculative.AddCustomerCreditIssued(input, Settings.AccessToken);
                            break;
                        default:
                            creditTask = _apiService.UserInitiated.AddCustomerCreditIssued(input, Settings.AccessToken);
                            break;
                    }

                    try
                    {
                        creditsResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await creditTask);
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        creditsResponse = await ex.GetContentAsAsync<ResponseModel<CreditBalanceHistoryDto>>();
                        if (creditsResponse != null && creditsResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            bool res = await accountService.GetRenewAccessToken(priority);
                            if (res)
                            {
                                goto Retry1;
                            }
                        }
                        //ex.LogError("customer","CreateOrUpdate",customer);
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
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return null;
                    }


                    if (creditsResponse != null && creditsResponse.success && creditsResponse.result != null)
                    {
                        if (syncLocal)
                        {
                            //creditsResponse.result.TransactionId = true;
                            if (!string.IsNullOrEmpty(input.TempId))
                            {
                                creditsResponse.result.TempId = input.TempId;
                            }
                            UpdateLocalCustomerCredits(creditsResponse.result);
                        }
                        return creditsResponse.result;
                    }
                    else if (creditsResponse != null && creditsResponse.error != null && creditsResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(creditsResponse.error.message, Colors.Red, Colors.White);
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    if (syncLocal)
                    {
                        UpdateLocalCustomerCredits(input);
                        return input;
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
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public bool UpdateLocalCustomerCredits(CreditBalanceHistoryDto input)
        {
            try
            {
                if (input == null)
                {
                    return false;
                }

                string docId = "";
                if (input.Id == 0)
                {
                    if (string.IsNullOrEmpty(input.TempId))
                    {
                        docId = nameof(CreditBalanceHistoryDto) + "_" + Guid.NewGuid().ToString();
                        input.TempId = docId;
                    }
                    else
                    {
                        docId = input.TempId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(input.TempId))
                    {
                        try
                        {
                            using var realm1 = RealmService.GetRealm();
                            var data = realm1.All<CreditBalanceHistoryDB>().Where(a => a.TempId == input.TempId);
                            realm1.Write(() =>
                            {
                                realm1.RemoveRange<CreditBalanceHistoryDB>(data);
                            });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }

                    docId = nameof(CustomerDto_POS) + "_" + input.Id.ToString();
                }
                try
                {
                   // await BlobCache.LocalMachine.InsertObject<CreditBalanceHistoryDto>(docId, input, DateTimeOffset.Now.AddYears(2));
                    using var realm1 = RealmService.GetRealm();
                    realm1.Write(() =>
                    {
                        realm1.Add(input.ToModel(), update: true);
                    }); 
                    return true;
                }
                catch (KeyNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }

            }
            catch (Exception ex)
            {
                ex.Track();

            }
            return false;
        }

        #endregion

        #region CustomerAddressDto

        //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
        public async Task<ObservableCollection<CustomerAddressDto>> GetRemoteAllDeliveryAddresses(Priority priority, bool syncLocal, string filter = "", DateTime? lastSyncDate = null)
        {
            try
            {
                var maxResult = 1;
                CustomerAddressInputDto customerAddressInput = new CustomerAddressInputDto
                {
                    modifiedDateTime = lastSyncDate,
                    maxResultCount = maxResult,
                    skipCount = 0,
                    filter = "",
                    sorting = "0"
                };
                Debug.WriteLine("customerAdressInput dto : " + Newtonsoft.Json.JsonConvert.SerializeObject(customerAddressInput));
                 remoteCustomerAddressResponse = new ListResponseModel<CustomerAddressDto>();

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<CustomerAddressDto>> customerAddressTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            customerAddressTask = _apiService.Background.GetAllDeliveryAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            customerAddressTask = _apiService.UserInitiated.GetAllDeliveryAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            customerAddressTask = _apiService.Speculative.GetAllDeliveryAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                        default:
                            customerAddressTask = _apiService.UserInitiated.GetAllDeliveryAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                    }

                    try
                    {

                        remoteCustomerAddressResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerAddressTask);

                        Debug.WriteLine("DeliveryAddresses response : " + Newtonsoft.Json.JsonConvert.SerializeObject(remoteCustomerAddressResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        remoteCustomerAddressResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerAddressDto>>();
                        if (remoteCustomerAddressResponse != null && remoteCustomerAddressResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Updaing customers.",ex);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return null;
                    }


                    if (remoteCustomerAddressResponse != null && remoteCustomerAddressResponse.success && remoteCustomerAddressResponse.result != null && remoteCustomerAddressResponse.result.totalCount > 0)
                    {
                        var totalcustomer = remoteCustomerAddressResponse.result.totalCount;
                        int totalStep = 1;
                        maxResult = 100;

                        //Ticket #11184 Start : Large Customers reading error. By Nikhil
                        if (5000 < totalcustomer && totalcustomer < 10000)
                        {
                            maxResult = 400;
                        }
                        else if (totalcustomer > 10000)
                        {

                            maxResult = 1000;
                        }

                        if (totalcustomer > maxResult)
                        {
                            double tstep = (Convert.ToDouble(totalcustomer) / maxResult);
                            totalStep = (int)Math.Ceiling(tstep);
                        }

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetRemoteAllDeliveryAddresses Chunks API Request Starts at: " + startDateTime);

                        int startpoint = 0;
                        do
                        {
                            var tasks = new List<Task>();
                            int endpoint = Math.Min(totalStep, startpoint + 10);// startpoint + 100;
                            for (var i = startpoint; i < endpoint; i++)
                            {
                                var skipCustomerCount = (i * maxResult);

                                if (skipCustomerCount > totalcustomer)
                                {
                                    break;
                                }
                                Debug.WriteLine("GetRemoteAllDeliveryAddresses Request : " + i);
                                var _customerFilter = customerAddressInput.Copy();
                                _customerFilter.skipCount = skipCustomerCount + 1;
                                _customerFilter.maxResultCount = maxResult;

                                Task tmptask = GetRemoteAllDeliveryAddressesAndStoreInLocal(priority, _customerFilter);
                                tasks.Add(tmptask);
                            }
                            await Task.WhenAll(tasks.ToArray());
                            startpoint = endpoint;
                        } while (startpoint < totalStep);
                        //Ticket #11184 End : By Nikhil

                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetRemoteAllDeliveryAddresses Chunks API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetRemoteAllDeliveryAddresses Chunks API Request Total Time : " + (endDateTime - startDateTime));

                        if (remoteCustomerAddressResponse != null && remoteCustomerAddressResponse.result != null && remoteCustomerAddressResponse.result.items != null && remoteCustomerAddressResponse.result.items.Any())
                        {
                            //Debug.WriteLine("all customer : " + Newtonsoft.Json.JsonConvert.SerializeObject(remoteCustomerResponse));
                            if (syncLocal)
                            {
                                remoteCustomerAddressResponse.result.items.All(c => { c.IsSync = true; return true; });
                                UpdateLocalDeliveryAddresses(remoteCustomerAddressResponse.result.items);
                            }
                            return remoteCustomerAddressResponse.result.items;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (remoteCustomerAddressResponse != null && remoteCustomerAddressResponse.error != null && remoteCustomerAddressResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(remoteCustomerAddressResponse.error.message, Colors.Red, Colors.White);
                        }
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
        public async Task GetRemoteAllDeliveryAddressesAndStoreInLocal(Priority priority, CustomerAddressInputDto customerAddressFilter)
        {
            try
            {
                ListResponseModel<CustomerAddressDto> tmpCustomerAddressResponse = new ListResponseModel<CustomerAddressDto>();

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ListResponseModel<CustomerAddressDto>> customerAddressTask;

                Retry:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                customerAddressTask = _apiService.Background.GetAllDeliveryAddresses(customerAddressFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                customerAddressTask = _apiService.Background.GetAllDeliveryAddresses(customerAddressFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                customerAddressTask = _apiService.Background.GetAllDeliveryAddresses(customerAddressFilter, Settings.AccessToken);
                                break;
                            default:
                                customerAddressTask = _apiService.Background.GetAllDeliveryAddresses(customerAddressFilter, Settings.AccessToken);
                                break;
                        }

                        var chunkStartDateTime = DateTime.Now;
                        tmpCustomerAddressResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds * 2))
                            .ExecuteAsync(async () => await customerAddressTask);

                        //await AssignCustomerGroups(tmpremoteCustomerResponse);
                        var chunkEndDateTime = DateTime.Now;

                        Debug.WriteLine("GetRemoteAllDeliveryAddresse Chunks API Request Total Time : " + customerAddressFilter.maxResultCount + " : " + (chunkEndDateTime - chunkStartDateTime));


                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        tmpCustomerAddressResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerAddressDto>>();

                        if (tmpCustomerAddressResponse != null && tmpCustomerAddressResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting customers and storing locally.",ex);
                            }
                        }
                        return;
                    }

                    if (tmpCustomerAddressResponse != null && tmpCustomerAddressResponse.success && tmpCustomerAddressResponse.result != null && tmpCustomerAddressResponse.result.totalCount > 0)
                    {
                        if (remoteCustomerResponse != null && remoteCustomerResponse.result != null && remoteCustomerResponse.result.items != null)
                        {
                            remoteCustomerAddressResponse.result.items = new ObservableCollection<CustomerAddressDto>(remoteCustomerAddressResponse.result.items.Concat(tmpCustomerAddressResponse.result.items));
                        }
                        return;
                    }
                    else
                    {
                        if (priority != Priority.Background && tmpCustomerAddressResponse != null && tmpCustomerAddressResponse.error != null && tmpCustomerAddressResponse.error.message != null && !string.IsNullOrEmpty(tmpCustomerAddressResponse.error.message))
                        {
                            Extensions.ServerMessage(tmpCustomerAddressResponse.error.message);
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
            }
            catch (Exception ex)
            {
                ex.Track();
                return;
            }
        }

        public async Task<bool> IsExistCustomerAddress(Priority priority, bool syncLocal, int customerId)
        {
            try
            {

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<bool> isCustomerExistResponse = new ResponseModel<bool>();
                    Task<ResponseModel<bool>> isCustomerExistTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            isCustomerExistTask = _apiService.Background.IsExistCustomerAddress(customerId, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            isCustomerExistTask = _apiService.UserInitiated.IsExistCustomerAddress(customerId, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            isCustomerExistTask = _apiService.Speculative.IsExistCustomerAddress(customerId, Settings.AccessToken);
                            break;
                        default:
                            isCustomerExistTask = _apiService.UserInitiated.IsExistCustomerAddress(customerId, Settings.AccessToken);
                            break;
                    }

                    try
                    {

                        isCustomerExistResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await isCustomerExistTask);

                        Debug.WriteLine("customer response : " + Newtonsoft.Json.JsonConvert.SerializeObject(isCustomerExistResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        isCustomerExistResponse = await ex.GetContentAsAsync<ResponseModel<bool>>();
                        if (isCustomerExistResponse != null && isCustomerExistResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Updaing customers.",ex);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return false;
                    }


                    if (isCustomerExistResponse != null && isCustomerExistResponse.success)
                    {

                        return isCustomerExistResponse.result;
                    }
                    else if (isCustomerExistResponse != null && isCustomerExistResponse.error != null && isCustomerExistResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(isCustomerExistResponse.error.message, Colors.Red, Colors.White);
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {

                    if (syncLocal)
                    {
                        //customerAddressResponse.IsSync = false;
                        //await UpdateLocalCustomer(customer);
                        return false;
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
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public async Task<ObservableCollection<CustomerAddressDto>> GetRemoteCustomerAddresses(Priority priority, bool syncLocal, int CustomerId)
        {
            try
            {
                CustomerAddressInputDto customerAddressInput = new CustomerAddressInputDto
                {
                    customerId = CustomerId,
                    maxResultCount = 100,
                    skipCount = 0,
                    filter = "",
                    sorting = "0"
                };
                Debug.WriteLine("customerAdressInput dto : " + Newtonsoft.Json.JsonConvert.SerializeObject(customerAddressInput));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ListResponseModel<CustomerAddressDto> customerAddressResponse = new ListResponseModel<CustomerAddressDto>();
                    Task<ListResponseModel<CustomerAddressDto>> customerAddressTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            customerAddressTask = _apiService.Background.GetCustomerAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            customerAddressTask = _apiService.UserInitiated.GetCustomerAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            customerAddressTask = _apiService.Speculative.GetCustomerAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                        default:
                            customerAddressTask = _apiService.UserInitiated.GetCustomerAddresses(customerAddressInput, Settings.AccessToken);
                            break;
                    }

                    try
                    {

                        customerAddressResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerAddressTask);

                        Debug.WriteLine("customer response : " + Newtonsoft.Json.JsonConvert.SerializeObject(customerAddressResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customerAddressResponse = await ex.GetContentAsAsync<ListResponseModel<CustomerAddressDto>>();
                        if (customerAddressResponse != null && customerAddressResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Updaing customers.",ex);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return null;
                    }


                    if (customerAddressResponse != null && customerAddressResponse.success && customerAddressResponse.result != null)
                    {
                        if (syncLocal)
                        {
                            customerAddressResponse.result.items.All(c => { c.IsSync = true; return true; });
                            UpdateLocalDeliveryAddresses(customerAddressResponse.result.items);
                        }


                        return customerAddressResponse.result.items;
                    }
                    else if (customerAddressResponse != null && customerAddressResponse.error != null && customerAddressResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(customerAddressResponse.error.message, Colors.Red, Colors.White);
                        }
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

        public bool UpdateLocalDeliveryAddresses(ObservableCollection<CustomerAddressDto> deliveryAddresses)
        {
            try
            {
                if (deliveryAddresses == null || !deliveryAddresses.Any())
                {
                    return false;
                }

                using var realm1 = RealmService.GetRealm();
                realm1.Write(() =>
                {
                    realm1.Add(deliveryAddresses.Select(a => a.ToModel()), update: true);
                });
                return true;
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
        public ObservableCollection<CustomerAddressDto> GetLocalDeliveryAddresses(int customerId)
        {
            try
            {
                //var addresses =  await CommonQueries.GetAllLocals<CustomerAddressDto>();
                using var realm1 = RealmService.GetRealm();
                var addresses = realm1.All<CustomerAddressDB>().ToList();
                var result = addresses.Where(x => x.CustomerId == customerId).Select(a => CustomerAddressDto.FromModel(a));
                return new ObservableCollection<CustomerAddressDto>(result.OrderBy(x=>x.Id));
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }
        public CustomerAddressDto GetLocalDeliveryAddressesById(int Id)
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                var addresses = realm1.Find<CustomerAddressDB>(Id.ToString());
                return CustomerAddressDto.FromModel(addresses);
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
        public ObservableCollection<CustomerAddressDto> GetUnSyncDeliveryAddresses()
        {
            try
            {
                //var deliveryAddresses = await BlobCache.LocalMachine.GetAllObjects<CustomerAddressDto>();
                using var realm1 = RealmService.GetRealm();
                var deliveryAddresses = realm1.All<CustomerAddressDB>().ToList();
                if (deliveryAddresses != null && deliveryAddresses.Any(x => x.IsSync == false))
                {
                    return new ObservableCollection<CustomerAddressDto>(deliveryAddresses.Where(x => x.IsSync == false).Select(a => CustomerAddressDto.FromModel(a)));
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
            return null;
        }

        public async Task<CustomerAddressDto> CreateOrUpdateRemoteCustomerAddress(Priority priority, bool syncLocal, CustomerAddressDto deliveryaddress)
        {
            try
            {
                if (deliveryaddress == null)
                {
                    return null;
                }
                Debug.WriteLine("customerAdress dto : " + Newtonsoft.Json.JsonConvert.SerializeObject(deliveryaddress));

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<CustomerAddressDto> customerAddressResponse = new ResponseModel<CustomerAddressDto>();
                    Task<ResponseModel<CustomerAddressDto>> customerAddressTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            customerAddressTask = _apiService.Background.CreateOrUpdateCustomerAddress(deliveryaddress, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            customerAddressTask = _apiService.UserInitiated.CreateOrUpdateCustomerAddress(deliveryaddress, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            customerAddressTask = _apiService.Speculative.CreateOrUpdateCustomerAddress(deliveryaddress, Settings.AccessToken);
                            break;
                        default:
                            customerAddressTask = _apiService.UserInitiated.CreateOrUpdateCustomerAddress(deliveryaddress, Settings.AccessToken);
                            break;
                    }

                    try
                    {

                        customerAddressResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await customerAddressTask);

                        Debug.WriteLine("customer response : " + Newtonsoft.Json.JsonConvert.SerializeObject(customerAddressResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        customerAddressResponse = await ex.GetContentAsAsync<ResponseModel<CustomerAddressDto>>();
                        if (customerAddressResponse != null && customerAddressResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Updaing customers.",ex);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return null;
                    }


                    if (customerAddressResponse != null && customerAddressResponse.success && customerAddressResponse.result != null)
                    {

                        if (syncLocal)
                        {
                            customerAddressResponse.result.IsSync = true;
                            if (!string.IsNullOrEmpty(deliveryaddress.TempId))
                            {
                                customerAddressResponse.result.TempId = deliveryaddress.TempId;
                            }
                            UpdateLocalDeliveryAddress(customerAddressResponse.result);
                        }


                        return customerAddressResponse.result;
                    }
                    else if (customerAddressResponse != null && customerAddressResponse.error != null && customerAddressResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(customerAddressResponse.error.message, Colors.Red, Colors.White);
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    if (syncLocal)
                    {
                        deliveryaddress.IsSync = false;
                        UpdateLocalDeliveryAddress(deliveryaddress);
                        return deliveryaddress;
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
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }
        public bool UpdateLocalDeliveryAddress(CustomerAddressDto deleiveryAddress)
        {
            try
            {
                if (deleiveryAddress == null)
                {
                    return false;
                }

                string docId = "";
                if (deleiveryAddress.Id == 0)
                {
                    if (string.IsNullOrEmpty(deleiveryAddress.TempId))
                    {
                        docId = nameof(CustomerAddressDto) + "_" + Guid.NewGuid().ToString();
                        deleiveryAddress.TempId = docId;
                    }
                    else
                    {
                        docId = deleiveryAddress.TempId;
                    }
                }
                else
                {

                    if (!string.IsNullOrEmpty(deleiveryAddress.TempId))
                    {
                        try
                        {
                           // await BlobCache.LocalMachine.Invalidate(deleiveryAddress.TempId);
                            using var realm1 = RealmService.GetRealm();
                            var data = realm1.All<CustomerAddressDB>().Where(a => a.TempId == deleiveryAddress.TempId);
                            realm1.Write(() =>
                            {
                                realm1.RemoveRange<CustomerAddressDB>(data);
                            });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }

                    docId = nameof(CustomerAddressDto) + "_" + deleiveryAddress.Id.ToString();
                }
                try
                {
                    if (deleiveryAddress.Id == 0)
                    {
                        using (var realm = RealmService.GetRealm())
                        {
                            var data = realm.Find<CustomerAddressDB>(deleiveryAddress.ToModel().TempId);
                            if (data != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(data);
                                    data = null;
                                });
                            }
                        };
                    }
                    else
                    {
                       using (var realm = RealmService.GetRealm())
                        {
                            var data = realm.Find<CustomerAddressDB>(deleiveryAddress.ToModel().Id.ToString());
                            if (data != null)
                            {
                                realm.Write(() =>
                                {
                                    realm.Remove(data);
                                    data = null;
                                });
                            }
                        };

                    }

                    // await BlobCache.LocalMachine.InsertObject<CustomerAddressDto>(docId, deleiveryAddress, DateTimeOffset.Now.AddYears(2));
                    using var realm1 = RealmService.GetRealm();
                    realm1.Write(() =>
                    {
                        realm1.Add(deleiveryAddress.ToModel(), update: true);
                    });

                    return true;
                }
                catch (KeyNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }

            }
            catch (Exception ex)
            {
                ex.Track();

            }
            return false;
        }
        public async Task<bool> DeleteRemoteDeliveryAddress(Priority priority, bool syncLocal, int customerAddressId)
        {
            try
            {

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    ResponseModel<object> deleteDeliveryAddressResponse = new ResponseModel<object>();
                    Task<ResponseModel<object>> deleteDeliveryAddressResponseTask;
                Retry1:
                    switch (priority)
                    {
                        case Priority.Background:
                            deleteDeliveryAddressResponseTask = _apiService.Background.DeleteDeliveryAddress(customerAddressId, Settings.AccessToken);
                            break;
                        case Priority.UserInitiated:
                            deleteDeliveryAddressResponseTask = _apiService.UserInitiated.DeleteDeliveryAddress(customerAddressId, Settings.AccessToken);
                            break;
                        case Priority.Speculative:
                            deleteDeliveryAddressResponseTask = _apiService.Speculative.DeleteDeliveryAddress(customerAddressId, Settings.AccessToken);
                            break;
                        default:
                            deleteDeliveryAddressResponseTask = _apiService.UserInitiated.DeleteDeliveryAddress(customerAddressId, Settings.AccessToken);
                            break;
                    }

                    try
                    {

                        deleteDeliveryAddressResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await deleteDeliveryAddressResponseTask);

                        Debug.WriteLine("customer response : " + Newtonsoft.Json.JsonConvert.SerializeObject(deleteDeliveryAddressResponse));
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        deleteDeliveryAddressResponse = await ex.GetContentAsAsync<ResponseModel<object>>();
                        if (deleteDeliveryAddressResponse != null && deleteDeliveryAddressResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Updaing customers.",ex);
                            }
                        }
                        //ex.LogError("customer", "CreateOrUpdate", customer);
                        return false;
                    }


                    if (deleteDeliveryAddressResponse != null && deleteDeliveryAddressResponse.success)
                    {
                        DeleteDeliveryAddress(customerAddressId.ToString());
                        return deleteDeliveryAddressResponse.success;
                    }
                    else if (deleteDeliveryAddressResponse != null && deleteDeliveryAddressResponse.error != null && deleteDeliveryAddressResponse.error.message != null)
                    {
                        if (priority != Priority.Background)
                        {
                            App.Instance.Hud.DisplayToast(deleteDeliveryAddressResponse.error.message, Colors.Red, Colors.White);
                        }
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
            }
            return false;
        }

        public bool DeleteDeliveryAddress(string deliveryAddressId)
        {
            try
            {
                if (!string.IsNullOrEmpty(deliveryAddressId))
                {
                    int id = 0;
                    if (int.TryParse(deliveryAddressId,out id))
                    {
                        using var realm1 = RealmService.GetRealm();
                        var data = realm1.All<CustomerAddressDB>().Where(a => a.Id == id);

                        realm1.Write(() =>
                        {
                            realm1.RemoveRange<CustomerAddressDB>(data);
                        });
                    }
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

        #endregion
    }
}
