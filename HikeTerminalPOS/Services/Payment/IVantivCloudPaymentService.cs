using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{
    [Headers("Accept: application/json")]
    public interface IVantivCloudPaymentService
    {
        [Post("/api/services/app/vantivPaymentConfiguration/SendRequest")]
        Task<ResponseModel<object>> CreateVantivClouldPayment([Body]VantivClouldRequestObject input, [Header("Authorization")] string accessToken);

    }
}
