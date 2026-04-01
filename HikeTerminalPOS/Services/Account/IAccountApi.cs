using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Models;

using Refit;

namespace HikePOS.Services
{
    [Headers("Accept: application/json")]
    public interface IAccountApi
    {
        [Post("/api/Account")]
        Task<ResponseModel<string>> Login([Body] LoginModel user);

        [Post("/oauth/token")]
        Task<AccessTockenDto> RenewAccessToken([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> RenewAccessTokenInput, [Header("Authorization")] string accessToken);

        [Post("/Account/SwitchToUserAccount")]
        Task<object> GetAdminURL([Body] AdminInputDto input, [Header("Authorization")] string accessToken);

        //Ticket:#71927
        [Post("/Account/SendPasswordResetLinkIpad")]
        Task<ResponseModel<object>> SendPasswordResetLink([Body] SendPasswordResetLinkModel input);

        [Post("/TenantRegistration/CheckTenantAvalilable")]
        Task<ResponseModel<object>> CheckTenantAvalilable([Body] CheckTenantInputModel input);

        [Post("/TenantRegistration/Register?returnUrl=")]
        //[Post("/TenantRegistration/RegisterStore?returnUrl=")]
        Task<ResponseModel<object>> CreateTenant([Body] CreateTenantInputModel input);


        [Post("/api/services/app/mirrorDevice/CreateOrUpdate")]
        Task<ResponseModel<CustomerDisplayCodeResponseModel>> CreateOrUpdate([Body] CustomerDisplayCodeRequestModel input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/Session/GetCurrentLoginInformations")]
        Task<ResponseModel<LoginInformation>> GetCurrentLoginInformations([Header("Authorization")] string accessToken);

        [Post("/Account/VerifySecurityCode")]
        Task<ResponseModel<object>> VerifySecurityCode([Body] VerifySecurityCodeDto input, [Header("Authorization")] string accessToken);

        [Post("/api/v1/login/LoginByPin")]
        Task<ResponseModel<string>> LoginByPin([Body] LoginByPinModel user);

    }

}