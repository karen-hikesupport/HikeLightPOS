using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{

    [Headers("Accept: application/json")]
    public interface IZipPaymentService
    {
        [Post("/api/services/app/zipPayment/SendPurchaseRequest")]
        Task<ResponseModel<object>> SendZipPurchaseRequest([Body]ZipPurchaseRequestObject input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/zipPayment/SendRefundRequest")]
        Task<ResponseModel<object>> SendZipRefundRequest([AliasAs("id")] long id,[Body]ZipRefundRequestObject input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/zipPayment/SendVoidPurchaseRequest")]
        Task<ResponseModel<object>> SendZipVoidRequest([Body]ZipPurchaseRequestObject input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/zipPayment/GetPurchaseRequest")]
        Task<ResponseModel<object>> GetZipPurchaseRequest([AliasAs("id")] long id,[Body]ZipConfiguration input, [Header("Authorization")] string accessToken);

    }
}
