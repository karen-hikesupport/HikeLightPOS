using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Shop;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IShopApi
	{
		[Post("/api/services/app/shop/GetDetail")]
		Task<ResponseModel<ShopGeneralDto>> GetDetail([Header("Authorization")] string accessToken);

		[Post("/api/services/app/shop/UpdateRuleDetail")]
		Task<ResponseModel<GeneralRuleDto>> UpdateRuleDetail([Body] GeneralRuleDto generalRule,[Header("Authorization")] string accessToken);

		[Post("/api/services/app/shop/UpdateBasicInfo")]
		Task<ResponseModel<BasicShopInfo>> UpdateBasicInfo([Body] BasicShopInfo basicShopInfo, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/register/AssociateRegisterPayments")]
        Task<IpadPaymentSyncResponse> IpadPaymentSyncWithServer([Body] PaymentSyncDto paymentSyncDto, [Header("Authorization")] string accessToken);

    }
}
