using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{
    [Headers("Accept: application/json")]
    public interface IAfterPayPaymentService
    {
        [Post("/api/services/app/paymentConfiguration/AfterPayCreateOrder")]
        //Task<RootObject> CreateAfterpayPayment([Body]AfterPayRequestObject input, [Header("Authorization")] string accessToken);
        Task<ResponseModel<object>> CreateAfterpayPayment([Body]AfterPayRequestObject input, [Header("Authorization")] string accessToken);




        [Post("/api/services/app/paymentConfiguration/AfterPayRefundOrder")]
        Task<ResponseModel<object>> CreateAfterPayRefundOrder([Body]AfterpayRefundRequestRootObject input, [Header("Authorization")] string accessToken);

    }
}
