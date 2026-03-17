using System.Threading.Tasks;
using HikePOS.Models;

using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface ISubscriptionAPI
	{
		[Post("/api/services/app/edition/GetAccountDetail")]
		Task<ResponseModel<SubscriptionDto>> GetAccountDetail([Header("Authorization")] string accessToken);


		//http://localhost:6235/api/services/app/user/GetFeaturesBysession
		//https://hikeasytest0r.asy.io:82/api/services/app/user/GetFeaturesBysession
		//https://hconnect.hikeup.com/api/services/app/user/GetFeaturesBysession
		[Post("/api/services/app/user/GetFeaturesBysession")]
		Task<ResponseModel<ShopFeature>> GetStorewiseFeatures([Header("Authorization")] string accessToken);

			
	}
}
