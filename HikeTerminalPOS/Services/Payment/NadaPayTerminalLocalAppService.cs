using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Adyen;
using Adyen.ApiSerialization;
using Adyen.Model.TerminalApi;
using Adyen.Model.TerminalApi.Message;
using Adyen.Security;
using Adyen.Service;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Helpers;
using Adyen.Model.Terminal;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using HikePOS.Handlers;

namespace HikePOS.Services
{
    public class NadaPayTerminalLocalAppService : INadaPayTerminalLocalAppService
    {
        #region Static Reusable Infrastructure
        // private static readonly X509Certificate2 RootCert = X509CertificateLoader.LoadCertificateFromFile("adyen-terminalfleet-test.cer");
        private static readonly HttpClientHandler _handler =
// #if DEBUG
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (message, certificate2, arg3, arg4) => true
                    // ServerCertificateCustomValidationCallback =
                    //  (request, cert, chain, errors) =>
                    // {
                    //     if (errors == SslPolicyErrors.None)
                    //         return true;

                    //     if (cert == null)
                    //         return false;

                    //     // Compare thumbprint (certificate pinning)
                    //     return true;//serverCert.Thumbprint == serverCert.Thumbprint;
                    // }
            };

        // #else
        //             new HttpClientHandler();
        // #endif
        private static readonly bool IsLiveEnvironment = Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live;

        private static readonly EncryptionCredentialDetails EncryptionCredentialDetails = new EncryptionCredentialDetails
        {
            KeyVersion = 1,
            AdyenCryptoVersion = 1,
            KeyIdentifier = IsLiveEnvironment ? "hikepaylocal" : "hikepaylocal",
            Password = IsLiveEnvironment ? "123456789012!Aa" : "123456789012!Aa"
        };

        private static readonly HttpClient _httpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        private static readonly Config _config = new Config
        {
            Environment = IsLiveEnvironment ? Adyen.Model.Environment.Test : Adyen.Model.Environment.Test,
            LocalTerminalApiEndpoint = @"https://192.168.1.5:8443/nexo/"
        };

        private static readonly Client _adyenClient = new Client(_config, _httpClient);
        private static readonly SaleToPoiMessageSerializer _serializer = new();
        private static readonly SaleToPoiMessageSecuredEncryptor _encryptor = new();
        private static readonly SaleToPoiMessageSecuredSerializer _securedSerializer = new();

        #endregion

        #region Public API
        public async Task<HikePayTerminalResponse> CreateNadaPayTerminalSale(
            decimal amount,
            string currency,
            string lastReference,
            PaymentOptionDto paymentOption,
            string invoiceSyncReference)
        {
            if (!HasInternet())
                return ShowError("NoInternetMessage");
            NadaPayConfigurationDto config = null;
            string serviceID = "";
            try
            {
                config = DeserializeConfig(paymentOption);
                _adyenClient.Config.LocalTerminalApiEndpoint = $"https://{config.IpAddress}:8443/nexo/";
                var request = PreparePaymentRequest(amount, currency, lastReference, config);
                serviceID = request.MessageHeader.ServiceID;
                Settings.HikePayVerify = new HikePayVerify
                {
                    InvoiceSyncReference = invoiceSyncReference,
                    PaymentId = paymentOption.Id,
                    ServiceId = serviceID,
                    SaleReference = lastReference
                };

                var result = await ExecuteAsync(request, new CancellationToken());

                if (result?.result?.PaymentStatusSuccess == true)
                    Settings.HikePayVerify = null;

                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    var result = await VerifyWithRetryAsync(
                                           lastReference,
                                           serviceID,
                                           config,
                                           new CancellationToken());
                    return result;

                }
                return ShowError(ex.Message);
            }

        }

        public async Task<HikePayTerminalResponse> CreateNadaPayTerminalRefund(
            bool isPartial,
            decimal amount,
            string currency,
            string saleId,
            string merchantReference,
            string poiTransactionId,
            DateTime poiTransactionTimeStamp,
            decimal surcharge,
            decimal fixedCommission,
            decimal variableCommissionPercentage,
            decimal paymentFee,
            PaymentOptionDto paymentOption,
            NadaPayTransactionDto nadaPayTransactionDto)
        {
            if (!HasInternet())
                return ShowError("NoInternetMessage");

            try
            {
                var config = DeserializeConfig(paymentOption);
                _adyenClient.Config.LocalTerminalApiEndpoint = $"https://{config.IpAddress}:8443/nexo/";
                var request = PrepareReversalRequest(
                    isPartial,
                    amount,
                    currency,
                    saleId,
                    merchantReference,
                    poiTransactionId,
                    poiTransactionTimeStamp,
                    paymentOption,
                    nadaPayTransactionDto);

                var result = await ExecuteAsync(request, new CancellationToken());

                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return ShowError(ex.Message);
            }
        }

        public async Task<HikePayTerminalResponse> CreateNadaPayTerminalCancel(
            decimal amount,
            string currency,
            string lastReference,
            PaymentOptionDto paymentOption,
            string serviceId)
        {
            if (!HasInternet())
                return ShowError("NoInternetMessage");

            try
            {
                var config = DeserializeConfig(paymentOption);
                _adyenClient.Config.LocalTerminalApiEndpoint = $"https://{config.IpAddress}:8443/nexo/";
                var request = PrepareCancelRequest(lastReference, serviceId, config);

                var result = await ExecuteAsync(request, new CancellationToken());

                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return ShowError(ex.Message);
            }
        }

        public async Task<HikePayTerminalResponse> VerifyNadaPayTerminalSale(
            decimal amount,
            string currency,
            string lastReference,
            PaymentOptionDto paymentOption,
            string serviceId)
        {
            if (!HasInternet())
                return ShowError("NoInternetMessage");

            try
            {
                var config = DeserializeConfig(paymentOption);
                _adyenClient.Config.LocalTerminalApiEndpoint = $"https://{config.IpAddress}:8443/nexo/";
                var result = await VerifyWithRetryAsync(lastReference, serviceId, config, new CancellationToken());

                return result;
            }
            catch (Exception ex)
            {
                return ShowError(ex.Message);
            }
        }

        #endregion

        #region Core Execution Engine

        private async Task<ResponseModel<HikePayTerminalResponse>> ExecuteAsync(
            Adyen.Model.TerminalApi.Message.SaleToPOIRequest request,
            CancellationToken cancellationToken)
        {
            var service = new TerminalApiLocalService(
                _adyenClient,
                _serializer,
                _encryptor,
                _securedSerializer);

            var response = await service.RequestEncryptedAsync(request, EncryptionCredentialDetails, cancellationToken);

            if (response?.MessagePayload is Adyen.Model.TerminalApi.PaymentResponse paymentResponse)
            {
                var json = JsonConvert.SerializeObject(paymentResponse);
                Debug.WriteLine(json);
                byte[] bytes = Convert.FromBase64String(paymentResponse.Response.AdditionalResponse);
                string additionResponse = Encoding.UTF8.GetString(bytes);
                var responseModel = new ResponseModel<HikePayTerminalResponse>
                {
                    result = new HikePayTerminalResponse
                    {
                        PaymentStatusSuccess = paymentResponse.Response.Result == ResultType.Success,
                        PoiTransactionId = paymentResponse.POIData?.POITransactionID?.TransactionID,
                        PoiTransactionTimeStamp = paymentResponse.POIData?.POITransactionID?.TimeStamp,
                        SaleTransactionId = paymentResponse.SaleData?.SaleTransactionID?.TransactionID,
                        SaleTransactionTimeStamp = paymentResponse.SaleData?.SaleTransactionID?.TimeStamp,
                        AdditonalResponse = additionResponse,
                        PaymentReceipt = paymentResponse.PaymentReceipt,
                        IsTaskCanceledException = false,
                        IsInProgress = false,
                        ErrorConditionType = paymentResponse.Response.ErrorCondition.HasValue ? (HikePOS.Models.Payment.ErrorConditionType)paymentResponse.Response.ErrorCondition.Value : HikePOS.Models.Payment.ErrorConditionType.Aborted // or default value
                    },
                    success = paymentResponse.Response.Result == ResultType.Success

                };
                var cashierReceipt = paymentResponse.PaymentReceipt?
                    .FirstOrDefault(x => x.DocumentQualifier == DocumentQualifierType.CashierReceipt);

                if (cashierReceipt != null)
                {
                    var receiptstr = JsonConvert.SerializeObject(cashierReceipt);
                    var receipt = JsonConvert.DeserializeObject<HikePOS.Models.Payment.PaymentReceipt>(receiptstr);
                    responseModel.result.MerchantReceipt =
                        CommonMethods.HikePayReceiptBuilder.BuildReceipt(receipt);
                }

                var customerReceipt = paymentResponse.PaymentReceipt?
                    .FirstOrDefault(x => x.DocumentQualifier == DocumentQualifierType.CustomerReceipt);

                if (customerReceipt != null)
                {
                    var receiptstr = JsonConvert.SerializeObject(customerReceipt);
                    var receipt = JsonConvert.DeserializeObject<HikePOS.Models.Payment.PaymentReceipt>(receiptstr);
                    responseModel.result.CustomerReceipt =
                        CommonMethods.HikePayReceiptBuilder.BuildReceipt(receipt);
                }
                responseModel.result.RefusalReason =
                    responseModel.result.DecodeAdditonalResponse.refusalReason
                        ?? responseModel.result.DecodeAdditonalResponse.message;
                return responseModel;

            }
            else if (response?.MessagePayload is Adyen.Model.TerminalApi.ReversalResponse reversalResponse)
            {
                var json = JsonConvert.SerializeObject(reversalResponse);
                Debug.WriteLine(json);
                byte[] bytes = Convert.FromBase64String(reversalResponse.Response.AdditionalResponse);
                string additionResponse = Encoding.UTF8.GetString(bytes);
                var responseModel = new ResponseModel<HikePayTerminalResponse>
                {
                    result = new HikePayTerminalResponse
                    {
                        PaymentStatusSuccess = reversalResponse.Response.Result == ResultType.Success,
                        PoiTransactionId = reversalResponse.POIData?.POITransactionID?.TransactionID,
                        PoiTransactionTimeStamp = reversalResponse.POIData?.POITransactionID?.TimeStamp,
                        SaleTransactionId = reversalResponse.OriginalPOITransaction?.POITransactionID?.TransactionID,
                        SaleTransactionTimeStamp = DateTime.Now,
                        AdditonalResponse = additionResponse,
                        PaymentReceipt = reversalResponse.PaymentReceipt,
                        IsTaskCanceledException = false,
                        IsInProgress = false,
                        ErrorConditionType = reversalResponse.Response.ErrorCondition.HasValue ? (HikePOS.Models.Payment.ErrorConditionType)reversalResponse.Response.ErrorCondition.Value : HikePOS.Models.Payment.ErrorConditionType.Aborted // or default value
                    },
                    success = reversalResponse.Response.Result == ResultType.Success

                };
                var cashierReceipt = reversalResponse.PaymentReceipt?
    .FirstOrDefault(x => x.DocumentQualifier == DocumentQualifierType.CashierReceipt);

                if (cashierReceipt != null)
                {
                    var receiptstr = JsonConvert.SerializeObject(cashierReceipt);
                    var receipt = JsonConvert.DeserializeObject<HikePOS.Models.Payment.PaymentReceipt>(receiptstr);
                    responseModel.result.MerchantReceipt =
                        CommonMethods.HikePayReceiptBuilder.BuildReceipt(receipt);
                }

                var customerReceipt = reversalResponse.PaymentReceipt?
                    .FirstOrDefault(x => x.DocumentQualifier == DocumentQualifierType.CustomerReceipt);

                if (customerReceipt != null)
                {
                    var receiptstr = JsonConvert.SerializeObject(customerReceipt);
                    var receipt = JsonConvert.DeserializeObject<HikePOS.Models.Payment.PaymentReceipt>(receiptstr);
                    responseModel.result.CustomerReceipt =
                        CommonMethods.HikePayReceiptBuilder.BuildReceipt(receipt);
                }
                responseModel.result.RefusalReason =
                    responseModel.result.DecodeAdditonalResponse.refusalReason
                        ?? responseModel.result.DecodeAdditonalResponse.message;
                return responseModel;

            }
            else if (response?.MessagePayload is Adyen.Model.TerminalApi.TransactionStatusResponse transactionStatusResponse)
            {
                var responseModel = new ResponseModel<HikePayTerminalResponse>
                {
                    result = new HikePayTerminalResponse
                    {
                        IsInProgress = transactionStatusResponse.Response.ErrorCondition == Adyen.Model.TerminalApi.ErrorConditionType.InProgress ? true : false,
                        ErrorConditionType = transactionStatusResponse.Response.ErrorCondition.HasValue ? (HikePOS.Models.Payment.ErrorConditionType)transactionStatusResponse.Response.ErrorCondition.Value : HikePOS.Models.Payment.ErrorConditionType.Aborted // or default value
                    },
                    success = transactionStatusResponse.Response.Result == ResultType.Success

                };

                if (transactionStatusResponse.RepeatedMessageResponse?.RepeatedResponseMessageBody?.MessagePayload is Adyen.Model.TerminalApi.PaymentResponse verifyPaymentResponse)
                {
                    var json = JsonConvert.SerializeObject(verifyPaymentResponse);
                    Debug.WriteLine(json);
                    byte[] bytes = Convert.FromBase64String(verifyPaymentResponse.Response.AdditionalResponse);
                    string additionResponse = Encoding.UTF8.GetString(bytes);
                    responseModel.result.PaymentStatusSuccess = verifyPaymentResponse.Response.Result == ResultType.Success;
                    responseModel.result.PoiTransactionId = verifyPaymentResponse.POIData?.POITransactionID?.TransactionID;
                    responseModel.result.PoiTransactionTimeStamp = verifyPaymentResponse.POIData?.POITransactionID?.TimeStamp;
                    responseModel.result.SaleTransactionId = verifyPaymentResponse.SaleData?.SaleTransactionID?.TransactionID;
                    responseModel.result.SaleTransactionTimeStamp = verifyPaymentResponse.SaleData?.SaleTransactionID?.TimeStamp;
                    responseModel.result.AdditonalResponse = additionResponse;
                    responseModel.result.PaymentReceipt = verifyPaymentResponse.PaymentReceipt;
                    responseModel.result.IsTaskCanceledException = false;
                    responseModel.result.RefusalReason =
                    responseModel.result.DecodeAdditonalResponse.refusalReason
                                    ?? responseModel.result.DecodeAdditonalResponse.message;
                    var cashierReceipt = verifyPaymentResponse.PaymentReceipt?.FirstOrDefault(x => x.DocumentQualifier == DocumentQualifierType.CashierReceipt);
                    if (cashierReceipt != null)
                    {
                        var receiptstr = JsonConvert.SerializeObject(cashierReceipt);
                        var receipt = JsonConvert.DeserializeObject<HikePOS.Models.Payment.PaymentReceipt>(receiptstr);
                        responseModel.result.MerchantReceipt =
                            CommonMethods.HikePayReceiptBuilder.BuildReceipt(receipt);
                    }
                    var customerReceipt = verifyPaymentResponse.PaymentReceipt?
                        .FirstOrDefault(x => x.DocumentQualifier == DocumentQualifierType.CustomerReceipt);
                    if (customerReceipt != null)
                    {
                        var receiptstr = JsonConvert.SerializeObject(customerReceipt);
                        var receipt = JsonConvert.DeserializeObject<HikePOS.Models.Payment.PaymentReceipt>(receiptstr);
                        responseModel.result.CustomerReceipt =
                            CommonMethods.HikePayReceiptBuilder.BuildReceipt(receipt);
                    }

                }
                return responseModel;

            }
            return null;
        }

        private async Task<HikePayTerminalResponse> VerifyWithRetryAsync(
            string lastSaleId,
            string lastServiceId,
            NadaPayConfigurationDto config,
            CancellationToken cancellationToken)
        {
                cancellationToken.ThrowIfCancellationRequested();
            CallAgain:
                var verifyRequest = PrepareVerifyRequest(lastSaleId, lastServiceId, config);
                var result = await ExecuteAsync(verifyRequest, cancellationToken);

                if (result?.result?.IsInProgress == true)
                {
                  await Task.Delay(5000, cancellationToken);
                   goto CallAgain;
                }

                if (result?.success == true)
                    Settings.HikePayVerify = null;

                return result?.result;
            

        }

        #endregion

        #region Request Builders

        private Adyen.Model.TerminalApi.Message.SaleToPOIRequest PreparePaymentRequest(
            decimal amount,
            string currency,
            string lastReference,
            NadaPayConfigurationDto config)
        {
            var serviceID = CommonMethods.Generate10CharsId();
            var saleID = $"Hike_{Settings.TenantId}_{lastReference}";
            var transactionID = Guid.NewGuid().ToString();

            return new Adyen.Model.TerminalApi.Message.SaleToPOIRequest
            {
                MessageHeader = new Adyen.Model.TerminalApi.MessageHeader
                {
                    MessageClass = Adyen.Model.TerminalApi.MessageClassType.Service,
                    MessageCategory = Adyen.Model.TerminalApi.MessageCategoryType.Payment,
                    MessageType = MessageType.Request,
                    ServiceID = serviceID,
                    SaleID = saleID,
                    POIID = config.TerminalId
                },
                MessagePayload = new Adyen.Model.TerminalApi.PaymentRequest
                {
                    SaleData = new Adyen.Model.TerminalApi.SaleData
                    {
                        SaleTransactionID = new TransactionIdentification
                        {
                            TransactionID = transactionID,
                            TimeStamp = DateTime.UtcNow
                        }
                    },
                    PaymentTransaction = new Adyen.Model.TerminalApi.PaymentTransaction
                    {
                        AmountsReq = new Adyen.Model.TerminalApi.AmountsReq
                        {
                            Currency = currency,
                            RequestedAmount = amount
                        }
                    }
                }
            };
        }

        private Adyen.Model.TerminalApi.Message.SaleToPOIRequest PrepareReversalRequest(
            bool isPartial,
            decimal amount,
            string currency,
            string saleId,
            string merchantReference,
            string poiTransactionId,
            DateTime poiTransactionTimeStamp,
            PaymentOptionDto paymentOption,
            NadaPayTransactionDto nadaPayTransactionDto)
        {
            var config = DeserializeConfig(paymentOption);

            var serviceID = CommonMethods.Generate10CharsId();
            var saleID = $"Hike_{Settings.TenantId}_{saleId}_refund";

            return new Adyen.Model.TerminalApi.Message.SaleToPOIRequest
            {
                MessageHeader = new Adyen.Model.TerminalApi.MessageHeader
                {
                    MessageClass = Adyen.Model.TerminalApi.MessageClassType.Service,
                    MessageCategory = Adyen.Model.TerminalApi.MessageCategoryType.Reversal,
                    MessageType = MessageType.Request,
                    ServiceID = serviceID,
                    SaleID = saleID,
                    POIID = config.TerminalId
                },
                MessagePayload = new ReversalRequest
                {
                    OriginalPOITransaction = new OriginalPOITransaction
                    {
                        POITransactionID = new TransactionIdentification
                        {
                            TransactionID = poiTransactionId,
                            TimeStamp = poiTransactionTimeStamp
                        }
                    },
                    SaleData = new Adyen.Model.TerminalApi.SaleData
                    {
                        SaleTransactionID = new TransactionIdentification
                        {
                            TransactionID = merchantReference,
                            TimeStamp = DateTime.UtcNow
                        },
                        SaleToAcquirerData = new SaleToAcquirerData
                        {
                            Currency = currency
                        }
                    },
                    ReversedAmount = amount,
                    ReversalReason = ReversalReasonType.MerchantCancel
                }
            };
        }

        private Adyen.Model.TerminalApi.Message.SaleToPOIRequest PrepareCancelRequest(
            string lastSaleId,
            string lastServiceId,
            NadaPayConfigurationDto config)
        {
            var serviceID = CommonMethods.Generate10CharsId();

            return new Adyen.Model.TerminalApi.Message.SaleToPOIRequest
            {
                MessageHeader = new Adyen.Model.TerminalApi.MessageHeader
                {
                    MessageClass = Adyen.Model.TerminalApi.MessageClassType.Service,
                    MessageCategory = Adyen.Model.TerminalApi.MessageCategoryType.Abort,
                    MessageType = MessageType.Request,
                    ServiceID = serviceID,
                    SaleID = $"Hike_{Settings.TenantId}_{Guid.NewGuid()}",
                    POIID = config.TerminalId
                },
                MessagePayload = new AbortRequest
                {
                    AbortReason = "MerchantAbort",
                    MessageReference = new Adyen.Model.TerminalApi.MessageReference
                    {
                        SaleID = $"Hike_{Settings.TenantId}_{lastSaleId}",
                        ServiceID = lastServiceId,
                        MessageCategory = Adyen.Model.TerminalApi.MessageCategoryType.Payment
                    }
                }
            };
        }

        private Adyen.Model.TerminalApi.Message.SaleToPOIRequest PrepareVerifyRequest(
            string lastSaleId,
            string lastServiceId,
            NadaPayConfigurationDto config)
        {
            return new Adyen.Model.TerminalApi.Message.SaleToPOIRequest
            {
                MessageHeader = new Adyen.Model.TerminalApi.MessageHeader
                {
                    MessageClass = Adyen.Model.TerminalApi.MessageClassType.Service,
                    MessageCategory = Adyen.Model.TerminalApi.MessageCategoryType.TransactionStatus,
                    MessageType = MessageType.Request,
                    ServiceID = CommonMethods.Generate10CharsId(),
                    SaleID = $"Hike_{Settings.TenantId}_{Guid.NewGuid()}",
                    POIID = config.TerminalId
                },
                MessagePayload = new TransactionStatusRequest
                {
                    ReceiptReprintFlag = true,
                    DocumentQualifier = new[]
                    {
                        DocumentQualifierType.CashierReceipt,
                        DocumentQualifierType.CustomerReceipt
                    },
                    MessageReference = new Adyen.Model.TerminalApi.MessageReference
                    {
                        SaleID = $"Hike_{Settings.TenantId}_{lastSaleId}",
                        ServiceID = lastServiceId,
                        MessageCategory = Adyen.Model.TerminalApi.MessageCategoryType.Payment
                    }
                }
            };
        }

        #endregion

        #region Helpers

        private static NadaPayConfigurationDto DeserializeConfig(PaymentOptionDto paymentOption)
            => JsonConvert.DeserializeObject<NadaPayConfigurationDto>(
                paymentOption.ConfigurationDetails);

        private static bool HasInternet()
            => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        private HikePayTerminalResponse HandleResponse(
            ResponseModel<HikePayTerminalResponse> response)
        {
            if (response?.success == true && response.result != null)
                return response.result;

            if (!string.IsNullOrWhiteSpace(response?.error?.message))
                ShowToast(response.error.message);

            return response?.result;
        }

        private HikePayTerminalResponse ShowError(string message)
        {
            ShowToast(message);
            return null;
        }

        private void ShowToast(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                App.Instance.Hud.DisplayToast(
                    LanguageExtension.Localize(message),
                    Colors.Red,
                    Colors.White);
            });
        }

        #endregion
    }
}