using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{
    [Headers("Accept: application/json")]
    public interface IEconduitPaymentApi
    {
        //https://shaun19085.hikeup.com

        //https://shaun19085.hikeup.com/api/services/app/paymentConfiguration/SendEconduitPurchaseOrRefundRequest
        //  POST /api/services/app/paymentConfiguration/SendEconduitPurchaseOrRefundRequest

        //Task<EConduitResponse> EConduitPaymentCall([Header("Authorization")] string accessToken);
        [Post("/api/services/app/paymentConfiguration/SendEconduitPurchaseOrRefundRequest")]
        Task<ResponseModel<object>> CreateconduitPayment([Body]EconduitRequestObject input, [Header("Authorization")] string accessToken);




        [Post("/api/services/app/paymentConfiguration/SendEconduitCloseBatchRequest")]
        Task<ResponseModel<object>> CloseconduitBatchRequest([Body]EconduitRequestObject input, [Header("Authorization")] string accessToken);

    }

}