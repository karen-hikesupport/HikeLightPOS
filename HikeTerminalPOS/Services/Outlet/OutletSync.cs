using System;
using HikePOS.Models;
using Fusillade;
using HikePOS.Helpers;
using System.Threading.Tasks;
using HikePOS.Model;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using HikePOS.Enums;
using HikePOS.Models.Log;
using System.Diagnostics;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Services.Payment;

namespace HikePOS.Services
{
    public class OutletSync
    {
        ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        OutletServices outletService;

        ApiService<IShopApi> shopApiService = new ApiService<IShopApi>();
        ShopServices shopService;

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        UserServices userService;

        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        ProductServices productService;

        ApiService<IOfferApi> offerApiService = new ApiService<IOfferApi>();
        OfferServices offerService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        ApiService<ICommonLookupAPI> LookupApiService = new ApiService<ICommonLookupAPI>();
        CommonLookupServices commonLookupServices;

        ApiService<ISubscriptionAPI> SubscriptionApiService = new ApiService<ISubscriptionAPI>();
        SubscriptionSevice subscriptionSevice;

        ApiService<IHearAboutApi> HearAboutApiService = new ApiService<IHearAboutApi>();
        HearAboutService hearAboutService;

        ApiService<ITaxApi> TaxApiService = new ApiService<ITaxApi>();
        TaxServices taxServices;

        ApiService<IRestaurantApi> restaurantApiService = new ApiService<IRestaurantApi>();
        RestaurantService restaurantService;

        ApiService<IHikePayService> hikePayApiService = new ApiService<IHikePayService>();
        HikePayService hikePayService;

        LastSyncService objLastSyncService;



        public OutletSync()
        {
            outletService = new OutletServices(outletApiService);
            shopService = new ShopServices(shopApiService);
            customerService = new CustomerServices(customerApiService);
            paymentService = new PaymentServices(paymentApiService);
            userService = new UserServices(userApiService);
            productService = new ProductServices(productApiService);
            offerService = new OfferServices(offerApiService);
            saleService = new SaleServices(saleApiService);
            objLastSyncService = new LastSyncService();
            commonLookupServices = new CommonLookupServices(LookupApiService);
            subscriptionSevice = new SubscriptionSevice(SubscriptionApiService);
            hearAboutService = new HearAboutService(HearAboutApiService);
            taxServices = new TaxServices(TaxApiService);
            restaurantService = new RestaurantService(restaurantApiService);
            hikePayService = new HikePayService(hikePayApiService);
        }

        public static void ClearData()
        {
            try
            {
                var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    // Remove all objects from the realm.
                    realm.RemoveAll();
                });
                /*  CommonQueries.InvalidateAllObjects<ShopGeneralDto>();
                  CommonQueries.InvalidateAllObjects<CustomerDto_POS>();
                  CommonQueries.InvalidateAllObjects<CustomerGroupDto>();
                  CommonQueries.InvalidateAllObjects<PaymentOptionDto>();
                  CommonQueries.InvalidateAllObjects<RoleListDto>();
                  CommonQueries.InvalidateAllObjects<UserListDto>();
                  CommonQueries.InvalidateAllObjects<CategoryDto>();
                  CommonQueries.InvalidateAllObjects<ProductDto_POS>();
                  CommonQueries.InvalidateAllObjects<OfferDto>();
                  CommonQueries.InvalidateAllObjects<InvoiceDto>();
                  CommonQueries.InvalidateAllObjects<LastSyncDto>();
                  CommonQueries.InvalidateAllObjects<SubscriptionDto>();
                  CommonQueries.InvalidateAllObjects<List<HikeAuditLog>>();
                  CommonQueries.InvalidateAllObjects<DenominationDto>();

                  CommonQueries.InvalidateAllObjects<CustomerAddressDto>();

                  CommonQueries.InvalidateAllObjects<ProductUnitOfMeasureDto>();
                  CommonQueries.InvalidateAllObjects<ProductTagDto>();*/

                DependencyService.Get<IImageServices>().ClearCaches();
                //Settings.HikePayStoreId = string.Empty;//clear adyen storeId

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task<bool> SetupOutlet()
        {
            try
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return false;
                }
                var outlets = outletService.GetLocalOutlets();
                ClearData();
                outletService.UpdateLocalOutlets(outlets);
                LastSyncDto objLastSyncDto = new LastSyncDto();

                //Get Account Subscription detail
                WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress2_Text")));
                SubscriptionDto Subscription = await subscriptionSevice.GetRemoteAccountDetail(Priority.UserInitiated, true);
                Settings.Subscription = Subscription;

                var isExpired =  await Subscription.IsExpired();
                if (isExpired)
                {
                    return false;
                }

             

                if (Settings.SelectedOutletId != 0)
                {
                    //#103669
                    ObservableCollection<OutletDto_POS> outletResponse = await outletService.GetRemoteOutlets(Priority.UserInitiated, true);
                    if (outletResponse != null)
                    {
                        if (outletResponse.Count == 1 && outletResponse[0] != null && outletResponse[0].OutletRegisters != null && outletResponse[0].OutletRegisters.Count == 1)
                        {
                            Settings.SelectedOutletId = outletResponse[0].Id;
                            Settings.SelectedOutletName = outletResponse[0].Title;
                            Settings.SelectedOutlet = outletResponse[0];
                            Settings.CurrentRegister = outletResponse[0].OutletRegisters[0];
                        }
                        else if(outletResponse.Count > 0 && Settings.CurrentRegister != null && Settings.CurrentRegister.OutletID > 0)
                        {
                            var fdata = outletResponse.FirstOrDefault(a=>a.Id == Settings.CurrentRegister.OutletID);
                            var isnotdelete = fdata?.OutletRegisters != null && fdata.OutletRegisters.Any(a=> a.Id == Settings.CurrentRegister.Id);
                            if(!isnotdelete)
                            {
                                Settings.SelectedOutlet = outletResponse.FirstOrDefault(a=>a.IsDefault == true) ?? outletResponse[0];
                                Settings.SelectedOutletId = Settings.SelectedOutlet .Id;
                                Settings.SelectedOutletName = Settings.SelectedOutlet .Title;
                                Settings.CurrentRegister = Settings.SelectedOutlet .OutletRegisters[0];
                            }
                        }
                    }
                    //#103669

                    await outletService.GetRemoteOutletById(Priority.UserInitiated, true, Settings.SelectedOutletId);
                }

                await outletService.GetAllTemplatesReceipt(Priority.UserInitiated, true);


             

                if (Settings.CurrentRegister != null && Settings.CurrentRegister.Id > 0)
                {
                    RegisterDto registerResponse = await outletService.GetRemoteRegisterById(Priority.UserInitiated, true, Settings.CurrentRegister.Id);

                    if (registerResponse != null)
                    {
                        Settings.CustomerAppConfigFrom = registerResponse.CustomerDisplayConfigureType.ToString();
                        Settings.CustomerAppPin = registerResponse.CustomerDisplayConfigurePin;
                    }
                }

                await userService.GetRemoteUserByEmail(Priority.UserInitiated, Settings.CurrentUserEmail, true);

                //Shop details
                WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress2_Text")));
                ShopGeneralDto shopResponse = await shopService.GetRemoteShops(Priority.UserInitiated, true);
                Settings.CountryCode = shopResponse?.Address?.Country;

                //Customers
                //  WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress7_Text")));

                //Customer groups
                List<Task> userstasks = new List<Task>();
                objLastSyncDto.LastCustomerGroupDateSyncTime = DateTime.UtcNow;
                userstasks.Add(customerService.GetRemoteCustomerGroups(Priority.UserInitiated, true));

                objLastSyncDto.LastCustomerDateSyncTime = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    userstasks.Add(customerService.GetRemoteCustomers(Priority.UserInitiated, true));
                }
                else
                {
                    userstasks.Add(customerService.GetRemoteCustomersLemda(Priority.UserInitiated, true));
                }

                //var localCustomers = await customerService.GetLocalCustomers();
                //Debug.WriteLine("All Customers : " + JsonConvert.SerializeObject(localCustomers));

                userstasks.Add(customerService.GetRemoteCustomerCustomFields(Priority.UserInitiated, true));

                //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer)
                {
                    objLastSyncDto.LastDeliveryAddressDateSyncTime = DateTime.UtcNow;
                    userstasks.Add(customerService.GetRemoteAllDeliveryAddresses(Priority.UserInitiated, true));
                }
                //Ticket end:#26664 .by rupesh


                //PaymentOptions
                //   WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress3_Text")));
                objLastSyncDto.LastPaymentDataSyncTime = DateTime.UtcNow;
                userstasks.Add(paymentService.GetRemotePaymentOptions(Priority.UserInitiated, true));

                //Roles
                //  WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress4_Text")));
                userstasks.Add(userService.GetRemoteRoles(Priority.UserInitiated, true, new RoleRequestModel()));

                //User list
                // WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress3_Text")));
                userstasks.Add(userService.GetRemoteUsers(Priority.UserInitiated, true));

                //Categories
                // WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress5_Text")));
                // var categories = await productService.GetRemoteCategories(Priority.UserInitiated, true);
                userstasks.Add(productService.GetRemoteCategoriesByOutlet(Priority.UserInitiated, true, Settings.SelectedOutletId));

                //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                if ( Settings.CurrentRegister != null && Settings.CurrentRegister.Id > 0)
                {
                    productService.ClearCurrentLayout();
                    userstasks.Add(productService.GetCurrentLayout(Priority.UserInitiated,true,Settings.CurrentRegister.Id));
                }
                //End #90945 By Pratik

                if (Settings.IsRestaurantPOS)
                {
                    userstasks.Add(restaurantService.GetFloors(Priority.UserInitiated, true, new GetFloorInput()
                    {
                        MaxResultCount = 100,
                        sorting = "0",
                        SkipCount = 0,
                        Filter = "",
                        Name = ""
                    }));
                }               

                

                await Task.WhenAll(userstasks);

               
                //Tax List
                ObservableCollection<TaxDto> taxes;
                taxes = await taxServices.GetRemoteTaxes(Priority.UserInitiated, true);

                WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress6_Text")));
                //ProductSocks
                var beforeTimeofSyncProductStock = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    var productstocks = await productService.GetRemoteProductStocks(Priority.UserInitiated, true, Settings.SelectedOutletId, "", objLastSyncDto.LastProductStockDataSyncTime?.ToUniversalTime());
                }
                else
                {
                    var productstocks = await productService.GetRemoteProductStocksLambda(Priority.UserInitiated, true, Settings.SelectedOutletId, "", objLastSyncDto.LastProductStockDataSyncTime?.ToUniversalTime());
                }
                objLastSyncDto.LastProductStockDataSyncTime = beforeTimeofSyncProductStock;

                //Products
                objLastSyncDto.LastProductDataSyncTime = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    var products = await productService.GetRemoteProducts(Priority.UserInitiated, true, Settings.SelectedOutletId, string.Empty, null);
                }
                else
                {
                    var products = await productService.GetRemoteProductsLambda(Priority.UserInitiated, true, Settings.SelectedOutletId, string.Empty, null, taxes);
                }
                WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress2_Text")));

                List<Task> salehistorytasks = new List<Task>();

                //UnityOfMeasures
                objLastSyncDto.LastUnityOfMeasureSyncTime = DateTime.UtcNow;
                salehistorytasks.Add(productService.GetRemoteUnityOfMeasures(Priority.UserInitiated, true, Settings.SelectedOutletId));


                //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                //Ticket start:#30282 Suggested payment amount by product tag during checkout.by rupesh
                //ProductTags
                // if (Settings.StoreGeneralRule.AutoSuggestPaymentByTag)
                // {
                    objLastSyncDto.LastProductTagsDataSyncTime = DateTime.UtcNow;
                    salehistorytasks.Add(productService.GetRemoteProductTags(Priority.UserInitiated, true));
                // }
                //Ticket end:#30282 .by rupesh
                //Start #End iOS : FR :add discount offers on product tag by Pratik

                //var localProducts = await productService.GetLocalProducts();
                //Debug.WriteLine("localProducts : " + localProducts.Count());

                //Offers
                objLastSyncDto.LastOfferDataSyncTime = DateTime.UtcNow;
                salehistorytasks.Add(offerService.GetRemoteOffers(Priority.UserInitiated, true));

                //Sales
                // WeakReferenceMessenger.Default.Send(new Messenger.ProgressStatusMessenger(LanguageExtension.Localize("Progress10_Text")));
                DateTime ToDate = DateTime.UtcNow;

                //Get all data expect Parked and LayBy
                DateTime Fromdate = DateTime.UtcNow.AddDays(-7);
                List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Completed, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.Exchange, InvoiceStatus.Quote };
                salehistorytasks.Add(saleService.GetRemoteInvoices(Priority.UserInitiated, true, Settings.SelectedOutletId, Fromdate, ToDate, status));

                Settings.LastLoadedHistoryDate = Fromdate;

                //Get all Parked and LayBy data
                Fromdate = DateTime.UtcNow.AddYears(-7);
                status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Parked, InvoiceStatus.LayBy };
                salehistorytasks.Add(saleService.GetRemoteInvoices(Priority.UserInitiated, true, Settings.SelectedOutletId, Fromdate, ToDate, status));
                await Task.WhenAll(salehistorytasks);
                objLastSyncDto.LastInvoiceDataSyncTime = ToDate;
                UpdateSyncTime(objLastSyncDto);

                //Get all Countries
                Settings.AllCountries = await commonLookupServices.GetRemoteAllCountries(Priority.UserInitiated, true);

                //Get all hearabout us
                Settings.AllHearAbout = await hearAboutService.GetRemoteAllHearAbout(Priority.UserInitiated, true);

                var AllClockInOutUsers = await userService.GetAllClockInOutUsers(Priority.UserInitiated, true);

                await outletService.GetRemoteDenomination(Priority.UserInitiated, true);

                await UpdateLocalTerminalIpAddress();
                
                if (Settings.StoreShopDto != null && Settings.StoreShopDto.IsHikePayActivated)
                    await hikePayService.GetSplitProfile(Priority.UserInitiated);
                if (shopResponse != null)
                {
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
            }
            return false;
        }


        public bool UpdateSyncTime(LastSyncDto NewLastSyncDto)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    LastSyncDto UpdatedLastSyncDto = objLastSyncService.GetLastSyncTime();
                    if (UpdatedLastSyncDto == null)
                    {
                        UpdatedLastSyncDto = new LastSyncDto();
                    }

                    if (NewLastSyncDto.LastOutletDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastOutletDataSyncTime = NewLastSyncDto.LastOutletDataSyncTime;
                    }

                    if (NewLastSyncDto.LastPaymentDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastPaymentDataSyncTime = NewLastSyncDto.LastPaymentDataSyncTime;
                    }

                    if (NewLastSyncDto.LastCustomerDateSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastCustomerDateSyncTime = NewLastSyncDto.LastCustomerDateSyncTime;
                    }

                    if (NewLastSyncDto.LastProductDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastProductDataSyncTime = NewLastSyncDto.LastProductDataSyncTime;
                    }

                    if (NewLastSyncDto.LastProductStockDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastProductStockDataSyncTime = NewLastSyncDto.LastProductStockDataSyncTime;

                        //Ticket start:#14146 Product stock displaying 0 though available in product listing page. by rupesh
                        //Ticket start :#14312,#14272 iOS - Products showing 0 price in iPad.by rupesh
                        UpdatedLastSyncDto.LastBackgroundProductStockDataSyncTime = NewLastSyncDto.LastProductStockDataSyncTime;
                        //Ticket End:#14312,#14272. by rupesh
                        //Ticket End:#14146. by rupesh

                    }

                    if (NewLastSyncDto.LastUnityOfMeasureSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastUnityOfMeasureSyncTime = NewLastSyncDto.LastUnityOfMeasureSyncTime;
                    }
                    //Ticket start:#30282 Suggested payment amount by product tag during checkout.by rupesh
                    if (NewLastSyncDto.LastProductTagsDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastProductTagsDataSyncTime = NewLastSyncDto.LastProductTagsDataSyncTime;
                    }
                    //Ticket end:#30282 .by rupesh
                    if (NewLastSyncDto.LastOfferDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastOfferDataSyncTime = NewLastSyncDto.LastOfferDataSyncTime;
                    }

                    if (NewLastSyncDto.LastInvoiceDataSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastInvoiceDataSyncTime = NewLastSyncDto.LastInvoiceDataSyncTime;
                    }

                    if (NewLastSyncDto.LastCustomerGroupDateSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastCustomerGroupDateSyncTime = NewLastSyncDto.LastCustomerGroupDateSyncTime;
                    }

                    if (NewLastSyncDto.LastDeliveryAddressDateSyncTime != null)
                    {
                        UpdatedLastSyncDto.LastDeliveryAddressDateSyncTime = NewLastSyncDto.LastDeliveryAddressDateSyncTime;
                    }


                    objLastSyncService.CreateUpdateLastSyncData(UpdatedLastSyncDto);
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        static bool isSyncronizationRunning;

        public async Task PushAllUnsyncDataOnRemote(bool inBackgroundMode = true, bool RequiredAllData = false, bool ResetAfterUppdate = false, bool onlyUpload = false)
        {
            if (isSyncronizationRunning)
            {
                return;
            }
            try
            {

                isSyncronizationRunning = true;

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

                        var lstInvoices = saleService.GetOfflineInvoices();
                        if (lstInvoices != null)
                        {
                            var lstMainInvoices = lstInvoices.Where(x => x.ReferenceTempInvoiceId == null && x.ReferenceInvoiceId == null);
                            var lstSubInvoices = lstInvoices.Where(x => x.ReferenceTempInvoiceId != null || x.ReferenceInvoiceId != null);

                            //save offline data 
                            foreach (InvoiceDto item in lstMainInvoices)
                            {
                                if ((item.CustomerId == null || item.CustomerId.Value == 0) && !string.IsNullOrEmpty(item.CustomerTempId))
                                {
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

                                //Ticket start:#22476,#25360 iOS: Ipad have unsynced sale error but cannot find any.by rupesh
                                //Start #92570 By Pratik
                                if (((item.Status == InvoiceStatus.initial || item.Status == InvoiceStatus.Pending) && item.InvoiceLineItems.Count == 0)
                                || (string.IsNullOrEmpty(item.Number) && (item.InvoicePayments == null || item.InvoicePayments.Count == 0) && item.Status == InvoiceStatus.Completed))
                                {
                                     //End #92570 By Pratik
                                    //Skiping pening and emply line items sale from sync.
                                    //Deleting pending sale with emply line items
                                    //var docId = item.Id > 0 ? nameof(InvoiceDto) + "_" + item.Id.ToString() : item.InvoiceTempId;
                                    //CommonQueries.Invalidate(docId);
                                    var realm = RealmService.GetRealm();
                                    var removedata = realm.Find<InvoiceDB>(item.KeyId);
                                    if (removedata != null)
                                    {
                                        realm.Write(() =>
                                        {
                                            realm.Remove(removedata);
                                        });
                                    }
                                    continue;
                                }
                                //Ticket end:#22476,#25360 iOS: .by rupesh

                                InvoiceDto invoice;
                                if (item.IsCustomerChange)
                                    invoice = await saleService.UpdateAssociateCustomerRemoteInvoice(processmode, true, item);
                                else
                                    //Ticket start:#41496 Duplicate sales issues for some customers.by rupesh
                                    invoice = await saleService.UpdateRemoteInvoice(processmode, true, item, true);
                                //Ticket end:#41496 .by rupesh

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
                                    invoice = await saleService.UpdateAssociateCustomerRemoteInvoice(processmode, true, item);
                                else
                                    //Ticket start:#41496 Duplicate sales issues for some customers.by rupesh
                                    invoice = await saleService.UpdateRemoteInvoice(processmode, true, item, true);
                                //Ticket end:#41496 .by rupesh

                                if (invoice != null && invoice.Id != 0)
                                {
                                    WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(invoice));
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(Settings.AccessToken))
                        {

                            var unsyncCustomer = customerService.GetUnSyncCustomer();
                            if (unsyncCustomer != null)
                            {
                                foreach (CustomerDto_POS item in unsyncCustomer)
                                {
                                    await customerService.UpdateRemoteCustomer(processmode, true, item);
                                }
                            }

                            //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                            var unsyncDeliveryAddress = customerService.GetUnSyncDeliveryAddresses();
                            if (unsyncDeliveryAddress != null)
                            {
                                foreach (CustomerAddressDto item in unsyncDeliveryAddress)
                                {
                                    await customerService.CreateOrUpdateRemoteCustomerAddress(processmode, true, item);
                                }
                            }
                            //Ticket end:#26664 .by rupesh

                        }

                        if (!onlyUpload)
                        {
                            if (ResetAfterUppdate)
                            {
                                var tmpInvoices = saleService.GetOfflineInvoices();
                                var tmpCustomer = customerService.GetUnSyncCustomer();
                                var tmpPayments = paymentService.GetLocalPaymentOptions();
                                await SetupOutlet();

                                if (tmpInvoices != null && tmpInvoices.Any())
                                {
                                    saleService.UpdateLocalInvoices(tmpInvoices);
                                    App.Instance.Hud.DisplayToast("Still, You have some unsync sale data in your local database.");
                                }

                                if (tmpCustomer != null && tmpCustomer.Any())
                                {
                                    foreach (var customer in tmpCustomer)
                                    {
                                        customerService.UpdateLocalCustomer(customer);
                                    }
                                    App.Instance.Hud.DisplayToast("Still, You have some unsync customer data in your local database.");
                                }

                                if (tmpPayments != null && tmpPayments.Any(x => x.Id != 0 && !x.IsDeleted && !string.IsNullOrEmpty(x.ConfigurationDetails)))
                                {
                                    foreach (var oldpayment in tmpPayments.Where(x => x.Id != 0 && !x.IsDeleted && !string.IsNullOrEmpty(x.ConfigurationDetails)))
                                    {
                                        var updatedPaymenOption = paymentService.GetLocalPaymentOption(oldpayment.Id);
                                        if (updatedPaymenOption != null)
                                        {
                                            updatedPaymenOption.ConfigurationDetails = oldpayment.ConfigurationDetails;
                                            paymentService.UpdateLocalPaymentOption(updatedPaymenOption);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var tmpPayments = paymentService.GetLocalPaymentOptions();
                                var realm = RealmService.GetRealm();
                                realm.Write(() =>
                                {
                                    realm.RemoveAll<PaymentOptionDB>();
                                });

                                //CommonQueries.InvalidateAllObjects<PaymentOptionDto>();
                                await paymentService.GetRemotePaymentOptions(Priority.UserInitiated, true);

                                if (Settings.IsRestaurantPOS)
                                {                                    
                                    await restaurantService.GetFloors(Priority.UserInitiated, true, new GetFloorInput()
                                    {
                                        MaxResultCount = 100,
                                        sorting = "0",
                                        SkipCount = 0,
                                        Filter = "",
                                        Name = ""
                                    });
                                }

                                await GetAllNewRemoteData(processmode, RequiredAllData);
                                if (tmpPayments != null && tmpPayments.Any(x => x.Id != 0 && !x.IsDeleted && !string.IsNullOrEmpty(x.ConfigurationDetails)))
                                {
                                    foreach (var oldpayment in tmpPayments.Where(x => x.Id != 0 && !x.IsDeleted && !string.IsNullOrEmpty(x.ConfigurationDetails)))
                                    {
                                        if (oldpayment.PaymentOptionType != PaymentOptionType.Windcave)
                                        {
                                            var updatedPaymenOption = paymentService.GetLocalPaymentOption(oldpayment.Id);
                                            if (updatedPaymenOption != null)
                                            {
                                                updatedPaymenOption.ConfigurationDetails = oldpayment.ConfigurationDetails;
                                                paymentService.UpdateLocalPaymentOption(updatedPaymenOption);
                                            }
                                        }
                                    }
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
            finally
            {
                isSyncronizationRunning = false;
            }
        }


        public async Task<bool> GetAllNewRemoteData(Fusillade.Priority processmode, bool RequiredAllData)
        {
            try
            {
                LastSyncDto objLastSyncDto = objLastSyncService.GetLastSyncTime();

                if (RequiredAllData)
                {
                    //Shop details
                    UserListDto user = await userService.GetRemoteUserByEmail(processmode, Settings.CurrentUserEmail, true);

                    //#103669
                    ObservableCollection<OutletDto_POS> outletResponse = await outletService.GetRemoteOutlets(Priority.UserInitiated, true);
                    if (outletResponse != null)
                    {
                        if (outletResponse.Count == 1 && outletResponse[0] != null && outletResponse[0].OutletRegisters != null && outletResponse[0].OutletRegisters.Count == 1)
                        {
                            Settings.SelectedOutletId = outletResponse[0].Id;
                            Settings.SelectedOutletName = outletResponse[0].Title;
                            Settings.SelectedOutlet = outletResponse[0];
                            Settings.CurrentRegister = outletResponse[0].OutletRegisters[0];
                        }
                        else if(outletResponse.Count > 0 && Settings.CurrentRegister != null && Settings.CurrentRegister.OutletID > 0)
                        {
                            var fdata = outletResponse.FirstOrDefault(a=>a.Id == Settings.CurrentRegister.OutletID);
                            var isnotdelete = fdata?.OutletRegisters != null && fdata.OutletRegisters.Any(a=> a.Id == Settings.CurrentRegister.Id);
                            if(!isnotdelete)
                            {
                                Settings.SelectedOutlet = outletResponse.FirstOrDefault(a=>a.IsDefault == true) ?? outletResponse[0];
                                Settings.SelectedOutletId = Settings.SelectedOutlet .Id;
                                Settings.SelectedOutletName = Settings.SelectedOutlet .Title;
                                Settings.CurrentRegister = Settings.SelectedOutlet .OutletRegisters[0];
                            }
                        }
                    }
                    //#103669
                    
                    //Ticket start:#17420 Alert and Action for Data Purge.by rupesh
                    var purgeDataCurrentDate = Settings.StoreGeneralRule.PurgeDataCurrentDate;
                    ShopGeneralDto shopResponse = await shopService.GetRemoteShops(processmode, true);
                    if (shopResponse.GeneralRule.PurgeDataCurrentDate != null && purgeDataCurrentDate != shopResponse.GeneralRule.PurgeDataCurrentDate)
                    {
                        await SetupOutlet();//reset data if purged data.
                        return false;
                    }
                    //Ticket end:#17420 .by rupesh

                    await outletService.GetAllTemplatesReceipt(processmode, true);

                    Settings.CountryCode = shopResponse?.Address?.Country;
                    RegisterDto registerResponse = await outletService.GetRemoteRegisterById(processmode, true, Settings.CurrentRegister.Id);

                    //#30495 iOS -Change in Register API for display app option
                    if (registerResponse != null)
                    {
                        Settings.CustomerAppConfigFrom = registerResponse.CustomerDisplayConfigureType.ToString();
                        Settings.CustomerAppPin = registerResponse.CustomerDisplayConfigurePin;
                    }
                    //#30495 iOS -Change in Register API for display app option
                }


                if (objLastSyncDto == null)
                {
                    objLastSyncDto = new LastSyncDto();
                }

                List<Task> userstasks = new List<Task>();
                //Customer groups
                var beforeTimeodSyncCustomerGroup = DateTime.UtcNow;
                userstasks.Add(customerService.GetRemoteCustomerGroups(processmode, true, "", objLastSyncDto.LastCustomerGroupDateSyncTime?.ToUniversalTime()));
                objLastSyncDto.LastCustomerGroupDateSyncTime = beforeTimeodSyncCustomerGroup;

                //Customers
                var beforeTimeodSyncCustomer = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                   userstasks.Add(customerService.GetRemoteCustomers(processmode, true, "", objLastSyncDto.LastCustomerDateSyncTime?.ToUniversalTime()));
                }
                else
                {
                   userstasks.Add(customerService.GetRemoteCustomersLemda(processmode, true, "", objLastSyncDto.LastCustomerDateSyncTime?.ToUniversalTime()));
                }
                objLastSyncDto.LastCustomerDateSyncTime = beforeTimeodSyncCustomer;


                userstasks.Add(customerService.GetRemoteCustomerCustomFields(processmode, true));

                //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                if ( Settings.CurrentRegister != null && Settings.CurrentRegister.Id > 0)
                {
                    productService.ClearCurrentLayout();
                    userstasks.Add(productService.GetCurrentLayout(processmode,true,Settings.CurrentRegister.Id));
                }
                //End #90945 By Pratik

                //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer)
                {
                    //Delivery Addresses
                    var beforeTimeodSynclDeliveryAddresses = DateTime.UtcNow;
                    userstasks.Add(customerService.GetRemoteAllDeliveryAddresses(processmode, true, "", objLastSyncDto.LastDeliveryAddressDateSyncTime?.ToUniversalTime()));
                    objLastSyncDto.LastDeliveryAddressDateSyncTime = beforeTimeodSynclDeliveryAddresses;
                    //Ticket end:#26664 IOS - New feature :: Customer delivery address.by rupesh
                }

                if (RequiredAllData)
                {
                    //PaymentOptions
                    objLastSyncDto.LastPaymentDataSyncTime = DateTime.UtcNow;
                    userstasks.Add(paymentService.GetRemotePaymentOptions(processmode, true));

                    //Roles
                    userstasks.Add(userService.GetRemoteRoles(processmode, true, new RoleRequestModel()));

                    //User list
                    userstasks.Add(userService.GetRemoteUsers(processmode, true));

                    //Categories
                    //await productService.GetRemoteCategories(processmode, true);
                    userstasks.Add(productService.GetRemoteCategoriesByOutlet(Priority.UserInitiated, true, Settings.SelectedOutletId));

                    //Offers
                    objLastSyncDto.LastOfferDataSyncTime = DateTime.UtcNow;
                    userstasks.Add(offerService.GetRemoteOffers(processmode, true));
                }

                await Task.WhenAll(userstasks);
                //Tax List
                ObservableCollection<TaxDto> taxes;
                taxes = await taxServices.GetRemoteTaxes(Priority.UserInitiated, true);

                //ProductSocks
                var beforeTimeofSyncProductStock = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    var productStocks = await productService.GetRemoteProductStocks(processmode, true, Settings.SelectedOutletId, "", objLastSyncDto.LastProductStockDataSyncTime?.ToUniversalTime());
                }
                else
                {
                    var productStocks = await productService.GetRemoteProductStocksLambda(processmode, true, Settings.SelectedOutletId, "", objLastSyncDto.LastProductStockDataSyncTime?.ToUniversalTime());
                }
                //Set updated product stock to respective products
                // await productService.UpdateProductStocks(productStocks,taxes);
                objLastSyncDto.LastProductStockDataSyncTime = beforeTimeofSyncProductStock;

                //Products
                var beforeTimeodSyncProduct = DateTime.UtcNow;
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    var products = await productService.GetRemoteProducts(processmode, true, Settings.SelectedOutletId, "", objLastSyncDto.LastProductDataSyncTime?.ToUniversalTime());
                }
                else
                {
                    var products = await productService.GetRemoteProductsLambda(processmode, true, Settings.SelectedOutletId, "", objLastSyncDto.LastProductDataSyncTime?.ToUniversalTime(), taxes);
                }
                objLastSyncDto.LastProductDataSyncTime = beforeTimeodSyncProduct;

                List<Task> salehistorytasks = new List<Task>();

                //UnityOfMeasures
                var beforeUnityOfMeasureSyncTime = DateTime.UtcNow;
                salehistorytasks.Add(productService.GetRemoteUnityOfMeasures(Priority.UserInitiated, true, Settings.SelectedOutletId, "", objLastSyncDto.LastUnityOfMeasureSyncTime?.ToUniversalTime()));
                objLastSyncDto.LastUnityOfMeasureSyncTime = beforeUnityOfMeasureSyncTime;

                //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                //Ticket start:#30282 Suggested payment amount by product tag during checkout.by rupesh
                //ProductTags
                // if (Settings.StoreGeneralRule.AutoSuggestPaymentByTag)
                // {
                    var beforeProductTagsDataSyncTime = DateTime.UtcNow;
                    salehistorytasks.Add(productService.GetRemoteProductTags(Priority.UserInitiated, true, objLastSyncDto.LastProductTagsDataSyncTime?.ToUniversalTime()));
                    objLastSyncDto.LastProductTagsDataSyncTime = beforeProductTagsDataSyncTime;
                // }
                //Ticket end:#30282 .by rupesh
                //End #84438 by Pratik

                //Sales
                //Get all data expect Parked and LayBy
                DateTime ToDate = DateTime.UtcNow;
                DateTime Fromdate = DateTime.UtcNow.AddDays(-7);
                if (objLastSyncDto.LastInvoiceDataSyncTime != null)
                {
                    Fromdate = objLastSyncDto.LastInvoiceDataSyncTime.Value.ToUniversalTime();
                }
                List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Completed, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.Exchange, InvoiceStatus.Quote };
                salehistorytasks.Add(saleService.GetRemoteInvoices(processmode, true, Settings.SelectedOutletId, Fromdate, ToDate, status));

                //Get all Parked and LayBy data
                //Fromdate = DateTime.UtcNow.AddYears(-7);
                status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Parked, InvoiceStatus.LayBy };
                salehistorytasks.Add(saleService.GetRemoteInvoices(processmode, true, Settings.SelectedOutletId, Fromdate, ToDate, status));
                await Task.WhenAll(salehistorytasks);
                objLastSyncDto.LastInvoiceDataSyncTime = ToDate;
                UpdateSyncTime(objLastSyncDto);

                //Get all Countries
                Settings.AllCountries = await commonLookupServices.GetRemoteAllCountries(processmode, true);

                //Get all hearabout us
                Settings.AllHearAbout = await hearAboutService.GetRemoteAllHearAbout(processmode, true);

                var AllClockInOutUsers = await userService.GetAllClockInOutUsers(Priority.UserInitiated, true);

                await outletService.GetRemoteDenomination(processmode, true);

                await UpdateLocalTerminalIpAddress();

                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        private async Task UpdateLocalTerminalIpAddress()
        {
            var paymentOptions = paymentService.GetLocalPaymentOptions();

            var filteredPaymentOptions = paymentOptions
                .Where(x => x.PaymentOptionType == PaymentOptionType.HikePay &&
                       (x.RegisterPaymentOptions == null ||
                        x.RegisterPaymentOptions.Count < 1 ||
                        x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id)))
                .ToList();

            foreach (var payment in filteredPaymentOptions)
            {
                try
                {
                    var paymentConfig = JsonConvert.DeserializeObject<HikePOS.Models.Payment.NadaPayConfigurationDto>(payment.ConfigurationDetails);

                    if (paymentConfig == null || string.IsNullOrEmpty(paymentConfig.SerialNumber))
                        continue;

                    var paymentTerminal = await hikePayService.GetHikePayTerminals(
                        Priority.UserInitiated,
                        paymentConfig.SerialNumber);

                    var terminal = paymentTerminal?.result?.Data?.FirstOrDefault();
                    var ipAddress = terminal?.connectivity?.wifi?.ipAddress;

                    if (!string.IsNullOrWhiteSpace(ipAddress))
                    {
                        paymentConfig.IpAddress = ipAddress;
                        payment.ConfigurationDetails = JsonConvert.SerializeObject(paymentConfig);

                        paymentService.UpdateLocalPaymentOption(payment);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }

        }

    }
}
