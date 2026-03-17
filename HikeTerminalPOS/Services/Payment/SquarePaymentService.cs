using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Newtonsoft.Json;
using Polly;
using Refit;

namespace HikePOS.Services.Payment
{
    public class SquarePaymentService
    {
        private readonly IApiService<ISquarePaymentApi> squarePaymentAPIService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;
        string referenceId = Settings.TenantId + "_" + Settings.CurrentUser.Id;
        //Ticket start:#89734.by rupesh
        double[] delayTimes = new double[] { 15, 15, 15 };
        //Ticket end:#89734.by rupesh

        public SquarePaymentService(IApiService<ISquarePaymentApi> apiService)
        {
            squarePaymentAPIService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        string GetCurrency()
        {
            var currency = Settings.StoreZoneAndFormatDetail?.Currency;
            if (string.IsNullOrEmpty(currency))
                currency = SquarePaymentConfigurationDto.Def_Currency;
            return currency;
        }

        public async Task<SquareTerminalCheckoutResponse> CreateSquareTerminalCheckout(Priority priority, InvoiceDto invoice, PaymentOptionDto paymentOptionDto, SquarePaymentConfigurationDto configurationDto)
        {
            try
            {
                ResponseModel<SquareTerminalCheckoutResponse> response = null;

                Task<ResponseModel<SquareTerminalCheckoutResponse>> squareTask;

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                var amountInCents = (invoice.TenderAmount + (paymentOptionDto.DisplaySurcharge ?? 0)) * 100;
                //End ticket #73190  By Rupesh
                var deviceId = configurationDto.registerLocation?.FirstOrDefault(
                              x => Settings.SelectedOutletId == x.outletId && Settings.CurrentRegister.Id == x.registerId)?.deviceDetails?.deviceId;

                var checkOutRequest = new SquareTerminalCheckOutRequest
                {
                    paymentId = paymentOptionDto.Id,
                    squareConfiguratonDetail = paymentOptionDto.ConfigurationDetails,
                    squareCheckOutRequest = new SquareCheckOutRequest
                    {
                        idempotency_key = Guid.NewGuid().ToString(),
                        checkout = new Checkout
                        {
                            note = invoice.Note,
                            reference_id = referenceId,
                            amount_money = new AmountMoney
                            {
                                amount = (int)amountInCents,
                                currency = GetCurrency()
                            },
                            device_options = new DeviceOptions
                            {
                                device_id = deviceId
                            }
                        }
                    }
                };

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                squareTask = squarePaymentAPIService.Background.CreateSquareTerminalCheckout(checkOutRequest,Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                squareTask = squarePaymentAPIService.UserInitiated.CreateSquareTerminalCheckout(checkOutRequest,Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                squareTask = squarePaymentAPIService.Speculative.CreateSquareTerminalCheckout(checkOutRequest,Settings.AccessToken);
                                break;
                            default:
                                squareTask = squarePaymentAPIService.Background.CreateSquareTerminalCheckout(checkOutRequest,Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await squareTask);


                        SquareTerminalCheckoutResponse result = response.result;

                        Debug.WriteLine("Square terminal payment Response: " + JsonConvert.SerializeObject(result));

                        if (result != null && result.errors != null && result.errors.Count() > 0)
                        {
                            result.errors.ForEach(x =>
                            {
                                App.Instance.Hud.DisplayToast(x.detail, Colors.Red, Colors.White);
                            }
                            );
                        }
                        else
                        {
                            int delayCounter = 0;
                            bool isOveridePaid = false;
                        GetCheckoutStatus:
                            var checkoutId = result?.checkout?.id;
                            var status = result?.checkout.status;



                            if (status != SquarePaymentConfigurationDto.TERMINAL_STATUS_COMPLETED)
                            {
                                //Ticket #12397 Start : Payment Cancellation Scenario on Square Terminal. By Nikhil	
                                if (status == SquarePaymentConfigurationDto.TERMINAL_STATUS_CANCELED)
                                {
                                    //Ticket start:#31203 iOS - Override Square as Paid.by rupesh
                                    if (!isOveridePaid)
                                    {
                                        //Ticket start:#36328 iOS - Text Update for Overriding Square.by rupesh
                                        var isOveride = await App.Alert.ShowAlert("", LanguageExtension.Localize("SquareCancelMessage"), "Ignore & mark order as paid", "Cancel the order");
                                        //Ticket end:#36328 .by rupesh
                                        if (isOveride)
                                        {
                                            result = await GetSquareTerminalPaymentStatus(priority, paymentOptionDto, checkoutId);
                                            isOveridePaid = true;
                                            goto GetCheckoutStatus;

                                        }
                                        else
                                        {
                                            App.Instance.Hud.DisplayToast(status, Colors.Red, Colors.White);

                                        }
                                    }
                                    else
                                    {
                                        result.checkout.payment_ids = new string[] { result.checkout.id };
                                    }
                                    //Ticket end:#31203 .by rupesh
                                }
                                else
                                {
                                    //Ticket #12397 End. By Nikhil

                                    if (delayCounter >= delayTimes.Length)
                                    {

                                    //Ticket start:#23175 iOS - Retry for Square.by rupesh
                                    SetRetry:
                                        var retry = await App.Alert.ShowAlert("", LanguageExtension.Localize("SquareWarningTitle"), "Retry", "Cancel");
                                        if (retry)
                                        {
                                            delayCounter = 0;
                                            goto GetCheckoutStatus;

                                        }
                                        else
                                        {
                                            //Timeout. Cancel Transaction. 
                                            result = await CancelSquareTerminalCheckout(priority, paymentOptionDto, checkoutId);
                                            if(result?.checkout?.status == SquarePaymentConfigurationDto.TERMINAL_STATUS_IN_PROGRESS)
                                            {
                                                await App.Alert.ShowAlert("", "Press Cancel On Terminal", "Ok");
                                                goto SetRetry;

                                            }
                                        }
                                        //Ticket end:#23175 iOS - Retry for Square.by rupesh

                                    }
                                    else
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(delayTimes[delayCounter]));
                                        delayCounter++;
                                        result = await GetSquareTerminalPaymentStatus(priority, paymentOptionDto, checkoutId);
                                        goto GetCheckoutStatus;
                                    }
                                }
                            }

                            return result;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateSquareTerminalCheckout Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<SquareTerminalCheckoutResponse>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateSquareTerminalCheckout Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(squarePaymentAPIService.ApiBaseAddress);
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
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("CreateSquareTerminalCheckout Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }


        public async Task<SquareTerminalCheckoutResponse> GetSquareTerminalPaymentStatus(Priority priority, PaymentOptionDto paymentOptionDto, string checkoutId)
        {
            try
            {
                ResponseModel<SquareTerminalCheckoutResponse> response = null;

                Task<ResponseModel<SquareTerminalCheckoutResponse>> squareTask;

                var statusRequest = new SquareTerminalPaymentStatusRequest
                {
                    paymentId = paymentOptionDto.Id,
                    squareConfiguratonDetail = paymentOptionDto.ConfigurationDetails,
                    checkoutId = checkoutId
                };

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                squareTask = squarePaymentAPIService.Background.GetSquareTerminalPaymentStatus(statusRequest,Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                squareTask = squarePaymentAPIService.UserInitiated.GetSquareTerminalPaymentStatus(statusRequest,Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                squareTask = squarePaymentAPIService.Speculative.GetSquareTerminalPaymentStatus(statusRequest,Settings.AccessToken);
                                break;
                            default:
                                squareTask = squarePaymentAPIService.Background.GetSquareTerminalPaymentStatus(statusRequest,Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await squareTask);


                        SquareTerminalCheckoutResponse result = response.result;

                        Debug.WriteLine("Square terminal payment Response: " + JsonConvert.SerializeObject(result));

                        if (result != null && result.errors != null && result.errors.Count() > 0)
                        {
                            result.errors.ForEach(x =>
                            {
                                App.Instance.Hud.DisplayToast(x.detail, Colors.Red, Colors.White);
                            }
                            );
                        }
                        else
                        {
                            return result;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("GetSquareTerminalPaymentStatus Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<SquareTerminalCheckoutResponse>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("GetSquareTerminalPaymentStatus Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(squarePaymentAPIService.ApiBaseAddress);
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
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("GetSquareTerminalPaymentStatus Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }


        public async Task<SquareTerminalCheckoutResponse> CancelSquareTerminalCheckout(Priority priority, PaymentOptionDto paymentOptionDto, string checkoutId)
        {
            try
            {
                ResponseModel<SquareTerminalCheckoutResponse> response = null;

                Task<ResponseModel<SquareTerminalCheckoutResponse>> squareTask;

                var cancelRequest = new SquareTerminalPaymentStatusRequest
                {
                    paymentId = paymentOptionDto.Id,
                    squareConfiguratonDetail = paymentOptionDto.ConfigurationDetails,
                    checkoutId = checkoutId
                };


                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                squareTask = squarePaymentAPIService.Background.CancelSquareTerminalCheckout(cancelRequest,Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                squareTask = squarePaymentAPIService.UserInitiated.CancelSquareTerminalCheckout(cancelRequest,Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                squareTask = squarePaymentAPIService.Speculative.CancelSquareTerminalCheckout(cancelRequest,Settings.AccessToken);
                                break;
                            default:
                                squareTask = squarePaymentAPIService.Background.CancelSquareTerminalCheckout(cancelRequest,Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await squareTask);


                        SquareTerminalCheckoutResponse result = response.result;

                        Debug.WriteLine("Square terminal payment Response: " + JsonConvert.SerializeObject(result));

                        if (result != null && result.errors != null && result.errors.Count() > 0)
                        {

                            result.errors.ForEach(x =>
                            {
                                App.Instance.Hud.DisplayToast(x.detail, Colors.Red, Colors.White);
                            }
                            );
                        }
                        else
                        {
                            var status = result?.checkout?.status;
                            //Ticket start:#23175 iOS - Retry for Square.by rupesh
                            if (status == SquarePaymentConfigurationDto.TERMINAL_STATUS_CANCELED)
                            {
                                App.Instance.Hud.DisplayToast(status, Colors.Red, Colors.White);

                            }
                            else if (status == SquarePaymentConfigurationDto.TERMINAL_STATUS_COMPLETED)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SquareOrderAlreadyProcessed"), Colors.Red, Colors.White);

                            }
                            //Ticket end:#23175 iOS - Retry for Square.by rupesh
                            else if (status == SquarePaymentConfigurationDto.TERMINAL_STATUS_PENDING)
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Timeout. Press Cancel On Terminal"), Colors.Red, Colors.White);
                            //else
                            //    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Timeout. Transaction Cancelled"), Colors.Red, Colors.White);
                            return result;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CancelSquareTerminalCheckout Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<SquareTerminalCheckoutResponse>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CancelSquareTerminalCheckout Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(squarePaymentAPIService.ApiBaseAddress);
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
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("CancelSquareTerminalCheckout Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }


        public async Task<SquareTerminalRefundResponse> CreateSquareTerminalRefund(Priority priority, InvoiceDto invoice, PaymentOptionDto paymentOptionDto, string transactionId)
        {
            try
            {
                ResponseModel<SquareTerminalRefundResponse> response = null;

                Task<ResponseModel<SquareTerminalRefundResponse>> squareTask;

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                var amountInCents = ((invoice.TenderAmount * 100) + (paymentOptionDto.DisplaySurcharge ?? 0)).ToPositive();
                //End ticket #73190  By Rupesh


                var refundRequest = new SquareTerminalRefundRequest
                {
                    paymentId = paymentOptionDto.Id,
                    squareConfiguratonDetail = paymentOptionDto.ConfigurationDetails,
                    squareRefundRequestTerminal = new SquareRefundRequestTerminal
                    {
                        payment_id = transactionId,
                        reason = referenceId,
                        idempotency_key = Guid.NewGuid().ToString(),
                        amount_money = new AmountMoney
                        {
                            amount = (int)amountInCents,
                            currency = GetCurrency()
                        }
                    }
                };

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                squareTask = squarePaymentAPIService.Background.CreateSquareTerminalRefund(refundRequest,Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                squareTask = squarePaymentAPIService.UserInitiated.CreateSquareTerminalRefund(refundRequest,Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                squareTask = squarePaymentAPIService.Speculative.CreateSquareTerminalRefund(refundRequest,Settings.AccessToken);
                                break;
                            default:
                                squareTask = squarePaymentAPIService.Background.CreateSquareTerminalRefund(refundRequest,Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await squareTask);


                        SquareTerminalRefundResponse result = response.result;

                        Debug.WriteLine("Square terminal refund Response: " + JsonConvert.SerializeObject(result));

                        if (result != null && result.errors != null && result.errors.Count() > 0)
                        {
                            result.errors.ForEach(x =>
                            {
                                App.Instance.Hud.DisplayToast(x.detail, Colors.Red, Colors.White);
                            }
                            );
                        }
                        else
                        {
                            int delayCounter = 0;

                        GetRefundStatus:
                            var refundId = result?.refund?.id;
                            var status = result?.refund.status;

                            if (status == SquarePaymentConfigurationDto.TERMINAL_STATUS_PENDING)
                            {
                                if (delayCounter >= 1)//We will stop after 1 try for refund
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("The refund request is successfully submitted for processing."), Colors.Green, Colors.White);
                                    return result;
                                }
                                else
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(delayTimes[delayCounter]));
                                    delayCounter++;
                                    var refundStatus = await GetSquareTerminalRefundStatus(priority, paymentOptionDto, refundId);
                                    if (refundStatus != null && refundStatus.refund != null)
                                        result.refund = refundStatus.refund;

                                    goto GetRefundStatus;
                                }
                            }

                            return result;
                        }
                    }
                    catch (ApiException ex)
                    {
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateSquareTerminalRefund Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<SquareTerminalRefundResponse>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                        Logger.SaleLogger("CreateSquareTerminalRefund Exception Msg - " + ex.Message);
                        //Ticket end:#61832.by rupesh

                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(squarePaymentAPIService.ApiBaseAddress);
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
                //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
                Logger.SaleLogger("CreateSquareTerminalRefund Exception Msg - " + ex.Message);
                //Ticket end:#61832.by rupesh

                ex.Track();
                return null;
            }
            return null;
        }


        public async Task<SquareTerminalRefundStatusRepsponse> GetSquareTerminalRefundStatus(Priority priority, PaymentOptionDto paymentOptionDto, string refundId)
        {
            try
            {
                ResponseModel<SquareTerminalRefundStatusRepsponse> response = null;

                Task<ResponseModel<SquareTerminalRefundStatusRepsponse>> squareTask;

                var statusRequest = new SquareTerminalRefundStatusRequest
                {
                    paymentId = paymentOptionDto.Id,
                    squareConfiguratonDetail = paymentOptionDto.ConfigurationDetails,
                    refundId = refundId
                };

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {

                Retry1:

                    try
                    {
                        switch (priority)
                        {
                            case Priority.Background:
                                squareTask = squarePaymentAPIService.Background.GetSquareTerminalRefundStatus(statusRequest,Settings.AccessToken);
                                break;
                            case Priority.UserInitiated:
                                squareTask = squarePaymentAPIService.UserInitiated.GetSquareTerminalRefundStatus(statusRequest,Settings.AccessToken);
                                break;
                            case Priority.Speculative:
                                squareTask = squarePaymentAPIService.Speculative.GetSquareTerminalRefundStatus(statusRequest,Settings.AccessToken);
                                break;
                            default:
                                squareTask = squarePaymentAPIService.Background.GetSquareTerminalRefundStatus(statusRequest,Settings.AccessToken);
                                break;
                        }

                        response = await Policy
                             .Handle<ApiException>()
                             .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                             .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                             .ExecuteAsync(async () => await squareTask);


                        SquareTerminalRefundStatusRepsponse result = response.result;

                        Debug.WriteLine("Square terminal refund Response: " + JsonConvert.SerializeObject(result));

                        if (result != null && result.errors != null && result.errors.Count() > 0)
                        {
                            result.errors.ForEach(x =>
                            {
                                App.Instance.Hud.DisplayToast(x.detail, Colors.Red, Colors.White);
                            }
                            );
                        }
                        else
                        {
                            return result;
                        }
                    }
                    catch (ApiException ex)
                    {
                        // Get Exception content
                        response = await ex.GetContentAsAsync<ResponseModel<SquareTerminalRefundStatusRepsponse>>();
                        if (response != null && response.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        if (priority != Priority.Background)
                        {
                            if (ex.Message == "An error occurred while sending the request")
                            {
                                bool isReachable = await CommonMethods.ReachableCheck(squarePaymentAPIService.ApiBaseAddress);
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
                return null;
            }
            return null;
        }
    }
}
