using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{

    [Headers("Accept: application/json")]
    public interface IHikePayService
    {
        [Post("/api/services/app/hikePayment/GetSplitProfile")]
        Task<ResponseModel<HikePaySplitProfile>> GetSplitProfile([AliasAs("tenantId")] int tenantId, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/hikePayment/GetHikePayTerminals")]
        Task<ResponseModel<HikePayTerminalDetailData>> GetHikePayTerminals([Body] HikePayTerminalDetailRequest input, [Header("Authorization")] string accessToken);

    }
}
