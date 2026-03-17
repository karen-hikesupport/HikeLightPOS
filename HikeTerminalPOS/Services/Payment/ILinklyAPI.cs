using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{
    [Headers("Accept: application/json")]
    public interface ILinklyAPI
    {
        //https://hikeasytestlinkly.asy.io:82/api/services/app/linklyPayment/LinklySaleTransaction?paymentId=10&sessionId=afe91583-12bc-4c9d-8f35-c9ce4be04d09
        [Post("/api/services/app/linklyPayment/LinklySaleTransaction")]
        Task<ResponseModel<LinklyResponseRoot>> LinklySaleTransaction([AliasAs("paymentId")] int paymentId, [AliasAs("sessionId")] string sessionId, [Body] LinklySaleRootRequest input,[Header("Authorization")] string accessToken);
        //https://hikeasytestlinkly.asy.io:82/api/services/app/linklyPayment/LinklySaleTransaction?paymentId=10&sessionId=afe91583-12bc-4c9d-8f35-c9ce4be04d09
        [Post("/api/services/app/linklyPayment/LinklyRefundTransaction")]
        Task<ResponseModel<LinklyResponseRoot>> LinklyRefundTransaction([AliasAs("paymentId")] int paymentId, [AliasAs("sessionId")] string sessionId, [Body] LinklyRefundRootRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/linklyPayment/GetLinklyTransaction")]
        Task<ResponseModel<LinklyResponseRoot>> GetLinklyTransaction([AliasAs("paymentId")] int paymentId, [AliasAs("sessionId")] string sessionId, [Header("Authorization")] string accessToken);
    }
}
