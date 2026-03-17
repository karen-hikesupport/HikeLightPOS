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
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.IO;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;


namespace HikePOS.Services
{
    public class ProductServices
    {

        private readonly IApiService<IProductApi> _apiService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;
        private IApiService<ITaxApi> taxApiService;
        private TaxServices taxService;

        public ProductServices(IApiService<IProductApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
            taxApiService = new ApiService<ITaxApi>();
            taxService = new TaxServices(taxApiService);
        }

        ObservableCollection<ProductDto_POS> SetProductDetails(ObservableCollection<ProductDto_POS> products)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var cat = realm.All<CategoryDB>().ToList();
                var localCategories = cat.Select(a => CategoryDto.FromModel(a)).ToList();
                var productStocks = remoteProductStocksResponse.result.items.Where(x => x.ProductId > 0);
                var localTaxes = new ObservableCollection<TaxDto>(realm.All<TaxDB>().ToList().Select(a=>TaxDto.FromModel(a)));
                var tasks = new List<Task>();

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                   
                    var stock = productStocks.FirstOrDefault(x => x.ProductId == product.Id);
                    if (stock == null)
                    {
                            var localProduct = products.FirstOrDefault(x => x.Id == product.Id);
                            stock = localProduct?.ProductOutlet;
                    }
                    ProductDto_POS parentProduct = null;
                    if (product.ParentId != null)
                        parentProduct = products.FirstOrDefault(x => x.Id == product.ParentId);
                     SetProductStocks(product, parentProduct, stock, localTaxes);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in SetProductDetails : " + ex.Message);
            }
            return products;
        }

        public void SetProductVarients(ProductDto_POS product, IEnumerable<ProductDto_POS> varientProducts)
        {
            try
            {
                product.ProductVarients.Clear();
                varientProducts.ForEach(
                    a =>
                    {
                        product.ProductVarients.Add(new ProductVarientsDto()
                        {
                            ProductVarientId = a.Id,
                            Name = a.Name,
                            BarCode = a.BarCode,
                            sku = a.Sku,
                            VariantAttributesValues = a.VariantAttributesValues
                        });
                    }
                );
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in SetProductAttributes : " + ex.Message);
            }
        }

        public void SetProductAttributes(ProductDto_POS product, IEnumerable<ProductDto_POS> varientProducts)
        {
            try
            {
                var attributes = product.ProductAttributes;
                //Ticket start:#30054 iPad: variant product stock is counted double during composite sale.by rupesh
                //product.ProductVarients.Clear();
                //Ticket end:#30054 .by rupesh
                varientProducts.ForEach(a =>
                    {
                        a.VariantAttributesValues.ForEach(
                            b =>
                            {

                                var attribute = attributes.FirstOrDefault(x => x.AttributeId == b.AttributeId);

                                if (attribute == null)
                                {

                                    attribute = new ProductAttributeDto()
                                    {
                                        AttributeId = b.AttributeId,
                                        Name = b.Name,
                                        ProductAttributeId = b.ProductAttributeId,
                                        Sequence = b.Sequence
                                    };
                                    attributes.Add(attribute);
                                }

                                var attributeValue = attribute.ProductAttributeValues.FirstOrDefault(x => x.AttributeValueId == b.AttributeValueId);

                                if (attributeValue == null)
                                {
                                    attributeValue = new ProductAttributeValueDto()
                                    {
                                        AttributeId = b.AttributeId,
                                        AttributeValueId = b.AttributeValueId,
                                        ProductAttributeId = b.ProductAttributeId,
                                        ProductAttributeValueId = b.ProductAttributeValueId,
                                        Value = b.Value

                                    };
                                    attribute.ProductAttributeValues.Add(attributeValue);
                                }
                            }
                            );

                            //product.ProductVarients.Add(new ProductVarientsDto()
                            //{
                            //    ProductVarientId = a.Id,
                            //    Name = a.Name,
                            //    BarCode = a.BarCode,
                            //    sku = a.Sku,
                            //    VariantAttributesValues = a.VariantAttributesValues
                            //});
                    });

            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in SetProductAttributes : " + ex.Message);
            }
        }

        void SetProductStocks(ProductDto_POS product, ProductDto_POS parentProduct, ProductOutletDto_POS stock, ObservableCollection<TaxDto> taxList)
        {
            try
            {
                if (stock != null)
                {
                    var tax = taxList.FirstOrDefault(x => x.Id == stock.TaxID);
                    stock.TaxName = tax?.Name;
                    stock.TaxRate = tax?.Rate;

                    product.ProductOutlet = stock;

                    product.ProductOutlets.Clear();
                    product.ProductOutlets.Add(stock);

                    //Set Variant Product Stock if any 
                    if (product.ParentId != null && parentProduct != null)
                    {
                        var variantProduct = parentProduct.ProductVarients.FirstOrDefault(x => x.ProductVarientId == product.Id);
                        if (variantProduct != null)
                        {
                            variantProduct.VariantOutlet = stock;
                            variantProduct.VarientOutlets.Clear();
                            variantProduct.VarientOutlets.Add(stock);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in SetProductStocks : " + ex.Message);
            }
        }

        public async Task UpdateProductStocks(ObservableCollection<ProductOutletDto_POS> productStocks, ObservableCollection<TaxDto> taxes)
        {
            try
            {
                if (productStocks != null && productStocks.Count > 0)
                {
                    var updatedProducts = new ObservableCollection<ProductDto_POS>();
                    var localProducts = GetLocalProducts();
                    foreach (var stock in productStocks)
                    {
                        ProductDto_POS product =  GetLocalProduct(stock.ProductId);
                        if (product != null)
                        {
                            ProductDto_POS parentProduct = null;
                            if (product.ParentId != null)
                                parentProduct = localProducts.FirstOrDefault(x => x.Id == product.ParentId);
                            SetProductStocks(product, parentProduct, stock, taxes);
                            updatedProducts.Add(product);

                            //Ticket start:#20890 data sync is not working for variant product.by rupesh
                            //varients on parent product was not updated locally while syncing.Added by rupesh
                            if (parentProduct != null)
                                updatedProducts.Add(parentProduct);
                            //Ticket end:#20890 .by rupesh

                        }
                    }
                    await UpdateLocalProducts(updatedProducts);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in UpdateProductStocks : " + ex.Message);
            }
        }

        public async Task<ObservableCollection<ProductDto_POS>> GetRemoteProducts(Priority priority, bool syncLocal, int outletId, string filter = "", DateTime? lastSyncDate = null)
        {
            ListResponseModel<ProductDto_POS> remoteProductResponse = null;
            Task<ListResponseModel<ProductDto_POS>> productTask;
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();

            if (lastSyncDate == null)
            {
                lastSyncDate = DateTime.MinValue;
            }

            var maxResult = 1;

            ProductFilterModel productFilter = new ProductFilterModel
            {
                modifiedDateTime = lastSyncDate.Value,
                maxResultCount = maxResult,
                skipCount = 0,
                outletId = outletId,
                filter = filter,
                sorting = "0"
            };

            Debug.WriteLine("GetRemoteProducts : " + productFilter.ToJson().ToString());


        Retry:
            switch (priority)
            {
                case Priority.Background:
                    productTask = _apiService.Background.GetAllProducts(productFilter, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    productTask = _apiService.UserInitiated.GetAllProducts(productFilter, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    productTask = _apiService.Speculative.GetAllProducts(productFilter, Settings.AccessToken);
                    break;
                default:
                    productTask = _apiService.UserInitiated.GetAllProducts(productFilter, Settings.AccessToken);
                    break;
            }
            Debug.WriteLine("GetAllProducts API : /api/services/app/productSearch/GetOutletProductsWithMinimumPayload");
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var startDateTime = DateTime.Now;
                    Debug.WriteLine("GetAllProducts API Request Starts at : " + startDateTime);

                    remoteProductResponse = await Policy
                      .Handle<Exception>()
                      .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                      .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                      .ExecuteAsync(async () => await productTask);

                    var endDateTime = DateTime.Now;
                    Debug.WriteLine("GetAllProducts API Request Ends at : " + endDateTime);
                    Debug.WriteLine("GetAllProducts API Request Total Time : " + (endDateTime - startDateTime));

                    Debug.WriteLine("GetAllProducts API Response : " + JsonConvert.SerializeObject(remoteProductResponse));
                }
                catch (ApiException ex)
                {
                    //Get Exception content

                    Debug.WriteLine("Exception in GetRemoteProducts : " + ex.Message + " : " + ex.StackTrace);

                    remoteProductResponse = await ex.GetContentAsAsync<ListResponseModel<ProductDto_POS>>();
                    if (remoteProductResponse != null && remoteProductResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                    Debug.WriteLine("Exception in GetRemoteProducts : " + ex.Message + " : " + ex.StackTrace);
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting products.",ex);
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

            if (remoteProductResponse.success && remoteProductResponse.result != null && remoteProductResponse.result.totalCount > 0)
            {
                var totalProduct = remoteProductResponse.result.totalCount;
                //totalProduct = 1000;
                int totalStep = 1;
                maxResult = 100;
                Debug.WriteLine("Before product load...");

                if (5000 < totalProduct && totalProduct < 10000)
                {
                    maxResult = 400;
                }
                else if (totalProduct > 10000)
                {

                    maxResult = 1000;
                }

                //maxResult = 2000;


                if (totalProduct > maxResult)
                {
                    double tstep = (Convert.ToDouble(totalProduct) / maxResult);
                    totalStep = (int)Math.Ceiling(tstep);
                }

                var startDateTime = DateTime.Now;
                Debug.WriteLine("Overall GetProducts API Request Starts at : " + startDateTime);

                int startpoint = 0;
                do
                {
                    var tasks = new List<Task>();
                    int endpoint = totalStep;// Math.Min(totalStep, startpoint + 10);// startpoint + 100;
                    for (var i = startpoint; i < endpoint; i++)
                    {
                        var skipproductcount = (i * maxResult);

                        if (skipproductcount > totalProduct)
                        {
                            break;
                        }
                        Debug.WriteLine("Request : " + (i + 1));
                        var _productFilter = productFilter.Copy();
                        //Ticket #12999 Products Not Loaded.By Nikhil	
                        _productFilter.skipCount = skipproductcount;
                        //Ticket #12999 End.By Nikhil
                        _productFilter.maxResultCount = maxResult;

                        Task tmptask = GetRemoteProductsByFilter(priority, _productFilter, syncLocal);
                        tasks.Add(tmptask);
                    }
                    await Task.WhenAll(tasks.ToArray());
                    startpoint = endpoint;
                    //List<ProductDto_POS> list = new List<ProductDto_POS>();
                    //foreach (Task<ListResponseModel<ProductDto_POS>> t in tasks)
                    //{
                    //    if (t.Result.result?.items != null)
                    //        list.AddRange(t.Result.result.items);
                    //}
                    //remoteProductResponse.result.items = new ObservableCollection<ProductDto_POS>(remoteProductResponse.result.items.Concat(list));

                } while (startpoint < totalStep);

                var endDateTime = DateTime.Now;
                Debug.WriteLine("Overall GetProducts API Request Ends at : " + endDateTime);
                Debug.WriteLine("Overall GetProducts API Request Total Time : " + (endDateTime - startDateTime));


                //Ticket #13341 Variant Products not appearing correctly in outlet. By Nikhil
                startDateTime = DateTime.Now;
                //Debug.WriteLine("SetProductDetails Starts at : " + startDateTime);
                //var dbProducts = await GetLocalProducts();
                var dbProducts = remoteProductResponse.result.items;
                Debug.WriteLine("dbProducts : " + dbProducts.Count());
            //    dbProducts = await SetProductDetails(dbProducts);
                endDateTime = DateTime.Now;
                //Debug.WriteLine("SetProductDetails Ends at : " + endDateTime);
                Debug.WriteLine("SetProductDetails Total Time : " + (endDateTime - startDateTime));

                if (syncLocal && dbProducts != null && dbProducts.Count > 0)
                {
                    startDateTime = DateTime.Now;
                    //Debug.WriteLine("AllProducts DB Update Starts at : " + startDateTime);
                  //  await UpdateLocalProducts(dbProducts);
                    endDateTime = DateTime.Now;
                    //Debug.WriteLine("AllProducts DB Update Ends at : " + endDateTime);
                    Debug.WriteLine("AllProducts DB Update Total Time : " + (endDateTime - startDateTime));

                }
                //Ticket #13341 End.By Nikhil

                return remoteProductResponse.result.items;
            }
            else
            {
                if (remoteProductResponse != null && remoteProductResponse.error != null && remoteProductResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        Extensions.ServerMessage(remoteProductResponse.error.message);
                    }

                    return null;
                }

                return null;
            }
        }

        public async Task<ObservableCollection<ProductDto_POS>> GetRemoteProductsLambda(Priority priority, bool syncLocal, int outletId, string filter = "", DateTime? lastSyncDate = null, ObservableCollection<TaxDto> taxes = null)
        {
            ListResponseModel<ProductDto_POS> remoteProductResponse = null;
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();

            if (lastSyncDate == null)
            {
                lastSyncDate = DateTime.MinValue;
            }

            var maxResult = 1;

            ProductFilterModel productFilter = new ProductFilterModel
            {
                modifiedDateTime = lastSyncDate.Value,
                maxResultCount = maxResult,
                skipCount = 0,
                outletId = outletId,
                filter = filter,
                sorting = "0"
            };

            Debug.WriteLine("GetRemoteProductsLambda : " + productFilter.ToJson().ToString());


        Retry:
            var httpService = new HttpService();
            Debug.WriteLine("GetAllProducts API : /y0a4jgvqo6.execute-api.us-west-2.amazonaws.com/Prod/GetHikeProducts?dbType");
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    using (var httpClient = httpService.httpClient())
                    {
                        //Ticket end:#25014.by rupesh
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", ""));
                        httpClient.Timeout = new TimeSpan(0, 20, 0);
                        var data = JsonConvert.SerializeObject(productFilter);
                        var content = new StringContent(data, Encoding.UTF8, "application/json");

                        var startDateTime = DateTime.Now;
                        var urlPath = string.Format("https://y0a4jgvqo6.execute-api.us-west-2.amazonaws.com/Prod/GetHikeProducts?dbType={0}&dbname={1}&lastSyncDate={2}&onlyCount=1&outletId={3}&liveMode=true", Settings.CurrentDatabaseType, Settings.CurrentDatabaseName, productFilter.modifiedDateTime.ToString(CultureInfo.InvariantCulture), productFilter.outletId);
                        var response = await httpClient.GetAsync(urlPath).ConfigureAwait(false);
                        var endDateTime = DateTime.Now;

                        Debug.WriteLine("GetAllProducts API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetAllProducts API Request Total Time : " + (endDateTime - startDateTime));

                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    var productResponse = _serializer.Deserialize<ListResult<ProductDto_POS>>(jsonSTR);
                                    remoteProductResponse = new ListResponseModel<ProductDto_POS>
                                    {
                                        success = true,
                                        result = new ListResultDto<ProductDto_POS>
                                        {
                                            totalCount = productResponse.Count,
                                            items = productResponse.items ?? new ObservableCollection<ProductDto_POS>()
                                        }
                                    };

                                }
                            }
                        }

                        Debug.WriteLine("GetAllProducts API Response : " + JsonConvert.SerializeObject(remoteProductResponse));
                    }
                }
                catch (ApiException ex)
                {
                    //Get Exception content

                    Debug.WriteLine("Exception in GetRemoteProductsLambda : " + ex.Message + " : " + ex.StackTrace);

                    remoteProductResponse = await ex.GetContentAsAsync<ListResponseModel<ProductDto_POS>>();
                    if (remoteProductResponse != null && remoteProductResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                    Debug.WriteLine("Exception in GetRemoteProductsLambda : " + ex.Message + " : " + ex.StackTrace);
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
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting products.", ex);
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

            if (remoteProductResponse != null && remoteProductResponse.success && remoteProductResponse.result != null && remoteProductResponse.result.totalCount > 0)
            {
                var totalProduct = remoteProductResponse.result.totalCount;
                WeakReferenceMessenger.Default.Send(new Messenger.ProductProgressMessenger(totalProduct));
                //totalProduct = 1000;
                int totalStep = 1;
                maxResult = 100;
                Debug.WriteLine("Before product load...");

                if (5000 < totalProduct && totalProduct < 10000)
                {
                    maxResult = 400;
                }
                else if (totalProduct > 10000)
                {

                    maxResult = 1000;
                }

                //maxResult = 2000;


                if (totalProduct > maxResult)
                {
                    double tstep = (Convert.ToDouble(totalProduct) / maxResult);
                    totalStep = (int)Math.Ceiling(tstep);
                }

                var startDateTime = DateTime.Now;
                Debug.WriteLine("Overall GetProducts API Request Starts at : " + startDateTime);
                if (taxes == null || (taxes != null && taxes.Count <= 0))
                    taxes = taxService.GetLocalTaxes();
                int startpoint = 0;
                do
                {
                    var tasks = new List<Task>();
                    int endpoint = totalStep;// Math.Min(totalStep, startpoint + 10);// startpoint + 100;
                    for (var i = startpoint; i < endpoint; i++)
                    {
                        var skipproductcount = (i * maxResult);

                        if (skipproductcount > totalProduct)
                        {
                            break;
                        }
                        Debug.WriteLine("Request : " + (i + 1));
                        var _productFilter = productFilter.Copy();
                        //Ticket #12999 Products Not Loaded.By Nikhil	
                        _productFilter.skipCount = skipproductcount;
                        //Ticket #12999 End.By Nikhil
                        _productFilter.maxResultCount = maxResult;

                        Task tmptask = GetRemoteProductsByFilterLambda(priority, _productFilter, syncLocal,taxes);
                        tasks.Add(tmptask);
                    }
                    await Task.WhenAll(tasks.ToArray());
                    startpoint = endpoint;
                    //List<ProductDto_POS> list = new List<ProductDto_POS>();
                    //foreach (Task<ListResponseModel<ProductDto_POS>> t in tasks)
                    //{
                    //    if (t.Result.result?.items != null)
                    //        list.AddRange(t.Result.result.items);
                    //}
                    //remoteProductResponse.result.items = new ObservableCollection<ProductDto_POS>(remoteProductResponse.result.items.Concat(list));

                } while (startpoint < totalStep);

                var endDateTime = DateTime.Now;
                Debug.WriteLine("Overall GetProducts API Request Ends at : " + endDateTime);
                Debug.WriteLine("Overall GetProducts API Request Total Time : " + (endDateTime - startDateTime));


                //Ticket #13341 Variant Products not appearing correctly in outlet. By Nikhil
                startDateTime = DateTime.Now;
                //Debug.WriteLine("SetProductDetails Starts at : " + startDateTime);
                //var dbProducts = await GetLocalProducts();
                var dbProducts = remoteProductResponse.result.items;
                Debug.WriteLine("dbProducts : " + dbProducts.Count());
                //    dbProducts = await SetProductDetails(dbProducts);
                endDateTime = DateTime.Now;
                //Debug.WriteLine("SetProductDetails Ends at : " + endDateTime);
                Debug.WriteLine("SetProductDetails Total Time : " + (endDateTime - startDateTime));

                if (syncLocal && dbProducts != null && dbProducts.Count > 0)
                {
                    startDateTime = DateTime.Now;
                    //Debug.WriteLine("AllProducts DB Update Starts at : " + startDateTime);
                    //  await UpdateLocalProducts(dbProducts);
                    endDateTime = DateTime.Now;
                    //Debug.WriteLine("AllProducts DB Update Ends at : " + endDateTime);
                    Debug.WriteLine("AllProducts DB Update Total Time : " + (endDateTime - startDateTime));

                }
                //Ticket #13341 End.By Nikhil

                return remoteProductResponse.result.items;
            }
            else
            {
                if (remoteProductResponse != null && remoteProductResponse.error != null && remoteProductResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        Extensions.ServerMessage(remoteProductResponse.error.message);
                    }

                    return null;
                }

                return null;
            }
        }

        private JsonSerializer _serializer = new JsonSerializer();
        public async Task<ListResponseModel<ProductDto_POS>> GetRemoteProductsByFilter(Priority priority, ProductFilterModel productFilter, bool syncLocal)
        {

            ListResponseModel<ProductDto_POS> tmpremoteProductResponse = new ListResponseModel<ProductDto_POS>();

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

                try
                {
                    var httpService = new HttpService();
                    //Ticket start:#25014 Hike store - iPad - Product "HOD-H4831SC" not getting searched.by rupesh
                    using (var httpClient = httpService.httpClient())
                    {
                        //Ticket end:#25014.by rupesh
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", ""));
                        httpClient.Timeout = new TimeSpan(0, 20, 0);
                        var data = JsonConvert.SerializeObject(productFilter);
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

                        var chunkStartDateTime = DateTime.Now;
                        var response = await httpClient.PostAsync(url + "/api/services/app/productSearch/GetOutletProductsWithMinimumPayload", content).ConfigureAwait(false);
                        var chunkEndDateTime = DateTime.Now;

                        Debug.WriteLine("GetProducts Chunks API Request Total Time : " + productFilter.maxResultCount + " : " + (chunkEndDateTime - chunkStartDateTime));

                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    tmpremoteProductResponse = _serializer.Deserialize<ListResponseModel<ProductDto_POS>>(jsonSTR);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (tmpremoteProductResponse != null && tmpremoteProductResponse.result != null && tmpremoteProductResponse.result.items != null)
                    {
                        Debug.WriteLine("Exception in GetRemoteProductsByFilter : " + tmpremoteProductResponse.result.items.Count + " : " + ex.Message + " : " + ex.StackTrace);
                    }
                    ex.Track();
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
                            Debug.WriteLine("Error on:" + ex.Message.ToString());
                            App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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


            if (tmpremoteProductResponse.success && tmpremoteProductResponse.result != null && tmpremoteProductResponse.result.items != null)
            {
                //Ticket #12422 Start : Products missing issue. By Nikhil	
                var remoteProducts = tmpremoteProductResponse.result.items;

               if (syncLocal && tmpremoteProductResponse.result != null && tmpremoteProductResponse.result.items.Count > 0)
                {
                    //Ticket #13733 Wrong product count After Activating/Deactivating Discount Offers. By Nikhil
                    //Set product stock from local db before updating products;because new products response doesnt have stock details
                    var tasks = new List<Task>();
                    foreach (ProductDto_POS product in remoteProducts)
                    {
                        var task = Task.Run(() =>
                        {

                            // var localProduct = await GetLocalProduct(product.Id);
                            var stock = remoteProductStocksResponse.result.items.FirstOrDefault(x => x.ProductId == product.Id);
                            if (stock != null)
                            {
                                if (stock.TaxID != null)
                                {
                                    var tax = taxService.GetLocalTaxById(stock.TaxID.Value);
                                    stock.TaxName = tax?.Name;
                                    stock.TaxRate = tax?.Rate;
                                }
                                if (product.HasVarients)
                                {
                                    var stocks = remoteProductStocksResponse.result.items.Where(x => x.ParentProductId == product.Id);
                                    stock.OnHandstock = stocks.Sum(x => x.OnHandstock);
                                    stock.Committedstock = stocks.Sum(x => x.Committedstock);

                                }

                                product.ProductOutlet = stock;
                                product.ProductOutlets.Clear();
                                product.ProductOutlets.Add(stock);
                            }
                        });
                        tasks.Add(task);
                    }
                    //Ticket #13733 End.By Nikhil
                    await Task.WhenAll(tasks);
                    await UpdateLocalProducts(remoteProducts);
                }
                //Ticket #12422 End. By Nikhil

                return tmpremoteProductResponse;
            }
            else
            {
                if (tmpremoteProductResponse != null && tmpremoteProductResponse.error != null && tmpremoteProductResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(tmpremoteProductResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }
        public async Task<ListResponseModel<ProductDto_POS>> GetRemoteProductsByFilterLambda(Priority priority, ProductFilterModel productFilter, bool syncLocal, ObservableCollection<TaxDto> taxes=null)
        {

            ListResponseModel<ProductDto_POS> tmpremoteProductResponse = new ListResponseModel<ProductDto_POS>();

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

                try
                {
                    var httpService = new HttpService();
                    //Ticket start:#25014 Hike store - iPad - Product "HOD-H4831SC" not getting searched.by rupesh
                    using (var httpClient = httpService.httpClient())
                    {
                        httpClient.Timeout = new TimeSpan(0, 20, 0);
                        var urlPath = string.Format("https://y0a4jgvqo6.execute-api.us-west-2.amazonaws.com/Prod/GetHikeProducts?dbType={0}&dbname={1}&lastSyncDate={2}&skip={3}&take={4}&outletId={5}&liveMode=true", Settings.CurrentDatabaseType, Settings.CurrentDatabaseName, productFilter.modifiedDateTime.ToString(CultureInfo.InvariantCulture), productFilter.skipCount, productFilter.maxResultCount, productFilter.outletId);
                        var chunkStartDateTime = DateTime.Now;
                        var response = await httpClient.GetAsync(urlPath).ConfigureAwait(false);
                        var chunkEndDateTime = DateTime.Now;
                        Debug.WriteLine("GetProducts Chunks API Request Total Time : " + productFilter.maxResultCount + " : " + (chunkEndDateTime - chunkStartDateTime));

                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    var productResponse = _serializer.Deserialize<ListResult<ProductDto_POS>>(jsonSTR);
                                    tmpremoteProductResponse = new ListResponseModel<ProductDto_POS>
                                    {
                                        success = true,
                                        result = new ListResultDto<ProductDto_POS>
                                        {
                                            totalCount = productResponse.Count,
                                            items = productResponse.items
                                        }
                                    };

                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (tmpremoteProductResponse != null && tmpremoteProductResponse.result != null && tmpremoteProductResponse.result.items != null)
                    {
                        Debug.WriteLine("Exception in GetRemoteProductsByFilterLambda : " + tmpremoteProductResponse.result.items.Count + " : " + ex.Message + " : " + ex.StackTrace);
                    }
                    ex.Track();
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
                            Debug.WriteLine("Error on:" + ex.Message.ToString());
                            App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            WeakReferenceMessenger.Default.Send(new Messenger.ProductProgressMessenger(productFilter.maxResultCount));

            if (tmpremoteProductResponse.success && tmpremoteProductResponse.result != null && tmpremoteProductResponse.result.items != null)
            {
                //Ticket #12422 Start : Products missing issue. By Nikhil	
                var remoteProducts = tmpremoteProductResponse.result.items;

                if (syncLocal && tmpremoteProductResponse.result != null && tmpremoteProductResponse.result.items.Count > 0)
                {
                 // _= Task.Factory.StartNew(async () =>
                 //   {
                        //Ticket #13733 Wrong product count After Activating/Deactivating Discount Offers. By Nikhil
                        //Set product stock from local db before updating products;because new products response doesnt have stock details
                        var tasks = new List<Task>();
                        var task = Task.Factory.StartNew(() =>
                        {
                            foreach (ProductDto_POS product in remoteProducts)
                            {
                              // var localProduct = await GetLocalProduct(product.Id);
                              var stock = remoteProductStocksResponse.result.items.FirstOrDefault(x => x.ProductId == product.Id);
                              if (stock != null)
                              {
                                    if (stock.TaxID != null && taxes != null)
                                    {
                                        var tax = taxes.FirstOrDefault(a=>a.Id == stock.TaxID.Value);
                                        stock.TaxName = tax?.Name;
                                        stock.TaxRate = tax?.Rate;
                                    }
                                    if (product.HasVarients)
                                    {
                                        var stocks = remoteProductStocksResponse.result.items.Where(x => x.ParentProductId == product.Id);
                                        stock.OnHandstock = stocks.Sum(x => x.OnHandstock);
                                        stock.Committedstock = stocks.Sum(x => x.Committedstock);

                                    }

                                    product.ProductOutlet = stock;
                                    product.ProductOutlets.Clear();
                                    product.ProductOutlets.Add(stock);
                              }
                                else
                                {
                                    var tempProduct = GetLocalProduct(product.Id);
                                    stock = tempProduct?.ProductOutlet;
                                    if (stock != null)
                                    {
                                        product.ProductOutlet = stock;
                                        product.ProductOutlets.Clear();
                                        product.ProductOutlets.Add(stock);
                                    }
                                }
                            }
                        });
                        tasks.Add(task);
                        //Ticket #13733 End.By Nikhil
                        await Task.WhenAll(tasks);
                        await UpdateLocalProducts(remoteProducts);
                   // });
                }
                //Ticket #12422 End. By Nikhil

                return tmpremoteProductResponse;
            }
            else
            {
                if (tmpremoteProductResponse != null && tmpremoteProductResponse.error != null && tmpremoteProductResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(tmpremoteProductResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }

        public ObservableCollection<ProductDto_POS> GetLocalProducts()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                return new ObservableCollection<ProductDto_POS>(realm.All<ProductDB>().ToList().Select(a => ProductDto_POS.FromModel(a)));
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }
               
        public ProductDto_POS GetLocalProduct(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var product = ProductDto_POS.FromModel(realm.Find<ProductDB>(id));
                return product;
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public ProductDB GetLocalProductDB(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var product =realm.Find<ProductDB>(id);
                return product;
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public ProductDto_POS GetLocalProductSync(int id)
        {
            try
            {
                //return BlobCache.LocalMachine.GetObject<ProductDto_POS>(nameof(ProductDto_POS) + "_" + id.ToString()).Wait();
                var realm = RealmService.GetRealm();
                var product = ProductDto_POS.FromModel(realm.Find<ProductDB>(id));
                return product;
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public ObservableCollection<ProductDto_POS> GetLocalProductsByCategoryId(int categoryId)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var catId = "," + categoryId + ",";
                var saleId = "," + (int)SellsChannel.PointOfSale + ",";
                var products = realm.All<ProductDB>().Where(x => x.IsActive && !x.DisableSellIndividually && (x.ParentId == 0 || x.ParentId == null) && x.ProductCategoriesDesc.Contains(catId) && x.SalesChannelsStr.Contains(saleId)).ToList().Select(a => ProductDto_POS.FromModel(a));
                return new ObservableCollection<ProductDto_POS>(products);
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public ObservableCollection<ProductDto_POS> GetLocalSearchProduct(string name_barcode_sku)
        {
            try
            {
                if (string.IsNullOrEmpty(name_barcode_sku))
                {
                    return new ObservableCollection<ProductDto_POS>();
                }
                using var realm = RealmService.GetRealm();
                var products = realm.All<ProductDB>().Where(x => x.Name.Contains(name_barcode_sku) || x.Sku == name_barcode_sku || x.BarCode == name_barcode_sku || x.BranName == name_barcode_sku).ToList().Select(a => ProductDto_POS.FromModel(a));
                return new ObservableCollection<ProductDto_POS>(products);
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public List<ProductDto_POS> GetLocalSearchProductByFilter(string name_barcode_sku)
        {
            try
            {
                if (string.IsNullOrEmpty(name_barcode_sku))
                {
                    return new List<ProductDto_POS>();
                }
                //name_barcode_sku = name_barcode_sku.ToLower();
                var realm = RealmService.GetRealm();
                var saleId = "," + (int)SellsChannel.PointOfSale + ",";
                var products = realm.All<ProductDB>().Where(x => x.IsActive && !x.DisableSellIndividually && x.SalesChannelsStr.Contains(saleId) && (x.Name.Contains(name_barcode_sku, StringComparison.OrdinalIgnoreCase)
                                                                 || (x.Sku != null && x.Sku != "" && x.Sku.Contains(name_barcode_sku, StringComparison.OrdinalIgnoreCase))
                                                                 || (x.BarCode != null && x.BarCode != "" && x.BarCode.Contains(name_barcode_sku, StringComparison.OrdinalIgnoreCase))
                                                                 || (x.BranName != null && x.BranName != "" && x.BranName.Contains(name_barcode_sku, StringComparison.OrdinalIgnoreCase)))
                                                             ).ToList().Take(100).Select(a => ProductDto_POS.FromModel(a));
              
                return products.ToList();
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public List<ProductDto_POS> GetLocalProductVariants(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var products = realm.All<ProductDB>().Where(x => x.ParentId != null && x.ParentId == id && x.IsActive == true).ToList().Select(a => ProductDto_POS.FromModel(a));
                return products.ToList();
            }
            catch (KeyNotFoundException ex)
            {
                ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        ListResponseModel<ProductOutletDto_POS> remoteProductStocksResponse;

        public async Task<ObservableCollection<ProductOutletDto_POS>> GetRemoteProductStocks(Priority priority, bool syncLocal, int outletId, string filter = "", DateTime? lastSyncDate = null)
        {
            remoteProductStocksResponse = new ListResponseModel<ProductOutletDto_POS>();
            Task<ListResponseModel<ProductOutletDto_POS>> productTask;
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();


            if (lastSyncDate == null)
            {
                lastSyncDate = DateTime.MinValue;
            }



            // string stringDate = lastSyncDate.Value.ToString("o", CultureInfo.InvariantCulture);

            //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
            string stringlastSyncDate = lastSyncDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ", CultureInfo.InvariantCulture);
            //Ticket end:#34015 .by rupesh

            Debug.WriteLine("string date : " + stringlastSyncDate);
            Debug.WriteLine("lastSyncDate  : " + lastSyncDate.Value.ToString());

        Retry:
            switch (priority)
            {
                case Priority.Background:
                    productTask = _apiService.Background.GetOutletProductStock(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    productTask = _apiService.UserInitiated.GetOutletProductStock(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    productTask = _apiService.Speculative.GetOutletProductStock(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
                default:
                    productTask = _apiService.UserInitiated.GetOutletProductStock(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
            }
            Debug.WriteLine("GetOutletProductStock API : /api/services/app/productSearch/GetProductOutletWithMinimumPayload");
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var startDateTime = DateTime.Now;
                    Debug.WriteLine("GetOutletProductStock API Request Starts at : " + startDateTime);
                    remoteProductStocksResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await productTask);

                    var endDateTime = DateTime.Now;
                    Debug.WriteLine("GetOutletProductStock API Request Ends at : " + endDateTime);
                    Debug.WriteLine("GetOutletProductStock API Request Total Time : " + (endDateTime - startDateTime));
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    remoteProductStocksResponse = await ex.GetContentAsAsync<ListResponseModel<ProductOutletDto_POS>>();
                    if (remoteProductStocksResponse != null && remoteProductStocksResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting product stocks.", ex);
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

            if (remoteProductStocksResponse != null && remoteProductStocksResponse.success && remoteProductStocksResponse.result != null && remoteProductStocksResponse.result.totalCount > 0)
            {
                //if (syncLocal)
                //{
                //    var startDateTime = DateTime.Now;
                //    await UpdateLocalProductStocks(remoteProductStocksResponse.result.items.Where(x => x.ProductId > 0));
                //    var endDateTime = DateTime.Now;
                //    Debug.WriteLine("Product Stocks DB Store Total Time : " + (endDateTime - startDateTime));
                //}
                return remoteProductStocksResponse.result.items;
            }
            else
            {
                if (remoteProductStocksResponse != null && remoteProductStocksResponse.error != null && remoteProductStocksResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(remoteProductStocksResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }

        public async Task<ObservableCollection<ProductOutletDto_POS>> GetRemoteProductStocksLambda(Priority priority, bool syncLocal, int outletId, string filter = "", DateTime? lastSyncDate = null)
        {
            remoteProductStocksResponse = new ListResponseModel<ProductOutletDto_POS>();
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();

            if (lastSyncDate == null)
            {
                lastSyncDate = DateTime.MinValue;
            }
            //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
            string stringDate = lastSyncDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ", CultureInfo.InvariantCulture);
            //Ticket end:#34015 .by rupesh

            var maxResult = 1;

            ProductFilterModel productFilter = new ProductFilterModel
            {
                modifiedDateTime = lastSyncDate.Value,
                maxResultCount = maxResult,
                skipCount = 0,
                outletId = outletId,
                filter = filter,
                sorting = "0"
            };

        Retry:
            var httpService = new HttpService();
            Debug.WriteLine("GetOutletProductStock API : /api/services/app/productSearch/GetProductOutletWithMinimumPayload");
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                 try
                {
                    using (var httpClient = httpService.httpClient())
                    {
                        //Ticket end:#25014.by rupesh
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", ""));
                        httpClient.Timeout = new TimeSpan(0, 20, 0);

                        var data = JsonConvert.SerializeObject(productFilter);
                        var content = new StringContent(data, Encoding.UTF8, "application/json");

                        var startDateTime = DateTime.Now;
                        Debug.WriteLine("GetOutletProductStock API Request Starts at : " + startDateTime);
                        var urlPath = string.Format("https://y0a4jgvqo6.execute-api.us-west-2.amazonaws.com/Prod/GetHikeProductOutlets?dbType={0}&dbname={1}&lastSyncDate={2}&skip={3}&take={4}&outletId={5}&liveMode=true&onlyCount=1", Settings.CurrentDatabaseType, Settings.CurrentDatabaseName, productFilter.modifiedDateTime.ToString(CultureInfo.InvariantCulture), productFilter.skipCount, productFilter.maxResultCount, productFilter.outletId);
                        var response = await httpClient.GetAsync(urlPath).ConfigureAwait(false);
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    var productResponse = _serializer.Deserialize<ListResult<ProductOutletDto_POS>>(jsonSTR);
                                    remoteProductStocksResponse = new ListResponseModel<ProductOutletDto_POS>
                                    {
                                        success = true,
                                        result = new ListResultDto<ProductOutletDto_POS>
                                        {
                                            totalCount = productResponse.Count,
                                            items = productResponse.items ?? new ObservableCollection<ProductOutletDto_POS>()
                                        }
                                    };
                                }
                            }


                        }


                        var endDateTime = DateTime.Now;
                        Debug.WriteLine("GetOutletProductStock API Request Ends at : " + endDateTime);
                        Debug.WriteLine("GetOutletProductStock API Request Total Time : " + (endDateTime - startDateTime));

                        //Debug.WriteLine("GetOutletProductStock API Response : " + JsonConvert.SerializeObject(remoteProductStocksResponse));
                    }
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    remoteProductStocksResponse = await ex.GetContentAsAsync<ListResponseModel<ProductOutletDto_POS>>();
                    if (remoteProductStocksResponse != null && remoteProductStocksResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting product stocks.", ex);
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

            if (remoteProductStocksResponse.success && remoteProductStocksResponse.result != null && remoteProductStocksResponse.result.totalCount > 0)
            {
                var totalProduct = remoteProductStocksResponse.result.totalCount;
                int totalStep = 1;
                maxResult = 100;


                if (5000 < totalProduct && totalProduct < 10000)
                {
                    maxResult = 400;
                }
                else if (totalProduct > 10000)
                {
                    maxResult = 1000;
                }

                if (totalProduct > maxResult)
                {
                    double tstep = (Convert.ToDouble(totalProduct) / maxResult);
                    totalStep = (int)Math.Ceiling(tstep);
                }

                int startpoint = 0;
                do
                {
                    var tasks = new List<Task>();
                    int endpoint = totalStep;// Math.Min(totalStep, startpoint + 10);// startpoint + 100;
                    for (var i = startpoint; i < endpoint; i++)
                    {
                        var skipproductcount = (i * maxResult);

                        if (skipproductcount > totalProduct)
                        {
                            break;
                        }
                        var _productFilter = productFilter.Copy();
                        _productFilter.skipCount = skipproductcount;
                        _productFilter.maxResultCount = maxResult;
                        Task tmptask = GetRemoteProductStocksByFilterLambda(priority, _productFilter);
                        tasks.Add(tmptask);
                    }
                    await Task.WhenAll(tasks.ToArray());

                    //Debug.WriteLine("Complete steps: " + startpoint + " To: " + endpoint);
                    startpoint = endpoint;
                    List<ProductOutletDto_POS> list = new List<ProductOutletDto_POS>();
                    foreach (Task<ListResponseModel<ProductOutletDto_POS>> t in tasks)
                    {
                        if (t.Result.result?.items != null)
                            list.AddRange(t.Result.result.items);
                    }
                    remoteProductStocksResponse.result.items = new ObservableCollection<ProductOutletDto_POS>(remoteProductStocksResponse.result.items.Concat(list));

                } while (startpoint < totalStep);

                //if (syncLocal)
                //{
                //    var startDateTime = DateTime.Now;
                // //   await UpdateLocalProductStocks(remoteProductStocksResponse.result.items.Where(x => x.ProductId > 0));
                //    var endDateTime = DateTime.Now;
                //    Debug.WriteLine("Product Stocks DB Store Total Time : " + (endDateTime - startDateTime));
                //}
                return remoteProductStocksResponse.result.items;
            }
            else
            {
                if (remoteProductStocksResponse != null && remoteProductStocksResponse.error != null && remoteProductStocksResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(remoteProductStocksResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }

        public async Task<ListResponseModel<ProductOutletDto_POS>> GetRemoteProductStocksByFilterLambda(Priority priority, ProductFilterModel productFilter)
        {

            ListResponseModel<ProductOutletDto_POS> tmpremoteProductStocksResponse = new ListResponseModel<ProductOutletDto_POS>();


            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var httpService = new HttpService();
                    using (var httpClient = httpService.httpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); //new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                        httpClient.Timeout = new TimeSpan(0, 20, 0);
                        var data = JsonConvert.SerializeObject(productFilter);
                        var content = new StringContent(data, Encoding.UTF8, "application/json");
                        var urlPath = string.Format("https://y0a4jgvqo6.execute-api.us-west-2.amazonaws.com/Prod/GetHikeProductOutlets?dbType={0}&dbname={1}&lastSyncDate={2}&skip={3}&take={4}&outletId={5}&liveMode=true", Settings.CurrentDatabaseType, Settings.CurrentDatabaseName, productFilter.modifiedDateTime.ToString(CultureInfo.InvariantCulture), productFilter.skipCount, productFilter.maxResultCount, productFilter.outletId);
                        var chunkStartDateTime = DateTime.Now;
                        var response = await httpClient.GetAsync(urlPath).ConfigureAwait(false);
                        var chunkEndDateTime = DateTime.Now;


                        //var response = await httpClient.PostAsync(url + "/api/services/app/productSearch/GetOutletProducts", content).ConfigureAwait(false);
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream))
                            {
                                using (var jsonSTR = new JsonTextReader(reader))
                                {
                                    var productStockResponse = _serializer.Deserialize<ListResult<ProductOutletDto_POS>>(jsonSTR);
                                    tmpremoteProductStocksResponse = new ListResponseModel<ProductOutletDto_POS>
                                    {
                                        success = true,
                                        result = new ListResultDto<ProductOutletDto_POS>
                                        {
                                            totalCount = productStockResponse.Count,
                                            items = productStockResponse.items
                                        }
                                    };

                                }
                            }


                        }
                        Debug.WriteLine("GetProductStocks Chunks API Request Total Time : " + productFilter.maxResultCount + " : " + (chunkEndDateTime - chunkStartDateTime));

                    }

                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("Error on:" + productFilter.skipCount + ":" + JsonConvert.SerializeObject(ex));
                    ex.Track();
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

                            //Need to log this error to backend
                            //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Color.Red, Color.White);
                            App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            if (tmpremoteProductStocksResponse.success && tmpremoteProductStocksResponse.result != null && tmpremoteProductStocksResponse.result.items != null)
            {
                //if (remoteProductStocksResponse != null && remoteProductStocksResponse.result != null && remoteProductStocksResponse.result.items != null)
                //{
                //    remoteProductStocksResponse.result.items = new ObservableCollection<ProductOutletDto_POS>(remoteProductStocksResponse.result.items.Concat(tmpremoteProductStocksResponse.result.items));
                //    Debug.WriteLine("Concated count :" + remoteProductStocksResponse.result.items.Count);

                //}
                return tmpremoteProductStocksResponse;
            }
            else
            {
                if (tmpremoteProductStocksResponse != null && tmpremoteProductStocksResponse.error != null && tmpremoteProductStocksResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(tmpremoteProductStocksResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }

        public bool UpdateLocalProductStocks(IEnumerable<ProductOutletDto_POS> productstocks)
        {
            try
            {
                if (productstocks != null)
                {
                    int total = productstocks.Count();
                    int index = 0;
                    foreach (var item in productstocks)
                    {
                        var product =  GetLocalProduct(item.ProductId);
                        if (product != null)
                        {
                            product.ProductOutlet = item;
                            product.ProductOutlets.Clear();
                            product.ProductOutlets.Add(item);

                            //Set Variant Product Stock if any 
                            if (product.ParentId != null)
                            {
                                var parentProduct =  GetLocalProduct(product.ParentId.Value);
                                if (parentProduct != null)
                                {
                                    var variantProduct = parentProduct.ProductVarients.FirstOrDefault(x => x.ProductVarientId == product.Id);
                                    if (variantProduct != null)
                                    {
                                        variantProduct.VariantOutlet = item;
                                        variantProduct.VarientOutlets.Clear();
                                        variantProduct.VarientOutlets.Add(item);

                                        UpdateLocalProduct(parentProduct);
                                    }
                                }
                            }

                            UpdateLocalProduct(product);
                        }
                        index++;
                        Debug.WriteLine("Updating Product Stock : " + index + "/" + total);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in UpdateLocalProductStocks : " + ex.Message + " : " + ex.StackTrace);
                return false;
            }
        }

        public bool AddAllOutletProductStocks(ObservableCollection<ProductOutletDto_POS> productstocks)
        {
            try
            {
                if (productstocks != null && productstocks.FirstOrDefault() != null)
                {
                    var product = GetLocalProduct(productstocks.FirstOrDefault().ProductId);
                    if (product != null)
                    {
                        product.ProductOutlets = productstocks;
                        UpdateLocalProduct(product);
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

        public async Task<bool> UpdateLocalProducts(ObservableCollection<ProductDto_POS> products)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                await realm.WriteAsync(() =>
                {
                    realm.Add(products.Select(a=>a.ToModel()).ToList(), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in UpdateLocalProducts : " + ex.Message + " : " + ex.StackTrace);
                return false;
            }
        }

        public bool UpdateLocalProduct(ProductDto_POS product)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(product.ToModel(), update: true);
                });
                return true;

            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool UpdateLocalProductOutlet(ProductOutletDto_POS productOutlet)
        {
            try
            {
                if (productOutlet == null)
                    return false;
                
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(productOutlet.ToModel(), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool DeleteLocalProduct(string productId)
        {
            try
            {
                //await BlobCache.LocalMachine.InvalidateObject<ProductDto_POS>(nameof(ProductDto_POS) + "_" + productId);
                using var realm = RealmService.GetRealm();
                var product = realm.All<ProductDB>().FirstOrDefault(a => a.Id.ToString() == productId);
                if (product != null)
                {
                    realm.Write(() =>
                    {
                        realm.Remove(product);
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public ObservableCollection<CategoryDto> GetLocalCategories()
        {
            var realm = RealmService.GetRealm();
            return new ObservableCollection<CategoryDto>(realm.All<CategoryDB>().ToList().Select(a => CategoryDto.FromModel(a)));
        }

        public List<CategoryDto> GetLocalCategoriesByIds(List<int> ids)
        {
            var strids = "";
            if (ids != null && ids.Any())
                strids = "," + string.Join(",", ids) + ",";
            var realm = RealmService.GetRealm();
            // var filter = realm.All<CategoryDB>().ToList();
            return realm.All<CategoryDB>().ToList().Where(a => strids.Contains("," + a.Id + ",")).OrderBy(d => ids.IndexOf(d.Id)).Select(a => CategoryDto.FromModel(a)).ToList();
        }

        public ObservableCollection<CategoryDto> GetLocalCategoriesSync()
        {
            // return new ObservableCollection<CategoryDto>(BlobCache.LocalMachine.GetAllObjects<CategoryDto>().Wait());
            var realm = RealmService.GetRealm();
            return new ObservableCollection<CategoryDto>(realm.All<CategoryDB>().ToList().Select(a => CategoryDto.FromModel(a)));
        }

        public ObservableCollection<CategoryDto> GetLocalActiveCategories()
        {
            var realm = RealmService.GetRealm();
            var Categories = realm.All<CategoryDB>().ToList().Select(a => CategoryDto.FromModel(a));
            return new ObservableCollection<CategoryDto>(Categories.Where(x => x.IsActive == true));
        }

        public CategoryDto GetLocalCategory(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                return CategoryDto.FromModel(realm.Find<CategoryDB>(id));
            }
            catch (KeyNotFoundException ex)
            {
                 ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public bool DeleteLocalCategory(string id)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var delcat = realm.All<CategoryDB>().ToList().FirstOrDefault(a => a.Id.ToString() == id);
                if (delcat != null)
                {
                    realm.Write(() =>
                    {
                        realm.Remove(delcat);
                        delcat = null;
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool UpdateLocalCategories(ObservableCollection<CategoryDto> categories)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(categories.Select(a => a.ToModel()).ToList(), update: true);
                });

                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool UpdateLocalCategory(CategoryDto category)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(category.ToModel(), update: true); 
                });

                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public ProductDto_POS GetLocalProductByBarcode(string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode))
                {
                    return new ProductDto_POS();
                }
                var realm = RealmService.GetRealm();
                var products = realm.All<ProductDB>().Where(x => x.BarCode == barcode).ToList();
                return ProductDto_POS.FromModel(products?.FirstOrDefault());
            }
            catch (KeyNotFoundException ex)
            {
                 ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public async Task<ObservableCollection<ProductOutletDto_POS>> GetRemoteProductStocksByProductId1(Priority priority, int ProductId, bool IsSync = true)
        {

            Debug.WriteLine("GetProductStocks Product ID : " + ProductId.ToString());

            ResponseModel<Stock> productStocksResponse = new ResponseModel<Stock>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<Stock>> productStockTask;

                Retry2:
                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                productStockTask = _apiService.Background.GetAllOutletProductStocks1(ProductId, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                productStockTask = _apiService.UserInitiated.GetAllOutletProductStocks1(ProductId, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                productStockTask = _apiService.Speculative.GetAllOutletProductStocks1(ProductId, Settings.AccessToken);
                                break;
                            default:
                                productStockTask = _apiService.UserInitiated.GetAllOutletProductStocks1(ProductId, Settings.AccessToken);
                                break;
                        }


                        productStocksResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await productStockTask);

                        Debug.WriteLine("productStocksResponse : " + productStocksResponse.ToJson());
                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        //productStocksResponse = await ex.GetContentAsAsync<ResponseModel<InventoryDto>>();
                        if (productStocksResponse != null && productStocksResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting product stock by product id.", ex);
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

                //if (productStocksResponse != null && productStocksResponse.success && productStocksResponse.result != null)
                //{
                //    if (IsSync)
                //    {
                //        await AddAllOutletProductStocks(productStocksResponse.result.Stocks);
                //    }
                //    return productStocksResponse.result.Stocks;
                //}
                //else if (priority != Priority.Background && productStocksResponse != null && productStocksResponse.error != null && productStocksResponse.error.message != null && !string.IsNullOrEmpty(productStocksResponse.error.message))
                //{
                //    App.Instance.Hud.DisplayToast(productStocksResponse.error.message, Colors.Red, Colors.White);
                //    return null;
                //}
                //else
                //{
                //    return null;
                //}

            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return null;
        }

        public async Task<ObservableCollection<ProductOutletDto_POS>> GetRemoteProductStocksByProductId(Priority priority, int ProductId, bool IsSync = true)
        {

            Debug.WriteLine("GetProductStocks Product ID : " + ProductId.ToString());

            ResponseModel<InventoryDto> productStocksResponse = new ResponseModel<InventoryDto>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<InventoryDto>> productStockTask;


                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                productStockTask = _apiService.Background.GetAllOutletProductStocks(ProductId, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                productStockTask = _apiService.UserInitiated.GetAllOutletProductStocks(ProductId, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                productStockTask = _apiService.Speculative.GetAllOutletProductStocks(ProductId, Settings.AccessToken);
                                break;
                            default:
                                productStockTask = _apiService.UserInitiated.GetAllOutletProductStocks(ProductId, Settings.AccessToken);
                                break;
                        }


                        productStocksResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await productStockTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        productStocksResponse = await ex.GetContentAsAsync<ResponseModel<InventoryDto>>();
                        if (productStocksResponse != null && productStocksResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting product stock by product id.",ex);
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

                if (productStocksResponse != null && productStocksResponse.success && productStocksResponse.result != null)
                {
                    if (IsSync)
                    {
                        AddAllOutletProductStocks(productStocksResponse.result.Stocks);
                    }
                    return productStocksResponse.result.Stocks;
                }
                else if (priority != Priority.Background && productStocksResponse != null && productStocksResponse.error != null && productStocksResponse.error.message != null && !string.IsNullOrEmpty(productStocksResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(productStocksResponse.error.message, Colors.Red, Colors.White);
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

        public async Task<ProductDto_POS> GetRemoteProductDetail(Priority priority, int id, bool IsSync = true)
        {
            ResponseModel<ProductDto_POS> productResponse = new ResponseModel<ProductDto_POS>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<ProductDto_POS>> ProductTask;

                    GetProductDetailInput productFilter = new GetProductDetailInput
                    {
                        id = id
                    };
                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                ProductTask = _apiService.Background.GetProductDetailById(productFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                ProductTask = _apiService.UserInitiated.GetProductDetailById(productFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                ProductTask = _apiService.Speculative.GetProductDetailById(productFilter, Settings.AccessToken);
                                break;
                            default:
                                ProductTask = _apiService.UserInitiated.GetProductDetailById(productFilter, Settings.AccessToken);
                                break;
                        }


                        productResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await ProductTask);

                    }
                    catch (ApiException ex)
                    {

                        //Get Exception content
                        productResponse = await ex.GetContentAsAsync<ResponseModel<ProductDto_POS>>();
                        if (productResponse != null && productResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting product detail.", ex);
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

                if (productResponse != null && productResponse.success && productResponse.result != null)
                {
                    if (IsSync)
                    {
                        UpdateLocalProduct(productResponse.result);
                    }
                    return productResponse.result;
                }
                else if (priority != Priority.Background && productResponse != null && productResponse.error != null && productResponse.error.message != null && !string.IsNullOrEmpty(productResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(productResponse.error.message, Colors.Red, Colors.White);
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

        //Ticket start:#20064 Unit of measurement feature for iPad app.
        public async Task<ObservableCollection<ProductUnitOfMeasureDto>> GetRemoteUnityOfMeasures(Priority priority, bool syncLocal, int outletId, string filter = "", DateTime? lastSyncDate = null)
        {
            ListResponseModel<ProductUnitOfMeasureDto> remoteUnityOfMeasuresResponse = new ListResponseModel<ProductUnitOfMeasureDto>();
            Task<ListResponseModel<ProductUnitOfMeasureDto>> unityOfMeasuresTask;
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();


            if (lastSyncDate == null)
            {
                lastSyncDate = DateTime.MinValue;
            }



            // string stringDate = lastSyncDate.Value.ToString("o", CultureInfo.InvariantCulture);

            //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
            string stringlastSyncDate = lastSyncDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ", CultureInfo.InvariantCulture);
            //Ticket end:#34015 .by rupesh

            Debug.WriteLine("string date : " + stringlastSyncDate);
            Debug.WriteLine("lastSyncDate  : " + lastSyncDate.Value.ToString());

        Retry:
            switch (priority)
            {
                case Priority.Background:
                    unityOfMeasuresTask = _apiService.Background.GetUnityOfMeasuresByOutlet(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    unityOfMeasuresTask = _apiService.UserInitiated.GetUnityOfMeasuresByOutlet(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    unityOfMeasuresTask = _apiService.Speculative.GetUnityOfMeasuresByOutlet(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
                default:
                    unityOfMeasuresTask = _apiService.UserInitiated.GetUnityOfMeasuresByOutlet(outletId, stringlastSyncDate, Settings.AccessToken);
                    break;
            }
            Debug.WriteLine("GetOutletProductStock API : /api/services/app/productSearch/GetRemoteUnityOfMeasures");
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var startDateTime = DateTime.Now;
                    Debug.WriteLine("GetRemoteUnityOfMeasures API Request Starts at : " + startDateTime);
                    remoteUnityOfMeasuresResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await unityOfMeasuresTask);

                    var endDateTime = DateTime.Now;
                    Debug.WriteLine("GetRemoteUnityOfMeasures API Request Ends at : " + endDateTime);
                    Debug.WriteLine("GetRemoteUnityOfMeasures API Request Total Time : " + (endDateTime - startDateTime));
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    remoteUnityOfMeasuresResponse = await ex.GetContentAsAsync<ListResponseModel<ProductUnitOfMeasureDto>>();
                    if (remoteUnityOfMeasuresResponse != null && remoteUnityOfMeasuresResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting Unity Of Measures.",ex);
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

            if (remoteUnityOfMeasuresResponse.success && remoteUnityOfMeasuresResponse.result != null && remoteUnityOfMeasuresResponse.result.items.Count > 0)
            {
                UpdateUnityOfMeasures(remoteUnityOfMeasuresResponse.result.items);
                return remoteUnityOfMeasuresResponse.result.items;
            }
            else
            {
                if (remoteUnityOfMeasuresResponse != null && remoteUnityOfMeasuresResponse.error != null && remoteUnityOfMeasuresResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(remoteUnityOfMeasuresResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }

        public bool UpdateUnityOfMeasures(ObservableCollection<ProductUnitOfMeasureDto> unityOfMeasures)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                     realm.Add(unityOfMeasures.Select(a=>a.ToModel()).ToList(), update: true);
                }); 
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in UpdateUnityOfMeasures : " + ex.Message + " : " + ex.StackTrace);
                return false;
            }
        }

        public ObservableCollection<ProductUnitOfMeasureDto> GetLocalUnityOfMeasures()
        {
            try
            {
                //return await CommonQueries.GetAllLocals<ProductUnitOfMeasureDto>();
                var realm = RealmService.GetRealm();
                return new ObservableCollection<ProductUnitOfMeasureDto>(realm.All<ProductUnitOfMeasureDB>().ToList().Select(a => ProductUnitOfMeasureDto.FromModel(a)));
            }
            catch (Exception ex)
            {
                ex.Track();
                //Debug.WriteLine("Exception in GetLocalProducts : " + ex.Message + " : " + ex.StackTrace);
                return null;
            }
        }

        public ProductDto_POS GetLocalUnitOfMeasureProduct(int id)
        {
            try
            {
                var realm = RealmService.GetRealm();
                var unitOfMeasure = ProductUnitOfMeasureDto.FromModel(realm.All<ProductUnitOfMeasureDB>()?.ToList().FirstOrDefault(a => a.Id == id));

                var product =  GetLocalProduct(unitOfMeasure.MeasureProductId);
                if (product != null)
                {
                    var unitOfMeasureProduct = product.Copy();
                    unitOfMeasureProduct.Id = unitOfMeasure.Id;
                    unitOfMeasureProduct.Name = unitOfMeasure.Name;
                    unitOfMeasureProduct.BarCode = unitOfMeasure.BarCode;
                    unitOfMeasureProduct.Sku = unitOfMeasure.Sku;
                    unitOfMeasureProduct.ProductUnitOfMeasureDto = unitOfMeasure;
                    unitOfMeasureProduct.IsUnitOfMeasure = true;
                    if (unitOfMeasureProduct.ParentId != null)
                    {
                        var parentProduct =  GetLocalProductDB(unitOfMeasureProduct.ParentId.Value);
                        unitOfMeasureProduct.ItemImageUrl = parentProduct.ItemImageUrl;
                    }

                    return unitOfMeasureProduct;
                }
                return null;

            }
            catch (KeyNotFoundException ex)
            {
                 ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }



        //Ticket end:#20064 Unit of measurement feature for iPad app.

        //Ticket start:#30282 Suggested payment amount by product tag during checkout.by rupesh
        public async Task<ObservableCollection<ProductTagDto>> GetRemoteProductTags(Priority priority, bool syncLocal, DateTime? lastSyncDate = null)
        {
            ListResponseModel<ProductTagDto> remoteProductTagsResponse = new ListResponseModel<ProductTagDto>();
            Task<ListResponseModel<ProductTagDto>> productTagsResponseTask;
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();


            if (lastSyncDate == null)
            {
                lastSyncDate = DateTime.MinValue;
            }

            //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
            string stringlastSyncDate = lastSyncDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ", CultureInfo.InvariantCulture);
            //Ticket end:#34015 .by rupesh

            Debug.WriteLine("string date : " + stringlastSyncDate);
            Debug.WriteLine("lastSyncDate  : " + lastSyncDate.Value.ToString());

        Retry:
            switch (priority)
            {
                case Priority.Background:
                    productTagsResponseTask = _apiService.Background.GetAllProductTags(stringlastSyncDate, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    productTagsResponseTask = _apiService.UserInitiated.GetAllProductTags(stringlastSyncDate, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    productTagsResponseTask = _apiService.Speculative.GetAllProductTags(stringlastSyncDate, Settings.AccessToken);
                    break;
                default:
                    productTagsResponseTask = _apiService.UserInitiated.GetAllProductTags(stringlastSyncDate, Settings.AccessToken);
                    break;
            }
            Debug.WriteLine("GetRemoteProductTags API : /api/services/app/tag/GetAll_POS");
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var startDateTime = DateTime.Now;
                    Debug.WriteLine("GetRemoteProductTags API Request Starts at : " + startDateTime);
                    remoteProductTagsResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await productTagsResponseTask);

                    var endDateTime = DateTime.Now;
                    Debug.WriteLine("GetRemoteProductTags API Request Ends at : " + endDateTime);
                    Debug.WriteLine("GetRemoteProductTags API Request Total Time : " + (endDateTime - startDateTime));
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    remoteProductTagsResponse = await ex.GetContentAsAsync<ListResponseModel<ProductTagDto>>();
                    if (remoteProductTagsResponse != null && remoteProductTagsResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            //Need to log this error to backend
                            Extensions.SomethingWentWrong("Getting product tags.",ex);
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

            if (remoteProductTagsResponse.success && remoteProductTagsResponse.result != null && remoteProductTagsResponse.result.items.Count > 0)
            {
                UpdateProductTags(remoteProductTagsResponse.result.items);
                return remoteProductTagsResponse.result.items;
            }
            else
            {
                if (remoteProductTagsResponse != null && remoteProductTagsResponse.error != null && remoteProductTagsResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(remoteProductTagsResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }

        public bool UpdateProductTags(ObservableCollection<ProductTagDto> productTagDtos)
        {
            try
            {
                if (productTagDtos != null && productTagDtos.Count > 0)
                {
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(productTagDtos.Select(a => a.ToModel()).ToList(), update: true);
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in ProductTagDto : " + ex.Message + " : " + ex.StackTrace);
                return false;
            }
        }

        public ProductTagDto GetLocalProductTag(string tagId)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var productTag = realm.Find<ProductTagDB>(tagId);
                return ProductTagDto.FromModel(productTag);
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        //Start #84438 iOS : FR :add discount offers on product tag by Pratik
        public List<ProductTagDto> GetLocalProductTagByIds(List<int> ids)
        {
            var realm = RealmService.GetRealm();
            var ssd = realm.All<ProductTagDB>().ToList();
            return ssd.Where(a => a.Id != null && ids.Contains(Convert.ToInt32(a.Id))).Select(a => ProductTagDto.FromModel(a)).ToList();
        }
        //End #84438 by Pratik

        //Ticket end:#30282 Suggested payment amount by product tag during checkout.by rupesh
        public async Task<ObservableCollection<CategoryDto>> GetRemoteCategoriesByOutlet(Priority priority, bool syncLocal, int outletId)
        {
            ListResponseModel<CategoryDto> categoryResponse = null;

            Task<ListResponseModel<CategoryDto>> categoryTask;
        Retry1:
            switch (priority)
            {
                case Priority.Background:
                    categoryTask = _apiService.Background.GetAllCategoriesByOutlet(outletId, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    categoryTask = _apiService.UserInitiated.GetAllCategoriesByOutlet(outletId, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    categoryTask = _apiService.Speculative.GetAllCategoriesByOutlet(outletId, Settings.AccessToken);
                    break;
                default:
                    categoryTask = _apiService.UserInitiated.GetAllCategoriesByOutlet(outletId, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    categoryResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await categoryTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    categoryResponse = await ex.GetContentAsAsync<ListResponseModel<CategoryDto>>();

                    if (categoryResponse != null && categoryResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting categories.",ex);
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

            if (categoryResponse.success)
            {
                if (syncLocal && categoryResponse.result != null && categoryResponse.result.items.Count > 0)
                {
                    UpdateLocalCategories(categoryResponse.result.items);
                }
                return categoryResponse.result.items;
            }
            else
            {
                if (priority != Priority.Background && categoryResponse != null && categoryResponse.error != null && categoryResponse.error.message != null)
                {
                    Extensions.ServerMessage(categoryResponse.error.message);
                }
                return null;
            }
        }
        public async Task<string> GetProductDescription(Priority priority, int ProductId)
        {

            Debug.WriteLine("GetProductDescription Product ID : " + ProductId.ToString());
            ResponseModel<string> productDescriptionResponse = new ResponseModel<string>();

            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Task<ResponseModel<string>> productDescriptionTask;
                    GetProductDetailInput productFilter = new GetProductDetailInput
                    {
                        id = ProductId
                    };


                Retry2:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                productDescriptionTask = _apiService.Background.GetProductDescription(productFilter, Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                productDescriptionTask = _apiService.UserInitiated.GetProductDescription(productFilter, Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                productDescriptionTask = _apiService.Speculative.GetProductDescription(productFilter, Settings.AccessToken);
                                break;
                            default:
                                productDescriptionTask = _apiService.UserInitiated.GetProductDescription(productFilter, Settings.AccessToken);
                                break;
                        }


                        productDescriptionResponse = await Policy
                            .Handle<Exception>()
                            .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                            .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                            .ExecuteAsync(async () => await productDescriptionTask);

                    }
                    catch (ApiException ex)
                    {
                        //Get Exception content
                        productDescriptionResponse = await ex.GetContentAsAsync<ResponseModel<string>>();
                        if (productDescriptionResponse != null && productDescriptionResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                Extensions.SomethingWentWrong("Getting product stock by product id.",ex);
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

                if (productDescriptionResponse != null && productDescriptionResponse.success && productDescriptionResponse.result != null)
                {
                    return productDescriptionResponse.result;
                }
                else if (priority != Priority.Background && productDescriptionResponse != null && productDescriptionResponse.error != null && productDescriptionResponse.error.message != null && !string.IsNullOrEmpty(productDescriptionResponse.error.message))
                {
                    App.Instance.Hud.DisplayToast(productDescriptionResponse.error.message, Colors.Red, Colors.White);
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

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public async Task<ObservableCollection<ProductAvailableStocks>> GetProductAvailableStocks(Priority priority, ProductAvailableStocksRequest productFilter)
        {
            var remoteProductAvailableStocksResponse = new ResponseModel<List<ProductAvailableStocks>>();
            Task<ResponseModel<List<ProductAvailableStocks>>> productTask;
            DateTime CurrentTime = DateTime.Now.ToUniversalTime();

        Retry:
            switch (priority)
            {
                case Priority.Background:
                    productTask = _apiService.Background.GetProductAvailableStocks(productFilter.outletId, productFilter.invoiceId, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    productTask = _apiService.UserInitiated.GetProductAvailableStocks(productFilter.outletId, productFilter.invoiceId, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    productTask = _apiService.Speculative.GetProductAvailableStocks(productFilter.outletId, productFilter.invoiceId, Settings.AccessToken);
                    break;
                default:
                    productTask = _apiService.UserInitiated.GetProductAvailableStocks(productFilter.outletId, productFilter.invoiceId, Settings.AccessToken);
                    break;
            }
            Debug.WriteLine("GetOutletProductStock API : /api/services/app/stock/GetProductAvailableStocks");
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    remoteProductAvailableStocksResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await productTask);

                }
                catch (ApiException ex)
                {
                    remoteProductAvailableStocksResponse = await ex.GetContentAsAsync<ResponseModel<List<ProductAvailableStocks>>>();
                    if (remoteProductAvailableStocksResponse != null && remoteProductAvailableStocksResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting product stocks.", ex);
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

            if (remoteProductAvailableStocksResponse != null && remoteProductAvailableStocksResponse.success && remoteProductAvailableStocksResponse.result != null && remoteProductAvailableStocksResponse.result.Count > 0)
            {
                return new ObservableCollection<ProductAvailableStocks>(remoteProductAvailableStocksResponse.result);
            }
            else
            {
                if (remoteProductAvailableStocksResponse != null && remoteProductAvailableStocksResponse.error != null && remoteProductAvailableStocksResponse.error.message != null)
                {
                    if (priority != Priority.Background)
                    {
                        App.Instance.Hud.DisplayToast(remoteProductAvailableStocksResponse.error.message, Colors.Red, Colors.White);
                    }
                    return null;
                }
                return null;
            }
        }
        //End #84293 by Pratik

        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        public List<ProductDto_POS> GetLocalProductsByIds(List<int> ids)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                return realm.All<ProductDB>().ToList().Where(a => ids.Any(x => x == a.Id)).Select(a => ProductDto_POS.FromModel(a)).ToList();
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }

        public async Task<InvoiceLayoutSellDto> GetCurrentLayout(Priority priority, bool syncLocal, int registerId)
        {
            ResponseModel<InvoiceLayoutSellDto> invoiceLayoutSellResponse = null;

            Task<ResponseModel<InvoiceLayoutSellDto>> invoiceLayoutSellTask;
        Retry1:
            switch (priority)
            {
                case Priority.Background:
                    invoiceLayoutSellTask = _apiService.Background.GetCurrentLayout(registerId, Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    invoiceLayoutSellTask = _apiService.UserInitiated.GetCurrentLayout(registerId, Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    invoiceLayoutSellTask = _apiService.Speculative.GetCurrentLayout(registerId, Settings.AccessToken);
                    break;
                default:
                    invoiceLayoutSellTask = _apiService.UserInitiated.GetCurrentLayout(registerId, Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    invoiceLayoutSellResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await invoiceLayoutSellTask);
                }
                catch (ApiException ex)
                {
                    invoiceLayoutSellResponse = await ex.GetContentAsAsync<ResponseModel<InvoiceLayoutSellDto>>();

                    if (invoiceLayoutSellResponse != null && invoiceLayoutSellResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting categories.",ex);
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
            if (invoiceLayoutSellResponse.success)
            {
                if (syncLocal && invoiceLayoutSellResponse.result != null && invoiceLayoutSellResponse.result.Id > 0)
                {                    
                    UpdateCurrentLayout(invoiceLayoutSellResponse.result);
                }
                return invoiceLayoutSellResponse.result;
            }
            else
            {
                if (priority != Priority.Background && invoiceLayoutSellResponse?.error?.message != null)
                {
                    Extensions.ServerMessage(invoiceLayoutSellResponse.error.message);
                }
                return null;
            }
        }

        public bool UpdateCurrentLayout(InvoiceLayoutSellDto dto)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(dto.ToModel(), update: true); 
                });

                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public bool ClearCurrentLayout()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    var result = realm.All<InvoiceLayoutSellDB>();
                    if(result!= null && result.Count() > 0)
                        realm.RemoveRange(result); 
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public InvoiceLayoutSellDto GetLocalCurrentLayoutByID(int id)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                return  InvoiceLayoutSellDto.FromModel(realm.Find<InvoiceLayoutSellDB>(id));
            }
            catch (KeyNotFoundException ex)
            {
                 ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }

        }

        public InvoiceLayoutSellDto GetCurrentLayout()
        {
            try
            {
                using var realm = RealmService.GetRealm();
                return  InvoiceLayoutSellDto.FromModel(realm.All<InvoiceLayoutSellDB>()?.ToList()?.FirstOrDefault());
            }
             catch (KeyNotFoundException ex)
            {
                 ex.Track();
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }

        }
        //End #90945 By Pratik
    }
}
