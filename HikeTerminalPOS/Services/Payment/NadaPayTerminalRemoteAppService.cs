using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Newtonsoft.Json;

namespace HikePOS.Services
{
    public class NadaPayTerminalRemoteAppService : INadaPayTerminalRemoteAppService
    {
        readonly IApiService<IAccountApi> accountApiService;
        readonly AccountServices accountService;
        public NadaPayTerminalRemoteAppService()
        {
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        public async Task<HikePayTerminalResponse> CreateNadaPayTerminalSale(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption, string InvoiceSyncReference)
        {
            ResponseModel<HikePayTerminalResponse> hikePayTerminalResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                    HikePayTerminalRequest hikePayTerminalRequest = PrepareHikePayTerminalRequest(amount, currency, lastReference, paymentOption);
                    Settings.HikePayVerify = new HikePayVerify { InvoiceSyncReference = InvoiceSyncReference, PaymentId = paymentOption.Id, ServiceId = hikePayTerminalRequest.SaleToPOIRequest.MessageHeader.ServiceID,SaleReference = lastReference };
                    using (var httpClient = new HttpClient())
                    {
                    AgainCall:
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                        httpClient.Timeout = new TimeSpan(0, 10, 0);
                        var data = JsonConvert.SerializeObject(hikePayTerminalRequest);
                        var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");


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

                        var response = await httpClient.PostAsync(url + "/api/services/app/nadaPayTerminal/CreateNadaPayTerminalSale", content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                hikePayTerminalResponse = JsonConvert.DeserializeObject<ResponseModel<HikePayTerminalResponse>>(json);
                                if (hikePayTerminalResponse.result.IsTaskCanceledException)
                                {
                                    hikePayTerminalResponse.result = await VerifyNadaPayTerminalSale(hikePayTerminalRequest);
                                }
                                if (hikePayTerminalResponse.success)
                                    Settings.HikePayVerify = null;
                            }
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            bool res = await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated);
                            if (res)
                            {
                                goto AgainCall;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            if (hikePayTerminalResponse != null && hikePayTerminalResponse.success && hikePayTerminalResponse.result != null)
            {
                return hikePayTerminalResponse.result;
            }
            else
            {
                if (hikePayTerminalResponse != null && hikePayTerminalResponse.error != null && hikePayTerminalResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hikePayTerminalResponse.error.message, Colors.Red, Colors.White);
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);

                }
                return hikePayTerminalResponse?.result;
            }
        }

        private HikePayTerminalRequest PrepareHikePayTerminalRequest(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption, MessageCategoryType messageCategoryType = MessageCategoryType.Payment, string serviceId = null)
        {
            var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
            HikePayTerminalRequest hikePayTerminalRequest = new HikePayTerminalRequest()
            {
                TenantId = Settings.TenantId,
                StoreId = config.StoreReferenceId,
                PaymentId = paymentOption.Id,
                MainSaleServiceId = messageCategoryType == MessageCategoryType.Abort ? serviceId : null,
                SaleToPOIRequest = new SaleToPOIRequest()
                {
                    MessageHeader = new MessageHeaderTerminal()
                    {
                        ServiceID = string.IsNullOrEmpty(serviceId) ? CommonMethods.Generate10CharsId() : serviceId,
                        MessageClass = (int)MessageClassType.Service,
                        MessageCategory = (int)messageCategoryType,
                        MessageType = (int)MessageHeaderTerminalMessageType.Response,
                        SaleID = "Hike_" + Settings.TenantId + "_" + lastReference,
                        ProtocolVersion = "3.0",
                        POIID = config.TerminalId,
                    },
                    MessagePayload = new PaymentRequest()
                    {
                        SaleData = messageCategoryType == MessageCategoryType.Abort ? null : new SaleData
                        {
                            SaleTransactionID = new SaleTransactionID
                            {
                                TransactionID = Guid.NewGuid().ToString(),
                                TimeStamp = DateTime.UtcNow
                            },
                            SaleToAcquirerData = config.StoreReferenceId,
                        },
                        AbortReason = messageCategoryType == MessageCategoryType.Abort ? "MerchantAbort" : null,
                        MessageReference = messageCategoryType == MessageCategoryType.Abort ? new MessageReference()
                        {
                            MessageCategory = "Abort",
                            SaleID = "Hike_" + Settings.TenantId + "_" + lastReference,
                            ServiceID = serviceId,
                        } : null,
                        PaymentTransaction = new PaymentTransaction()
                        {
                            AmountsReq = new AmountsReq()
                            {
                                Currency = currency,
                                RequestedAmount = amount
                            }
                        }
                    }
                }
            };
            return hikePayTerminalRequest;
        }

        public async Task<HikePayTerminalResponse> CreateNadaPayTerminalRefund(bool isPartial, decimal amount, string currency, string saleId, string merchantReference,
        string poiTransactionId, DateTime poiTransactionTimeStamp, decimal surcharge, decimal fixedCommission, decimal variableCommissionPercentage,
        decimal paymentFee, PaymentOptionDto paymentOption, NadaPayTransactionDto nadaPayTransactionDto)
        {
            ResponseModel<HikePayTerminalResponse> hikePayTerminalResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                    HikePayTerminalRequest hikePayTerminalRequest = new HikePayTerminalRequest()
                    {
                        TenantId = Settings.TenantId,
                        StoreId = config.StoreId,
                        PaymentId = paymentOption.Id,
                        SurchargeAmount = surcharge,
                        PaymentMethod = nadaPayTransactionDto.PaymentMethod,
                        PoiTransactionId = poiTransactionId,
                        isFullRefund = !isPartial,
                        SaleRefundTransactionId = merchantReference,
                        BalanceAccountId = config.BalanceAccountId,


                        SaleToPOIRequest = new SaleToPOIRequest()
                        {
                            MessageHeader = new MessageHeaderTerminal()
                            {
                                ServiceID = CommonMethods.Generate10CharsId(),
                                MessageClass = (int)MessageClassType.Service,
                                MessageCategory = (int)MessageCategoryType.Reversal,
                                MessageType = (int)MessageHeaderTerminalMessageType.Response,
                                SaleID = "Hike_" + Settings.TenantId + "_" + saleId + "_refund",
                                ProtocolVersion = "3.0",
                                POIID = config.TerminalId,
                            },
                            MessagePayload = new PaymentRequest()
                            {
                                SaleData = new SaleData
                                {
                                    SaleTransactionID = new SaleTransactionID
                                    {
                                        TransactionID = merchantReference,
                                        TimeStamp = DateTime.UtcNow
                                    },
                                    SaleToAcquirerData = config.StoreReferenceId,
                                },
                                PaymentTransaction = new PaymentTransaction()
                                {
                                    AmountsReq = new AmountsReq()
                                    {
                                        Currency = currency,
                                        RequestedAmount = amount
                                    }
                                }
                            }
                        }
                    };


                    using (var httpClient = new HttpClient())
                    {
                    AgainCall:
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                        httpClient.Timeout = new TimeSpan(0, 10, 0);
                        var data = JsonConvert.SerializeObject(hikePayTerminalRequest);
                        var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");


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

                        var response = await httpClient.PostAsync(url + "/api/services/app/nadaPayTerminal/CreateNadaPayTerminalRefund", content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                hikePayTerminalResponse = JsonConvert.DeserializeObject<ResponseModel<HikePayTerminalResponse>>(json);
                            }
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            bool res = await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated);
                            if (res)
                            {
                                goto AgainCall;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            if (hikePayTerminalResponse != null && hikePayTerminalResponse.success && hikePayTerminalResponse.result != null)
            {
                return hikePayTerminalResponse.result;
            }
            else
            {
                if (hikePayTerminalResponse != null && hikePayTerminalResponse.error != null && hikePayTerminalResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hikePayTerminalResponse.error.message, Colors.Red, Colors.White);
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                }
                return hikePayTerminalResponse?.result;
            }
        }

        public async Task<HikePayTerminalResponse> VerifyNadaPayTerminalSale(HikePayTerminalRequest hikePayTerminalRequest)
        {
            ResponseModel<HikePayTerminalResponse> hikePayTerminalResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                AgainCall:
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                        httpClient.Timeout = new TimeSpan(0, 10, 0);
                        var data = JsonConvert.SerializeObject(hikePayTerminalRequest);
                        var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");


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

                        var response = await httpClient.PostAsync(url + "/api/services/app/nadaPayTerminal/VerifyNadaPayTerminalSale", content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Debug.WriteLine("VerifyNadapay:" + json);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                hikePayTerminalResponse = JsonConvert.DeserializeObject<ResponseModel<HikePayTerminalResponse>>(json);
                                if (hikePayTerminalResponse.result.IsInProgress)
                                {
                                    await Task.Delay(5000);
                                    goto AgainCall;
                                }

                            }
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            bool res = await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated);
                            if (res)
                            {
                                goto AgainCall;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            if (hikePayTerminalResponse != null && hikePayTerminalResponse.success && hikePayTerminalResponse.result != null)
            {
                return hikePayTerminalResponse.result;
            }
            else
            {
                if (hikePayTerminalResponse != null && hikePayTerminalResponse.error != null && hikePayTerminalResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hikePayTerminalResponse.error.message, Colors.Red, Colors.White);
                }
                return hikePayTerminalResponse?.result;
            }
        }

        public async Task<HikePayTerminalResponse> CreateNadaPayTerminalCancel(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption, string serviceId)
        {
            ResponseModel<HikePayTerminalResponse> hikePayTerminalResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    HikePayTerminalRequest hikePayTerminalRequest = PrepareHikePayTerminalRequest(amount, currency, lastReference, paymentOption, MessageCategoryType.Abort,serviceId);
                    using (var httpClient = new HttpClient())
                    {
                    AgainCall:
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                        httpClient.Timeout = new TimeSpan(0, 10, 0);
                        var data = JsonConvert.SerializeObject(hikePayTerminalRequest);
                        var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");


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

                        var response = await httpClient.PostAsync(url + "/api/services/app/nadaPayTerminal/CreateNadaPayTerminalCancel", content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                hikePayTerminalResponse = JsonConvert.DeserializeObject<ResponseModel<HikePayTerminalResponse>>(json);
                            }
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            bool res = await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated);
                            if (res)
                            {
                                goto AgainCall;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            if (hikePayTerminalResponse != null && hikePayTerminalResponse.success && hikePayTerminalResponse.result != null)
            {
                return hikePayTerminalResponse.result;
            }
            else
            {
                if (hikePayTerminalResponse != null && hikePayTerminalResponse.error != null && hikePayTerminalResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hikePayTerminalResponse.error.message, Colors.Red, Colors.White);
                }
                return hikePayTerminalResponse?.result;
            }
        }
       
        public async Task<HikePayTerminalResponse> VerifyNadaPayTerminalSale(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption,string serviceId)
        {
            ResponseModel<HikePayTerminalResponse> hikePayTerminalResponse = null;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                AgainCall:
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", Settings.AccessToken.Replace("bearer ", "")); ///new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                        httpClient.Timeout = new TimeSpan(0, 10, 0);
                        HikePayTerminalRequest hikePayTerminalRequest = PrepareHikePayTerminalRequest(amount, currency, lastReference, paymentOption, MessageCategoryType.Abort,serviceId);
                        var data = JsonConvert.SerializeObject(hikePayTerminalRequest);
                        var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");


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

                        var response = await httpClient.PostAsync(url + "/api/services/app/nadaPayTerminal/VerifyNadaPayTerminalSale", content).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                hikePayTerminalResponse = JsonConvert.DeserializeObject<ResponseModel<HikePayTerminalResponse>>(json);
                                if (hikePayTerminalResponse.result.IsInProgress)
                                {
                                    await Task.Delay(5000);
                                    goto AgainCall;
                                }
                                if (hikePayTerminalResponse.success)
                                    Settings.HikePayVerify = null;

                            }
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            bool res = await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated);
                            if (res)
                            {
                                goto AgainCall;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "An error occurred while sending the request")
                    {
                        App.Instance.Hud.DisplayToast(ex.Message, Colors.Red, Colors.White);
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

            if (hikePayTerminalResponse != null && hikePayTerminalResponse.success && hikePayTerminalResponse.result != null)
            {
                return hikePayTerminalResponse.result;
            }
            else
            {
                if (hikePayTerminalResponse != null && hikePayTerminalResponse.error != null && hikePayTerminalResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(hikePayTerminalResponse.error.message, Colors.Red, Colors.White);
                }
                return hikePayTerminalResponse?.result;
            }
        }
    }
}
