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
using System.Diagnostics;

namespace HikePOS.Services
{
	public class InventoriesServices
	{
		private readonly IApiService<IInventoriesApi> _apiService;
		private readonly IApiService<IAccountApi> accountApiService;
		private readonly AccountServices accountService;

		public InventoriesServices(IApiService<IInventoriesApi> apiService)
		{
			_apiService = apiService;
			accountApiService = new ApiService<IAccountApi>();
			accountService = new AccountServices(accountApiService);
		}

		public async Task<ObservableCollection<PurchaseOrderDto>> GetRemotePurchaseOrders(Priority priority, bool syncLocal, int InvoiceId)
		{
			ResponseListModel<PurchaseOrderDto> purchaseOrderResponse = null;

			Task<ResponseListModel<PurchaseOrderDto>> purchaseOrderTask;

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{

			Retry:
				switch (priority)
				{
					case Priority.Background:
						purchaseOrderTask = _apiService.Background.GetPOFromSale(InvoiceId, Settings.AccessToken);
						break;
					case Priority.UserInitiated:
						purchaseOrderTask = _apiService.UserInitiated.GetPOFromSale(InvoiceId, Settings.AccessToken);
						break;
					case Priority.Speculative:
						purchaseOrderTask = _apiService.Speculative.GetPOFromSale(InvoiceId, Settings.AccessToken);
						break;
					default:
						purchaseOrderTask = _apiService.UserInitiated.GetPOFromSale(InvoiceId, Settings.AccessToken);
						break;
				}


				try
				{
					purchaseOrderResponse = await Policy
						.Handle<Exception>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
						.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await purchaseOrderTask);
				}
				catch (ApiException ex)
				{
					//Get Exception content
					purchaseOrderResponse = await ex.GetContentAsAsync<ResponseListModel<PurchaseOrderDto>>();

					if (purchaseOrderResponse != null && purchaseOrderResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting purchase orders.");
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

			if (purchaseOrderResponse !=null && purchaseOrderResponse.result !=null)
			{
				if (syncLocal)
				{
					UpdateLocalPurchaseOrderDto(purchaseOrderResponse.result, InvoiceId);
				}
				return purchaseOrderResponse.result;
			}
			else
			{
				App.Instance.Hud.DisplayToast(purchaseOrderResponse.error.message, Colors.Red, Colors.White);
				return null;
			}
		}
	
		public async Task<PurchaseOrderDto> CreateorUpdatePO(Priority priority, bool syncLocal, PurchaseOrderDto purchaseOrder)
		{
			ResponseModel<PurchaseOrderDto> createUpdatePOResponse = null;
			Task<ResponseModel<PurchaseOrderDto>> createUpdatePOTask;
		Retry2:
			switch (priority)
			{
				case Priority.Background:
					createUpdatePOTask = _apiService.Background.CreateorUpdatePO(purchaseOrder, Settings.AccessToken);
					break;
				case Priority.UserInitiated:
					createUpdatePOTask = _apiService.UserInitiated.CreateorUpdatePO(purchaseOrder, Settings.AccessToken);
					break;
				case Priority.Speculative:
					createUpdatePOTask = _apiService.Speculative.CreateorUpdatePO(purchaseOrder, Settings.AccessToken);
					break;
				default:
					createUpdatePOTask = _apiService.UserInitiated.CreateorUpdatePO(purchaseOrder, Settings.AccessToken);
					break;
			}

			if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
			{
				try
				{
                    createUpdatePOResponse = await Policy
						.Handle<ApiException>()
						.RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        			.WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
						.ExecuteAsync(async () => await createUpdatePOTask);

				}
				catch (ApiException ex)
				{
					createUpdatePOResponse = await ex.GetContentAsAsync<ResponseModel<PurchaseOrderDto>>();
					if (createUpdatePOResponse != null && createUpdatePOResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
			else
			{
				App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
			}



			if (createUpdatePOResponse != null && createUpdatePOResponse.success && createUpdatePOResponse.result != null)
			{
				return createUpdatePOResponse.result;
			}
			else
			{
				if (createUpdatePOResponse != null && createUpdatePOResponse.error != null && createUpdatePOResponse.error.message != null)
				{
					App.Instance.Hud.DisplayToast(createUpdatePOResponse.error.message, Colors.Red, Colors.White);
				}

				return purchaseOrder;
			}
		}
	
		public bool UpdateLocalPurchaseOrderDto(ObservableCollection<PurchaseOrderDto> purchaseOrderes, int InvoiceId)
		{
			try
			{
				if (purchaseOrderes != null && purchaseOrderes.Any())
				{
                    using var realm = RealmService.GetRealm();
                    realm.Write(() =>
                    {
                        realm.Add(purchaseOrderes.Select(a =>  a.ToModel(InvoiceId)).ToList(), update: true);
                    });
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

		public ObservableCollection<PurchaseOrderDto> GetLocalPurchaseOrder(int InvoiceId)
		{
			try
			{
                var realm = RealmService.GetRealm();
                var purchaseOrders = realm.All<PurchaseOrderDB>().Where(x=>x.InvoiceId == InvoiceId).Select(a=> PurchaseOrderDto.FromModel(a)).ToList();
				return new ObservableCollection<PurchaseOrderDto>(purchaseOrders);
            }
            catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return null;
		}

        //Ticket Start:#11847 Feature Request - Cannot link backorders to existing PO by rupesh
        public async Task<ObservableCollection<PurchaseOrderListDto>> GetPendingOrdersListBySupplier(Priority priority, int outletId, int supplierId)
        {
            ResponseModel<PurchaseOrderListResponseObjectResult> pendingOrdersListResponse = null;

            Task<ResponseModel<PurchaseOrderListResponseObjectResult>> pendingOrdersListTask;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        pendingOrdersListTask = _apiService.Background.GetPendingOrdersListBySupplier(outletId,supplierId, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        pendingOrdersListTask = _apiService.UserInitiated.GetPendingOrdersListBySupplier(outletId, supplierId, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        pendingOrdersListTask = _apiService.Speculative.GetPendingOrdersListBySupplier(outletId, supplierId, Settings.AccessToken);
                        break;
                    default:
                        pendingOrdersListTask = _apiService.UserInitiated.GetPendingOrdersListBySupplier(outletId, supplierId, Settings.AccessToken);
                        break;
                }


                try
                {
                    pendingOrdersListResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await pendingOrdersListTask);


                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    pendingOrdersListResponse = await ex.GetContentAsAsync<ResponseModel<PurchaseOrderListResponseObjectResult>>();

                    if (pendingOrdersListResponse != null && pendingOrdersListResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting PendingOrdersList By Supplier");
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

            if (pendingOrdersListResponse != null && pendingOrdersListResponse.result != null)
            {
                return pendingOrdersListResponse.result.items; 
            }
            else
            {
                App.Instance.Hud.DisplayToast(pendingOrdersListResponse.error.message, Colors.Red, Colors.White);
                return null;
            }
        }
        public async Task<bool> AssignBackorderToPO(Priority priority, int orderId, int saleInvoiceId, PurchaseOrderDto purchaseOrder)
        {
            ResponseModel<object> assignBackorderToPOResponse = null;

            Task<ResponseModel<object>> assignBackorderToPOTask;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        assignBackorderToPOTask = _apiService.Background.AssignBackorderToPO(orderId, saleInvoiceId, purchaseOrder,Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        assignBackorderToPOTask = _apiService.UserInitiated.AssignBackorderToPO(orderId, saleInvoiceId, purchaseOrder, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        assignBackorderToPOTask = _apiService.Speculative.AssignBackorderToPO(orderId, saleInvoiceId, purchaseOrder, Settings.AccessToken);
                        break;
                    default:
                        assignBackorderToPOTask = _apiService.UserInitiated.AssignBackorderToPO(orderId, saleInvoiceId, purchaseOrder, Settings.AccessToken);
                        break;
                }


                try
                {
                    assignBackorderToPOResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await assignBackorderToPOTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    assignBackorderToPOResponse = await ex.GetContentAsAsync<ResponseModel<object>>();

                    if (assignBackorderToPOResponse != null && assignBackorderToPOResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Assign Backorder To PO.");
                        }
                    }
                    return false;
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return false;
            }

            if (assignBackorderToPOResponse != null && assignBackorderToPOResponse.success)
            {
                return true;
            }
            else
            {
                App.Instance.Hud.DisplayToast(assignBackorderToPOResponse.error.message, Colors.Red, Colors.White);
                return false;
            }
        }
        //Ticket End

		 public async Task<POReferenceDto> GetPOReferenceSettting(Priority priority, int outletId)
        {
            ResponseModel<POReferenceDto> poReferenceSetttingResponse = null;

            Task<ResponseModel<POReferenceDto>> poReferenceSetttingResponseTask;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

            Retry:
                switch (priority)
                {
                    case Priority.Background:
                        poReferenceSetttingResponseTask = _apiService.Background.GetPOReferenceSettting(outletId, Settings.AccessToken);
                        break;
                    case Priority.UserInitiated:
                        poReferenceSetttingResponseTask = _apiService.UserInitiated.GetPOReferenceSettting(outletId, Settings.AccessToken);
                        break;
                    case Priority.Speculative:
                        poReferenceSetttingResponseTask = _apiService.Speculative.GetPOReferenceSettting(outletId, Settings.AccessToken);
                        break;
                    default:
                        poReferenceSetttingResponseTask = _apiService.UserInitiated.GetPOReferenceSettting(outletId, Settings.AccessToken);
                        break;
                }


                try
                {
                    poReferenceSetttingResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await poReferenceSetttingResponseTask);


                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    poReferenceSetttingResponse = await ex.GetContentAsAsync<ResponseModel<POReferenceDto>>();

                    if (poReferenceSetttingResponse != null && poReferenceSetttingResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                            Extensions.SomethingWentWrong("Getting PendingOrdersList By Supplier");
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

            if (poReferenceSetttingResponse != null && poReferenceSetttingResponse.result != null)
            {
                return poReferenceSetttingResponse.result; 
            }
            else
            {
                App.Instance.Hud.DisplayToast(poReferenceSetttingResponse.error.message, Colors.Red, Colors.White);
                return null;
            }
        }
    }
}
