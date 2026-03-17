using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{

    [Headers("Accept: application/json")]
    public interface IClearantPaymentService
    {
        [Post("/api/services/app/clearentPayment/ClearentSaleTransaction")]
        Task<ResponseModel<object>> ClearantSaleRequest([AliasAs("apiKey")] string apiKey, [Body] ClearantRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/clearentPayment/ClearentRefundTransaction")]
        Task<ResponseModel<object>> ClearentRefundRequest([AliasAs("apiKey")] string apiKey, [Body] ClearantRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/clearentPayment/ClearentMatchRefundTransaction")]
        Task<ResponseModel<object>> ClearentMatchRefundRequest([AliasAs("apiKey")] string apiKey, [Body] ClearantRequest input, [Header("Authorization")] string accessToken);
        [Post("/api/services/app/clearentPayment/ClearentVoidTransaction")]
        Task<ResponseModel<object>> ClearentVoidRequest([AliasAs("apiKey")] string apiKey, [Body] ClearantRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/clearentPayment/ClearentBatch")]
        Task<ResponseModel<object>> ClearentBatchRequest([AliasAs("apiKey")] string apiKey, [Body] ClearantBatchRequest input, [Header("Authorization")] string accessToken);


    }
}
